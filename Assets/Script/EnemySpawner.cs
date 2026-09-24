using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialSpawnCount = 10;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxEnemyCount = 30;

    [Header("Boss Phase Spawn")]
    [SerializeField] private int bossSpawnCount = 1;
    [SerializeField] private float bossSpawnInterval = 3f;
    [SerializeField] private int bossMaxEnemyCount = 10;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(20f, 0f, 20f);

    private float spawnTimer;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool isBossPhase;
    private bool isStopped;
    private int currentWave = 1;
    private void Start()
    {
        SpawnEnemies(initialSpawnCount);
    }

    private void Update()
    {
        if (isStopped)
        {
            return;
        }
        RemoveDestroyedEnemies();

        spawnTimer += Time.deltaTime;

        float currentSpawnInterval =
            isBossPhase ? bossSpawnInterval : spawnInterval;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;

            int currentSpawnCount =
                isBossPhase ? bossSpawnCount : spawnCount;

            SpawnEnemies(currentSpawnCount);
        }
    }
    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
    }
    private void SpawnEnemies(int count)
    {
        int currentEnemyCount = spawnedEnemies.Count;

        int currentMaxEnemyCount =
            isBossPhase ? bossMaxEnemyCount : maxEnemyCount;

        if (currentEnemyCount >= currentMaxEnemyCount)
        {
            return;
        }

        int remainingCount =
            currentMaxEnemyCount - currentEnemyCount;

        int spawnAmount =
            Mathf.Min(count, remainingCount);

        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();

            GameObject enemy = Instantiate(
                enemyPrefab,
                spawnPosition,
                Quaternion.identity
            );

            Enemy enemyComponent = enemy.GetComponent<Enemy>();

            if (enemyComponent != null)
            {
                enemyComponent.SetWaveStats(currentWave);
                enemyComponent.SetDropWater(!isBossPhase);
            }

            spawnedEnemies.Add(enemy);
        }
    }

    public void StartBossPhase()
    {
        isBossPhase = true;

        spawnTimer = 0f;

        ClearEnemies();

        Debug.Log("EnemySpawner : Boss Phase");
    }

    public void StartNormalPhase()
    {
        isBossPhase = false;

        spawnTimer = 0f;

        Debug.Log("EnemySpawner : Normal Phase");

        SpawnEnemies(initialSpawnCount);
    }

    public void ClearEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] != null)
            {
                Destroy(spawnedEnemies[i]);
            }
        }

        spawnedEnemies.Clear();
    }

    private void RemoveDestroyedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
            {
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(
            -spawnAreaSize.x / 2f,
            spawnAreaSize.x / 2f
        );

        float z = Random.Range(
            -spawnAreaSize.z / 2f,
            spawnAreaSize.z / 2f
        );

        return transform.position + new Vector3(x, 0f, z);
    }

    public void StopSpawner()
    {
        isStopped = true;

        spawnTimer = 0f;

        ClearEnemies();

        Debug.Log("EnemySpawner Stopped");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(
            transform.position,
            spawnAreaSize
        );
    }
}