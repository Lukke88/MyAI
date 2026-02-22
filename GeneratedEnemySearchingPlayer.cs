using UnityEngine;

public class GeneratedEnemySearchingPlayer : MonoBehaviour
{
    public float moveSpeed = 0.8f;
    public float rayDistance = 20f;
    public float wallOffsetPercent = 0.2f;

    public GameObject player, enemy, detectedWall, lastWall;
    public GameObject nearestWall;

    public bool avoidingWall = false;
    private bool takingCover = false;
    private Vector3 targetPosition;
	
	public Transform gun;
	public float fireRate = 0.5f;
	private float nextFireTime;
	
	public bool IsMovingToPlayer;
	
	public enum EnemyState
{
    Searching,
    Attack
}

public GameObject projectilePrefab;
public Transform projectileSpawnPoint;

public float attackDistance = 20f;
public float shootInterval = 0.05f;

private float nextShootTime;
public EnemyState currentState = EnemyState.Searching;

public LineRenderer projectileTrail;
public GameObject explosionPrefab;
public GameObject holePrefab;

public float coneLength = 25f;
public float coneWidth = 15f;

    void Start()
    {
		enemy = GameObject.Find(this.name);
        FindNearestPlayer();
		IsMovingToPlayer = true;
    }

    void Update()
{
    if (player == null)
        FindNearestPlayer();

    if (player == null)
        return;
if(IsMovingToPlayer==false && nearestWall!=null)
	{
		RotateGunTowardsGameObject(nearestWall);
	}
	if(IsMovingToPlayer==true)
    RotateGunTowardsPlayer();

    RaycastHit hit;
	if(nearestWall==null)//if is null, get new wall otherwise go around nearestWall and set to null after passing PointA & PointB
	{
    if (Physics.Raycast(enemy.transform.position, enemy.transform.forward, out hit, rayDistance))
    {
        Debug.DrawRay(enemy.transform.position, enemy.transform.forward * rayDistance, Color.green);

        if (hit.collider.CompareTag("Player"))
        {
         //   Shoot();
        }

        if (hit.collider.CompareTag("Wall") && lastWall==null)
        {
            nearestWall = hit.collider.gameObject;
			IsMovingToPlayer = false;
          //  AvoidWall();
            return;
        }
		else if (hit.collider.CompareTag("Wall") && lastWall!=null)
        {
			if(Vector3.Distance(hit.collider.gameObject.transform.position,player.transform.position)<Vector3.Distance(lastWall.transform.position,player.transform.position))
            nearestWall = hit.collider.gameObject;
			IsMovingToPlayer = false;
          //  AvoidWall();
            return;
        }
    }
    else
    {
        Debug.DrawRay(enemy.transform.position, enemy.transform.up * rayDistance, Color.red);
    }
	}

    if (!takingCover && PointA==Vector3.zero && PointB==Vector3.zero && IsMovingToPlayer==true)
        MoveTowardsPlayer();
	else if(nearestWall.transform.position.z>player.transform.position.z)//if the wall isn't behind player
	{
		MoveToNearestWall(nearestWall, 2.0f, 0.8f);
	}
	
	float distToPlayer = Vector3.Distance(enemy.transform.position,
                                     player.transform.position);

switch(currentState)
{
    case EnemyState.Searching:

        if(distToPlayer < attackDistance &&
           IsPlayerInVisionCone())
        {
			Attack();
            currentState = EnemyState.Attack;
        }

        break;

    case EnemyState.Attack:

        // Stop moving
        if(distToPlayer > attackDistance ||
           !IsPlayerInVisionCone())
        {
			Searching();
            currentState = EnemyState.Searching;
            break;
        }

        // Rotate toward player
        Vector3 dir = (player.transform.position - enemy.transform.position).normalized;
        enemy.transform.rotation =
            Quaternion.Slerp(enemy.transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 5f);

        Shoot();

        break;
}
if(currentState == EnemyState.Searching)
{
    Vector3 left = Quaternion.Euler(0,-coneWidth,0) * enemy.transform.forward;
    Vector3 right = Quaternion.Euler(0,coneWidth,0) * enemy.transform.forward;

    Debug.DrawLine(enemy.transform.position,
        enemy.transform.position + left * coneLength,
        new Color(0,0.6f,0));

    Debug.DrawLine(enemy.transform.position,
        enemy.transform.position + right * coneLength,
        new Color(0,0.6f,0));

    Debug.DrawLine(enemy.transform.position + left * coneLength,
        enemy.transform.position + right * coneLength,
        new Color(0,0.4f,0));
}
}
void Attack()
{
    float distToPlayer = Vector3.Distance(
        enemy.transform.position,
        player.transform.position);

    // If player is behind wall → back to search
    if(Physics.Linecast(enemy.transform.position,
        player.transform.position,
        LayerMask.GetMask("Wall")))
    {
        currentState = EnemyState.Searching;
        return;
    }

    if(distToPlayer > attackDistance)
    {
        currentState = EnemyState.Searching;
        return;
    }

    // Rotate toward player
    Vector3 dir =
        (player.transform.position - enemy.transform.position).normalized;

    enemy.transform.rotation =
        Quaternion.Slerp(
            enemy.transform.rotation,
            Quaternion.LookRotation(dir) * Quaternion.Euler(0,180f,0),
            Time.deltaTime * 5f);

    // Shoot
    Shoot();
}
bool IsPlayerInVisionCone()
{
    Vector3 dirToPlayer = (player.transform.position - enemy.transform.position).normalized;
    float angle = Vector3.Angle(enemy.transform.forward, dirToPlayer);

    if(angle < 45f) // half cone angle
    {
        if(!Physics.Linecast(enemy.transform.position, player.transform.position,
            LayerMask.GetMask("Wall")))
        {
            Debug.DrawLine(enemy.transform.position,
                player.transform.position,
                Color.cyan);

            return true;
        }
    }

    return false;
}

void Shoot()
{
    if(Time.time < nextShootTime) return;

    nextShootTime = Time.time + shootInterval;

    GameObject proj = Instantiate(
        projectilePrefab,
        projectileSpawnPoint.position,
        Quaternion.identity);

    Vector3 dir = (player.transform.position - projectileSpawnPoint.position).normalized;

    Rigidbody rb = proj.AddComponent<Rigidbody>();
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    rb.useGravity = false;
    rb.velocity = dir * 25f;

    // Line renderer trail
    LineRenderer lr = proj.AddComponent<LineRenderer>();
    lr.positionCount = 2;
    lr.widthMultiplier = 0.15f;
    lr.material = new Material(Shader.Find("Sprites/Default"));
    lr.startColor = Color.green;
    lr.endColor = Color.green;

    proj.AddComponent<ProjectileHitEffect>().Init(this);

    StartCoroutine(DestroyProjectileAfterTime(proj));
}

using UnityEngine;

public class ProjectileHitEffect : MonoBehaviour
{
    GeneratedEnemySearchingPlayer enemyAI;

