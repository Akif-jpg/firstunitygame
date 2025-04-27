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
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 1f;
    
    [Header("Accuracy Settings")]
    [SerializeField] private float maxSpreadAngle = 15f; // Maximum angle of bullet spread in degrees
    [SerializeField] private float minSpreadAngle = 3f;  // Minimum angle of bullet spread in degrees
    [SerializeField] private float accuracyByDistance = 0.5f; // Higher values mean less accurate at greater distances
    [SerializeField] private bool visualizeSpread = true; // For debugging
    
    [Header("Burst Settings")]
    [SerializeField] private int bulletsPerBurst = 3; // How many bullets to fire in a burst
    [SerializeField] private float burstDelay = 0.15f; // Delay between bullets in a burst

    [Header("ParticleSystem")]
    [SerializeField] private GameObject muzzleFlashPrefab; // Atış efekti prefab'ı
    [SerializeField] private GameObject impactEffectPrefab; // Mermi çarpma efekti prefab'ı
    [SerializeField] private float effectDestroyTime = 2f; // Efekt yok olma süresi
    
    private Transform playerTransform;
    private float nextFireTime;
    private bool isFiring = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // Initialize the next fire time
        nextFireTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player is in range using raycast
        DetectPlayer();
        
        // If player is detected and it's time to fire, shoot a bullet
        if (playerTransform != null && Time.time >= nextFireTime && !isFiring)
        {
            StartCoroutine(FireBurst());
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
    
    private IEnumerator FireBurst()
    {
        isFiring = true;
        
        for (int i = 0; i < bulletsPerBurst; i++)
        {
            ShootBullet();
            yield return new WaitForSeconds(burstDelay);
        }
        
        isFiring = false;
    }
    
    private void ShootBullet()
    {
        if (bulletPrefab == null || firePoint == null || playerTransform == null)
        {
            Debug.LogWarning("Bullet prefab, fire point, or player not available!");
            return;
        }
        
        // Look at the player
        transform.LookAt(playerTransform);
        
        // Calculate base direction to player
        Vector3 baseDirection = (playerTransform.position - firePoint.position).normalized;
        
        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Calculate spread angle based on distance (more spread at greater distances)
        float currentSpreadAngle = Mathf.Lerp(minSpreadAngle, maxSpreadAngle, 
                                             distanceToPlayer * accuracyByDistance / detectionRange);
        
        // Apply random spread to the direction
        Vector3 spreadDirection = ApplySpread(baseDirection, currentSpreadAngle);
        
        // Optional debug visualization
        if (visualizeSpread)
        {
            Debug.DrawRay(firePoint.position, baseDirection * 5f, Color.red, 1f);
            Debug.DrawRay(firePoint.position, spreadDirection * 5f, Color.green, 1f);
        }
        
        // Create muzzle flash effect with correct rotation
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, Quaternion.LookRotation(spreadDirection));
            // Alternatif olarak muzzleFlash'ı firePoint'in child'ı yapabiliriz
            // muzzleFlash.transform.SetParent(firePoint);
            Destroy(muzzleFlash, effectDestroyTime);
            
            // Particle sistemini aktifleştir (eğer başlangıçta deaktif ise)
            ParticleSystem[] particleSystems = muzzleFlash.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
        }
        
        // Create the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(spreadDirection));
        
        // Get the rigidbody component and apply velocity
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply velocity to the bullet
            rb.velocity = spreadDirection * bulletSpeed;
            
            // Make sure bullet is oriented in the direction of travel
            bullet.transform.forward = spreadDirection;
            
            // Add component to handle bullet collision and impact effect
            BulletImpactHandler impactHandler = bullet.AddComponent<BulletImpactHandler>();
            if (impactHandler != null && impactEffectPrefab != null)
            {
                impactHandler.Initialize(impactEffectPrefab, effectDestroyTime);
            }
        }
        else
        {
            Debug.LogWarning("Bullet prefab does not have a Rigidbody component!");
        }
        
        // Destroy bullet after some time to avoid memory leaks
        Destroy(bullet, 5f);
    }
    
    private Vector3 ApplySpread(Vector3 baseDirection, float spreadAngle)
    {
        // Create a random spread within a cone
        float randomAngleX = Random.Range(-spreadAngle, spreadAngle);
        float randomAngleY = Random.Range(-spreadAngle, spreadAngle);
        float randomAngleZ = Random.Range(-spreadAngle, spreadAngle);
        
        // Apply rotation to the base direction
        Quaternion spreadRotation = Quaternion.Euler(randomAngleX, randomAngleY, randomAngleZ);
        return spreadRotation * baseDirection;
    }
    
    // Visualize the detection range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (firePoint != null && visualizeSpread)
        {
            // Draw the forward direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * detectionRange);
            
            // Draw the potential spread cone
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange, semi-transparent
            
            // Approximating a cone with lines
            int numLines = 8;
            float angleStep = 360f / numLines;
            
            for (int i = 0; i < numLines; i++)
            {
                float angle = i * angleStep;
                Vector3 direction = Quaternion.AngleAxis(angle, firePoint.forward) * (Quaternion.AngleAxis(maxSpreadAngle, Vector3.right) * firePoint.forward);
                Gizmos.DrawLine(firePoint.position, firePoint.position + direction * detectionRange * 0.8f);
            }
        }
    }
}

// Merminin çarpma olayını yönetecek yardımcı sınıf
public class BulletImpactHandler : MonoBehaviour
{
    private GameObject impactEffectPrefab;
    private float effectDestroyTime;
    
    public void Initialize(GameObject impactEffect, float destroyTime)
    {
        impactEffectPrefab = impactEffect;
        effectDestroyTime = destroyTime;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Çarpışma noktasını ve normal vektörünü al
        ContactPoint contact = collision.contacts[0];
        Vector3 position = contact.point;
        Quaternion rotation = Quaternion.LookRotation(contact.normal);
        
        // Çarpma efektini oluştur
        if (impactEffectPrefab != null)
        {
            GameObject impactEffect = Instantiate(impactEffectPrefab, position, rotation);
            
            // Particle sistemini aktifleştir
            ParticleSystem[] particleSystems = impactEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ps.Play();
            }
            
            // Belirli bir süre sonra efekti yok et
            Destroy(impactEffect, effectDestroyTime);
        }
        
        // Mermiyi yok et
        Destroy(gameObject);
    }
}