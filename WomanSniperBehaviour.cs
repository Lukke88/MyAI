using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WomanSniperBehaviour : MonoBehaviour
{
    public GameObject player,generatedWeapon, WeaponPrefab, weaponHolder, firePoint, muzzlePrefab, muzzleGenerated, prefabBeam, barrel_end, obstruction_object;
    public float raycast_distance = 2000.0f;
	public GameObject[] allWalls;
    public Vector3 hit_point;
    public string enemyName = "";
    public bool showHitText = false;
    private float showTimer = 0f;
	public float moveSpeed = 85f;
	public Animator animator;
	public bool IsObstructionDetected, IsInMovement, IsShooting, IsIdle;
	private Vector3 destinationPoint;
	private bool hasDestination = false;
	public bool IsPlayerActivated;
	//3 animace pro 3 tlacitka
	public string running_animation_name = "FastRun (1)";
	public string crouch_animation_name = "Crouching Idle";
	public string proning_animation_name = "Prone Forward (1)";
	public string crouching_animation_name;
	public string muzzle_prefab_name = "WFX_MF 4P RIFLE1";
	public bool IsRunning;
	private float lastClickTime = 0f;
	public Button btnStand, btnCrouch, btnProne; //prone je Crawl
	private float doubleClickThreshold = 0.3f;
	public Vector3 enemyPosition;
	public GameObject moveMarker;//pro marker nasledujici hrace
	public float activationDistance = 10f;
	
	public WomanSniperAnimState currentState;
	public WomanSniperAnimParam currentParameter;
	public StaturesEnum currentStature;
	public enum WomanSniperAnimState
{
    Idle,
    HappyIdle,
    Gunplay4,
    Gunplay5,
    Shooting,
    FastRun1,
    RunLeft,
    RunRight,
	Crouching,
	Proning
}
public enum StaturesEnum
{
	Standing,
	Crouching,
	Proning
}

public enum StanceState
{
    Standing,
    Crouching,
    Proning
}
public enum FireStance
{
    StandingFire,
    CrouchFire,
    ProneFire
}
public bool IsHidden;
public StanceState currentStance;
public FireStance currentFireStance;

public GameObject coverTilePrefab;//6 pozic podel zdi
public GameObject[] coverTiles =
    new GameObject[6];

public Vector3[] coverPositions =
    new Vector3[6];
//we don't need to look for names in engine, all names used are here, can be changed accordingly to changed environment and type of player
public string standingAnim = "FastRun";
public string crouchAnim = "CrouchWalk";
public string proneAnim = "IsProningForward";
public string IdleAnimation = "HappyIdle";
public string standingFireAnim = "GunPlay";
public string crouchFireAnim = "SniperGirlCrouchFire";
public string proneFireAnim = "ProneShoot";
public string HidingAnimation = "CrouchingIdleBehindWall";
public string ProneToCrouchAnimation = "ProneToCrouch";
public string ForwardHit = "FallForwardHit";
public string RibHit = "RibHit";
public string RifleHit = "RifleHit";
public string GetUpAnimation = "GettingUp";
public Vector3 markerBaseScale;
public string moveMarkerName = "MarkerParent";
public string groundName = "Plane";
public string running_anim_parameter = "IsRunning";
//preview tile variables
public Vector3 coverPoint, hitPoint, playerPos;
public float offset;
private GameObject activeCoverTile, wall;
private Vector3 currentCoverPoint;

private enum PlayerIntent
{
    None,
    Move,
    Shoot
}

private PlayerIntent currentIntent = PlayerIntent.None;
private GameObject currentTarget;
public enum WomanSniperAnimParam
{
    IsIdle,
    IsRunning,
    IsShooting
}

    void Start()
    {
        player = GameObject.Find(this.name);
		animator = GetComponent<Animator>();
		IsObstructionDetected = false;
		allWalls = GameObject.FindGameObjectsWithTag("Wall");
		crouching_animation_name = crouch_animation_name;
		if(moveMarker==null)
		moveMarker= GameObject.Find(moveMarkerName);
        markerBaseScale = moveMarker.transform.localScale;
		if(IsPlayerActivated==true)
		{
			ActivateButtons();
		}
		else
		{
			DeactivateButtons();
		}
    }
	public bool IsHitPointNear;
	public float hit_point_distance;
	public Vector3 hit_point2;
	public float activation_distance = 45.0f;//activation distance of the cursor from player to be activated on double click
	void Update()
    {
		
		




raycast_distance = 2000;
	Ray ray2 =
        Camera.main.ScreenPointToRay(
            Input.mousePosition);

    RaycastHit hit2;

    if(Physics.Raycast(
        ray2,
        out hit2,
        raycast_distance))
		{
			hit_point2 = hit2.point;
		}
		if(player!=null)//basic player activation
		hit_point_distance = Vector3.Distance(hit_point2, player.transform.position);
		else
			player = GameObject.Find(this.name);
		if(Input.GetMouseButtonDown(0) && hit_point_distance<=activation_distance)
		{
			IsPlayerActivated = true;
		}
		else if(Input.GetMouseButtonDown(1))
			IsPlayerActivated = false;
			
if(IsPlayerActivated)
{
	GameObject plane = GameObject.Find(groundName);
	float plane_height_y = plane.transform.position.y + 5.0f;
	moveMarker = GameObject.Find(moveMarkerName);
	MainMovementLoop();

	if(moveMarker!=null)
		moveMarker.transform.position = new Vector3(player.transform.position.x, plane_height_y, player.transform.position.z);
        ActivateButtons();
		
		//creates tile near to wall, to where player will be navigated
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
RaycastHit hit;

if (Physics.Raycast(ray, out hit, raycast_distance))
{
    if (hit.collider.CompareTag("Wall"))
    {
        Vector3 wallPoint = hit.point;
        Vector3 playerPos = player.transform.position;

        float distanceToPlayerSide =
            Vector3.Distance(playerPos, hit.collider.ClosestPoint(playerPos));

        if (distanceToPlayerSide < 25f) // “mouse near wall from player side”
        {
            GenerateCoverPreview(hit.collider.gameObject, wallPoint);//preview tile at wall
        }
    }
}

if (Input.GetMouseButtonDown(1))//set new tile as destination point for player figure
{
    if (currentCoverPoint != Vector3.zero)
    {
        destinationPoint = currentCoverPoint;

        IsInMovement = true;
        IsShooting = false;

        currentIntent = PlayerIntent.Move; // pokud máš intent systém
    }
}

Vector3 directionToWall = (wall.transform.position - playerPos);
directionToWall.y = 0f;

coverPoint = hitPoint + directionToWall.normalized * offset;//cover point position
}
    else if(IsPlayerActivated==false)
	{
        DeactivateButtons();
		IsShooting = false;
		animator.Play(IdleAnimation);
	}
	
	if(Input.GetKeyDown(KeyCode.H))//schovani za prekazku
	{
    animator.Play(HidingAnimation);

    currentStance =
        StanceState.Crouching;

    IsInMovement = false;
	}
	
	if (currentIntent == PlayerIntent.Shoot && currentTarget != null)//lock to target destination
{
    Vector3 dir = currentTarget.transform.position - player.transform.position;
    dir.y = 0f;

    player.transform.rotation = Quaternion.Slerp(
        player.transform.rotation,
        Quaternion.LookRotation(dir),
        10f * Time.deltaTime
    );
}


//activate player by right click near character
if (Input.GetMouseButtonDown(1))
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, raycast_distance))
    {
        if (hit.collider.CompareTag("Plane"))
        {
            float dist = Vector3.Distance(hit.point, player.transform.position);

            if (dist <= activationDistance)
            {
                SetPlayerActive(true);

                MoveMarkerToPlayer();
            }
        }
    }
}
/*
else if(IsPlayerActivated==true && Input.GetMouseButtonDown(0))//levym tlacitkem deaktivace hrace
    {
        IsPlayerActivated = false;
    }*/


		
    }//end of Update()
	
	public void MainMovementLoop()
	{
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		float animTime = stateInfo.normalizedTime % 1f;
		float locomotionMultiplier = 1f;
float min_step_multiplier = 0.01f;
float max_step_multiplier = 1.35f;
		float speedMultiplier = 1f;

switch(currentStance)
{
    case StanceState.Standing:
        speedMultiplier = 1f;
        break;

    case StanceState.Crouching:
        speedMultiplier = 0.6f;
        break;

    case StanceState.Proning:
        speedMultiplier = 0.3f;
        break;
}
// levá noha dopad
if (animTime < 0.01f) //managing the speed of movement during phases of movement
{
    locomotionMultiplier = min_step_multiplier; //prekrok
}
// přenos váhy
else if (animTime >= 0.01f && animTime < 0.06f)
{
    locomotionMultiplier = max_step_multiplier; //preskok z nohy na nohu
}
// další kontakt
else if (animTime >= 0.06f && animTime < 0.11f)
{
    locomotionMultiplier = min_step_multiplier; //druhy dopad a prekrok
}
// druhý přenos
else if (animTime >= 0.11f && animTime < 0.84f)
{
    locomotionMultiplier = max_step_multiplier; //druhy preskok z nohy na nohu
}
// dopad druhé nohy
else if (animTime >= 0.84f && animTime < 0.988f)
{
    locomotionMultiplier = min_step_multiplier; //dopad na druhou nohu
}
// odraz
else
{
    locomotionMultiplier = max_step_multiplier; //odraz
}
		if(IsPlayerActivated==true)//activate player(by clicking nearby), then can start moving
		{
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycast_distance))
		{
    hit_point = hit.point;

    if (hit.collider.CompareTag("Enemy"))
    {
        currentIntent = PlayerIntent.Shoot;
        currentTarget = hit.collider.gameObject;

        IsShooting = true;
        IsInMovement = false;
    }
    else if (hit.collider.CompareTag("Plane") || hit.collider.CompareTag("CoverTile"))
    {
        currentIntent = PlayerIntent.Move;
        currentTarget = null;

        destinationPoint = hit.point + hit.normal * 20.0f;

        IsInMovement = true;
        IsShooting = false;
    }
	}
		if(destinationPoint!=Vector3.zero && Vector3.Distance(player.transform.position, destinationPoint)>activation_distance)
		{
			IsShooting = false;
			IsInMovement = true;
		}
		HandleMovement(); //this handles movement and sets up the idle state
		HandleDoubleClickMove(hit); //this handles double click
		if(generatedWeapon!=null)//then can shoot
		{
			 barrel_end = generatedWeapon.transform.GetChild(1).gameObject;//end of the weapon
		if(
		(Input.GetKeyDown(KeyCode.Return) ||
			Input.GetKeyDown(KeyCode.Space))
				&& IsShooting == true
				)//shooting loop
		{
			UpdateFireStance();
			if(prefabBeam!=null && barrel_end!=null)
				ShootTheBeamToEnemy(prefabBeam, barrel_end);//shooting function
		}
		}
		
		if (currentIntent == PlayerIntent.Move && destinationPoint != Vector3.zero)
		if (destinationPoint != Vector3.zero && IsObstructionDetected == false)
		{
	if(Vector3.Distance(destinationPoint, player.transform.position)>5.0f)
	{
		IsShooting = false;
		IsInMovement = true;
		IsRunning = true;
		
		animator = player.transform.GetComponent<Animator>();
		animator.SetInteger(running_anim_parameter,1);
		animator.Play(running_animation_name);
	}
	else
	{
		IsInMovement = false;
		IsRunning = false;
		//IsIdle = true nebo IsShooting = true, podle nastaveni stavu
	}
	if(IsShooting==false && IsInMovement==true)
		{
			ExitShootingMode();
		}
    ResetAnimatorParams();
	//OnAnimatorMove();//blokuje z osu
	animator.SetFloat("Speed", moveSpeed);
  //here are made decisions based on press of the button.
  
	if(currentStature == StaturesEnum.Crouching)
		currentState = WomanSniperAnimState.Crouching;
	else if(currentStature == StaturesEnum.Proning)
		currentState = WomanSniperAnimState.Proning;
	else
		 currentState = WomanSniperAnimState.FastRun1;
	 
    currentParameter = WomanSniperAnimParam.IsRunning;
    IsInMovement = true;

    animator.SetInteger("IsRunning", 1);
  //  animator.Play("FastRun (1)");

    // 🔥 NOVÉ: otočení směrem k cíli
    Vector3 direction = destinationPoint - player.transform.position;
    direction.y = 0f;

    if (direction.sqrMagnitude > 0.5f)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            targetRotation,
            12f * Time.deltaTime
        );
    }

    // pohyb
    Vector3 targetDir = (destinationPoint - transform.position).normalized;
	player.transform.position += 
    targetDir * 
    moveSpeed * 
    locomotionMultiplier * 
    Time.deltaTime;

    // ❌ oprava chyby (máš tam broken výraz)
    if (Vector3.Distance(player.transform.position, destinationPoint) <= 1.5f)
    {
        ResetAnimatorParams();
        currentState = WomanSniperAnimState.Idle;
        currentParameter = WomanSniperAnimParam.IsIdle;

        SetAnimState(currentState);
		
		currentIntent = PlayerIntent.None;
		IsInMovement = false;
		destinationPoint = Vector3.zero;
    }
	IsObstructionDetected = CheckObstructionsInWay(20.0f);//finds out if something is blockint the way
	}
		else if(destinationPoint!=Vector3.zero && IsObstructionDetected==true)//if yes, then continues to overcome obstruction
		{
			AvoidObstruction();
		}
		else if(destinationPoint==Vector3.zero)
		{
			//idle state
			IsInMovement = false;
		}
		
        if (Input.GetKeyDown(KeyCode.LeftControl) && hit_point != Vector3.zero)
        {
            // otočení směrem k hit pointu (jen Y osa kvůli sniperce)
            Vector3 direction = hit_point - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(direction);
                transform.rotation = rot;
            }

            // GUI text pokud byl zásah enemy
            if (!string.IsNullOrEmpty(enemyName))
            {
                showHitText = true;
                showTimer = 2f; // zobraz 2 sekundy
            }
			
			  // přepnutí animace podle "aim"
			SetAnimState(Random.value > 0.5f 
				? WomanSniperAnimState.Gunplay4 
				: WomanSniperAnimState.Gunplay5);
        }

        if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0)
            {
                showHitText = false;
                enemyName = "";
            }
        }
		
			if(moveMarker != null)
{
    Camera cam = Camera.main;

    float distance =
        Vector3.Distance(
            cam.transform.position,
            moveMarker.transform.position);

    float scaleFactor =
        distance * 0.003f; // ladicí hodnota

    moveMarker.transform.localScale =
        Vector3.one * scaleFactor;
}
	}//player activated loop
	
	ResolveFinalState();//switches final states for the case of conflict
	}
	
	void LateUpdate()
{
    if (IsPlayerActivated && moveMarker != null)
    {
        Vector3 followPos = player.transform.position;
        followPos.y = moveMarker.transform.position.y;

        moveMarker.transform.position = followPos;
    }
}
//generates preview tile at wall for cover position of player
void GenerateCoverPreview(GameObject wall, Vector3 hitPoint)
{
    Collider col = wall.GetComponent<Collider>();
    Bounds b = col.bounds;

    Vector3 playerPos = player.transform.position;

    bool isLongX = b.size.x > b.size.z;

    Vector3 normal = (hitPoint - wall.transform.position).normalized;
    normal.y = 0f;

    Vector3 coverPoint = hitPoint;

    float offset = 20.0f;

    // 🔥 rozhodnutí směru offsetu (směr ke hráči)
    Vector3 toPlayer = (playerPos - hitPoint).normalized;
    toPlayer.y = 0f;

    if (isLongX)
    {
        // zeď je “dlouhá po X”
        coverPoint += new Vector3(
            toPlayer.x * offset,
            0f,
            0f
        );
    }
    else
    {
        // zeď je “dlouhá po Z”
        coverPoint += new Vector3(
            0f,
            0f,
            toPlayer.z * offset
        );
    }

    currentCoverPoint = coverPoint;

    // 🔥 spawn / move tile
    if (activeCoverTile == null)
    {
        activeCoverTile = Instantiate(coverTilePrefab, coverPoint, Quaternion.identity);
    }
    else
    {
        activeCoverTile.transform.position = coverPoint;
    }
}

