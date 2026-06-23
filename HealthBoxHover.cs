using UnityEngine;
using TMPro;

public class HealthBoxHover : MonoBehaviour
{
    public GameObject CanvasHealthBox;
    public TextMeshProUGUI infoText;

    void Start()
    {
        CanvasHealthBox.SetActive(false);
    }

    void OnMouseEnter()
    {
        CanvasHealthBox.SetActive(true);

        if(infoText != null)
            infoText.text = "Do you want to grab this stimpack?";
    }

    void OnMouseExit()
    {
        CanvasHealthBox.SetActive(false);
    }
}