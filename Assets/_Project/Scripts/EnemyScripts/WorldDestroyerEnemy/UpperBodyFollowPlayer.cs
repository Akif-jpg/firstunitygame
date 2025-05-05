using UnityEngine;
using System.Collections; // Required for IEnumerator

/**
* Ensures the upper body follows the player.
* If there is an obstacle (excluding the player) directly ahead at close range,
* it rotates clockwise to avoid the obstacle. Once the obstacle is cleared, it resumes following the player.
*/
public class UpperBodyFollowPlayer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Transform from which the ray will be cast")]
    [SerializeField] private Transform rayTransform;
    [Tooltip("Transform of the player to follow")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("Transform of the upper body that will rotate (if left empty, this object's transform will be used)")]
    [SerializeField] private Transform upperBodyTransform;

    [Header("Player Detection")]
    [Tooltip("LayerMask used to detect the player")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("Maximum distance to detect the player")]
    [SerializeField] private float maxPlayerDetectDistance = 20f;
    [Tooltip("How fast the upper body rotates towards the player")]
    [SerializeField] private float playerRotationSpeed = 5f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Ray distance for obstacle detection")]
    [SerializeField] private float obstacleCheckDistance = 5f;
    [Tooltip("Rotation speed for avoiding obstacles (degrees/second)")]
    [SerializeField] private float avoidanceRotationSpeed = 90f;
    [Tooltip("LayerMask to detect obstacles (everything except PlayerLayer)")]
    [SerializeField] private LayerMask obstacleLayerMask; // Will be set automatically

    private bool playerDetected = false;
    private bool isAvoidingObstacle = false;
    private Coroutine avoidanceCoroutine = null;

    private const float PLAYER_TARGET_OFFSET = -90f; // Angular offset applied when facing the player

    private void Awake()
    {
        // If upperBodyTransform is not assigned, use this object's transform
        if (upperBodyTransform == null)
        {
            upperBodyTransform = transform;
        }

        // Set the obstacle layer mask (everything except PlayerLayer)
        // If obstacleLayerMask is not manually set in the Inspector:
        if (obstacleLayerMask == 0) // 0 is the default value (Nothing)
        {
            obstacleLayerMask = ~playerLayer; // Select all layers except PlayerLayer
        }

        // Null checks at the start
        if (rayTransform == null)
        {
            Debug.LogError("Ray Transform is not assigned!", this);
            enabled = false; // Disable the script
        }
        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform is not assigned. Player detection will not work.", this);
            // enabled = false; // Can still work without a player, just show a warning.
        }
    }

    private void Update()
    {
        // If not currently rotating to avoid an obstacle
        if (!isAvoidingObstacle)
        {
            // 1. Check if there is an obstacle directly ahead
            if (CheckForObstacle())
            {
                // Obstacle found, enter avoidance mode and start the coroutine
                isAvoidingObstacle = true;
                // Stop any previous avoidance coroutine if running
                if (avoidanceCoroutine != null)
                {
                    StopCoroutine(avoidanceCoroutine);
                }
                avoidanceCoroutine = StartCoroutine(AvoidObstacleRoutine());
            }
            else
            {
                // No obstacle, detect the player and rotate towards them
                DetectPlayer();

                if (playerDetected && playerTransform != null)
                {
                    RotateTowardsPlayer();
                }
                // Optional: If the player is not detected or is missing, rotate to a default direction
                // else { /* RotateToDefault(); */ }
            }
        }
        // If isAvoidingObstacle is true, the coroutine will handle the rotation.
    }

    /// <summary>
    /// Checks if there is an obstacle directly ahead within the specified distance.
    /// The player is not considered an obstacle in this check.
    /// </summary>
    /// <returns>Returns true if an obstacle is detected, false otherwise.</returns>
    private bool CheckForObstacle()
    {
        if (rayTransform == null) return false;

        Ray ray = new Ray(rayTransform.position, rayTransform.forward);
        // Draw the ray for debugging (in the Scene view)
        Debug.DrawRay(ray.origin, ray.direction * obstacleCheckDistance, Color.blue);

        // Check if there is a collision with any layer except PlayerLayer
        return Physics.Raycast(ray, obstacleCheckDistance, obstacleLayerMask) && Vector3.Magnitude(rayTransform.position - playerTransform.position) >= maxPlayerDetectDistance;
    }

    /// <summary>
    /// Coroutine that manages clockwise rotation until the obstacle is cleared.
    /// </summary>
    private IEnumerator AvoidObstacleRoutine()
    {
        Debug.Log("Obstacle detected, starting avoidance.");
        // Continue rotating as long as there is an obstacle ahead
        while (CheckForObstacle())
        {

            // Clockwise rotation (positive rotation around the Y-axis)
            float rotationAmount = avoidanceRotationSpeed * Time.deltaTime;
            upperBodyTransform.Rotate(0f, rotationAmount, 0f, Space.World); // Rotate in world coordinates

            // Wait until the next frame

            yield return null;
        }

        Debug.Log("Obstacle cleared, ending avoidance.");
        isAvoidingObstacle = false; // Exit avoidance mode
        avoidanceCoroutine = null; // Clear the coroutine reference

    }

    /// <summary>
    /// Attempts to detect the player using both a forward ray and a direct ray to the player.
    /// </summary>
    private void DetectPlayer()
    {
        if (rayTransform == null || playerTransform == null)
        {
            playerDetected = false;
            return;
        }

        bool foundByForwardRay = false;
        bool foundByDirectRay = false;

        // 1. Forward ray (in the direction of rayTransform)
        Ray forwardRay = new Ray(rayTransform.position, rayTransform.forward);
        RaycastHit forwardHit;
        Debug.DrawRay(forwardRay.origin, forwardRay.direction * maxPlayerDetectDistance, Color.red);
        if (Physics.Raycast(forwardRay, out forwardHit, maxPlayerDetectDistance, playerLayer))
        {
            if (forwardHit.transform == playerTransform || forwardHit.transform.IsChildOf(playerTransform))
            {
                foundByForwardRay = true;
            }
        }

        // 2. Direct ray to the player (line of sight check)
        Vector3 directionToPlayer = playerTransform.position - rayTransform.position;
        float distanceToPlayer = directionToPlayer.magnitude; // Actual distance

        // Cast the second ray only if within maximum distance
        if (distanceToPlayer <= maxPlayerDetectDistance)
        {
            Ray directRay = new Ray(rayTransform.position, directionToPlayer.normalized);
            RaycastHit directHit;
            Debug.DrawRay(directRay.origin, directRay.direction * distanceToPlayer, Color.green); // Green ray
            // Here we use playerLayer as the LayerMask, but obstacle checks can also be added.
            // For now, just check if it directly hits the player:
            if (Physics.Raycast(directRay, out directHit, distanceToPlayer, playerLayer)) // Limit distance to distanceToPlayer
            {
                if (directHit.transform == playerTransform || directHit.transform.IsChildOf(playerTransform))
                {
                    // To check if there is another obstacle in between:
                    // RaycastHit blockingHit;
                    // if (!Physics.Raycast(directRay, out blockingHit, distanceToPlayer, ~playerLayer)) // Layers other than Player
                    // {
                    //      foundByDirectRay = true; // Direct line of sight to the player
                    // }
                    foundByDirectRay = true; // For now, consider it sufficient if it directly hits
                }
            }
        }

        // If the player is found by either method
        playerDetected = foundByForwardRay || foundByDirectRay;

        // For debugging
        // if (playerDetected) Debug.Log("Player Detected!");
        // else Debug.Log("Player NOT Detected.");
    }

    /// <summary>
    /// Rotates the upper body towards the detected player along the Y-axis.
    /// Applies the desired -90 degree offset.
    /// </summary>
    private void RotateTowardsPlayer()
    {
        if (upperBodyTransform == null || playerTransform == null) return;

        // Calculate the direction vector to the player (ignoring the Y-axis for now)
        Vector3 direction = playerTransform.position - upperBodyTransform.position;
        direction.y = 0; // Remove the Y-axis difference to rotate only on the horizontal plane

        // Rotate if there is a valid direction
        if (direction != Vector3.zero)
        {
            // Calculate the target angle (Atan2 takes x and z components) and convert to degrees
            float targetAngleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Apply the desired offset
            targetAngleY += PLAYER_TARGET_OFFSET;

            // Preserve the current X and Z rotations
            float currentAngleX = upperBodyTransform.eulerAngles.x;
            float currentAngleZ = upperBodyTransform.eulerAngles.z;

            // Smoothly transition to the target Y angle (LerpAngle solves angular wrapping issues)
            float currentAngleY = upperBodyTransform.eulerAngles.y;
            float newAngleY = Mathf.LerpAngle(currentAngleY, targetAngleY, playerRotationSpeed * Time.deltaTime);

            // Apply the new rotation, preserving X and Z
            upperBodyTransform.eulerAngles = new Vector3(currentAngleX, newAngleY, currentAngleZ);
        }
    }

    // Optional: Visualize detection ranges in the editor
    private void OnDrawGizmosSelected()
    {
        if (rayTransform != null)
        {
            // Player detection ray (red)
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayTransform.position, rayTransform.forward * maxPlayerDetectDistance);

            // Obstacle detection ray (blue)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rayTransform.position, rayTransform.forward * obstacleCheckDistance);

            // Ray starting point
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rayTransform.position, 0.1f);
        }
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}