public void ReceiveHit()//zasah hrace
{
    animator.Play(
        Random.value > 0.5f ?
        RibHit :
        RifleHit
    );
}
//generovani pozic podel zdi
public void GenerateCoverPoints(
    GameObject wall)
{
    Collider col =
        wall.GetComponent<Collider>();

    float xSize =
        col.bounds.size.x;

    float zSize =
        col.bounds.size.z;

    Vector3 center =
        wall.transform.position;

    bool wallAlongX =
        xSize > zSize;
		
		if(wallAlongX)//kdyz je zed delsi v x-se nez z-ose
	{
    float step = xSize / 5f;

    for(int i=0;i<6;i++)//generuje 6 cover points rovnobezne s x-osou, offset je posunuty v z-ose
    {
        coverPositions[i] =
            new Vector3(
                center.x -
                xSize/2f +
                step*i,

                player.transform.position.y,

                center.z + 10f
            );
    }
	}
	else //jinak je generuje za z-osou, x je offset
	{
    float step = zSize / 5f;

    for(int i=0;i<6;i++)
    {
        coverPositions[i] =
            new Vector3(
                center.x + 10f,

                player.transform.position.y,

                center.z -
                zSize/2f +
                step*i
            );
    }
	//dvojklik, oznaceni dlazdice jako destination pointu
	if(Time.time-lastClickTime
    < doubleClickThreshold)
{
    Ray ray =
        Camera.main.ScreenPointToRay(
            Input.mousePosition);

    RaycastHit hit;

    if(Physics.Raycast(
        ray,
        out hit,
        raycast_distance))
    {
        if(hit.collider.CompareTag(
            "CoverTile"))
        {
            destinationPoint =
                hit.collider.transform.position;
        }
    }
}
enemyPosition =
    FindNearestEnemyPosition();

Vector3 dir =
    enemyPosition -
    player.transform.position;

dir.y = 0f;

if(dir != Vector3.zero)
{
    player.transform.rotation =
        Quaternion.LookRotation(dir);
}
if(//dobehnuti k dlazdici
Vector3.Distance(
    player.transform.position,
    destinationPoint)
    < 1.5f)
	{
		animator.Play(
		HidingAnimation);

	currentStance =
    StanceState.Crouching;

	IsInMovement = false;
	transform.LookAt(enemyPosition);
	}
	}
	
	for(int i=0;i<6;i++)//vytvoreni dlazdic u zdi
{
    coverTiles[i] =
        Instantiate(
            coverTilePrefab,
            coverPositions[i],
            Quaternion.identity
        );
}
}
public Vector3 FindNearestEnemyPosition()
{
    GameObject[] enemies =
        GameObject.FindGameObjectsWithTag("Enemy");

    float nearestDistance = Mathf.Infinity;
    Vector3 nearestPosition = Vector3.zero;

    foreach(GameObject enemy in enemies)
    {
        if(enemy == null)
            continue;

        float distance =
            Vector3.Distance(
                player.transform.position,
                enemy.transform.position);

        if(distance < nearestDistance)
        {
            nearestDistance = distance;
            nearestPosition =
                enemy.transform.position;
        }
    }

    enemyPosition = nearestPosition;

    return nearestPosition;
}
public void SetStanding()
{
    currentStance = StanceState.Standing;
    animator.Play(standingAnim);
}

