using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RocketTargetFinder : MonoBehaviour
{
    private Vector3 destination;
    private NavMeshAgent agent;

    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float initialForwardTime = 0.5f; // ilk düz gidiş süresi (saniye)

    private bool launchPhase = true;
    private bool switchedToForwardMotion = false;
    private bool hasDestination = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false; // Başta kapalı, çünkü launch fazında kullanılmaz
    }

    public void SetTarget(Vector3 targetPosition)
    {
        destination = targetPosition;
        hasDestination = true;
        StartCoroutine(InitialLaunchThenSeek());
    }

    IEnumerator InitialLaunchThenSeek()
    {
        float timer = 0f;

        // Launch fazı: sabit ileri hareket
        while (timer < initialForwardTime)
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // NavMeshAgent aktifleşir ve hedefe yönelir
        launchPhase = false;
        agent.enabled = true;
        if (hasDestination)
        {
            agent.SetDestination(destination);
        }
    }

    void Update()
    {
        // Hedefe vardığında sürekli ileri gitmeye başla
        if (!launchPhase && hasDestination && !switchedToForwardMotion && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.enabled = false;
            switchedToForwardMotion = true;
        }

        if (switchedToForwardMotion)
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
        }
    }
}
