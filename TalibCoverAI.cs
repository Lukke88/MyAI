using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TalibCoverAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        SearchingCover,
        MovingToCover,
        BehindCover,
		Shooting,
		MovingToRandomPosition,
		ThrowingGrenade,   // ← PŘIDÁNO
		RunToWall,         // ← PŘIDÁNO
		 TalibPicksWeapon
    }

    public State currentState = State.Idle;
	public Transform player;
    [Header("AI Settings")]
    public float detectRange = 120f;
    public float wallOffset = 5f;
    public float moveSpeed = 3f;

    public BaseTalibEnemyAI baseAI;
    public Transform cursorSquare;
    public GameObject enemy, nearestWall, targetPlayerObject;
	public Animator animator;
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;

    public bool passedA = false;
    public bool passedB = false;
    public bool passedC = false;
	
	public float dist_to_cursor;
	public bool IsSelectedEndPoint;
	public GameObject[] walls;
	
    public GameObject TargetedPlayer;
    public float hideDuration = 1f; // čas skrčení


    private bool isHiding = false;
	
	public float desiredRotation;
	[Header("Grenade Settings")]
public GameObject grenadePrefab;
public Transform grenadeSpawnPoint;
public float grenadeForce = 12f;
public float grenadeUpwardForce = 4f;
public GameObject explosionPrefab;

private bool isGrenadeInstantiated = false;

    private void Awake()
    {
        baseAI = GetComponent<BaseTalibEnemyAI>();
        cursorSquare = GameObject.Find("cursor_square")?.transform;
    }

    public void Start()
    {
        // Najde přímo svého GameObject podle jména
        enemy = GameObject.Find(this.name);
		dist_to_cursor = Vector3.Distance(enemy.transform.position, cursorSquare.position);
    }

    public void Update()
    {
        if (cursorSquare == null || enemy == null) return;
		dist_to_cursor = Vector3.Distance(enemy.transform.position, cursorSquare.position);
        // Detekce nepřítele v okolí cursor_square
        Collider[] enemies = Physics.OverlapSphere(cursorSquare.position, detectRange);
        foreach (Collider col in enemies)
        {
            if (col.CompareTag("Enemy") && col.gameObject == enemy)
            {
                if (currentState == State.Idle)
                {
                    StartSearchingCover();
                }
            }
        }

      if (currentState == State.Idle)
		{
			StartSearchingCover();
		}

		if (currentState == State.MovingToCover)
		{
			MoveAlongPoints();
		}
		if(currentState==State.Shooting)
		{
			FireToPlayer();
		}
		if(currentState==State.MovingToRandomPosition)
		{
			MoveToRandomPosition();
		}
		animator = enemy.transform.GetComponent<Animator>();

    if (currentState == State.MovingToCover)
    {
        animator.applyRootMotion = false;     // ← vypneme root motion při ručním pohybu
    }
    else if (currentState == State.BehindCover)
		{
			TurnToNearestPlayerFigure();


   // Řekněme, že chceš, aby enemy mířil na hráče
		Vector3 directionToPlayer = (player.position - enemy.transform.position).normalized;

// Vytvoří Quaternion z vektoru směru
		Quaternion desiredRotation = Quaternion.LookRotation(directionToPlayer);

// Porovnání úhlu
		if (Quaternion.Angle(enemy.transform.rotation, desiredRotation) < 5f)
		{
			currentState = State.Shooting;

			BaseTalibEnemyAI baseAI = enemy.GetComponent<BaseTalibEnemyAI>();
			if (baseAI != null)
			{
				baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibShooting);
		}
	}
	StartHideAndShoot();
	}
		
		walls = GameObject.FindGameObjectsWithTag("Wall");
		animator = enemy.transform.GetComponent<Animator>();
		float distanceFromPivot = Vector3.Distance(transform.position, pointC);
		if (distanceFromPivot >= 14f)
		{
			animator.applyRootMotion = false; // root motion vypnuto
		}
		else
		{
			animator.applyRootMotion = true;  // root motion zapnuto
		}
    }
	
	// Coroutine pro skrčení, přesun a znovu střelbu
    public void StartHideAndShoot()
    {
        if (!isHiding)
            StartCoroutine(MoveToRandomTile());
    }
	

    private IEnumerator MoveToRandomTile()
    {
        isHiding = true;

        // 1️⃣ Skrčení
        baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibHiding);
        yield return new WaitForSeconds(hideDuration);

        // 2️⃣ Najít všechny tile objekty
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tiles");
        if (tiles.Length == 0)
        {
            Debug.LogWarning("Nenašel jsem žádný tile!");
            isHiding = false;
            yield break;
        }

        // 3️⃣ Vybrat náhodný tile za zdi
        Transform randomTile = tiles[Random.Range(0, tiles.Length)].transform;

        // 4️⃣ Přesun AI k vybranému tile (smooth move)
        Vector3 targetPos = randomTile.position;
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // volitelně: AI se otáčí směrem k cíli
            Vector3 dir = (targetPos - transform.position).normalized;
            dir.y = 0f;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

            yield return null;
        }

