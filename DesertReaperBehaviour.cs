using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DesertReaperBehaviour : MonoBehaviour
{
	public bool IsDead, IsChosen, IsTandemScriptActivated;
    public GameObject player;
    public float MovementSpeed = 25.0f;
	public Vector3 newPlayerDestination;
    public Transform GunHolder;
    public GameObject SelectedWeaponPrefab; // prefab zbraně (např. AKM)
	public string WeaponName = "AKM";
    private Vector3 hit_point;
	
	[Header("Weapons")]
	public Transform FirePoint;

	[Header("Rifle")]
	public LineRenderer bulletTrail;
	public float rifleRange = 200f;

	[Header("Grenade")]
	public GameObject grenadePrefab;
	public float grenadeForce = 20f;
	public float explosionRadius = 5f;
	public float explosionForce = 500f;
	public ParticleSystem explosionEffect;
	public AudioSource explosionAudio;
	
	[Header("UI")]
	public Button RifleButton;
	public Button GrenadeButton;

	public Image RifleImage;
	public Image GrenadeImage;
	
	[Header("Tandem Follow")]
	public Transform FollowTarget;   // Mia1 transform
	public float FollowDistance = 5f; // vzdálenost vzad
	public float PatrolAmplitude = 2f; // šířka pohybu ze strany na stranu
	public float PatrolSpeed = 2f;
	public float ShootingCooldown = 2f;

	private float patrolTimer = 0f;
	private float shootTimer = 0f;
	
	[Header("Tandem GUI")]
	public LineRenderer TandemCirclePrefab;
	private LineRenderer circleInstance;
	public float CircleRadius = 12f;
	public int CircleSegments = 36;
	
	[Header("Helicopter Detection")]
public TMPro.TextMeshProUGUI InfoText;
public GameObject detected_object;
private float lastClickTime = 0f;
public float doubleClickTime = 0.3f;
public GameObject helicopter;
	//public Color SelectedColor = Color.yellow;
	public Color DefaultColor = Color.black;
	public Color SelectedColor = new Color(1f, 0.5f, 0f);
	public enum WeaponType
	{
    Rifle,
    GrenadeLauncher,
    Pistole
	}
	
	private bool hasInitialized = false;
	
	public bool ActivatedToUseHelicopter;

public void SetInitialTandemPosition()
{
    if (FollowTarget == null) return;

    Vector3 backwardOffset = -FollowTarget.forward * FollowDistance;
    Vector3 diagonalOffset = (FollowTarget.right + FollowTarget.forward).normalized * 0.5f * FollowDistance;
    player.transform.position = FollowTarget.position + backwardOffset + diagonalOffset;

    patrolTimer = 0f;  // reset timerů
    shootTimer = 0f;
}
	
	void UpdateUIState()
{
    RifleButton.interactable = IsChosen;
    GrenadeButton.interactable = IsChosen;

    if (!IsChosen)
    {
        RifleImage.color = DefaultColor;
        GrenadeImage.color = DefaultColor;
    }
}
	public WeaponType CurrentWeapon;
    // Start is called before the first frame update
    void Start()
    {
        player = gameObject;
		SelectedWeaponPrefab = GameObject.Find(WeaponName);
        // default weapon (nastav v inspectoru na AKM prefab)
        if (GunHolder.childCount == 0 && SelectedWeaponPrefab != null)
        {
            Instantiate(SelectedWeaponPrefab, GunHolder.position, GunHolder.rotation, GunHolder);
        }
		
		if (IsTandemScriptActivated && circleInstance == null && TandemCirclePrefab != null)
		{
			circleInstance = Instantiate(TandemCirclePrefab, player.transform.position, Quaternion.identity);
			circleInstance.positionCount = CircleSegments + 1;
		}
    }

    // Update is called once per frame
    void Update()
    {
        // Ray z kamery na myš
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2000))
        {
            hit_point = hit.point;
        }
		if(IsChosen==true)
			Camera.main.transform.GetComponent<CameraFollowsHero>().mainHero = player.transform;//this will set up the camera to stick behind main player 
		TurnPlayerToCursor();
		
		if(Input.GetKeyDown(KeyCode.Space))
		{
			ShootBullet();
		}
		RaycastForward();
        if (Input.GetKey(KeyCode.LeftControl))
        {
            

            // pokud nemá zbraň → přidej
            if (GunHolder.childCount == 0 && SelectedWeaponPrefab != null)
            {
                Instantiate(SelectedWeaponPrefab, GunHolder.position, GunHolder.rotation, GunHolder);
            }

            // forward raycast + debug line
            Ray forwardRay = new Ray(transform.position, transform.forward);
            Debug.DrawLine(transform.position, transform.position + transform.forward * 200.0f, Color.red);
        }
		
		if (Input.GetKeyDown(KeyCode.Space))
		{
			FireWeapon();
		}
		/*
		UpdateUIState();
		if(IsTandemScriptActivated)
		{
			if (IsTandemScriptActivated && FollowTarget != null)
					TandemFollowBehavior();
		}
	
		if (IsTandemScriptActivated && !hasInitialized)
		{
			SetInitialTandemPosition();
			hasInitialized = true;
		}

	if (IsTandemScriptActivated && circleInstance == null && TandemCirclePrefab != null)
		{
			circleInstance = Instantiate(TandemCirclePrefab, player.transform.position, Quaternion.identity);
			circleInstance.positionCount = CircleSegments + 1;
		}
		
		if (IsTandemScriptActivated && circleInstance != null)
	{
    float deltaAngle = 360f / CircleSegments;
    for (int i = 0; i <= CircleSegments; i++)
    {
        float angle = Mathf.Deg2Rad * deltaAngle * i;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * CircleRadius, 0.05f, Mathf.Sin(angle) * CircleRadius);
        circleInstance.SetPosition(i, player.transform.position + offset);
    }
	}
	
	
	
	if (!IsTandemScriptActivated && circleInstance != null)
		{
			Destroy(circleInstance.gameObject);
		}
		*/
		//AimToImportantGameObject();
		
		if(ActivatedToUseHelicopter==true)
		NavigatePlayerToKeyObject();
	
		HandleDoubleClick();

    // pohyb k cíli
    if (isMovingToTarget)
    {
        player.transform.position = Vector3.MoveTowards(player.transform.position, targetPoint, moveSpeed * Time.deltaTime);

        // plynulé otáčení k cíli
        Vector3 moveDirection = (targetPoint - player.transform.position).normalized;
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(moveDirection.x, 0, moveDirection.z));
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRot, 0.05f);
        }
		PlayMovingAnimation();
        // zastavení, pokud jsme u cíle
        if (Vector3.Distance(player.transform.position, targetPoint) < 0.1f)
            isMovingToTarget = false;
    }
	else
	{
		PlayIdleAnimation();
	}
    }
	
	public void PlayMovingAnimation()
	{
		Animator animator = player.transform.GetComponent<Animator>();
		animator.SetInteger("IsReaperRunsFast",1);
		animator.Play("ReaperRunsFast");
	}
	
	public void PlayIdleAnimation()
	{
		Animator animator = player.transform.GetComponent<Animator>();
		animator.SetInteger("IsReaperGunplayShooting",1);
		animator.Play("ReaperGunplayShoot");
	}
	
	void HandleDoubleClick()
{
	flag = GameObject.Find("Flag-float.fbx (1)");
    if (Input.GetMouseButtonDown(0))
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickTime)
        {
            // DVOJITÝ KLIK DETEKOVÁN
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000f))
            {
                targetPoint = hit.point;
				flag.transform.position = targetPoint;//ukazatel, marker
                isMovingToTarget = true;
            }
        }
        lastClickTime = Time.time;
    }
}
//	float lastClickTime = 0f;
	//float doubleClickTime = 0.3f;
	public float movementSpeed = 25.0f;
	public void NavigatePlayerToKeyObject()
{
//    AimToImportantGameObject();

    bool IsEnemyNear = CheckDistanceFromNearestEnemy();

    if (detected_object != null && IsEnemyNear == false)
    {
        if (Input.GetMouseButtonDown(0))
        {
            // kontrola dvojkliku
            if (Time.time - lastClickTime < doubleClickTime)
            {
                Debug.Log("DOUBLE CLICK - aktivuji objekt");
				newPlayerDestination = detected_object.transform.position;
                // tady aktivuj helikoptéru nebo jiný objekt
                detected_object.GetComponent<ActivateHelicopter>().ActivateObject();
            }

            lastClickTime = Time.time;
        }
    }
	else
	{
		if (detected_object == null && IsEnemyNear == false) //we clicked to free space
    {
        if (Input.GetMouseButtonDown(0))
        {
            // kontrola dvojkliku
            if (Time.time - lastClickTime < doubleClickTime)
            {
                Debug.Log("DOUBLE CLICK - aktivuji objekt");
				newPlayerDestination = detected_object.transform.position;
             
            }

            lastClickTime = Time.time;
        }
    }
	}
	
	if(newPlayerDestination!=Vector3.zero)
	{
		player.transform.position = Vector3.MoveTowards(player.transform.position, newPlayerDestination, movementSpeed*Time.deltaTime);
	}
	
	helicopter = GameObject.Find("MH-60L");
	if(ActivatedToUseHelicopter==true && helicopter!=null)//activated from CursorKillsEnemyWarrior, where mouse detects nearby object, click on it, activated bool here and navigates player to clicked object
	{
		player.transform.position = Vector3.MoveTowards(
    player.transform.position,
    helicopter.transform.position,
    movementSpeed * Time.deltaTime
	);
	}
}
	public bool CheckDistanceFromNearestEnemy()
	{
    bool IsNearToEnemy = false;
    float min_distance = 30.0f;

    GameObject cursor = GameObject.Find("Quad");
    allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

    foreach (GameObject go in allEnemies)
    {
        if (Vector3.Distance(go.transform.position, cursor.transform.position) < min_distance)
        {
            IsNearToEnemy = true;
            break;
        }
    }

    return IsNearToEnemy;
	}
	public void AimToImportantGameObject()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 2000f))
    {
        GameObject obj = hit.collider.gameObject;

        // kontrola tagu nebo názvu
        if (obj.CompareTag("Helicopter") || obj.name.Contains("MH-60L"))
        {
            detected_object = obj;

            // výpis do TMP textu
            if (InfoText != null)
                InfoText.text = "You have found " + detected_object.name;

            // DOUBLE CLICK
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastClickTime < doubleClickTime)
                {
                    IsChosen = true;
					InfoText.text = "Do you want to capture this (" + detected_object.name + ") helicopter ?";
                }

                lastClickTime = Time.time;
            }
        }
    }

    // pokud je zvolená helikoptéra → jdeme k ní
    if (IsChosen && detected_object != null)
    {
        Vector3 dir = detected_object.transform.position - player.transform.position;
        dir.y = 0f;

        float distance = dir.magnitude;

        // zastavíme se 10 jednotek před ní
        if (distance > 10f)
        {
            Vector3 targetPos = detected_object.transform.position - dir.normalized * 10f;

            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                targetPos,
                MovementSpeed * Time.deltaTime
            );

            // otočení směrem k helikoptéře
            Quaternion rot = Quaternion.LookRotation(dir);
           // player.transform.rotation = Quaternion.Lerp(player.transform.rotation, rot, Time.deltaTime * 5f);
        }
    }
}
	
	public void SelectRifle()
{
    if (!IsChosen) return;

    CurrentWeapon = WeaponType.Rifle;
    UpdateWeaponUI();
}

