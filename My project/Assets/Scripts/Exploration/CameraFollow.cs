using UnityEngine;

/// <summary>
/// Caméra qui suit le joueur avec smooth et limites de carte.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private Transform target;
    [SerializeField] private float     smoothSpeed = 5f;

    [Header("Limites de carte")]
    [SerializeField] private bool  useBounds = true;
    [SerializeField] private float minX = -21f;
    [SerializeField] private float maxX =  21f;
    [SerializeField] private float minY = -15f;
    [SerializeField] private float maxY =  17f;

    private Camera _cam;
    private float  _halfH, _halfW;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (target == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        if (useBounds && _cam != null)
        {
            _halfH = _cam.orthographicSize;
            _halfW = _halfH * _cam.aspect;
            desired.x = Mathf.Clamp(desired.x, minX + _halfW, maxX - _halfW);
            desired.y = Mathf.Clamp(desired.y, minY + _halfH, maxY - _halfH);
        }

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
