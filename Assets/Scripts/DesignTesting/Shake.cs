using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Shake : MonoBehaviour
{
    public AnimationCurve curve;
    private Coroutine currentShakeCoroutine;

    public static Shake Instance;

    // Guardamos la posición local inicial por si acaso
    private Vector3 originalLocalPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        // Al empezar, registramos cuál es la posición base de la cámara respecto al jugador
        originalLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerShake(1f);
        }
    }

    public void TriggerShake(float customDuration)
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            // IMPORTANTE: Si cortamos un shake a la mitad, reseteamos al punto base 
            // para que el siguiente shake no empiece desde una posición desfasada.
            transform.localPosition = originalLocalPosition;
        }

        currentShakeCoroutine = StartCoroutine(Shaking(customDuration));
    }

    private IEnumerator Shaking(float duration)
    {
        // El punto de partida siempre será nuestra posición local original
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float strength = curve.Evaluate(elapsedTime / duration);

            // Aplicamos el desfase sobre la posición local original
            transform.localPosition = originalLocalPosition + Random.insideUnitSphere * strength;

            yield return null;
        }

        // Al terminar, nos aseguramos de volver exactamente al origen local
        transform.localPosition = originalLocalPosition;
        currentShakeCoroutine = null;
    }
}