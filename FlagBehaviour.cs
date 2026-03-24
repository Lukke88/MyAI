using TMPro;
using UnityEngine;

public class FlagBehaviour : MonoBehaviour
{
    [Header("Destination Source")]
    public CursorForceEnable cursorSource;

    [Header("Visual")]
    public GameObject flagVisual; // model vlajky
    public TextMeshPro worldText;  // 3D TMP text nad vlajkou

    void Update()
    {
        if (cursorSource == null)
            return;

        Vector3 destination = cursorSource.finalDestinationForPlayer;

        if (destination != Vector3.zero)
        {
            // Přesun vlajky na místo
            transform.position = destination;

            // Aktivuj vizuál
            if (flagVisual != null)
                flagVisual.SetActive(true);

            // Aktualizuj text
            if (worldText != null)
            {
                worldText.text =
                    "x: " + destination.x.ToString("F1") +
                    "\ny: " + destination.y.ToString("F1") +
                    "\nz: " + destination.z.ToString("F1");
            }
        }
        else
        {
            if (flagVisual != null)
                flagVisual.SetActive(false);
        }
    }
}