public void SelectGrenade()
{
    if (!IsChosen) return;

    CurrentWeapon = WeaponType.GrenadeLauncher;
    UpdateWeaponUI();
}

void FireRifle()
{
    // jednoduchý Raycast střelby
    Ray ray = new Ray(FirePoint.position, FirePoint.forward);
    if (Physics.Raycast(ray, out RaycastHit hit, rifleRange))
    {
        Debug.DrawLine(FirePoint.position, hit.point, Color.red, 1f);
        // případně poškodit enemy
    }

    if (bulletTrail != null)
    {
        bulletTrail.SetPosition(0, FirePoint.position);
        bulletTrail.SetPosition(1, FirePoint.position + FirePoint.forward * rifleRange);
    }
}

void UpdateWeaponUI()
{
    RifleImage.color = (CurrentWeapon == WeaponType.Rifle) ? SelectedColor : DefaultColor;
    GrenadeImage.color = (CurrentWeapon == WeaponType.GrenadeLauncher) ? SelectedColor : DefaultColor;
}
	void FireWeapon()
	{
    switch (CurrentWeapon)
    {
        case WeaponType.Rifle:
            FireRifle();
            break;

        case WeaponType.GrenadeLauncher:
            FireGrenade();
            break;

        case WeaponType.Pistole:
            // zatím prázdné
            break;
    }
	}
	
	
	
	void FireGrenade()
	{
    GameObject grenade = Instantiate(grenadePrefab, FirePoint.position, Quaternion.identity);

    Rigidbody rb = grenade.GetComponent<Rigidbody>();

    Vector3 direction = (hit_point - FirePoint.position);
    direction.y += 1.5f; // oblouk

    rb.AddForce(direction.normalized * grenadeForce, ForceMode.Impulse);

    // přidej skript na granát (viz níže)
    grenade.GetComponent<Grenade>().Init(this);
	}
	
	
    void MovePlayerToClickedPosition(Vector3 DestinationPoint)
    {
        if (DestinationPoint != Vector3.zero)
        {
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                DestinationPoint,
                MovementSpeed * Time.deltaTime
            );
        }
    }
	public float dist_sqrt, player_angle, x_dist, z_dist;
