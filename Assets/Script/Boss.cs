using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private float maxHP = 5000f;
    [SerializeField] private float attackPower = 80f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDistance = 2f;

    [Header("Contact Attack")]
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 2f;


    [SerializeField] private WaveManager waveManager;

    private float currentHP;
    private float damageTimer;

    private bool isStunned;
    private float stunTimer;

    private Transform player;
    private PlayerStats playerStats;
    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
            playerStats =
                playerObject.GetComponent<PlayerStats>();
        }
    }
    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
            {
                EndStun();
            }

            return;
        }

        MoveToPlayer();

        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }
    public void SetWaveManager(WaveManager manager)
    {
        waveManager = manager;
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
    public void SetWaveStats(int wave)
    {
        maxHP = 5000f + (wave - 1) * 3000f;
        attackPower = 80f + (wave - 1) * 100f;

        currentHP = maxHP;

        Debug.Log(
            "Boss WAVE " + wave +
            " HP : " + maxHP +
            " Attack : " + attackPower
        );
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
            // ÉpÉäÉBê¨å˜
            if (playerController.IsParrying)
            {
                Debug.Log("Parry Success : Boss");

                StartStun();

                damageTimer = damageInterval;

                return;
            }

            // âÒîíÜ
            if (playerController.IsInvincible)
            {
                Debug.Log("Dodge : Damage Avoid");
                return;
            }
        }

        playerStats.TakeDamage(attackPower);

        damageTimer = damageInterval;

        Debug.Log("Boss Contact Attack : " + attackPower);
    }

    private void StartStun()
    {
        isStunned = true;
        stunTimer = stunDuration;

        Debug.Log("Boss Stun");
    }

    private void EndStun()
    {
        isStunned = false;

        Debug.Log("Boss Stun End");
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        currentHP -= damage;

        Debug.Log("Boss Damage : " + damage);
        Debug.Log("Boss HP : " + currentHP);

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss Dead");

        if (waveManager != null)
        {
            waveManager.BossDefeated();
        }
        else
        {
            Debug.LogError("WaveManagerÇ™BossÇ…ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
        }

        Destroy(gameObject);
    }
}