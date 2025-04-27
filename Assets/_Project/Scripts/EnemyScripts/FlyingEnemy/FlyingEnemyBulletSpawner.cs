using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyBulletSpawner : MonoBehaviour
{
    [Header("Target Detection")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask playerLayerMask;
    
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 1f;
    
    private Transform playerTransform;
    private float nextFireTime;
    private Transform firePoint;
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialize the next fire time
        nextFireTime = Time.time;
        this.firePoint = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player is in range using raycast
        DetectPlayer();
        
        // If player is detected and it's time to fire, shoot a bullet
        if (playerTransform != null && Time.time >= nextFireTime)
        {
            ShootBullet();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
    
    private void DetectPlayer()
    {
        // Cast a ray in the forward direction of the enemy
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRange, playerLayerMask))
        {
            // Check if the hit object has the "Player" tag
            if (hit.collider.CompareTag("Player"))
            {
                playerTransform = hit.transform;
                return;
            }
        }
        
        // Alternative: Sphere cast to detect player in any direction
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayerMask);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                // Check if there's a clear line of sight to the player
                Vector3 directionToPlayer = (collider.transform.position - transform.position).normalized;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerTransform = hit.transform;
                        return;
                    }
                }
            }
        }
        
        // If player is not in range or not in line of sight, set to null
        playerTransform = null;
    }
    
    private void ShootBullet()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Bullet prefab or fire point not assigned!");
            return;
        }
        
        // Look at the player
        transform.LookAt(playerTransform);
        
        // Create the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Get the rigidbody component and apply velocity
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Calculate direction to player
            Vector3 direction = (playerTransform.position - firePoint.position).normalized;
            
            // Apply velocity to the bullet
            rb.velocity = direction * bulletSpeed;
            
            // Optional: Make sure bullet is oriented in the direction of travel
            bullet.transform.forward = direction;
        }
        else
        {
            Debug.LogWarning("Bullet prefab does not have a Rigidbody component!");
        }
        
        // Optional: Destroy bullet after some time to avoid memory leaks
        Destroy(bullet, 5f);
    }
    
    // Optional: Visualize the detection range in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (firePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * detectionRange);
        }
    }
}