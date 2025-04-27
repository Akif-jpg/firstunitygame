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

    public AudioSource audioSource;       // Ses çalmak için AudioSource

    public float time = 0.5f;
    public bool fire = false;

    void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
    }


    void Update()
    {
        // super new update
        if (time > 0 && fire)
        {
            time -= Time.deltaTime;
        }
        else
        {
            time = 0.5f;
            fire = false;
        }

        if (Input.GetMouseButtonDown(0) && !fire)
        {
            SpawnBullet();
            StartCoroutine(FireAnimation());
            fire = true;
        }



    }


    IEnumerator FireAnimation()
    {
        animator.SetBool("Fire", true);
        yield return new WaitForSeconds(0.1f); // i change the time second 0.5 to 0.1
        animator.SetBool("Fire", false);
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

        // 🔊 Ses efekti çal
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }


}
