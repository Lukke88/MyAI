using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnit : MonoBehaviour //dat to do noveho enemy
{
	private bool isEnemySubstituted = false;
	public Vector3 attack_position_point;
	public bool IsEnemyReversing, IsEnemySeekingHideout;
	public GameObject substitutionEnemyObject, sub_weaponHolder, muzzleGenerated, barrel_end, muzzlePrefab, firePoint, generatedWeapon, weaponHolder;
	public float lastSeenTime;
	public Vector3 lastSeenPosition;
	public string muzzle_prefab_name;
	public string substitutionEnemyobjectName = "";
	public string sub_weaponHolder_name = "";
	public Vector3 predictedPosition;
	public bool hasOverrideTarget;
	public Vector3 overrideTarget;

	public enum CharacterType
{
    Player,
    EnemyRifleman,
    EnemySniper,
    EnemyMachineGunner,
    EnemyOfficer,
    EnemyWolf
}
[System.Serializable]
public class CharacterSetup
{
    public CharacterType type;
    public GameObject modelPrefab;
    public string weaponHolderName;
    public WeaponType weaponType;
}

public CharacterType characterType;
[Header("Reference")]
public GameObject playerWeaponHolder;
public GameObject riflemanWeaponHolder;
public GameObject sniperWeaponHolder;
public GameObject mgWeaponHolder;
	[Header("Weapon")]
public GameObject prefabBeam;
public GameObject muzzleNullObject;

public enum WeaponType
{
    Pistol,
    SMG,
    Rifle,
    AK47,
    SniperRifle
}

public WeaponType weaponType;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public DifficultyLevel difficulty;

[Header("Audio")]
public AudioSource audioSource;
public AudioClip shotClip;
public AudioClip playerHitClip;

[Header("Damage")]
public float healthDamage = 10f;

[Header("Shooting")]
public float attackRange = 200f;

private float nextShotTime;
	public GameObject player;
	public Vector3 currentAvoidTarget;
    private Transform targetSlot;
    private int slotIndex;
    private WallSlots wall;
    private EnemyManager manager;
	public GameObject enemy, selected_wall, destinationObject;
    public float speed = 25f;
	public float min_dist_from_wall = 40.0f;
	public float dist_to_wall;
	public float distanceToPlayer;
	public Transform tank;
	public float shootDamage = 10f;
	public float shootCooldown = 1f;
	private float shootTimer;
	public LineRenderer lr;
	public GameObject muzzle;
	public bool hasLineOfSight;
	public GameObject gunInstance;
	public GameObject gunMuzzle;
	
	public Vector3 pointA, pointB;
	public float gap;
	public bool avoiding, IsAvoidingObstruction, IsRunning, IsShooting;
	public GameObject obstructionObject;
	
	static List<Vector3> wallSlots = new List<Vector3>();
	static List<bool> slotTaken = new List<bool>();
	
	public bool HasFreeSlot;
	public Vector3 NewDestination;//slot position
	public bool IsEnemyBorn;
	public string enemyToInstantiate_name;
	public GameObject enemyToInstantiate, prefab_enemyToInstantiate;
	public bool IsBetweenChosenEndObjects;
	
	[Header("Animations")]
	public string reversing_animation = "Reverse";
	public string takeCover_animation = "TakeCover";
	
	private HezbollahTerroristBehaviour attackScript;
	private EnemyUnit movementScript;
	public enum MoveMode
{
    Normal,
    AvoidingCar,
    AvoidingWall,
    SlotMove,
	Reversing,
    // NEW
    Retreating,
    TacticalRetreat,
    HoldingCover
}

public AudioClip takeCoverClip;
public AudioSource voiceSource;

bool wasPlayerInTank;
public float retreatDistance = 300f;

	public MoveMode moveMode;
	public enum EnemyBehaviourState
	{
		Running,
		Pursuit,
		Shooting,
		Searching
	}
	public EnemyBehaviourState currentState;
	public void Start()
	{	
	enemy = GameObject.Find(this.name);
		 attackScript = GetComponent<HezbollahTerroristBehaviour>();
		movementScript = GetComponent<EnemyUnit>();
		nextShotTime = Time.time + Random.Range(0.5f, 2.0f);
		
		UpdateWeaponStats();
		
		if(IsEnemyBorn==false)
		{
			enemy = this.gameObject;
			if(enemyToInstantiate==null)
			{
				prefab_enemyToInstantiate = GameObject.Find(enemyToInstantiate_name); //find in scene chosen enemy prefab by name
				enemyToInstantiate = Instantiate(prefab_enemyToInstantiate, enemy.transform.position, enemy.transform.rotation); //instantiate that prefab
				enemyToInstantiate.transform.parent = enemy.transform;//adds new enemy as child the old capsule
				enemy.transform.GetComponent<Renderer>().enabled = false; //switch off capsule render
			}
		}
		if(substitutionEnemyobjectName!="" && isEnemySubstituted==false)
		{
			substitutionEnemyObject = GameObject.Find(substitutionEnemyobjectName);
			SubstituteDefaultEnemyForNewOne();
			enemy = substitutionEnemyObject;
			
			Transform holder =
		FindChildRecursive(
        substitutionEnemyObject.transform,
        sub_weaponHolder_name);

		if(holder != null)
		{
			weaponHolder = holder.gameObject;
		}
		isEnemySubstituted = true;
		}
			
		if(selected_wall!=null)
		GenerateSlots(selected_wall.transform.position);
	}
	
	Vector3 GetTarget()
{
    if(hasOverrideTarget)
        return overrideTarget;

    return predictedPosition;
}
	
	void SetupWeaponHolder()
{
    switch(characterType)
{
    case CharacterType.Player:
        weaponHolder = playerWeaponHolder;
        break;

    case CharacterType.EnemyRifleman:
        weaponHolder = riflemanWeaponHolder;
        break;

    case CharacterType.EnemySniper:
        weaponHolder = sniperWeaponHolder;
        break;

    case CharacterType.EnemyMachineGunner:
        weaponHolder = mgWeaponHolder;
        break;
}
}

  void Update()
    {
      
	  /*
	  //schovani kapsle
	  GetComponent<MeshRenderer>().enabled = false;
	  
	  GameObject model = Instantiate(enemyPrefab, transform); //enemy model
		model.transform.localPosition = Vector3.zero;
		model.transform.localRotation = Quaternion.identity;
		
		visual.transform.localPosition = Vector3.zero;
		visual.transform.localRotation = Quaternion.identity;
		
		animator.SetFloat("Speed", agent.velocity.magnitude);
	  */
	  
	  bool isPlayerInTank =
			Vector3.Distance(player.transform.position, tank.position) < 10f;
			
			if(isPlayerInTank && !wasPlayerInTank)
			{
				StartRetreatMode();
			}

			wasPlayerInTank = isPlayerInTank;
	
		if(enemy==null)enemy = GameObject.Find(this.name);
		if(substitutionEnemyobjectName!="" && isEnemySubstituted==false)
		{
			substitutionEnemyObject = GameObject.Find(substitutionEnemyobjectName);
			SubstituteDefaultEnemyForNewOne();
			enemy = substitutionEnemyObject;
			
			Transform holder =
		FindChildRecursive(
        substitutionEnemyObject.transform,
        sub_weaponHolder_name);

		if(holder != null)
		{
			weaponHolder = holder.gameObject;
		}
		isEnemySubstituted = true;
		}
		float height_y = enemy.transform.position.y;
		if(height_y<32)
			enemy.transform.position = new Vector3(enemy.transform.position.x,height_y,enemy.transform.position.z);
		if(selected_wall==null)
			selected_wall = GameObject.Find("Main_wall");//if is not null, then can be changed
		
		if(destinationObject!=null)
			selected_wall = destinationObject;//go to there
		float wallLength = selected_wall.transform.GetComponent<Collider>().bounds.size.z;
		float gap = wallLength / 10f;
		
		ReactToPlayer();//changes state depending on distance of the player figure
		
		if (distanceToPlayer <= 200f && hasLineOfSight)
{
    FireWeapon();
    lastSeenPosition = player.transform.position;
    lastSeenTime = Time.time;
}
else if (Time.time - lastSeenTime < 5f)
{
    Chase(lastSeenPosition);
}
else
{
    MoveTo(predictedPosition);
}

if(Vector3.Distance(enemy.transform.position, player.transform.position) <= 300f) //strelba do maximalni vzdalenosti, podle typu zbrane
{
    IsShooting = true;
}
else
{
    IsShooting = false;
}

float dist = Vector3.Distance(enemy.position, player.position);//distance player - enemy

Vector3 toEnemy = (enemy.position - turret.position).normalized;//enemy - (player)tank turret position

float angle = Vector3.Angle(turret.forward, toEnemy);//angle turret to enemy

Vector3 toEnemy2 = (enemy.position - cannon.position).normalized; //normalized line of player tank cannon to enemy

float cannonAngle = Vector3.Angle(cannon.forward, toEnemy2); //angle of cannon
//conditions for enemy shooting, if is not in shooting field of tank cannon and is on the side of the tank or behind, then can shoot
bool inRange = dist <= 500f;

bool inTurretCone = angle <= 90f;

bool inCannonDeadZone = cannonAngle <= 15f;

bool canShoot =
    inRange &&
    inTurretCone &&
    !inCannonDeadZone;
	
	if(playerInTank || playerInactive)//if player is inactive then enemy retreats or attacks(depending on cannon turret angle)
{
    moveMode = MoveMode.TacticalRetreat; // nebo CoverIdle
}
else
{
    moveMode = MoveMode.Normal;
}

if(canShoot)
{
    FireWeapon();
}
else
{
    // suppressive fire / reposition / track
    TrackTarget();
}
		
		switch(moveMode)//typ pohybu
{
    case MoveMode.TacticalRetreat:
        TacticalRetreat();//ustup se strelbou
        break;

    case MoveMode.Retreating:
        Retreat();//ustup, hledani hideoutu
        break;

    case MoveMode.Normal:
        NormalMove();//pohyb k cilovemu pointu
        break;

    case MoveMode.AvoidingWall:
        ConstructEdgePointsAroundObstruction(obstructionObject);//obchazeni zdi
        break;

    case MoveMode.AvoidingCar: //obchazeni auta
        enemy.transform.position = Vector3.MoveTowards(
            enemy.transform.position,
            currentAvoidTarget,
            speed * Time.deltaTime
        );
        break;
}
		if(selected_wall!=null) //generace slotu u zdi pro nepratele(hideouty)
		{
		GenerateSlots(selected_wall.transform.position);
		if(Mathf.Abs(enemy.transform.position.z - selected_wall.transform.position.z)<=10.0f)
			IsAtFinalWall = true;
			
		}
		GameObject start_tile = GameObject.Find("enemy_tile");
		GameObject nearest_wall = GameObject.Find("Main_wall");
		if(enemy.transform.position.z<start_tile.transform.position.z && enemy.transform.position.z>nearest_wall.transform.position.z)//generace "dlazdic" u cilove zdi
		{
			if(Mathf.Abs(enemy.transform.position.z - start_tile.transform.position.z)<=10.0f)//is at level of chosen object ?
			IsBetweenChosenEndObjects = true;//this switch of searching way to player object
			
			if(enemyToInstantiate!=null)
			{
				attackScript.enabled = true;
				//movementScript.enabled = false;
				//enemy.transform.GetComponent<HezbollahTerroristBehaviour>().enabled = true;//switch on attack script
				//enemy.transform.GetComponent<EnemyUnit>().enabled = false; //switch off pathfinding script
			}
		}
		if(!HasFreeSlot)//ma volnou dlazdici (waypoint, hideout)?
		{
			GetFreeSlot();//gets slot at wall
		}
		else if(HasFreeSlot==true)
		{
		}
			
		/*if(Vector3.Distance(transform.position, player.position) < shootRange)
		{
			ShootAtPlayer();
		}*/  
		DetectWallInFront();//detects obstruction
		float side_speed = 5.0f;
		if(IsAvoidingObstruction==true)
	{
		if(obstructionObject!=null)
		{
			//enemy moves to side along building/wall, until is view empty and obstruction is not in path
			if(enemy.transform.position.z<obstructionObject.transform.position.z)
				if(Mathf.Abs(enemy.transform.position.z - obstructionObject.transform.position.z)<30.0f)
					enemy.transform.Translate(0,0,-side_speed);
			else if(enemy.transform.position.z>=obstructionObject.transform.position.z)
				if(Mathf.Abs(enemy.transform.position.z- obstructionObject.transform.position.z)<30.0f)
					enemy.transform.Translate(0,0,side_speed);
		}
	}
		/**/if(Vector3.Distance(enemy.transform.position, selected_wall.transform.position) > min_dist_from_wall && IsBetweenChosenEndObjects==false)
		{
			DetectWallInFront();
			if(IsAvoidingObstruction==false)//go straight
			{
				enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, selected_wall.transform.position, speed*Time.deltaTime);
			}
			else if(IsAvoidingObstruction==true)//go along building/wall
			{
				/*
				if(obstructionObject.name.Contains("wall") || obstructionObject.name.Contains("building"))
				ConstructEdgePointsAroundObstruction(obstructionObject);
				else //v pripade vraku ci tanku
				{
					enemy.transform.position = Vector3.MoveTowards(
					enemy.transform.position,
					currentAvoidTarget,
					speed * Time.deltaTime
					);
					
					if(Vector3.Distance(transform.position, currentAvoidTarget) < 2f)
					{
						IsAvoidingObstruction = false;
					}
				}*/
				
				if(obstructionObject.CompareTag("CarWreck"))
				{
					moveMode = MoveMode.AvoidingCar;
				}
				else
				{
					moveMode = MoveMode.AvoidingWall;
				}
			}
			dist_to_wall = Vector3.Distance(enemy.transform.position, selected_wall.transform.position);
		}
		else if(IsPreparedToShoot==false)
		{
		SetupGun();
/*
    if(tank == null)
        tank = GameObject.Find("Merkava Mk_lowpoly").transform;

    Vector3 dir = (tank.position - enemy.transform.position).normalized;

    enemy.transform.rotation = Quaternion.LookRotation(dir);

    gunInstance.transform.position = enemy.transform.position;

    shootTimer -= Time.deltaTime;

    if(shootTimer <= 0f)
    {
        ShootBeam(dir);
        shootTimer = shootCooldown;
    }*/
	}
	else if(IsPreparedToShoot==true)
	{
		TurnEnemyToPlayerObject();
	}
	
	if(IsShooting==true)
	{
		AddWeaponToHands(weaponHolder);//put weapon in front of enemy
		FireWeapon();//one system for shooting involves 3 functions
	}
	
	
		//MoveToTarget();
    }
	
	void StartRetreatMode()
{
    moveMode = MoveMode.TacticalRetreat;

    if(voiceSource != null && takeCoverClip != null)
        voiceSource.PlayOneShot(takeCoverClip);

    Animator anim = GetComponent<Animator>();
    if(anim != null)
        anim.Play(takeCover_animation);
}

