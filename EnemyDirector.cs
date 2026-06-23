using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDirector : MonoBehaviour
{
	public float start_height_y, speed;
    [Header("Enemy Setup")]
    public GameObject enemyPrefab;
    public string enemyPrefabName =
        "hezbollahTerrorist_gun_animated 1";
	public GameObject[] allEnemies;
	private List<GameObject> freeTiles =
    new List<GameObject>();

    public int maxEnemies = 10;

    public int count_spawned_enemies;

    public bool IsBlockedSpawning;

    [Header("References")]
    public GameObject player;

    public GameObject finalTargetObject;

    public GameObject currentClosestWall, nearest_wall_object;

    public GameObject mainWall;

    [Header("Generation Houses")]
    public GameObject[] houses;

    [Header("Tile System")]
    public GameObject tilePrefab;

    public int tileCount = 6;

    public float tileSpacing = 3f;

    public List<GameObject> generatedTiles =
        new List<GameObject>();
		
		[Header("Double Click")]

public float doubleClickTime = 0.3f;

private float lastClickTime;

private GameObject lastClickedObject;

    [Header("Enemy Movement")]
    public float enemySpeedMetersPerSecond =
        3.5f;

    public float enemyAttackRange = 120f;

    [Header("Outline")]
    public Material redOutline;

    public Material greenOutline;

    private Renderer lastRenderer;

    private Material originalMat;

    [Header("Debug")]
    public bool debugMode;
	
	public float hearingRadius = 500f;
	public float sightDistance = 500f;
	public float hearingForgetDelay = 30f;

	private Vector3 lastPlayerPosition;
	private bool playerIsMoving;
	
	public float minWallDistanceFromPlayer = 200f;
	public int max_number_reinforcement_units = 10;

public int flankEnemyCount = 5;

private List<GameObject> availableTiles = new List<GameObject>();
private bool isFlankActive;

    void Start()
    {
		allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        houses =
            GameObject.FindGameObjectsWithTag(
                "GenerationHouse"
            );

        enemyPrefab =
            GameObject.Find(enemyPrefabName);
			
			start_height_y = enemyPrefab.transform.position.y;

        currentClosestWall =
            FindClosestWallToPlayer();

        if(currentClosestWall != null)
        {
            GenerateTilesBehindWall(
                currentClosestWall
            );
        }
		
		if (player != null)
        lastPlayerPosition = player.transform.position;
    }
	public int max_number_of_enemies = 10;
    void Update()
    {
		allEnemies =
    GameObject.FindGameObjectsWithTag(
        "Enemy"
    );
        if(enemyPrefab == null)
        {
            enemyPrefab =
                GameObject.Find(enemyPrefabName);
        }
		if(enemyPrefab==null)
		enemyPrefab = GameObject.Find("Enemy");
		start_height_y = enemyPrefab.transform.position.y;
        HandleOutline();
		foreach(GameObject go in allEnemies)
		{
			
			if(go.transform.position.y<start_height_y)//prevents them going under ground
				go.transform.position = new Vector3(go.transform.position.x, start_height_y, go.transform.position.z);
				if(currentClosestWall!=mainWall)
				{
					if(go.name.Contains("Enemy"))
					{
						//go.transform.GetComponent<EnemyUnit>().destinationObject = currentClosestWall;//changes the destination for enemy AI
						
						EnemyUnit ai =
							go.GetComponent<EnemyUnit>();

						if(ai.destinationObject == null)
						{
							ai.destinationObject = currentClosestWall;
						}
					}
				}
		}
        UpdateClosestWallLogic();
		
		//ReactToPlayer();//put to EnemyUnit Update()
		
		
        SpawnLogic();
		
		HandleDoubleClick();
		
		HearingFunctionForEnemies();
		
		SightFunctionForEnemies();
		if(defenders<=max_number_reinforcement_units)
		SendGeneratedEnemiesToNewWall();
	
		RegulateNumberOfCreatedEnemies(max_number_reinforcement_units);
		
    }
	
	public void ReactToPlayer(GameObject go)//not active function, but can be used for all enemies simultaneously to navigate to player
	{
		GameObject player =
			GameObject.FindGameObjectWithTag("Player");

			if(player != null)
			{
			Vector3 dir =
			player.transform.position -
			transform.position;

			dir.y = 0;

			if(dir.magnitude > 0.1f)
			{
        Quaternion lookRotation =
            Quaternion.LookRotation(dir);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * 5f
            );
			}
			float distToPlayer =
		Vector3.Distance(
        transform.position,
        player.transform.position
		);

		if(distToPlayer > 200f)
		{
			MoveTowardsPlayer();
		}
		else
		{
		EnemyUnit ai =
		go.GetComponent<EnemyUnit>();

		ai.currentState =
		EnemyUnit.EnemyBehaviourState.Shooting;
		}
		}
	}
	
	void MoveTowardsPlayer()
{
    if(player == null)
        return;

    Vector3 dir =
        (player.transform.position - transform.position).normalized;

    transform.position +=
        dir * speed * Time.deltaTime;
}
	
	public int defenders;
	public int enemy_counter;
	public void RegulateNumberOfCreatedEnemies(int max_number_of_generated_enemies)
	{
		
		allEnemies =
    GameObject.FindGameObjectsWithTag(
        "Enemy"
    );
		
		foreach(GameObject go in allEnemies)
		{
			if(go.name.Contains("Clone") && enemy_counter<max_number_of_generated_enemies)
			{
				go.name = "generated_enemy_"+enemy_counter.ToString();
				enemy_counter++;
			}
			else if(go.name.Contains("Clone")&&enemy_counter>=max_number_of_generated_enemies)
			{
				Destroy(go);
			}
			if(go.transform.position.y<start_height_y)//prevents them going under ground
				go.transform.position = new Vector3(go.transform.position.x, start_height_y, go.transform.position.z);
		}
	}
	
	public void GenerateTilesBehindWall(
    GameObject wall
)
{
    ClearTiles();

    freeTiles.Clear();

   Vector3 right = wall.transform.right;

float wallLength =
    wall.GetComponent<Collider>().bounds.size.x;

for(int i = 0; i < tileCount; i++)
{
    float t =
        ((float)i / (tileCount - 1) - 0.5f);

    Vector3 pos =
        wall.transform.position +
        right * t * wallLength +
        (-wall.transform.forward * 3f);

    GameObject tile =
        Instantiate(tilePrefab, pos, Quaternion.identity);

    generatedTiles.Add(tile);
    freeTiles.Add(tile);
}
}

