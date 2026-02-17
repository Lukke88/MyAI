using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//how do you like my work today ? Am I skilled programmer ? What do you think?
public class BTR_behaviour : MonoBehaviour
{
    public GameObject armored_car, main_hero, cursor_square, projectile;
    public GameObject turret;
    public bool isActivated = false;
	public Vector3 moveDir;
    [Header("Vehicle Movement")]
    public float forwardSpeed = 8f;
    public float backwardSpeed = 5f;
    public float turnSpeed = 90f;

    [Header("Turret Rotation")]
    public float turretHorizontalSpeed = 180f;
    public float turretVerticalSpeed = 90f;
    public float verticalMinAngle = -5f;
    public float verticalMaxAngle = 60f;
	public float distance_hero_car;
    private Camera mainCamera;
	public Vector3 hit_point;
	
	[Header("Ballistics Debug")]
	public float ballisticHeight = 8f;
	public int ballisticResolution = 30;
    // Nové: seznam kol
    private List<Transform> wheels = new List<Transform>();
	
	public LineRenderer lineRenderer;
	public GameObject cannon_muzzle;
	
	[Header("Prefabs & Audio")]
public GameObject projectilePrefab;       // Prefab projektilu
public GameObject muzzleFlashPrefab;      // Prefab pro hlaveň (vystřel)
public GameObject impactExplosionPrefab;  // Prefab exploze při dopadu
public AudioClip fireSound;               // Zvuk výstřelu
public AudioClip explosionSound;          // Zvuk exploze

[Header("Settings")]
public float projectileSpeed = 20f;       // rychlost projektilu
public Transform cannonMuzzle;            // hlaveň, odkud střílí

public enum Animations
{
    TalibIdle,
    TalibRunning,
    TalibShooting,
    TalibThrowGrenade,
    TalibPicksWeapon   // ← PŘIDEJ TOTO
}
    void Start()
    {
        if (armored_car == null)
        {
            armored_car = GameObject.Find(this.name);
        }

        if (turret == null && armored_car != null)
        {
            turret = armored_car.transform.GetChild(39).gameObject;
        }

        mainCamera = Camera.main;
		cursor_square = GameObject.Find("cursor_square");
		target = cursor_square.transform;
        // Najdeme kola
        if (armored_car != null)
            FindAndAssignWheels(armored_car.transform);
    }