Vector3 GetRetreatPoint()
{
    Vector3 A = PointA;
    Vector3 B = PointB;

    Vector3 enemyPos = transform.position;

    float dA = Vector3.Distance(enemyPos, A);
    float dB = Vector3.Distance(enemyPos, B);

    Vector3 closest = dA < dB ? A : B;
    Vector3 farthest = dA < dB ? B : A;

    Vector3 C = selected_wall.transform.position +
                new Vector3(Random.Range(-10f, 10f), 0,
                            Random.Range(-10f, 10f));

    // bias: cover preference
    return (Random.value < 0.7f) ? closest : C;
}

void Retreat()
{
    Vector3 target = GetRetreatPoint();

    Vector3 dir = (target - transform.position).normalized;

    // otočení k cíli
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(dir),
        Time.deltaTime * 5f
    );

    Animator anim = GetComponent<Animator>();
    if(anim != null)
        anim.Play(reversing_animation);

    transform.position = Vector3.MoveTowards(
        transform.position,
        target,
        speed * 0.6f * Time.deltaTime
    );
}
/*
Tactical retreat (střelba + couvání zároveň)

Tohle je tvoje:

“strileji a ustupuji”
*/
void TacticalRetreat()
{
    Vector3 target = GetRetreatPoint();

    Vector3 dir = (player.transform.position - transform.position).normalized;

    // otočení k hráči (ale pohyb dozadu)
    transform.rotation = Quaternion.Slerp(
        transform.rotation,
        Quaternion.LookRotation(dir),
        Time.deltaTime * 5f
    );

    // střelba
    IsShooting = true;

    // couvání
    Vector3 backward = -dir;

    transform.position += backward * speed * 0.5f * Time.deltaTime;
}
	
	void Chase(Vector3 targetPos)
	{
    enemy.transform.position =
        Vector3.MoveTowards(
            enemy.transform.position,
            targetPos,
            speed * Time.deltaTime);
	}
	
	void MoveTo(Vector3 targetPos)
	{
    enemy.transform.position =
        Vector3.MoveTowards(
            enemy.transform.position,
            targetPos,
            speed * Time.deltaTime);
	}
	
	public float shootRange;

