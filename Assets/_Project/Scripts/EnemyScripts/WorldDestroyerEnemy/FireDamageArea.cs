using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDamageArea : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 5f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private Vector3 checkSize = new Vector3(3f, 2f, 3f);
    [SerializeField] private Color gizmoColor = new Color(1f, 0.3f, 0.1f, 0.4f);
    
    private readonly List<PlayerHealth> playersInRange = new List<PlayerHealth>();
    private Dictionary<PlayerHealth, string> damageIds = new Dictionary<PlayerHealth, string>();

    private void OnEnable()
    {
        // Start checking for players in range
        StartCoroutine(CheckOverlapRoutine());
    }

    private void OnDisable()
    {
        // When disabled, remove damage from any affected players
        foreach (PlayerHealth player in playersInRange)
        {
            if (player != null && damageIds.TryGetValue(player, out string damageId))
            {
                player.RemoveDamage(damageId);
            }
        }
        playersInRange.Clear();
        damageIds.Clear();
    }

    // Generate unique damage ID using UUID
    private string GenerateUniqueId()
    {
        return "fire_" + Guid.NewGuid().ToString();
    }

    // Check for players in the overlap area
    private IEnumerator CheckOverlapRoutine()
    {
        while (enabled)
        {
            // Perform the overlap check
            Collider[] colliders = Physics.OverlapBox(
                transform.position,
                checkSize / 2f,
                transform.rotation
            );

            // Track new players that entered the area
            List<PlayerHealth> currentPlayersInRange = new List<PlayerHealth>();

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    // Get the PlayerHealth component
                    PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
                    
                    if (playerHealth != null && playerHealth.Alive())
                    {
                        currentPlayersInRange.Add(playerHealth);
                        
                        // If this is a new player in range, apply damage
                        if (!playersInRange.Contains(playerHealth))
                        {
                            string uniqueDamageId = GenerateUniqueId();
                            playerHealth.AddDamage(damagePerSecond, uniqueDamageId, damageInterval);
                            damageIds[playerHealth] = uniqueDamageId;
                            Debug.Log($"Player entered fire area, applying {damagePerSecond} damage per second with ID: {uniqueDamageId}");
                        }
                    }
                }
            }

            // Remove damage from players who left the area
            foreach (PlayerHealth player in playersInRange)
            {
                if (!currentPlayersInRange.Contains(player) && player != null)
                {
                    if (damageIds.TryGetValue(player, out string damageId))
                    {
                        player.RemoveDamage(damageId);
                        damageIds.Remove(player);
                        Debug.Log($"Player left fire area, removing damage with ID: {damageId}");
                    }
                }
            }

            // Update the list of players in range
            playersInRange.Clear();
            playersInRange.AddRange(currentPlayersInRange);

            // Wait before next check
            yield return new WaitForSeconds(0.2f);
        }
    }

    // Draw gizmos to visualize the damage area
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, checkSize);
    }
}