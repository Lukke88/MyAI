using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class JennyBehaviour : MonoBehaviour
{
    public Animator animator;

    [Header("Movement")]
    public float moveSpeed = 25.0f;
    public float rotationSpeed = 120.0f;
	
	[Header("Management hero")]
	public GameObject heroObj;
	public Vector3 DestinationPoint;
	public float speed = 5.0f;


    // =========================
    // ENUMS
    // =========================

    public enum JennyAnimatorParam
    {
        IsJennyRunning,
        IsJennyShootingStand,
        IsJennyShootingCrouch,
        IsJennyWalking,
        IsJennyStandingUp,
        IsJennyIdle,
        IsJennyHit,
        IsJennyFalls,
        IsJennyCreepWalking
    }

    public enum JennyAnimationState
    {
        Idle,
        Run,
        Walk,
        Hides,
        ShootCrouch,
        FallsToGround,
        StandsUp,
        Idle_1,
        ShootsStand,
        HitStand,
        CreepWalk
    }

    private JennyAnimationState currentState = JennyAnimationState.Idle;

    // =========================
    // UNITY
    // =========================
	public enum ObstacleTag
{
    Wall,
    Building,
	wall,
	building,
	car, 
	tank
}

[Header("Obstacle Avoidance")]
public GameObject flagPrefab;
private GameObject activeFlag;

private Vector3 originalDestination;
private Vector3 pointA;
private Vector3 pointB;
private bool pointA_passed = false;
private bool pointB_passed = false;


    void Start()
    {
        SetAnimationState(JennyAnimationState.Idle);
    }

    void Update()
    {
		DetectObjectUsingRaycastAndAvoid();
		TurnUsingControl();
        HandleMovement();
		
		 Ray ray = new Ray(heroObj.transform.position, heroObj.transform.forward);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 50f))
    {
        Debug.Log("Hit: " + hit.collider.name);
    }

    // Červená čára od hrace dopředu
    Debug.DrawRay(heroObj.transform.position, heroObj.transform.forward * 50f, Color.red);
    }

    // =========================
    // MOVEMENT + INPUT
    // =========================
	
    void HandleMovement()
    {
        bool moveForward = Input.GetKey(KeyCode.W);
        bool turnLeft = Input.GetKey(KeyCode.A);
        bool turnRight = Input.GetKey(KeyCode.D);

        // ROTATION
        if (turnLeft)
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        else if (turnRight)
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // MOVEMENT
        if (moveForward)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            SetAnimationState(JennyAnimationState.Run);
        }
        else
        {
            SetAnimationState(JennyAnimationState.Idle);
        }
    }

    // =========================
    // ANIMATION CONTROL
    // =========================

    void SetAnimationState(JennyAnimationState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (newState)
        {
            case JennyAnimationState.Idle:
                animator.SetFloat(JennyAnimatorParam.IsJennyRunning.ToString(), 0.0f);
                animator.SetFloat(JennyAnimatorParam.IsJennyIdle.ToString(), 1.0f);
                break;

            case JennyAnimationState.Run:
                animator.SetFloat(JennyAnimatorParam.IsJennyIdle.ToString(), 0.0f);
                animator.SetFloat(JennyAnimatorParam.IsJennyRunning.ToString(), 1.0f);
                break;
        }
    }
	
	public void TurnUsingControl()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 2000) && Input.GetKeyDown(KeyCode.LeftControl))
    {
        DestinationPoint = hit.point;
    }

    if (DestinationPoint != Vector3.zero)
    {
        // ROTACE směrem k DestinationPoint
        Vector3 direction = DestinationPoint - heroObj.transform.position;
        direction.y = 0; // zamezí naklánění nahoru/dolů
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            heroObj.transform.rotation = Quaternion.RotateTowards(
                heroObj.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // POHYB
        heroObj.transform.position = Vector3.MoveTowards(heroObj.transform.position, DestinationPoint, speed * Time.deltaTime);

        // ANIMACE
        if (Vector3.Distance(heroObj.transform.position, DestinationPoint) < 0.1f)
        {
            // jsme u cíle -> Idle
            SetAnimationState(JennyAnimationState.Idle);
        }
        else
        {
            // běh k cíli
            SetAnimationState(JennyAnimationState.Run);
            animator.SetFloat("IsJennyRunning", 1.0f); // JennyRun parametr
        }
    }
}
/*
void DetectObjectUsingRaycastAndAvoid()
{
    if (DestinationPoint == Vector3.zero) return;

    // Umístění flagu při prvním kliknutí
    if (activeFlag == null)
    {
        activeFlag = Instantiate(flagPrefab, DestinationPoint, Quaternion.identity);
    }

    // Raycast před hrdinou
    Ray forwardRay = new Ray(heroObj.transform.position, heroObj.transform.forward);
    RaycastHit hit;

    if (Physics.Raycast(forwardRay, out hit, 50.0f))
    {
        // Detekce objektu s našimi tagy
        foreach (ObstacleTag tag in System.Enum.GetValues(typeof(ObstacleTag)))
        {
            if (hit.collider.CompareTag(tag.ToString()))
            {
                // uložíme původní cíl
                originalDestination = DestinationPoint;

                // nastavíme PointA a PointB + offset 5.0f
                Vector3 objPos = hit.collider.bounds.center;
                Vector3 objSize = hit.collider.bounds.size;

                // PointA = roh + offset v ose X
                pointA = new Vector3(objPos.x + objSize.x / 2 + 5.0f, heroObj.transform.position.y, objPos.z);
                pointB = new Vector3(objPos.x - objSize.x / 2 - 5.0f, heroObj.transform.position.y, objPos.z);

                DestinationPoint = pointA;
                pointA_passed = false;
                pointB_passed = false;
                break;
            }
        }
    }

    // POHYB K POINTům
    if (!pointA_passed && Vector3.Distance(heroObj.transform.position, pointA) < 0.5f)
    {
        pointA_passed = true;
        DestinationPoint = pointB;
    }
    else if (pointA_passed && !pointB_passed && Vector3.Distance(heroObj.transform.position, pointB) < 0.5f)
    {
        pointB_passed = true;
        DestinationPoint = originalDestination; // zpět k původnímu cíli
    }
}*/
[Header("Obstacle Avoidance Settings")]
public float detectDistance = 50.0f;
public float pointOffset = 5.0f;
public Color buildingDebugColor = Color.magenta;
public Color tankDebugColor = Color.red;
public Color carDebugColor = Color.yellow;
public Color wallDebugColor = Color.gray;