GameObject GetFreeRandomTile()
{
    if(freeTiles.Count == 0)
        return null;

    int randomIndex =
        Random.Range(
            0,
            freeTiles.Count
        );

    GameObject tile =
        freeTiles[randomIndex];

    freeTiles.RemoveAt(
        randomIndex
    );

    return tile;
}

GameObject GetFreeTile()
{
    if(freeTiles.Count == 0)
        return null;

    GameObject tile =
        freeTiles[0];

    freeTiles.RemoveAt(0);

    return tile;
}
	void SendGeneratedEnemiesToNewWall()
	{
		
		if(nearest_wall_object!=finalTargetObject)
		{
			
			foreach(GameObject go in allEnemies)
			{
				float wall_size = nearest_wall_object.transform.GetComponent<Collider>().bounds.size.z;
				/*if(//unnecessary
			Vector3.Distance(
            go.transform.position,
            currentClosestWall.transform.position
				) < 20f
				)
				{
					//defenders++;
				}*/
				if(Vector3.Distance(go.transform.position, nearest_wall_object.transform.position)>wall_size)
				{
					if(defenders < 3)
						{
						SendReinforcementsToWall(
						3 - defenders
						);
						}
					finalTargetObject = nearest_wall_object;//this stops the flow of enemies, once when function will generate 3
				}
			}
		}
	}

	public GameObject[] generated_enemies;
	public void SendReinforcementsToWall(
    int number_reinforcement_units)
{
	if(enemyPrefab==null)
		enemyPrefab = GameObject.Find("Enemy");
    GameObject[] allGenHouses =
        GameObject.FindGameObjectsWithTag(
            "GenerationHouse"
        );

    if(allGenHouses.Length == 0)
        return;

    List<GameObject> sortedHouses =
    new List<GameObject>(
        allGenHouses
    );

sortedHouses.Sort(
    (a,b)=>
    Vector3.Distance(
        a.transform.position,
        currentClosestWall.transform.position
    ).CompareTo(
        Vector3.Distance(
            b.transform.position,
            currentClosestWall.transform.position
        )
    )
);

    for(
        int i = 0;
        i < number_reinforcement_units;
        i++
    )
    {
         GameObject selectedHouse =
        sortedHouses[
            Random.Range(
                0,
                Mathf.Min(
                    3,
                    sortedHouses.Count
                )
            )
        ];
		if(defenders<=number_reinforcement_units)
		{
    GameObject enemy =
        Instantiate(
            enemyPrefab,
            selectedHouse.transform.position,
            Quaternion.identity
        );
		GameObject targetTile =
				GetFreeTile();
		defenders++;
		if(enemyPrefab.name.Contains("Enemy"))
		{
			EnemyUnit ai = enemy.transform.GetComponent<EnemyUnit>();
			ai.selected_wall = finalTargetObject;
			
			

			if(ai != null)
			{
				ai.destinationObject =
					targetTile;

				ai.selected_wall =
					currentClosestWall;
			}
		}
		else
		{
        HezbollahTerroristBehaviour ai =
            enemy.GetComponent
            <HezbollahTerroristBehaviour>();
		if(ai != null)
        {
            ai.nearest_wall_object =
                currentClosestWall;

            ai.finalTargetObject =
                currentClosestWall;

            ai.IsNewlyGenerated =
                true;
        }	
		}

        }
		
    }
}
	void UpdateClosestWallLogic()
{
    GameObject newWall = FindClosestWallToPlayer();

    if (newWall == null)
        return;

    Vector3 toWall = newWall.transform.position - player.transform.position;
    float dist = toWall.magnitude;

    Vector3 playerForward = player.transform.forward;
    playerForward.y = 0f;
    toWall.y = 0f;

    float forwardDot = Vector3.Dot(playerForward.normalized, toWall.normalized);

    bool isInFront = forwardDot > 0.3f; // v “směru pohybu”

    if (dist >= minWallDistanceFromPlayer && isInFront)
    {
        if (newWall != currentClosestWall)
		{
			currentClosestWall = newWall;

			GenerateTilesBehindWall(
				currentClosestWall
			);

			TriggerFlankSetup(
				currentClosestWall
			);
		}
    }
}
	
	public void SightFunctionForEnemies()
{
    GameObject[] enemies =
        GameObject.FindGameObjectsWithTag("Enemy");

    foreach (GameObject enemy in enemies)
    {
        HezbollahTerroristBehaviour ai =
            enemy.GetComponent<HezbollahTerroristBehaviour>();

        if (ai == null || player == null)
            continue;

        Vector3 origin = enemy.transform.position;
        Vector3 target = player.transform.position;

        float dist = Vector3.Distance(origin, target);

        if (dist > sightDistance)
        {
            ai.CanSeePlayer = false;
            continue;
        }

        Vector3 dir = (target - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, sightDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                ai.CanSeePlayer = true;
                ai.CanHearPlayer = true;

                ai.lastTimeSawPlayer = Time.time;
                continue;
            }
			if (!hit.collider.CompareTag("Player"))
			{
			ai.CanSeePlayer = false;

			// uloží poslední pozici
			ai.lastKnownPlayerPosition = player.transform.position;

			// aktivuj flank režim pokud ho slyší
			if (ai.CanHearPlayer)
			{
			ai.isFlanking = true;
			}
			}
        }

        // pokud nevidí
        ai.CanSeePlayer = false;

        // 30s decay pro hearing
        if (Time.time - ai.lastTimeSawPlayer > hearingForgetDelay)
        {
            ai.CanHearPlayer = false;
        }
    }
}
	
	void HearingFunctionForEnemies()
{
    if (player == null) return;

    float playerMoveDistance =
        Vector3.Distance(player.transform.position, lastPlayerPosition);

    playerIsMoving = playerMoveDistance > 0.05f;

    if (playerIsMoving)
    {
        Collider[] enemies =
            Physics.OverlapSphere(player.transform.position, hearingRadius);

        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                HezbollahTerroristBehaviour ai =
                    col.GetComponent<HezbollahTerroristBehaviour>();

                if (ai != null)
                {
                    ai.CanHearPlayer = true;
                }
            }
        }
    }
    else
    {
        // když se player nehýbe → postupné “ztrácení zvuku”
        Collider[] enemies =
            Physics.OverlapSphere(player.transform.position, hearingRadius);

        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy"))
            {
                HezbollahTerroristBehaviour ai =
                    col.GetComponent<HezbollahTerroristBehaviour>();

                if (ai != null)
                {
                    ai.CanHearPlayer = false;
                }
            }
        }
    }

    lastPlayerPosition = player.transform.position;
}

