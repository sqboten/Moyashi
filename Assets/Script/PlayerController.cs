using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Spin Attack")]
    [SerializeField] private float spinAttackStaminaCost = 20f;
    [SerializeField] private float spinAttackDuration = 0.5f;
    public bool IsParrying => _isParrying;
    private Rigidbody _rigidbody;
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;

    private PlayerAttack _playerAttack;

    private PlayerStats _playerStats;

    private SkillManager _skillManager;

    private SpinAttack _spinAttack;

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
        _spinAttack = GetComponent<SpinAttack>();
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
        //パリィ
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
        //回転切り
        if (_inputActions.Player.SpinAttack.WasPressedThisFrame())
        {
            StartSpinAttack();
        }
    }
    private void FixedUpdate()
    {
        if (_isDodging)
        {
            DodgeMove();
            return;
        }

        if (_spinAttack != null && _spinAttack.IsSpinning)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (_playerAttack != null && _playerAttack.IsAttacking)//攻撃中の移動速度低下
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

        // 入力がない場合は現在の向きを維持
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
    private void StartDodge()//回避処理
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
    //パリィ処理
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
    private void StartSpinAttack()
    {
        Debug.Log("① StartSpinAttack");

        if (_skillManager != null &&
            !_skillManager.IsUnlocked("SpinAttack"))
        {
            Debug.Log("② SpinAttack 未解放");
            return;
        }

        Debug.Log("③ スキル解放済み");

        if (_isDodging || _isParrying)
        {
            Debug.Log("④ 現在使用できない状態");
            return;
        }

        Debug.Log("⑤ 使用可能");

        if (_spinAttack == null)
        {
            Debug.Log("⑥ SpinAttack コンポーネントが見つからない");
            return;
        }

        Debug.Log("⑦ SpinAttack.TryStart()を呼びます");

        bool result = _spinAttack.TryStart();

        Debug.Log("⑧ TryStart結果 : " + result);
    }
}