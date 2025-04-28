using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlasmaRif : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    [Header("Effects")]
    public ParticleSystem muzzleEffect;
    public AudioSource audioSource;       // AudioSource for playing sounds

    [Header("Weapon Properties")]
    public float fireRate = 0.5f;         // Fire rate (shots every 0.5 seconds)
    public bool isAutomatic = true;       // Toggle automatic fire mode

    [Header("Ammo System")]
    public int maxAmmo = 30;              // Maximum ammo capacity
    public int currentAmmo;               // Current ammo count
    public TextMeshProUGUI ammoText;      // Reference to UI text for displaying ammo

    private Animator animator;
    private float nextFireTime = 0f;      // Time for next fire

    void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
        // Initialize ammo to max capacity
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }

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

    IEnumerator FireAnimation()
    {
        animator.SetBool("Fire", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Fire", false);
    }

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
        currentAmmo = maxAmmo;
        UpdateAmmoText();
    }
}