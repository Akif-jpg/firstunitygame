using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls the plasma rifle weapon, handling firing, ammo management, and visual/audio effects
/// </summary>
public class PlasmaRif : MonoBehaviour
{
    [Header("Bullet Settings")]
    [Tooltip("The bullet prefab that will be instantiated when firing")]
    public GameObject bulletPrefab;
    
    [Tooltip("Speed at which bullets will travel")]
    public float bulletSpeed = 10f;
    
    [Header("Effects")]
    [Tooltip("Particle system for muzzle flash when firing")]
    public ParticleSystem muzzleEffect;
    
    [Tooltip("Audio source component for playing firing sounds")]
    public AudioSource audioSource;       // AudioSource for playing sounds
    
    [Header("Weapon Properties")]
    [Tooltip("Time between shots in seconds")]
    public float fireRate = 0.5f;         // Fire rate (shots every 0.5 seconds)
    
    [Tooltip("If true, weapon will continuously fire while mouse button is held")]
    public bool isAutomatic = true;       // Toggle automatic fire mode
    
    [Header("Ammo System")]
    [Tooltip("Maximum ammo capacity for this weapon")]
    public int maxAmmo = 30;              // Maximum ammo capacity
    
    [Tooltip("Current amount of ammo in the weapon")]
    public int currentAmmo;               // Current ammo count
    
    [Tooltip("UI text element to display current ammo count")]
    public TextMeshProUGUI ammoText;      // Reference to UI text for displaying ammo
    
    /// <summary>
    /// Reference to the parent object's animator component
    /// </summary>
    private Animator animator;
    
    /// <summary>
    /// Tracks when the weapon can fire next based on fire rate
    /// </summary>
    private float nextFireTime = 0f;      // Time for next fire
    
    /// <summary>
    /// Initialize the weapon on start
    /// </summary>
    void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
        // Initialize ammo to max capacity
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }
    
    /// <summary>
    /// Handle weapon firing and ammo management each frame
    /// </summary>
    void Update()
    {
        UpdateAmmoText();
        if (isAutomatic)
        {
            // Automatic fire - continuously fires while button is held down
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                SpawnBullet();
                StartCoroutine(FireAnimation());
                nextFireTime = Time.time + fireRate; // Set time for next shot
                // Decrease ammo and update UI
                currentAmmo--;
            }
        }
        else
        {
            // Single fire - fires once per button press
            if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime && currentAmmo > 0)
            {
                SpawnBullet();
                StartCoroutine(FireAnimation());
                nextFireTime = Time.time + fireRate; // Set time for next shot
                // Decrease ammo and update UI
                currentAmmo--;
            }
        }
    }
    
    /// <summary>
    /// Coroutine to play the firing animation
    /// </summary>
    IEnumerator FireAnimation()
    {
        animator.SetBool("Fire", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Fire", false);
    }
    
    /// <summary>
    /// Creates and launches a bullet from the weapon
    /// </summary>
    void SpawnBullet()
    {
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;
        // Create the bullet
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
        // Apply velocity to the bullet
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = spawnRotation * Vector3.forward * bulletSpeed;
        }
        // Play muzzle flash effect
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
    
    /// <summary>
    /// Update the ammo count display in the UI
    /// </summary>
    void UpdateAmmoText()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }
    
    /// <summary>
    /// Reloads the weapon to maximum ammo capacity
    /// </summary>
    public void Reload()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }
    
    /// <summary>
    /// Adds additional ammo to the weapon, not exceeding max capacity
    /// </summary>
    /// <param name="amount">Amount of ammo to add</param>
    public void AddAdditionalAmmo(int amount)
    {
        // Add ammo but don't exceed maximum capacity
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        UpdateAmmoText();
    }
}