public GameObject flag, gun, gun_holder, generated_gun;
public ParticleSystem prefabMuzzleFlash, generatedMuzzleFlash;

public GameObject muzzlePoint, child, parent_object;        // konec hlavně

public string GunName = "AKM_";
public Vector3 hit_point2;
public LayerMask groundMask;     // nastav v Inspectoru na Terrain/Ground
public Vector3 raycastGroundPosition;   // konec žluté čáry na zemi

public float forwardRayDistance = 200f;
public LayerMask shootMask;   // můžeš nechat prázdné = trefí všechno

public GameObject bullet, cursor;
public GameObject generated_bullet;

public int selected_munition = 1;   // 1 = kulka (default), 2 = granát

public float bulletSpeed = 300f;
//public float grenadeForce = 25f;
public float grenadeHeight = 8f;
// přidej do třídy
public GameObject currentBulletTrail;
public LineRenderer currentLineRenderer, lastLineRenderer;
public float trailMaxLength = 50f;

public int bullet_trail_counter;

// --- DVOJITÝ KLIK PRO PŘIBLÍŽENÍ K CÍLI ---
//private float lastClickTime = 0f; //uz definovane nahore
//private float doubleClickTime = 0.3f; // max čas mezi kliky - uz definovane jinde
public Vector3 targetPoint;          // místo, kam se přiblíží hráč
public bool isMovingToTarget = false;
public float moveSpeed = 10f;
public void TurnPlayerToCursor()
{
	parent_object = GameObject.Find("DesertReaper_parent");
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
	GameObject cursor = GameObject.Find("Quad");
    // najdeme flag jen jednou (je to rychlejší než pořád Find)
    if(flag == null)
        flag = GameObject.Find("Flag-float.fbx (1)");

    // ŽLUTÝ RAY Z KAMERY
    Debug.DrawRay(ray.origin, ray.direction * 5000f, Color.yellow);

    

    // hledáme průnik se zemí
    if (Physics.Raycast(ray, out hit, 5000f, groundMask))
    {
        hit_point2 = hit.point;
		// TADY získáš konec žluté čáry
        raycastGroundPosition = hit.point;

        // jen pro kontrolu (můžeš pak smazat)
        Debug.Log(raycastGroundPosition);

        // VLajka se přesune jen při kliknutí
        if (Input.GetMouseButtonDown(0))
        {
            flag.transform.position = raycastGroundPosition;
        }
		else
		{
			cursor.transform.position = raycastGroundPosition;
		}
		
    }

    Vector3 direction = flag.transform.position - player.transform.position;
    direction.y = 0f;

    if (cursor!=null) //puvodni funkce, silene otaceni
    {
		flag = cursor;//test
        dist_sqrt = Vector3.Distance(player.transform.position, flag.transform.position);

        x_dist = Mathf.Abs(player.transform.position.x - flag.transform.position.x);
        z_dist = Mathf.Abs(player.transform.position.z - flag.transform.position.z);

        player_angle = Mathf.Asin(x_dist / dist_sqrt) * 180 / Mathf.PI;

       /* if (flag.transform.position.z < player.transform.position.z)
        {
            if (flag.transform.position.x > player.transform.position.x)
                player.transform.rotation = Quaternion.Euler(0, -player_angle + 180, 0);
            else
                player.transform.rotation = Quaternion.Euler(0, player_angle + 180, 0);
        }
        else
        {
            if (flag.transform.position.x > player.transform.position.x)
                player.transform.rotation = Quaternion.Euler(0, player_angle, 0);
            else
                player.transform.rotation = Quaternion.Euler(0, -player_angle, 0);
        }*/
    }
	
	// --- OTÁČENÍ HRÁČE PLYNULE ---
if (cursor != null)
{
    flag = cursor; // test
    dist_sqrt = Vector3.Distance(player.transform.position, flag.transform.position);

   direction = (flag.transform.position - player.transform.position).normalized;
    Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

    // plynulé otáčení s Lerp, 0.05f je rychlost
    player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRotation, 0.05f);
}
	
	gun_holder = player.transform.GetChild(2).gameObject;
	//if(parent_object!=null)
	 //generated_gun.transform.localPosition = parent_object.transform.position;
	if(gun_holder.transform.childCount == 0)
{
    generated_gun = Instantiate(gun, parent_object.transform.position, gun_holder.transform.rotation);
    generated_gun.transform.localPosition = gun_holder.transform.position;
    generated_gun.transform.localRotation = Quaternion.identity;
	generated_gun.transform.parent = gun_holder.transform;
	generated_gun.transform.localScale = new Vector3(0.004288562f, 0.004288562f, 0.004288562f);
    // vytvoření bodu na konci hlavně
    GameObject mp = new GameObject("MuzzlePoint");
    mp.transform.parent = generated_gun.transform;
    mp.transform.localPosition = new Vector3(0f, 0f, 0.6f); // upravíš podle modelu
    mp.transform.localRotation = Quaternion.identity;

    muzzlePoint = mp;
}
else if(gun_holder.transform.childCount > 0)
{
	 //child = player.transform.GetChild(1).gameObject;
	 generated_gun.transform.position = new Vector3(parent_object.transform.position.x + 0.00366f,parent_object.transform.position.y + 15.0f,parent_object.transform.position.z + 5.0f);
    generated_gun.transform.localRotation = Quaternion.identity;
	generated_gun.transform.localScale = new Vector3(0.004288562f, 0.004288562f, 0.004288562f);
}
/*
if( Input.GetKey("Fire"))
{
    if(muzzlePoint != null)
    {
        generatedMuzzleFlash = Instantiate(prefabMuzzleFlash,
            muzzlePoint.transform.position,
            muzzlePoint.transform.rotation);

        generatedMuzzleFlash.Play();
    }
}
*/
Animator anim = player.transform.GetComponent<Animator>();
anim.Play("ReaperGunplayShooting 0");
bullet = GameObject.Find("bullet");
if (Input.GetKeyDown(KeyCode.Space))
{
    ShootBullet();
}
}

