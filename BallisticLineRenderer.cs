using UnityEngine;
using TMPro;

public class BallisticLineRenderer : MonoBehaviour
{
    public Transform turret;           // hlaveň věže
    public Transform cursor_square;    // cíl
    public LineRenderer lineRenderer;  // line renderer
    public TMP_Text statsText;         // TextMeshPro pro výpis

    public int ballisticResolution = 30;
    public float ballisticHeight = 8f; // výška oblouku
    public float jennyHeight = 1.8f;  // 1.8 m = reference

    void Update()
    {
        if (turret == null || cursor_square == null || lineRenderer == null || statsText == null)
            return;

        DrawBallisticLine();
        UpdateStats();
    }

    void DrawBallisticLine()
    {
        Vector3 start = turret.GetChild(0).position;  // hlaveň věže
        Vector3 end = cursor_square.position;         // cíl

        lineRenderer.positionCount = ballisticResolution + 1;
        lineRenderer.enabled = true;

        for (int i = 0; i <= ballisticResolution; i++)
        {
            float t = i / (float)ballisticResolution;
            Vector3 point = GetBallisticPoint(start, end, t);
            lineRenderer.SetPosition(i, point);
        }
    }

    Vector3 GetBallisticPoint(Vector3 start, Vector3 end, float t)
    {
        Vector3 mid = Vector3.Lerp(start, end, t);
        mid.y += Mathf.Sin(Mathf.PI * t) * ballisticHeight; // parabola
        return mid;
    }

    void UpdateStats()
    {
        Vector3 start = turret.GetChild(0).position;
        Vector3 end = cursor_square.position;

        // vzdálenost turret -> cursor
        float distanceMeters = Vector3.Distance(start, end) / jennyHeight;

        // délka balistické křivky
        float curveLength = 0f;
        Vector3 prev = start;
        for (int i = 1; i <= ballisticResolution; i++)
        {
            float t = i / (float)ballisticResolution;
            Vector3 point = GetBallisticPoint(start, end, t);
            curveLength += Vector3.Distance(prev, point);
            prev = point;
        }
        curveLength /= jennyHeight;

        // vzdálenost BTR -> camera (CMA)
        Vector3 btrPos = turret.parent.position; // assuming turret je child BTR
        Vector3 cameraPos = Camera.main.transform.position;
        float btrToCamera = Vector3.Distance(btrPos, cameraPos) / jennyHeight;

        // výpis do TMP_Text
        statsText.text = $"Vzdálenost turret -> cursor: {distanceMeters:F2} m\n" +
                         $"Délka balistické křivky: {curveLength:F2} m\n" +
                         $"Délka BTR -> CMA: {btrToCamera:F2} m";
    }
}
