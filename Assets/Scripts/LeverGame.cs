using UnityEngine;

public class LeverGame : MonoBehaviour
{
    bool isUp = false;
    public float movement = 0.2f;
    Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    public void Toggle()
    {
        isUp = !isUp;

        Debug.Log("Palanca:" + isUp);

        if (isUp)
        {
            transform.localPosition = startPos + new Vector3(0, movement, 0);
            Debug.Log("Palanca subida");
        }
        else
        {
            transform.localPosition = startPos;
            Debug.Log("Palanca bajada");
        }
    }
}
