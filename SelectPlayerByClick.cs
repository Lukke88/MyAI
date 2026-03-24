using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectPlayerByClick : MonoBehaviour
{
	public GameObject SelectionMarkerPrefab;
	private Dictionary<GameObject, GameObject> activeMarkers = new Dictionary<GameObject, GameObject>();
	public GameObject SelectionMarker;
    public List<GameObject> selectedPlayers = new List<GameObject>();
    public string selectableTag = "Player";
	public Texture2D whiteTexture;
	
	float doubleClickTime = 0.3f;
float lastRightClickTime;
Vector3 moveTargetPosition;

public float moveSpeed = 3f;
public float formationSpacing = 1.5f;

    Vector2 dragStartPos;
    bool isDragging;
	public void Start()
	{
		whiteTexture = new Texture2D(1, 1);
		whiteTexture.SetPixel(0, 0, Color.white);
		whiteTexture.Apply();
	}
    void Update()
    {
        HandleInput();
        HandleDragSelection();
		UpdateMarkers();
		 UpdateUnitMovement();
    }
	
	void UpdateUnitMovement()
{
    for (int i = 0; i < selectedPlayers.Count; i++)
    {
        GameObject unit = selectedPlayers[i];

        Vector3 formationOffset = GetFormationPosition(i);
        Vector3 targetPos = moveTargetPosition + formationOffset;

        unit.transform.position = Vector3.MoveTowards(
            unit.transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (selectedPlayers.Count > 0)
        {
            unit.transform.LookAt(targetPos);
        }
    }
}

Vector3 GetFormationPosition(int index)
{
    int row = Mathf.FloorToInt(Mathf.Sqrt(index));
    int col = index - row * row;

    float x = (col % 3) * formationSpacing;
    float z = (col / 3) * formationSpacing;

    return new Vector3(x, 0, z);
}
void UpdateMarkers()
{
    // Spawn / show markers for selected units
    foreach (GameObject unit in selectedPlayers)
    {
        if (!activeMarkers.ContainsKey(unit))
        {
            GameObject marker = Instantiate(
                SelectionMarkerPrefab,
                unit.transform.position + Vector3.up * 0.1f,
                Quaternion.identity
            );

            marker.transform.SetParent(unit.transform);
            activeMarkers.Add(unit, marker);
        }

        activeMarkers[unit].SetActive(true);
    }

    // Hide markers for unselected units
    List<GameObject> keys = new List<GameObject>(activeMarkers.Keys);

    foreach (GameObject unit in keys)
    {
        if (!selectedPlayers.Contains(unit))
        {
            activeMarkers[unit].SetActive(false);
        }
    }
}
    // ================================
    // INPUT ROUTER
    // ================================
    void HandleInput()
    {
        // Start drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
        }

        // Release click / drag
        if (Input.GetMouseButtonUp(0))
        {
            if (Vector2.Distance(dragStartPos, Input.mousePosition) < 10f)
            {
                SelectSingleUnit();
            }
            else
            {
                SelectUnitsInBox();
            }
        }

        // Deselect if clicking empty space
        if (Input.GetMouseButtonDown(1))
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                if (!IsPointerOverUI())
                {
                    selectedPlayers.Clear();
                }
            }
        }
		if (Input.GetMouseButtonDown(0))
		{
			if (Time.time - lastRightClickTime < doubleClickTime)
			{
				MoveSelectedUnits();
			}

			lastRightClickTime = Time.time;
		}
    }
	
	void MoveSelectedUnits()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit))
    {
        moveTargetPosition = hit.point;
    }
}
	void SelectUnitsInBox()
{
    foreach (GameObject unit in GameObject.FindGameObjectsWithTag(selectableTag))
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

        if (IsInsideDragBox(screenPos))
        {
            if (!selectedPlayers.Contains(unit))
                selectedPlayers.Add(unit);
        }
    }
}
    // ================================
    // SINGLE CLICK SELECT
    // ================================
    void SelectSingleUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            GameObject obj = hit.transform.gameObject;

            if (obj.CompareTag(selectableTag))
            {
                if (!selectedPlayers.Contains(obj))
                {
                    // If SHIFT not pressed → replace selection
                    if (!Input.GetKey(KeyCode.LeftShift))
                        selectedPlayers.Clear();

                    selectedPlayers.Add(obj);
                }
            }
        }
    }

    // ================================
    // DRAG BOX SELECTION
    // ================================
    void HandleDragSelection()
    {
        if (!Input.GetMouseButton(0)) return;

        isDragging = Vector2.Distance(dragStartPos, Input.mousePosition) > 10f;

        if (!isDragging) return;

        foreach (GameObject unit in GameObject.FindGameObjectsWithTag(selectableTag))
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);

            if (IsInsideDragBox(screenPos))
            {
                if (!selectedPlayers.Contains(unit))
                    selectedPlayers.Add(unit);
            }
        }
    }

    bool IsInsideDragBox(Vector3 screenPos)
    {
        Rect rect = new Rect(
            Mathf.Min(dragStartPos.x, Input.mousePosition.x),
            Mathf.Min(dragStartPos.y, Input.mousePosition.y),
            Mathf.Abs(dragStartPos.x - Input.mousePosition.x),
            Mathf.Abs(dragStartPos.y - Input.mousePosition.y)
        );

        return rect.Contains(screenPos);
    }

    // ================================
    // UI CHECK
    // ================================
    bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    // ================================
    // DEBUG INFO
    // ================================
   void OnGUI()
{
    if (Input.GetMouseButton(0))
    {
        Rect rect = GetScreenRect(dragStartPos, Input.mousePosition);
        DrawScreenRect(rect, new Color(1, 1, 1, 0.1f));
        DrawScreenRectBorder(rect, 2, Color.white);
    }

    GUI.Label(new Rect(10, 10, 300, 20),
        "Active figures: " + selectedPlayers.Count);
}

Rect GetScreenRect(Vector2 start, Vector2 end)
{
    start.y = Screen.height - start.y;
    end.y = Screen.height - end.y;

    Vector2 topLeft = Vector2.Min(start, end);
    Vector2 bottomRight = Vector2.Max(start, end);

    return Rect.MinMaxRect(
        topLeft.x,
        topLeft.y,
        bottomRight.x,
        bottomRight.y
    );
}

void DrawScreenRect(Rect rect, Color color)
{
    GUI.color = color;
    GUI.DrawTexture(rect, whiteTexture);
    GUI.color = Color.green;
}

void DrawScreenRectBorder(Rect rect, float thickness, Color color)
{
    DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
    DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
    DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
}
}