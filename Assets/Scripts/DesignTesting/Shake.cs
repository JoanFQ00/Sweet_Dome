using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Shake : MonoBehaviour
{
    public AnimationCurve curve;
    private Coroutine currentShakeCoroutine;

    public static Shake Instance;

    private Vector3 originalLocalPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        originalLocalPosition = transform.localPosition;
    }

    public void TriggerShake(float customDuration)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            transform.localPosition = originalLocalPosition;
        }

        currentShakeCoroutine = StartCoroutine(Shaking(customDuration));
    }

    private IEnumerator Shaking(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float strength = curve.Evaluate(elapsedTime / duration);

            transform.localPosition = originalLocalPosition + Random.insideUnitSphere * strength;

            yield return null;
        }

        transform.localPosition = originalLocalPosition;
        currentShakeCoroutine = null;
    }
}