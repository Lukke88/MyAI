using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class JennyFindsWayToFlag : MonoBehaviour
{
    public Transform jenny;
    public Transform flag;
	public Collider jennyCollider;
	public float safetyMargin = 0.5f; // rezerva

    public Transform building1;
    public Transform building2;
    public Transform building3;

public bool building1_passed = false;
public bool building2_passed = false;
public bool building3_passed = false;
[Header("Navigation Points")]
    // Path points
    public Vector3 Point_A;
    public Vector3 Point_B;
    public Vector3 Point_C;
    public Vector3 Point_D;
	public Vector3 Point_E;
    public Vector3 Point_F;
	public Vector3 targetPos;
    // State flags
    public bool pointA_pass = false;
    public bool pointB_pass = false;
    public bool pointC_pass = false;
    public bool pointD_pass = false;
	public bool pointE_pass = false;
    public bool pointF_pass = false;
    public float cornerOffsetX = 15f;
	
	[Header("Interaction Raycast")]
public float interactionRayLength = 50f;
public LayerMask interactionMask;

[Header("UI")]
public GameObject buttonEnterVehicle;
public GameObject buttonGetItem;
public GameObject buttonOpenBox;

public Text actionInfoText;

private Transform detectedInteractable;
private JennyAnimState currentAnimState;

	
	public enum JennyAnimState
{
    Idle,
    Walking,
    Shooting,
    EnteringVehicle
}

public enum JennyAnimParam
{
    IsJennyShooting,
    IsJennyWalking,
    IsJennyInVehicle
}


    public List<Vector3> pathPoints = new List<Vector3>();
	
	public Vector3 cursorPoint, destinationPoint, target;//target je globalni promenna, urcujici cil cesty
private bool hasDestination = false;
	
	public float walkSpeed = 0.5f;        // rychlost chůze
private int currentPathIndex = 0;   // index aktuálního bodu, kam jít
public bool HasBuildingsBetween_bool;
    void Start()
    {
		if (!jennyCollider)
		jennyCollider = jenny.GetComponent<Collider>();

        SetUpSceneDefaults();
		SetBuildingTags();
    } 
	
	void Update()
{
    DetectMouseClick();
	DetectMouseClick();
    DetectForwardInteraction();
    DetectCursorInteraction();
    RotateJennyByMouse();
    if (!hasDestination)
        return;
	HasBuildingsBetween_bool = HasBuildingsBetween();

	if (HasBuildingsBetween_bool)
    {
		
        WalkThePath();
        WalkToDestination();
    }
    else 
    {
		if(Input.GetMouseButtonDown(0))target = cursorPoint;
        WalkDirectlyToFlag();
    }
}

	void LateUpdate()
	{
		//if(jenny==null || flag==null)
		
		//
        // === DEBUG DRAW ===
        /*
		for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Debug.DrawLine(
    pathPoints[i],
    pathPoints[i + 1],
    Color.magenta
);
        }*/
		if (!hasDestination)
        return;

    if (HasBuildingsBetween_bool)
    {	
SetBuildingTags();
        SetUpSceneDefaults();
        AssignPathPointsToNamedVectors();
    }

    GetUnitOffset();
	}
	
	public void DetectForwardInteraction()
{
    Ray ray = new Ray(jenny.position + Vector3.up * 1.5f, jenny.forward);
    RaycastHit hit;

    Debug.DrawRay(ray.origin, ray.direction * interactionRayLength, Color.red);

    if (Physics.Raycast(ray, out hit, interactionRayLength))
    {
        detectedInteractable = hit.transform;

        switch (hit.transform.tag)
        {
            case "car":
            case "helicopter":
                ShowEnterVehicleUI(hit.transform);
                break;

            case "Enemy":
                ActivateShootingRegime(hit.transform);
                break;

            default:
          //      ClearInteractionUI();
                break;
        }
    }
    else
    {
      //  ClearInteractionUI();
    }
}
void ShowEnterVehicleUI(Transform vehicle)
{
    buttonEnterVehicle.SetActive(true);
    actionInfoText.gameObject.SetActive(true);
    actionInfoText.text = "Enter " + vehicle.name;

    if (Input.GetMouseButtonDown(0)) // klik na tlačítko můžeš později oddělit
    {
        EnterVehicle(vehicle);
    }
}

void EnterVehicle(Transform vehicle)
{
    currentAnimState = JennyAnimState.EnteringVehicle;

    // navigace k vozidlu
    target = vehicle.position;
    hasDestination = true;

    // později:
    // jenny.SetParent(vehicle);
    // animator.SetBool("IsJennyInVehicle", true);

    Debug.Log("Entering vehicle: " + vehicle.name);
}
void ActivateShootingRegime(Transform enemy)
{
    currentAnimState = JennyAnimState.Shooting;

    // Animator
    // animator.SetFloat("IsJennyShooting", 1f);

    actionInfoText.gameObject.SetActive(true);
    actionInfoText.text = "Engaging enemy: " + enemy.name;

    // rotace k nepříteli
    Vector3 dir = enemy.position - jenny.position;
    dir.y = 0;
    if (dir.sqrMagnitude > 0.01f)
    {
        Quaternion rot = Quaternion.LookRotation(dir);
        jenny.rotation = Quaternion.Slerp(jenny.rotation, rot, 8f * Time.deltaTime);
    }
}
void DetectCursorInteraction()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 10f))
    {
        if (hit.transform.CompareTag("Item"))
        {
            buttonGetItem.SetActive(true);
            actionInfoText.text = "Get " + hit.transform.name;
        }
        else if (hit.transform.CompareTag("Box"))
        {
            buttonOpenBox.SetActive(true);
            actionInfoText.text = "Open the box";
        }
        else
        {
            buttonGetItem.SetActive(false);
            buttonOpenBox.SetActive(false);
        }
    }
}
void RotateJennyByMouse()
{
    if (!Input.GetKey(KeyCode.LeftControl))
        return;

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    Plane groundPlane = new Plane(Vector3.up, jenny.position);

    float enter;
    if (groundPlane.Raycast(ray, out enter))
    {
        Vector3 hitPoint = ray.GetPoint(enter);
        Vector3 dir = hitPoint - jenny.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            jenny.rotation = Quaternion.Slerp(jenny.rotation, rot, 10f * Time.deltaTime);
        }
    }
}

	void MarkPointAsPassed(int index)
{
    switch (index)
    {
        case 0: pointA_pass = true; break;
        case 1: pointB_pass = true; break;
        case 2: pointC_pass = true; break;
        case 3: pointD_pass = true; break;
        case 4: pointE_pass = true; break;
        case 5: pointF_pass = true; break;
    }
}

	void WalkDirectlyToFlag()
{
    if (!hasDestination || jenny == null)
        return;

    Vector3 target = destinationPoint;
    float step = walkSpeed * Time.deltaTime;

    jenny.position = Vector3.MoveTowards(jenny.position, target, step);

    Vector3 lookDir = target - jenny.position;
    if (lookDir.sqrMagnitude > 0.001f)
    {
        Quaternion rot = Quaternion.LookRotation(lookDir);
        jenny.rotation = Quaternion.Slerp(jenny.rotation, rot, 6f * Time.deltaTime);
    }

    if (Vector3.Distance(jenny.position, target) <= 0.2f)
    {
        hasDestination = false;
        currentPathIndex = 0;
    }
}

	bool HasBuildingsBetween()
{
    Vector3 dir = (flag.position - jenny.position).normalized;
    float dist = Vector3.Distance(jenny.position, flag.position);

    RaycastHit[] hits = Physics.RaycastAll(
        jenny.position + Vector3.up,
        dir,
        dist
    );

    foreach (RaycastHit hit in hits)
    {
        if (hit.transform.CompareTag("building"))
            return true;
    }

    return false;
}

	
	void DetectMouseClick()
{
	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
   // 
    {
        

        if (Physics.Raycast(ray, out hit))
        {
            // zde uložíme bod kam klikneme
            cursorPoint = hit.point;
            hasDestination = true;

            // aktualizujeme pathPoints podle budov
          //  SetUpSceneDefaults(); // pathPoints se aktualizují podle nového cíle
        }
		if (Input.GetMouseButtonDown(1))
		{
			destinationPoint = cursorPoint;
			targetPos = destinationPoint;
			hasDestination = true;

			currentPathIndex = 0;
			ResetPassedPoints();
		}
		else if(Vector3.Distance(jenny.position,targetPos)<=2.0f)
		{
			destinationPoint=Vector3.zero;
			targetPos = Vector3.zero;
		}
		//if(destinationPoint==Vector3.zero)//we didn't clicked yet
		//flag.transform.position = cursorPoint;
		if(destinationPoint!=Vector3.zero)
			flag.transform.position = destinationPoint;
    }
}

