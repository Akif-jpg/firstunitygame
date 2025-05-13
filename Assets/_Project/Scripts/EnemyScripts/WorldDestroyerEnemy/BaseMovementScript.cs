using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq; // Using Linq library (for finding closest and filtering)

// Enum for Map Trigger IDs - Assuming this exists elsewhere.
// IMPORTANT: Add a 'None' value to your MapTriggerId enum if it doesn't exist.
/* Example:
public enum MapTriggerId
{
    None = 0, // Represents no specific trigger or initial state
    holeWPart2,
    holeSPart2,
    holeNPart2,
    holeEPart2,
    holeWPart1,
    holeEPart1,
    holeNPart1,
    holeSPart1,
    conrnerES,
    cornerNE,
    cornerWN,
    cornerWS
    // Add other IDs as needed
}
*/

[RequireComponent(typeof(NavMeshAgent))]
public class BaseMovementScript : MonoBehaviour
{
    // Movement states enum to control agent behavior
    private enum MovementState
    {
        Idle,           // Agent is waiting or has no target (e.g., initial state, player not found)
        FollowingPlayer,// Agent is directly following the player (only when player hasn't triggered any zone yet)
        MovingToTarget, // Agent is moving toward a specific map trigger target
        ReachedTarget,  // Agent has reached its map trigger target and is waiting/wandering locally
        PathFailed      // Agent couldn't find a path to the target
    }

    [SerializeField] private DestroyerEnemyMapController destroyerEnemyMapController;
    [SerializeField] private float stoppingDistance = 1.0f; // How close to get to the target before stopping (for NavMeshAgent)
    [SerializeField] private float waitTimeAtTarget = 3.0f; // Time to wait/wander at target before potentially choosing another random spot nearby
    [SerializeField] private Transform playerTransform; // Reference to the player's transform for initial following

    private Dictionary<MapTriggerId, MapTrigger> mapList;
    private NavMeshAgent agent;
    private MovementState currentState = MovementState.Idle;
    private float waitTimer = 0f; // Timer used in ReachedTarget state for wandering logic

    // Transition graph defining allowed movements between MapTrigger points
    private Dictionary<MapTriggerId, MapTriggerId[]> transitions = new Dictionary<MapTriggerId, MapTriggerId[]>
    {
        // "_2" points
        { MapTriggerId.holeWPart2, new []{ MapTriggerId.holeNPart2, MapTriggerId.holeSPart2 } },
        { MapTriggerId.holeSPart2, new []{ MapTriggerId.holeEPart2, MapTriggerId.holeWPart2 } },
        { MapTriggerId.holeNPart2, new []{ MapTriggerId.holeEPart2, MapTriggerId.holeWPart2 } },
        { MapTriggerId.holeEPart2, new []{ MapTriggerId.holeNPart2, MapTriggerId.holeSPart2 } },

        // "_1" points
        { MapTriggerId.holeWPart1, new []{ MapTriggerId.cornerWN, MapTriggerId.cornerWS } },
        { MapTriggerId.holeEPart1, new []{ MapTriggerId.conrnerES, MapTriggerId.cornerNE } },
        { MapTriggerId.holeNPart1, new []{ MapTriggerId.cornerNE, MapTriggerId.cornerWN } },
        { MapTriggerId.holeSPart1, new []{ MapTriggerId.conrnerES, MapTriggerId.cornerWS } },

        // corner points
        { MapTriggerId.conrnerES,  new []{ MapTriggerId.holeEPart1, MapTriggerId.holeSPart1 } },
        { MapTriggerId.cornerNE,  new []{ MapTriggerId.holeEPart1, MapTriggerId.holeNPart1 } },
        { MapTriggerId.cornerWN,  new []{ MapTriggerId.holeNPart1, MapTriggerId.holeWPart1 } },
        { MapTriggerId.cornerWS,  new []{ MapTriggerId.holeWPart1, MapTriggerId.holeSPart1 } },
    };