//private Vector3 originalDestination;
private List<Vector3> pathPoints = new List<Vector3>();
private int currentPathIndex = 0;
public GameObject flag;
void DetectObjectUsingRaycastAndAvoid()
{
	//testing purposes
	flag = GameObject.Find("flag");
	DestinationPoint = flag.transform.position;
	
    if (DestinationPoint == Vector3.zero) return;

    // Pokud je cesta prázdná, vytvoříme ji
    if (pathPoints.Count == 0)
    {
        originalDestination = DestinationPoint;
        pathPoints.Clear();
        pathPoints.Add(heroObj.transform.position); // start

        Vector3 lastPoint = heroObj.transform.position;
        bool obstacleFound = true;

        while (obstacleFound)
        {
            obstacleFound = false;

            RaycastHit hit;
            Vector3 direction = (DestinationPoint - lastPoint).normalized;

            if (Physics.Raycast(lastPoint, direction, out hit, detectDistance))
            {
                if (hit.collider.CompareTag("building"))
                {
                    obstacleFound = true;

                    // Debug fialová linie k překážce
                    Debug.DrawLine(lastPoint, hit.point, buildingDebugColor, 1.0f);

                    // body kolem překážky
                    Bounds b = hit.collider.bounds;
                    Vector3 pointA = new Vector3(b.max.x + pointOffset, lastPoint.y, b.center.z);
                    Vector3 pointB = new Vector3(b.min.x - pointOffset, lastPoint.y, b.center.z);

                    // Přidáme body podle vzdálenosti k cíli
                    if (Vector3.Distance(pointA, DestinationPoint) < Vector3.Distance(pointB, DestinationPoint))
                    {
                        pathPoints.Add(pointA);
                        pathPoints.Add(pointB);
                    }
                    else
                    {
                        pathPoints.Add(pointB);
                        pathPoints.Add(pointA);
                    }

                    lastPoint = pathPoints[pathPoints.Count - 1];
                }
            }
        }

        // přidáme finální DestinationPoint
        pathPoints.Add(DestinationPoint);
        currentPathIndex = 1;
    }

    // pohyb hrdiny podél pathPoints
    if (currentPathIndex < pathPoints.Count)
    {
        Vector3 target = pathPoints[currentPathIndex];
        heroObj.transform.position = Vector3.MoveTowards(heroObj.transform.position, target, speed * Time.deltaTime);

        // rotace k bodu
        Vector3 dir = target - heroObj.transform.position;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            heroObj.transform.rotation = Quaternion.RotateTowards(heroObj.transform.rotation, rot, rotationSpeed * Time.deltaTime);
        }

        // kontrola dosažení bodu
        if (Vector3.Distance(heroObj.transform.position, target) < 0.5f)
        {
            currentPathIndex++;
        }
    }

    // vykreslení celé cesty fialově
    for (int i = 0; i < pathPoints.Count - 1; i++)
    {
        Debug.DrawLine(pathPoints[i], pathPoints[i + 1], buildingDebugColor);
    }
}


}
