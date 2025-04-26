using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem destroyParticle;
    private NavMeshAgent navMeshAgent;
    private EnemyHealth enemyHealth;

    // Variable to track if the enemy was moving in the previous frame
    private bool wasMoving = false;
    // Threshold to determine if the enemy is considered moving
    private float movementThreshold = 0.1f;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        this.enemyHealth = new EnemyHealth();

        // Make sure animator is assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Try to find player if not already set
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Player not found by Enemy " + gameObject.name);
            }
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 destination = playerTransform.position + Vector3.Normalize(this.transform.position - playerTransform.position) * 1.6f;
            navMeshAgent.SetDestination(destination);
        }

        // Update Animator parameter based on movement
        UpdateMovementAnimation();

        // If enemy is death.
        if (!enemyHealth.IsEnemyAlive())
        {
            StartCoroutine(DestroyAnimation());
        }
    }

    private void UpdateMovementAnimation()
    {
        if (animator != null)
        {
            // Check if the enemy is currently moving (using velocity magnitude)
            bool isMoving = navMeshAgent.velocity.magnitude > movementThreshold;

            // Only update the animator if the movement state has changed
            if (isMoving != wasMoving)
            {
                animator.SetBool("IsEnemyMoving", isMoving);
                wasMoving = isMoving;
            }
        }
    }

    // Method to set player transform reference
    public void SetPlayerTransform(Transform player)
    {
        this.playerTransform = player;
    }

    void OnTriggerEnter(Collider other)
    {
        string tagName = other.tag;

        // If enemy hitted by bullet throw by plater.
        if (tagName == DamageAreas.PLAYER_BULLET)
        {
            StartCoroutine(DamageAnimation());
            // Apply damage to the enemy
            this.enemyHealth.AddDamage(DamageAreas.PLAYER_BULLET_VALUE);
            Destroy(other.gameObject);

        }
    }

    IEnumerator DamageAnimation()
    {
        animator.SetBool("IsHurt", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("IsHurt", false);
    }

    IEnumerator DestroyAnimation()
    {
        destroyParticle.Play();
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }

}