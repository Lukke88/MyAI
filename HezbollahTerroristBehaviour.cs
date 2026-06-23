using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class HezbollahTerroristBehaviour : MonoBehaviour
{
    public GameObject enemy, enemy_prefab,target_object, nearest_wall_object, tile_object, nearest_player_object, player, finalTargetObject;
	
	public string animation_string = "hezbollah_terrorist_run";
	public string parameter_animation = "IsRunning";
	public Transform finalCoverPoint;
	public GameObject[] Generated_tiles;
	public Vector3[] generated_tiles_positions;
	public int max_count_tiles = 6, count_of_tiles;
	public float terrain_height_y = 18.0f;
	public float tile_gap;
	public float start_distance_enemy_wall, start_distance_tile_wall;
    public float minimal_contact_distance = 600.0f;
    public float speed = 5f;
	public float distance;
    public bool IsEnabledToShoot, IsShootingToPlayer;
	public bool canSee;
	public GameObject beam;
	
	[Header("Combat")]
public float shootRange = 200.0f;
public float rotationSpeed = 4.0f;
public LayerMask visibilityMask;

private bool playerVisible;
private float currentDistanceToPlayer;

[Header("Audio")]
public AudioSource audioSource;
public AudioClip shootClip;

[Range(0.5f, 1.5f)]
public float minPitch = 0.95f;

[Range(0.5f, 1.5f)]
public float maxPitch = 1.05f;

[Header("AI Behaviour")]
public float hideChance = 30f; // 30%
public float hideDuration = 2f;

private bool isHiding = false;
private bool canShoot = true;

public float shootDelayMin = 0.3f;
public float shootDelayMax = 1.2f;

private float nextShootTime;

public GameObject muzzleObject; // místo odkud se střílí
public GameObject muzzleFlashPrefab;
private GameObject generated_muzzle_flash;

public LineRenderer beam_lineRenderer;

public float beamLength = 30f;
public float beamDuration = 0.05f; // jak dlouho je paprsek vidět
public GameObject generated_beam;
public float enemy_height_y;
public string prefab_name = "hezbollahTerrorist_gun_animated 1";
public bool IsDeployedAtWall, IsNewlyGenerated;
public bool CanSeePlayer;
public bool CanHearPlayer;

public Vector3 lastKnownPlayerPosition;
public GameObject flankTargetWall;
public bool isFlanking = false;

public float lastTimeSawPlayer;

    enum EnemyState
    {
        Idle,
        MovingToPointA,
        MovingToPointB,
        Flanking,
        Shooting,
		FlankThroughWall
    }

    EnemyState currentState = EnemyState.Idle;

    Vector3 pointA;
    Vector3 pointB;
    Vector3 flankPoint;
	public List<GameObject> generatedTiles = new List<GameObject>();
    void Start()
    {
		enemy = GameObject.Find(this.name);
		
		if(this.name.Contains("(Clone)")) //"newborn" produced in GenerationHouse will be automatically activated
			IsNewlyGenerated = true;
			enemy_prefab = GameObject.Find(prefab_name);
		if(enemy_prefab!=null)
			enemy_height_y = enemy.transform.position.y;
		
        target_object = GameObject.Find("Merkava Mk_lowpoly");
		beam = GameObject.Find("lightBeam");
		nearest_wall_object = GameObject.Find("Main_wall");
		tile_object = GameObject.Find("enemy_tile");
		start_distance_enemy_wall = Vector3.Distance(enemy.transform.position, nearest_wall_object.transform.position);
		start_distance_tile_wall = Vector3.Distance(tile_object.transform.position, nearest_wall_object.transform.position);
		
		CreateTileRow(max_count_tiles, nearest_wall_object, tile_object.transform.position);
    }
	
	public Vector3 GetFlankPoint(GameObject wall)
{
    Vector3 wallPos = wall.transform.position;

    Vector3 dirToPlayer =
        (lastKnownPlayerPosition - wallPos).normalized;

    Vector3 side = Vector3.Cross(Vector3.up, dirToPlayer);

    float offset = 25f;

    // 3 body: bok → zadek → druhá strana
    Vector3 sidePoint = wallPos + side * offset;
    Vector3 backPoint = wallPos - dirToPlayer * offset;
    Vector3 finalPoint = lastKnownPlayerPosition + (-dirToPlayer * 10f);

    // uložíme první krok
    pointA = sidePoint;
    pointB = backPoint;
    flankPoint = finalPoint;

    return sidePoint;
}
	public int control_count_generated_tiles;
public void CreateTileRow(int count_of_tiles, GameObject selected_wall, Vector3 starting_tile_position)
{
    tile_gap = selected_wall.transform.GetComponent<Collider>().bounds.size.z / count_of_tiles;

    for (int i = 0; i < count_of_tiles; i++)
    {
        Vector3 new_tile_position = new Vector3(
            starting_tile_position.x,
            starting_tile_position.y,
            starting_tile_position.z - i * tile_gap
        );

        GameObject newTile = Instantiate(tile_object, new_tile_position, tile_object.transform.rotation);
        generatedTiles.Add(newTile);
    }
}
	
	void SendEnemyToRandomTileAfterShot()
{
    if (generatedTiles.Count == 0) return;

    GameObject randomTile = generatedTiles[Random.Range(0, generatedTiles.Count)];
    if (randomTile == null) return;

    Vector3 targetPos = randomTile.transform.position;

    // omezení: max 10 jednotek v X od aktuální pozice tile_object
    float deltaX = Mathf.Abs(targetPos.x - tile_object.transform.position.x);

    if (deltaX <= 10f)
    {
        StartCoroutine(MoveEnemyToPosition(targetPos));
    }
}

IEnumerator MoveEnemyToPosition(Vector3 targetPos)
{
    currentState = EnemyState.Idle;

    while (Vector3.Distance(transform.position, targetPos) > 0.5f)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        yield return null;
    }
}
public float nearest_building_distance;
public float CheckNearestBuildingDistance()
{
    GameObject[] buildings = GameObject.FindGameObjectsWithTag("building");

    float minDist = Mathf.Infinity;
    GameObject nearest = null;

    foreach (GameObject b in buildings)
    {
        float dist = Vector3.Distance(transform.position, b.transform.position);

        if (dist < minDist)
        {
            minDist = dist;
            nearest = b;
        }
    }

    // pokud nic nenajdeš
    if (nearest == null)
        return -1f;

    return minDist;
}
	//public bool IsDeployedAtWall;
	public float dist_player, angle_to_player, x_diff, z_diff;
	public float dist_to_be_activated = 120.0f;
	public float enemy_speed = 5.0f;
	public GameObject enemyDirector;
    void Update()
    {
		if(enemy.transform.position.y<terrain_height_y)//Plane y-axis
		enemy.transform.position = new Vector3(enemy.transform.position.x,terrain_height_y,enemy.transform.position.z);
		
		if(enemyDirector==null)
		{
			GameObject.Find("EnemyManager");
			IsDeployedAtWall = true;//these two unblocks movement, I don't know why
		}
        if (target_object == null)
		{			
			FindNearestPlayer();
			target_object = nearest_wall_object;
		}
		else
		{
			if(nearest_building_distance==0)
			nearest_building_distance = CheckNearestBuildingDistance();
			
			if(nearest_building_distance>20.0f && target_object!=null)
				enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target_object.transform.position, enemy_speed*Time.deltaTime);
		}
		
		if (!playerVisible && CanHearPlayer) //player je za zdi, prepneme na flanking rezim, kde ho enemy vyhledava, obejde zdi a vynori se za hracem
		{
		if (isFlanking == false)
		{
        GetFlankPoint(nearest_wall_object);//nejblizsi zed vedle hrace
        currentState = EnemyState.FlankThroughWall;
        isFlanking = true;
		}
		}
		if(Vector3.Distance(enemy.transform.position, target_object.transform.position)<dist_to_be_activated)
		{
			enemyDirector.transform.GetComponent<EnemyDirector>().IsBlockedSpawning = false;//while enemy appears in front of player, spawning is released for new one
			IsEnabledToShoot = true;
		}
		if(IsDeployedAtWall==true)
		{
        distance = Vector3.Distance(transform.position, target_object.transform.position);

        canSee = CheckLineOfSight();
		
		if(nearest_player_object!=null)
		{
			player = nearest_player_object;
			target_object = nearest_player_object;
			dist_player = Vector3.Distance(enemy.transform.position, nearest_player_object.transform.position);
			x_diff = Mathf.Abs(enemy.transform.position.x - nearest_player_object.transform.position.x);
			z_diff = Mathf.Abs(enemy.transform.position.z - nearest_player_object.transform.position.z);
			angle_to_player = Mathf.Asin(x_diff/dist_player)*180/Mathf.PI;
			if(enemy.transform.position.z>player.transform.position.z)
				enemy.transform.rotation = Quaternion.Euler(0,-angle_to_player,0);//nataceni k hraci
			else
				enemy.transform.rotation = Quaternion.Euler(0,angle_to_player,0);//nataceni k hraci
			
			
			IsEnabledToShoot = true;
		}
		if(nearest_player_object == null)
		{
			FindNearestPlayer();
		}
		else
		{
			target_object = nearest_player_object;
		currentDistanceToPlayer =
        Vector3.Distance(
            transform.position,
            nearest_player_object.transform.position
        );
		float release_beam_distance = 200.0f;
		if(CanHearPlayer==true || CanSeePlayer==true)
		{
		if(IsEnabledToShoot==true && beam!=null && muzzleFlashPrefab!=null && muzzleObject!=null && generated_beam==null)
		{
			Vector3 dir = (target_object.transform.position - muzzleObject.transform.position).normalized;

			generated_beam = Instantiate(
			beam,
			muzzleObject.transform.position,
			Quaternion.LookRotation(dir)
			);
			//this releases beam light towards player
			generated_beam.transform.GetComponent<lightBeam_travels_to_target>().target_name = nearest_player_object.name;
			generated_beam.transform.GetComponent<lightBeam_travels_to_target>().IsTriggered = true;
			if(generated_muzzle_flash==null)
			{
			generated_muzzle_flash = Instantiate(muzzleFlashPrefab, muzzleObject.transform.position, muzzleObject.transform.rotation);
			ParticleSystem ps = generated_muzzle_flash.transform.GetComponent<ParticleSystem>();
			ps.Play();
			PlayMuzzleAudioClip();
			}
		}
		else if(generated_muzzle_flash!=null)
		{
			if(Vector3.Distance(enemy.transform.position,generated_muzzle_flash.transform.position)>=release_beam_distance)
			{
				generated_beam = null;
			}
		}
		}

		playerVisible = CheckPlayerVisibility();

		// enemy vidi hrace + hrac je v dostrelu
		if(playerVisible && currentDistanceToPlayer <= shootRange)
		{
        SmoothTurnToPlayer();

        enemy.GetComponent<CombatGUI>().IsShootingToPlayer = true;

        IsEnabledToShoot = true;
		}
		else
		{
        enemy.GetComponent<CombatGUI>().IsShootingToPlayer = false;

        IsEnabledToShoot = false;
		}
		}
        // ===== HLAVNÍ ROZHODOVÁNÍ =====
        if (canSee && distance <= minimal_contact_distance)
        {
            IsEnabledToShoot = true;
            currentState = EnemyState.Shooting;
        }
        else
        {
            IsEnabledToShoot = false;

            if (currentState == EnemyState.Idle || currentState == EnemyState.Shooting)
            {
                RaycastHit hit;
                Vector3 dir = (target_object.transform.position - transform.position).normalized;

                if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, 500f))
                {
                    if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("building"))
                    {
                        CalculatePoints(hit);
                        currentState = EnemyState.MovingToPointA;
                    }
                    else
                    {
                        // nic nepřekáží → jdi flankovat
                        currentState = EnemyState.Flanking;
                    }
                }
            }
        }
		
		if(control_count_generated_tiles>=count_of_tiles)//send enemy to random tile
		SendEnemyToRandomTileAfterShot();

        // ===== STAVY =====
        switch (currentState)
        {
            case EnemyState.MovingToPointA:
                MoveTo(pointA);

                if (Vector3.Distance(transform.position, pointA) < 1f)
                    currentState = EnemyState.MovingToPointB;
                break;

            case EnemyState.MovingToPointB:
                MoveTo(pointB);

                if (Vector3.Distance(transform.position, pointB) < 1f)
                    currentState = EnemyState.Flanking;
                break;

            case EnemyState.Flanking:
                flankPoint = GetBestFlankPosition();
                MoveTo(flankPoint);

                if (CheckLineOfSight())
                    currentState = EnemyState.Shooting;
                break;

            case EnemyState.Shooting:
                RotateToTarget();
				if (IsEnabledToShoot)
				{
				TryShoot();
				}	
                // střelbu přidáš později
                break;
				
				case EnemyState.FlankThroughWall:

				MoveTo(pointA);

					if (Vector3.Distance(transform.position, pointA) < 2f)
					{
						MoveTo(pointB);
					}

					if (Vector3.Distance(transform.position, pointB) < 2f)
					{
					MoveTo(flankPoint);
					}

					if (Vector3.Distance(transform.position, flankPoint) < 3f)
					{
					isFlanking = false;
					currentState = EnemyState.Shooting;
					}

				break;
        }
		}//IsDeployedAtWall - je v konecne stanici, u zdi v utocne pozici ?
		else if(IsDeployedAtWall==false && IsNewlyGenerated==true)
		{
			//MoveToDeploymentPoint(nearest_wall_object);//old version
			/*
			here is new version
			*/
			if(dist_player>shootRange)
				MoveToFinalCoverPoint();
			else if(dist_player<=shootRange)
			{
				IsDeployedAtWall = true; IsShootingToPlayer = true; IsNewlyGenerated = false;
			}
			GameObject DeploymentNodeObject = nearest_wall_object;
			if(Vector3.Distance(enemy.transform.position,DeploymentNodeObject.transform.position)<=dist_to_be_activated)
			{
				IsDeployedAtWall = true;
				IsNewlyGenerated = false;
			}
		}
    }
	
	public void MoveToFinalCoverPoint()
	{
		if(finalCoverPoint != null)
{
    enemy.transform.position =
        Vector3.MoveTowards(
            enemy.transform.position,
            finalCoverPoint.position,
            enemy_speed * Time.deltaTime
        );

    Vector3 dir =
        finalCoverPoint.position -
        enemy.transform.position;
		
		Animator anim = enemy.transform.GetComponent<Animator>();
		animation_string = "hezbollah_terrorist_run";
		parameter_animation = "IsRunning";
		anim.SetInteger(parameter_animation,1);
		anim.Play(animation_string);

    dir.y = 0;

    if(dir != Vector3.zero)
    {
        Quaternion rot =
            Quaternion.LookRotation(dir);

        enemy.transform.rotation =
            Quaternion.Slerp(
                enemy.transform.rotation,
                rot,
                5.0f * Time.deltaTime
            );
    }

    if(Vector3.Distance(
        enemy.transform.position,
        finalCoverPoint.position) < 2.0f)
    {
        IsDeployedAtWall = true;
        IsNewlyGenerated = false;
    }
}
	}
	public float dist_from_birth_object;
	public float min_dist_to_start_the_way = 50.0f;//enemy starts his life journey
	public bool StartedLifeJourneyThroughCity;
	public float turn_angle = 90.0f;
	public bool IsDetectedBuildingInWay;
	void MoveToDeploymentPoint(GameObject AttackPositionObject)
{
    if(AttackPositionObject == null)
        return;

    Vector3 dir =
        (AttackPositionObject.transform.position -
        transform.position).normalized;

    
		
		if(nearest_Gen_House==null)
			FindNearestGenerationHouse();//this is where enemy is born
		else if(nearest_Gen_House!=null && StartedLifeJourneyThroughCity==false)
		{
			dist_from_birth_object = Vector3.Distance(nearest_Gen_House.transform.position, enemy.transform.position);
			if(dist_from_birth_object>=min_dist_to_start_the_way)//is 50 units right from borm house, lookin ?
			{
				StartedLifeJourneyThroughCity = true;
				enemy.transform.rotation = Quaternion.Euler(0,90,0);
			}
		}
		else if(StartedLifeJourneyThroughCity==true && IsDeployedAtWall==false)
		{
			float diff_x = Mathf.Abs(enemy.transform.position.x - AttackPositionObject.transform.position.x);
			float diff_z = Mathf.Abs(enemy.transform.position.z - AttackPositionObject.transform.position.z);
			if(diff_x>diff_z && IsDetectedBuildingInWay==false)
			{
				
				DetectbuildingInLine(1, nearest_wall_object);//detects building in the street, then go there
				turn_angle = 90.0f;
			}
			//enemy is not in the end but wander through the city
		}

    if(dir != Vector3.zero)
    {
        Quaternion rot =
            Quaternion.LookRotation(dir);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                rot,
                Time.deltaTime * 5f
            );
    }
}
public GameObject nearest_Gen_House, nearest_House;
public void FindNearestGenerationHouse()
{
	GameObject[] allGenerationHouses = GameObject.FindGameObjectsWithTag("GenerationHouse");
	
	float dist_from_nearest_house = Mathf.Infinity;
	
	foreach(GameObject go in allGenerationHouses)
	{
		if(Vector3.Distance(go.transform.position, enemy.transform.position)<dist_from_nearest_house)
		{
			dist_from_nearest_house = Vector3.Distance(go.transform.position, enemy.transform.position);
			nearest_Gen_House = go;
		}
	}
}
public GameObject v_nearest_House;
public bool IsBuildingHotBySideRaycast, IsBuildingHotByForewardRaycast, leftBlocked, rightBlocked, forwardBlocked;
public Vector3 dir;
public void DetectbuildingInLine(int case_number, GameObject FinalTargetObject)
{
	GameObject[] allHouses = GameObject.FindGameObjectsWithTag("building");
	
	float dist_from_nearest_house = Mathf.Infinity;
	float min_distance_to_stop_from_building = 10.0f;
	float speed = 25.0f;
	float minimal_x_difference_enemy_building = 5.0f;
	float last_mile_distance = 300.0f;
	foreach(GameObject go in allHouses)
	{
		float x_distance = Mathf.Abs(enemy.transform.position.x - go.transform.position.x);
		float z_distance = Mathf.Abs(enemy.transform.position.z - go.transform.position.z);
		float building_size_x = go.transform.GetComponent<Collider>().bounds.size.x;
		float building_size_z = go.transform.GetComponent<Collider>().bounds.size.z;
		if(z_distance<x_distance //horizontal movement
		&& Vector3.Distance(enemy.transform.position, go.transform.position) > 3 * building_size_x
		&& IsBuildingHotByForewardRaycast == true)
		{
			v_nearest_House = null;
			dist_from_nearest_house = Vector3.Distance(go.transform.position, enemy.transform.position);
			nearest_House = go;
			
		}
		else if(Vector3.Distance(enemy.transform.position, go.transform.position)<=min_distance_to_stop_from_building && IsBuildingHotBySideRaycast==true)
		{
			//find another building to continue
			if(x_distance<z_distance && x_distance<=minimal_x_difference_enemy_building)//so enemy and building has center in same line
			{
				if(Vector3.Distance(enemy.transform.position, go.transform.position)>3*building_size_z)//so there is long "vertical street"
				{
					//calculates shorter distance between two opposite buildings
					if(nearest_House!=null && FinalTargetObject!=null)
					if(Vector3.Distance(go.transform.position, FinalTargetObject.transform.position)<Vector3.Distance(nearest_House.transform.position, FinalTargetObject.transform.position))
					{
						
						nearest_House = null;
						v_nearest_House = go;//selects shorter distance
						
					}
				}
			}
		}
		//moving part
		if(v_nearest_House!=null && nearest_House==null)
		{
			//moves side to side, until gets to position of the "street", where is nearest building in front && in distance 3 times lenght of firstly detected building from enemy
		enemy.transform.position = Vector3.MoveTowards(
		enemy.transform.position,
		v_nearest_House.transform.position,
		speed * Time.deltaTime
		);
		RaycastForeward();//hits building in front, confirmed hit means navigation point
		if(Vector3.Distance(enemy.transform.position, v_nearest_House.transform.position)<=minimal_contact_distance)
		{
			
			v_nearest_House = null;
		}
		}
		else if(v_nearest_House==null && nearest_House!=null)
		{
		enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, nearest_House.transform.position, speed*Time.deltaTime);	//moves horizontally forward(backward), detects buildings on side, in case of abnormality(higher distance) detects side "street"
		RaycastSide();//raycast building on the side
		if(Vector3.Distance(enemy.transform.position, nearest_House.transform.position)<=minimal_contact_distance)
		{
			
			nearest_House = null;
		}
		}
		else if(Vector3.Distance(enemy.transform.position, FinalTargetObject.transform.position)<=last_mile_distance)
		{
			RaycastForeward();
			if(IsBuildingHotByForewardRaycast==false)//nothing detected in way
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, FinalTargetObject.transform.position, speed*Time.deltaTime);	//moves to end destination
		}
		else if(Vector3.Distance(enemy.transform.position, FinalTargetObject.transform.position)<=last_mile_distance
		&& Mathf.Abs(enemy.transform.position.z - FinalTargetObject.transform.position.z)<FinalTargetObject.transform.GetComponent<Collider>().bounds.size.z)//enemy stays in front of the target object
		{
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, FinalTargetObject.transform.position, speed*Time.deltaTime);	//moves to end destination
		}
		else if(Vector3.Distance(enemy.transform.position, FinalTargetObject.transform.position)<=last_mile_distance && IsBuildingHotByForewardRaycast==true)
		{
			AvoidBuildingAndContinue(FinalTargetObject);
		}
	}
}
public void AvoidBuildingAndContinue(GameObject FinalTargetObject)
{
    float avoidSpeed = 20.0f;

    // zkus nejdriv pravou stranu
    Vector3 avoidDirection = transform.right;
	
	if(leftBlocked)
    avoidDirection = transform.right;
	else
    avoidDirection = -transform.right;

    // pohyb do strany
    enemy.transform.position +=
        avoidDirection *
        avoidSpeed *
        Time.deltaTime;

    // kontrola jestli uz nic neni v ceste
    RaycastSide();
    RaycastForeward();

    // obe strany blokovane
    if(leftBlocked && rightBlocked)
    {
        // stop nebo couvni
        return;
    }

    // leva blokovana -> doprava
    if(leftBlocked && !rightBlocked)
    {
        avoidDirection = transform.right;
    }

    // prava blokovana -> doleva
    else if(rightBlocked && !leftBlocked)
    {
        avoidDirection = -transform.right;
    }

    // nic neblokuje -> random
    else
    {
        avoidDirection =
            (Random.value > 0.5f)
            ? transform.right
            : -transform.right;
    }

    enemy.transform.position +=
        avoidDirection *
        avoidSpeed *
        Time.deltaTime;

    // pokud uz nic neni pred enemy
    if(!forwardBlocked)
    {
        enemy.transform.position =
            Vector3.MoveTowards(
                enemy.transform.position,
                FinalTargetObject.transform.position,
                speed * Time.deltaTime
            );
    }

    // cesta je volna
    if(IsBuildingHotByForewardRaycast == false)
    {
        // reset obstacle modu
        IsBuildingHotByForewardRaycast = false;

        // pokracuj ke cilenemu objektu
        enemy.transform.position =
            Vector3.MoveTowards(
                enemy.transform.position,
                FinalTargetObject.transform.position,
                speed * Time.deltaTime
            );
    }
}
GameObject sideHitObject;
Vector3 sideHitPoint;

