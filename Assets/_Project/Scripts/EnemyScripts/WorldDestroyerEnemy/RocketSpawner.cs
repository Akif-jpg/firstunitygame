using UnityEngine;

public class RocketSpawner : MonoBehaviour
{
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private AudioSource launchSound;
    [SerializeField] private ParticleSystem launchEffect;
    [SerializeField] private float spawnInterval = 8f; // Füze serisi başlama aralığı
    [SerializeField] private int rocketsPerBurst = 4; // Her seride kaç füze
    [SerializeField] private float burstInterval = 0.4f; // Füzeler arası süre

    private float globalTimer = 0f;
    private float burstTimer = 0f;
    private int burstRocketsFired = 0;
    private bool isBursting = false;

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

        if (launchSound != null)
            launchSound.Play();
        else
            Debug.LogWarning("Launch sound is not set!");

        if (launchEffect != null)
            launchEffect.Play();
        else
            Debug.LogWarning("Launch effect is not set!");

        GameObject rocketInstance = Instantiate(rocketPrefab, transform.position, transform.rotation);
        RocketTargetFinder rocketTargetFinder = rocketInstance.GetComponentInChildren<RocketTargetFinder>();

        if (rocketTargetFinder != null)
            rocketTargetFinder.SetTarget(this.playerTransform.position);
        else
            Debug.LogWarning("RocketTargetFinder not found on rocketPrefab.");
    }

    void Awake()
    {
        Debug.Log("Missile Launcher awaked");
        SpawnMissile(); // İlk füze
        globalTimer = 0f;
    }

    void Update()
    {
        if (!isBursting)
        {
            globalTimer += Time.deltaTime;
            if (globalTimer >= spawnInterval)
            {
                isBursting = true;
                burstTimer = 0f;
                burstRocketsFired = 0;
                globalTimer = 0f;
            }
        }
        else
        {
            burstTimer += Time.deltaTime;
            if (burstTimer >= burstInterval)
            {
                SpawnMissile();
                burstRocketsFired++;
                burstTimer = 0f;
            }

            if (burstRocketsFired >= rocketsPerBurst)
            {
                isBursting = false;
            }
        }
    }
}
