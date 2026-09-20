using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ChalengerBehaviour : MonoBehaviour
{

	public GameObject turningLogo;
	[Header("logo variables")]
	public float logoHeight = 80.0f;	
	public float logoRotationSpeed = 100.0f;
	[Header("machinegun variables")]
	public float mg_target_distance, mg_target_angle;

	[Header("Multiple Clicks Variables")]
	public GameObject clickWaypointOriginal;
	public GameObject WaypointsGenerated;
	public Vector3[] clickWaypointsGeneratedPosition;
	public int clickCount, currentIndex;
	public bool isClickedMultipleTimes;
	public string CursorName = "Cursor_X (1)";
	[Header("Basic Variables")]
	public GameObject tank, player, turret, main_gun, firePoint, machinegun;
	public GameObject cursor;
	public TMP_Text infoMessage;//for player
	public float player_distance;
	public float minimal_dist_to_enter_vehicle = 920.0f;
	public GameObject EnterButton;
	public bool isTankActivated;
	public Vector3 lockedPositionForNavigation;
	public float tank_height;
	//TRACKS VARIABLES
	public string trackObjectName = "track_piece_60";//this is original name on model, _0 > _60

	public GameObject leftTrackPoint;
	public GameObject rightTrackPoint;

	public GameObject trackMarkPrefab;

	public float trackSegmentDistance = 30.0f;
	private float trackDistanceCounter = 0.0f;

	private GameObject[] trackObjects;
	private int trackIndex = 0;
	public float tank_speed = 120.0f;
	Vector3 previousTankPosition;
	//starting variables for smoke & audio effects
	// AUDIO
public AudioSource engineAudio;
public AudioSource trackAudio;
public AudioSource turretAudio;

public AudioClip engineStartClip;
public AudioClip engineIdleClip;
public AudioClip trackMovementClip;
public AudioClip turretRotationClip;

// EXHAUST
public GameObject exhaustLeft;
public GameObject exhaustRight;

public ParticleSystem exhaustSmokeLeft;
public ParticleSystem exhaustSmokeRight;

public float exhaustSmokeDuration = 4.0f;

// ENGINE STATE
public bool engineStarted = false;
public bool engineStarting = false;

// UI TOGGLES
public Toggle mgToggle;
public Toggle cannonToggle;
public Toggle tankToggle;

// WEAPON / VEHICLE STATES
public bool IsCannonActivated;
	
public bool IsMG_aCTIVATED;//MACHINEGUN
    // Start is called before the first frame update
    void Start()
    {
		tank = GameObject.Find(this.name);
        player = GameObject.Find("WomanSniper");
		cursor = GameObject.Find(CursorName);
		tank_height = tank.transform.position.y;
		turningLogo = GameObject.Find("TurningLogo");
		
		VerifyChildSettings();
		clickWaypointsGeneratedPosition = new Vector3[100];
		
		previousTankPosition = tank.transform.position;

    if (turningLogo != null)
    {
        turningLogo.SetActive(false);
    }
	
	Camera.main.transform.GetComponent<CameraFollowsHero>().target = player.transform;
		Camera.main.transform.GetComponent<CameraFollowsHero>().player = player.transform;
		
		UpdateActivationToggles();
    }
	
	public void VerifyChildSettings()
	{
		List<GameObject> tracks = new List<GameObject>();

		foreach (Transform child in tank.GetComponentsInChildren<Transform>())
		{
			if (child.name.Contains(trackObjectName))
				{
					tracks.Add(child.gameObject);
				}
			
			
			if(child.name.Contains("hatch_behind_machinegun"))
			{
				machinegun = child.gameObject;
				if(machinegun.transform.GetComponent<TankMachinegunBehaviour>().IsMachinegunActivated==true)
				{
					IsMG_aCTIVATED = true;
				}
				else 
				{
					IsMG_aCTIVATED = false;
				}
			}
		}
		
		trackObjects = tracks.ToArray();
		
	}
	
	public void UpdateActivationToggles()
{
    // MACHINEGUN
    if (mgToggle != null)
    {
        mgToggle.isOn = IsMG_aCTIVATED;
    }

    // CANNON
    if (cannonToggle != null)
    {
        cannonToggle.isOn = IsCannonActivated;
    }

    // TANK
    if (tankToggle != null)
    {
        tankToggle.isOn = isTankActivated;
    }
}

    // Update is called once per frame
    void Update()
    {
        player_distance = Vector3.Distance(tank.transform.position, player.transform.position);
		float raycast_distance = 6000.0f;//we have big world
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		string defaultName = CursorName;
		if(Physics.Raycast(ray, out hit, raycast_distance))
		{
			if(hit.collider.gameObject.CompareTag("Enemy"))
			{
				CursorName = hit.collider.gameObject.name; //changes cursor name for the purpose of navigating machinegun to target
			}
			else
			{
				CursorName = defaultName; //else, if raycast won't hit enemy, cursor name goes back to default
			}
		}
		
		if(player_distance<750.0f)
		{
			isTankActivated = true;
			Camera.main.transform.GetComponent<CameraFollowsHero>().target = tank.transform;
		}
		
		if(player_distance<=minimal_dist_to_enter_vehicle)
		{
			EnterButton.SetActive(true);
			infoMessage.text = "Press E or Enter to onboard the vehicle.";
		}
		else
		{
			EnterButton.SetActive(false);
			infoMessage.text = "";
		}
		
		
		
		if(Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.E))
		{
			ActivateTank();
			infoMessage.text = "Tank " + this.name + "activated.";
		}
		
		float distanceMoved = Vector3.Distance(
			tank.transform.position,
			previousTankPosition
			);

		trackDistanceCounter += distanceMoved;
		previousTankPosition = tank.transform.position;

		if (trackDistanceCounter >= trackSegmentDistance)
		{
			CreateTrackMark();
			MoveTrackSegments();

			trackDistanceCounter = 0.0f;
		}
		
		GameObject FireballPrefab = GameObject.Find("WFX_Nuke");
		
		turret = tank.transform.Find("Challenger2/Turret/turret")?.gameObject;

		if (turret != null)
		{
			main_gun = turret.transform.Find("cannon")?.gameObject;

				if (main_gun != null)
				{
					firePoint = main_gun.transform.Find("muzzlePoint")?.gameObject;
				}
		}
		
		if(Input.GetKeyDown(KeyCode.M))
		{
			IsMG_aCTIVATED = true; IsCannonActivated = false;
			infoMessage.text = "You have activated machinegun by pressing M key.";
		}
		if(Input.GetKeyDown(KeyCode.K))
		{
			IsMG_aCTIVATED = false; IsCannonActivated = true;
				infoMessage.text = "You have activated machinegun by pressing K key.";
		}
		if(Input.GetKeyDown(KeyCode.E))
		{
			IsMG_aCTIVATED = false; IsCannonActivated = true;
				infoMessage.text = "You have activated main tank by pressing E key. Pres left Ctrl to rotate the turret";
		}
			
		if(isTankActivated==true)
		{
			MakePlayerInvisible();
			AttachCameraToVehicle();
			ManualTankMovement();
			
			if(turningLogo!=null)
			 turningLogo.SetActive(true);
			UpdateTurningLogo();
			UpdateTrackAudio();
			if(Input.GetKey(KeyCode.Space))
			{
				ShootFromMainGun();
			}
			
			if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftAlt))
			{
				if(turret!=null)
				{
					Camera.main.GetComponent<CameraFollowsHero>().target = turret.transform;
				}
			}
			else if(Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) && IsMG_aCTIVATED==false)//rotate turret only if ctrl is pressed
			RotateTurretToCursor(turret); //overloaded function
			else if(/*Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftAlt) &&*/ IsMG_aCTIVATED==true)//rotate machinegun only if ctrl is pressed
			RotateMachinegunToCursor(machinegun); //overloaded function
			else if(!Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftAlt))
			RotateTurretToCursor(tank); //overloaded function
			GameObject newPositionWaypoint = null;
			if(cursor!=null)
			{
				if(Input.GetMouseButtonDown(0) && clickCount==0)
				{
					isClickedMultipleTimes = false;//from higher click count will this change 
					lockedPositionForNavigation = cursor.transform.position;
					newPositionWaypoint = Instantiate(clickWaypointOriginal, lockedPositionForNavigation, Quaternion.identity);//creates a tile with circle on the place, as visual marker for navigation
					lockedPositionForNavigation.y = tank_height;
					clickCount++;
				}
				else if(Input.GetMouseButtonDown(0) && clickCount>=1)
				{
					lockedPositionForNavigation = cursor.transform.position;
					lockedPositionForNavigation.y = tank_height;
					clickWaypointsGeneratedPosition[clickCount] = lockedPositionForNavigation;
					clickWaypointsGenerated[clickCount] = Instantiate(clickWaypointOriginal, lockedPositionForNavigation, Quaternion.identity);//creates a tile with circle on the place, as visual marker for navigation
					clickCount++;
					isClickedMultipleTimes = true;
				}
			}
			if(lockedPositionForNavigation!=Vector3.zero && isClickedMultipleTimes==false)
			{
				
				tank.transform.position = Vector3.MoveTowards(tank.transform.position, lockedPositionForNavigation,tank_speed*Time.deltaTime);
				
				if(Vector3.Distance(tank.transform.position, lockedPositionForNavigation)<=5.0f)
				{
					lockedPositionForNavigation = Vector3.zero;//old nav point is eliminated
					Destroy(newPositionWaypoint);//we just passed it so it can be destroyed
				}
				else
				{
					TurnTankToDestonationPoint(lockedPositionForNavigation);
				}
			}
			else if(isClickedMultipleTimes==true)
			{
				//tank goes to path of waypoints
				Vector3 currentPosition = clickWaypointsGenerated[currentIndex];
				tank.transform.position = Vector3.MoveTowards(tank.transform.position, currentPosition, tank_speed*Time.deltaTime);//navigates tank to current selected waypoint in front of the game object
				//current position, basically player will click in front of tank, new waypoint will appear and tank will go to nearest waypoint, then next one, then next... till the end
				if(Vector3.Distance(tank.transform.position, currentPosition)<=5.0f && currentIndex<clickCount)
				{
					currentIndex++;//this moves tank to new position
					if(currentIndex-1>0)
					Destroy(WaypointsGenerated[currentIndex - 1]);//destroys last waypoint
				}
				
				
			}
				
			if(Input.GetMouseButtonDown(1))//put all to default
			{
				clickCount = 0;
				currentIndex = 0;
			}
			VerifyChildSettings();//check if machinegun is switched on
				RotateMachinegunToCursor(machinegun); //overloaded function
				UpdateActivationToggles();
		}
    }
	
	public void RotateMachinegunToCursor(GameObject machinegun_object)
	{
		cursor = GameObject.Find(CursorName);/// testing purposes
		if(cursor!=null && machinegun_object!=null)
		{
			mg_target_distance = Vector3.Distance(machinegun_object.transform.position, cursor.transform.position);
			float z_distance = Mathf.Abs(machinegun_object.transform.position.z - cursor.transform.position.z);
			float x_distance = Mathf.Abs(machinegun_object.transform.position.x - cursor.transform.position.x);
			mg_target_angle = Mathf.Acos(z_distance/mg_target_distance)*180/Mathf.PI;
			//turning machinegun to target
			if(cursor.transform.position.x<machinegun_object.transform.position.x && cursor.transform.position.z<machinegun_object.transform.position.z)
			machinegun_object.transform.rotation = Quaternion.Euler(0,mg_target_angle + 180,0);
			else if(cursor.transform.position.x>machinegun_object.transform.position.x && cursor.transform.position.z>machinegun_object.transform.position.z)
			machinegun_object.transform.rotation = Quaternion.Euler(0,mg_target_angle,0);
			else if(cursor.transform.position.x<machinegun_object.transform.position.x && cursor.transform.position.z>machinegun_object.transform.position.z)
			machinegun_object.transform.rotation = Quaternion.Euler(0,-mg_target_angle,0);
			else if(cursor.transform.position.x>machinegun_object.transform.position.x && cursor.transform.position.z<machinegun_object.transform.position.z)
			machinegun_object.transform.rotation = Quaternion.Euler(0,-mg_target_angle+180,0);
		}
	}
	public GameObject mg_firePoint, mg_fireMuzzle, mg_projectile, mg_muzzleFire, mg_generated_bullet;
	public Vector3 mg_projectile_target_position;
	public string explosion_prefab_string = "WFX_Explosion StarSmoke";
	public bool IsBulletTriggered;
	public float trigger_time = 0.05f;

	// čas, kdy smí proběhnout další výstřel
	private float nextFireTime = 0.0f;
	public void FireFromMachinegun()
{
    float firing_distance =
        Vector3.Distance(
            machinegun.transform.position,
            cursor.transform.position
        );

    // --------------------------------------------------
    // FIND FIREPOINT, PROJECTILE AND MUZZLE FIRE
    // --------------------------------------------------

    if (mg_firePoint == null)
    {
        // name firepoint:
        // "machinegun_firePoint"
        // bullet:
        // "bullet_Nagant3_enemy"
        // muzzle fire:
        // "WFX_MF 4P Rifle1"

        mg_projectile = GameObject.Find("bullet_Nagant3_enemy");
        mg_muzzleFire = GameObject.Find("WFX_MF 4P Rifle1");

        foreach (Transform child in
                 machinegun.GetComponentsInChildren<Transform>())
        {
            if (child.name.Contains("machinegun_firePoint"))
            {
                mg_firePoint = child.gameObject;
            }
        }
    }

    // --------------------------------------------------
    // FIRE COOLDOWN
    // --------------------------------------------------

    if (mg_firePoint != null &&
        IsBulletTriggered == true &&
        Time.time >= nextFireTime && Input.GetKey(Keycode.Space))
    {
        // ----------------------------------------------
        // MUZZLE FIRE
        // ----------------------------------------------

        GameObject generatedMuzzleFire =
            Instantiate(
                mg_muzzleFire,
                mg_firePoint.transform.position,
                mg_firePoint.transform.rotation
            );

        ParticleSystem ps =
            generatedMuzzleFire.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            ps.Play();
        }

        Destroy(generatedMuzzleFire, 3.0f);

        // ----------------------------------------------
        // CREATE PROJECTILE
        // ----------------------------------------------

        CreateMGProjectile(
            mg_firePoint.transform.position,
            mg_projectile,
            mg_firePoint,
            firing_distance
        );

        // ----------------------------------------------
        // NEXT ALLOWED SHOT
        // ----------------------------------------------

        nextFireTime = Time.time + trigger_time;

        // Reset trigger
        IsBulletTriggered = false;
    }

    // --------------------------------------------------
    // ARM THE NEXT SHOT
    // --------------------------------------------------

    if (Time.time >= nextFireTime)
    {
        IsBulletTriggered = true;
    }

    // --------------------------------------------------
    // DESTROY ENEMY BULLETS OUTSIDE FIRE RANGE
    // --------------------------------------------------

    GameObject[] allEnemyBullets =
        GameObject.FindGameObjectsWithTag("enemyBullet");

    foreach (GameObject go in allEnemyBullets)
    {
        if (Vector3.Distance(
                machinegun.transform.position,
                go.transform.position) > fire_range
            || go.transform.position.y < 16.07f)
        {
            ExplodeBullet(
                explosion_prefab_string,
                go.transform.position,
                go
            );
        }
    }
}
	
	public void ExplodeBullet(string prefb_explosion_name, Vector3 explosion_point, GameObject bullet_object)
	{
		GameObject explosionPrefab = GameObject.Find(prefb_explosion_name);
		GameObject generatedBulletExplosion = Instantiate(explosionPrefab, explosion_point, Quaternion.identity);
		
		ParticleSystem ps = generatedBulletExplosion.transform.GetComponent<ParticleSystem>();
		ps.Play();
		Destroy(generatedBulletExplosion,2.0f);
		Destroy(bullet_object, 2.0f);
	}
	
	public void CreateMGProjectile(Vector3 starting_position, GameObject bulletPrefab, GameObject firePoint_prefab, float fire_range)
	{
		if(mg_generated_bullet==null)
		{
			mg_generated_bullet = Instantiate(bulletPrefab, starting_position, firePoint_prefab.transform.rotation);
			
		}
		else if(mg_generated_bullet!=null)
		{
			float x_pos = starting_position.x + Mathf.Asin(firePoint_prefab.transform.eulerAngles.x*Rad2Deg)*fire_range;
			float y_pos = starting_position.x + Mathf.Asin(firePoint_prefab.transform.eulerAngles.x*Rad2Deg)*fire_range;
			float z_pos = starting_position.x + Mathf.Acos(firePoint_prefab.transform.eulerAngles.x*Rad2Deg)*fire_range;
			mg_projectile_target_position = new Vector3(x_pos, y_pos, z_pos);//projectile destination
			mg_generated_bullet.transform.GetComponent<BulletPrefab>().targetPoint = mg_projectile_target_position;
		}
	}
	
	public void StartTankEngine()
{
    if (engineStarted || engineStarting)
        return;

    engineStarting = true;

    if (engineAudio != null && engineStartClip != null)
    {
        engineAudio.Stop();
        engineAudio.clip = engineStartClip;
        engineAudio.loop = false;
        engineAudio.Play();
    }

    StartCoroutine(FinishEngineStart());
}

