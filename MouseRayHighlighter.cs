using UnityEngine;

public class MouseRayHighlighter : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask pickupLayer;

    private SimpleHighlight currentHighlight;

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, pickupLayer))
        {
            SimpleHighlight highlight = hit.collider.GetComponent<SimpleHighlight>();

            if (highlight != null)
            {
                // Pokud míříme na jiný objekt než předtím
                if (currentHighlight != highlight)
                {
                    ClearHighlight();
                    currentHighlight = highlight;
                    currentHighlight.SetHighlight(true);
                }

                return; // důležité – jinak by se hned zrušil
            }
        }

        // Pokud jsme nic netrefili
        ClearHighlight();
    }

    void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);
            currentHighlight = null;
        }
    }
}
