using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody2D    _rb;
    private Vector2        _moveInput;
    private PlayerAnimator _anim;

    private void Awake()
    {
        _rb   = GetComponent<Rigidbody2D>();
        _rb.gravityScale  = 0f;
        _rb.freezeRotation = true;
        _anim = GetComponent<PlayerAnimator>();
    }

    private void Update()
    {
        _moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            _moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            _moveInput.y -= 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            _moveInput.x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            _moveInput.x += 1f;

        if (_anim != null) _anim.UpdateAnimation(_moveInput);
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _moveInput.normalized * moveSpeed;
    }
}
