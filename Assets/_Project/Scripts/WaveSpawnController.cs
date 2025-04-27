using System.Collections;
using UnityEngine;

public class WaveSpawnController : MonoBehaviour
{
    [Header("References")]
    public GameController gameController;
    public GameObject standardEnemy;
    public Transform spawnPoint;
    public Transform playerTransform;
    public Animator doorAnimator; // optional

    [Header("Timing")]
    public float timeBetweenSpawns = 3f;
    public float timeBetweenWaves = 5f;

    [Header("Spawn Probability Settings")]
    [Tooltip("Maximum number of enemies that can be spawned")]
    public int maxEnemiesPerWave = 5;
    [Tooltip("Growth factor for spawn intensity formula")]
    public float spawnGrowthFactor = 5f;
    [Tooltip("Minimum number of enemies to spawn in each wave")]
    public int minEnemiesPerWave = 1;

    private int lastSpawnedWave = -1;
    private const string DOOR_SPAWN_PARAM = "IsCharacterSpawning";

    void Start()
    {
        // Basic validation
        if (standardEnemy == null)
        {
            Debug.LogError("Enemy prefab not assigned to WaveSpawnController!");
            return;
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

    void Update()
    {
        if (gameController.IsGameStart() && lastSpawnedWave < gameController.GetWaveState())
        {
            lastSpawnedWave = gameController.GetWaveState();
            StartCoroutine(BeginSpawning());
        }
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
        // 2) Random value between 0-1
        float rnd = Random.value;
        // 3) Determine number of enemies to spawn:
        int calculatedEnemies = Mathf.RoundToInt(rnd * intensity * maxEnemiesPerWave);
        // 4) Ensure at least minEnemiesPerWave enemies are spawned
        int enemiesToSpawn = Mathf.Max(calculatedEnemies, minEnemiesPerWave);


        for (int i = 0; i < enemiesToSpawn; i++)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            SpawnSingleStandardEnemy();
        }

    }

    private void SpawnSingleStandardEnemy()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        // Door animation handling (optional)
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(DOOR_SPAWN_PARAM, true);
            yield return new WaitForSeconds(1f);
        }

        // The actual enemy spawning - this is the critical part
        GameObject enemy = Instantiate(standardEnemy, spawnPoint.position, spawnPoint.rotation);

        if (enemy == null)
        {
            Debug.LogError("Failed to instantiate enemy!");
            yield break;
        }

   
        // Try to set player reference using direct component access
        Component[] components = enemy.GetComponents(typeof(Component));

        foreach (Component component in components)
        {
            // Try to find any method that might be used to set player target
            string[] methodsToTry = { "SetPlayerTransform", "SetTarget", "SetPlayer" };

            foreach (string methodName in methodsToTry)
            {
                System.Reflection.MethodInfo method = component.GetType().GetMethod(methodName);
                if (method != null && playerTransform != null)
                {
                    try
                    {
                        method.Invoke(component, new object[] { playerTransform });
                        break;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Error setting player reference: " + e.Message);
                    }
                }
            }
        }

        // Close door if needed
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(DOOR_SPAWN_PARAM, false);
            yield return new WaitForSeconds(0.5f);
        }
    }
}