public void SetCrouch()
{
    currentStance = StanceState.Crouching;
    animator.Play(crouchAnim);
}

public void SetProne()
{
    currentStance = StanceState.Proning;
    animator.Play(proneAnim);
}

public void UpdateFireStance()
{
    switch(currentStance)
    {
        case StanceState.Standing:
            currentFireStance = FireStance.StandingFire;
            animator.Play(standingFireAnim);
            break;

        case StanceState.Crouching:
            currentFireStance = FireStance.CrouchFire;
            animator.Play(crouchFireAnim);
            break;

        case StanceState.Proning:
            currentFireStance = FireStance.ProneFire;
            animator.Play(proneFireAnim);
            break;
    }
}
void OnAnimatorMove()
{
    Vector3 delta = animator.deltaPosition;

    // ❌ zablokuješ Z pohyb z animace
    delta.z = 0f;

    transform.position += delta;
}

void HideMarker()
{
    if (moveMarker == null)
        return;

    Vector3 hidden = moveMarker.transform.position;
    hidden.y = -100f; // “pod zem”

    moveMarker.transform.position = hidden;
    moveMarker.SetActive(false);
}
	
	public void ActivateButtons()
	{
		btnStand.interactable = true;
		btnCrouch.interactable = true;
		btnProne.interactable = true;
	}
	
	public void DeactivateButtons()
	{
		btnStand.interactable = false;
		btnCrouch.interactable = false;
		btnProne.interactable = false;
	}
	
	public void SetPlayerActive(bool active)
{
     IsPlayerActivated = active;

    if (active)
    {
        IsInMovement = false;
    }
    else
    {
        HideMarker();
    }

    btnStand.interactable = active;
    btnCrouch.interactable = active;
    btnProne.interactable = active;
}

