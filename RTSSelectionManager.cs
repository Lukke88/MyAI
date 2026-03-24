using UnityEngine;
using System.Collections.Generic;

public class RTSSelectionManager : MonoBehaviour
{
    public RectTransform selectionBox;
    public Canvas canvas;

    private Vector2 startPos;
    private List<UnitController> selectedUnits = new List<UnitController>();

    void Update()
    {
        HandleSelection();
        HandleAttackInput();
    }

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
        }

        if (Input.GetMouseButton(0))
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            selectionBox.gameObject.SetActive(false);
        }
    }

    void UpdateSelectionBox(Vector2 currentMousePos)
    {
        Vector2 size = currentMousePos - startPos;

        selectionBox.anchoredPosition = startPos;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    void SelectUnits()
    {
        selectedUnits.Clear();

        foreach (UnitController unit in FindObjectsOfType<UnitController>())
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (IsWithin(screenPos))
            {
                unit.IsSelected = true;
                selectedUnits.Add(unit);
            }
            else
            {
                unit.IsSelected = false;
            }
        }
    }

    bool IsWithin(Vector3 screenPos)
    {
        Vector2 min = selectionBox.anchoredPosition;
        Vector2 max = min + selectionBox.sizeDelta;

        return screenPos.x > Mathf.Min(min.x, max.x) &&
               screenPos.x < Mathf.Max(min.x, max.x) &&
               screenPos.y > Mathf.Min(min.y, max.y) &&
               screenPos.y < Mathf.Max(min.y, max.y);
    }

    void HandleAttackInput()
    {
        if (selectedUnits.Count == 0)
            return;

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                foreach (var unit in selectedUnits)
                    unit.LookAtPoint(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (var unit in selectedUnits)
                unit.Shoot();
        }
    }

    public List<UnitController> GetSelectedUnits()
    {
        return selectedUnits;
    }
	
	public void ClearSelection()
{
    foreach (var unit in selectedUnits)
        unit.IsSelected = false;

    selectedUnits.Clear();
}

public void AddToSelection(UnitController unit)
{
    if (!selectedUnits.Contains(unit))
        selectedUnits.Add(unit);
}
}