public void RaycastSide()
{
    Vector3 origin = transform.position + Vector3.up * 1.5f;
	


    // levá a pravá strana
    Vector3 leftDir = -transform.right;
    Vector3 rightDir = transform.right;
	leftBlocked =
    Physics.Raycast(origin, leftDir, 20f);

	rightBlocked =
    Physics.Raycast(origin, rightDir, 20f);
    RaycastHit hitLeft;
    RaycastHit hitRight;

    bool hitL = Physics.Raycast(origin, leftDir, out hitLeft, 500f);
    bool hitR = Physics.Raycast(origin, rightDir, out hitRight, 500f);

    GameObject chosen = null;

    if (hitL && hitR)
    {
        // vezme bližší hit
        chosen = (hitLeft.distance < hitRight.distance) ? hitLeft.collider.gameObject : hitRight.collider.gameObject;
    }
    else if (hitL)
    {
        chosen = hitLeft.collider.gameObject;
    }
    else if (hitR)
    {
        chosen = hitRight.collider.gameObject;
    }

    if (chosen != null && chosen.CompareTag("building"))
    {
        sideHitObject = chosen;
        IsBuildingHotBySideRaycast = true;

        Debug.DrawLine(origin, chosen.transform.position, Color.blue, 0.1f);
    }
    else
    {
        sideHitObject = null;
        IsBuildingHotBySideRaycast = false;
    }
	
	if(Physics.Raycast(origin, leftDir, out hitLeft, 10f))
    {
        if(
            hitLeft.collider.CompareTag("Enemy") ||
            hitLeft.collider.CompareTag("vehicle") ||
            hitLeft.collider.CompareTag("building") ||
            hitLeft.collider.CompareTag("Tank")
        )
        {
            leftBlocked = true;

            Debug.DrawRay(origin, leftDir * 10f, Color.red);
        }
    }

    // RIGHT
    if(Physics.Raycast(origin, rightDir, out hitRight, 10f))
    {
        if(
            hitRight.collider.CompareTag("Enemy") ||
            hitRight.collider.CompareTag("vehicle") ||
            hitRight.collider.CompareTag("building") ||
            hitRight.collider.CompareTag("Tank")
        )
        {
            rightBlocked = true;

            Debug.DrawRay(origin, rightDir * 10f, Color.blue);
        }
    }
}
GameObject forwardHitObject;
Vector3 forwardHitPoint;
public float raycastForwardDetectionRange = 500.0f;
public void RaycastForeward()
{
     Vector3 origin = transform.position + Vector3.up * 1.5f;

    RaycastHit hit;

    forwardBlocked = false;

    if(Physics.Raycast(origin, transform.forward, out hit, 15f))
    {
        if(
            hit.collider.CompareTag("Enemy") ||
            hit.collider.CompareTag("vehicle") ||
            hit.collider.CompareTag("building") ||
            hit.collider.CompareTag("Tank")
        )
        {
            forwardBlocked = true;

            Debug.DrawRay(origin, transform.forward * 15f, Color.yellow);
        }
    }

    if (Physics.Raycast(origin, dir, out hit, raycastForwardDetectionRange))
    {
        forwardHitObject = hit.collider.gameObject;
        forwardHitPoint = hit.point;

        // pokud je to budova
        if (hit.collider.CompareTag("building"))
        {
            IsBuildingHotByForewardRaycast = true;
        }
        else
        {
            IsBuildingHotByForewardRaycast = false;
        }

        Debug.DrawLine(origin, hit.point, Color.red, 0.1f);
    }
    else
    {
        forwardHitObject = null;
        IsBuildingHotByForewardRaycast = false;

        Debug.DrawLine(origin, origin + dir * raycastForwardDetectionRange, Color.green, 0.1f);
    }
}
	void PlayMuzzleAudioClip()
{
    if(audioSource == null)
    {
        Debug.LogWarning("AudioSource missing!");
        return;
    }

    if(shootClip == null)
    {
        Debug.LogWarning("ShootClip missing!");
        return;
    }

    // lehka variace zvuku
    audioSource.pitch =
        Random.Range(minPitch, maxPitch);

    audioSource.PlayOneShot(shootClip);
}
	
	void FindNearestPlayer()
{
    GameObject[] players =
        GameObject.FindGameObjectsWithTag("Player");

    if(players.Length == 0)
    {
        nearest_player_object = null;
        return;
    }

    float closestDistance = Mathf.Infinity;

    GameObject closestPlayer = null;

    foreach(GameObject player in players)
    {
        if(player == null)
            continue;

        float dist =
            Vector3.Distance(
                transform.position,
                player.transform.position
            );

        // ignoruje mrtve / deaktivovane hrace
        if(!player.activeInHierarchy)
            continue;

        if(dist < closestDistance)
        {
            closestDistance = dist;
            closestPlayer = player;
        }
    }

    nearest_player_object = closestPlayer;
}
	
	

