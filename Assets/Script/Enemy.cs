using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHP = 150f;
    [SerializeField] private float attackPower = 10f;

    private float baseHP;
    private float baseAttackPower;

    [Header("Drop")]
    [SerializeField] private GameObject waterPrefab;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float damageInterval = 0.5f;

    private float damageTimer;
    private float currentHP;

    private Transform player;
    private PlayerStats playerStats;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

    private bool dropWater = true;
    private void Awake()
    {
        baseHP = maxHP;
        baseAttackPower = attackPower;

        currentHP = maxHP;
    }
    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
    }

    private void Update()
    {
        MoveToPlayer();

        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void MoveToPlayer()
    {
        if (player == null)
        {
            return;
        }

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        direction.Normalize();

        transform.position +=
            direction * moveSpeed * Time.deltaTime;
    }
    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (damageTimer > 0f)
        {
            return;
        }

        PlayerStats playerStats =
            collision.gameObject.GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            return;
        }

        PlayerController playerController =
            collision.gameObject.GetComponent<PlayerController>();

        if (playerController != null)
        {
            // パリィ成功
            if (playerController.IsParrying)
            {
                Debug.Log("Parry Success : Enemy");

                Knockback(collision.transform);

                damageTimer = damageInterval;

                return;
            }

            // 回避中
            if (playerController.IsInvincible)
            {
                Debug.Log("Dodge : Damage Avoid");
                return;
            }
        }

        playerStats.TakeDamage(attackPower);

        damageTimer = damageInterval;

        Debug.Log("Enemy Contact Attack : " + attackPower);
    }
    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        currentHP -= damage;

        Debug.Log("Enemy Damage : " + damage);
        Debug.Log("Enemy HP : " + currentHP);

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }
    private void Knockback(Transform player)//パリィ成功時ノックバック
    {
        Vector3 direction =
            transform.position - player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        direction.Normalize();

        transform.position +=
            direction * 2f;
    }

    private void Die()
    {
        Debug.Log("Enemy Dead");

        if (playerStats != null)
        {
            playerStats.AddKillCount();
        }

        SkillManager skillManager =
            FindFirstObjectByType<SkillManager>();

        if (skillManager != null)
        {
            skillManager.AddSkillPoint(1);
        }

        if (dropWater && waterPrefab != null)
        {
            Instantiate(
                waterPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
    public void SetDropWater(bool value)
    {
        dropWater = value;
    }
    public void SetWaveStats(int wave)
    {
        maxHP = baseHP + (wave - 1) * 500f;
        attackPower = baseAttackPower + (wave - 1) * 50f;

        currentHP = maxHP;

        Debug.Log(
            "Enemy WAVE " + wave +
            " HP : " + maxHP +
            " Attack : " + attackPower
        );
    }
}