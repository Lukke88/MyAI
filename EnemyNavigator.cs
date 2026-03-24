using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // nutné pro TMP text
//created by Lucas Juricka, 2026, maddex88@gmail.com
public class EnemyNavigator : MonoBehaviour
{
    public GameObject[] allGenHouses;
    public GameObject targetWall;

    public string runAnimationState = "Run";
    public string runParameter = "MoveState";

    private Animator anim;

    private float speed_movement = 20.0f;
    private bool isGoingToTarget = false;
    private bool isAvoidingWall = false;
	public bool isAtTargetWall;
    private Vector3 PointA;
    private Vector3 PointB;
    private bool passedA;
    private bool passedB;
	public GameObject[] allWalls;
	public GameObject currentWall, lastWall;
	public GameObject nextHideoutWall;
	public float distance;
	public float reachDistance = 20.0f;
	
	bool isFlanking;
	bool reachedZAlignment;

	float flankAmplitude = 4f;
	float flankFrequency = 2f;

	float startZ;
	float flankTimer;
	
	public Vector3[] wallWaypoints;

	public int waypointCount = 6;
	public float wallOffsetZ = 5.0f;
	public float flankStopOffset = 10.0f;

	private int currentWaypointIndex = -1;
	private float waypointTimer;
	private float nextWaypointChange;

	public float rayLength = 20.0f;
	
	public GameObject enemyPrefab;
	private GameObject enemyModel;
	
	 [Header("Speed Tracking")]
    public TMP_Text speedDisplay;   // TMP text na rychlost
    private Vector3 lastPosition;   // pro výpočet aktuální rychlosti
    private float totalDistance;    // celková vzdálenost od startu
    private float startTime;        // čas začátku pohybu

    private bool trackingSpeed = false;
	
	private float startDistance;
	
	bool isPeekRunning;
float peekTimer;

public float peekDistance = 30f;
public float peekSpeed = 1.5f;

Vector3 peekStart;
Vector3 peekDirection;
	
	public enum AnimationState
	{
		Navigating, 
		Shooting
	}
	
	public GameObject gunPrefab;
public ParticleSystem muzzleFlashPrefab;

private GameObject currentGun;
private Transform currentPlayer;
private bool isShooting = false;

public float fireDistance = 200f;
public int burstShots = 5;
public float spreadAngle = 5f;
    void Start()
{
    SetupEnvironment();

    anim = GetComponent<Animator>();

    if(anim == null)
        anim = gameObject.AddComponent<Animator>();

    allWalls = GameObject.FindGameObjectsWithTag("Wall");

    currentWall = GameObject.Find("big_wall");

    isGoingToTarget = true;

    PlayRunAnimation();
	
	lastPosition = transform.position; // počáteční pozice
        startTime = Time.time;
        trackingSpeed = true;
	
	
   // GameObject model = GameObject.Find("TalibanWarrior (4)");
GameObject model = Instantiate(enemyPrefab, transform);
    if(model != null)
    {
        enemyModel = model;

        enemyModel.transform.SetParent(transform);

        enemyModel.transform.localPosition = Vector3.zero;

        enemyModel.transform.localRotation = Quaternion.identity;
    }

    // schovat kapsli
    GetComponent<MeshRenderer>().enabled = false;
	GetComponent<Collider>().enabled = true; // necháme pro kolize
	
	
	
	model.transform.localPosition = Vector3.zero;
	model.transform.localRotation = Quaternion.identity;
	
	anim = GetComponentInChildren<Animator>();
	
}

    void Update()
	{
    if(currentWall == null) return;
	anim.SetFloat("Speed", speed_movement);
    distance = Vector3.Distance(transform.position, currentWall.transform.position);

    if(distance <= reachDistance)
    {
		isAtTargetWall = true;
		lastWall = currentWall;
		foreach(GameObject go in allWalls)
		{
			if(go!=lastWall && go.transform.position.x>lastWall.transform.position.x)
			{
				currentWall = go;
			}
		}
        FindNextHideoutWall();
    }
	if(isFlanking)
	{
    FlankMovement();
	}
	else
	{
    MoveTowardsTarget();
	}
	
	if(isAtTargetWall)
	{
    float zDiff = Mathf.Abs(transform.position.z - currentWall.transform.position.z);

    if(zDiff <= flankStopOffset)
    {
        GenerateWallWaypoints();

        SelectRandomWaypoint();

        isAtTargetWall = false;
    }
	}
	
	if(currentWaypointIndex >= 0)
	{
		MoveToWaypoint();
	}
	
	ForwardRaycast();
	
	// --- aktualizace rychlosti ---
        UpdateSpeedDisplay();
		
		UpdateSpeedTracking();
		
		if(WayPointTargetPosition != Vector3.zero &&
   Vector3.Distance(WayPointTargetPosition, transform.position) <= 1.0f)
	{
    StartFiringToPlayer();
	}

		if(isPeekRunning)
		{
			PeekMovement();
		}	
		
		if(Random.value < 0.002f)
		{
			StartPeekMovement();
		}
	}
	