// 5️⃣ Rozhodovací logika po vzpřímení
float decision = Random.Range(0f, 100f);

if (decision <= 30f)
{
    // 🔫 SHOOTING 30%
    currentState = State.Shooting;
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibShooting);

    if (TargetedPlayer != null)
        baseAI.TargetedPlayer = TargetedPlayer;

    baseAI.Fire();
}
else if (decision > 30f && decision<50f)
{
    // 💣 GRENADE 20%
	//enemy bezi k hraci, zastavi se na odhozove vzdalenosti, hodi granat, bezi zpet na puv. pozici
    StartCoroutine(RunAndThrowGrenade());
}
else if (decision > 50f && decision<70f)
{
    
    StartCoroutine(RunAndSnatchWeapon());
	/*
	enemy zjisti kde je nejblizsi zed, vybehne k ni, prehraje animace behem behu (BaseTalibEnemyAI.Animations) TalibRunning, TalibPicksWeapon, s odpovidajicimi parametry v BaseTalibEnemyAI.Parameters
	IsTalibRunning, IsTalibPicksWeapon, nastavenymi na integer 1. Pobezi k zbrani na zemi, zvedne ji, ukradne a po dobehnuti na cilovou pozici zacne strilet na hrace pomoci Fire();
	*/
}
else if (decision > 70f && decision<90f)
{
    
    StartCoroutine(RabbitRunSnatchWeapon());
	/*
	enemy zjisti kde je nejblizsi zbran na zemi,pokud je v dosahu 200f, vybehne k ni, sebere ji, prehraje animace behem behu (BaseTalibEnemyAI.Animations) TalibRunning, TalibPicksWeapon, s odpovidajicimi parametry v BaseTalibEnemyAI.Parameters
	IsTalibRunning, IsTalibPicksWeapon, nastavenymi na integer 1. Pobezi k zbrani na zemi, zvedne ji, ukradne, vrati se na puvodni pozici a po dobehnuti zacne strilet na hrace pomoci Fire();
	Trasa behu bude smycka od vychozi pozice, ke zbrani a zpet.
	*/
}
else
{
    // 🏃 RUN TO OTHER WALL 10%
	//bezi k nahodne zdi, ukrytu
    currentState = State.RunToWall;
    StartSearchingCover();
}

isHiding = false;

    }
	
	private IEnumerator RunAndSnatchWeapon()
{
    GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");
    if (weapons.Length == 0) yield break;

    GameObject closest = null;
    float minDist = Mathf.Infinity;

    foreach (var w in weapons)
    {
        float dist = Vector3.Distance(transform.position, w.transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            closest = w;
        }
    }

    if (closest == null) yield break;

    Vector3 targetPos = closest.transform.position;

    // 🏃 Běh ke zbrani
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibRunning);

    while (Vector3.Distance(transform.position, targetPos) > 0.2f)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;

        transform.position += dir * moveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 6f
            );
        }

        yield return null;
    }

    // 🎯 Pick animace
  //  baseAI.UpdateIndexedAction(State.TalibPicksWeapon);
    yield return new WaitForSeconds(0.8f);

    Destroy(closest);

    // 🔫 Začne střílet
    currentState = State.Shooting;
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibShooting);
    baseAI.Fire();
}

	
	private IEnumerator MoveAlongPath(List<Vector3> path, float speed)
{
    foreach (Vector3 point in path)
    {
        while (Vector3.Distance(transform.position, point) > 0.1f)
        {
            Vector3 dir = (point - transform.position).normalized;
            dir.y = 0f;

            transform.position += dir * speed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 6f
                );
            }

            yield return null;
        }
    }
}
/*
private IEnumerator RunAndThrowGrenade()
{
    currentState = State.ThrowingGrenade;

    Vector3 startPos = transform.position;
    Vector3 playerPos = player.position;

    float throwDistance = 10f;

    Vector3 direction = (playerPos - startPos).normalized;
    Vector3 throwPoint = startPos + direction * throwDistance;

    // Body trasy
    List<Vector3> pathToThrow = new List<Vector3>()
    {
        throwPoint
    };

    // Běh animace
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibCrouchRunning);

    yield return StartCoroutine(MoveAlongPath(pathToThrow, moveSpeed));

    // Hod animace
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibThrowing);
    yield return new WaitForSeconds(0.5f);

    ThrowGrenade();

    yield return new WaitForSeconds(1.5f);

    // Zpět na původní místo
    List<Vector3> returnPath = new List<Vector3>()
    {
        startPos
    };

    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibCrouchRunning);

    yield return StartCoroutine(MoveAlongPath(returnPath, moveSpeed));

    currentState = State.Shooting;
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibShooting);
}*/
private void ThrowGrenade()
{
    if (grenadePrefab == null || player == null) return;

    GameObject grenade = Instantiate(
        grenadePrefab,
        grenadeSpawnPoint.position,
        Quaternion.identity
    );

    Rigidbody rb = grenade.GetComponent<Rigidbody>();

    if (rb != null)
    {
        Vector3 dir = (player.position - grenadeSpawnPoint.position);
        dir.y = 0f;

        rb.velocity = dir.normalized * grenadeForce + Vector3.up * grenadeUpwardForce;
    }

    StartCoroutine(HandleExplosion(grenade));
}
private IEnumerator HandleExplosion(GameObject grenade)
{
    yield return new WaitForSeconds(2f);

    if (explosionPrefab != null)
    {
        GameObject explosion = Instantiate(
            explosionPrefab,
            grenade.transform.position,
            Quaternion.identity
        );

        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        Destroy(explosion, 2f);
    }

    Destroy(grenade);
}
private IEnumerator RabbitRunSnatchWeapon()
{
    GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapon");

    if (weapons.Length == 0) yield break;

    GameObject closest = null;
    float minDist = 200f;

    foreach (var w in weapons)
    {
        float dist = Vector3.Distance(transform.position, w.transform.position);
        if (dist < minDist)
        {
            minDist = dist;
            closest = w;
        }
    }

    if (closest == null) yield break;

    Vector3 startPos = transform.position;
    Vector3 weaponPos = closest.transform.position;

    List<Vector3> loopPath = new List<Vector3>()
    {
        weaponPos,
        startPos
    };

   // baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibRunning);

    yield return StartCoroutine(MoveAlongPath(loopPath, moveSpeed * 1.3f));

   // baseAI.UpdateIndexedAction(State.TalibPicksWeapon);
    yield return new WaitForSeconds(0.8f);

    Destroy(closest);

    currentState = State.Shooting;
    baseAI.Fire();
}