public void ShootBullet()
{
	cursor = GameObject.Find("Quad");
    /*if (generated_gun == null)
        return;*/

    // vytvoření střely v hlavni
    generated_bullet = Instantiate(
        bullet,
        gun_holder.transform.position,
       gun_holder.transform.rotation
    );

    // přidání rigidbody automaticky
  

    //---------------------------------------------------
    // 1 = klasická kulka (letí rovně)
    //---------------------------------------------------
   
	
	if (selected_munition == 1)
    {
        // Nastavení cíle
        Vector3 target = cursor.transform.position;

        generated_bullet.transform.GetComponent<BulletBehaviour>().targetPoint = cursor.transform.position;//kulka leti do mista posledniho vyskytu kurzoru pri stisku mezerniku
       // rb.velocity = gun_holder.transform.forward * bulletSpeed;
	   generated_bullet.transform.position = Vector3.MoveTowards(generated_bullet.transform.position, cursor.transform.position, bulletSpeed*Time.deltaTime);
    
	 // --- Particle System pro Muzzle Flash ---
    if (muzzlePoint != null && prefabMuzzleFlash != null)
{
    // Vytvoření muzzle flash s posunem o 0.5f dopředu
    Vector3 spawnPosition = muzzlePoint.transform.position + muzzlePoint.transform.forward * 0.5f;

    // Otáčení: původní rotace + 90 stupňů kolem lokální osy X (např.)
    Quaternion spawnRotation = muzzlePoint.transform.rotation * Quaternion.Euler(0f, 90f, 0f);

    generatedMuzzleFlash = Instantiate(
        prefabMuzzleFlash,
        spawnPosition,
        spawnRotation
    );
    generatedMuzzleFlash.Play();

    // automatické zničení po době trvání
    Destroy(generatedMuzzleFlash.gameObject, generatedMuzzleFlash.main.duration);
}
		if(currentBulletTrail!=null )
		{
			Destroy(currentBulletTrail);//cleaning
			bullet_trail_counter = 0;
		}
        // Vytvoření trailu
        currentBulletTrail = new GameObject("BulletTrail" + bullet_trail_counter.ToString());
        currentLineRenderer = currentBulletTrail.AddComponent<LineRenderer>();
        currentLineRenderer.positionCount = 2;
		currentLineRenderer.startWidth = 1f;
		currentLineRenderer.endWidth = 1f;
        currentLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        currentLineRenderer.startColor = Color.yellow;
        currentLineRenderer.endColor = Color.red;
		bullet_trail_counter++;
        // spustíme korutinu pro pohyb kulky
        StartCoroutine(MoveBulletWithTrail(generated_bullet, target, bulletSpeed));
    }

    //---------------------------------------------------
    // 2 = granát (letí na kurzor)
    //---------------------------------------------------
    if (selected_munition == 2)
    { 
Rigidbody rb = generated_bullet.GetComponent<Rigidbody>();
    if (rb == null)
        rb = generated_bullet.AddComponent<Rigidbody>();
        rb.useGravity = true;

        Vector3 target = generated_gun.transform.position;

        Vector3 direction = target - muzzlePoint.transform.position;

        // přidáme výšku aby to letělo obloukem
        direction.y += grenadeHeight;

        rb.velocity = direction.normalized * grenadeForce;
    }
}