private IEnumerator FinishEngineStart()
{
    yield return new WaitForSeconds(2.5f);

    engineStarted = true;
    engineStarting = false;

    if (engineAudio != null && engineIdleClip != null)
    {
        engineAudio.clip = engineIdleClip;
        engineAudio.loop = true;
        engineAudio.Play();
    }

    StartExhaustSmoke();
}

public void StartExhaustSmoke()
{
    if (exhaustSmokeLeft != null)
    {
        exhaustSmokeLeft.gameObject.SetActive(true);
        exhaustSmokeLeft.Play();
    }

    if (exhaustSmokeRight != null)
    {
        exhaustSmokeRight.gameObject.SetActive(true);
        exhaustSmokeRight.Play();
    }
}

public void StopExhaustSmoke()
{
    if (exhaustSmokeLeft != null)
    {
        exhaustSmokeLeft.Stop();
        exhaustSmokeLeft.gameObject.SetActive(false);
    }

    if (exhaustSmokeRight != null)
    {
        exhaustSmokeRight.Stop();
        exhaustSmokeRight.gameObject.SetActive(false);
    }
}

public void UpdateTrackAudio()
{
    if (trackAudio == null)
        return;

    bool moving =
        Input.GetKey(KeyCode.W) ||
        Input.GetKey(KeyCode.S) ||
        lockedPositionForNavigation != Vector3.zero;

    if (moving && engineStarted)
    {
        if (!trackAudio.isPlaying)
        {
            trackAudio.loop = true;
            trackAudio.clip = trackMovementClip;
            trackAudio.Play();
        }

        trackAudio.volume = 1.0f;
    }
    else
    {
        if (trackAudio.isPlaying)
        {
            trackAudio.Stop();
        }
    }
}
	
	public void MoveTrackSegments()
	{
    if (trackObjects == null || trackObjects.Length == 0)
        return;

    trackIndex++;

    if (trackIndex >= trackObjects.Length)
        trackIndex = 0;

    GameObject segment = trackObjects[trackIndex];

    // zde budeš segmentu nastavovat novou pozici
	}
	
	public void CreateTrackMark()
{
    if (leftTrackPoint != null)
    {
        Instantiate(
            trackMarkPrefab,
            leftTrackPoint.transform.position,
            leftTrackPoint.transform.rotation
        );
    }

    if (rightTrackPoint != null)
    {
        Instantiate(
            trackMarkPrefab,
            rightTrackPoint.transform.position,
            rightTrackPoint.transform.rotation
        );
    }
}
	
	public void TurnTankToDestonationPoint(Vector3 destination)
{
    Vector3 direction = destination - tank.transform.position;

    direction.y = 0.0f;

    if (direction.sqrMagnitude < 0.001f)
        return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);

    tank.transform.rotation = Quaternion.RotateTowards(
        tank.transform.rotation,
        targetRotation,
        90.0f * Time.deltaTime
    );
}
	
	public void UpdateTurningLogo()
{
    if (turningLogo == null || tank == null)
        return;
	logoHeight = 900.0f;
    // Logo sleduje tank v X a Z,
    // ale Y zůstává vždy stejné.
	if(isTankActivated==true)
    turningLogo.transform.position = new Vector3(
        tank.transform.position.x,
        logoHeight,
        tank.transform.position.z
    );
	else
	{
		 turningLogo.transform.position = new Vector3(
        player.transform.position.x,
        logoHeight,
        player.transform.position.z
    );
	}

    // Neustálé otáčení kolem Y
    turningLogo.transform.Rotate(
		0.0f,
        logoRotationSpeed * Time.deltaTime,	
        0.0f
    );
}
	
	public void RotateTurretToCursor(GameObject rotated_object)
{
    if (turret == null || cursor == null)
        return;

    Vector3 direction = cursor.transform.position - rotated_object.transform.position;

    // Ignorujeme výšku, aby se turret nenakláněl nahoru/dolů
    direction.y = 0.0f;

    if (direction.sqrMagnitude < 0.001f)
        return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);
	//turns whole turret using machinegun formula
	mg_target_distance = Vector3.Distance(rotated_object.transform.position, cursor.transform.position);
			float z_distance = Mathf.Abs(rotated_object.transform.position.z - cursor.transform.position.z);
			float x_distance = Mathf.Abs(rotated_object.transform.position.x - cursor.transform.position.x);
			mg_target_angle = Mathf.Acos(z_distance/mg_target_distance)*180/Mathf.PI;
			//turning machinegun to target
			if(cursor.transform.position.x<rotated_object.transform.position.x && cursor.transform.position.z<rotated_object.transform.position.z)
			rotated_object.transform.rotation = Quaternion.Euler(0,mg_target_angle + 180,0);
			else if(cursor.transform.position.x>rotated_object.transform.position.x && cursor.transform.position.z>rotated_object.transform.position.z)
			rotated_object.transform.rotation = Quaternion.Euler(0,mg_target_angle,0);
			else if(cursor.transform.position.x<rotated_object.transform.position.x && cursor.transform.position.z>rotated_object.transform.position.z)
			rotated_object.transform.rotation = Quaternion.Euler(0,-mg_target_angle,0);
			else if(cursor.transform.position.x>rotated_object.transform.position.x && cursor.transform.position.z<rotated_object.transform.position.z)
			rotated_object.transform.rotation = Quaternion.Euler(0,-mg_target_angle+180,0);
	/*
    rotated_object.transform.rotation = Quaternion.Euler(
        0.0f,
        targetRotation.eulerAngles.y,
        0.0f
    );*/
	//Quaternion targetRotation = Quaternion.LookRotation(-direction);//pro pripad ze je vez obracene
	
	float angleDifference = Quaternion.Angle(
    rotated_object.transform.rotation,
    targetRotation
);
if(rotated_object==turret)
{
if (angleDifference > 1.0f)
{
    if (turretAudio != null && turretRotationClip != null)
    {
        if (!turretAudio.isPlaying)
        {
            turretAudio.clip = turretRotationClip;
            turretAudio.loop = true;
            turretAudio.Play();
        }
    }
}
else
{
    if (turretAudio != null && turretAudio.isPlaying)
    {
        turretAudio.Stop();
    }
}
}
}
	//primy hit RPG -> DORCHESTER PLATE ARMOUR SURVIVED AGAIN.DIRECT HIT. DORCHESTER ARMOUR SURVIVED AGAIN.
	public void ActivateTank()
{
    isTankActivated = true;

    if (player != null)
    {
        MakePlayerInvisible();
        Camera.main.transform
            .GetComponent<CameraFollowsHero>()
            .target = tank.transform;
    }

    StartTankEngine();
	UpdateActivationToggles();
	//„Your Challenger 2 just catched first breath and burned…“ :D
	infoMessage.text = "Your Challenger 2 has just taken its first breath. The burnt oil from its mighty V12 turbocarburator now fills the room. Do you feel the power in the air?";
	
	
}
	

