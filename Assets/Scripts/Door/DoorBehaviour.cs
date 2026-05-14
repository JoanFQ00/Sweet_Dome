
using UnityEngine;
using System.Collections;

public class DoorBehaviour : MonoBehaviour
{
    public Animation m_Animation;
    public AnimationClip Open;
    public AnimationClip Opened;
    public AnimationClip Close;
    public AnimationClip Closed;

    public bool m_IsOpened;

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
        if (Input.GetKeyDown(KeyCode.P))
        {
            OpenDoor();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            CloseDoor();
        }
    }
}

