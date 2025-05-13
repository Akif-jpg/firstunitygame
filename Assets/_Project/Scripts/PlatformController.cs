using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [Tooltip("This game controller needed for control platform movements.")]
    [SerializeField] private GameController gameController;

    [Header("Invisible boundaries settings.")]
    [Tooltip("Radius for the platform's invisible cylindrical boundaries")]
    [SerializeField] private float radius = 5f;

    [Tooltip("Height of the cylindrical boundaries")]
    [SerializeField] private float height = 2f;

    [Tooltip("Center offset for invisible boundaries.")]
    [SerializeField] private Vector3 centerOffset = new Vector3(0f, 5f, 0f);
    [SerializeField] private GameObject boundaryGameOject;

    [Header("UI Elements")]
    [SerializeField] private TMPro.TextMeshProUGUI pressSpaceText;

    private Vector3 platformGroundPosition;
    private Vector3 platformTopPosition;
    private Vector3 platformMarketplacePosition;
    private bool isPlayerInPlatform;
    private const float AWAIT_TAKE_TO_TAKEOFF = 15;
    private float awaitToTakeOff;
    private enum AnimationState
    {
        Landing,
        Takeoff,
        TakeOffToMarket,
        LandingFromMarket,
        AwaitLanding,
        AwaitTakeoff,
        AwaitInMarket,
        PausedLanding  // New state for when landing is paused because player is in the way
    }

    private AnimationState animationState;
    private float moveSpeed = 3.0f;
    private float marketMoveSpeed = 6.0f; // Market hareketleri için farklı bir hız tanımladım
    private bool isPlayerInLandingArea = false; // Flag to track if player is in landing area

    void Start()
    {
        animationState = AnimationState.AwaitLanding; // Başlangıçta beklemede olabilir
        platformTopPosition = new Vector3(0, 9.2f, 0);
        platformGroundPosition = new Vector3(0, 1.0f, 0);
        platformMarketplacePosition = new Vector3(0f, 40.83f, 0f);
        this.pressSpaceText.gameObject.SetActive(false);
        this.awaitToTakeOff = AWAIT_TAKE_TO_TAKEOFF;
    }

    void FixedUpdate()
    {
        switch (animationState)
        {
            case AnimationState.Landing:
                // Only move the platform down if no player is in the landing area
                if (!isPlayerInLandingArea)
                {
                    MovePlatform(platformGroundPosition, AnimationState.AwaitTakeoff);
                    this.awaitToTakeOff = AWAIT_TAKE_TO_TAKEOFF;
                }
                else
                {
                    // If player is detected in landing area, pause the landing
                    animationState = AnimationState.PausedLanding;
                }
                break;
            case AnimationState.PausedLanding:
                // Stay in this state until player leaves the landing area
                if (!isPlayerInLandingArea)
                {
                    // Resume landing when player leaves
                    animationState = AnimationState.Landing;
                }
                break;
            case AnimationState.Takeoff:
                MovePlatform(platformTopPosition, AnimationState.AwaitLanding);
                break;
            case AnimationState.TakeOffToMarket:
                // Market pozisyonuna hareket
                MovePlatformWithSpeed(platformMarketplacePosition, AnimationState.AwaitInMarket, marketMoveSpeed);
                EnableBoundaries(true); // Market sırasında sınırları etkinleştir
                break;
            case AnimationState.LandingFromMarket:
                // Marketten normal oyun alanına dönüş
                MovePlatformWithSpeed(platformTopPosition, AnimationState.AwaitLanding, marketMoveSpeed);
                break;
            case AnimationState.AwaitLanding:
                break;
            case AnimationState.AwaitTakeoff:
                EnableBoundaries(false);

                break;
            case AnimationState.AwaitInMarket:
                // Market alanında bekleme durumu
                break;
        }


        if (gameController.IsRestingState() || gameController.IsGameStart())
        {
            this.animationState = AnimationState.TakeOffToMarket;
            EnableBoundaries(true);
        }

        if (gameController.IsRoundOver())
        {
            // Only start landing if player is not in landing area
            if (!isPlayerInLandingArea)
            {
                this.animationState = AnimationState.Landing;
            }
            else
            {
                this.animationState = AnimationState.PausedLanding;
            }
            EnableBoundaries(false);
        }

        if (gameController.IsGameStarting() && this.animationState != AnimationState.AwaitTakeoff)
        {
            // Only start landing if player is not in landing area
            if (!isPlayerInLandingArea)
            {
                this.animationState = AnimationState.Landing;
            }
            else
            {
                this.animationState = AnimationState.PausedLanding;
            }
            EnableBoundaries(true);
        }

        if (gameController.IsMarketingState())
        {
            // Eğer şu anda markette değilsek markete hareket başlat
            if (this.animationState != AnimationState.TakeOffToMarket &&
                this.animationState != AnimationState.AwaitInMarket)
            {
                this.animationState = AnimationState.TakeOffToMarket;
            }
        }
        else
        {
            // Markette olduğumuzda market durumu biterse normal alana geri dön
            if (this.animationState == AnimationState.AwaitInMarket)
            {
                this.animationState = AnimationState.LandingFromMarket;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isPlayerInPlatform)
        {
            this.gameController.SendSignalHitNextWave();
            this.pressSpaceText.gameObject.SetActive(false);
        }
    }

    private void MovePlatform(Vector3 targetPosition, AnimationState nextState)
    {
        MovePlatformWithSpeed(targetPosition, nextState, moveSpeed);
    }

    private void MovePlatformWithSpeed(Vector3 targetPosition, AnimationState nextState, float speed)
    {
         transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
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
        // Only start landing if no player is in landing area
        if (!isPlayerInLandingArea)
        {
            animationState = AnimationState.Landing;
        }
        else
        {
            animationState = AnimationState.PausedLanding;
        }
    }

    // Market durumuna geçiş için yeni method
    public void GoToMarket()
    {
        animationState = AnimationState.TakeOffToMarket;
    }

    // Marketten çıkış için yeni method
    public void LeaveMarket()
    {
        animationState = AnimationState.LandingFromMarket;
    }

    void OnTriggerEnter(Collider other)
    {
        // If player entered to platform
        if (other.tag == "Player")
        {
            this.gameController.SendSignalPlayerEnterToPlatform();
            this.isPlayerInPlatform = true;
            this.pressSpaceText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            this.isPlayerInPlatform = false;
            this.pressSpaceText.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        // Set the color for the gizmo visualization
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent red

        // Draw wireframe for better visibility
        Gizmos.color = Color.red;
        DrawWireCylinder(transform.position + centerOffset, radius, height);
    }

    private void EnableBoundaries(bool enable)
    {
        this.boundaryGameOject.SetActive(enable);
    }

    // Custom method to draw a wire cylinder
    private void DrawWireCylinder(Vector3 center, float radius, float height)
    {
        // Number of segments to make the cylinder look smooth
        int segments = 20;
        float halfHeight = height / 2;

        // Draw top and bottom circles
        DrawWireCircle(center + Vector3.up * halfHeight, radius, segments);
        DrawWireCircle(center - Vector3.up * halfHeight, radius, segments);

        // Draw vertical lines connecting top and bottom
        for (int i = 0; i < segments; i++)
        {
            float angle = i * (2 * Mathf.PI / segments);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            Vector3 topPoint = center + Vector3.up * halfHeight + dir * radius;
            Vector3 bottomPoint = center - Vector3.up * halfHeight + dir * radius;

            Gizmos.DrawLine(topPoint, bottomPoint);
        }
    }

    // Custom method to draw a wire circle
    private void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * (2 * Mathf.PI / segments);
            float angle2 = (i + 1) * (2 * Mathf.PI / segments);

            Vector3 point1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 point2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;

            Gizmos.DrawLine(point1, point2);
        }
    }

    // Implement logic for when player enters landing area
    public void PlayerTriggeredLandingArea()
    {
        isPlayerInLandingArea = true;

        // If platform is currently landing, pause it
        if (animationState == AnimationState.Landing)
        {
            animationState = AnimationState.PausedLanding;
            Debug.Log("Triggered pause state");
        }
    }

    // Implement logic for when player exits landing area
    public void PlayerExitLandingArea()
    {
        isPlayerInLandingArea = false;

        // If platform was paused during landing, resume landing
        if (animationState == AnimationState.PausedLanding)
        {
            animationState = AnimationState.Landing;
        }
    }
}