public void MakePlayerInvisible()
{
    if (player != null)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        GameObject healthBarCanvas = player.transform.Find("healthBarCanvas")?.gameObject;

        if (healthBarCanvas != null)
        {
            healthBarCanvas.SetActive(false);
        }
    }
}


	public void AttachCameraToVehicle()
	{
	}
	
	public void ManualTankMovement()
	{
		float rotation_speed = 0.5f;
		float movement_speed = 20.0f;
		if(Input.GetKey(KeyCode.A))
		{
			//turn left
			tank.transform.Rotate(0, -rotation_speed, 0);
		}
		if(Input.GetKey(KeyCode.D))
		{
			//turn left
			tank.transform.Rotate(0, rotation_speed, 0);
		}
		if(Input.GetKey(KeyCode.W))
		{
			//turn left
			tank.transform.Translate(0, 0, movement_speed);
		}
		if(Input.GetKey(KeyCode.S))
		{
			//turn left
			tank.transform.Translate(0, 0, -movement_speed);
		}
	}
	
	public void ShootFromMainGun()
	{
		GameObject FireballPrefab = GameObject.Find("WFX_Nuke");
		if(turret==null || main_gun==null ||firePoint==null)
		{
		turret = tank.transform.Find("Challenger2/Turret/turret")?.gameObject;

		if (turret != null)
			{
				main_gun = turret.transform.Find("cannon")?.gameObject;

				if (main_gun != null)
				{
					firePoint = main_gun.transform.Find("muzzlePoint")?.gameObject;
				}
			}
		}
		else
		{
			if(Input.GetKeyDown(KeyCode.Space) && IsMG_aCTIVATED==false)
			{
				if(firePoint!=null)
				{
					GameObject generatedMuzzleFire = Instantiate(FireballPrefab, firePoint.transform.position, firePoint.transform.rotation);
					ParticleSystem ps = generatedMuzzleFire.transform.GetComponent<ParticleSystem>();
					ps.Play();
					Destroy(generatedMuzzleFire, 3.0f);
					CreateBallProjectile(firePoint.transform.position);
				}
			}
			else
			{
				FireFromMachinegun();
			}
		}
	}
	
	public void CreateBallProjectile(Vector3 projectile_start_point)
{
    GameObject projectilePrefab = GameObject.Find("TankProjectile");

    if (projectilePrefab == null)
    {
        Debug.LogError("TankProjectile was not found!");
        return;
    }

    if (firePoint == null)
    {
        Debug.LogError("FirePoint was not found!");
        return;
    }

    // Směr hlavně
    Vector3 projectile_direction = firePoint.transform.forward;

    // Bod 3000 jednotek před hlavní
    Vector3 destination_point =
        projectile_start_point + projectile_direction * 3000.0f;

    // Vytvoření projektilu na muzzlePoint
    GameObject generatedProjectile = Instantiate(
        projectilePrefab,
        projectile_start_point,
        firePoint.transform.rotation
    );

    // Spustíme let projektilu
    StartCoroutine(
        MoveProjectile(
            generatedProjectile,
            destination_point,
            1200.0f
        )
    );
}

private IEnumerator MoveProjectile(
    GameObject projectile,
    Vector3 destination_point,
    float projectile_speed)
{
    while (projectile != null)
    {
        projectile.transform.position = Vector3.MoveTowards(
            projectile.transform.position,
            destination_point,
            projectile_speed * Time.deltaTime
        );

        if (Vector3.Distance(
            projectile.transform.position,
            destination_point) < 0.1f)
        {
            Destroy(projectile);
            yield break;
        }

        yield return null;
    }
}
}