void SmoothTurnToPlayer()
{
    if(nearest_player_object == null)
        return;

    Vector3 dir =
        nearest_player_object.transform.position -
        transform.position;

    dir.y = 0f;

    if(dir != Vector3.zero)
    {
        Quaternion targetRot =
            Quaternion.LookRotation(dir);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
    }
}
void ShootToTarget()
{
    if (muzzleObject == null || target_object == null) return;
	
	if(!playerVisible)
    return;

	if(currentDistanceToPlayer > shootRange)
    return;

	/*if(Time.time > lastShootTime + shootCooldown)
    {
        ShootToTarget();
        lastShootTime = Time.time;
    }*/

    // ===== MUZZLE FLASH =====
    if (muzzleFlashPrefab != null)
    {
		Quaternion rot = muzzleObject.transform.rotation * Quaternion.Euler(0, 180+90, 0);
        generated_muzzle_flash = Instantiate(
            muzzleFlashPrefab,
            muzzleObject.transform.position,
            rot
        );

        ParticleSystem ps = generated_muzzle_flash.GetComponent<ParticleSystem>();
        if (ps != null) ps.Play();
		
		if(beam!=null && generated_beam==null)
		{
			generated_beam = Instantiate(beam, muzzleObject.transform.position, muzzleObject.transform.rotation);
		}
		
		SendEnemyToRandomTileAfterShot();

        Destroy(generated_muzzle_flash, 0.2f); // necháme chvíli existovat
    }

    // ===== ZVUK =====
    if (audioSource != null && shootClip != null)
    {
        audioSource.PlayOneShot(shootClip);
    }

    // ===== SMĚR =====
    Vector3 origin = muzzleObject.transform.position;
    Vector3 direction = (target_object.transform.position - origin).normalized;

    RaycastHit hit;
    Vector3 endPoint;

    // ===== RAYCAST (zásah) =====
    if (Physics.Raycast(origin, direction, out hit, beamLength))
    {
        endPoint = hit.point;

        // tady později můžeš řešit damage:
        // if(hit.collider.CompareTag("Player")) ...
    }
    else
    {
        endPoint = origin + direction * beamLength;
    }
	float hit_distance = 20.0f;
	if(generated_beam!=null)
	{
		generated_beam.transform.Translate(0,0,1.0f);//towards target, not changing direction
		
		if(Vector3.Distance(generated_beam.transform.position,target_object.transform.position)<=hit_distance)
			Destroy(generated_beam);
	}

    // ===== LINE RENDERER (beam) =====
    if (beam_lineRenderer != null)
    {
        beam_lineRenderer.enabled = true;

        beam_lineRenderer.SetPosition(0, origin);
        beam_lineRenderer.SetPosition(1, endPoint);

        StopAllCoroutines();
        StartCoroutine(DisableBeam());
    }
}