// korutina pro pohyb kulky s trail
IEnumerator MoveBulletWithTrail(GameObject bulletObj, Vector3 targetPos, float speed)
{
    while (bulletObj != null && Vector3.Distance(bulletObj.transform.position, targetPos) > 0.1f)
    {
        bulletObj.transform.position = Vector3.MoveTowards(bulletObj.transform.position, targetPos, speed * Time.deltaTime);

        // aktualizace trailu
        Vector3 dir = bulletObj.transform.position - player.transform.position;
        float len = Mathf.Min(dir.magnitude, trailMaxLength);
        Vector3 startPos = bulletObj.transform.position - dir.normalized * len;

        if (currentLineRenderer != null)
        {
            currentLineRenderer.SetPosition(0, startPos);
            currentLineRenderer.SetPosition(1, bulletObj.transform.position);
        }

        yield return null;
    }

    // dopad kulky – znič kulku i trail
    if (bulletObj != null)
        Destroy(bulletObj);
    if (currentBulletTrail != null)
	{
        Destroy(currentBulletTrail);
		Destroy(currentLineRenderer);
	}
	
	
}
public string lastColliderName;

public void RaycastForward()
{
    forwardRayDistance = 500.0f;

    Ray ray = new Ray(generated_gun.transform.position, generated_gun.transform.forward);
    RaycastHit hit;

    Debug.DrawRay(ray.origin, ray.direction * forwardRayDistance, Color.red);

    if (Physics.Raycast(ray, out hit, forwardRayDistance))
    {
        Debug.Log("Zásah: " + hit.collider.name);

        lastColliderName = hit.collider.name;
     

        Debug.DrawLine(muzzlePoint.transform.position, hit.point, Color.green);
    }
	if(Input.GetKey(KeyCode.Space))//az po vystrelu
	{
		   HitEnemy(lastColliderName);
	}
}