void UpdateWeaponStats()
{
    switch(weaponType)
    {
        case WeaponType.Pistol:
            shootRange = 200f;
            break;

        case WeaponType.SMG:
            shootRange = 500f;
            break;

        case WeaponType.Rifle:
            shootRange = 500f;
            break;

        case WeaponType.AK47:
            shootRange = 800f;
            break;

        case WeaponType.SniperRifle:
            shootRange = 1200f;
            break;
    }
}
//substitute enemy with new one by name
void SubstituteDefaultEnemyForNewOne()
{
    if(substitutionEnemyObject == null)
        return;

    // schovej kapsli
    MeshRenderer mr = GetComponent<MeshRenderer>();

    if(mr != null)
        mr.enabled = false;

    // připoj model ke kapsli
    substitutionEnemyObject.transform.SetParent(transform);

    substitutionEnemyObject.transform.localPosition = Vector3.zero;
    substitutionEnemyObject.transform.localRotation = Quaternion.identity;

    // najdi držák zbraně
    Transform holder =
        FindChildRecursive(
            substitutionEnemyObject.transform,
            sub_weaponHolder_name);

    if(holder != null)
    {
        sub_weaponHolder = holder.gameObject;
        weaponHolder = sub_weaponHolder;
    }

    enemy = substitutionEnemyObject;
}
//hledani objektu child podle jmena
Transform FindChildRecursive(Transform parent, string targetName)
{
    foreach(Transform child in parent)
    {
        if(child.name == targetName)
            return child;

        Transform result =
            FindChildRecursive(child, targetName);

        if(result != null)
            return result;
    }

    return null;
}

