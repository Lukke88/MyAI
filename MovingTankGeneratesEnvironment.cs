using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MovingTankGeneratesEnvironment : MonoBehaviour
{
	public GameObject tank;
	public GameObject[] enemies;
	public float arc_size = 45.0f; 
	public float arc_radius = 200.0f;
	public bool IsTankLookingForward;
    // Start is called before the first frame update
    private Vector3 lastPosition;
	public float forwardThreshold = 0.7f; // jak moc musí směřovat dopředu

	public GameObject groundPrefab, prefab;
	public float rayHeight = 200f;
	public float rayDistance = 500f;
	public LayerMask groundLayer;
	public Vector3 spawnPos;
	private float timer = 0f;
	public float generateInterval = 1f;
	public List<GridCell> gridCells = new List<GridCell>();
	public float cellSize = 20f;
	public bool IsGeneratedGround;
	float averageDistance;
	public GameObject VehiclePrefab, GenerationHousePrefab, WeaponLootPrefab, WallPrefab;
void Start()
{
    tank = GameObject.Find(this.name);
    lastPosition = tank.transform.position;
	AddBuildingsToList();
	averageDistance = GetAverageDistance();
	enemies = GameObject.FindGameObjectsWithTag("Enemy");
}

void Update()
{
	timer += Time.deltaTime;

	if (timer < generateInterval) return;
	timer = 0f;

	GenerateGrid();
	GenerateGrid2();
    Vector3 movement = (tank.transform.position - lastPosition).normalized;

    if (movement.magnitude > 0.01f)
    {
        float dot = Vector3.Dot(tank.transform.forward, movement);

        IsTankLookingForward = dot > forwardThreshold;
    }

    lastPosition = tank.transform.position;
	
	Vector3 origin = tank.transform.position;
Vector3 forward = tank.transform.forward;
float outerRadius = arc_radius + 200.0f;
float angle_turn = 180.0f;
float forwardDistance = 200.0f;
Vector3 outerLeft = origin + Quaternion.Euler(0, -arc_size / 2 + angle_turn, 0) * forward * outerRadius;
Vector3 outerRight = origin + Quaternion.Euler(0, arc_size / 2 + angle_turn, 0) * forward * outerRadius;

Vector3 forwardPoint = tank.transform.position + tank.transform.forward * forwardDistance;

        RaycastHit hit;
        bool groundBelow = IsGroundBelow(forwardPoint, out hit);

        if (!groundBelow && IsGeneratedGround)
        {
            // Spawnujeme prefab
            Vector3 spawnPos = forwardPoint;
            spawnPos.y = tank.transform.position.y; // nebo hit.point.y, pokud chceme přesně na terén
            Instantiate(groundPrefab, spawnPos, Quaternion.identity);

            IsGeneratedGround = false; // deaktivujeme další generaci, dokud nevyjede dál
        }
        else if (groundBelow == true)
        {
            IsGeneratedGround = true; // znovu umožníme generaci, až bude pod bodem znovu země
        }

        lastPosition = tank.transform.position;


}

bool IsGroundBelow(Vector3 point, out RaycastHit hit)
    {
        Vector3 rayOrigin = point + Vector3.up * rayHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance, groundLayer))
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.name.Contains("Ground"))
                return true;
        }

        return false;
    }

GameObject FindNearestGround(Vector3 point)
{
    GameObject[] allGrounds = GameObject.FindGameObjectsWithTag("Ground");

    GameObject closest = null;
    float minDist = Mathf.Infinity;

    foreach (GameObject g in allGrounds)
    {
        float dist = Vector3.Distance(point, g.transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            closest = g;
        }
    }

    return closest;
}

void CheckAndGenerate(Vector3 checkPoint)
{
    RaycastHit hit;

    if (!IsGroundBelow(checkPoint, out hit))
    {
        GameObject nearest = FindNearestGround(checkPoint);
        if (nearest == null) return;

        Renderer r = nearest.GetComponent<Renderer>();
        float sizeX = r.bounds.size.x;

        Vector3 spawnPos = nearest.transform.position;

        // posun ve směru Z (jak chceš)
        spawnPos.z += sizeX;

        Instantiate(groundPrefab, spawnPos, Quaternion.identity);
    }
}