public GameObject[] allEnemies;

void HitEnemy(string collider_name)
{
    allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

    foreach (GameObject go in allEnemies)
    {
        if (collider_name == go.name)
        {
            Animator enemy_anim = go.GetComponent<Animator>();

            if (enemy_anim != null)
            {
                enemy_anim.Play("TalibFallsLikeZombie");
                enemy_anim.SetInteger("IsTalibFallingLikeZombie", 1);
            }
        }
    }
}
	
	void TandemFollowBehavior()
{
    if (FollowTarget == null) return;

    // 1. Výpočet diagonální pozice za cílem (DesertReaper)
    Vector3 backwardOffset = -FollowTarget.forward * FollowDistance; // vzdálenost vzad
    Vector3 diagonalOffset = (FollowTarget.right + FollowTarget.forward).normalized * 0.5f * FollowDistance; 
    // 0.5f = poloviční vzdálenost diagonálně, můžete ladit

    Vector3 desiredPosition = FollowTarget.position + backwardOffset + diagonalOffset;

    // 2. Přidáme pohyb ze strany na stranu (sinu)
    patrolTimer += Time.deltaTime * PatrolSpeed;
    Vector3 sideOffset = FollowTarget.right * Mathf.Sin(patrolTimer) * PatrolAmplitude;
    desiredPosition += sideOffset;

    // 3. Pohyb Mia1 k požadované pozici
    player.transform.position = Vector3.MoveTowards(
        player.transform.position,
        desiredPosition,
        MovementSpeed * Time.deltaTime
    );

    // 4. Natáčení na cíl
    GameObject nearestEnemy = FindNearestEnemy(200f);
    if (nearestEnemy != null)
    {
        Vector3 dir = nearestEnemy.transform.position - player.transform.position;
        dir.y = 0;
        if (dir != Vector3.zero && Input.GetKey(KeyCode.LeftControl))
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, rot, Time.deltaTime * 0.05f);
        }

        // střelba
        shootTimer += Time.deltaTime;
        if (shootTimer >= ShootingCooldown)
        {
            FireWeapon();
            shootTimer = 0f;
        }
    }
    else
    {
        // pokud žádný nepřítel, díváme se na náhodný směr
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        if (randomDir != Vector3.zero && Input.GetKey(KeyCode.LeftAlt))
        {
            Quaternion rot = Quaternion.LookRotation(randomDir);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, rot, Time.deltaTime * 0.2f);
        }
    }
	
	desiredPosition = FollowTarget.position + backwardOffset + diagonalOffset;
	desiredPosition += FollowTarget.right * Mathf.Sin(patrolTimer) * PatrolAmplitude;

		// Zkontrolujeme překážku
		desiredPosition = CalculateAvoidancePosition(desiredPosition);

		player.transform.position = Vector3.MoveTowards(player.transform.position, desiredPosition, MovementSpeed * Time.deltaTime);
}