void MoveMarkerToPlayer()
{
    if (moveMarker == null)
        return;

    moveMarker.transform.position = player.transform.position;
    moveMarker.SetActive(true);
}

void ResolveFinalState()
{
    if (IsShooting)
    {
        SetAnimState(WomanSniperAnimState.Gunplay4);
        return;
    }

    if (IsInMovement)
    {
        SetAnimState(WomanSniperAnimState.FastRun1);
        return;
    }

    SetAnimState(WomanSniperAnimState.Idle);
}
void ExitShootingMode()
{
    IsShooting = false;
    IsInMovement = true;

    ResetAnimatorParams();

    animator.SetInteger("IsShooting", 0);
    animator.SetInteger("IsRunning", 1);

    SetAnimState(WomanSniperAnimState.FastRun1);
	
	if (muzzleGenerated != null)
{
    var ps = muzzleGenerated.GetComponent<ParticleSystem>();
    if (ps != null)
    {
        if (IsShooting)
            ps.Play();
        else
            ps.Stop();
    }
}
}
    
	
	//button functions


public bool CheckObstructionsInWay(float detection_distance)
{
    bool isObstructed = false;

    RaycastHit hit;

    // směr dopředu od hráče
    Vector3 origin = player.transform.position;
    Vector3 direction = player.transform.forward;

    if (Physics.Raycast(origin, direction, out hit, detection_distance))
    {
        if (hit.collider.CompareTag("Wall") || 
            hit.collider.CompareTag("Building"))
        {
            isObstructed = true;
            obstruction_object = hit.collider.gameObject;
        }
    }

    return isObstructed;
}
	public void StandUp()
	{
		currentStature = StaturesEnum.Standing;
	}
	public void CrouchUp()
	{
		currentStature = StaturesEnum.Crouching;
	}
	public void ProneDown()
	{
		currentStature = StaturesEnum.Proning;
	}
	//button functions
	public Vector3 PointA, PointB;
	public float x_distance, z_distance, x_size, z_size;
	public bool passA, passB, passedObstruction;
	public void AvoidObstruction()
	{
		float movement_speed = 25.0f;
		float x_offset = 5.0f;
		float z_offset = 5.0f;
		if(obstruction_object!=null && destinationPoint!=Vector3.zero)
		{
			x_distance = Mathf.Abs(player.transform.position.x - obstruction_object.transform.position.x);
			z_distance = Mathf.Abs(player.transform.position.z - obstruction_object.transform.position.z);
			if(PointA==Vector3.zero || PointB==Vector3.zero)
			{
			if(x_distance>z_distance)//jde z vychodu na zapad ci obracene
			{
				x_size = obstruction_object.transform.GetComponent<Collider>().bounds.size.x;
				z_size = obstruction_object.transform.GetComponent<Collider>().bounds.size.z;
				int a = 1;
				if(player.transform.position.z>obstruction_object.transform.position.z)
				{
					a = -1; //a changes order, from left to right side
				}
				if(z_size>x_size)//zed je otocena o 90 stupnu k hraci
				{
					float p_x = obstruction_object.transform.position.x - (x_size + x_offset)*a;
					float p_y = player.transform.position.y;
					float p_z = obstruction_object.transform.position.z + z_size/2 + z_offset;
					PointA = new Vector3(p_x, p_y, p_z);
					
					 p_x = obstruction_object.transform.position.x + (x_size + x_offset)*a;
					 p_y = player.transform.position.y;
					 p_z = obstruction_object.transform.position.z + z_size/2 + z_offset;
					PointB = new Vector3(p_x, p_y, p_z);
				}
			}
			else if(z_distance>x_distance)//jde z jihu ke zdi nebo ze severu
			{
				x_size = obstruction_object.transform.GetComponent<Collider>().bounds.size.x;
				z_size = obstruction_object.transform.GetComponent<Collider>().bounds.size.z;
				int a = 1;
				if(player.transform.position.z>obstruction_object.transform.position.z)
				{
					a = -1; //a changes order, from top to bottom side PointA becomes PointB etc.
				}
				if(x_size>z_size)//zed je otocena o 90 stupnu k hraci, je nad ci pod nim
				{
					float p_x = obstruction_object.transform.position.x - (x_size/2 + x_offset);
					float p_y = player.transform.position.y;
					float p_z = obstruction_object.transform.position.z - (z_size + z_offset)*a;
					PointA = new Vector3(p_x, p_y, p_z);
					
					 p_x = obstruction_object.transform.position.x + (x_size/2 + x_offset);
					 p_y = player.transform.position.y;
					 p_z = obstruction_object.transform.position.z + (z_size + z_offset)*a;
					PointB = new Vector3(p_x, p_y, p_z);
				}
			}
			}
			else if(PointA!=Vector3.zero && PointB!=Vector3.zero)
			{
				if(passA==false && passB==false)
				{
					player.transform.position = Vector3.MoveTowards(player.transform.position,PointA,movement_speed*Time.deltaTime);
					
					if(Vector3.Distance(player.transform.position, PointA)<=5.0f)
						passA = true;
				}
				else if(passA==true && passB==false)
				{
					player.transform.position = Vector3.MoveTowards(player.transform.position,PointB,movement_speed*Time.deltaTime);
					
					if(Vector3.Distance(player.transform.position, PointB)<=5.0f)
						passB = true;
				}
				else if(passA==true && passB==true)
				{
					//cleaning up
					passedObstruction = true;
					obstruction_object = null;
					PointA = Vector3.zero;
					PointB = Vector3.zero;
					IsObstructionDetected = false;//player can continue straight to target position
				}
			}
		}
	}
	void HandleMovement()
{
    Vector3 move = Vector3.zero;

    if (Input.GetKey(KeyCode.W))
    {
        move += transform.forward;
        SetAnimState(WomanSniperAnimState.FastRun1);
    }
    else if (Input.GetKey(KeyCode.A))
    {
        move -= transform.right;
        SetAnimState(WomanSniperAnimState.RunLeft);
    }
    else if (Input.GetKey(KeyCode.D))
    {
        move += transform.right;
        SetAnimState(WomanSniperAnimState.RunRight);
    }
    if (!IsInMovement && !IsShooting)
		{
			SetAnimState(WomanSniperAnimState.Idle);
		}
		else if (IsInMovement && !IsShooting)
		{
			SetAnimState(WomanSniperAnimState.FastRun1);
		}
	if(IsInMovement==true)
    transform.position += move.normalized * moveSpeed * Time.deltaTime;
	if (generatedWeapon != null)
{
    Renderer rend = generatedWeapon.GetComponent<Renderer>();

    if (rend != null)
    {
        if (IsInMovement)
        {
            rend.enabled = false;
        }
        else if (IsShooting)
        {
            rend.enabled = true;
        }
    }
}
	if(Input.GetKey(KeyCode.LeftControl))
		{
			IsShooting = true;
			IsInMovement = false;
			IsIdle = false;
			ResetAnimatorParams();
			SetAnimState(WomanSniperAnimState.Gunplay4);
			AddWeaponToHands(WeaponPrefab);
		}
		else if(IsShooting==false && IsInMovement==true)
		{
			ExitShootingMode();
		}
}