    public void Init(GeneratedEnemySearchingPlayer ai)
    {
        enemyAI = ai;
    }

    void Update()
    {
        if(GetComponent<Rigidbody>() == null) return;

        LineRenderer lr = GetComponent<LineRenderer>();
        if(lr != null)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position + transform.forward * 1.5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            Instantiate(enemyAI.explosionPrefab,
                transform.position,
                Quaternion.identity);

            Instantiate(enemyAI.holePrefab,
                collision.contacts[0].point,
                Quaternion.LookRotation(collision.contacts[0].normal));

            Destroy(gameObject);
        }

        if(collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
IEnumerator DestroyProjectileAfterTime(GameObject projectile)
{
    yield return new WaitForSeconds(3f);

    if(projectile != null)
        Destroy(projectile);
}
void RotateGunTowardsPlayer()
{
    Vector3 dir = (player.transform.position - enemy.transform.position).normalized;
    Quaternion lookRot = Quaternion.LookRotation(dir);
    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRot, 10f * Time.deltaTime);
}

void RotateGunTowardsGameObject(GameObject selectedWall)
{
    Vector3 dir = (selectedWall.transform.position - enemy.transform.position).normalized;
    Quaternion lookRot = Quaternion.LookRotation(dir);
    enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, lookRot, 10f * Time.deltaTime);
	
