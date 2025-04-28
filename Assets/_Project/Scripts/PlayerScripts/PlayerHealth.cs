using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    private bool isCharacterAlive = true;
    private float characterHealth;


    // Damage following system.
    private Dictionary<string, DamageRoutine> activeRoutines = new Dictionary<string, DamageRoutine>();

    public PlayerHealth()
    {
        characterHealth = 100f;
        isCharacterAlive = true;
    }

    public bool Alive() => isCharacterAlive;

    public float GetCharacterHealth() => characterHealth;

    public void SetCharacterHealth(float characterHealth)
    {
        this.characterHealth = characterHealth;
        this.healthText.text = "" + characterHealth;
    }

    public void AddDamage(float damagePerSecond, string damageId, float interval = 1f)
    {
        if (activeRoutines.ContainsKey(damageId))
            return; // Zaten çalışıyorsa tekrar başlatma

        DamageRoutine routine = new DamageRoutine(damageId, damagePerSecond, interval);
        routine.Start(this, ApplyDamage);
        activeRoutines.Add(damageId, routine);
    }

    public void RemoveDamage(string damageId)
    {

        if (activeRoutines.TryGetValue(damageId, out var routine))
        {
            routine.Stop(this);
            activeRoutines.Remove(damageId);
        }
    }

    private void ApplyDamage(float amount)
    {
        if (!isCharacterAlive) return;

        characterHealth -= amount;
        Debug.Log($"Player took {amount} damage. Health: {characterHealth}");

        this.healthText.text = "" + characterHealth;

        if(this.characterHealth < 30f)
        {
            this.healthText.color = Color.red;
        }

        if (characterHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isCharacterAlive = false;
        Debug.Log("Character is dead.");
        // Ölüm animasyonu veya oyun sonu ekranı burada olabilir.
    }
}
