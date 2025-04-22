using System.Collections;
using UnityEngine;

/**
* This class spawn waves according to GameController class.
*/
public class WaveSpawnController : MonoBehaviour
{
    public GameController gameController;
    public GameObject standardEnemy;
    public Transform spawnPoint;
    public Transform playerTransform;

    // Door animation is optional in this version
    public Animator doorAnimator;
    private const string DOOR_SPAWN_PARAM = "IsCharacterSpawning";

    // Spawn settings
    public float timeBetweenSpawns = 3.0f;
    public float timeBetweenWaves = 5.0f;

    // Debug options
    public bool debugMode = true;
    public bool skipDoorAnimation = false; // Set to true to bypass door animation issues

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

        // Start direct spawning test
        StartCoroutine(BeginSpawning());
    }

    IEnumerator BeginSpawning()
    {
        Debug.Log("Starting wave spawning in " + timeBetweenWaves + " seconds...");
        yield return new WaitForSeconds(timeBetweenWaves);

        // Start with a single test enemy
        Debug.Log("Spawning test enemy");
        SpawnSingleEnemy();

        yield return new WaitForSeconds(2.0f);

        // Start actual wave system if test was successful
        Debug.Log("Starting wave 1");
        StartCoroutine(SpawnWave(1));
    }

    IEnumerator SpawnWave(int waveNumber)
    {
        int enemiesInWave = waveNumber * 2; // Same formula as before
        Debug.Log("Wave " + waveNumber + " started with " + enemiesInWave + " enemies");

        for (int i = 0; i < enemiesInWave; i++)
        {
            yield return new WaitForSeconds(timeBetweenSpawns);
            SpawnSingleEnemy();
            
        }

        Debug.Log("Wave " + waveNumber + " completed");

    }

    private void SpawnSingleEnemy()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        // Door animation handling (optional)
        if (doorAnimator != null && !skipDoorAnimation)
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

        Debug.Log("Enemy spawned at " + enemy.transform.position);

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
                        Debug.Log("Set player reference via " + methodName);
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
        if (doorAnimator != null && !skipDoorAnimation)
        {
            doorAnimator.SetBool(DOOR_SPAWN_PARAM, false);
            yield return new WaitForSeconds(0.5f);
        }
    }
}