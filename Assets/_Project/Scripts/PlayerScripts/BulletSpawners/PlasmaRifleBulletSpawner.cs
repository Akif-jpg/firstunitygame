using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaRif : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public ParticleSystem muzzleEffect;
    private Animator animator;
    public AudioSource audioSource;       // Ses çalmak için AudioSource
    public float fireRate = 0.5f;         // Ateş etme hızı (0.5 saniye aralıklarla)
    private float nextFireTime = 0f;      // Bir sonraki ateş için zaman
    public bool isAutomatic = true;       // Otomatik ateş modunu açıp kapatabilirsiniz

    void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
    }

    void Update()
    {
        if (isAutomatic)
        {
            // Otomatik ateş - tuşa basılı tutulduğunda sürekli ateş eder
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                SpawnBullet();
                StartCoroutine(FireAnimation());
                nextFireTime = Time.time + fireRate; // Bir sonraki ateş zamanını ayarla
            }
        }
        else
        {
            // Tek tek ateş - tuş her basıldığında bir kez ateş eder
            if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
            {
                SpawnBullet();
                StartCoroutine(FireAnimation());
                nextFireTime = Time.time + fireRate; // Bir sonraki ateş zamanını ayarla
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
        
        // Merminin oluşturulması
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, spawnRotation);
        
        // Mermiye hız verilmesi
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = spawnRotation * Vector3.forward * bulletSpeed;
        }
        
        // Namlu efektinin oynatılması
        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }
        
        // Ses efektinin çalınması
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}