    // State tracking
    private MapTriggerId currentDestinationTriggerId = MapTriggerId.None; // The trigger ID we are CURRENTLY GOING TO
    private MapTriggerId playerLastTriggeredId = MapTriggerId.None;      // Player's latest triggered ID (starts as None)
    private MapTriggerId lastReachedTriggerId = MapTriggerId.None;       // The trigger ID we LAST REACHED (to prevent repetition)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // --- Map Data Initialization ---
        mapList = destroyerEnemyMapController.GetMapTriggersList();
        if (mapList == null || mapList.Count == 0)
        {
            Debug.LogError("MapList could not be retrieved from DestroyerEnemyMapController or is empty!", this);
            enabled = false; // Disable the script
            return;
        }

        // --- NavMeshAgent Settings ---
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true; // Enable auto braking to stop precisely at stoppingDistance

        // --- Initial State ---
        currentState = MovementState.Idle;
        playerLastTriggeredId = MapTriggerId.None; // Explicitly start with no known player trigger
        lastReachedTriggerId = MapTriggerId.None;
        currentDestinationTriggerId = MapTriggerId.None;
    }

    void Update()
    {
        // --- Check for Player Trigger Changes ---
        MapTriggerId currentPlayerTrigger = destroyerEnemyMapController.GetPlayerLocatedTriggerId();

        // Condition to react:
        // 1. Player has *never* been detected in a trigger zone before (playerLastTriggeredId == MapTriggerId.None)
        // 2. OR Player has moved into a *different* trigger zone than the last known one.
        bool playerTriggerChanged = (playerLastTriggeredId == MapTriggerId.None && currentPlayerTrigger != MapTriggerId.None) ||
                                    (currentPlayerTrigger != MapTriggerId.None && currentPlayerTrigger != playerLastTriggeredId);

        if (playerTriggerChanged)
        {
            Debug.Log($"Player moved to trigger: {currentPlayerTrigger}. Previous: {playerLastTriggeredId}. Choosing new destination.");
            playerLastTriggeredId = currentPlayerTrigger;
            // When the player moves, always try to pick a new destination based on their *current* trigger zone.
            ChooseNewDestination(playerLastTriggeredId);
            // ChooseNewDestination will set the state to MovingToTarget if successful
        }
        // --- Initial Player Following Logic (Only if player hasn't triggered any zone yet) ---
        else if (currentState == MovementState.Idle && playerLastTriggeredId == MapTriggerId.None && playerTransform != null)
        {
            // If we are Idle, haven't detected the player in a zone yet, and we know the player's transform, follow the player directly.
            Debug.Log("Player not detected in a trigger zone yet. Following player directly.");
            if (agent.SetDestination(playerTransform.position))
            {
                currentState = MovementState.FollowingPlayer; // Change state to specifically track player following
                currentDestinationTriggerId = MapTriggerId.None; // Not heading to a specific trigger point
            }
            else
            {
                Debug.LogWarning("Could not set path to player's initial position. Remaining Idle.", this);
                // Path might be invalid, stay Idle.
            }
        }


        // --- State Machine Execution ---
        switch (currentState)
        {
            case MovementState.Idle:
                // Waiting for initial player detection or for player to move to a new trigger zone.
                // If playerTransform is null, or SetDestination failed above, we stay here.
                // No active movement needed in this state unless the conditions above are met.
                break;

            case MovementState.FollowingPlayer:
                // Continuously update destination to player's position if following directly
                if (playerTransform != null)
                {
                    agent.SetDestination(playerTransform.position); // Keep updating path to player
                    // Check if reached player (within stopping distance) - can add interaction logic here if needed
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                       // Optional: Add behavior when reaching the player while following directly
                       // Debug.Log("Reached player while following directly.");
                    }
                }
                else
                {
                    // Lost player transform somehow, revert to Idle
                    currentState = MovementState.Idle;
                }
                // Note: This state will be interrupted if the player enters a trigger zone (handled by the check at the start of Update).
                break;

            case MovementState.MovingToTarget:
                // Behavior is handled within the MoveToTarget method.
                MoveToTarget();
                break;

            case MovementState.ReachedTarget:
                // Player hasn't moved to a new trigger yet. Wait and wander near the last reached target.
                // Use the GoAround function to implement local waiting/wandering.
                if (mapList.TryGetValue(lastReachedTriggerId, out MapTrigger reachedTrigger)) // Use lastReachedTriggerId as center
                {
                    GoAround(reachedTrigger.transform, stoppingDistance, waitTimeAtTarget);
                }
                else
                {
                    Debug.LogError($"Reached target {lastReachedTriggerId} but cannot find its transform! Going Idle.", this);
                    currentState = MovementState.Idle; // Fallback if data is inconsistent
                }
                break;

            case MovementState.PathFailed:
                // Attempt to recalculate the path to the same destination immediately.
                Debug.LogWarning($"Path calculation failed trying to reach {currentDestinationTriggerId}. Retrying path calculation.");
                if (mapList.TryGetValue(currentDestinationTriggerId, out MapTrigger failedTargetTrigger))
                {
                    if (agent.SetDestination(failedTargetTrigger.transform.position))
                    {
                        // Successfully set a new path, try moving again
                        currentState = MovementState.MovingToTarget;
                        Debug.Log("Retry path set successfully. Moving to target.");
                    }
                    else
                    {
                        // Setting the destination failed *again*. The point might be truly unreachable from current pos.
                        Debug.LogError($"Cannot set destination to {currentDestinationTriggerId} ({failedTargetTrigger.transform.position}) even on retry. Going Idle.", this);
                        currentState = MovementState.Idle; // Give up on this target for now, wait for player movement.
                        currentDestinationTriggerId = MapTriggerId.None; // Clear the failed destination
                    }
                }
                else
                {
                    // This shouldn't happen if currentDestinationTriggerId was valid before failing.
                    Debug.LogError($"Target Trigger ID {currentDestinationTriggerId} not found in mapList during path retry. Going Idle.", this);
                    currentState = MovementState.Idle;
                    currentDestinationTriggerId = MapTriggerId.None; // Clear the invalid destination
                }
                break;
        }
    }

    /// <summary>
    /// Chooses a new destination trigger based on the player's current trigger ID
    /// and the predefined transitions. Avoids immediately returning to the last reached trigger.
    /// Sets the agent's destination and changes state to MovingToTarget if successful.
    /// </summary>
    /// <param name="playerTriggerId">The MapTriggerId where the player is currently located.</param>
    public void ChooseNewDestination(MapTriggerId playerTriggerId)
    {
        if (playerTriggerId == MapTriggerId.None)
        {
            Debug.LogWarning("ChooseNewDestination called with None trigger ID. Cannot determine transitions. Going Idle.", this);
            currentState = MovementState.Idle;
            return;
        }

        // Check if the player's current location has defined transitions
        if (!transitions.TryGetValue(playerTriggerId, out MapTriggerId[] possibleNextIds))
        {
            Debug.LogWarning($"No transitions defined for player's current trigger: {playerTriggerId}. Agent will remain in current state or Idle.", this);
            // Optionally, could make the agent Idle or choose a random point from *all* points.
            // For now, let's make it Idle if it wasn't already doing something important.
            if(currentState != MovementState.ReachedTarget) // Don't interrupt waiting state unless necessary
                currentState = MovementState.Idle;
            return;
        }

        if (possibleNextIds.Length == 0)
        {
             Debug.LogWarning($"Transitions defined for {playerTriggerId}, but the list is empty. Agent will remain in current state or Idle.", this);
             if(currentState != MovementState.ReachedTarget)
                currentState = MovementState.Idle;
             return;
        }

        // --- Select the best next destination ---
        List<MapTriggerId> validDestinations = possibleNextIds.ToList();

        // Try to avoid going back immediately to the trigger we just arrived from.
        // Only do this if there's more than one option.
        if (validDestinations.Count > 1 && lastReachedTriggerId != MapTriggerId.None && validDestinations.Contains(lastReachedTriggerId))
        {
            validDestinations.Remove(lastReachedTriggerId);
            // Debug.Log($"Removed {lastReachedTriggerId} from possible destinations.");
        }

        // Choose a random destination from the filtered list
        MapTriggerId nextDestinationId = validDestinations[Random.Range(0, validDestinations.Count)];

        // --- Set the chosen destination ---
        if (mapList.TryGetValue(nextDestinationId, out MapTrigger destinationTrigger))
        {
            if (agent.SetDestination(destinationTrigger.transform.position))
            {
                currentDestinationTriggerId = nextDestinationId; // Store the ID we are heading towards
                currentState = MovementState.MovingToTarget;     // Update state
                waitTimer = 0f; // Reset wait timer when starting new movement
                // Debug.Log($"New destination set: {currentDestinationTriggerId} at {destinationTrigger.transform.position}");
            }
            else
            {
                // Failed to set the path (e.g., target off NavMesh)
                Debug.LogWarning($"Failed to set path to destination {nextDestinationId} at {destinationTrigger.transform.position}. Triggering PathFailed state.", this);
                currentDestinationTriggerId = nextDestinationId; // Still store the intended target for the PathFailed state
                currentState = MovementState.PathFailed;
            }
        }
        else
        {
            // This indicates an issue with the mapList or transitions dictionary consistency
            Debug.LogError($"Selected destination ID {nextDestinationId} not found in mapList! Check configuration. Going Idle.", this);
            currentState = MovementState.Idle;
        }
    }

    /// <summary>
    /// Checks if the agent has reached its current target destination.
    /// Updates the state to ReachedTarget or PathFailed based on NavMeshAgent status.
    /// </summary>
    public void MoveToTarget()
    {
        // Wait if path is still being calculated
        if (agent.pathPending)
        {
            return;
        }

        // Check if path calculation failed
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.LogWarning($"Path to {currentDestinationTriggerId} is invalid or partial. Setting state to PathFailed.");
            currentState = MovementState.PathFailed;
            return;
        }

        // Check if the agent is close enough to the destination
        // Use remainingDistance which is more reliable than distance checks for NavMeshAgent
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // We have arrived at the target
            Debug.Log($"Reached target: {currentDestinationTriggerId}");
            lastReachedTriggerId = currentDestinationTriggerId; // Remember where we just arrived
            currentState = MovementState.ReachedTarget;       // Change state
            waitTimer = 0f; // Reset timer for waiting/wandering at the target
            // agent.ResetPath(); // Optional: Clear path immediately upon arrival
        }
    }

    /// <summary>
    /// Handles behavior when the agent has reached a target and is waiting.
    /// Waits for 'waitTime' seconds, then optionally picks a random point nearby to wander to.
    /// This local wandering continues until the player moves to a new trigger zone.
    /// </summary>
    /// <param name="centerPoint">The transform of the reached target.</param>
    /// <param name="wanderRadius">The radius around the center point for wandering (using stoppingDistance).</param>
    /// <param name="waitTime">The time to wait before picking a new wander point.</param>
    public void GoAround(Transform centerPoint, float wanderRadius, float waitTime)
    {
        // Ensure the agent isn't actively pathing somewhere far away unexpectedly
        if (agent.hasPath && agent.remainingDistance > wanderRadius * 1.5f) // If agent is somehow far from target, stop it.
        {
           // agent.ResetPath(); // Stop current path if it's leading too far
        }

        // Increment the timer
        waitTimer += Time.deltaTime;

        // Check if the wait time has elapsed OR if the agent finished its last small wander movement
        if (waitTimer >= waitTime && (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance))
        {
            waitTimer = 0f; // Reset timer

            // Find a random point within the wanderRadius around the center point on the NavMesh
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += centerPoint.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // Set a new destination nearby to simulate wandering
                // Check distance to prevent setting the same point repeatedly
                 if (Vector3.Distance(agent.destination, hit.position) > 0.2f)
                 {
                     agent.SetDestination(hit.position);
                     // Debug.Log($"Wandering near {lastReachedTriggerId} to {hit.position}");
                 }
            }
            // If SamplePosition fails, just wait for the next timer cycle
        }
         // If the timer hasn't elapsed, the agent just waits or continues its short wander path.
    }

    public void SetDestroyerMapController(DestroyerEnemyMapController enemyMapController)
    {
        this.destroyerEnemyMapController = enemyMapController;
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        this.playerTransform = playerTransform;
    }
}