void OnDrawGizmos()
{
    if (tank == null) return;

    int segments = 30;
    float angleStep = arc_size / segments;

    Vector3 origin = tank.transform.position;
    Vector3 forward = tank.transform.forward;

    float outerRadius = arc_radius + 200.0f;
	float angle_turn = 180.0f;
    // === VNITŘNÍ OBLOUK (žlutý) ===
    Gizmos.color = Color.yellow;

    Vector3 innerPrev = origin + Quaternion.Euler(0, -arc_size / 2 + angle_turn, 0) * forward * arc_radius;

    for (int i = 1; i <= segments; i++)
    {
        float angle = -arc_size / 2 + angleStep * i;
        Vector3 innerNext = origin + Quaternion.Euler(0, angle + angle_turn, 0) * forward * arc_radius;

        Gizmos.DrawLine(innerPrev, innerNext);
        innerPrev = innerNext;
    }

    // === VNĚJŠÍ OBLOUK (oranžový) ===
    Gizmos.color = new Color(1f, 0.5f, 0f); // oranžová

    Vector3 outerPrev = origin + Quaternion.Euler(0, -arc_size / 2 + angle_turn, 0) * forward * outerRadius;

    for (int i = 1; i <= segments; i++)
    {
        float angle = -arc_size / 2 + angleStep * i;
        Vector3 outerNext = origin + Quaternion.Euler(0, angle + angle_turn, 0) * forward * outerRadius;

        Gizmos.DrawLine(outerPrev, outerNext);
        outerPrev = outerNext;
    }

    // === KRAJNÍ BODY ===
    Vector3 innerLeft = origin + Quaternion.Euler(0, -arc_size / 2 + angle_turn, 0) * forward * arc_radius;
    Vector3 innerRight = origin + Quaternion.Euler(0, arc_size / 2 + angle_turn, 0) * forward * arc_radius;

    Vector3 outerLeft = origin + Quaternion.Euler(0, -arc_size / 2 + angle_turn, 0) * forward * outerRadius;
    Vector3 outerRight = origin + Quaternion.Euler(0, arc_size / 2 + angle_turn, 0) * forward * outerRadius;

    // === SPOJENÍ ===
    Gizmos.color = Color.white;

    Gizmos.DrawLine(innerLeft, outerLeft);
    Gizmos.DrawLine(innerRight, outerRight);
	
	Gizmos.color = Color.gray;

	foreach (var cell in gridCells)
	{
    Gizmos.color = cell.occupied ? Color.red : Color.yellow;
    Gizmos.DrawWireCube(cell.worldPos, new Vector3(cellSize, 1f, cellSize));
	}
}

