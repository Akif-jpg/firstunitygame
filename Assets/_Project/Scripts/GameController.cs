using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Control game states and waves
 */
public class GameController : MonoBehaviour
{
    [Header("Wave settings")]
    [SerializeField] private int waveState = 0;

    [Header("Check enemies area settings")]
    [Tooltip("Check enemies area for starting new round.")]
    [SerializeField] private Vector3 boxSize = new Vector3(20f,5f,20f); //Check enemies area boxsize
    [Tooltip("Area offset for check enemies area.")]
    [SerializeField] private Vector3 centerOffset = new Vector3(0f,2.5f,0f); // Offset position for scan area to enemy count.

    [Header("UI Elements")]
    [Tooltip("Load when game Over")]
    [SerializeField] private Canvas gameOverCanvas;
    [Tooltip("Game over canvas scoreboard")]
    [SerializeField] private TextMeshProUGUI scoreBoard;

    [Header("Scene Names")]
    [Tooltip("Name of the scene for the marketplace area.")]
    [SerializeField] private string marketplaceSceneName = "MarketplaceScene";
    
    [Tooltip("Name of the scene for the battle arena.")]
    [SerializeField] private string arenaSceneName = "ArenaScene";

    public void LoadGameOverCanvas()
    {
        gameOverCanvas.gameObject.SetActive(true);
        this.scoreBoard.text += "\n" + this.GetWaveState();
    }

    /// <summary>
    /// Loads the marketplace scene.
    /// </summary>
    public void LoadMarketplaceScene()
    {
        Debug.Log("Loading Marketplace Scene: " + marketplaceSceneName);
        SceneManager.LoadScene(marketplaceSceneName);
    }

    /// <summary>
    /// Loads the main battle arena scene.
    /// </summary>
    public void LoadArenaScene()
    {
        Debug.Log("Loading Arena Scene: " + arenaSceneName);
        SceneManager.LoadScene(arenaSceneName);
    }

    /// <summary>
    /// Reloads the current active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Reloading Scene: " + currentSceneName);
        SceneManager.LoadScene(currentSceneName);
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Loads scene by name.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Loading Scene: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is null or empty.");
        }
    }
    private String enemyTagPattern;

    private enum GameState {
        GameStarting,
        GameStarted,
        RoundStarting,
        RoundStarted,
        RoundOver,
        RestingRange,
        Marketplace
        
    }

    private GameState gameState;

    void Start()
    {
        /**
         * It will start with capital letters
         * Lowercase letters can then be used
         * It will end with "enemy"
        */
        this.enemyTagPattern = "^[A-Z][a-z]*Enemy$"; 
        this.waveState = 0;
        this.gameState = GameState.GameStarting;
        this.gameOverCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        int enemiesInArea = CheckEnemiesInArea();
        
        if(enemiesInArea > 0 && this.gameState == GameState.GameStarted)
        {
            this.gameState = GameState.RoundStarted;
        }

        if(enemiesInArea <= 0 && this.gameState == GameState.RoundStarted)
        {
            this.gameState = GameState.RoundOver;
            IncreaseWaveState();
        }
    }

    public int GetWaveState()
    {
        return this.waveState;
    }

    public void IncreaseWaveState()
    {
        waveState++;
        Debug.Log("Wave increased to: " + waveState);
    }

    void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;
        if (tag == "Player" && this.gameState == GameState.GameStarting)
        {
            this.gameState = GameState.GameStarted;
        }
    }

    // This function trigger bt PlatformController.cs script.
    public void SendSignalPlayerEnterToPlatform()
    {
        if(this.gameState == GameState.RoundOver){
            this.gameState = GameState.RestingRange;
        }   
    }

    // Trigger when hit to HitForNextWave box.
    public void SendSignalHitNextWave()
    {
        if(this.gameState == GameState.RestingRange)
        {
            this.gameState = GameState.GameStarting;
        }

        if(this.gameState == GameState.Marketplace)
        {
            this.gameState = GameState.RestingRange;
        }
    }

    // Trigger when hit to HitForMarketplace box.
    public void SendSignalHitMarketplace()
    {
        if(this.gameState == GameState.RestingRange)
        {
            this.gameState = GameState.Marketplace;
        }
    }

    // Game start and enemies will be spawn in x seconds.
    public bool IsGameStart()
    {
        return this.gameState == GameState.GameStarted;
    }

    public bool IsGameStarting()
    {
        return this.gameState == GameState.GameStarting;
    }

    // All enemies killed then land to platform.
    public bool IsRoundOver()
    {
        return this.gameState == GameState.RoundOver;
    }

    // Player entered to platform then player can rest and trade.
    public bool IsRestingState()
    {
        return this.gameState == GameState.RestingRange;
    }

    public bool IsMarketingState()
    {
        return this.gameState == GameState.Marketplace;
    }

    int CheckEnemiesInArea(){
        Collider[] colliders = Physics.OverlapBox(transform.position+centerOffset, boxSize);
        int count = 0;

        foreach(var collider in colliders)
        {
            string objectTag = collider.tag;
            if(!string.IsNullOrEmpty(objectTag) && Regex.IsMatch(objectTag, enemyTagPattern))
            {
                count ++;
            }

        }

        return count;
    }

    // This function needed for see to check enemies area in the scene.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + centerOffset, boxSize);
    }
}