using UnityEngine;

public class SpinAttack : MonoBehaviour
{
    [Header("Spin Attack")]
    [SerializeField] private float staminaCost = 20f;
    [SerializeField] private float spinDuration = 0.5f;
    [SerializeField] private float attackRadius = 2.5f;

    [Header("Attack")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerStats _playerStats;

    private bool _isSpinning;
    private float _spinTimer;

    public bool IsSpinning => _isSpinning;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    public bool TryStart()
    {

        if (_isSpinning)
        {
            return false;
        }

        if (_playerStats == null)
        {
            return false;
        }

        if (!_playerStats.TryUseStamina(staminaCost))
        {
            Debug.Log("Not Enough Stamina");
            return false;
        }

        _isSpinning = true;
        _spinTimer = spinDuration;

        SpinAttackHit();

        return true;
    }
    private void Update()
    {
        if (!_isSpinning)
        {
            return;
        }

        _spinTimer -= Time.deltaTime;

        if (_spinTimer <= 0f)
        {
            EndSpin();
        }
    }

    private void EndSpin()
    {
        _isSpinning = false;

        Debug.Log("Spin Attack End");
    }

    private void SpinAttackHit()
    {

        Collider[] hitTargets = Physics.OverlapSphere(
            transform.position,
            attackRadius,
            enemyLayer
        );

        int spinDamage = Mathf.CeilToInt(
            _playerStats.AttackPower * 1.5f
        );

        foreach (Collider hitTarget in hitTargets)
        {
            Enemy enemy =
                hitTarget.GetComponentInParent<Enemy>();

            if (enemy != null)
            {

                enemy.TakeDamage(spinDamage);
                continue;
            }

            Boss boss =
                hitTarget.GetComponentInParent<Boss>();

            if (boss != null)
            {

                boss.TakeDamage(spinDamage);
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRadius
        );
    }
}