bool IsPointInRing(Vector3 point, Vector3 origin, Vector3 forward, float innerR, float outerR)
{
    Vector3 dir = point - origin;
    float dist = dir.magnitude;

    if (dist < innerR || dist > outerR)
        return false;

    float angle = Vector3.Angle(forward, dir);
    return angle <= arc_size / 2;
}
public int ground_counter;
void GenerateGrid()
{
    gridCells.Clear();

    Vector3 origin = tank.transform.position;
    Vector3 forward = Quaternion.Euler(0, 180f, 0) * tank.transform.forward;

    float innerR = arc_radius;
    float outerR = arc_radius + 200.0f;

    int gridExtent = Mathf.CeilToInt(outerR / cellSize);

    bool groundGeneratedInThisUpdate = false; // jednou za Update

    // --- NOVÉ: hranice generování ---
    GameObject bigWall = GameObject.Find("big_wall");
    GameObject nearestBuilding = FindNearestBuilding(tank.transform.position); // najdeme nejbližší budovu
    float minX = (bigWall != null) ? bigWall.transform.position.x + 10f : float.MinValue;
    float maxZ = (nearestBuilding != null) ? nearestBuilding.transform.position.z : float.MaxValue;
    float minMargin = 50f;

    for (int x = -gridExtent; x <= gridExtent; x++)
    {
        for (int z = -gridExtent; z <= gridExtent; z++)
        {
            Vector3 point = origin + new Vector3(x * cellSize, 0, z * cellSize);

            if (IsPointInRing(point, origin, forward, innerR, outerR))
            {
                GridCell cell = new GridCell(point);

                // --- GENERACE TERÉNU ---
                RaycastHit hit;
                if (!IsGroundBelow(point, out hit) && !groundGeneratedInThisUpdate)
                {
                    GameObject currentGround = FindNearestGround(tank.transform.position);
                    if (currentGround != null && ground_counter < 1)
                    {
                        float sizeZ = currentGround.GetComponent<Collider>().bounds.size.z;
                        float sizeX = currentGround.GetComponent<Collider>().bounds.size.x;

                        Vector3 spawnPos = currentGround.transform.position;

                        float deltaX = Mathf.Abs(point.x - tank.transform.position.x);
                        float deltaZ = Mathf.Abs(point.z - tank.transform.position.z);

                        if (deltaX < deltaZ)
                            spawnPos += currentGround.transform.forward * -sizeZ;
                        else
                            spawnPos += currentGround.transform.right * sizeX;

                        spawnPos.y = currentGround.transform.position.y;

                        GameObject newGround = Instantiate(currentGround, spawnPos, currentGround.transform.rotation);
                        newGround.name = "GeneratedGround_" + ground_counter.ToString();

                        groundGeneratedInThisUpdate = true;
                        cell.occupied = true;
                        ground_counter++;
                    }
                }
                else
                {
                    // --- GENERACE GRID DUMMY ---
                    if (!cell.occupied)
                    {
                        // --- NOVÉ: kontrola povolené zóny ---
                        bool canGenerate = true;
                        if (bigWall != null && tank.transform.position.x > bigWall.transform.position.x)
                        {
                            if (nearestBuilding != null && Vector3.Distance(point, nearestBuilding.transform.position) < 50f)
                            {
                                // oblast: od leveho okraje budovy + 10 za bigWall, do poslední budovy - margin
                                float leftX = bigWall.transform.position.x + 10f;
                                float rightX = nearestBuilding.transform.position.x; // nebo last building
                                float topZ = nearestBuilding.transform.position.z;
                                float bottomZ = topZ - minMargin;

                                if (!(point.x >= leftX && point.x <= rightX && point.z >= bottomZ && point.z <= topZ))
                                    canGenerate = false;
                            }
                        }

                        if (canGenerate)
                        {
                            float rand = Random.Range(0f, 100f);
                            GameObject objToSpawn = null;

                            if (rand <= 40f) objToSpawn = GenerationHousePrefab;    // 40% budova
                            else if (rand <= 55f)
							{
								objToSpawn = VehiclePrefab;      // 15% vozidlo
								/*
								//nahodne generovani vozidel ze slozky
								GameObject[] vehicles = Resources.LoadAll<GameObject>("Vehicles");

								GameObject randomVehicle = vehicles[Random.Range(0, vehicles.Length)];

								Instantiate(randomVehicle, pos, Quaternion.identity);
								*/
						
							}
                            else if (rand <= 60f) objToSpawn = WallPrefab;         // 5% zeď
                            else if (rand <= 62f) objToSpawn = WeaponLootPrefab;   // 2% zbraň/lootbox
							/*
							float chance = Random.Range(0f, 1f); //spawnovani kornetu
							if(chance <= 0.2f) SpawnKornetAtHiddenSpot(housePos);
							*/
                            if (objToSpawn != null)
                            {
                                GameObject obj = Instantiate(objToSpawn, point, Quaternion.identity);
                                obj.name = objToSpawn.name;
                                cell.occupied = true;
                            }
                        }
                    }
                }

                gridCells.Add(cell);
            }
        }
    }
}

