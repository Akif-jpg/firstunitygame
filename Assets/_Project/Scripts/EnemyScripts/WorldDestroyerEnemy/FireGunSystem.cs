using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGunSystem : MonoBehaviour
{
    [Header("For Starting State")]
    [SerializeField] private AudioSource chargingSound; // AudioSource for the charging sound
    [SerializeField] private GameObject animationsGameObject; // GameObject containing charging animations

    [Header("Actual Weapons Will Be Activate")]
    [SerializeField] private GameObject fireGuns; // GameObject containing the actual gun models/systems to activate

    // Reference to the currently running coroutine for the firing sequence
    private Coroutine _fireSequenceCoroutine;

    // --- Unity Lifecycle Methods ---

    // Start is called before the first frame update
    void Start()
    {
        // Ensure objects are initially inactive if needed
        if (animationsGameObject != null)
        {
            animationsGameObject.SetActive(false);
        }
        if (fireGuns != null)
        {
            fireGuns.SetActive(false);
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        // Usually used for continuous checks, input handling, etc.
        // Not strictly needed for this specific sequence logic triggered by functions.
    }

    // --- Public Control Methods ---

    /// <summary>
    /// Starts the sequence to activate the fire guns.
    /// Plays a charging sound segment, shows animations, waits, then activates guns.
    /// </summary>
    public void StartFire()
    {
        // Stop any previous sequence if it's still running
        if (_fireSequenceCoroutine != null)
        {
            StopCoroutine(_fireSequenceCoroutine);
             // Reset states immediately if starting again
            ResetStates();
        }
        // Start the new firing sequence
        _fireSequenceCoroutine = StartCoroutine(FireSequenceCoroutine());
    }

    /// <summary>
    /// Stops the firing sequence immediately.
    /// Stops sounds, hides animations, and deactivates guns.
    /// </summary>
    public void StopFire()
    {
        if (_fireSequenceCoroutine != null)
        {
            StopCoroutine(_fireSequenceCoroutine);
            _fireSequenceCoroutine = null; // Clear the reference
        }
        // Reset states when stopping
        ResetStates();
    }

    // --- Private Helper Methods ---

    /// <summary>
    /// Coroutine that handles the timed sequence of charging and activating the guns.
    /// </summary>
    /// <returns>IEnumerator for the coroutine</returns>
    private IEnumerator FireSequenceCoroutine()
    {
        // --- Phase 1: Charging ---

        // Activate charging animations
        if (animationsGameObject != null)
        {
            yield return new WaitForSeconds(1f);
            animationsGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Animations GameObject is not assigned.", this);
        }

        // Play charging sound segment (2s to 6s)
        if (chargingSound != null && chargingSound.clip != null)
        {
            // Ensure the requested time range is valid for the clip
            if (chargingSound.clip.length >= 6.0f)
            {
                chargingSound.time = 2.0f; // Start playing from the 2-second mark
                chargingSound.Play();
            }
            else
            {
                 Debug.LogWarning("Charging sound clip is shorter than 6 seconds. Playing from start.", this);
                 chargingSound.time = 0f;
                 chargingSound.Play();
                 // Adjust wait time if clip is shorter than desired segment end
                 float waitDuration = Mathf.Min(4.0f, chargingSound.clip.length - chargingSound.time);
                 yield return new WaitForSeconds(waitDuration); // Wait for the adjusted duration
                 chargingSound.Stop(); // Stop sound after adjusted duration
                 // Skip the rest of the specific sound handling if clip was too short
                 goto SkipSoundWait;
            }
        }
        else
        {
            Debug.LogWarning("Charging AudioSource or its AudioClip is not assigned.", this);
        }

        // Wait for 4 seconds (duration of the sound segment 2s to 6s)
        yield return new WaitForSeconds(1.0f);

        // Stop the charging sound specifically after 4 seconds
        if (chargingSound != null && chargingSound.isPlaying)
        {
            chargingSound.Stop();
        }

        SkipSoundWait: // Label to jump to if sound handling was skipped

        // Optionally deactivate animations after charging is complete
        // if (animationsGameObject != null)
        // {
        //     animationsGameObject.SetActive(false);
        // }

        // --- Phase 2: Activate Guns ---
        if (fireGuns != null)
        {
            fireGuns.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Fire Guns GameObject is not assigned.", this);
        }

        // Sequence finished, clear the coroutine reference
        _fireSequenceCoroutine = null;
    }

    /// <summary>
    /// Resets the state by stopping sounds and deactivating related GameObjects.
    /// </summary>
    private void ResetStates()
    {
         // Stop sound
        if (chargingSound != null && chargingSound.isPlaying)
        {
            chargingSound.Stop();
        }

        // Deactivate animations
        if (animationsGameObject != null)
        {
            animationsGameObject.SetActive(false);
        }

        // Deactivate guns
        if (fireGuns != null)
        {
            fireGuns.SetActive(false);
        }
    }
}