float GetHitChance()
{
    switch(difficulty)
    {
        case DifficultyLevel.Easy:
            return 0.20f;

        case DifficultyLevel.Medium:
            return 0.60f;

        case DifficultyLevel.Hard:
            return 0.80f;
    }

    return 0.60f;
}


void FireWeapon() //jednotny system strelby
{
    if(Time.time < nextShotTime)
        return;

    nextShotTime =
        Time.time +
        Random.Range(0.5f,2.0f);

    PlayMuzzleEffects();

    ShootTheBeamToEnemy(
        prefabBeam,
        muzzleNullObject);

    TryHitPlayer();
}

void PlayMuzzleEffects()
{
    if(audioSource != null &&
       shotClip != null)
    {
        audioSource.PlayOneShot(shotClip);
    }

    ParticleSystem ps =
        muzzleGenerated.GetComponent<ParticleSystem>();

    if(ps != null)
    {
        ps.Play();
    }

    Animator anim =
        GetComponent<Animator>();

    if(anim != null)
    {
        anim.SetTrigger("Fire");
    }
}

void TryHitPlayer()
{
    Vector3 dir =
        (player.transform.position -
        muzzleNullObject.transform.position)
        .normalized;

    dir = AddSpread(dir);

    RaycastHit hit;

    if(
        Physics.Raycast(
            muzzleNullObject.transform.position,
            dir,
            out hit,
            shootRange))
    {
        if(hit.collider.CompareTag("Player"))
        {
            float chance =
                GetHitChance();

            if(Random.value <= chance)
            {
                ApplyDamageToPlayer(
                    hit.collider.gameObject);
            }
        }
    }
}