void GenerateGrid2()
{
    gridCells.Clear();

    Vector3 origin = tank.transform.position;
    Vector3 forward = Quaternion.Euler(0, 180f, 0) * tank.transform.forward;

    float innerR = arc_radius;
    float outerR = arc_radius + 200.0f;

    int gridExtent = Mathf.CeilToInt(outerR / cellSize);

    for (int x = -gridExtent; x <= gridExtent; x++)
    {
        for (int z = -gridExtent; z <= gridExtent; z++)
        {
            Vector3 point = origin + new Vector3(x * cellSize, 0, z * cellSize);

            if (!IsPointInRing(point, origin, forward, innerR, outerR))
                continue;

            GridCell cell = new GridCell(point);

            if (!cell.occupied)
            {
                float rand = Random.Range(0f, 100f);

                // =========================
                // 🏢 HLAVNÍ BUDOVA
                // =========================
                if (rand <= 30f && ApprovedBuildings.Count > 0)
                {
                    if (IsFarEnough(point))
                    {
                        GameObject prefab = ApprovedBuildings[Random.Range(0, ApprovedBuildings.Count)];

                        GameObject mainBuilding = Instantiate(prefab, point, Quaternion.identity);
                        cell.occupied = true;

                        // 🔥 vytvoříme cluster domů
                   //     GenerateHouseCluster(point);
                    }
                }
                // =========================
                // 🚗 VOZIDLO
                // =========================
                else if (rand <= 45f)
                {
                    Instantiate(VehiclePrefab, point, Quaternion.identity);
                    cell.occupied = true;
                }
                // =========================
                // 🧱 ZEĎ
                // =========================
                else if (rand <= 55f)
                {
                    Instantiate(WallPrefab, point, Quaternion.identity);
                    cell.occupied = true;
                }
                // =========================
                // 🎁 LOOT
                // =========================
                else if (rand <= 60f)
                {
                    Instantiate(WeaponLootPrefab, point, Quaternion.identity);
                    cell.occupied = true;
                }
            }

            gridCells.Add(cell);
        }
    }
}
bool IsFarEnough(Vector3 point)
{
    foreach (GameObject b in ApprovedBuildings)
    {
        float dist = Vector3.Distance(point, b.transform.position);

        if (dist < averageDistance)
            return false;
    }

    return true;
}
void GenerateHouseCluster(Vector3 center)
{
    int totalCount = Random.Range(4, 7); // celkem objektů v clusteru

    // 👉 1x hlavní GenerationHouse
    Vector2 mainOffset2D = Random.insideUnitCircle * (cellSize * 1.5f);
    Vector3 mainPos = center + new Vector3(mainOffset2D.x, 0, mainOffset2D.y);

    if (IsPositionFree(spawnPos))
	{
		Instantiate(prefab, spawnPos, Quaternion.identity);
	}

    // 👉 zbytek = klasické budovy
    for (int i = 1; i < totalCount; i++)
    {
        if (ApprovedBuildings.Count == 0) return;

        Vector2 offset2D = Random.insideUnitCircle * (cellSize * 2.5f);
        Vector3 spawnPos = center + new Vector3(offset2D.x, 0, offset2D.y);

        GameObject prefab = ApprovedBuildings[Random.Range(0, ApprovedBuildings.Count)];

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}

bool IsPositionFree(Vector3 pos, float radius = 5f)
{
    Collider[] hits = Physics.OverlapSphere(pos, radius);
    return hits.Length == 0;
}
// Pomocná funkce: najde nejbližší budovu s názvem obsahujícím "Building" nebo "arab_house" a tag "GenerationHouse"
GameObject FindNearestBuilding(Vector3 pos)
{
    GameObject[] allBuildings = GameObject.FindGameObjectsWithTag("GenerationHouse");
    GameObject closest = null;
    float minDist = Mathf.Infinity;

    foreach (GameObject b in allBuildings)
    {
        if (b.name.Contains("Building") || b.name.Contains("arab_house"))
        {
            float dist = Vector3.Distance(pos, b.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = b;
            }
        }
    }

    return closest;
}
// radius, ve kterém GenerationHouse uslyší tank
public float hearingRadius = 25f;

// volat třeba v Update() nebo při pohybu tanku
void CheckHearing()
{
    Collider[] hitColliders = Physics.OverlapSphere(tank.transform.position, hearingRadius);
    foreach (var col in hitColliders)
    {
        if (col.CompareTag("GenerationHouse"))
        {
            GenerateEnemies(col.transform.position, tank.transform.position);
        }
    }
}

// --- hlavní funkce ---
void GenerateEnemies(Vector3 genHousePos, Vector3 tankPos)
{
    // najdeme všechny nepřátele uvnitř domu
    /*EnemyController2[] enemies = Physics.OverlapBox(genHousePos, new Vector3(5f, 5f, 5f))
                                    .Select(c => c.GetComponent<EnemyController2>())
                                    .Where(e => e != null)
                                    .ToArray();*/

    foreach (var enemy in enemies)
    {
        // spustíme AI, aby vyběhli z domu
     //   enemy.AlertTank(tankPos);
    }

float hearingRadius = 25f; // slyšitelný okruh kolem tanku

Collider[] hitColliders = Physics.OverlapSphere(tank.transform.position, hearingRadius);
foreach (var col in hitColliders)
{
    if (col.CompareTag("GenerationHouse"))
    {
        // aktivujeme nepřátele uvnitř domu
        EnemyController2[] enemies = col.GetComponentsInChildren<EnemyController2>();
        foreach (var enemy in enemies)
        {
            enemy.AlertTank(tank.transform.position);
        }
    }
}
}
// --- Volitelné: vizualizace povolené zóny ---
void OnDrawGizmosSelected()
{
    GameObject bigWall = GameObject.Find("big_wall");
    GameObject nearestBuilding = FindNearestBuilding(tank.transform.position);
    if (bigWall != null && nearestBuilding != null)
    {
        float leftX = bigWall.transform.position.x + 10f;
        float rightX = nearestBuilding.transform.position.x;
        float topZ = nearestBuilding.transform.position.z;
        float bottomZ = topZ - 50f;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3((leftX + rightX) / 2f, tank.transform.position.y, (topZ + bottomZ) / 2f),
            new Vector3(rightX - leftX, 1f, topZ - bottomZ)
        );
    }
}
public List<GameObject> ApprovedBuildings = new List<GameObject>();

    public void AddBuildingsToList()
    {
        ApprovedBuildings.Clear();

        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform t in children)
            {
                if (t.gameObject.name.Contains("Building") && !t.gameObject.name.Contains("2_2"))
                {
                    ApprovedBuildings.Add(t.gameObject);
                }
            }
        }

        Debug.Log("Buildings count: " + ApprovedBuildings.Count);
    }

    public float GetAverageDistance()
    {
        if (ApprovedBuildings.Count < 2)
            return 0f;

        float totalDistance = 0f;
        int pairCount = 0;

        for (int i = 0; i < ApprovedBuildings.Count; i++)
        {
            for (int j = i + 1; j < ApprovedBuildings.Count; j++)
            {
                float dist = Vector3.Distance(
                    ApprovedBuildings[i].transform.position,
                    ApprovedBuildings[j].transform.position
                );

                totalDistance += dist;
                pairCount++;
            }
        }

        return totalDistance / pairCount;
    }
}


public class GridCell
{
    public Vector3 worldPos;
    public bool occupied;

    public GridCell(Vector3 pos)
    {
        worldPos = pos;
        occupied = false;
    }
	
	
}

public class EnemyController2 : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 targetPos;
    private bool alerted = false;

    // zavolat, když nepřítel uslyší tank
    public void AlertTank(Vector3 tankPos)
    {
        alerted = true;
        // např. cíl k tanku, nebo náhodně kolem něj
        targetPos = tankPos + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
    }

    void Update()
    {
        if (alerted)
        {
            // pohyb k cíli
            Vector3 dir = (targetPos - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            // po dosažení cíle může AI začít střílet, hledat další waypoint apod.
        }
    }
}