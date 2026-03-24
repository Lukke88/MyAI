using UnityEngine;
using UnityEngine.UI;

public class TandemButtonController : MonoBehaviour
{
    public DesertReaperBehaviour desertReaper; // skript DesertReapera s TandemFollowBehavior
    public Button tandemButton;                 // tlačítko v UI

    private bool isTandemActive = false;

    void Start()
    {
        if (tandemButton != null)
        {
            tandemButton.onClick.AddListener(ToggleTandem);
        }
    }

    void ToggleTandem()
    {
        if (desertReaper != null)
        {
            isTandemActive = !isTandemActive;           // přepíná stav
            desertReaper.IsTandemScriptActivated = isTandemActive;

            // volitelně změna barvy tlačítka
            if (tandemButton.image != null)
                tandemButton.image.color = isTandemActive ? new Color(1f, 0.5f, 0f) : Color.black;
        }
    }
}