void ResetPassedPoints()
{
    pointA_pass = false;
    pointB_pass = false;
    pointC_pass = false;
    pointD_pass = false;
    pointE_pass = false;
    pointF_pass = false;
}

	/*
	//multi-rts version
	void DetectMouseClick()
{
    // === RIGHT CLICK ===
    if (Input.GetMouseButtonDown(1))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            cursorPoint = hit.point;
            destinationPoint = cursorPoint;
            targetPos = destinationPoint;
            hasDestination = true;

            // vizuální feedback
            flag.transform.position = destinationPoint;

            // tady později:
            // SetUpSceneDefaults();
        }
    }

    // === DESTINATION REACHED ===
    if (hasDestination && Vector3.Distance(jenny.position, targetPos) <= 2.0f)
    {
        hasDestination = false;
        destinationPoint = Vector3.zero;
        targetPos = Vector3.zero;
    }
}

	*/
	
void AssignPathPointsToNamedVectors()
{
    for (int i = 0; i < pathPoints.Count; i++)
    {
        char letter = (char)('A' + i); // 'A' + 0 = 'A', 'A' + 1 = 'B' ...
        string fieldName = "Point_" + letter;

        // Použití reflection k přiřazení hodnoty do Vector3 proměnné
        var field = this.GetType().GetField(fieldName);
        if (field != null)
        {
            field.SetValue(this, pathPoints[i]);
            Debug.Log($"{fieldName} = {pathPoints[i]}");
        }
        else
        {
            Debug.LogWarning($"{fieldName} neexistuje!");
        }
    }
}


