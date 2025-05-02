using System.Collections;
using UnityEngine;

/**
* This class provides the rocket explosion system.
*/
public class RocketExplosion : MonoBehaviour
{
    [SerializeField] private GameObject particleGameObject;
    [SerializeField] private GameObject damageArea;
    [SerializeField] private float destroyingTime = 2f;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Rocket triggered by collision with: " + other.gameObject.name);
        if (other.gameObject.name != "GameController" || other.tag != "Missile" || other.tag != "WorldDestroyerEnemy")
        {
            // Stop movement by disabling physics
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;  // Make the object ignore physics forces
                rb.velocity = Vector3.zero;  // Clear any existing velocity
            }

            // Disable any scripts that might be moving the object
            var movementScript = GetComponent<MonoBehaviour>();  // Replace with your actual movement script type
            if (movementScript != null)
            {
                movementScript.enabled = false;
            }

            // The rest of the existing code
            if (damageArea != null)
            {
                damageArea.SetActive(true);
            }


            particleGameObject.SetActive(true);
            Destroy(particleGameObject, 2);
            particleGameObject.transform.parent = null;

            Destroy(gameObject, destroyingTime);
        }
    }
}