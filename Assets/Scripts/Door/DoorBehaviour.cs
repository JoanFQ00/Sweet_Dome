using UnityEngine;
using System.Collections;

public class DoorBehaviour : MonoBehaviour, IResetable
{
    [Header("Referencias de Animación")]
    public Animation m_Animation;
    public AnimationClip Open;
    public AnimationClip Opened;
    public AnimationClip Close;
    public AnimationClip Closed;

    public enum Tstate { OPEN, OPENED, CLOSE, CLOSED }
    private Tstate m_State;

    private bool m_IsOpened;

    // Candado de seguridad para que tras cerrarse a las 9 o en la silla, no se vuelva a abrir sola hasta las 15
    private bool hasOpenedAt15Today = false;

    void Start()
    {
        if (m_Animation == null)
        {
            m_Animation = GetComponent<Animation>();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        // REGLA DE INICIO: La puerta inicialmente SIEMPRE está cerrada al comenzar la mañana
        SetClosedState();
    }

    private void OnEnable()
    {
        // Nos vinculamos directamente al evento de la silla de la consola
        MiniGameTest.OnMachineEntered += HandleChairInteraction;
    }

    private void OnDisable()
    {
        MiniGameTest.OnMachineEntered -= HandleChairInteraction;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    void Update()
    {
        if (DayTime.Instance == null) return;

        int currentHour = DayTime.Instance.GetCurrentHour();

        // REGLA 1: La puerta se abre automáticamente a las 15:00 cada día
        if (!m_IsOpened && currentHour >= 15 && currentHour < 21 && !hasOpenedAt15Today)
        {
            OpenDoor();
            hasOpenedAt15Today = true; // Bloqueamos el re-encendido automático
        }

        // REGLA 2: Si por algún motivo llega a las 21:00 y estuviera cerrada, asegura la apertura de salida
        if (!m_IsOpened && currentHour >= 21)
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Escucha el frame exacto en el que el jugador interactúa con la silla de control.
    /// </summary>
    private void HandleChairInteraction(bool isSeated)
    {
        // Si el jugador se SIENTA en la silla, la puerta se cierra fulminantemente si estaba abierta
        if (isSeated)
        {
            if (m_IsOpened)
            {
                CloseDoor();
            }
        }
        // Si el jugador se LEVANTA de la silla (Fin del día, hora >= 21)
        else
        {
            if (!m_IsOpened && DayTime.Instance.GetCurrentHour() >= 21)
            {
                OpenDoor();
            }
        }
    }

    public void OpenDoor()
    {
        if (m_State == Tstate.CLOSED)
        {
            m_State = Tstate.OPEN;
            m_IsOpened = true;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.doorOpen, gameObject);

            if (m_Animation != null && Open != null)
            {
                m_Animation.Play(Open.name);
                StartCoroutine(SetStateAfterAnimation(Open.length, Tstate.OPENED));
            }
            else
            {
                SetOpenedState();
            }
        }
    }

    public void CloseDoor()
    {
        if (m_State == Tstate.OPENED || m_State == Tstate.OPEN)
        {
            m_State = Tstate.CLOSE;
            m_IsOpened = false;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.doorClose, gameObject);

            if (m_Animation != null && Close != null)
            {
                m_Animation.Play(Close.name);
                StartCoroutine(SetStateAfterAnimation(Close.length, Tstate.CLOSED));
            }
            else
            {
                SetClosedState();
            }
        }
    }

    void SetOpenedState()
    {
        m_State = Tstate.OPENED;
        m_IsOpened = true;
        if (m_Animation != null && Opened != null) m_Animation.Play(Opened.name);
    }

    void SetClosedState()
    {
        m_State = Tstate.CLOSED;
        m_IsOpened = false;
        if (m_Animation != null && Closed != null) m_Animation.Play(Closed.name);
    }

    IEnumerator SetStateAfterAnimation(float animationTime, Tstate newState)
    {
        yield return new WaitForSeconds(animationTime);
        m_State = newState;
        m_IsOpened = (newState == Tstate.OPENED);
    }

    // =================================================================
    // INTERFAZ IRESETABLE: El reseteo diario perfecto de la mañana
    // =================================================================
    public void ResetValues()
    {
        StopAllCoroutines();
        hasOpenedAt15Today = false; // Permitimos la apertura del nuevo día a las 15
        SetClosedState(); // Forzamos que empiece el día cerrada estéticamente
    }
}