public void AddWeaponToHands(GameObject weaponPrefab)
{
	weaponHolder = player.transform.GetChild(1).GetChild(1).gameObject;
	if(weaponHolder!=null && weaponHolder.transform.childCount<=0) //stvorime zbran
	{
		if(weaponPrefab!=null)
		{
			generatedWeapon = Instantiate(
				weaponPrefab,
				weaponHolder.transform.position,
				weaponHolder.transform.rotation
		);
		generatedWeapon.transform.SetParent(weaponHolder.transform);
	//	generatedWeapon.transform.localPosition = Vector3.zero;
		generatedWeapon.transform.localRotation = Quaternion.identity;
		}
	}
	else if(weaponHolder!=null && weaponHolder.transform.childCount>0)//zbran mame v ruce
	{
		if(generatedWeapon!=null)
		{
			//generatedWeapon.transform.position = weaponHolder.transform.position;
			generatedWeapon.transform.SetParent(weaponHolder.transform);

			weaponHolder.transform.localPosition =
    Vector3.zero;
	//na nic nesahat !!!
	generatedWeapon.transform.localPosition =
    new Vector3(
        3.93f,
        2.13f,
        -0.28f
    );
weaponHolder.transform.localRotation =
    Quaternion.Euler(
        0f,
        90f,
        0f
    );
	//nic nemenit !!!
			generatedWeapon.transform.localRotation = Quaternion.identity;
			generatedWeapon.transform.localScale = Vector3.one;
			firePoint = generatedWeapon.transform.GetChild(0).gameObject; //mame fire point
			muzzlePrefab = GameObject.Find(muzzle_prefab_name);//mame muzzle prefab
			 barrel_end = generatedWeapon.transform.GetChild(1).gameObject;
			if(barrel_end.transform.childCount<=0)
			{
				muzzleGenerated = Instantiate(muzzlePrefab, barrel_end.transform.position, firePoint.transform.rotation);
				muzzleGenerated.transform.localScale = new Vector3(1.34f,2.38f,1.46f);
				muzzleGenerated.transform.localPosition = new Vector3(-0.34f,-2.98f,1.93f);
				muzzleGenerated.transform.parent = barrel_end.transform;
			}
			else if(barrel_end.transform.childCount>0)
			{
				muzzleGenerated.transform.position = barrel_end.transform.position;
			muzzleGenerated.transform.localScale = new Vector3(1.34f,2.38f,1.46f);
				if(Input.GetKeyDown(KeyCode.Space))
				{
					muzzleGenerated.transform.localPosition = new Vector3(-0.34f,-2.98f,1.93f);
					
					ParticleSystem ps = muzzleGenerated.transform.GetComponent<ParticleSystem>();
					ps.Play();
					
					
				}
			}
		}
	}
}

