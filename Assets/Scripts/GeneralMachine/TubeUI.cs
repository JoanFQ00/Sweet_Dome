using UnityEngine;
using UnityEngine.UI;

public class TubeUI : MonoBehaviour
{
    public Image cityFill;
    public TubeController tubeController;

    public Funnel cityFunnel;

    void Update()
    {
    }

    void UpdateBar(Image image, Funnel funnel)
    {
        if (funnel == null || image == null) return;

        float pct = funnel.GetPercentage();
        image.fillAmount = pct;
    }
}
