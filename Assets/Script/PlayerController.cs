using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody _rigidbody;
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;

    private PlayerAttack _playerAttack;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _inputActions = new PlayerInputActions();

        _playerAttack = GetComponent<PlayerAttack>();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Player.Disable();
    }

    private void Update()
    {
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        // UŒ‚’†‚ÍPlayerAttack‘¤‚ÉˆÚ“®‚ğ”C‚¹‚é
        if (_playerAttack != null && _playerAttack.IsAttacking)
        {
            RotatePlayer();
            return;
        }

        Move();
        RotatePlayer();
    }

    private void Move()
    {
        Vector3 moveDirection = new Vector3(
            _moveInput.x,
            0f,
            _moveInput.y
        );

        _rigidbody.linearVelocity =
            moveDirection * moveSpeed;
    }

    private void RotatePlayer()
    {
        Vector3 moveDirection = new Vector3(
            _moveInput.x,
            0f,
            _moveInput.y
        );

        // “ü—Í‚ª‚È‚¢ê‡‚ÍŒ»İ‚ÌŒü‚«‚ğˆÛ
        if (moveDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        moveDirection.Normalize();

        Quaternion targetRotation =
            Quaternion.LookRotation(moveDirection);

        Quaternion newRotation = Quaternion.Slerp(
            _rigidbody.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        _rigidbody.MoveRotation(newRotation);
    }
}