public void ShootTheBeamToEnemy(GameObject beam, GameObject firingObject)
{
    if (beam == null)
        return;

    // CREATE BEAM
    GameObject generatedBeam = Instantiate(
        beam,
        firingObject.transform.position,
        firingObject.transform.rotation * Quaternion.Euler(0f,90f,0f)
    );

    generatedBeam.transform.localRotation *=
        Quaternion.Euler(90.0f,90.0f,0);

    // =========================
    // NEON EMISSION
    // =========================

    Renderer beamRenderer =
        generatedBeam.GetComponentInChildren<Renderer>();

    if (beamRenderer != null)
    {
        Material mat = beamRenderer.material;

        // enable emission
        mat.EnableKeyword("_EMISSION");

        // HDR neon color
        Color neonColor =
            Color.cyan * 8.0f;

        mat.SetColor("_EmissionColor", neonColor);
    }

    // =========================
    // DYNAMIC LIGHT
    // =========================

    Light beamLight =
        generatedBeam.AddComponent<Light>();

    beamLight.color = Color.cyan;
    beamLight.intensity = 8f;
    beamLight.range = 14f;
    beamLight.shadows = LightShadows.None;

    // =========================
    // TRAIL EFFECT
    // =========================

    TrailRenderer tr =
        generatedBeam.GetComponent<TrailRenderer>();

    if (tr == null)
    {
        tr = generatedBeam.AddComponent<TrailRenderer>();
    }

    tr.time = 0.18f;
    tr.startWidth = 0.45f;
    tr.endWidth = 0.05f;

    // glowing material
    Material trailMat =
        new Material(Shader.Find("Sprites/Default"));

    trailMat.color = Color.cyan;

    tr.material = trailMat;

    // =========================
    // RIGIDBODY VELOCITY
    // =========================

    Rigidbody rb =
        generatedBeam.GetComponent<Rigidbody>();

    if (rb != null)
    {
        rb.velocity =
            firingObject.transform.right *
            350f *
            (-1.0f);
    }

    // =========================
    // RANDOM SCALE PULSE
    // =========================

    generatedBeam.transform.localScale *=
        Random.Range(1.0f, 1.35f);

    // DESTROY
    Destroy(generatedBeam, 25f);
}