    void Update()
    {
		main_hero = GameObject.Find("JennyFinal_lowpoly_z_erased");
		turret = armored_car.transform.GetChild(39).gameObject;
        if (armored_car == null)
        {
            armored_car = GameObject.Find(this.name);
        }
		distance_hero_car = Vector3.Distance(armored_car.transform.position, main_hero.transform.position);
		
		if(distance_hero_car<=5.0f)
			isActivated = true;
		if (Input.GetKeyDown(KeyCode.Space))
			{
				FireProjectile();
			}
		/*turret.transform.rotation = Quaternion.Euler(
    2*45f,0f,   // X – červená osa dolů
     0f// Y – natočení do směru (uprav podle modelu)     // Z – zamčeno
);*/
		//GameObject turret_child = turret.transform.GetChild(0).gameObject;
    // aplikujeme – X a Z zůstávají zamčené
    

		if(distance_hero_car<5)
		{
			isActivated = true;
			main_hero.transform.position = armored_car.transform.position;
		}
        if (turret == null && armored_car != null)
        {
            
        }
		lineRenderer = gameObject.GetComponent<LineRenderer>();
		if (lineRenderer == null)
		{
			lineRenderer = gameObject.AddComponent<LineRenderer>();
		}

			//lineRenderer.enabled = false;
			lineRenderer.useWorldSpace = true;
			lineRenderer.widthMultiplier = 0.1f;
			lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			lineRenderer.startColor = Color.red;
			lineRenderer.endColor = Color.red;
			
        if (!isActivated) return;

        HandleVehicleMovement();
      //  HandleTurretRotation();
		UpdateCursorOnTerrain();
		//RotateTurretYawToCursor();
		target = GameObject.Find("plane_cursor").transform;
		 if (Input.GetKey(KeyCode.LeftControl) && target != null)
{
    // Vektor od věže k cíli
    Vector3 dir = target.position - turret.transform.position;

    // horizontální vzdálenosti
    float x = dir.x;
    float z = dir.z;

    // délka přepony v horizontální rovině
    float hypotenuse = Mathf.Sqrt(x * x + z * z);

    if (hypotenuse > 0.001f) // zabránění dělení nulou
    {
        // úhel v radiánech
        float angleRad = Mathf.Asin(x / hypotenuse);

        // úhel ve stupních
        float yAngle = angleRad * Mathf.Rad2Deg;

        // korekce podle kvadrantu
        if (z < 0) yAngle = 180 - yAngle;

        // nastavíme rotaci věže, zachováme X a Z osu
        turret.transform.rotation = Quaternion.Euler(turret.transform.eulerAngles.x, yAngle+180, turret.transform.eulerAngles.z);
    }
	if (lineRenderer == null || turret == null || cursor_square == null) return;

    if (Input.GetKey(KeyCode.LeftControl)) // Ctrl stisknutý
    {
        lineRenderer.enabled = true; // zapneme LineRenderer

        Transform barrel = turret.transform.GetChild(0); // hlaveň věže
        Vector3 start = barrel.position;
        Vector3 end = hit_point;

        lineRenderer.positionCount = ballisticResolution + 1;

        for (int i = 0; i <= ballisticResolution; i++)
        {
            float t = i / (float)ballisticResolution;
            Vector3 point = GetBallisticPoint(start, end, t);
            lineRenderer.SetPosition(i, point);
        }
    }
    else
    {
     //   lineRenderer.enabled = false; // Ctrl není stisknutý, skryjeme čáru
    }
	
	cannon_muzzle = turret.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
	HandleShooting();
}
projectile.transform.rotation = Quaternion.LookRotation(moveDir);


    }
	void OnDrawGizmos()
{
    if (turret == null || cursor_square == null) return;

    Transform barrel = turret.transform.GetChild(0);
    Vector3 start = barrel.position;
    Vector3 end = hit_point;

    Gizmos.color = Color.yellow;

    Vector3 prev = start;
    for (int i = 1; i <= ballisticResolution; i++)
    {
        float t = i / (float)ballisticResolution;
        Vector3 point = GetBallisticPoint(start, end, t);
        Gizmos.DrawLine(prev, point);
        prev = point;
    }
}

// Funkce 1 – vystřelí projektil po balistické křivce
void FireProjectileAtTarget(Vector3 targetPos)
{
    if (projectilePrefab == null || cannonMuzzle == null) return;

    GameObject projectile = Instantiate(projectilePrefab, cannonMuzzle.position, Quaternion.identity);

    // Start coroutine pro pohyb projektilu po balistické křivce
    StartCoroutine(MoveProjectileAlongBallistic(projectile, cannonMuzzle.position, targetPos));
}

// Coroutine pro balistickou dráhu
/*
IEnumerator MoveProjectileAlongBallistic(GameObject projectile, Vector3 start, Vector3 end)
{
    float t = 0f;
    while (t < 1f)
    {
        t += Time.deltaTime * projectileSpeed / Vector3.Distance(start, end); // normalizované podle vzdálenosti
        projectile.transform.position = GetBallisticPoint(start, end, t);
        yield return null;
    }

    // Dopad – vyvoláme explozi
    ExplodeProjectile(projectile.transform.position);

    Destroy(projectile); // odstraníme projektil
}*/

// Funkce 2 – efekty hlaveň + audio
public void PlayMuzzleFlash()
{
    if (muzzleFlashPrefab == null || cannonMuzzle == null) return;

    GameObject flash = Instantiate(muzzleFlashPrefab, cannonMuzzle.position, cannonMuzzle.rotation);
    
    // Audio
    if (fireSound != null)
    {
        AudioSource.PlayClipAtPoint(fireSound, cannonMuzzle.position);
    }

    ParticleSystem ps = flash.GetComponent<ParticleSystem>();
    if (ps != null) ps.Play();

    Destroy(flash, 2f); // po 2 sekundách odstraníme efekt
}

// Funkce 3 – exploze projektilu po dopadu
public void ExplodeProjectile(Vector3 position)
{
    if (impactExplosionPrefab == null) return;

    GameObject explosion = Instantiate(impactExplosionPrefab, position, Quaternion.identity);

    // Audio
    if (explosionSound != null)
    {
        AudioSource.PlayClipAtPoint(explosionSound, position);
    }

    ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
    if (ps != null) ps.Play();

    Destroy(explosion, 3f); // po 3 sekundách odstraníme explozi
}

// --- Integrace do Update pro stisk mezerníku ---
void HandleShooting()
{
    if (Input.GetKeyDown(KeyCode.Space) && target != null)
    {
        PlayMuzzleFlash();                     // efekt hlaveň + zvuk
        FireProjectileAtTarget(target.position); // vystřel projektil
    }
}

	Vector3 GetBallisticPoint(Vector3 start, Vector3 end, float t)
{
    Vector3 mid = Vector3.Lerp(start, end, t);
    float height = Mathf.Sin(Mathf.PI * t) * ballisticHeight;
    mid.y += height;
    return mid;
}

	void CreateBalisticLine()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, 2000))
		{
			hit_point = hit.point;
		}
	}
	
	// Coroutine pro balistickou dráhu s otáčením projektilu a trail efektem