void HandleDoubleClick()
{
    if(Input.GetMouseButtonDown(0))
    {
        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        RaycastHit hit;

        if(
            Physics.Raycast(
                ray,
                out hit,
                500f
            )
        )
        {
            if(
                hit.collider.CompareTag(
                    "Player"
                )
            )
            {
                GameObject clickedObject =
                    hit.collider.gameObject;

                // stejny objekt + rychly klik
                if(
                    clickedObject ==
                    lastClickedObject &&
                    Time.time - lastClickTime
                    <= doubleClickTime
                )
                {
                    ActivatePlayer(
                        clickedObject
                    );
                }

                lastClickTime =
                    Time.time;

                lastClickedObject =
                    clickedObject;
            }
        }
    }
}

void ActivatePlayer(
    GameObject target
)
{
    WomanSniperBehaviour script =
        target.GetComponent
        <WomanSniperBehaviour>();

    if(script != null)
    {
        script.IsPlayerActivated = true;
    }
}
    //==================================================
    // SPAWN LOGIC
    //==================================================

    void SpawnLogic()
    {
        count_spawned_enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            ).Length;

        if(
            count_spawned_enemies < maxEnemies &&
            IsBlockedSpawning == false &&
            enemyPrefab != null
        )
        {
            SpawnEnemyFromRandomHouse();
        }
    }

    void SpawnEnemyFromRandomHouse()
    {
        if(houses.Length == 0)
            return;

        GameObject selectedHouse =
            houses[
                Random.Range(
                    0,
                    houses.Length
                )
            ];

        Vector3 offset =
            new Vector3(
                Random.Range(-2f, 2f),
                0,
                Random.Range(-2f, 2f)
            );

        GameObject generated_enemy =
            Instantiate(
                enemyPrefab,
                selectedHouse.transform.position +
                offset,
                Quaternion.identity
            );

        HezbollahTerroristBehaviour ai =
            generated_enemy.GetComponent
            <HezbollahTerroristBehaviour>();

        if(ai != null)
        {
            ai.nearest_wall_object =
                currentClosestWall;

            ai.IsNewlyGenerated = true;
        }

        IsBlockedSpawning = true;

        StartCoroutine(
            SpawnCooldown()
        );
    }

    IEnumerator SpawnCooldown()
    {
        yield return new WaitForSeconds(3f);

        IsBlockedSpawning = false;
    }

    //==================================================
    // WALL DETECTION
    //==================================================

    public GameObject FindClosestWallToPlayer()
    {
        GameObject[] walls =
            GameObject.FindGameObjectsWithTag(
                "Wall"
            );

        GameObject closest = null;

        float closestDist =
            Mathf.Infinity;

        foreach(GameObject wall in walls)
        {
            if(mainWall != null)
            {
                float yDiff =
                    Mathf.Abs(
                        wall.transform.eulerAngles.y -
                        mainWall.transform.eulerAngles.y
                    );

                if(yDiff > 5f)
                    continue;
            }

            float dist =
                Vector3.Distance(
                    player.transform.position,
                    wall.transform.position
                );

            if(dist < closestDist)
            {
                closestDist = dist;
                closest = wall;
            }
        }

        return closest;
    }

    void TriggerFlankSetup(GameObject wall)
{
    if (isFlankActive)
        return;

    isFlankActive = true;

    GenerateFlankTiles(wall);

    StartCoroutine(SpawnFlankWave());
}