void HandleDoubleClickMove(RaycastHit hit2)
    {
        if (Input.GetMouseButtonDown(0))
        {
			ResetAnimatorParams();
            if (Time.time - lastClickTime < doubleClickThreshold)
			{
				destinationPoint = hit_point + hit2.normal * 20.0f;

				currentIntent = PlayerIntent.Move;
				IsInMovement = true;
				IsShooting = false;
			}

            lastClickTime = Time.time;
        }
    }

void ResetAnimatorParams()
{
    animator.SetInteger("IsIdle", 0);
    animator.SetInteger("IsRunning", 0);
    animator.SetInteger("IsShooting", 0);
}
	void SetAnimState(WomanSniperAnimState state)
{
    currentState = state;
	ResetAnimatorParams();
    switch (state)
    {
        case WomanSniperAnimState.Idle:
            animator.Play("Idle");
			animator.SetInteger("IsIdle",1);
            break;

        case WomanSniperAnimState.HappyIdle:
            animator.Play("Happy Idle");
			animator.SetInteger("IsIdle",1);
            break;

        case WomanSniperAnimState.Gunplay4:
            animator.Play("Gunplay (4)");
			animator.SetInteger("IsShooting",1);
            break;

        case WomanSniperAnimState.Gunplay5:
            animator.Play("Gunplay (5)");
			animator.SetInteger("IsShooting",1);
            break;

        case WomanSniperAnimState.Shooting:
            animator.Play("Shooting (1)");
			animator.SetInteger("IsShooting",1);
            break;
			
		case WomanSniperAnimState.FastRun1:
		animator.Play(running_animation_name);
		animator.SetInteger("IsRunning",1);
		break;
		
		case WomanSniperAnimState.Crouching:
		animator.Play(crouching_animation_name);
		animator.SetInteger("IsRunning",1);
		break;
		
		case WomanSniperAnimState.Proning:
		animator.Play(proning_animation_name);
		animator.SetInteger("IsRunning",1);
		break;
    }
}

    void OnGUI()
    {
        if (showHitText)
        {
            GUI.Label(new Rect(20, 20, 300, 30), "You hit " + enemyName);
        }
    }
}
