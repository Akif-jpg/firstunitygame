using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float followRange = 1.6f;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem destroyParticle;
    [SerializeField] private ParticleSystem damageParticle;
    [SerializeField] private AudioSource destroyAudio;
    private NavMeshAgent navMeshAgent;
    private EnemyHealth enemyHealth;

    private bool isDeathStatusStarted = false;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;

        this.enemyHealth = new EnemyHealth(20, 40);

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
        if (playerTransform != null && enemyHealth.IsEnemyAlive())
        {
            Vector3 direction = this.transform.position - playerTransform.position;

            if (direction.sqrMagnitude > 0.01f)
            {
                Vector3 destination = playerTransform.position + direction.normalized * this.followRange;

                if ((destination - this.transform.position).sqrMagnitude > 0.01f)
                {
                    navMeshAgent.SetDestination(destination);

                    // Geri geri giderken sürekli oyuncuya bakmasını sağla
                    Vector3 lookDirection = playerTransform.position - this.transform.position;
                    lookDirection.y = 0; // Yükseklik farkını yok sayalım
                    if (lookDirection.sqrMagnitude > 0.001f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 5f); // 5 burada dönüş hızı
                    }
                }
            }
        }

        // If enemy is death.
        if (!enemyHealth.IsEnemyAlive() && !isDeathStatusStarted)
        {
            isDeathStatusStarted = true;
            StartCoroutine(DestroyAnimation());
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

        if (tagName == DamageAreas.PLAYER_RIFFLE_BULLET)
        {
            StartCoroutine(DamageAnimation());
            // Apply damage to the enemy
            this.enemyHealth.AddDamage(DamageAreas.PLAYER_RIFFLE_BULLET_VALUE);
            Destroy(other.gameObject);

        }


        if (tagName == DamageAreas.PLASMA_BOMB_DAMAGE_AREA)
        {
            StartCoroutine(DamageAnimation());
            this.enemyHealth.AddDamage(DamageAreas.PLASMA_BOMB_DAMAGE_AREA_VALUE);
        }
    }

    IEnumerator DamageAnimation()
    {
        animator.SetBool("IsHurt", true);
        damageParticle.Play();
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("IsHurt", false);
        damageParticle.Stop();
    }

    IEnumerator DestroyAnimation()
    {
        destroyParticle.Play();
        destroyAudio.Play();
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }

}