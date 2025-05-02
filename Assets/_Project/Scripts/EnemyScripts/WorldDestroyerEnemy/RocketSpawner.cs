using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketSpawner : MonoBehaviour
{
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AudioSource launchSound;
    [SerializeField] private ParticleSystem launchEffect;

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }

    public void SpawnMissile()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform is not set!");
            return;
        }

        // Play the launch sound effect
        if (launchSound != null)
        {
            launchSound.Play();
        }
        else
        {
            Debug.LogWarning("Launch sound is not set!");
        }

        // Play the particle effect
        if (launchEffect != null)
        {
            launchEffect.Play();
        }
        else
        {
            Debug.LogWarning("Launch effect is not set!");
        }

        GameObject rocketInstance = Instantiate(rocketPrefab, transform.position, transform.rotation);
        RocketTargetFinder rocketTargetFinder = rocketInstance.GetComponentInChildren<RocketTargetFinder>();
        
        if (rocketTargetFinder != null)
        {
            rocketTargetFinder.SetTarget(this.playerTransform.position);
        }
        else
        {
            Debug.LogWarning("RocketTargetFinder not found on rocketPrefab.");
        }
    }

    IEnumerator MissileSpawner()
    {
        while(true)
        {
            yield return new WaitForSeconds(2f);
            SpawnMissile();
        }
    }
    
    void Start()
    {
        SpawnMissile();
        StartCoroutine(MissileSpawner());
    }
}