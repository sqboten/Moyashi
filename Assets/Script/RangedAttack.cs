using UnityEngine;

public class RangedAttack : MonoBehaviour
{
    [Header("Ranged Attack")]
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float attackWidth = 1f;

    [Header("HP Cost")]
    [SerializeField] private float hpCostPercent = 0.2f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 0.5f;

    [Header("Effect")]
    [SerializeField] private GameObject rangedAttackEffect;

    private PlayerStats _playerStats;
    private SkillManager _skillManager;
    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();

        _skillManager =
            FindFirstObjectByType<SkillManager>();

        _inputActions =
            new PlayerInputActions();
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
        if (_inputActions.Player.RangedAttack.WasPressedThisFrame())
        {
            TryRangedAttack();
        }
    }

    private void TryRangedAttack()
    {
        // スキル未解放
        if (_skillManager != null &&
            !_skillManager.IsUnlocked("RangedAttack"))
        {
            Debug.Log("RangedAttack 未解放");
            return;
        }

        if (_playerStats == null)
        {
            return;
        }

        // 最大HPの20%
        float hpCost =
            _playerStats.MaxHP * hpCostPercent;

        // 現在HPが消費量以下なら使用不可
        if (_playerStats.CurrentHP <= hpCost)
        {
            Debug.Log("HP不足");
            return;
        }

        // HP消費
        _playerStats.TakeDamage(hpCost);

        // 攻撃力 × 消費HP × 0.5
        int damage =
            Mathf.FloorToInt(
                _playerStats.AttackPower *
                hpCost *
                damageMultiplier
            );

        PlayRangedAttackEffect();

        Debug.Log(
            "Ranged Attack / HP Cost : " +
            hpCost +
            " / Damage : " +
            damage
        );

        RangedAttackHit(damage);
    }

    private void RangedAttackHit(int damage)
    {
        Vector3 startPosition =
            transform.position;

        Vector3 direction =
            transform.forward;

        Ray ray =
            new Ray(startPosition, direction);

        RaycastHit[] hits =
            Physics.SphereCastAll(
                ray,
                attackWidth / 2f,
                attackRange,
                ~0,
                QueryTriggerInteraction.Ignore
            );

        foreach (RaycastHit hit in hits)
        {
            Enemy enemy =
                hit.collider.GetComponentInParent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.PlayHitEffect(
                        hit.collider.ClosestPoint(transform.position)
                    );
                }

                continue;
            }

            Boss boss =
                hit.collider.GetComponentInParent<Boss>();

            if (boss != null)
            {
                boss.TakeDamage(damage);

                if (HitEffectManager.Instance != null)
                {
                    HitEffectManager.Instance.PlayHitEffect(
                        hit.collider.ClosestPoint(transform.position)
                    );
                }
            }
        }
    }
    private void PlayRangedAttackEffect()
    {
        if (rangedAttackEffect == null)
        {
            return;
        }

        GameObject effect = Instantiate(
            rangedAttackEffect,
            transform.position,
            transform.rotation
        );

        effect.transform.forward = transform.forward;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Vector3 startPosition =
            transform.position;

        Vector3 endPosition =
            startPosition +
            transform.forward * attackRange;

        Gizmos.DrawLine(
            startPosition,
            endPosition
        );

        Gizmos.DrawWireSphere(
            startPosition,
            attackWidth / 2f
        );

        Gizmos.DrawWireSphere(
            endPosition,
            attackWidth / 2f
        );
    }
}