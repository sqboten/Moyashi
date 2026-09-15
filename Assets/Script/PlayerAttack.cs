using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 0.6f;
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float comboInputTime = 0.5f;
    [SerializeField] private float attackMoveSpeed = 3f;

    [Header("Combo Settings")]
    [SerializeField] private int maxComboCount = 3;

    [Header("Attack Hit")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackWidth = 3f;
    [SerializeField] private float attackDepth = 2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Stamina")]
    [SerializeField] private float attackStaminaCost = 10f;

    private Rigidbody _rigidbody;
    private PlayerInputActions _inputActions;
    private PlayerStats _playerStats;

    private int _comboCount;
    private bool _isAttacking;
    private bool _comboInput;

    private float _attackTimer;
    private float _comboTimer;

    private Vector2 _moveInput;

    public bool IsAttacking => _isAttacking;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _inputActions = new PlayerInputActions();
        _playerStats = GetComponent<PlayerStats>();
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

        // 攻撃ボタンが押された
        if (_inputActions.Player.Attack.WasPressedThisFrame())
        {
            if (_isAttacking)
            {
                // 攻撃中ならコンボ入力として受け付ける
                _comboInput = true;
            }
            else
            {
                // 攻撃していないなら1段目開始
                StartAttack();
            }
        }

        if (_isAttacking)
        {
            _attackTimer += Time.deltaTime;

            // 攻撃終了
            if (_attackTimer >= attackDuration)
            {
                EndAttack();
            }
        }

        // コンボ受付時間
        if (_comboTimer > 0f)
        {
            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_isAttacking)
        {
            AttackMove();
        }
    }

    private void StartAttack()
    {
        // スタミナが足りなければ攻撃しない
        if (!_playerStats.TryUseStamina(attackStaminaCost))
        {
            Debug.Log("Not Enough Stamina");
            return;
        }

        _comboCount++;

        if (_comboCount > maxComboCount)
        {
            _comboCount = 1;
        }

        _isAttacking = true;
        _comboInput = false;
        _attackTimer = 0f;

        Debug.Log("Attack " + _comboCount);

        AttackHit();
    }

    private void EndAttack()
    {
        _isAttacking = false;

        // 3段目ならコンボ終了
        if (_comboCount >= maxComboCount)
        {
            ResetCombo();
            return;
        }

        // 次の攻撃を受け付ける時間
        _comboTimer = comboInputTime;

        // コンボ入力があった場合は次の攻撃へ
        if (_comboInput)
        {
            StartAttack();
        }
    }

    private void AttackMove()
    {
        Vector3 moveDirection = new Vector3(
            _moveInput.x,
            0f,
            _moveInput.y
        );

        // 入力がなければ移動しない
        if (moveDirection.sqrMagnitude <= 0.01f)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        moveDirection.Normalize();

        _rigidbody.linearVelocity =
            moveDirection * attackMoveSpeed;
    }

    private void ResetCombo()
    {
        _comboCount = 0;
        _comboInput = false;
        _comboTimer = 0f;
    }

    private void AttackHit()
    {
        Vector3 attackPosition =
            transform.position + transform.forward * attackRange;

        Vector3 halfExtents = new Vector3(
            attackWidth / 2f,
            1f,
            attackDepth / 2f
        );

        Collider[] hitTargets = Physics.OverlapBox(
            attackPosition,
            halfExtents,
            transform.rotation,
            enemyLayer
        );

        foreach (Collider hitTarget in hitTargets)
        {
            Enemy enemy = hitTarget.GetComponent<Enemy>();

            if (enemy == null)
            {
                continue;
            }

            enemy.TakeDamage(_playerStats.AttackPower);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Vector3 attackPosition =
            transform.position + transform.forward * attackRange;

        Vector3 halfExtents = new Vector3(
            attackWidth / 2f,
            1f,
            attackDepth / 2f
        );

        Gizmos.matrix = Matrix4x4.TRS(
            attackPosition,
            transform.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            halfExtents * 2f
        );

        Gizmos.matrix = Matrix4x4.identity;
    }
}