Vector3 AddSpread(Vector3 dir)
{
    float spread = 5f;

    dir =
        Quaternion.Euler
        (
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0
        )
        * dir;

    return dir;
}
	
	public void AddWeaponToHands(GameObject weaponPrefab) //gives weapon to enemy hands, weaponHolder is null object in front of the enemy, to attach weapon there
	{
	//weaponHolder = player.transform.GetChild(1).GetChild(1).gameObject;//default
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
				if(CanShootPlayer())
				{
					TryShootPlayer();
				}
			}
		}
	}
}

bool CanShootPlayer()
{
    if(player == null)
        return false;

    float dist =
        Vector3.Distance(
            transform.position,
            player.transform.position);

    return dist <= attackRange;
}

void TryShootPlayer()
{
    if(Time.time < nextShotTime)
        return;

    nextShotTime =
        Time.time +
        Random.Range(0.5f, 2.5f);

    ShootTheBeamToEnemy(
        prefabBeam,
        muzzleNullObject);

    if(audioSource != null &&
       shotClip != null)
    {
        audioSource.PlayOneShot(shotClip);
    }

    CheckPlayerHit();
}

void CheckPlayerHit()
{
    if(player == null)
        return;

    Vector3 dir =
        (player.transform.position -
        muzzleNullObject.transform.position)
        .normalized;

    RaycastHit hit;

    if(Physics.Raycast(
        muzzleNullObject.transform.position,
        dir,
        out hit,
        attackRange))
    {
        if(hit.collider.CompareTag("Player"))
        {
            ApplyDamageToPlayer(hit.collider.gameObject);
        }
    }
}

void ApplyDamageToPlayer(GameObject playerObj)
{
    // Damage

    TankHealth hp =
        playerObj.GetComponent<TankHealth>();

    if(hp != null)
    {
        hp.ApplyDamage(healthDamage);
    }

    // Hit animation

    Animator anim =
        playerObj.GetComponent<Animator>();

    if(anim != null)
    {
        anim.SetTrigger("Hit");
    }

    // Aargh sound

    AudioSource playerAudio =
        playerObj.GetComponent<AudioSource>();

    if(playerAudio != null &&
       playerHitClip != null)
    {
        playerAudio.PlayOneShot(playerHitClip);
    }
}
	
	public void ShootTheBeamToEnemy(GameObject beam, GameObject firingObject) //borrowed from WomanSniperBehaviour, beam is prefab beam, firing object is muzzle null game object
{
    if (beam == null)
        return;

    // CREATE BEAM
    GameObject generatedBeam = Instantiate(
        beam,
        firingObject.transform.position,
        firingObject.transform.rotation * Quaternion.Euler(0f,90f,0f)
    );
	//this hits player at raycast
    generatedBeam.transform.localRotation *=
        Quaternion.Euler(90.0f,90.0f,0);
	RaycastHit hit;

	Vector3 dir =
	(
    player.transform.position -
    firingObject.transform.position
	).normalized;

	if(
    Physics.Raycast(
        firingObject.transform.position,
        dir,
        out hit,
        attackRange))
	{
    if(hit.collider.CompareTag("Player"))
    {
        ApplyDamageToPlayer(
            hit.collider.gameObject);
    }
	}
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
	
	public void ReactToPlayer()
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
		currentState =
        EnemyBehaviourState.Shooting;
		}
		}
		
		float attackDistance = 200f;
		float pursuitDistance = 250f;

		float dist =
		Vector3.Distance(
        transform.position,
        player.transform.position
    );