public void WalkThePath()
{
    if (pathPoints.Count == 0 || jenny == null)
        return;

    // Pokud jsme došli na konec cesty, nic nedělej
    if (currentPathIndex >= pathPoints.Count)
        return;

    Vector3 targetPos = pathPoints[currentPathIndex];
    float step = walkSpeed * Time.deltaTime;

    // Pohyb směrem k aktuálnímu bodu
    jenny.position = Vector3.MoveTowards(jenny.position, targetPos, step);

    // Otáčení Jenny směrem k bodu
    Vector3 lookDir = targetPos - jenny.position;
    if (lookDir.sqrMagnitude > 0.001f)
    {
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        jenny.rotation = Quaternion.Slerp(jenny.rotation, targetRot, 5f * Time.deltaTime);
    }

    // Pokud jsme dosáhli bodu, přejdi na další
    float distanceThreshold = 0.1f; // tolerance, aby se nepřeskakovalo
    if (Vector3.Distance(jenny.position, targetPos) <= distanceThreshold)
    {
        currentPathIndex++;
    }
}

Vector3 NodePoint;
void WalkToDestination()
{
    if (!hasDestination || jenny == null || pathPoints.Count == 0)
        return;

    // Pokud cesta prázdná (nic mezi), jdi přímo
    if (pathPoints.Count == 1)
    {
        Vector3 dir = (NodePoint - jenny.position).normalized;
        float step = walkSpeed * Time.deltaTime;
        jenny.position = Vector3.MoveTowards(jenny.position, NodePoint, step);

        // otočení Jenny směrem k cíli
        if ((NodePoint - jenny.position).sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(NodePoint - jenny.position);
            jenny.rotation = Quaternion.Slerp(jenny.rotation, targetRot, 5f * Time.deltaTime);
        }

        // dorazila-li Jenny k cíli, zastav
        if (Vector3.Distance(jenny.position, NodePoint) <= 0.1f)
		{
    // označ bod jako prošlý
    MarkPointAsPassed(currentPathIndex);

    // posuň se jen dopředu – NIKDY zpět
    currentPathIndex++;

    // pojistka proti přetečení
    currentPathIndex = Mathf.Clamp(currentPathIndex, 0, pathPoints.Count);
	NodePoint=pathPoints[currentPathIndex];//novy bod pro navigaci
}

        return;
    }

    // Pokud je pathPoints > 1, postupujeme po bodech
    if (currentPathIndex >= pathPoints.Count)
        return;

    Vector3 targetPos = pathPoints[currentPathIndex];
    float stepMove = walkSpeed * Time.deltaTime;

    jenny.position = Vector3.MoveTowards(jenny.position, targetPos, stepMove);

    Vector3 lookDir = targetPos - jenny.position;
    if (lookDir.sqrMagnitude > 0.001f)
    {
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        jenny.rotation = Quaternion.Slerp(jenny.rotation, targetRot, 5f * Time.deltaTime);
    }

    // pokud dorazila k bodu, přejdi na další
    if (Vector3.Distance(jenny.position, targetPos) <= 0.1f)
    {
        currentPathIndex++;
    }

    // pokud jsme došli na poslední bod, zastav
    if (currentPathIndex >= pathPoints.Count)
    {
        hasDestination = false;
        currentPathIndex = 0;
    }
}

	float GetUnitOffset()
{
    if (!jennyCollider)
        return cornerOffsetX; // fallback

    // největší horizontální rozměr
    float halfWidth = Mathf.Max(
        jennyCollider.bounds.extents.x,
        jennyCollider.bounds.extents.z
    );

    return halfWidth + safetyMargin;
}

	public void SetUpSceneDefaults()
{
    pathPoints.Clear();

    if (!jenny)
        jenny = GameObject.Find("JennyFinal_lowpoly_z_erased").transform;

    if (!flag)
        flag = GameObject.Find("Flag-float.fbx").transform;
	if (!building1)
        building1 = GameObject.Find("building_1").transform;
		if (!building2)
        building1 = GameObject.Find("building_2").transform;
		if (!building3)
        building3 = GameObject.Find("building_3").transform;
    Vector3 currentPos = jenny.position;
    pathPoints.Add(currentPos);

    List<Transform> buildings = GetBuildingsBetween();

    foreach (Transform b in buildings)
    {
        Vector3 A, B;

        // první budova zprava, další klidně zleva – můžeš později rozšířit
        GetClosestWallCorners(b, currentPos, out A, out B);

        if (A != Vector3.zero && B != Vector3.zero)
        {
            pathPoints.Add(A);
            pathPoints.Add(B);
            currentPos = B;
        }
    }

    pathPoints.Add(flag.position);
}

	
	void GetClosestWallCornersLeft(
    Transform building,
    Vector3 fromPos,
    out Vector3 pointC,
    out Vector3 pointD)
{
    Renderer r = building.GetComponent<Renderer>();
    Bounds b = r.bounds;

    Vector3[] corners = new Vector3[4]
    {
        new Vector3(b.min.x, b.min.y, b.min.z),
        new Vector3(b.max.x, b.min.y, b.min.z),
        new Vector3(b.min.x, b.min.y, b.max.z),
        new Vector3(b.max.x, b.min.y, b.max.z)
    };

    pointC = corners[0];
    float minDist = Vector3.Distance(fromPos, pointC);

    foreach (Vector3 c in corners)
    {
        float d = Vector3.Distance(fromPos, c);
        if (d < minDist)
        {
            minDist = d;
            pointC = c;
        }
    }

    pointD = pointC;

    foreach (Vector3 c in corners)
    {
        if (Mathf.Approximately(c.x, pointC.x) &&
            !Mathf.Approximately(c.z, pointC.z))
        {
            pointD = c;
            break;
        }
    }

    // vždy LEFT offset – logiku strany řeš venku
    float offset = GetUnitOffset();
		pointC += Vector3.left * offset;
		pointD += Vector3.left * offset;
}