	MoveToNearestWall(selectedWall, 2.0f, 0.8f);
}
public Vector3 PointA, PointB;
public bool passedA, passedB;
//original-NEMAZAT !!!
/*
public void MoveToNearestWall(GameObject selectedWall, float min_distance, float movementSpeed)
{
	float threshold = 0.5f;
	if(Vector3.Distance(enemy.transform.position,selectedWall.transform.position)>min_distance && PointA==Vector3.zero && PointB==Vector3.zero)
	enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, selectedWall.transform.position, movementSpeed*Time.deltaTime);
	else if((Vector3.Distance(enemy.transform.position,selectedWall.transform.position)<=min_distance)||(PointA!=Vector3.zero || PointB!=Vector3.zero))
	{
		//move to PointA, B
		float size_x = selectedWall.transform.GetComponent<Collider>().bounds.size.x;
		float size_z = selectedWall.transform.GetComponent<Collider>().bounds.size.z;
		if(PointA==Vector3.zero)
		PointA = new Vector3(selectedWall.transform.position.x - size_x/2, enemy.transform.position.y, selectedWall.transform.position.z + ((size_z/2)*1.2f));
			IsMovingToPlayer = false;
		if(passedA==false && (Vector3.Distance(enemy.transform.position, PointA)>threshold))
		{
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, PointA, movementSpeed*Time.deltaTime);
		}
		else
		{
			if(Vector3.Distance(enemy.transform.position, PointA)<=threshold)passedA=true;
		}
			
		if(PointB==Vector3.zero)
		PointB = new Vector3(selectedWall.transform.position.x - size_x/2, enemy.transform.position.y, selectedWall.transform.position.z - ((size_z/2)*1.2f));
		if(passedA==true && passedB==false && (Vector3.Distance(enemy.transform.position, PointB)>threshold))
		{
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, PointB, movementSpeed*Time.deltaTime);
		}
		else
		{
			if(Vector3.Distance(enemy.transform.position, PointB)<=threshold)passedB=true;
		}
		
		if(passedA==true && passedB==true)
		{
			nearestWall=null;
			selectedWall = null;
			IsMovingToPlayer=true;
			PointA=Vector3.zero;
			PointB = Vector3.zero;
			passedA = false;
			passedB = false;
		}
		
		if(PointA!=Vector3.zero && PointB!=Vector3.zero)
		{
			Debug.DrawLine(enemy.transform.position, PointA,Color.blue);
			Debug.DrawLine(PointB, PointA,Color.blue);
		}
	}
}
*/
public void MoveToNearestWall(GameObject selectedWall, float min_distance, float movementSpeed)
{
    float threshold = 0.5f;

    if(selectedWall == null) return;

    Collider wallCollider = selectedWall.GetComponent<Collider>();

    float sizeX = wallCollider.bounds.size.x;
    float sizeZ = wallCollider.bounds.size.z;

    Vector3 wallPos = selectedWall.transform.position;
    Vector3 enemyPos = enemy.transform.position;

    float distToWall = Vector3.Distance(enemyPos, wallPos);
	Vector3 dir = (wallPos - enemyPos).normalized;
    // ----------------------
    // Approach wall
    // ----------------------
    if(distToWall > min_distance && PointA == Vector3.zero && PointB == Vector3.zero)
    {
        

if(dir != Vector3.zero)
{
    enemy.transform.rotation = 
        Quaternion.LookRotation(dir) * Quaternion.Euler(0,180f,0);
}

        enemy.transform.position = Vector3.MoveTowards(
            enemyPos,
            wallPos,
            movementSpeed * Time.deltaTime);
        return;
    }

    // ----------------------
    // Calculate points once
    // ----------------------
    if(PointA == Vector3.zero)
    {
        PointA = new Vector3(
            wallPos.x - sizeX/2,
            enemyPos.y,
            wallPos.z + (sizeZ/2 * 1.2f));
    }

    if(PointB == Vector3.zero)
    {
        PointB = new Vector3(
            wallPos.x - sizeX/2,
            enemyPos.y,
            wallPos.z - (sizeZ/2 * 1.2f));
    }

    // ----------------------
    // Move to PointA
    // ----------------------
    if(!passedA)
    {
        Vector3 dirA = (PointA - enemyPos).normalized;
        
if(dir != Vector3.zero)
{
    enemy.transform.rotation = 
        Quaternion.LookRotation(dir) * Quaternion.Euler(0,180f,0);
}

        enemy.transform.position = Vector3.MoveTowards(
            enemyPos,
            PointA,
            movementSpeed * Time.deltaTime);

        if(Vector3.Distance(enemyPos, PointA) <= threshold)
            passedA = true;

        return;
    }

    // ----------------------
    // Move to PointB
    // ----------------------
    if(!passedB)
    {
        Vector3 dirB = (PointB - enemyPos).normalized;
        
if(dir != Vector3.zero)
{
    enemy.transform.rotation = 
        Quaternion.LookRotation(dir) * Quaternion.Euler(0,180f,0);
}

        enemy.transform.position = Vector3.MoveTowards(
            enemyPos,
            PointB,
            movementSpeed * Time.deltaTime);

        if(Vector3.Distance(enemyPos, PointB) <= threshold)
            passedB = true;

        return;
    }

    // ----------------------
    // Reset state
    // ----------------------
    nearestWall = null;
    selectedWall = null;

    IsMovingToPlayer = true;

    PointA = Vector3.zero;
    PointB = Vector3.zero;

    passedA = false;
    passedB = false;
}

void Searching()
{
    float distToPlayer = Vector3.Distance(
        enemy.transform.position,
        player.transform.position);

    // -------------------------
    // Move toward player
    // -------------------------
    if(distToPlayer > attackDistance)
    {
        Vector3 dir =
            (player.transform.position - enemy.transform.position).normalized;

        enemy.transform.position += dir * moveSpeed * Time.deltaTime;

        if(dir != Vector3.zero)
        {
            enemy.transform.rotation =
                Quaternion.Slerp(
                    enemy.transform.rotation,
                    Quaternion.LookRotation(dir) * Quaternion.Euler(0,180f,0),
                    Time.deltaTime * 5f);
        }
    }

    // -------------------------
    // Check detection cone + LOS
    // -------------------------
    if(IsPlayerInVisionCone())
    {
        if(distToPlayer < attackDistance)
        {
            currentState = EnemyState.Attack;
        }
    }
}
    void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;
        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                player = p;
            }
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        if(IsMovingToPlayer==true)
		transform.position += direction * moveSpeed * Time.deltaTime;
      //  transform.rotation = Quaternion.LookRotation(direction)* Quaternion.Euler(0,180f,0);
	  if(direction != Vector3.zero)
{
    enemy.transform.rotation = 
        Quaternion.LookRotation(direction) * Quaternion.Euler(0,180f,0);
}
    }

    void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            takingCover = false;
            RotateTowardsPlayer();
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    void AvoidWall()
    {
        if (nearestWall == null) return;

        avoidingWall = true;

        Collider col = nearestWall.GetComponent<Collider>();
        float wallHalfLengthZ = col.bounds.size.z / 2f;
        float offset = wallHalfLengthZ + (wallHalfLengthZ * wallOffsetPercent);

        Vector3 wallPos = col.bounds.center;

        // Pokud je zeď blíž hráči než enemy → krytí
        float wallToPlayer = Vector3.Distance(wallPos, player.transform.position);
        float enemyToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (wallToPlayer < enemyToPlayer)
        {
            takingCover = true;

            targetPosition = new Vector3(
                wallPos.x,
                transform.position.y,
                wallPos.z - offset
            );

            return;
        }

        // Obcházení
        if (transform.position.x < wallPos.x)
        {
            transform.Rotate(Vector3.up, -90f);
        }
        else
        {
            transform.Rotate(Vector3.up, 90f);
        }

        targetPosition = transform.position + transform.forward * col.bounds.size.x;
    }
}