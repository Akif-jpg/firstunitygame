using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlasmaShotgunSpawner : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;           // Plasma bullet prefab to spawn
    public float bulletBaseSpeed = 21f;       // Base speed of bullets (lower than regular gun for better close-range effect)
    public float speedVariation = 2f;         // Random variation in bullet speed
    public int pelletCount = 6;               // Number of plasma pellets to spawn at once
    public float spreadAngle = 15f;           // Maximum angle of bullet spread
    
    [Header("Effects")]
    public ParticleSystem muzzleEffect;       // Muzzle flash effect
    public AudioSource audioSource;           // Audio source for shotgun sound
    
    [Header("Weapon Properties")]
    public float cooldownTime = 1.2f;         // Cooldown between shots (longer than regular gun)
    public bool canFire = true;               // Flag to track if weapon can fire
    
    [Header("Ammo System")]
    public int maxAmmo = 12;                  // Maximum ammo capacity for shotgun
    public int currentAmmo;                   // Current ammo count
    public int ammoPerShot = 1;               // Ammo used per shot
    public TextMeshProUGUI ammoText;          // Reference to UI text for displaying ammo
    
    private Animator animator;                // Reference to parent's animator component
    private float cooldownTimer = 0f;         // Timer to track cooldown
    
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
        UpdateAmmoText();
        // Handle cooldown timer
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            canFire = false;
        }
        else if (!canFire)
        {
            canFire = true;
        }
        
        // Check for fire input when weapon is ready and has ammo
        if (Input.GetMouseButtonDown(0) && canFire && currentAmmo >= ammoPerShot)
        {
            FireShotgun();
            StartCoroutine(PlayFireAnimation());
            cooldownTimer = cooldownTime;
            canFire = false;
            
            // Decrease ammo and update UI
            currentAmmo -= ammoPerShot;
        }
        
        // Option to reload (could use a different key like 'R')
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            Reload();
        }
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
        
        // Spawn multiple pellets with random spread
        for (int i = 0; i < pelletCount; i++)
        {
            SpawnPlasmaPellet();
        }
    }
    
    // Method to spawn individual plasma pellets with spread
    void SpawnPlasmaPellet()
    {
        // Calculate spread direction
        Vector3 spreadDirection = CalculateSpreadDirection();
        
        // Create the bullet at the spawn point
        Vector3 spawnPosition = transform.position;
        GameObject pellet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        // Get and set the rigidbody velocity with random speed variation
        Rigidbody rb = pellet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Add random variation to speed for each pellet
            float randomSpeed = bulletBaseSpeed + Random.Range(-speedVariation, speedVariation);
            rb.velocity = spreadDirection * randomSpeed;
        }
        
        // Set bullet lifetime for better performance
        Destroy(pellet, 5.0f);
    }
    
    // Calculate a random direction within the spread angle
    Vector3 CalculateSpreadDirection()
    {
        // Get the forward direction of the gun
        Vector3 forwardDir = transform.forward;
        
        // Add random spread within the specified angle
        float randomSpreadX = Random.Range(-spreadAngle, spreadAngle);
        float randomSpreadY = Random.Range(-spreadAngle, spreadAngle);
        
        // Apply the random spread to the forward direction
        Quaternion spreadRotation = Quaternion.Euler(randomSpreadX, randomSpreadY, 0);
        Vector3 spreadDirection = spreadRotation * forwardDir;
        
        return spreadDirection.normalized;
    }
    
    // Update the ammo count display
    void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }
    
    // Method to reload the weapon
    public void Reload()
    {
        // Could add reload animation or sound here
        StartCoroutine(ReloadRoutine());
    }
    
    // Coroutine for reload animation and timing
    IEnumerator ReloadRoutine()
    {
        // Could set reloading animation flag here
        // animator.SetBool("Reloading", true);
        
        // Wait for reload time
        yield return new WaitForSeconds(2.0f);
        
        // Reset ammo count
        currentAmmo = maxAmmo;
        UpdateAmmoText();
        
        // Turn off reload animation
        // animator.SetBool("Reloading", false);
    }
}