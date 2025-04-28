using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public ParticleSystem muzzleEffect;
    private Animator animator;
    public AudioSource audioSource;       // AudioSource for playing sounds
    public float time = 0.5f;
    public bool fire = false;
    
    // Track bullet count for reload
    private int bulletsFired = 0;
    public int bulletsBeforeReload = 6;
    private bool isReloading = false;
    public float reloadTime = 3f;

    void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
    }

    void Update()
    {
        // Cooldown timer for firing
        if (time > 0 && fire)
        {
            time -= Time.deltaTime;
        }
        else
        {
            time = 0.5f;
            fire = false;
        }

        // Fire when left mouse button is clicked and not in cooldown or reload
        if (Input.GetMouseButtonDown(0) && !fire && !isReloading)
        {
            SpawnBullet();
            StartCoroutine(FireAnimation());
            fire = true;
            
            // Increment bullet counter
            bulletsFired++;
            
            // Check if we need to reload
            if (bulletsFired >= bulletsBeforeReload)
            {
                StartCoroutine(ReloadAnimation());
            }
        }
    }

    IEnumerator FireAnimation()
    {
        animator.SetBool("Fire", true);
        yield return new WaitForSeconds(0.1f); // Changed time from 0.5 to 0.1 seconds
        animator.SetBool("Fire", false);
    }
    
    IEnumerator ReloadAnimation()
    {
        // Start reload process
        isReloading = true;
        animator.SetBool("Reload", true);
        
        // Wait for reload animation
        yield return new WaitForSeconds(reloadTime);
        
        // End reload
        animator.SetBool("Reload", false);
        bulletsFired = 0;
        isReloading = false;
    }

    void SpawnBullet()
    {
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = spawnRotation * Vector3.forward * bulletSpeed;
        }
        
        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }
        
        // Play sound effect
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}