void SpawnEnemyOnTile(GameObject tile) //navigate enemy to tile
{
    GameObject selectedHouse = houses[Random.Range(0, houses.Length)];

    GameObject enemy = Instantiate(
        enemyPrefab,
        selectedHouse.transform.position,
        Quaternion.identity
    );

    HezbollahTerroristBehaviour ai =
        enemy.GetComponent<HezbollahTerroristBehaviour>();

    if (ai != null)
    {
        ai.finalTargetObject = tile;
        ai.nearest_wall_object = currentClosestWall;
        ai.IsNewlyGenerated = true;
    }
}

void GenerateFlankTiles(GameObject wall) //flank tiles behind wall
{
    ClearTiles();
    availableTiles.Clear();

    Vector3 backward = -wall.transform.forward;

    for (int i = 0; i < 6; i++)
    {
        Vector3 pos =
            wall.transform.position +
            backward * (i * tileSpacing) +
            new Vector3(Random.Range(-2f,2f), 0, Random.Range(-2f,2f));

        GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity);

        generatedTiles.Add(tile);
        availableTiles.Add(tile);
    }
}

IEnumerator SpawnFlankWave() //delayed generation of enemies
{
    int spawned = 0;

    while (spawned < flankEnemyCount && availableTiles.Count > 0)
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));

        int tileIndex = Random.Range(0, availableTiles.Count);

        GameObject tile = availableTiles[tileIndex];
        availableTiles.RemoveAt(tileIndex);

        SpawnEnemyOnTile(tile);

        spawned++;
    }

    isFlankActive = false;
}

    //==================================================
    // TILE SYSTEM
    //==================================================

    public void GenerateNewTilesBehindWall(
        GameObject wall
    )
    {
        ClearTiles();

        Vector3 backward =
            -wall.transform.forward;

        for(int i = 0; i < tileCount; i++)
        {
            Vector3 pos =
                wall.transform.position +
                backward *
                (i * tileSpacing);

            GameObject tile =
                Instantiate(
                    tilePrefab,
                    pos,
                    Quaternion.identity
                );

            generatedTiles.Add(tile);
        }
    }

    public void ClearTiles()
    {
        foreach(GameObject tile in generatedTiles)
        {
            if(tile != null)
            {
                Destroy(tile);
            }
        }

        generatedTiles.Clear();
    }

    //==================================================
    // ENEMY MOVEMENT
    //==================================================

    public void SendEnemiesToTiles()
    {
        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag(
                "Enemy"
            );

        int tileIndex = 0;

        foreach(GameObject enemy in enemies)
        {
            if(tileIndex >= generatedTiles.Count)
                return;

            HezbollahTerroristBehaviour ai =
                enemy.GetComponent
                <HezbollahTerroristBehaviour>();

            if(ai != null)
            {
                ai.finalTargetObject =
                    generatedTiles[tileIndex];

                ai.nearest_wall_object =
                    currentClosestWall;

                ai.IsNewlyGenerated = true;
            }

            tileIndex++;
        }
    }

    //==================================================
    // REINFORCEMENTS
    //==================================================

    public void EvaluateReinforcements(
        GameObject targetWall
    )
    {
        if(houses.Length == 0)
            return;

        GameObject bestHouse = null;

        float shortestTime =
            Mathf.Infinity;

        foreach(GameObject house in houses)
        {
            float dist =
                Vector3.Distance(
                    house.transform.position,
                    targetWall.transform.position
                );

            float travelTime =
                dist /
                enemySpeedMetersPerSecond;

            if(travelTime < shortestTime)
            {
                shortestTime =
                    travelTime;

                bestHouse = house;
            }
        }

        if(bestHouse != null)
        {
            float wallDistToPlayer =
                Vector3.Distance(
                    targetWall.transform.position,
                    player.transform.position
                );

            if(
                wallDistToPlayer <=
                enemyAttackRange
            )
            {
                SpawnReinforcements(
                    bestHouse,
                    targetWall
                );
            }
        }
    }

    public void SpawnReinforcements(
        GameObject house,
        GameObject targetWall
    )
    {
        for(int i = 0; i < 3; i++)
        {
            Vector3 offset =
                new Vector3(
                    Random.Range(-3f, 3f),
                    0,
                    Random.Range(-3f, 3f)
                );

            GameObject newEnemy =
                Instantiate(
                    enemyPrefab,
                    house.transform.position +
                    offset,
                    Quaternion.identity
                );

            HezbollahTerroristBehaviour ai =
                newEnemy.GetComponent
                <HezbollahTerroristBehaviour>();

            if(ai != null)
            {
                ai.nearest_wall_object =
                    targetWall;

                ai.finalTargetObject =
                    targetWall;

                ai.IsNewlyGenerated = true;
            }
        }
    }

    //==================================================
    // TRAVEL TIME
    //==================================================

    public float CalculateTravelTime(
        Vector3 startPos,
        Vector3 endPos
    )
    {
        float dist =
            Vector3.Distance(
                startPos,
                endPos
            );

        return
            dist /
            enemySpeedMetersPerSecond;
    }

    //==================================================
    // OUTLINE SYSTEM
    //==================================================

    void HandleOutline()
    {
        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        RaycastHit hit;

        if(
            Physics.Raycast(
                ray,
                out hit,
                500f
            )
        )
        {
            Renderer rend =
                hit.collider.GetComponent
                <Renderer>();

            if(rend != null)
            {
                if(
                    lastRenderer != null &&
                    lastRenderer != rend
                )
                {
                    lastRenderer.material =
                        originalMat;
                }

                originalMat =
                    rend.material;

                if(
                    hit.collider.CompareTag(
                        "Enemy"
                    )
                )
                {
                    rend.material =
                        redOutline;
                }
                else if(
                    hit.collider.CompareTag(
                        "Player"
                    )
                )
                {
                    rend.material =
                        greenOutline;
                }

                lastRenderer = rend;
            }
        }
    }

    //==================================================
    // DEBUG
    //==================================================

    void OnDrawGizmos()
    {
        if(debugMode == false)
            return;

        Gizmos.color = Color.red;

        if(currentClosestWall != null)
        {
            Gizmos.DrawLine(
                player.transform.position,
                currentClosestWall.transform.position
            );
        }

        Gizmos.color = Color.green;

        foreach(GameObject tile in generatedTiles)
        {
            if(tile != null)
            {
                Gizmos.DrawSphere(
                    tile.transform.position,
                    1f
                );
            }
        }
    }
}