GameObject FindNearestEnemy(float maxDistance)
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    GameObject nearest = null;
    float minDist = maxDistance;

    foreach (GameObject enemy in enemies)
    {
        float dist = Vector3.Distance(player.transform.position, enemy.transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            nearest = enemy;
        }
    }

    return nearest;
}
Vector3 CalculateAvoidancePosition(Vector3 targetPosition)
{
    Vector3 direction = targetPosition - player.transform.position;
    RaycastHit hit;

    if (Physics.Raycast(player.transform.position, direction.normalized, out hit, direction.magnitude))
    {
        // našli jsme překážku
        Vector3 hitNormal = hit.normal;
        Vector3 obstacleCenter = hit.point;

        float offsetDistance = 10f; // vzdálenost od překážky
        Vector3 pointA = obstacleCenter + Vector3.Cross(Vector3.up, hitNormal).normalized * offsetDistance;
        Vector3 pointB = obstacleCenter - Vector3.Cross(Vector3.up, hitNormal).normalized * offsetDistance;

        // zvolíme bližší bod
        float distA = Vector3.Distance(player.transform.position, pointA);
        float distB = Vector3.Distance(player.transform.position, pointB);

        return (distA < distB) ? pointA : pointB;
    }

    return targetPosition; // žádná překážka
}
}

/*
Přetáhni:

RifleButton

GrenadeButton

jejich Image komponenty

Do Button → OnClick():

Rifle → SelectRifle()

Grenade → SelectGrenade()
*/

