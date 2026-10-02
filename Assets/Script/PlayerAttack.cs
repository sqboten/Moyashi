using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 0.6f;
    [SerializeField] private float attackDuration = 0.4f;
    [SerializeField] private float comboInputTime = 0.5f;
    [SerializeField] private float attackMoveSpeed = 3f;

    [Header("Normal Attack Effect")]
    [SerializeField] private GameObject normalAttackEffect;
    [SerializeField] private Transform attackEffectPoint;
    [SerializeField]
    private Vector3 attackEffectRotationOffset =
        new Vector3(-90f, 0f, 0f);

    [Header("Combo Settings")]
    [SerializeField] private int maxComboCount = 3;

    [Header("Normal Attack Hit")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackAngle = 90f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Stamina")]
    [SerializeField] private float attackStaminaCost = 10f;

    [Header("Charge Attack")]
    [SerializeField] private float chargeStartTime = 0.4f;
    [SerializeField] private float chargeStaminaPerSecond = 15f;
    [SerializeField] private float chargeMoveSpeed = 1f;

    [Header("Charge Attack Hit")]
    [SerializeField] private float chargeAttackRange = 2.5f;
    [SerializeField] private float chargeAttackAngle = 120f;

    private Rigidbody _rigidbody;
    private PlayerInputActions _inputActions;
    private PlayerStats _playerStats;
    private SkillManager _skillManager;

    private int _comboCount;
    private bool _isAttacking;
    private bool _comboInput;

    private float _attackTimer;
    private float _comboTimer;

    private Vector2 _moveInput;

    // チャージ
    private bool _isHoldingAttack;
    private bool _isCharging;
    private float _chargeTimer;
    private float _chargeStaminaTimer;

    public bool IsAttacking => _isAttacking;

    public bool IsCharging => _isCharging;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _inputActions = new PlayerInputActions();
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

        HandleAttackInput();

        if (_isAttacking)
        {
            _attackTimer += Time.deltaTime;

            if (_attackTimer >= attackDuration)
            {
                EndAttack();
            }
        }

        if (_comboTimer > 0f)
        {
            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f)
            {
                ResetCombo();
            }
        }

        if (_isCharging)
        {
            UpdateCharge();
        }
    }

    private void FixedUpdate()
    {
        if (_isCharging)
        {
            ChargeMove();
            return;
        }

        if (_isAttacking)
        {
            AttackMove();
        }
    }

    private void HandleAttackInput()
    {
        // 攻撃ボタンを押した瞬間
        if (_inputActions.Player.Attack.WasPressedThisFrame())
        {
            _isHoldingAttack = true;

            // チャージ攻撃が未解放なら従来の通常攻撃
            if (_skillManager == null ||
                !_skillManager.IsUnlocked("ChargeAttack"))
            {
                if (_isAttacking)
                {
                    _comboInput = true;
                }
                else
                {
                    StartAttack();
                }

                return;
            }

            // チャージ可能なら、まず長押し判定を開始
            _chargeTimer = 0f;
        }

        // 攻撃ボタンを押している間
        if (_isHoldingAttack)
        {
            _chargeTimer += Time.deltaTime;

            if (!_isCharging &&
                _chargeTimer >= chargeStartTime)
            {
                StartCharge();
            }
        }

        // 攻撃ボタンを離した瞬間
        if (_inputActions.Player.Attack.WasReleasedThisFrame())
        {
            _isHoldingAttack = false;

            // チャージ中ならチャージ攻撃を発動
            if (_isCharging)
            {
                EndCharge();
                return;
            }

            // 0.4秒未満なら通常攻撃
            if (_chargeTimer < chargeStartTime)
            {
                if (_isAttacking)
                {
                    _comboInput = true;
                }
                else
                {
                    StartAttack();
                }
            }
        }
    }

    // =========================
    // 通常攻撃
    // =========================

    private void StartAttack()
    {
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

        // 通常攻撃エフェクト
        PlayNormalAttackEffect();

        AttackHit();
    }

    private void PlayNormalAttackEffect()
    {
        if (normalAttackEffect == null)
        {
            return;
        }

        Transform spawnPoint = attackEffectPoint != null
            ? attackEffectPoint
            : transform;

        Quaternion rotation =
            transform.rotation *
            Quaternion.Euler(attackEffectRotationOffset);

        Instantiate(
            normalAttackEffect,
            spawnPoint.position,
            rotation
        );
    }

    private void EndAttack()
    {
        _isAttacking = false;

        if (_comboCount >= maxComboCount)
        {
            ResetCombo();
            return;
        }

        _comboTimer = comboInputTime;

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

    // =========================
    // 通常攻撃 扇形判定
    // =========================

    private void AttackHit()
    {
        Collider[] hitTargets = Physics.OverlapSphere(
            transform.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider hitTarget in hitTargets)
        {
            if (!IsInsideAttackAngle(hitTarget, attackAngle))
            {
                continue;
            }

            Enemy enemy =
                hitTarget.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(_playerStats.AttackPower);

                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.PlayHitEffect(
                        hitTarget.ClosestPoint(transform.position)
                    );
                }

                continue;
            }

            Boss boss =
                hitTarget.GetComponent<Boss>();

            if (boss != null)
            {
                boss.TakeDamage(_playerStats.AttackPower);

                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.PlayHitEffect(
                        hitTarget.ClosestPoint(transform.position)
                    );
                }
            }
        }
    }

    // =========================
    // 扇形内判定
    // =========================

    private bool IsInsideAttackAngle(
        Collider target,
        float attackAngle)
    {
        Vector3 direction =
            target.ClosestPoint(transform.position)
            - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return true;
        }

        direction.Normalize();

        float angle =
            Vector3.Angle(transform.forward, direction);

        return angle <= attackAngle * 0.5f;
    }

    // =========================
    // チャージ攻撃
    // =========================

    private void StartCharge()
    {
        if (_skillManager == null)
        {
            return;
        }

        if (!_skillManager.IsUnlocked("ChargeAttack"))
        {
            Debug.Log("ChargeAttack 未解放");
            return;
        }

        if (_isCharging)
        {
            return;
        }

        if (_isAttacking)
        {
            return;
        }

        _isCharging = true;
        _chargeStaminaTimer = 0f;

        _playerStats.SetCharging(true);

        Debug.Log("Charge Start");
    }

    private void UpdateCharge()
    {
        _chargeStaminaTimer += Time.deltaTime;

        // 1秒ごとにスタミナ15消費
        if (_chargeStaminaTimer >= 1f)
        {
            _chargeStaminaTimer -= 1f;

            if (!_playerStats.TryUseStamina(
                chargeStaminaPerSecond))
            {
                // スタミナが足りなくなったら強制発動
                _playerStats.TryUseStamina(
                    _playerStats.CurrentStamina
                );

                Debug.Log("Charge Force Release");

                EndCharge();
            }
        }
    }

    private void EndCharge()
    {
        if (!_isCharging)
        {
            return;
        }

        _isCharging = false;
        _playerStats.SetCharging(false);

        int chargeDamage =
            Mathf.FloorToInt(
                _playerStats.AttackPower *
                _chargeTimer
            );

        Debug.Log(
            "Charge Attack / Time : " +
            _chargeTimer +
            " / Damage : " +
            chargeDamage
        );

        ChargeAttackHit(chargeDamage);

        _chargeTimer = 0f;
        _chargeStaminaTimer = 0f;
    }

    private void ChargeMove()
    {
        Vector3 moveDirection = new Vector3(
            _moveInput.x,
            0f,
            _moveInput.y
        );

        if (moveDirection.sqrMagnitude <= 0.01f)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        moveDirection.Normalize();

        _rigidbody.linearVelocity =
            moveDirection * chargeMoveSpeed;
    }

    // =========================
    // チャージ攻撃 扇形判定
    // =========================

    private void ChargeAttackHit(int damage)
    {
        Collider[] hitTargets = Physics.OverlapSphere(
            transform.position,
            chargeAttackRange,
            enemyLayer
        );

        foreach (Collider hitTarget in hitTargets)
        {
            if (!IsInsideAttackAngle(
                hitTarget,
                chargeAttackAngle))
            {
                continue;
            }

            Enemy enemy =
                hitTarget.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.PlayHitEffect(
                        hitTarget.ClosestPoint(transform.position)
                    );
                }

                continue;
            }

            Boss boss =
                hitTarget.GetComponent<Boss>();

            if (boss != null)
            {
                boss.TakeDamage(damage);

                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.PlayHitEffect(
                        hitTarget.ClosestPoint(transform.position)
                    );
                }
            }
        }
    }

    // =========================
    // Gizmos
    // =========================

    private void OnDrawGizmosSelected()
    {
        // 通常攻撃
        DrawAttackGizmo(
            attackRange,
            attackAngle
        );

        // チャージ攻撃
        DrawAttackGizmo(
            chargeAttackRange,
            chargeAttackAngle
        );
    }

    private void DrawAttackGizmo(
        float range,
        float angle)
    {
        Vector3 forward =
            transform.forward;

        Quaternion leftRotation =
            Quaternion.Euler(
                0f,
                -angle * 0.5f,
                0f
            );

        Quaternion rightRotation =
            Quaternion.Euler(
                0f,
                angle * 0.5f,
                0f
            );

        Vector3 leftDirection =
            leftRotation * forward;

        Vector3 rightDirection =
            rightRotation * forward;

        Gizmos.DrawWireSphere(
            transform.position,
            range
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position + leftDirection * range
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position + rightDirection * range
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position + forward * range
        );
    }
}