	void StartPeekMovement()
{
    isPeekRunning = true;
    peekTimer = 0f;

    peekStart = transform.position;

    // směr od zdi směrem k hráči
    peekDirection = (currentPlayer.position - transform.position).normalized;
}

void PeekMovement()
{
    peekTimer += Time.deltaTime * peekSpeed;

    float t = Mathf.Sin(peekTimer * Mathf.PI);

    Vector3 forward = peekDirection * peekDistance * t;

    Vector3 side =
        transform.right *
        Mathf.Sin(peekTimer * Mathf.PI * 2f) *
        5f;

    transform.position =
        peekStart +
        forward +
        side;

    if(peekTimer >= 1f)
    {
        isPeekRunning = false;
        transform.position = peekStart;
    }
}
	public void StartFiringToPlayer()
{
    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

    if(players.Length == 0) return;

    float closest = Mathf.Infinity;
    GameObject nearest = null;

    foreach(GameObject p in players)
    {
        float d = Vector3.Distance(transform.position, p.transform.position);

        if(d < closest)
        {
            closest = d;
            nearest = p;
        }
    }

    if(nearest == null) return;

    currentPlayer = nearest.transform;

    // otočení k hráči
    Vector3 dir = (currentPlayer.position - transform.position).normalized;
    transform.rotation = Quaternion.LookRotation(dir);

    // vytvoření zbraně
    if(currentGun == null)
    {
        currentGun = Instantiate(gunPrefab);

        currentGun.transform.SetParent(transform);

        currentGun.transform.localPosition =
            new Vector3(0,1.2f,0.6f);

        currentGun.transform.localRotation =
            Quaternion.identity;

        // muzzle flash
        if(muzzleFlashPrefab != null)
        {
            ParticleSystem flash =
            Instantiate(muzzleFlashPrefab,
            currentGun.transform);

            flash.transform.localPosition =
                new Vector3(0,0,1.0f);
        }
    }

    if(!isShooting)
        StartCoroutine(FireBurst());
}

IEnumerator FireBurst()
{
    isShooting = true;

    for(int i=0;i<burstShots;i++)
    {
        ShootOnce();

        yield return new WaitForSeconds(
            Random.Range(0.1f,0.25f));
    }

    isShooting = false;
}

void ShootOnce()
{
    if(currentPlayer == null) return;

    Vector3 muzzle =
        currentGun.transform.position +
        currentGun.transform.forward * 0.8f;

    Vector3 dir =
        (currentPlayer.position - muzzle).normalized;

    // rozptyl
    dir = Quaternion.Euler(
        Random.Range(-spreadAngle,spreadAngle),
        Random.Range(-spreadAngle,spreadAngle),
        0) * dir;

    Ray ray = new Ray(muzzle, dir);

    RaycastHit hit;

    Vector3 endPoint;

    if(Physics.Raycast(ray,out hit, fireDistance))
    {
        endPoint = hit.point;
    }
    else
    {
        endPoint = muzzle + dir * fireDistance;
    }

    // beam
    GameObject beam =
        new GameObject("BulletTrace");

    LineRenderer lr =
        beam.AddComponent<LineRenderer>();

    lr.startWidth = 0.05f;
    lr.endWidth = 0.01f;

    lr.positionCount = 2;

    lr.SetPosition(0, muzzle);
    lr.SetPosition(1, endPoint);

    lr.material =
        new Material(
        Shader.Find("Sprites/Default"));

    lr.startColor = Color.yellow;
    lr.endColor = Color.yellow;

    Destroy(beam,0.1f);
}
	void StartSpeedTracking(Vector3 targetPosition)
    {
        startDistance = Vector3.Distance(transform.position, targetPosition);
        startTime = Time.time;
        trackingSpeed = true;
    }

