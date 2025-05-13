using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawnController : MonoBehaviour
{
    [System.Serializable]
    public class EnemyTypeInfo
    {
        public GameObject enemyPrefab;
        [Range(0f, 1f)]
        [Tooltip("0-1 arasında spawn olma olasılığı (0: hiç spawn olmaz, 1: her zaman spawn olur)")]
        public float spawnRatio = 0.5f;
        [HideInInspector]
        public int spawnCount; // İstatistik amaçlı
    }

    [Header("References")]
    public GameController gameController;
    public Transform spawnPoint;
    public Transform playerTransform;
    public DestroyerEnemyMapController destroyerEnemyMapController;
    public Animator doorAnimator; // optional

    [Header("Enemy Settings")]
    public List<EnemyTypeInfo> enemyTypes = new List<EnemyTypeInfo>();

    [Header("Standard Enemy Minimum")]
    [Tooltip("Sadece ilk düşman türü (standart düşman) için minimum sayı")]
    public int minStandardEnemiesPerWave = 1;

    [Header("Timing")]
    public float timeBetweenSpawns = 3f;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Probability Settings")]
    [Tooltip("Maximum number of enemies that can be spawned")]
    public int maxEnemiesPerWave = 5;
    [Tooltip("Growth factor for spawn intensity formula")]
    public float spawnGrowthFactor = 5f;
    public bool enableSpawn = true;

    [Header("Optimization")]
    [Tooltip("Düşman sayısı bu değere ulaştığında yeni düşman spawn etmeyi durdurur")]
    public int maxActiveEnemies = 15;

    private int lastSpawnedWave = -1;
    private const string DOOR_SPAWN_PARAM = "IsCharacterSpawning";
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (enableSpawn)
        {
            ValidateSetup();
        }
    }

    void Update()
    {
        // Clear destroyed enemies from active enemy list.
        CleanupDestroyedEnemies();

        if (gameController.IsGameStart() && lastSpawnedWave < gameController.GetWaveState() && enableSpawn)
        {
            lastSpawnedWave = gameController.GetWaveState();
            StartCoroutine(BeginSpawning());
        }
    }

    private void ValidateSetup()
    {
        // Enemy prefab validation
        if (enemyTypes.Count == 0)
        {
            Debug.LogError("No enemy types assigned to WaveSpawnController!");
            return;
        }

        foreach (var enemyType in enemyTypes)
        {
            if (enemyType.enemyPrefab == null)
            {
                Debug.LogError("Null enemy prefab found in WaveSpawnController!");
            }
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform; // Use this object's position if no spawn point set
            Debug.LogWarning("No spawn point set, using controller position");
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            playerTransform = player != null ? player.transform : null;

            if (playerTransform == null)
                Debug.LogWarning("No player found! Enemies may not function correctly.");
        }

        if (gameController == null)
        {
            gameController = FindObjectOfType<GameController>();
            if (gameController == null)
                Debug.LogError("GameController not found!");
        }
    }

    private void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    IEnumerator BeginSpawning()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        StartCoroutine(SpawnWave(lastSpawnedWave));
    }

    IEnumerator SpawnWave(int waveNumber)
    {
        // 1) Calculate spawn intensity (range 0-1)
        float intensity = (float)waveNumber / (waveNumber + spawnGrowthFactor);
        // 2) Determine total number of enemies to spawn
        int baseEnemyCount = Mathf.RoundToInt(intensity * maxEnemiesPerWave);
        int totalEnemiesToSpawn = Mathf.Max(baseEnemyCount, minStandardEnemiesPerWave);

        Debug.Log($"Wave {waveNumber}: Spawning {totalEnemiesToSpawn} enemies with intensity {intensity}");

        // Guarantee minimum standard enemies (Enemy type 0 is assumed to be the standard enemy)
        int standardEnemiesSpawned = 0;

        for (int i = 0; i < totalEnemiesToSpawn; i++)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);

            // Check if we have too many active enemies
            if (activeEnemies.Count >= maxActiveEnemies)
            {
                Debug.Log("Maximum active enemies reached, skipping spawn");
                continue;
            }

            // First ensure we're meeting minimum standard enemy requirements
            if (standardEnemiesSpawned < minStandardEnemiesPerWave && enemyTypes.Count > 0)
            {
                SpawnEnemy(0); // Spawn standard enemy (index 0)
                standardEnemiesSpawned++;
            }
            else
            {
                // For remaining spawns, use weighted random selection based on spawn ratios
                SpawnRandomEnemyByRatio();
            }
        }
    }

    private void SpawnRandomEnemyByRatio()
    {
        if (enemyTypes.Count == 0) return;

        // Calculate total weight
        float totalRatio = 0f;
        foreach (var enemyType in enemyTypes)
        {
            totalRatio += enemyType.spawnRatio;
        }

        if (totalRatio <= 0)
        {
            // Fallback to first enemy if all ratios are 0
            SpawnEnemy(0);
            return;
        }

        // Select enemy type based on spawn ratio
        float randomValue = Random.Range(0f, totalRatio);
        float cumulativeRatio = 0f;

        for (int i = 0; i < enemyTypes.Count; i++)
        {
            cumulativeRatio += enemyTypes[i].spawnRatio;

            if (randomValue <= cumulativeRatio)
            {
                SpawnEnemy(i);
                break;
            }
        }
    }

    private void SpawnEnemy(int enemyTypeIndex)
    {
        if (enemyTypeIndex < 0 || enemyTypeIndex >= enemyTypes.Count) return;

        StartCoroutine(SpawnEnemyRoutine(enemyTypeIndex));
    }

    IEnumerator SpawnEnemyRoutine(int enemyTypeIndex)
    {
        EnemyTypeInfo enemyType = enemyTypes[enemyTypeIndex];

        // Door animation handling (optional)
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(DOOR_SPAWN_PARAM, true);
            yield return new WaitForSeconds(1f);
        }

        // The actual enemy spawning
        GameObject enemy = Instantiate(enemyType.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        if (enemy == null)
        {
            Debug.LogError($"Failed to instantiate enemy type {enemyTypeIndex}!");
            yield break;
        }

        // Track the enemy for optimization purposes
        activeEnemies.Add(enemy);

        // Update statistics
        enemyType.spawnCount++;

        // Try to set player reference using direct component access - any component that might need player target
        SetReferencesForEnemy(enemy);

        // Close door if needed
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(DOOR_SPAWN_PARAM, false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SetReferencesForEnemy(GameObject enemy)
    {
        if (playerTransform == null) return;

        Component[] components = enemy.GetComponents(typeof(Component));

        PlayerTransformAssigner playerTransformAssigner = enemy.GetComponentInChildren<PlayerTransformAssigner>();

        if (playerTransformAssigner != null)
        {
            playerTransformAssigner.SetPlayerTransform(this.playerTransform);
            BaseMovementScript baseMovementScript = enemy.GetComponent<BaseMovementScript>();
            baseMovementScript.SetDestroyerMapController(this.destroyerEnemyMapController);
            baseMovementScript.SetPlayerTransform(this.playerTransform);
            Debug.Log("Player tranform assigner work");

        }else
        {
            Debug.Log("playertransform assigner null");
        }



        foreach (Component component in components)
        {
            // Try to find any method that might be used to set player target
            string[] methodsToTry = { "SetPlayerTransform", "SetTarget", "SetPlayer" };

            foreach (string methodName in methodsToTry)
            {
                System.Reflection.MethodInfo method = component.GetType().GetMethod(methodName);
                if (method != null)
                {
                    try
                    {
                        method.Invoke(component, new object[] { playerTransform });
                        break;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Error setting player reference via {methodName}: {e.Message}");
                    }
                }
            }
        }
    }

    // Debug ve istatistik amaçlı
    public string GetSpawnStatistics()
    {
        string stats = "Enemy Spawn Statistics:\n";
        for (int i = 0; i < enemyTypes.Count; i++)
        {
            stats += $"Enemy Type {i}: {enemyTypes[i].spawnCount} spawned\n";
        }
        return stats;
    }
}