IEnumerator MoveProjectileAlongBallistic(GameObject projectile, Vector3 start, Vector3 end)
{
    float t = 0f;
    Vector3 prevPos = start;

    // Pokud má projektil TrailRenderer, deaktivujeme ho na začátku a aktivujeme až po prvním kroku
    TrailRenderer trail = projectile.GetComponent<TrailRenderer>();
    if (trail != null) trail.Clear();

    while (t < 1f)
    {
        t += Time.deltaTime * projectileSpeed / Vector3.Distance(start, end);
        Vector3 nextPos = GetBallisticPoint(start, end, t);

        // Nastavení pozice
        projectile.transform.position = nextPos;

        // Otáčení projektilu směrem k pohybu
        Vector3 moveDir = (nextPos - prevPos).normalized;
        if (moveDir != Vector3.zero)
        {
            projectile.transform.rotation = Quaternion.LookRotation(moveDir);
        }

        prevPos = nextPos;
        yield return null;
    }

    // Dopad
    ExplodeProjectile(projectile.transform.position);
    Destroy(projectile);
}

// Funkce pro instanci trailu (pokud chcete samostatně, lze i v prefabu)
void AttachTrail(GameObject projectilePrefab)
{
    TrailRenderer trail = projectilePrefab.GetComponent<TrailRenderer>();
    if (trail != null)
    {
        trail.Clear(); // vyčistí předchozí stopy
    }
}

// --- Integrace do HandleShooting ---
void HandleShooting2()
{
    if (Input.GetKeyDown(KeyCode.Space) && target != null)
    {
        PlayMuzzleFlash();                     // efekt hlaveň + zvuk
        GameObject projectile = Instantiate(projectilePrefab, cannonMuzzle.position, cannonMuzzle.rotation);

        // Trail renderer pokud existuje
        AttachTrail(projectile);

        // Spustíme pohyb projektilu s otáčením
        StartCoroutine(MoveProjectileAlongBallistic(projectile, cannonMuzzle.position, target.position));
    }
}

	void RotateTurretYawToCursor()
{
    if (turret == null) return;

    Vector3 dir = hit_point - turret.transform.position;
    dir.y = 0f;

    if (dir.sqrMagnitude < 0.01f) return;

    Quaternion targetRot = Quaternion.LookRotation(dir);
    turret.transform.rotation = Quaternion.RotateTowards(
        turret.transform.rotation,
        targetRot,
        turretHorizontalSpeed * Time.deltaTime
    );
}

	void UpdateCursorOnTerrain()
{
    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 3000f))
    {
        hit_point = hit.point;

        if (cursor_square != null)
        {
            cursor_square.transform.position = hit_point + Vector3.up * 0.05f; // lehce nad zem
        }
    }
}
public Transform target; // objekt, ke kterému má věž směřovat
public bool ProjectileCreated;
public GameObject instantiated_projectile;

//public GameObject projectile;
//public Transform cannon_muzzle;
//public Transform cursor_square;

