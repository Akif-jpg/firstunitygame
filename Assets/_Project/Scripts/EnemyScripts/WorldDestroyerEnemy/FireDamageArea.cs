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
    private string damageId;

    private void OnEnable()
    {
        damageId = "fire_" + Guid.NewGuid().ToString();
        StartCoroutine(CheckOverlapRoutine());
    }

    private void OnDisable()
    {
        foreach (PlayerHealth player in playersInRange)
        {
            if (player != null)
            {
                player.RemoveDamage(damageId);
            }
        }
        playersInRange.Clear();
    }

    private IEnumerator CheckOverlapRoutine()
    {
        while (enabled)
        {
            Collider[] colliders = Physics.OverlapBox(
                transform.position,
                checkSize / 2f,
                transform.rotation
            );

            List<PlayerHealth> currentPlayersInRange = new List<PlayerHealth>();

            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();

                    if (playerHealth != null && playerHealth.Alive())
                    {
                        currentPlayersInRange.Add(playerHealth);

                        if (!playersInRange.Contains(playerHealth))
                        {
                            playerHealth.AddDamage(damagePerSecond, damageId, damageInterval);
                            Debug.Log($"Player entered fire area, applying damage with ID: {damageId}");
                        }
                    }
                }
            }

            foreach (PlayerHealth player in playersInRange)
            {
                if (!currentPlayersInRange.Contains(player) && player != null)
                {
                    player.RemoveDamage(damageId);
                    Debug.Log($"Player left fire area, removing damage with ID: {damageId}");
                }
            }

            playersInRange.Clear();
            playersInRange.AddRange(currentPlayersInRange);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, checkSize);
    }
}
