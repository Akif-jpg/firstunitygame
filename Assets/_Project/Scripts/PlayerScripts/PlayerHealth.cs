using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameController gameController;
    private bool isCharacterAlive = true;
    private float characterHealth;

    // Track currently active damage routines
    private Dictionary<string, DamageRoutine> activeRoutines = new Dictionary<string, DamageRoutine>();

    void Start()
    {
        // Initialize health and make sure game over canvas is hidden at start
        characterHealth = 100f;
        isCharacterAlive = true;    
    }

    public PlayerHealth()
    {
        characterHealth = 100f;
        isCharacterAlive = true;
    }

    // Returns whether the character is alive
    public bool Alive() => isCharacterAlive;

    // Get current health value
    public float GetCharacterHealth() => characterHealth;

    // Set current health and update UI
    public void SetCharacterHealth(float characterHealth)
    {
        this.characterHealth = characterHealth;
        this.healthText.text = "" + characterHealth;
    }

    // Start a new damage-over-time routine if not already active
    public void AddDamage(float damagePerSecond, string damageId, float interval = 1f)
    {
        if (activeRoutines.ContainsKey(damageId))
            return; // Do not restart if already active

        DamageRoutine routine = new DamageRoutine(damageId, damagePerSecond, interval);
        routine.Start(this, ApplyDamage);
        activeRoutines.Add(damageId, routine);
    }

    // Stop and remove a running damage routine
    public void RemoveDamage(string damageId)
    {
        if (activeRoutines.TryGetValue(damageId, out var routine))
        {
            routine.Stop(this);
            activeRoutines.Remove(damageId);
        }
    }

    // Apply damage to the player
    private void ApplyDamage(float amount)
    {
        if (!isCharacterAlive) return;

        characterHealth -= amount;
        Debug.Log($"Player took {amount} damage. Health: {characterHealth}");

        if (healthText != null)
        {
            healthText.text = "" + characterHealth;

            if (this.characterHealth < 30f)
            {
                healthText.color = Color.red;
            }
        }

        if (characterHealth <= 0f)
        {
            Die();
        }
    }

    // Handle player death
    private void Die()
    {
        isCharacterAlive = false;
        Debug.Log("Character is dead.");

        // Unlock and show mouse cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show game over UI
        if (gameController != null)
        {
            gameController.LoadGameOverCanvas();
            Debug.Log("Game over canvas activated!");
        }
        else
        {
            Debug.LogError("Game Over Canvas not assigned in the inspector!");
        }

        // Pause the game
        Time.timeScale = 0f;
    }
}