IEnumerator DisableBeam()
{
    yield return new WaitForSeconds(beamDuration);

    if (beam_lineRenderer != null)
    {
        beam_lineRenderer.enabled = false;
    }
}

float shootCooldown = 0.5f;
float lastShootTime;

void TryShoot()
{
    if(isHiding)
        return;

    if(!canShoot)
        return;

    if(!playerVisible)
        return;

    if(currentDistanceToPlayer > shootRange)
        return;

    if(Time.time >= nextShootTime)
    {
        ShootToTarget();

        // dalsi nahodny delay
        nextShootTime =
            Time.time +
            Random.Range(shootDelayMin, shootDelayMax);

        // sance na schovani
        float randomValue = Random.Range(0f, 100f);

        if(randomValue <= hideChance)
        {
            StartCoroutine(HideRoutine());
        }
    }
}

IEnumerator HideRoutine()
{
    isHiding = true;

    currentState = EnemyState.Idle;

    // presun za cover
    SendEnemyToRandomTileAfterShot();

    // docasne zakaz strelby
    canShoot = false;

    yield return new WaitForSeconds(hideDuration);

    canShoot = true;

    isHiding = false;

    currentState = EnemyState.Shooting;
}

    // ===== LINE OF SIGHT =====
    bool CheckLineOfSight()
    {
        Vector3 dir = (target_object.transform.position - transform.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir, out hit, 500f))
        {
            return hit.collider.gameObject == target_object;
        }

        return false;
    }
	
	bool CheckPlayerVisibility()
{
    if(nearest_player_object == null)
        return false;

    Vector3 origin =
        transform.position + Vector3.up * 1.5f;

    Vector3 targetPos =
        nearest_player_object.transform.position + Vector3.up;

    Vector3 dir = (targetPos - origin).normalized;

    RaycastHit hit;

    if(
        Physics.Raycast(
            origin,
            dir,
            out hit,
            shootRange,
            visibilityMask
        )
    )
    {
        if(hit.collider.gameObject == nearest_player_object)
        {
            Debug.DrawRay(origin, dir * shootRange, Color.red);

            return true;
        }
    }

    Debug.DrawRay(origin, dir * shootRange, Color.yellow);

    return false;
}

    // ===== FLANKING LOGIKA =====
    Vector3 GetBestFlankPosition()
    {
        Vector3 tankPos = target_object.transform.position;
        Vector3 forward = target_object.transform.forward;

        // boky tanku
        Vector3 left = Vector3.Cross(Vector3.up, forward).normalized;
        Vector3 right = -left;

        float desiredDistance = minimal_contact_distance;

        Vector3 posLeft = tankPos + left * desiredDistance;
        Vector3 posRight = tankPos + right * desiredDistance;

        float scoreLeft = GetSideScore(posLeft);
        float scoreRight = GetSideScore(posRight);

        return (scoreLeft < scoreRight) ? posLeft : posRight;
    }

    float GetSideScore(Vector3 point)
    {
        Vector3 toEnemy = (point - target_object.transform.position).normalized;
        Vector3 tankForward = target_object.transform.forward;

        float dot = Vector3.Dot(tankForward, toEnemy);
        return Mathf.Abs(dot);
    }

    // ===== OBCHÁZENÍ ZDI =====
    void CalculatePoints(RaycastHit hit)
    {
        Vector3 wallNormal = hit.normal;

        Vector3 side1 = Vector3.Cross(Vector3.up, wallNormal).normalized;
        Vector3 side2 = -side1;

        float offset = 15f;

        Vector3 candidateA1 = hit.point + side1 * offset;
        Vector3 candidateA2 = hit.point + side2 * offset;

        float score1 = GetSideScore(candidateA1);
        float score2 = GetSideScore(candidateA2);

        Vector3 chosenSide = (score1 < score2) ? side1 : side2;

        pointA = hit.point + chosenSide * offset;

        Vector3 dirToTarget = (target_object.transform.position - pointA).normalized;
        pointB = pointA + dirToTarget * offset;
    }

    // ===== POHYB =====
    void MoveTo(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
      //  transform.position += dir * speed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    void RotateToTarget()
    {
        Vector3 dir = target_object.transform.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }
}