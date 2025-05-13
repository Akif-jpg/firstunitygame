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
    private Coroutine reloadCoroutine;
    private Coroutine fireAnimationCoroutine;
    
    private void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
    }
    
    private void Update()
    {
        UpdateAmmoDisplay();
        // Handle reload input
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
        {
            reloadCoroutine = StartCoroutine(Reload());
            return;
        }
        
        // Handle fire input
        if (Input.GetMouseButtonDown(0) && CanFire())
        {
            Fire();
        }
    }
    
    private void OnDisable()
    {
        // Stop all coroutines safely when object is disabled
        StopAllCoroutines();
        
        // Reset reload state
        if (isReloading)
        {
            isReloading = false;
            if (animator != null)
            {
                animator.SetBool("Reload", false);
            }
        }
        
        // Reset fire animation state
        if (animator != null)
        {
            animator.SetBool("Fire", false);
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
        if (fireAnimationCoroutine != null)
        {
            StopCoroutine(fireAnimationCoroutine);
        }
        fireAnimationCoroutine = StartCoroutine(PlayFireAnimation());
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
        if (animator != null)
        {
            animator.SetBool("Fire", true);
            yield return new WaitForSeconds(0.1f);
            animator.SetBool("Fire", false);
        }
        fireAnimationCoroutine = null;
    }
    
    private IEnumerator Reload()
    {
        isReloading = true;
       
        // Start reload animation
        if (animator != null)
        {
            animator.SetBool("Reload", true);
            yield return new WaitForSeconds(1f);
            animator.SetBool("Reload", false);
        }
       
        // Wait for reload to complete
        yield return new WaitForSeconds(reloadTime - 1f);
       
        // Restore ammo
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
       
        isReloading = false;
        reloadCoroutine = null;
    }
    
    private void UpdateAmmoDisplay()
    {
        if (ammoText != null)
        {
            ammoText.text =  this.currentAmmo + "/" +this.maxAmmo;
        }
    }
    
    // Public method to cancel reload (useful for other scripts)
    public void CancelReload()
    {
        if (isReloading && reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            isReloading = false;
            if (animator != null)
            {
                animator.SetBool("Reload", false);
            }
            reloadCoroutine = null;
        }
    }
}