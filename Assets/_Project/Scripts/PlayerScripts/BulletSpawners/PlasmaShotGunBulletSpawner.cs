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
    [SerializeField] private bool canFire = true;               // Flag to track if weapon can fire
    
    [Header("Ammo System")]
    [SerializeField] private int maxAmmo = 12;                  // Maximum ammo capacity for shotgun
    [SerializeField] private int currentAmmo;                   // Current ammo count
    [SerializeField] private int ammoPerShot = 1;               // Ammo used per shot
    [SerializeField] private TextMeshProUGUI ammoText;          // Reference to UI text for displaying ammo
    
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
        

        StartCoroutine(PlasmaBulletSpawner());

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
            rb.velocity = spawnRotation * spreadDirection * bulletBaseSpeed;
        }
        else
        {
            Debug.Log("Bullet rigidbody Null");
        }
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
        yield return new WaitForSeconds(3.0f);
        
        // Reset ammo count
        currentAmmo = maxAmmo;
        UpdateAmmoText();
        
        // Turn off reload animation
        // animator.SetBool("Reloading", false);
    }
}