public float projectileMass = 8f;
public float launchForce = 60f;




    void HandleVehicleMovement()
    {
		projectile = GameObject.Find("projectile");
        float moveInput = 0f;
        float turnInput = 0f;

        if (Input.GetKey(KeyCode.W)) moveInput = -1f;
        if (Input.GetKey(KeyCode.S)) moveInput = 1f;

        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;

        float currentSpeed = moveInput >= 0 ? forwardSpeed : backwardSpeed;

        // Pohyb auta
        transform.Translate(Vector3.right * moveInput * currentSpeed * Time.deltaTime);

        if (Mathf.Abs(moveInput) > 0.01f && !Input.GetKey(KeyCode.LeftControl))
        {
			 transform.Rotate(Vector3.forward, turnInput * turnSpeed * Time.deltaTime);
            
        }
		else if (Input.GetKey(KeyCode.LeftControl))
		{
    if (turret != null && cursor_square != null)
    {
        // Vektor směřující od věže k cíli
        Vector3 dir = cursor_square.transform.position - turret.transform.position;

        // zamknout pouze Y osu
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            // směr rotace
            Quaternion targetRotation = Quaternion.LookRotation(dir);
			// Korekce 90 stupňů, protože věž modelově směřuje podél X
            targetRotation *= Quaternion.Euler(0f, 90f, 0f);

            // plynulé otočení věže (můžeš přidat Time.deltaTime * speed)
            turret.transform.rotation = Quaternion.RotateTowards(
                turret.transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }
		}
		/*else if(Input.GetKey(KeyCode.LeftControl)) //old, working function, don't erase !!!
		{
		//	turret.transform.Rotate(Vector3.right, -turnInput * turnSpeed * Time.deltaTime);
		turret.transform.Rotate(Vector3.up, -turnInput * turnSpeed * Time.deltaTime);
		}
		
		if(cannon_muzzle!=null && projectile!=null)
		{
			if(ProjectileCreated==false)
			{
				instantiated_projectile =Instantiate(projectile, cannon_muzzle.transform.position, cannon_muzzle.transform.rotation);
			}
		}*/

        // Otáčení kol
        RotateWheels(moveInput, currentSpeed);
    }
	

[SerializeField] private AudioClip fireClip;
[SerializeField] private float muzzleLifeTime = 2f;

private AudioSource audioSource;
void Awake()
{
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();
}

void FireProjectile()
{
    if (cannon_muzzle == null || projectile == null) return;

    GameObject go = Instantiate(
        projectile,
        cannon_muzzle.transform.position,
        cannon_muzzle.transform.rotation
    );
	go.transform.GetComponent<ProjectileCollision>().IsShot=true;
    // === Projectile script ===
    ProjectileCollision pc = go.GetComponent<ProjectileCollision>();
    if (pc == null)
    {
        Debug.LogError("Projectile nemá ProjectileCollision!");
        return;
    }

    pc.InitBallisticPath(
        turret.transform,
        cursor_square.transform,
        ballisticResolution,
        GetBallisticPoint
    );

    // === Rigidbody (jen pro kolize) ===
    Rigidbody rb = go.GetComponent<Rigidbody>();
    if (rb == null)
        rb = go.AddComponent<Rigidbody>();

    rb.isKinematic = true;   // ❗ let řídíme ručně
    rb.useGravity = false;
    rb.mass = 8f;
	
	if (cannon_muzzle != null && muzzleFlashPrefab != null)
{
    GameObject instantiatedMuzzle = Instantiate(
        muzzleFlashPrefab,
        cannon_muzzle.transform.position,//weapon_muzzle is name
        cannon_muzzle.transform.rotation
    );
	ParticleSystem ps = instantiatedMuzzle.transform.GetComponent<ParticleSystem>();
	ps.Play();
    Destroy(instantiatedMuzzle, muzzleLifeTime);
}

if (fireClip != null && audioSource != null)
{
    audioSource.PlayOneShot(fireClip);
}

}


void LateUpdate()
{
    if (ProjectileCreated && instantiated_projectile == null)
    {
        ProjectileCreated = false;
    }
}
   void HandleTurretRotation()
{
    if (turret == null || mainCamera == null) return;

    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
   /* Plane groundPlane = new Plane(Vector3.up, armored_car.transform.position);

    if (!groundPlane.Raycast(ray, out float distance)) return;

    Vector3 targetPoint = ray.GetPoint(distance);

    // směr k cíli – pouze v XZ rovině
    Vector3 dir = targetPoint - turret.transform.position;
    dir.y = 0f;

    if (dir.sqrMagnitude < 0.001f) return;

    // spočítáme pouze Y úhel
    float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

    // aktuální rotace
    Vector3 currentEuler = turret.transform.eulerAngles;

    // plynulý přechod pouze v Y
    float newYaw = Mathf.MoveTowardsAngle(
        currentEuler.y,
        targetYaw,
        turretHorizontalSpeed * Time.deltaTime
    );*/
	GameObject turret_child = turret.transform.GetChild(0).gameObject;
    // aplikujeme – X a Z zůstávají zamčené
    turret_child.transform.rotation = Quaternion.Euler(
    -90f,   // X – červená osa dolů
    -45f,   // Y – natočení do směru (uprav podle modelu)
    0f      // Z – zamčeno
);


    // === HLAVEŇ (pitch) ===
    /*Transform barrel = turret.transform.GetChild(0);

    Vector3 localTarget = turret.transform.InverseTransformPoint(targetPoint);
    float pitch = Mathf.Atan2(localTarget.y, localTarget.z) * Mathf.Rad2Deg;
    pitch = Mathf.Clamp(pitch, verticalMinAngle, verticalMaxAngle);

    barrel.localRotation = Quaternion.Euler(pitch, 0f, 0f);*/
}


    // Rekurzivně najde kola a vytvoří pro každé pivot
void FindAndAssignWheels(Transform parent)
{
    foreach (Transform child in parent)
    {
        if (child.name.ToLower().Contains("wheels"))
        {
            wheels.Add(child); // pivot je už vytvořený, uložíme ho do seznamu
        }

        if (child.childCount > 0)
            FindAndAssignWheels(child);
    }
}

// Otáčení kol – teď otáčíme pivoty
void RotateWheels(float moveInput, float speed)
{
    float rotationAmount = moveInput * speed * Time.deltaTime * 360f;
    foreach (Transform wheelPivot in wheels)
    {
        wheelPivot.Rotate(Vector3.up, rotationAmount); // otáčí pivot → kolo se otáčí kolem středu
    }
}

}