    void UpdateSpeedDisplay()
    {
        if(!trackingSpeed || speedDisplay == null) return;

        float elapsedTime = Time.time - startTime;
        if(elapsedTime <= 0.01f) return;

        float currentDistance = Vector3.Distance(transform.position, currentWall.transform.position);
        float distanceTravelled = startDistance - currentDistance; // v metrech

        float speed_m_s = distanceTravelled / elapsedTime;
        float speed_kmh = speed_m_s * 3.6f;

        speedDisplay.text = $"Speed: {speed_m_s:F2} m/s | {speed_kmh:F2} km/h";

        // ukončení trackingu po dosažení cíle
        if(currentDistance <= 0.5f)
        {
            trackingSpeed = false;
        }
    }
	
	void UpdateSpeedTracking()
    {
        if (!trackingSpeed || speedDisplay == null) return;

        // vzdálenost od posledního frame
        float frameDistance = Vector3.Distance(transform.position, lastPosition);
        totalDistance += frameDistance;
        lastPosition = transform.position;

        // čas od startu
        float elapsedTime = Time.time - startTime;
        if(elapsedTime <= 0.001f) return;

        // aktuální rychlost (m/s) - předpoklad: 1 jednotka Unity = 1 metr
        float currentSpeed = frameDistance / Time.deltaTime;
        float currentSpeedKmh = currentSpeed * 3.6f;

        // průměrná rychlost
        float averageSpeed = totalDistance / elapsedTime;
        float averageSpeedKmh = averageSpeed * 3.6f;

        // zobrazit v TMP
        speedDisplay.text = $"Current: {currentSpeed:F2} m/s | {currentSpeedKmh:F1} km/h\n" +
                            $"Average: {averageSpeed:F2} m/s | {averageSpeedKmh:F1} km/h";
    }
	void ForwardRaycast()
{
    Ray ray = new Ray(transform.position, transform.forward);

    RaycastHit hit;

    Debug.DrawRay(transform.position, transform.forward * rayLength, Color.red);

    if(Physics.Raycast(ray, out hit, rayLength))
    {
        if(hit.collider.CompareTag("Player"))
        {
            Debug.Log("Enemy sees player");
        }
    }
}
	void GenerateWallWaypoints()
{
    wallWaypoints = new Vector3[waypointCount];

    Collider col = currentWall.GetComponent<Collider>();

    float wallSizeX = col.bounds.size.x;
    float startX = currentWall.transform.position.x - wallSizeX / 2f;

    float segment = wallSizeX / waypointCount;

    for(int i=0;i<waypointCount;i++)
    {
        float x = startX + segment * i;

        float y = transform.position.y;

        float z = currentWall.transform.position.z + wallOffsetZ;

        wallWaypoints[i] = new Vector3(x,y,z);
    }
}

void SelectRandomWaypoint()
{
    if(wallWaypoints == null || wallWaypoints.Length == 0)
        return;

    currentWaypointIndex = Random.Range(0, wallWaypoints.Length);

    nextWaypointChange = Random.Range(3f,7f);
    waypointTimer = 0f;
}
public Vector3 WayPointTargetPosition;
void MoveToWaypoint()
{
    if(currentWaypointIndex < 0) return;

    Vector3 target = wallWaypoints[currentWaypointIndex];
	WayPointTargetPosition = target;
    transform.position = Vector3.MoveTowards(
        transform.position,
        target,
        speed_movement * Time.deltaTime
    );

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(target - transform.position),
        Time.deltaTime * 5f
    );

    waypointTimer += Time.deltaTime;

