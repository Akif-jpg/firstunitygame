using System.Collections;
using UnityEngine;
using TMPro;

public class BulletSpawner : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private AudioSource audioSource;

    [Header("Weapon Properties")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private float reloadTime = 3f;
    [SerializeField] private TextMeshProUGUI ammoText;

    // Private variables
    private Animator animator;
    private float nextFireTime;
    private int currentAmmo;
    private bool isReloading;

    private void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        // Handle reload input
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return;
        }

        // Handle fire input
        if (Input.GetMouseButtonDown(0) && CanFire())
        {
            Fire();
        }
    }

    private bool CanFire()
    {
        return !isReloading && Time.time >= nextFireTime && currentAmmo > 0;
    }

    private void Fire()
    {
        // Set next fire time based on fire rate
        nextFireTime = Time.time + fireRate;
        
        // Decrease ammo
        currentAmmo--;
        UpdateAmmoDisplay();
        
        // Spawn bullet
        SpawnBullet();
        
        // Play fire animation
        StartCoroutine(PlayFireAnimation());
    }

    private void SpawnBullet()
    {
        // Spawn bullet and set velocity
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.rotation * Vector3.forward * bulletSpeed;
        }

        // Play effects
        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }
        
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private IEnumerator PlayFireAnimation()
    {
        animator.SetBool("Fire", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("Fire", false);
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        
        // Start reload animation
        animator.SetBool("Reload", true);
        yield return new WaitForSeconds(1f);
        animator.SetBool("Reload", false);
        
        // Wait for reload to complete
        yield return new WaitForSeconds(reloadTime - 1f);
        
        // Restore ammo
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
        
        isReloading = false;
    }

    private void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }
}