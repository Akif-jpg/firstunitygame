using UnityEngine;

public class PlatformController : MonoBehaviour
{
    private Vector3 platformGroundPosition;
    private Vector3 platformTopPosition;

    private enum AnimationState
    {
        Landing,
        Takeoff,
        AwaitLanding,
        AwaitTakeoff
    }

    private AnimationState animationState;
    private float moveSpeed = 2.0f;

    void Start()
    {
        animationState = AnimationState.Landing; // Başlangıçta beklemede olabilir
        platformTopPosition = new Vector3(0, 9.2f, 0);
        platformGroundPosition = new Vector3(0, 1.0f, 0);
    }

    void FixedUpdate()
    {
        switch (animationState)
        {
            case AnimationState.Landing:
                MovePlatform(platformGroundPosition, AnimationState.AwaitTakeoff);
                break;
            case AnimationState.Takeoff:
                MovePlatform(platformTopPosition, AnimationState.AwaitLanding);
                break;
            case AnimationState.AwaitLanding:
            case AnimationState.AwaitTakeoff:
                // Bekleme durumları, hareket yok
                break;
        }
    }

    private void MovePlatform(Vector3 targetPosition, AnimationState nextState)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            animationState = nextState;
        }
    }

    public void Takeoff()
    {
        animationState = AnimationState.Takeoff;
    }

    public void Landing()
    {
        animationState = AnimationState.Landing;
    }
}