    if(waypointTimer > nextWaypointChange)
    {
        SelectRandomWaypoint();
    }
}
void FlankMovement()
{
    Vector3 pos = transform.position;

    float targetZ = currentWall.transform.position.z;

    // krok 1: srovnání v ose Z
    if(!reachedZAlignment)
    {
        float zStep = speed_movement * Time.deltaTime;

        pos.z = Mathf.MoveTowards(pos.z, targetZ, zStep);

        transform.position = pos;

        if(Mathf.Abs(pos.z - targetZ) < 1f)
        {
            reachedZAlignment = true;
        }

        return;
    }

    // krok 2: sinusový pohyb v ose Z při postupu v X
    flankTimer += Time.deltaTime * flankFrequency;

    float sinOffset = Mathf.Sin(flankTimer) * flankAmplitude;

    pos.x = Mathf.MoveTowards(pos.x, currentWall.transform.position.x, speed_movement * Time.deltaTime);

    pos.z = targetZ + sinOffset;

    transform.position = pos;

    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(currentWall.transform.position - transform.position),
        Time.deltaTime * 5f
    );

    // dosažení cíle
    if(Vector3.Distance(transform.position, currentWall.transform.position) < reachDistance)
    {
        isFlanking = false;
    }
}
    // -----------------------------
    // NOVÁ FUNKCE PRO NASTAVENÍ PROSTŘEDÍ
    // -----------------------------
    void SetupEnvironment()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach(GameObject go in allObjects)
        {
            if(go.name.Contains("Building"))
            {
                // nastavíme tag
                go.tag = "Building";

                // přidáme collider pokud chybí
                if(go.GetComponent<Collider>() == null)
                {
                    BoxCollider bc = go.AddComponent<BoxCollider>();

                    Renderer r = go.GetComponent<Renderer>();
                    if(r != null)
                    {
                        bc.center = r.bounds.center - go.transform.position;
                        bc.size = r.bounds.size;
                    }
                }
            }
        }
    }

    void MoveTowardsTarget()
{
    if(!isAvoidingWall)
    {
        RaycastForward(20f);
		
		  if(!trackingSpeed)
                StartSpeedTracking(currentWall.transform.position);

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentWall.transform.position,
            speed_movement * Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(currentWall.transform.position - transform.position),
            Time.deltaTime * 5f
        );
    }
}

void FindNextHideoutWall()
{
    List<GameObject> possibleWalls = new List<GameObject>();

    foreach(GameObject wall in allWalls)
    {
        if(wall != lastWall && wall.transform.position.x > lastWall.transform.position.x)
        {
            possibleWalls.Add(wall);
        }
    }

    if(possibleWalls.Count == 0)
    {
        isAtTargetWall = false;
        return;
    }

    int randIndex = Random.Range(0, possibleWalls.Count);

    nextHideoutWall = possibleWalls[randIndex];

    currentWall = nextHideoutWall;

    isAtTargetWall = false;
	
	startZ = transform.position.z;
	isFlanking = true;
	reachedZAlignment = false;
	flankTimer = 0f;
}

    void PlayRunAnimation()
    {
        if(anim == null) return;

        anim.Play(runAnimationState);
        anim.SetInteger(runParameter, 1);
    }

    void RaycastForward(float rayLength)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, rayLength))
        {
            if(hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Building"))
            {
                AvoidWall(hit.collider.gameObject);
                isAvoidingWall = true;
            }
        }
    }

    void AvoidWall(GameObject obstruction)
    {
        float size_x = obstruction.GetComponent<Collider>().bounds.size.x;
        float size_z = obstruction.GetComponent<Collider>().bounds.size.z;

        float offset = 5.0f;

        if(PointA == Vector3.zero || PointB == Vector3.zero)
        {
            if(transform.position.x < obstruction.transform.position.x)
            {
                PointA = new Vector3(
                    obstruction.transform.position.x - size_x/2 - offset,
                    transform.position.y,
                    obstruction.transform.position.z - size_z/2 - offset
                );

                PointB = new Vector3(
                    obstruction.transform.position.x - size_x/2 - offset,
                    transform.position.y,
                    obstruction.transform.position.z + size_z/2 + offset
                );
            }
            else
            {
                PointA = new Vector3(
                    obstruction.transform.position.x + size_x/2 + offset,
                    transform.position.y,
                    obstruction.transform.position.z - size_z/2 - offset
                );

                PointB = new Vector3(
                    obstruction.transform.position.x + size_x/2 + offset,
                    transform.position.y,
                    obstruction.transform.position.z + size_z/2 + offset
                );
            }
        }
        else
        {
            if(!passedA && !passedB)
            {
                transform.position = Vector3.MoveTowards(transform.position, PointA, speed_movement * Time.deltaTime);

                if(Vector3.Distance(transform.position, PointA) <= 1f)
                    passedA = true;
            }
            else if(passedA && !passedB)
            {
                transform.position = Vector3.MoveTowards(transform.position, PointB, speed_movement * Time.deltaTime);

                if(Vector3.Distance(transform.position, PointB) <= 1f)
                    passedB = true;
            }

            if(passedA && passedB)
            {
                isAvoidingWall = false;
                PointA = Vector3.zero;
                PointB = Vector3.zero;
                passedA = false;
                passedB = false;
            }
        }
    }
}