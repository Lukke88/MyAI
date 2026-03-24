using UnityEngine;
using System.Collections.Generic;

public class FormationManager : MonoBehaviour
{
    public RTSSelectionManager selectionManager;
    public float spacing = 2.0f;

    private enum FormationType
    {
        Line,
        TwoSideBySide,
        Triangle,
        Square,
        Pentagon_3_2,
        Hexagon
    }

    private FormationType currentFormation = FormationType.Line;

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftControl))
        {
            MoveInFormation();
        }
    }

    // ===============================
    // ======= UI BUTTONS ============
    // ===============================

    // BUTTON 1 – Select All + Line
    public void Formation_SelectAll_Line()
    {
        SelectAllUnits();
        currentFormation = FormationType.Line;
    }

    // BUTTON 2 – 2 vedle sebe
    public void Formation_Two()
    {
        currentFormation = FormationType.TwoSideBySide;
    }

    // BUTTON 3 – Triangle
    public void Formation_Triangle()
    {
        currentFormation = FormationType.Triangle;
    }

    // BUTTON 4 – Square
    public void Formation_Square()
    {
        currentFormation = FormationType.Square;
    }

    // BUTTON 5 – 3 vpředu, 2 vzadu
    public void Formation_Pentagon()
    {
        currentFormation = FormationType.Pentagon_3_2;
    }

    // BUTTON 6 – Hexagon
    public void Formation_Hexagon()
    {
        currentFormation = FormationType.Hexagon;
    }

    // ===============================

    void MoveInFormation()
    {
        List<UnitController> selected = selectionManager.GetSelectedUnits();
        if (selected.Count == 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        Vector3 center = hit.point;
        List<Vector3> positions = GenerateFormation(selected.Count, center);

        for (int i = 0; i < selected.Count && i < positions.Count; i++)
        {
            selected[i].MoveTo(positions[i]);
        }
    }

    List<Vector3> GenerateFormation(int count, Vector3 center)
    {
        List<Vector3> positions = new List<Vector3>();

        switch (currentFormation)
        {
            case FormationType.Line:
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - count / 2f) * spacing;
                    positions.Add(center + new Vector3(offset, 0, 0));
                }
                break;

            case FormationType.TwoSideBySide:
                if (count >= 2)
                {
                    positions.Add(center + new Vector3(-spacing / 2, 0, 0));
                    positions.Add(center + new Vector3(spacing / 2, 0, 0));
                }
                break;

            case FormationType.Triangle:
                if (count >= 3)
                {
                    positions.Add(center + new Vector3(0, 0, spacing));
                    positions.Add(center + new Vector3(-spacing, 0, -spacing));
                    positions.Add(center + new Vector3(spacing, 0, -spacing));
                }
                break;

            case FormationType.Square:
                if (count >= 4)
                {
                    positions.Add(center + new Vector3(-spacing, 0, spacing));
                    positions.Add(center + new Vector3(spacing, 0, spacing));
                    positions.Add(center + new Vector3(-spacing, 0, -spacing));
                    positions.Add(center + new Vector3(spacing, 0, -spacing));
                }
                break;

            case FormationType.Pentagon_3_2:
                if (count >= 5)
                {
                    positions.Add(center + new Vector3(-spacing, 0, spacing));
                    positions.Add(center + new Vector3(0, 0, spacing));
                    positions.Add(center + new Vector3(spacing, 0, spacing));
                    positions.Add(center + new Vector3(-spacing / 2, 0, -spacing));
                    positions.Add(center + new Vector3(spacing / 2, 0, -spacing));
                }
                break;

            case FormationType.Hexagon:
                if (count >= 6)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = i * Mathf.PI * 2f / 6;
                        float x = Mathf.Cos(angle) * spacing;
                        float z = Mathf.Sin(angle) * spacing;
                        positions.Add(center + new Vector3(x, 0, z));
                    }
                }
                break;
        }

        return positions;
    }

    void SelectAllUnits()
    {
        UnitController[] allUnits = FindObjectsOfType<UnitController>();
        selectionManager.ClearSelection();

        foreach (UnitController unit in allUnits)
        {
            unit.IsSelected = true;
            selectionManager.AddToSelection(unit);
        }
    }
}