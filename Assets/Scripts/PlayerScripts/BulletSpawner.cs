using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public Transform firePoint;
    public ParticleSystem muzzleEffect;

    public AudioSource audioSource;       // Ses çalmak için AudioSource

    void Update()
    {
        firePoint = this.transform;

        if (Input.GetMouseButtonDown(0))
        {
            SpawnBullet();
        }
    }

    void SpawnBullet()
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRotation = firePoint != null ? firePoint.rotation : transform.rotation;

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

        // 🔊 Ses efekti çal
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }


}
