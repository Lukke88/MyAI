using UnityEngine;
using TMPro;

public class GroundCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;              // Main Camera (hráčovo oko)
    [SerializeField] private RectTransform cursorRect;       // RectTransform UI kurzoru (cursor_screen)
    [SerializeField] private Transform groundMarker;         // Transform markeru (Plane/Quad na zemi)
    [SerializeField] private TMP_Text infoText;              // TMP_Text "infoCoordinates"

    [Header("Settings")]
    [SerializeField] private float markerOffset = 0.5f;      // Nadzemi offset
    [SerializeField] private LayerMask groundLayer = -1;     // Layer pro Ground (nebo -1 pro vše)
    [SerializeField] private float cursorSmoothSpeed = 10f;  // Rychlost smooth pohybu kurzoru
    [SerializeField] private bool lockCursor = true;         // Zamknout systémový kurzor?

    private Vector3 targetCursorPos;
    private bool isInitialized = false;

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (groundMarker != null) groundMarker.gameObject.SetActive(false);
        if (!isInitialized)
        {
            groundLayer = LayerMask.GetMask("Ground"); // Auto-nastavení layeru
            isInitialized = true;
        }
    }

    private void Update()
    {
        // 1. Pohyb kurzoru podle myši (smooth)
        targetCursorPos = Input.mousePosition;
        cursorRect.position = Vector3.Lerp(cursorRect.position, targetCursorPos, Time.deltaTime * cursorSmoothSpeed);

        // 2. Raycast z kamery přes kurzor
        Ray ray = mainCamera.ScreenPointToRay(cursorRect.position);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (hit.collider.CompareTag("Ground")) // Extra check tagu
            {
                // Pozice markeru
                Vector3 markerPos = hit.point + Vector3.up * markerOffset;
                groundMarker.position = markerPos;

                // Billboard: marker čelí kameře
                groundMarker.LookAt(groundMarker.position + mainCamera.transform.forward);

                groundMarker.gameObject.SetActive(true);

                // Vzdálenost od kamery
                float distance = Vector3.Distance(mainCamera.transform.position, hit.point);

                // Výpis – seřazené pod sebe, přesné na 2 des. místa
                infoText.text = $"x: {hit.point.x:F2}\n" +
                               $"y: {hit.point.y:F2}\n" +
                               $"z: {hit.point.z:F2}\n" +
                               $"Distance: {distance:F2}m";

                return; // Úspěch!
            }
        }

        // Žádný hit → schovat
        HideMarkerAndInfo();
    }

    private void HideMarkerAndInfo()
    {
        if (groundMarker != null) groundMarker.gameObject.SetActive(false);
        if (infoText != null) infoText.text = "No ground target";
    }

    private void OnDestroy()
    {
        // Uvolnit kurzor při zničení
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Bonus: Public method pro externí volání (např. z jiného skriptu)
    public RaycastHit? GetGroundHit()
    {
        Ray ray = mainCamera.ScreenPointToRay(cursorRect.position);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer) && hit.collider.CompareTag("Ground"))
            return hit;
        return null;
    }
}