using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPickupCarBehaviour : MonoBehaviour
{
    public GameObject car;

    [Header("Target")]
    public Transform playerTank; // assign nebo najdi podle tagu/name "Merkava"
    public Transform rocketLauncher;

    [Header("Combat")]
    public GameObject rocketPrefab;
    public float detectionRange = 500f;
    public float fireCooldown = 6f;
    public int salvoSize = 5;

    [Header("Flanking")]
    public float flankMinAngle = 45f;
    public float flankMaxAngle = 60f;
    public float repositionDistance = 80f;
	public float linearVelocity;

    private bool canFire = true;
    private NavMeshAgent agent;
	public float enemy_tank_dist;
	
	[Header("Debug Trajectory")]
	public int trajectoryResolution = 30;
	public float arcHeight = 25f;
	public Color trajectoryColor = Color.green;
	
	public Vector3[] pathPoints;
	private int currentPathIndex;
	
	public Vector3[] orbitPoints;
public int orbitIndex;

public float safeDistance = 500f;
public float safeTolerance = 0.2f;

private float currentOrbitRadius;
private float minDist;
private float maxDist;

public float orbitRebuildCooldown = 2f;
private float nextOrbitRebuildTime = 0f;

public float orbitRadius = 20f; // 40 diameter
public int orbitResolution = 24;

public float orbitRandomOffset = 8f;

public Vector3 orbitCenter;
public float orbitCenterDistance = 20f;

public bool IsAvoidingObstacle;

private bool passA;
private bool passB;

private Vector3 PointA;
private Vector3 PointB;
private Vector3 resumeTarget;

public float height_y;
public Transform target;

[Header("Line of Sight")]
public float losCheckDistance = 200f, searchTime=3.0f;
public bool hasLineOfSight;

[Header("LOS Reposition")]
public bool isSearchingLOS;
public float searchAngle = 135f; // 90 / 135 / 180
public int searchSteps = 8;

private Vector3[] losSearchPoints;
private int losSearchIndex;

[Header("Memory")]
public Vector3 lastKnownPlayerPos;
public bool hasLastKnownPos;
public float memoryDuration = 5f;

private float lastSeenTime;

    void Start()
    {
        car = this.gameObject;
		height_y = car.transform.position.y;
        rocketLauncher = car.transform.GetChild(0)
                                    .GetChild(2)
                                    .GetChild(0);

        agent = GetComponent<NavMeshAgent>();

        if (playerTank == null)
        {
            GameObject target = GameObject.Find("Merkava");
            if (target != null) playerTank = target.transform;
        }
		
		minDist = safeDistance * (1f - safeTolerance); // 400
		maxDist = safeDistance * (1f + safeTolerance); // 600

		currentOrbitRadius = Random.Range(minDist, maxDist);

		GenerateOrbitCenter();
		GenerateOrbitPath();
    }

    void Update()
    {
		
        if (playerTank == null) return;
		if(rocketLauncher==null)
			rocketLauncher = car.transform.GetChild(0).GetChild(2).GetChild(0).gameObject.transform;
		
		float playerYaw = GetYawToTarget(playerTank, transform);
		float pickupYaw = transform.eulerAngles.y;

		float diff = Mathf.DeltaAngle(playerYaw, pickupYaw);	
		
		float distToPlayer = Vector3.Distance(transform.position, playerTank.position);
		
		// otočení směru jízdy (důležité pro "AI feeling")
    Vector3 dir = (target.position - transform.position);
    if (dir != Vector3.zero)
    {
       Quaternion rot = Quaternion.LookRotation(dir);

	// 🔥 otočení o 180° (jede pozadu, ale kouká dopředu)
	Quaternion reverseRot = rot * Quaternion.Euler(0f, 180f, 0f);

	transform.rotation = Quaternion.Slerp(
    transform.rotation,
    reverseRot,
    5f * Time.deltaTime
	);
	
		
		if (Time.time > nextOrbitRebuildTime)
	{
    bool needsRebuild = false;

    if (distToPlayer < minDist)
    {
        // hráč je moc blízko → zvětšit orbit
        currentOrbitRadius = Mathf.Lerp(currentOrbitRadius, maxDist, 0.5f);
        needsRebuild = true;
    }
    else if (distToPlayer > maxDist)
    {
        // hráč je daleko → zmenšit orbit
        currentOrbitRadius = Mathf.Lerp(currentOrbitRadius, minDist, 0.5f);
        needsRebuild = true;
    }

    if (needsRebuild)
    {
        GenerateOrbitCenter();
        GenerateOrbitPath();
        nextOrbitRebuildTime = Time.time + orbitRebuildCooldown;
    }
	}
	
	if (!IsAvoidingObstacle)
	{
    dir = (orbitPoints[orbitIndex] - transform.position).normalized;

    Ray ray = new Ray(transform.position + Vector3.up, dir);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 15f))
    {
        if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("building"))
        {
            StartObstacleAvoidance(hit);
        }
    }
	}
	
	if (Mathf.Abs(diff) < 30f)
	{
    // hráč je v přímé ose → posuň orbit center
    Vector3 right = playerTank.right;

    float side = (diff > 0f) ? 1f : -1f;

    orbitCenter = playerTank.position + right * side * 30f;

    GenerateOrbitPath();
	}
	//	rocketLauncher = transform.Find("rocket_launcher");
        enemy_tank_dist = Vector3.Distance(transform.position, playerTank.position);

        if (enemy_tank_dist <= detectionRange)
		{
			RotateLauncher();

			RaycastHit losHit;
			hasLineOfSight = CheckLineOfSight(out losHit);
			
			if (hasLineOfSight)//enemy "see" player in direct line
				{
				lastKnownPlayerPos = playerTank.position; //last known position of player
				hasLastKnownPos = true;
				lastSeenTime = Time.time;
				}
				else
				{
				if (Time.time - lastSeenTime > memoryDuration)
					{
						hasLastKnownPos = false;
					}
				}

			if (!hasLineOfSight && !isSearchingLOS)
			{
				if (hasLastKnownPos)
					{
						GenerateLOSSearch(losHit, lastKnownPlayerPos);
					}
			}
			if (Vector3.Distance(transform.position, lastKnownPlayerPos) < 5f)
			{
				hasLastKnownPos = false;
				GenerateOrbitPath(); // fallback
			}
			if (isSearchingLOS && searchTime > 4f) //kratka trpelivost, pokud hleda dlouho, vypne funkci
			{
				string[] lostSightLines = {
				"Lost visual!",
				"No line of sight!",
				"Target behind cover!",
				"Repositioning!"
				};
				/*
				. 🧠 Pojmenuj to

„Adaptive LOS AI“
„Obstacle-aware combat AI“
„Tactical pickup behavior“
				*/

				string line = lostSightLines[Random.Range(0, lostSightLines.Length)];//no "Aargh !!! Fucking jihad !!!" :D
				isSearchingLOS = false;
				GenerateOrbitPath();
			}
		}
		if (IsAvoidingObstacle)
{
    Vector3 target = !passA ? PointA :
                     !passB ? PointB :
                     resumeTarget;
/**/
    transform.position = Vector3.MoveTowards(
        transform.position,
        target,
        20f * Time.deltaTime
    );
	Vector3 groundPos = transform.position;
	groundPos.y = height_y;
	transform.position = groundPos;
    dir = (target - transform.position);

    if (dir != Vector3.zero)
    {
        rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 5f * Time.deltaTime);
    }
	}
    float dist = Vector3.Distance(transform.position, target.transform.position);

    if (!passA && dist < 2f) passA = true;
    else if (passA && !passB && dist < 2f) passB = true;

    if (passA && passB)
    {
        IsAvoidingObstacle = false;
    }

    return;
}
else
{
		
    Vector3 target;
	
//line of sight detection
    if (isSearchingLOS && losSearchPoints != null && losSearchPoints.Length > 0)
    {
        target = losSearchPoints[losSearchIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            20f * Time.deltaTime
        );
		float dist = Vector3.Distance(transform.position, target);
        if (dist< 2f)
        {
            losSearchIndex++;

            if (losSearchIndex >= losSearchPoints.Length)
            {
                isSearchingLOS = false;
            }
        }

        // 🔥 zkus znovu LOS během pohybu
       /* if (CheckLineOfSight())
        {
            isSearchingLOS = false;
        }*/

if (orbitPoints != null && orbitPoints.Length > 0)
{
    target = orbitPoints[orbitIndex];
	if(Vector3.Distance(target, transform.position)>50.0f)
	{
    transform.position = Vector3.MoveTowards(
        transform.position,
        target,
        20f * Time.deltaTime
    );
	}
}
else
{
    return;
}
}

    
		
		
    }

    if (Vector3.Distance(transform.position, target.position) < 1.5f)
    {
        orbitIndex++;

        if (orbitIndex >= orbitPoints.Length)
        {
            GenerateOrbitPath(); // nový orbit = dynamická AI
        }
    }

    }//konec Update
	
	bool CheckLineOfSight(out RaycastHit hitInfo)
{
    Vector3 dir = (playerTank.position - rocketLauncher.position).normalized;

    Ray ray = new Ray(rocketLauncher.position, dir);

    if (Physics.Raycast(ray, out hitInfo, losCheckDistance))
    {
        if (hitInfo.transform == playerTank)
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.green);
            return true;
        }
        else
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
            return false;
        }
    }

    return false;
}

