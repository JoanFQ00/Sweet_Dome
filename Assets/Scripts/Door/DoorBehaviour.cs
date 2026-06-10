using UnityEngine;
using System.Collections;
using FMODUnity;

public class DoorBehaviour : MonoBehaviour
{
    public Animation m_Animation;
    public AnimationClip Open;
    public AnimationClip Opened;
    public AnimationClip Close;
    public AnimationClip Closed;

    public bool m_IsOpened;
    public bool m_CanOpen = true;

    public BoxCollider m_triggerCollision;

    public enum Tstate
    {
        OPEN,
        OPENED,
        CLOSE,
        CLOSED
    }

    private Tstate m_State;

    void Start()
    {
        m_CanOpen = true;

        if (m_Animation == null)
        {
            m_Animation = GetComponent<Animation>();
        }

        if (m_IsOpened)
        {
            SetOpenedState();
        }
        else
        {
            SetClosedState();
        }
    }

    void SetOpenedState()
    {
        m_State = Tstate.OPENED;
        m_IsOpened = true;

        if (m_Animation != null && Opened != null)
        {
            m_Animation.Play(Opened.name);
        }
    }

    void SetClosedState()
    {
        m_State = Tstate.CLOSED;
        m_IsOpened = false;

        if (m_Animation != null && Closed != null)
        {
            m_Animation.Play(Closed.name);
        }
    }

    public void OpenDoor()
    { 
        if (m_State == Tstate.CLOSED)
        {
            m_State = Tstate.OPEN;
            m_IsOpened = true;

            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.doorOpen, gameObject);

            if (m_Animation != null && Open != null)
            {
                m_Animation.Play(Open.name);
                StartCoroutine(SetState(Open.length, Tstate.OPENED));
            }
            else
            {
                SetOpenedState();
            }
        }
    }

    public void CloseDoor()
    {
        if (m_State == Tstate.OPENED)
        {
            m_State = Tstate.CLOSE;
            m_IsOpened = false;

            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.doorClose, gameObject);

            if (m_Animation != null && Close != null)
            {
                m_Animation.Play(Close.name);
                StartCoroutine(SetState(Close.length, Tstate.CLOSED));
            }
            else
            {
                SetClosedState();
            }
        }
    }

    IEnumerator SetState(float animationTime, Tstate newState)
    {
        yield return new WaitForSeconds(animationTime);

        m_State = newState;
        m_IsOpened = (newState == Tstate.OPENED);
    }

    void Update()
    {
        if (m_CanOpen && !m_IsOpened && DayTime.Instance.currentHour >= 15 && DayTime.Instance.currentHour < 21)
        {
            OpenDoor();
            m_CanOpen = false;
            m_triggerCollision.enabled = true;
        }
        else if (!m_IsOpened && DayTime.Instance.currentHour >= 21)
        {
            OpenDoor();
            m_CanOpen = true;
        }

        if (m_IsOpened && DayTime.Instance.currentHour <= 9)
            CloseDoor();
    }
}