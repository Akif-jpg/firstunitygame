using System.Collections;
using UnityEngine;

public class DamageRoutine
{
    public string DamageId { get; private set; }
    public float DamagePerSecond { get; private set; }
    public float IntervalSeconds { get; private set; }

    private Coroutine runningCoroutine;

    public DamageRoutine(string damageId, float dps, float interval)
    {
        this.DamageId = damageId;
        this.DamagePerSecond = dps;
        this.IntervalSeconds = interval;
    }

    public void Start(MonoBehaviour context, System.Action<float> applyDamage)
    {
        if (runningCoroutine == null)
        {
            runningCoroutine = context.StartCoroutine(DamageCoroutine(applyDamage));
        }
    }

    public void Stop(MonoBehaviour context)
    {
        if (runningCoroutine != null)
        {
            context.StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }
    }

    private IEnumerator DamageCoroutine(System.Action<float> applyDamage)
    {
        while (true)
        {
            applyDamage(DamagePerSecond);
            yield return new WaitForSeconds(IntervalSeconds);
        }
    }
}
