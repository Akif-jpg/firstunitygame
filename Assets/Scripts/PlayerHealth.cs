using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    private bool isCharacterAlive = true;
    private float characterHealth;
    private DamageIdProvider dIdProvider;

    // Damage following system.
    private Dictionary<string, DamageRoutine> activeRoutines = new Dictionary<string, DamageRoutine>();

    public PlayerHealth()
    {
        characterHealth = 100f;
        isCharacterAlive = true;
        this.dIdProvider = new DamageIdProvider();
    }

    public bool Alive() => isCharacterAlive;

    public float GetCharacterHealth() => characterHealth;

    public void SetCharacterHealth(float characterHealth)
    {
        this.characterHealth = characterHealth;
        this.healthText.text = "Health: " + characterHealth;
    }

    public void AddDamage(float damagePerSecond, string damageId, float interval = 1f)
    {
        damageId = this.dIdProvider.Add(damageId);
        if (activeRoutines.ContainsKey(damageId))
            return; // Zaten çalışıyorsa tekrar başlatma

        DamageRoutine routine = new DamageRoutine(damageId, damagePerSecond, interval);
        routine.Start(this, ApplyDamage);
        activeRoutines.Add(damageId, routine);
    }

    public void RemoveDamage(string damageId)
    {
        string damageIdCopy = damageId;
        damageId = this.dIdProvider.GetCount(damageId);
        if (activeRoutines.TryGetValue(damageId, out var routine))
        {
            routine.Stop(this);
            activeRoutines.Remove(damageId);
            this.dIdProvider.Remove(damageIdCopy);
        }
    }

    private void ApplyDamage(float amount)
    {
        if (!isCharacterAlive) return;

        characterHealth -= amount;
        Debug.Log($"Player took {amount} damage. Health: {characterHealth}");

        this.healthText.text = "Health: " + characterHealth;

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
