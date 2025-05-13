using UnityEngine;
using UnityEngine.SceneManagement;

public class GamePauseController : MonoBehaviour
{
    [Tooltip("GamePause screen object reference for display when game paused")]
    [SerializeField] private GameObject gamePauseCanvas;

    [Tooltip("Cursor visibility when playing the game")]
    [SerializeField] private bool hideCursorDuringGameplay = true;

    // Keep track of pause state
    private bool isPaused = false;

    private void Start()
    {
        // Initialize cursor state for gameplay
        SetCursorForGameplay();
    }

    public void Update()
    {
        // Stop game and show UI when pressed Esc or P keys
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void ReturnToMainMenu()
    {
        // Make sure time is back to normal before loading the main menu
        Time.timeScale = 1f;
        // Show cursor and unlock it before returning to menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Load the main menu scene - assuming it has index 0
        SceneManager.LoadScene(0);
    }

    public void ResumeGame()
    {
        // Hide the pause menu
        gamePauseCanvas.SetActive(false);
        // Set time scale back to normal
        Time.timeScale = 1f;
        // Update pause state
        isPaused = false;
        // Lock and hide cursor for gameplay
        SetCursorForGameplay();
    }

    public void PauseGame()
    {
        // Show the pause menu
        gamePauseCanvas.SetActive(true);
        // Freeze the game
        Time.timeScale = 0f;  
        // Update pause state
        isPaused = true;
        // Show cursor and unlock it for menu interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetCursorForGameplay()
    {
        if (hideCursorDuringGameplay)
        {
            // Hide and lock cursor during gameplay
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // Keep cursor visible but confined to the game window
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}