List<Transform> GetBuildingsBetween()
{
    List<Transform> buildings = new List<Transform>();

    Vector3 dir = (flag.position - jenny.position).normalized;
    float dist = Vector3.Distance(jenny.position, flag.position);

    RaycastHit[] hits = Physics.RaycastAll(
        jenny.position + Vector3.up, // trochu nad zemí
        dir,
        dist
    );

    foreach (RaycastHit hit in hits)
    {
        if (hit.transform.CompareTag("building"))
        {
            buildings.Add(hit.transform);
        }
    }

    // seřadit podle vzdálenosti od Jenny
    buildings.Sort((a, b) =>
        Vector3.Distance(jenny.position, a.position)
        .CompareTo(Vector3.Distance(jenny.position, b.position))
    );

    return buildings;
}

   

    void GetClosestWallCorners(
    Transform building,
    Vector3 fromPos,
    out Vector3 pointA,
    out Vector3 pointB)
{
    Renderer r = building.GetComponent<Renderer>();
    Bounds b = r.bounds;

    Vector3[] corners = new Vector3[4]
    {
        new Vector3(b.min.x, b.min.y, b.min.z),
        new Vector3(b.max.x, b.min.y, b.min.z),
        new Vector3(b.min.x, b.min.y, b.max.z),
        new Vector3(b.max.x, b.min.y, b.max.z)
    };

    // === FIND NEAREST CORNER (PointA) ===
    pointA = corners[0];
    float minDist = Vector3.Distance(fromPos, pointA);

    foreach (Vector3 c in corners)
    {
        float d = Vector3.Distance(fromPos, c);
        if (d < minDist)
        {
            minDist = d;
            pointA = c;
        }
    }

    // === FIND SECOND CORNER ON SAME WALL (PointB) ===
    pointB = pointA;

    foreach (Vector3 c in corners)
    {
        // Same X wall, different Z
        if (Mathf.Approximately(c.x, pointA.x) &&
            !Mathf.Approximately(c.z, pointA.z))
        {
            pointB = c;
            break;
        }
    }

    // === OFFSET FROM WALL ===
    float offset = GetUnitOffset();
		pointA += Vector3.right * offset;
		pointB += Vector3.right * offset;

}

private static void SetBuildingTags()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        int count = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("building_"))
            {
                obj.tag = "building";
                count++;
            }
        }

        Debug.Log($"Tag 'building' applied to {count} objects.");
    }

}
