using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Dodge")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeStaminaCost = 20f;

    [Header("Parry")]
    [SerializeField] private float parryDuration = 0.3f;
    [SerializeField] private float parryStaminaCost = 15f;

    public bool IsParrying => _isParrying;
    private Rigidbody _rigidbody;
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;

    private PlayerAttack _playerAttack;

    private PlayerStats _playerStats;

    private SkillManager _skillManager;

    private bool _isDodging;
    private float _dodgeTimer;
    private Vector3 _dodgeDirection;

    private bool _isInvincible;
    public bool IsInvincible => _isInvincible;

    private bool _isParrying;
    private float _parryTimer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _inputActions = new PlayerInputActions();

        _playerAttack = GetComponent<PlayerAttack>();
        _playerStats = GetComponent<PlayerStats>();
        _skillManager = FindFirstObjectByType<SkillManager>();
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
        _moveInput =
            _inputActions.Player.Move.ReadValue<Vector2>();

        if (_inputActions.Player.Dodge.WasPressedThisFrame())
        {
            StartDodge();
        }

        if (_isDodging)
        {
            _dodgeTimer -= Time.deltaTime;

            if (_dodgeTimer <= 0f)
            {
                EndDodge();
            }
        }
        if (_inputActions.Player.Parry.WasPressedThisFrame())
        {
            StartParry();
        }
        if (_isParrying)
        {
            _parryTimer -= Time.deltaTime;

            if (_parryTimer <= 0f)
            {
                EndParry();
            }
        }
    }
    private void FixedUpdate()
    {
        if (_isDodging)
        {
            DodgeMove();
            return;
        }

        if (_playerAttack != null && _playerAttack.IsAttacking)//çUåÇíÜÇÃà⁄ìÆë¨ìxí·â∫
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

        // ì¸óÕÇ™Ç»Ç¢èÍçáÇÕåªç›ÇÃå¸Ç´Çà€éù
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
    private void StartDodge()//âÒîèàóù
    {
        if (_skillManager != null &&
             !_skillManager.IsUnlocked("Dodge"))
        {
            return;
        }
        if (_isDodging)
        {
            return;
        }

        if (_playerStats == null)
        {
            return;
        }

        if (!_playerStats.TryUseStamina(dodgeStaminaCost))
        {
            Debug.Log("Not Enough Stamina");
            return;
        }

        _dodgeDirection = new Vector3(
            _moveInput.x,
            0f,
            _moveInput.y
        );

        if (_dodgeDirection.sqrMagnitude <= 0.01f)
        {
            _dodgeDirection = transform.forward;
        }

        _dodgeDirection.Normalize();

        _isDodging = true;
        _isInvincible = true;
        _dodgeTimer = dodgeDuration;
        IgnoreEnemyCollisions(true);
        Debug.Log("Dodge");
    }
    private void IgnoreEnemyCollisions(bool ignore)
    {
        Collider playerCollider = GetComponent<Collider>();

        if (playerCollider == null)
        {
            return;
        }

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Collider enemyCollider =
                enemy.GetComponent<Collider>();

            if (enemyCollider != null)
            {
                Physics.IgnoreCollision(
                    playerCollider,
                    enemyCollider,
                    ignore
                );
            }
        }

        GameObject boss =
            GameObject.FindGameObjectWithTag("Boss");

        if (boss != null)
        {
            Collider bossCollider =
                boss.GetComponent<Collider>();

            if (bossCollider != null)
            {
                Physics.IgnoreCollision(
                    playerCollider,
                    bossCollider,
                    ignore
                );
            }
        }
    }
    private void DodgeMove()
    {
        _rigidbody.linearVelocity =
            _dodgeDirection * dodgeSpeed;
    }
    private void EndDodge()
    {
        _isDodging = false;
        _isInvincible = false;
        IgnoreEnemyCollisions(false);
        _rigidbody.linearVelocity = Vector3.zero;
    }
    //ÉpÉäÉBèàóù
    private void StartParry()
    {
        if (_skillManager != null &&
             !_skillManager.IsUnlocked("Parry"))
        {
            return;
        }
        if (_isDodging || _isParrying)
        {
            return;
        }

        if (_playerStats == null)
        {
            return;
        }

        if (!_playerStats.TryUseStamina(parryStaminaCost))
        {
            Debug.Log("Not Enough Stamina");
            return;
        }

        _isParrying = true;
        _parryTimer = parryDuration;

        Debug.Log("Parry");
    }
    private void EndParry()
    {
        _isParrying = false;

        Debug.Log("Parry End");
    }
}