if(dist <= attackDistance)
{
    currentState =
        EnemyBehaviourState.Shooting;
}
else if(dist > pursuitDistance)
{
    currentState =
        EnemyBehaviourState.Pursuit;
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
	void GenerateSlots(Vector3 wallCenter)
{
	//enemy runs from left(enemy.x<wall.x) and wall is longer in z axis than x axis
	float offset_x = 10.0f;
    int count = 10;
	float size_z = selected_wall.transform.GetComponent<Collider>().bounds.size.z;
	float size_x = selected_wall.transform.GetComponent<Collider>().bounds.size.x;
	gap = size_z/count;
    for(int i = 0; i < count; i++)
    {
		
        Vector3 pos = new Vector3( wallCenter.x - offset_x, enemy.transform.position.y, wallCenter.z - size_z/2 + i * gap);
        wallSlots.Add(pos);
        slotTaken.Add(false);
    }
}

int GetFreeSlot()
{
    for(int i = 0; i < slotTaken.Count; i++)
    {
        if(!slotTaken[i])
        {
            slotTaken[i] = true;
			NewDestination = wallSlots[i];
            return i;
        }
    }
    return -1;
}
	public bool IsAtFinalWall;
	public GameObject Obstruction_Object;
	public bool DetectWallInFront()
	{
		IsAvoidingObstruction = false;
		float ray_length = 150.0f;
		Vector3 dir = enemy.transform.right;
		RaycastHit hit;
		Ray ray = new Ray(enemy.transform.position, dir);
		if(Physics.Raycast(ray, out hit, ray_length))
		{
			if(hit.collider.CompareTag("building"))
			{
				avoiding = true;
				Obstruction_Object = hit.collider.gameObject;
				obstructionObject = Obstruction_Object;
				IsAvoidingObstruction = true;
			}
			else if(hit.collider.CompareTag("Wall") && hit.collider.gameObject.name.Contains("MainWall"))
			{
				selected_wall = hit.collider.gameObject;
				CreateWallPosition();
				IsAtFinalWall = true;
				IsAvoidingObstruction = false;
			}
			else if(hit.collider.CompareTag("CarWreck"))
			{
			obstructionObject = hit.collider.gameObject;
			CalculateByBounds(obstructionObject);
			IsAvoidingObstruction = true;
			}
		}
		if(IsAtFinalWall==false)
		Debug.DrawRay(enemy.transform.position,enemy.transform.right * ray_length, Color.red);
		else if(IsAtFinalWall==true)
		Debug.DrawRay(enemy.transform.position,enemy.transform.forward * ray_length*2, Color.yellow);
		
		return avoiding;
	}
	
	public Vector3 PointA, PointB;

void CalculateByBounds(GameObject obj)
{
    Bounds b = obj.GetComponent<Collider>().bounds;

    Vector3 enemyPos = transform.position;

    // rozhodni osu obcházení (kratší strana)
    bool goX = b.size.x < b.size.z;

    if(goX)
    {
        // obejít po X stranách
        PointA = new Vector3(b.min.x - 5f, enemyPos.y, enemyPos.z);
        PointB = new Vector3(b.max.x + 5f, enemyPos.y, enemyPos.z);
    }
    else
    {
        // obejít po Z stranách
        PointA = new Vector3(enemyPos.x, enemyPos.y, b.min.z - 5f);
        PointB = new Vector3(enemyPos.x, enemyPos.y, b.max.z + 5f);
    }

    // vyber kratší
    float dA = Vector3.Distance(enemyPos, PointA);
    float dB = Vector3.Distance(enemyPos, PointB);

    if(dA < dB)
    {
        currentAvoidTarget = PointA;
    }
    else
    {
        currentAvoidTarget = PointB;
    }
}
	
	public void CreateWallPosition()
	{
		speed = 25.0f;
		GenerateSlots(selected_wall.transform.position);//creates positions behind selected wall
		GetFreeSlot();//choose one position at wall
		if(NewDestination!=Vector3.zero) //goes to position at wall
		{
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, NewDestination, speed*Time.deltaTime);
		}
		else if(Vector3.Distance(enemy.transform.position, enemy.transform.position)<=10.0f) //if is at position, starts attack
		{
			IsShooting = true;
			ShootToPlayerObject();
		}
	}
	public void ShootToPlayerObject()
{
    if(tank == null || gunMuzzle == null) return;

    // ⏱ náhodný cooldown (každý enemy jinak)
    shootTimer -= Time.deltaTime;
    if(shootTimer > 0f) return;

    shootTimer = Random.Range(0.5f, 2.0f); // nesynchronní střelba

    // 🎯 směr na tank
    Vector3 dir = (tank.position - gunMuzzle.transform.position).normalized;

    // 🎲 rozptyl (cca 5%)
    float spread = 0.05f;
    dir.x += Random.Range(-spread, spread);
    dir.y += Random.Range(-spread, spread);
    dir.z += Random.Range(-spread, spread);
    dir.Normalize();

    // 🔥 particle efekt výstřelu
    if(gunMuzzle.transform.childCount > 0)
    {
        ParticleSystem ps = gunMuzzle.GetComponentInChildren<ParticleSystem>();
        if(ps != null)
        {
            ps.Play();
        }
    }

    // 🔫 beam / raycast
    RaycastHit hit;
    Vector3 origin = gunMuzzle.transform.position;

    if(lr != null)
    {
        lr.enabled = true;
        lr.SetPosition(0, origin);
    }

    if(Physics.Raycast(origin, dir, out hit, 200f))
    {
        if(lr != null)
            lr.SetPosition(1, hit.point);

        // 🎯 šance na zásah (2–5%)
        float hitChance = Random.Range(0.02f, 0.05f);

        if(hit.transform.CompareTag("Player") && Random.value < hitChance)
        {
            TankHealth hp = hit.transform.GetComponent<TankHealth>();

            if(hp != null)
            {
                float dmg = Random.Range(2f, 5f); // malé damage
                hp.ApplyDamage(dmg);
            }
        }
    }
    else
    {
        if(lr != null)
            lr.SetPosition(1, origin + dir * 200f);
    }

    // vypnutí laseru
    StartCoroutine(DisableLaser());
}
	//public Vector3 PointA, PointB;
	public void CalculatePointAPointB(GameObject obstructionObject, int coef_x1, int coef_x2, int coef_z1, int coef_z2)
	{
		
		float offset_x = 10.0f;
		float offset_z = 10.0f;
		float size_x = obstructionObject.transform.GetComponent<Collider>().bounds.size.x;
		float size_z = obstructionObject.transform.GetComponent<Collider>().bounds.size.z;
		
		float diff_x = Mathf.Abs(enemy.transform.position.x - obstructionObject.transform.position.x);
		float diff_z = Mathf.Abs(enemy.transform.position.z - obstructionObject.transform.position.z);
		//changes coefficients accordingf to direction of the enemy
		
		float x = obstructionObject.transform.position.x + (size_x/2 - offset_x)*coef_x1;
				float y = enemy.transform.position.y;
				float z = obstructionObject.transform.position.z + (size_z/2 + offset_z)*coef_z1;
				PointA = new Vector3(x, y, z);
				
				float x2 = obstructionObject.transform.position.x + (size_x/2 - offset_x)*coef_x2;
				float y2 = enemy.transform.position.y;
				float z2 = obstructionObject.transform.position.z + (size_z/2 + offset_z)*coef_z2;
				PointB = new Vector3(x2, y2, z2);
		
	}
	public bool passedA, passedB;
	public void ConstructEdgePointsAroundObstruction(GameObject ObstructionObject)
	{
		float offset_x = 10.0f;
		float offset_z = 10.0f;
		
		if(ObstructionObject!=null)
		{
			float size_x = ObstructionObject.transform.GetComponent<Collider>().bounds.size.x;
			float size_z = ObstructionObject.transform.GetComponent<Collider>().bounds.size.z;
		}
		float diff_x = Mathf.Abs(enemy.transform.position.x - ObstructionObject.transform.position.x);
		float diff_z = Mathf.Abs(enemy.transform.position.z - ObstructionObject.transform.position.z);
		
		/*Vector3 right = Vector3.Cross(Vector3.up, (enemy.position - obj.position).normalized);
		PointA = obj.position + right * offset;
		PointB = obj.position - right * offset;*/
		if(diff_z>diff_x)
		{
			//creates 2 points at the side
			if(enemy.transform.position.x<ObstructionObject.transform.position.x && enemy.transform.position.z>ObstructionObject.transform.position.z)
			{
				CalculatePointAPointB(ObstructionObject,-1, 1, -1, -1);
				/*float x = ObstructionObject.transform.position.x - size_x/2 - offset_x;
				float y = enemy.transform.position.y;
				float z = ObstructionObject.transform.position.z + size_z/2 + offset_z;
				PointA = new Vector3(x, y, z);
				
				float x2 = ObstructionObject.transform.position.x - size_x/2 - offset_x;
				float y2 = enemy.transform.position.y;
				float z2 = ObstructionObject.transform.position.z - (size_z/2 + offset_z);
				PointB = new Vector3(x2, y2, z2);*/
			}
			//creates 2 points at the side
			else if(enemy.transform.position.x>ObstructionObject.transform.position.x && enemy.transform.position.z>ObstructionObject.transform.position.z)
			{
				CalculatePointAPointB(ObstructionObject, 1, 1, 1, -1);
				/*float x = ObstructionObject.transform.position.x + (size_x/2 + offset_x);
				float y = enemy.transform.position.y;
				float z = ObstructionObject.transform.position.z + size_z/2 + offset_z;
				PointA = new Vector3(x, y, z);
				
				float x2 = ObstructionObject.transform.position.x + (size_x/2 - offset_x);
				float y2 = enemy.transform.position.y;
				float z2 = ObstructionObject.transform.position.z - (size_z/2 + offset_z);
				PointB = new Vector3(x2, y2, z2);*/
			}
			else if(enemy.transform.position.x<ObstructionObject.transform.position.x && enemy.transform.position.z<ObstructionObject.transform.position.z)
			{
				CalculatePointAPointB(ObstructionObject,-1, -1, -1, 1);
			}
			else if(enemy.transform.position.x>ObstructionObject.transform.position.x && enemy.transform.position.z<ObstructionObject.transform.position.z)
			{
				CalculatePointAPointB(ObstructionObject,1, 1, -1, 1);
			}
				
			
		}
		else if(diff_x>diff_z)
		{
			if(enemy.transform.position.x<ObstructionObject.transform.position.x && enemy.transform.position.z>ObstructionObject.transform.position.z)
			{
				CalculatePointAPointB(ObstructionObject,-1, 1, -1, -1);
			}
		}
		if(passedA==false && passedB==false)
		{
			Debug.DrawLine(enemy.transform.position, PointA, Color.green);
			Debug.DrawLine(PointB, PointA, Color.green);
			Debug.DrawLine(selected_wall.transform.position, PointB, Color.green);
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, PointA, speed*Time.deltaTime);
		}
				else if(passedA==true && passedB==false)
				enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, PointB, speed*Time.deltaTime);
			
				if(Vector3.Distance(enemy.transform.position, PointA)<=5.0f)passedA = true;
				if(Vector3.Distance(enemy.transform.position, PointB)<=5.0f)passedB = true;
				
				if(passedA==true && passedB==true)
				{
					IsAvoidingObstruction = false;//can go back to normal regime
					avoiding = false;
					ResetAvoidance();
				}
	}
	void ResetAvoidance()
	{
    passedA = false;
    passedB = false;
	}
	public float Distance, Angle;
	void TurnEnemyToPlayerObject()
{
    if(tank == null)
        tank = GameObject.Find("Merkava Mk_lowpoly").transform;

    Vector3 dir = tank.position - enemy.transform.position;

    // ❗ ignoruj výšku (Y)
    dir.y = 0f;

    if(dir != Vector3.zero && IsAtFinalWall==false)
    {
        Quaternion targetRot = Quaternion.LookRotation(dir);
        enemy.transform.rotation = Quaternion.Slerp(
            enemy.transform.rotation,
            targetRot,
            Time.deltaTime * 5f // rychlost otáčení
        );
    }
	else if(IsAtFinalWall==true)
	{
		//manual angle calculation
		Distance = Vector3.Distance(enemy.transform.position, tank.transform.position);
		Angle = Mathf.Asin(Mathf.Abs(enemy.transform.position.z - tank.transform.position.z)/Distance)*180/Mathf.PI;
		if(tank.transform.position.x>enemy.transform.position.x)
		{
		if(tank.transform.position.z<enemy.transform.position.z)
		enemy.transform.rotation = Quaternion.Euler(0,Angle,0);
		else if(tank.transform.position.z>enemy.transform.position.z)
		enemy.transform.rotation = Quaternion.Euler(0,-Angle,0);
		}
		else if(tank.transform.position.x<=enemy.transform.position.x)
		{
		if(tank.transform.position.z<enemy.transform.position.z)
		enemy.transform.rotation = Quaternion.Euler(0,-Angle+180,0);
		else if(tank.transform.position.z>enemy.transform.position.z)
		enemy.transform.rotation = Quaternion.Euler(0,Angle + 180,0);
		}
	}
}
	void AvoidObstacle(RaycastHit hit)
	{
    if(!avoiding)
    {
        Vector3 normal = hit.normal;
        Vector3 side = Vector3.Cross(Vector3.up, normal);

        pointA = hit.point + side * 3f - normal * 5f;
        pointB = hit.point + side * 3f + normal * 5f;

        avoiding = true;
    }

    Vector3 target = pointA;

    transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

    if(Vector3.Distance(transform.position, pointA) < 1f)
    {
        avoiding = false;
    }
}
public bool IsPreparedToShoot;
void SetupGun()
{
    if(gunInstance != null) return;

    Transform gunHolder = enemy.transform.Find("Gun_holder");

    if(gunHolder == null)
    {
        Debug.LogWarning("GunHolder not found!");
        return;
    }

    // už existuje gun?
    if(gunHolder.childCount > 0)
    {
        gunInstance = gunHolder.GetChild(0).gameObject;
    }
    else
    {
        GameObject gunPrefab = GameObject.Find("Gun");

          

        gunInstance = Instantiate(gunPrefab, gunHolder);
        gunInstance.transform.localPosition = Vector3.zero;
        gunInstance.transform.localRotation = enemy.transform.rotation;
		gunInstance.transform.parent = gunHolder.transform;
		
		// RESET transformů (důležité!)
    gunInstance.transform.localPosition = Vector3.zero;
    gunInstance.transform.localRotation = Quaternion.Euler(0,0,90.0f);
    gunInstance.transform.localScale = new Vector3(0.2f, 1.0f, 0.2f);

    // 👉 pokud chceš offset (pravá / horní část)
    gunInstance.transform.localPosition = new Vector3(0.7f, 0f, 0f);
	IsPreparedToShoot = true;
    }

    gunMuzzle = gunInstance.transform.GetChild(0).gameObject;
}
public void Die()
{
    manager.OnEnemyDeath(gameObject, slotIndex);
    Destroy(gameObject);
}

    public void Init(Transform slot, int index, WallSlots w, EnemyManager m)
    {
        targetSlot = slot;
        slotIndex = index;
        wall = w;
        manager = m;
    }

  
	
	void ShootBeam(Vector3 dir)
{
    if(lr == null) return;

    RaycastHit hit;

    Vector3 origin = muzzle.transform.position;

    lr.enabled = true;
    lr.SetPosition(0, origin);

    if(Physics.Raycast(origin, dir, out hit, 200f))
    {
        lr.SetPosition(1, hit.point);

        if(hit.transform.CompareTag("Player"))
        {
            TankHealth hp = hit.transform.GetComponent<TankHealth>();
            if(hp != null)
            {
                hp.ApplyDamage(shootDamage);
            }
        }

        // po zásahu se paprsek „resetne“
        Destroy(lr.gameObject, 0.05f);
    }
    else
    {
        lr.SetPosition(1, origin + dir * 200f);
        lr.enabled = true;
		lr.SetPosition(0, origin);
		lr.SetPosition(1, hit.point);

		StartCoroutine(DisableLaser());
    }
	}	

	IEnumerator DisableLaser()
	{
    yield return new WaitForSeconds(0.05f);
    lr.enabled = false;
	}

    void MoveToTarget()
    {
        Vector3 dir = (targetSlot.position - transform.position).normalized;

        // 🔥 obstacle detection
        Ray ray = new Ray(transform.position + Vector3.up, dir);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 10f))
        {
            if(hit.collider.CompareTag("Wall") || hit.collider.CompareTag("building"))
            {
                AvoidObstacle(hit);
                return;
            }
        }

        transform.position += dir * speed * Time.deltaTime;
    }
	}