private IEnumerator RunAndThrowGrenade()
{
    currentState = State.ThrowingGrenade;

    // 1️⃣ Běh k hráči
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibCrouchRunning);

    float runTime = 1.2f;
    float timer = 0f;

    while (timer < runTime && player != null)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;

        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 6f);

        timer += Time.deltaTime;
        yield return null;
    }

    // 2️⃣ Throw animace
    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibThrowing);
    yield return new WaitForSeconds(0.5f);

    // 3️⃣ Instantiate grenade pouze jednou
    if (!isGrenadeInstantiated && grenadePrefab != null && player != null)
    {
        isGrenadeInstantiated = true;

        GameObject grenade = Instantiate(
            grenadePrefab,
            grenadeSpawnPoint.position,
            Quaternion.identity
        );

        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 direction = (player.position - grenadeSpawnPoint.position);
            direction.y = 0f;

            Vector3 ballisticVelocity =
                direction.normalized * grenadeForce +
                Vector3.up * grenadeUpwardForce;

            rb.velocity = ballisticVelocity;
        }

        // 4️⃣ Čas do výbuchu
        yield return new WaitForSeconds(2f);

        if (explosionPrefab != null)
        {
            GameObject explosion =
                Instantiate(explosionPrefab, grenade.transform.position, Quaternion.identity);

            ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Play();

            Destroy(explosion, 2f);
        }

        Destroy(grenade);
    }

    // 5️⃣ Reset
    isGrenadeInstantiated = false;
    currentState = State.Idle;
}

	public void TurnToNearestPlayerFigure()
{
    if (enemy == null || targetPlayerObject == null) return;

    // Směr k hráči
    Vector3 direction = (targetPlayerObject.transform.position - enemy.transform.position).normalized;
	if(targetPlayerObject!=null)player = targetPlayerObject.transform;
    // Jen otočíme kolem osy Y
    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
    
    // Plynulá rotace
    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRotation, Time.deltaTime * 5f);

    // Po otočení přejdeme ke střelbě
    if (Quaternion.Angle(enemy.transform.rotation, lookRotation) < 5f) // pokud je skoro otočen
    {
        currentState = State.Shooting;
    }
}
public void FireToPlayer()
{
    if (player == null || baseAI == null) return;

    Vector3 dir = (player.position - transform.position).normalized;
    dir.y = 0f;

    if (dir != Vector3.zero)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 6f
        );
    }

    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibShooting);
    baseAI.TargetedPlayer = TargetedPlayer;
    baseAI.Fire();
}


