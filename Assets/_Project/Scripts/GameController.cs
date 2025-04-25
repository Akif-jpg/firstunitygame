using System;
using System.Text.RegularExpressions;
using UnityEngine;

/**
 * Control game states and waves
 */
public class GameController : MonoBehaviour
{
    [Header("Wave settings")]
    [SerializeField] private int waveState = 0;
    [SerializeField] private int maxWaveState = 3;

    [Header("Check enemies area settings")]
    [Tooltip("Check enemies area for starting new round.")]
    [SerializeField] private Vector3 boxSize = new Vector3(20f,5f,20f); //Check enemies area boxsize
    [Tooltip("Area offset for check enemies area.")]
    [SerializeField] private Vector3 centerOffset = new Vector3(0f,2.5f,0f); // Offset position for scan area to enemy count.

    private String enemyTagPattern;

    private enum GameState {
        GameStarting,
        GameStarted,
        RoundStarting,
        RoundStarted,
        RoundOver,
        RestingRange
        
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
        this.maxWaveState = 3;
        this.gameState = GameState.GameStarting;
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
            if(this.waveState < this.maxWaveState)
            {
                IncreaseWaveState();
            }
        }
    }

    public int GetWaveState()
    {
        return this.waveState;
    }

    public int GetMaxWaveState()
    {
        return this.maxWaveState;
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

    public void SendSignalHitNextWave()
    {
        if(this.gameState == GameState.RestingRange && waveState < maxWaveState)
        {
            this.gameState = GameState.GameStarting;
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