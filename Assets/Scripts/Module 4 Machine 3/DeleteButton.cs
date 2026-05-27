using UnityEngine;

public class DeleteButton : MonoBehaviour
{
    public MainButt mainButt;
    public CodeSystem codeSystem;

    private void Awake()
    {
        if (mainButt != null)
            mainButt.onPress.AddListener(DeleteNumber);
    }

    private void OnDestroy()
    {
        if (mainButt != null)
            mainButt.onPress.RemoveListener(DeleteNumber);
    }

    void DeleteNumber()
    {
        if (codeSystem != null)
            codeSystem.DeleteLastNumber();
    }
}