void GenerateLOSSearch(RaycastHit hit, Vector3 targetPos)
{
    isSearchingLOS = true;

    losSearchPoints = new Vector3[searchSteps];
    losSearchIndex = 0;

    Vector3 obstaclePoint = hit.point;

    float radius = Vector3.Distance(transform.position, obstaclePoint);
    Vector3 fromObstacle = (transform.position - obstaclePoint).normalized;

    // 👉 směr k poslední známé pozici (ne aktuální hráč)
    Vector3 toTarget = (targetPos - transform.position).normalized;

    bool leftBlocked = CheckSideBlocked(-45f);
    bool rightBlocked = CheckSideBlocked(45f);

    float sideSign = 1f;

    if (leftBlocked && !rightBlocked)
        sideSign = 1f;
    else if (!leftBlocked && rightBlocked)
        sideSign = -1f;
    else
        sideSign = Mathf.Sign(Vector3.Dot(transform.right, toTarget));

    for (int i = 0; i < searchSteps; i++)
    {
        float t = i / (float)(searchSteps - 1);
        float angle = t * searchAngle * sideSign;

        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
        Vector3 dir = rot * fromObstacle;

        Vector3 point = obstaclePoint + dir * radius;
        point.y = transform.position.y;

        losSearchPoints[i] = point;
    }
}
bool CheckSideBlocked(float angle)//checks blocked side +/- 45 degrees from main raycast(forward)
{
    Vector3 dir = (playerTank.position - rocketLauncher.position).normalized;

    Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
    dir = rot * dir;

    Ray ray = new Ray(rocketLauncher.position, dir);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, losCheckDistance))
    {
        if (hit.transform != playerTank)
        {
            Debug.DrawLine(ray.origin, hit.point, Color.yellow);
            return true; // blokováno
        }
    }

    return false; // volné
}
	void StartObstacleAvoidance(RaycastHit hit)
{
    IsAvoidingObstacle = true;

    resumeTarget = orbitPoints[orbitIndex];

    Vector3 normal = hit.normal; 
    Vector3 hitPoint = hit.point;

    // směrová osa (boční směr kolem překážky)
    Vector3 side = Vector3.Cross(Vector3.up, normal).normalized;

    float sideSign = (Random.value > 0.5f) ? 1f : -1f;

    // 🔥 POINT A (blízko hráče)
    PointA = hitPoint
        + side * sideSign * 5f
        - normal * 30f;

    PointA.y = transform.position.y;

    // 🔥 POINT B (za překážkou)
    PointB = hitPoint
        + side * sideSign * 5f
        + normal * 30f;

    PointB.y = transform.position.y;

    passA = false;
    passB = false;
}
	
	float GetYawToTarget(Transform from, Transform to)
{
    Vector3 dir = (to.position - from.position);
    dir.y = 0f;

    return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
}
	
	void GenerateOrbitCenter()
	{
		Vector3 offset = Random.insideUnitSphere;
		offset.y = 0f;
		offset.Normalize();

		orbitCenter = playerTank.position + offset * orbitCenterDistance;
	}
	
	void GenerateOrbitPath()
{
    orbitPoints = new Vector3[orbitResolution + 1];

    float angleOffset = Random.Range(0f, 360f);
    float radius = currentOrbitRadius + Random.Range(-orbitRandomOffset, orbitRandomOffset);

    for (int i = 0; i <= orbitResolution; i++)
    {
        float t = i / (float)orbitResolution;
        float angle = t * 360f + angleOffset;

        float rad = angle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * radius;
        float z = Mathf.Sin(rad) * radius * 0.7f; // fazole

        // 🔥 TADY JE ZMĚNA
        Vector3 point = orbitCenter + new Vector3(x, 0f, z);

        orbitPoints[i] = point;
    }

    orbitIndex = 0;
}

    void RotateLauncher()
    {
		Vector3 dir = (playerTank.position - rocketLauncher.position).normalized;
		dir.y = 0f;


        Quaternion lookRot = Quaternion.LookRotation(dir);
        Vector3 euler = lookRot.eulerAngles;

        rocketLauncher.rotation = Quaternion.Euler(0f, euler.y, 0f);
		
		Ray ray = new Ray(rocketLauncher.position, dir);
		Debug.DrawLine(
		rocketLauncher.position,
		rocketLauncher.position + rocketLauncher.forward * detectionRange,
		Color.red
		);
    }

    IEnumerator FireSalvo()
    {
        canFire = false;

        for (int i = 0; i < salvoSize; i++)
        {
            FireRocket();
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(fireCooldown);

        RepositionFlank();

        canFire = true;
    }

  void FireRocket()
{
    GameObject rocket = Instantiate(
        rocketPrefab,
        rocketLauncher.position,
        rocketLauncher.rotation
    );

    RocketBehaviour2 rb = rocket.GetComponent<RocketBehaviour2>();
    rb.IsTriggered = true;

    float roll = Random.value;

    Vector3 targetPos = playerTank.position;

    if (roll <= 0.2f)
    {
        // 🎯 přímý zásah (nic neměň)
    }
    else if (roll <= 0.5f)
    {
        // 🛞 okraj tanku
        Vector3 offset = Random.insideUnitSphere * 5f;
        offset.y = 0f;
        targetPos += offset;
    }
    else
    {
        // 💥 miss
        Vector3 offset = Random.insideUnitSphere * 15f;
        offset.y = 0f;
        targetPos += offset;
    }

    rb.SetCustomTarget(targetPos);
}
	
	public bool IsMocingToNewDestination;
	public Vector3 NewEnemyDestination;
    void RepositionFlank()
{
    Vector3 toPlayer = (playerTank.position - transform.position).normalized;
    toPlayer.y = 0f;

    float angle = Random.Range(-15f, 15f);
    Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);

    Vector3 dir = rot * toPlayer;

    float dist = Vector3.Distance(transform.position, playerTank.position);
    float multiplier = Random.Range(1.0f, 1.2f);

    Vector3 target = playerTank.position + dir * dist * multiplier;

    GenerateFlankPath(transform.position, playerTank.position, target);
}

