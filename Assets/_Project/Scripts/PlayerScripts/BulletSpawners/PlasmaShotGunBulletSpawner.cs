using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.Mathematics;

public class PlasmaShotgunSpawner : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;           // Plasma bullet prefab to spawn
    [SerializeField] private float bulletBaseSpeed = 21f;       // Base speed of bullets (lower than regular gun for better close-range effect)
    [SerializeField] private float speedVariation = 2f;         // Random variation in bullet speed
    [SerializeField] private int pelletCount = 6;               // Number of plasma pellets to spawn at once
    [SerializeField] private float spreadAngle = 15f;           // Maximum angle of bullet spread
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleEffect;       // Muzzle flash effect
    [SerializeField] private AudioSource audioSource;           // Audio source for shotgun sound
    
    [Header("Weapon Properties")]
    [SerializeField] private float cooldownTime = 1.2f;         // Cooldown between shots (longer than regular gun)
    
    [Header("Ammo System")]
    [SerializeField] private int maxAmmo = 12;                  // Maximum ammo capacity for shotgun
    [SerializeField] private int currentAmmo;                   // Current ammo count
    [SerializeField] private int ammoPerShot = 1;               // Ammo used per shot
    [SerializeField] private TextMeshProUGUI ammoText;          // Reference to UI text for displaying ammo
    [SerializeField] private float reloadTime = 3.0f;           // Time it takes to reload
    
    private Animator animator;                // Reference to parent's animator component
    private float cooldownTimer = 0f;         // Timer to track cooldown
    private bool isReloading = false;         // Flag to track if weapon is reloading
    private float nextFireTime = 0f;          // Time when weapon can fire next
    
    void Start()
    {
        // Get the animator component from parent object
        animator = transform.parent.GetComponent<Animator>();
        
        // Initialize ammo to max capacity
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }
    
    void Update()
    {
        // Only update UI when needed instead of every frame
        if (ammoText != null && ammoText.text != currentAmmo + " / " + maxAmmo)
        {
            UpdateAmmoText();
        }
        
        // Handle cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        
        // Check for fire input when weapon is ready, has ammo, and is not reloading
        if (Input.GetMouseButtonDown(0) && CanFire())
        {
            FireShotgun();
        }
        
        // Check for reload input
        if (Input.GetKeyDown(KeyCode.R) && CanReload())
        {
            StartReload();
        }
    }
    
    // Check if weapon can fire
    private bool CanFire()
    {
        return !isReloading && cooldownTimer <= 0 && currentAmmo >= ammoPerShot && Time.time >= nextFireTime;
    }
    
    // Check if weapon can reload
    private bool CanReload()
    {
        return !isReloading && currentAmmo < maxAmmo;
    }
    
    // Coroutine to play fire animation
    IEnumerator PlayFireAnimation()
    {
        animator.SetBool("Fire", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Fire", false);
    }
    
    // Main shotgun firing method
    void FireShotgun()
    {
        // Play muzzle effect
        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }
        
        // Play sound effect
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        // Spawn bullets
        StartCoroutine(PlasmaBulletSpawner());
        
        // Play animation
        StartCoroutine(PlayFireAnimation());
        
        // Set cooldown
        cooldownTimer = cooldownTime;
        nextFireTime = Time.time + cooldownTime;
        
        // Decrease ammo and update UI
        currentAmmo -= ammoPerShot;
        UpdateAmmoText();
    }

    IEnumerator PlasmaBulletSpawner()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            SpawnPlasmaPellet(i);
            yield return new WaitForSeconds(0.01f);
        }
    }
    
    // Method to spawn individual plasma pellets with spread
    void SpawnPlasmaPellet(int index)
    {
        // Calculate spread direction
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
        Rigidbody rb = bullet.GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            // Calculate spread angle based on pellet index
            float spreadStep = spreadAngle / (pelletCount - 1);
            float spreadOffset = -spreadAngle / 2 + spreadStep * index;

            // Apply the calculated spread to the bullet's direction
            Vector3 spreadDirection = Quaternion.Euler(UnityEngine.Random.Range(-spreadAngle/2, spreadAngle/2), spreadOffset, UnityEngine.Random.Range(-1f, 1f)) * Vector3.forward;

            // Set the bullet's velocity with the spread direction and speed
            float actualSpeed = bulletBaseSpeed + UnityEngine.Random.Range(-speedVariation, speedVariation);
            rb.velocity = spawnRotation * spreadDirection * actualSpeed;
        }
        else
        {
            Debug.LogWarning("Bullet rigidbody is Null");
        }
    }
    
    // Update the ammo count display
    void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }
    
    // Method to start reload process
    public void StartReload()
    {
        // Set reloading flag to true to prevent firing
        isReloading = true;
        StartCoroutine(ReloadRoutine());
    }
    
    // Coroutine for reload animation and timing
    IEnumerator ReloadRoutine()
    {
        Debug.Log("Starting reload sequence");
        
        // Start reload animation
        animator.SetBool("Reloading", true);
        
        // Wait for animation transition
        yield return new WaitForSeconds(1f);
        
        // Turn off reload animation trigger but keep reloading state
        animator.SetBool("Reloading", false);
        
        // Continue waiting for full reload time
        yield return new WaitForSeconds(reloadTime - 1f);
        
        // Reset ammo count
        currentAmmo = maxAmmo;
        
        // Set reloading flag to false to allow firing again
        isReloading = false;
        
        Debug.Log("Reload complete");
    }

    public void AddAdditionalAmmo(int additionalAmmo)
    {
        this.currentAmmo += additionalAmmo;
    }
}