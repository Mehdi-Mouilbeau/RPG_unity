using UnityEngine;

/// <summary>
/// Anime le sprite du joueur selon la direction de déplacement.
/// Spritesheet Female.png : 3 colonnes × 4 lignes (32×32 px)
///   Ligne 0 (Female_0/1/2)   = face (bas)
///   Ligne 1 (Female_3/4/5)   = gauche
///   Ligne 2 (Female_6/7/8)   = droite
///   Ligne 3 (Female_9/10/11) = dos (haut)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprites — glisser Female_0 à Female_11 dans l'ordre")]
    [SerializeField] private Sprite[] spritesDown;   // 0,1,2
    [SerializeField] private Sprite[] spritesLeft;   // 3,4,5
    [SerializeField] private Sprite[] spritesRight;  // 6,7,8
    [SerializeField] private Sprite[] spritesUp;     // 9,10,11

    [Header("Vitesse animation")]
    [SerializeField] private float fps = 8f;

    private SpriteRenderer _sr;
    private Sprite[]       _current;
    private int            _frame;
    private float          _timer;

    private void Awake()
    {
        _sr      = GetComponent<SpriteRenderer>();
        _current = spritesDown;
    }

    /// <summary>Appelé par PlayerController à chaque frame avec le vecteur de déplacement.</summary>
    public void UpdateAnimation(Vector2 move)
    {
        bool moving = move.sqrMagnitude > 0.01f;

        // Choisir la bonne ligne selon la direction dominante
        if (moving)
        {
            if (Mathf.Abs(move.x) >= Mathf.Abs(move.y))
                _current = move.x > 0 ? spritesRight : spritesLeft;
            else
                _current = move.y > 0 ? spritesUp : spritesDown;
        }

        if (_current == null || _current.Length == 0) return;

        if (moving)
        {
            // Avancer les frames selon le timer
            _timer += Time.deltaTime;
            if (_timer >= 1f / fps)
            {
                _timer = 0f;
                _frame = (_frame + 1) % _current.Length;
            }
        }
        else
        {
            // Idle : frame du milieu (index 1)
            _frame = 1;
            _timer = 0f;
        }

        _sr.sprite = _current[_frame];
    }
}