void GenerateFlankPath(Vector3 start, Vector3 player, Vector3 end)
{
    Vector3 right = Vector3.Cross(Vector3.up, (player - start).normalized);

    float side = Random.value > 0.5f ? 1f : -1f;

    Vector3 mid1 = player + right * side * 15f;
    Vector3 mid2 = player + right * side * 25f;

    pathPoints = new Vector3[3];
    pathPoints[0] = mid1;
    pathPoints[1] = mid2;
    pathPoints[2] = end;

    currentPathIndex = 0;
}
	Vector3[] allPoints = new Vector3[100];
	public int point_index;
	void OnGUI()
{
    //if (playerTank == null || rocketLauncher == null) return;

    Vector3[] points = CalculateTrajectory();
	/**/
    for (int i = 1; i < points.Length; i++)
    {
        Vector3 p1 = Camera.main.WorldToScreenPoint(points[i - 1]);
        Vector3 p2 = Camera.main.WorldToScreenPoint(points[i]);

        p1.y = Screen.height - p1.y;
        p2.y = Screen.height - p2.y;

        DrawLine(p1, p2, trajectoryColor, 2f);
    }
}
public bool IsMissileInstantiated;
public GameObject generated_missile, missile_prefab;
public int passed_points_index, max_points_index;
public float random_timer;
float shootCooldown = 0f;
float nextShootTime = 0f;
Vector3[] CalculateTrajectory()
{
    Vector3[] points = new Vector3[trajectoryResolution + 1];

    Vector3 start = rocketLauncher.position;
    Vector3 end = playerTank.position;

    for (int i = 0; i <= trajectoryResolution; i++)
    {
        float t = i / (float)trajectoryResolution;

        Vector3 point = Vector3.Lerp(start, end, t);
        float height = 4f * arcHeight * t * (1 - t);
        point.y += height;

        points[i] = point;
		
    }

	//rocket.GetComponent<Rocket>().SetPath(path); //pozdeji
	/**/
	shootCooldown -= Time.deltaTime;
	if (!IsMissileInstantiated && shootCooldown<=0)
{
    generated_missile = Instantiate(
        missile_prefab,
        points[0],
        Quaternion.identity
    );
	generated_missile.transform.GetComponent<RocketBehaviour2>().IsTriggered = true;//can start muzzle fire
    // Nastavení scale
    generated_missile.transform.localScale = new Vector3(21.798f, 21.798f, 21.798f);

    passed_points_index = 0;
    max_points_index = trajectoryResolution;
    IsMissileInstantiated = true;
}
	else if (IsMissileInstantiated && (passed_points_index < max_points_index))
{
    generated_missile.transform.position = points[passed_points_index];
	generated_missile.transform.GetComponent<RocketBehaviour2>().IsTriggered = true;//can start muzzle fire

    // Natáčení přímo na tank
    generated_missile.transform.rotation = car.transform.localRotation;

    passed_points_index++;
}
	else if(IsMissileInstantiated==true && passed_points_index>=max_points_index)
	{
  //  Destroy(generated_missile);
    IsMissileInstantiated = false;
	max_points_index = 0;
	
	// 🔥 tady nastavíš pauzu mezi střelami
    shootCooldown = Random.Range(2.0f, 5.0f); // sekundy
	//RepositionFlank();
	GenerateOrbitPath();
	}
    return points;
}
void DrawOrbit()
{
    if (orbitPoints == null || orbitPoints.Length < 2) return;

    for (int i = 1; i < orbitPoints.Length; i++)
    {
        Vector3 p1 = Camera.main.WorldToScreenPoint(orbitPoints[i - 1]);
        Vector3 p2 = Camera.main.WorldToScreenPoint(orbitPoints[i]);

        if (p1.z < 0 || p2.z < 0) continue;

        p1.y = Screen.height - p1.y;
        p2.y = Screen.height - p2.y;

        DrawLine(p1, p2, Color.yellow, 2f);
    }
}
void DrawLine(Vector2 p1, Vector2 p2, Color color, float width)
{
    Matrix4x4 matrix = GUI.matrix;

    Color oldColor = GUI.color;
    GUI.color = color;

    float angle = Vector3.Angle(p2 - p1, Vector2.right);
    if (p1.y > p2.y) angle = -angle;

    float length = (p2 - p1).magnitude;

    GUIUtility.RotateAroundPivot(angle, p1);
    GUI.DrawTexture(new Rect(p1.x, p1.y, length, width), Texture2D.whiteTexture);
    GUI.matrix = matrix;

    GUI.color = oldColor;
}
public void OnGui()
{
	DrawOrbit();
}
}