public void MoveToRandomPosition()
{
    GameObject[] tiles = GameObject.FindGameObjectsWithTag("tiles");
    if (tiles.Length == 0) return;

    Transform randomTile = tiles[Random.Range(0, tiles.Length)].transform;

    Vector3 target = randomTile.position;
    target.y = transform.position.y;

    transform.position = Vector3.MoveTowards(
        transform.position,
        target,
        moveSpeed * Time.deltaTime
    );

    Vector3 dir = (target - transform.position).normalized;
    dir.y = 0f;

    if (dir != Vector3.zero)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 6f
        );
    }

    baseAI.UpdateIndexedAction(BaseTalibEnemyAI.Animations.TalibRunning);

    if (Vector3.Distance(transform.position, target) < 0.2f)
    {
        currentState = State.Shooting;
    }
}

    public void StartSearchingCover()
    {
        currentState = State.SearchingCover;
        baseAI.SetState(BaseTalibEnemyAI.Animations.TalibCrouchRunning);

        // Najdi nejbližší zeď
         walls = GameObject.FindGameObjectsWithTag("Wall");
        nearestWall = null;
        float minDist = Mathf.Infinity;
        foreach (var wall in walls)
        {
            float dist = Vector3.Distance(enemy.transform.position, wall.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearestWall = wall;
            }
        }

        if (nearestWall != null)
{
	float k_z = 12.0f;
    Vector3 wallPos = nearestWall.transform.position;
    Vector3 directionToEnemy = (transform.position - wallPos).normalized;

    Collider wallCol = nearestWall.GetComponent<Collider>();
    float size_x = wallCol.bounds.size.x;
    float size_z = wallCol.bounds.size.z;

    // Point A
    float offsetX_A = size_x/2 + wallOffset;
    float offsetZ_A = size_z/2;
    pointA = new Vector3(wallPos.x + offsetX_A * Mathf.Sign(directionToEnemy.x),
                         enemy.transform.position.y,
                         wallPos.z + offsetZ_A - k_z);

    // Point B (za zdí, ve směru opačném Z)
    float offsetX_B = offsetX_A;
    float offsetZ_B = -offsetZ_A;
    pointB = new Vector3(wallPos.x + offsetX_B * Mathf.Sign(directionToEnemy.x),
                         enemy.transform.position.y,
                         wallPos.z + offsetZ_B - k_z*3);

    // Point C = náhodný tile
    GameObject[] tiles = GameObject.FindGameObjectsWithTag("tiles");
    if (tiles.Length > 0 && IsSelectedEndPoint==false)
	{
       pointC = tiles[Random.Range(0, tiles.Length)].transform.position;
		pointC = new Vector3(pointC.x, enemy.transform.position.y, pointC.z - k_z);
		IsSelectedEndPoint = true;
	}
	Color lightPurple = new Color(0.7f, 0.4f, 0.9f, 1f); // světlejší fialová
	Color neonPurple = new Color(0.8f, 0f, 1f, 1f);      // neon fialová
	Color segmentA = Color.yellow;                           // první úsek – Point_A
	Color segmentB = new Color(1f, 0.6f, 0f, 1f);          // grepová/oranžovo-žlutá – Point_B
	Color segmentC = new Color(1f, 0.5f, 0f, 1f);          // oranžová – Point_C

    // Debug
	Debug.DrawLine(transform.position, pointA, segmentA, 5f);
	Debug.DrawLine(pointA, pointB, segmentB, 5f);
	Debug.DrawLine(pointB, pointC, segmentC, 5f);


    // Reset průchodových boolů
    //passedA = passedB = passedC = false;

    // Přepni stav na Moving
    currentState = State.MovingToCover;
}

        else if(IsSelectedEndPoint==false)
        {
            currentState = State.Idle;
        }
		else if(IsSelectedEndPoint==true && pointA!=Vector3.zero && pointB!=Vector3.zero && pointC!=Vector3.zero)
		{
			currentState = State.MovingToCover;
			MoveAlongPoints();
			
		}
    }

    public void MoveAlongPoints()
{
    Vector3 target = Vector3.zero;

    if (!passedA) target = pointA;
    else if (!passedB) target = pointB;
    else if (!passedC) target = pointC;

    transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

    if (!passedA && Vector3.Distance(transform.position, pointA) < 0.1f) passedA = true;
    else if (!passedB && Vector3.Distance(transform.position, pointB) < 0.1f) passedB = true;
    else if (!passedC && Vector3.Distance(transform.position, pointC) < 0.1f)
    {
        passedC = true;
        currentState = State.BehindCover;
        baseAI.SetState(BaseTalibEnemyAI.Animations.TalibHiding);
        SetAllToDefault();
    }
}

	public void SetAllToDefault()
	{	
			IsSelectedEndPoint=false;
			pointA = Vector3.zero; pointB=Vector3.zero; pointC = Vector3.zero;
			passedA = false; passedB=false; passedC = false;
	}
    // Debug vizualizace bodů v editoru
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointA, 0.2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pointB, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(pointC, 0.2f);
    }
}
