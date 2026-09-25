using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private int maxWave = 6;
    [SerializeField] private float normalTime = 180f;

    [Header("Boss Settings")]
    [SerializeField] private float bossStartDelay = 3f;
    [SerializeField] private float nextWaveDelay = 3f;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GameObject bossPrefab;

/*    [Header("Boss Spawn")]
    [SerializeField] private Transform bossSpawnPoint;
*/
    private int currentWave = 1;
    private float waveTimer;

    private bool isBossPhase;
    private bool isTransitioning;

    private GameObject currentBoss;

    public int CurrentWave => currentWave;

    private void Start()
    {
        StartWave();
    }

    private void Update()
    {
        if (isBossPhase || isTransitioning)
        {
            return;
        }

        waveTimer += Time.deltaTime;

        if (waveTimer >= normalTime)
        {
            StartBossPhase();
        }
    }

    private void StartWave()
    {
        waveTimer = 0f;

        isBossPhase = false;
        isTransitioning = false;

        if (enemySpawner != null)
        {
            enemySpawner.SetCurrentWave(currentWave);
            enemySpawner.StartNormalPhase();
        }
        Debug.Log("WAVE " + currentWave + " START");
    }

    private void StartBossPhase()
    {
        isBossPhase = true;

        Debug.Log("WAVE " + currentWave + " BOSS START");

        if (enemySpawner != null)
        {
            enemySpawner.StartBossPhase();
        }

        StartCoroutine(BossStartDelayCoroutine());
    }

    private IEnumerator BossStartDelayCoroutine()
    {
        yield return new WaitForSeconds(bossStartDelay);

        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss PrefabÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            return;
        }

        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawnerÇ™ê›íËÇ≥ÇÍÇƒÇ¢Ç‹ÇπÇÒÅB");
            return;
        }

        Vector3 spawnPosition =
            enemySpawner.GetBossSpawnPosition();

        currentBoss = Instantiate(
            bossPrefab,
            spawnPosition,
            Quaternion.identity
        );

        Boss boss =
            currentBoss.GetComponent<Boss>();

        if (boss != null)
        {
            boss.SetWaveStats(currentWave);
            boss.SetWaveManager(this);
        }

        Debug.Log("Boss Spawn");
    }
    public void BossDefeated()
    {
        if (isTransitioning)
        {
            return;
        }

        Debug.Log("Boss Defeated");

        if (enemySpawner != null)
        {
            enemySpawner.ClearEnemies();
        }

        if (currentWave >= maxWave)
        {
            Debug.Log("ALL WAVES CLEAR");

            if (enemySpawner != null)
            {
                enemySpawner.StopSpawner();
            }

            return;
        }

        isTransitioning = true;

        StartCoroutine(NextWaveCoroutine());
    }

    private IEnumerator NextWaveCoroutine()
    {
        Debug.Log("Next Wave Delay START");

        yield return new WaitForSeconds(nextWaveDelay);

        currentWave++;

        Debug.Log("NEXT WAVE : " + currentWave);

        StartWave();
    }
}