using UnityEngine;

/**
 * Control game states and waves
 */
public class GameController : MonoBehaviour
{
    [SerializeField] private int waveState = 0;
    [SerializeField] private int maxWaveState = 3;

    private bool isGameStart = false;

    void Start()
    {
        this.waveState = 0;
        this.maxWaveState = 3;
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
        Debug.Log("Enter area this: " + tag);
        if (tag == "Player")
        {
            this.isGameStart = true;
        }
    }

    public bool IsGameStart()
    {
        return this.isGameStart;
    }
}