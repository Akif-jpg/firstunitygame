using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VegasHealthSystem : MonoBehaviour
{
    // Reference to the EnemyHealth component/class
    private EnemyHealth enemyHealth;

    // Optional: Particle system for damage visual effect
    [SerializeField] private ParticleSystem damageParticle;
    // Optional: Particle system for destroy visual effect
    [SerializeField] private ParticleSystem destroyParticle;
    // Optional: AudioSource for destroy sound effect
    [SerializeField] private AudioSource destroyAudio;
    [SerializeField] private int health = 50;

    // Flag to prevent death sequence from running multiple times
    private bool isDeathSequenceStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize EnemyHealth with a starting health value (e.g., 150)
        this.enemyHealth = new EnemyHealth(this.health);

    }

    void Update()
    {
        // Check if the enemy is dead and the death sequence hasn't started
        if (enemyHealth != null && !enemyHealth.IsEnemyAlive() && !isDeathSequenceStarted)
        {
            isDeathSequenceStarted = true;
            StartCoroutine(DestroySequence());
        }
    }


    // Called when another Collider enters this GameObject's trigger Collider
    void OnTriggerEnter(Collider other)
    {
        // Check if the enemy is still alive before processing damage
        if (enemyHealth == null || !enemyHealth.IsEnemyAlive())
        {
            return; // Don't process damage if already dead or health component is missing
        }

        string tagName = other.tag;

        // Check if the colliding object is tagged as a player bullet
        if (tagName == DamageAreas.PLAYER_BULLET)
        {
            // Apply damage using the defined value
            this.enemyHealth.AddDamage(DamageAreas.PLAYER_BULLET_VALUE);
            // Play damage visual/audio effects (optional)
            StartCoroutine(DamageEffect());
            // Destroy the bullet object
            Destroy(other.gameObject);
        }        
        else if (tagName == DamageAreas.PLAYER_RIFFLE_BULLET)
        {
            StartCoroutine(DamageEffect());
            // Apply damage to the enemy
            this.enemyHealth.AddDamage(DamageAreas.PLAYER_RIFFLE_BULLET_VALUE);
            Destroy(other.gameObject);

        }
        // Check if the colliding object is tagged as a plasma bomb damage area
        else if (tagName == DamageAreas.PLASMA_BOMB_DAMAGE_AREA)
        {
            // Apply damage using the defined value
            this.enemyHealth.AddDamage(DamageAreas.PLASMA_BOMB_DAMAGE_AREA_VALUE);
            // Play damage visual/audio effects (optional)
            StartCoroutine(DamageEffect());
            // Note: Typically, you don't destroy the damage area collider itself,
            // unless it's a temporary effect object.
        }
        // Add more 'else if' blocks here for other types of damage sources
        // else if (tagName == "AnotherDamageType")
        // {
        //     this.enemyHealth.AddDamage(DamageValueForAnotherType);
        //     StartCoroutine(DamageEffect());
        //     // Handle the other object if needed (e.g., Destroy(other.gameObject))
        // }
    }

    // Coroutine for playing damage effects (visual/animation)
    IEnumerator DamageEffect()
    {
        // Play particle effect if assigned
        if (damageParticle != null)
        {
            damageParticle.Play();
        }

        // Wait for a short duration if needed, e.g., for particle effect to be visible
        yield return new WaitForSeconds(0.2f);

        // Stop particle effect if it was played and is looping (optional)
        // if (damageParticle != null && damageParticle.main.loop)
        // {
        //     damageParticle.Stop();
        // }
    }

    // Coroutine for handling the enemy's destruction
    IEnumerator DestroySequence()
    {
        Debug.Log("Starting Destroy Sequence for " + gameObject.name);

        // Stop enemy movement (if applicable, requires reference to movement script or NavMeshAgent)
        // Example: GetComponentInParent<NavMeshAgent>()?.Stop();
        // Example: GetComponentInParent<EnemyMovement>()?.DisableMovement();

        // Play destruction particle effect if assigned
        if (destroyParticle != null)
        {
            destroyParticle.Play();
        }

        // Play destruction audio if assigned
        if (destroyAudio != null)
        {
            destroyAudio.Play();
        }


        // Wait for the duration of the death animation/effects
        // Adjust this time based on your animation and particle effect length
        yield return new WaitForSeconds(1.5f);

        // Finally, destroy the main enemy GameObject
        // Use Destroy(transform.parent.gameObject) if this script is on a child object
        // representing the hitbox and the main logic/mesh is on the parent.
        // Or Destroy(this.gameObject) if this script is on the root enemy object.
        Destroy(gameObject); // Or Destroy(transform.parent.gameObject)
    }
}
