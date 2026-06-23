using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBubbleMovement : MonoBehaviour
{
	public GameObject player,enemy, FirePoint, generated_gun, muzzlePoint, HitPlayer;
	public GameObject[] allEnemies;
	public float range = 80.0f, distance, maxDistance = 220.0f, minDistance = 120.0f, shooting_dist = 90.0f, scanAngle=15.0f, scanTimer=200.0f, minShootDelay = 0.5f, maxShootDelay=2.0f, randomShootTime, scanInterval=2000.0f;
	public float detection_range = 20.0f;
	public float moveSpeed = 25.0f, ShootingCooldown = 2.0f;
	public Vector3 moveDirection;
	public float forwardRayDistance = 200.0f, shootTimer, rifleRange = 80.0f;
	public RaycastHit hit;
	public LineRenderer bulletTrail;
	public bool IsMovingBackWards, IsMovingForwards, AreEnemiesCounterAttacking, playerDetected;
	public Vector3 player_last_position_when_moves_forward, player_new_position_when_moves_backwards;
	public Vector3 playerPositionWhenEnemyStartedRetreat;
	public string SelectedGenericEnemyName = "RedEnemy"; //can contain "TalibanWarrior", for testing purposes
	enum EnemyRole
{
    Shooter,
    RunToCover,
    Flank
}

EnemyRole role;

	Dictionary<int, EnemyRole> enemyBehaviourRole = new Dictionary<int, EnemyRole>();
	//alternativa Dictionary<GameObject, EnemyRole> enemyBehaviourRole = new Dictionary<GameObject, EnemyRole>();
	
	public void GoToHideout()
	{
	}
	public void Start()
	{
		int r = Random.Range(0, 3);//nahodny vyber chovani

		if(r == 0) role = EnemyRole.Shooter;
		if(r == 1) role = EnemyRole.RunToCover;
		if(r == 2) role = EnemyRole.Flank;
		
		allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
	}
	#region Update Function
	public void Update()
    {
		player = GameObject.Find("DesertReaper");
		if(IsMovingBackWards==true)
		{
		}
		//minDistance = range;
		#region foreach all enemies
		foreach(GameObject go in allEnemies)
		{
			if(go.name.Contains(SelectedGenericEnemyName))
			{
				enemy = go;//make it simple
				distance = Vector3.Distance(go.transform.position, player.transform.position);
				if(go.transform.position.y<2.4f)
					go.transform.position = new Vector3(go.transform.position.x, 2.4f, go.transform.position.z);
			if (distance < minDistance)
			{
				Vector3 moveDirection;

				// hráč je moc blízko → enemy chce ustoupit
					if(distance < minDistance)
					{
					moveDirection = (go.transform.position - player.transform.position).normalized;
					if(go.name.Contains("TalibanWarrior"))
					PlayAnimationBackwards();//animation backwards
					// pohyb směrem od hráče
					go.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
					Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
					go.transform.rotation = Quaternion.Lerp(go.transform.rotation, targetRotation, 0.05f);
					player_last_position_when_moves_forward = player.transform.position;
					// zkontrolujeme, jestli za ním není zeď
					if(Physics.Raycast(go.transform.position, moveDirection, detection_range))
					{
					// místo dozadu se pohne do strany
					moveDirection = go.transform.right;
					if(IsMovingBackWards == false) // uloží se jen jednou
						{
							playerPositionWhenEnemyStartedRetreat = player.transform.position;
						}

						IsMovingBackWards = true;
						IsMovingForwards = false;
					Debug.DrawLine(go.transform.position, player.transform.position, Color.blue);
					}
			}
				
			}
			
			if(IsMovingBackWards == true)
				{
				float retreatDistance = Vector3.Distance(
				playerPositionWhenEnemyStartedRetreat,
				player.transform.position
				);

				if(retreatDistance >= 30.0f)
					{
						AreEnemiesCounterAttacking = true;
					}
				}
			
			if (distance > maxDistance && distance > shooting_dist)
			{
				// směr k hráči
				moveDirection = (player.transform.position - go.transform.position).normalized;

				// pohyb směrem k hráči
				FindNearestWallForHideout(go);
				if (!EnemyAssignedWall_dictionary.ContainsKey(go))
				{
					// enemy ještě nemá přiřazenou zeď → jde přímo k hráči
					go.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
				}
				else
				{
					// enemy už má svou zeď → jde se schovat
				//	GoToHideout(EnemyAssignedWall_dictionary[go]);
				}
				IsMovingBackWards = false; IsMovingForwards = true;
				player_new_position_when_moves_backwards = player.transform.position;
				PlayAnimationForwards();//animation forwards
				// plynulé otočení k hráči
					Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
					go.transform.rotation = Quaternion.Lerp(go.transform.rotation, targetRotation, 0.05f);
				}				
			if(Vector3.Distance(player_last_position_when_moves_forward, player_new_position_when_moves_backwards)>=30.0f)
			{
				AreEnemiesCounterAttacking = true; //so they can shoot
				
			//	foreach(GameObject go in allEnemies)
				{
					if(go.name.Contains(SelectedGenericEnemyName))
					{
						//tak kazdy zacne utocit...
					}
				}
			}
		
		float offset_z = 10.0f;

RaycastHit hit;

// ray z enemy směrem k hráči
Vector3 directionToPlayer = (player.transform.position - go.transform.position).normalized;
/*
//additional cycles for detection wall in front etc.
if (Physics.Raycast(go.transform.position, directionToPlayer, out hit, detection_range))
{
    // pokud je mezi nimi zeď
    if (hit.collider.CompareTag("Wall"))
    {
        // pozice ZA zdí (na opačné straně od hráče)
        Vector3 hidePosition = hit.point - directionToPlayer * offset_z;

        // směr k místu za zdí
        moveDirection = (hidePosition - go.transform.position).normalized;
		
		go.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
//Kód s otočením na hráče za zdí
if (Physics.Raycast(go.transform.position, moveDirection, out hit, detection_range))
{
    if (hit.collider.CompareTag("Wall"))
    {
        offset_z = 10.0f;

        // směr k hráči
       directionToPlayer = (player.transform.position - go.transform.position).normalized;

        // pozice ZA zdí
        Vector3 hidePosition = hit.point - directionToPlayer * offset_z;

        // pohyb k místu za zdí
        moveDirection = (hidePosition - go.transform.position).normalized;

        go.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // ===== OTOČENÍ NA HRÁČE =====
        Vector3 lookDirection = player.transform.position - go.transform.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            go.transform.rotation = Quaternion.Lerp(go.transform.rotation, targetRotation, 0.05f);
			
			//Shoot
			Shoot();
        }
    }
	}*/
	}//ending foreach
	/*
	foreach(GameObject go in allEnemies) //nahodna role
	{
    if(go.name.Contains(SelectedGenericEnemyName))
    {
        if(!enemyBehaviourRole.ContainsKey(go)) // jen jednou!
        {
            int r = Random.Range(0, 3);

            if(r == 0) enemyBehaviourRole[go] = EnemyRole.Shooter;
            if(r == 1) enemyBehaviourRole[go] = EnemyRole.RunToCover;
            if(r == 2) enemyBehaviourRole[go] = EnemyRole.Flank;
        }
    }
	}
	
	foreach(GameObject go in allEnemies)
{
    if(go.name.Contains(SelectedGenericEnemyName))
    {
        EnemyRole role = enemyBehaviourRole[go];

        if(role == EnemyRole.Shooter)
        {
            Shoot();
        }

        if(role == EnemyRole.RunToCover)
        {
            MoveToCover();
        }

        if(role == EnemyRole.Flank)
        {
            MoveSideways();
        }
    }
}*/
}//here ends foreach
#endregion

// kontrola překážky před nepřítelem

if (Physics.Raycast(enemy.transform.position, moveDirection, out hit, detection_range))
{
    if (hit.collider.CompareTag("Wall"))
    {
		/**/
        // zkusí cestu doprava
        if (!Physics.Raycast(enemy.transform.position, enemy.transform.right, detection_range))
        {
            moveDirection = enemy.transform.right;
        }
        // jinak zkusí doleva
        else if (!Physics.Raycast(enemy.transform.position, -enemy.transform.right, detection_range))
        {
            moveDirection = -enemy.transform.right;
        }
        // když je zeď i vpravo i vlevo → otočí se zpět
        else
        {
            moveDirection = -moveDirection;
        }
		
		
    }
}
	}//here ends function
	#endregion
	 public GameObject last_wall, nearestWall;
	 public Dictionary<GameObject, GameObject> EnemyAssignedWall_dictionary = new Dictionary<GameObject, GameObject>(); 
	 public float last_wall_distance, wall_distance;
	 public void FindNearestWallForHideout(GameObject enemy_go)
	{
    GameObject[] allWalls = GameObject.FindGameObjectsWithTag("Wall");

    float closestDistance = Mathf.Infinity;
    GameObject nearestWall = null;

    foreach(GameObject wall in allWalls)
    {
        float distance = Vector3.Distance(enemy_go.transform.position, wall.transform.position);

        if(distance < closestDistance)
        {
            closestDistance = distance;
            nearestWall = wall;
        }
    }

    if(nearestWall != null)
    {
        EnemyAssignedWall_dictionary[enemy_go] = nearestWall;
	//	ConstructWayToAssignedWall(EnemyAssignedWall_dictionary[enemy_go]);
    }
		
	}
	Animator anim;

	//float shootTimer = 0f;
	float hideTimer = 0f;

	bool isHiding = false;
	bool isShooting = false;

	
	
	void CoverShootCycle()
{
    // když se právě schovává → nic nedělá
    if(isHiding)
    {
        hideTimer += Time.deltaTime;

        if(hideTimer >= 2.5f)
        {
            isHiding = false;
            hideTimer = 0f;

            // animace vstání
            anim.SetInteger("standup_parameter", 1);
        }

        return;
    }

    // čeká než náhodně vystřelí
    shootTimer += Time.deltaTime;

    if(shootTimer >= randomShootTime)
    {
        ShootBeam();

        shootTimer = 0f;
        randomShootTime = Random.Range(minShootDelay, maxShootDelay);

        // spustí animaci střelby
        anim.SetInteger("shoot_parameter", 1);

        // hned po střelbě se schová
        anim.SetInteger("hide_parameter", 1);
        isHiding = true;
    }
}

void ShootBeam()
{
    Vector3 origin = transform.position + Vector3.up * 1.2f;
    Vector3 direction = transform.forward;

    Ray ray = new Ray(origin, direction);

    if (Physics.Raycast(ray, out RaycastHit hit, rifleRange))
    {
        Debug.DrawLine(origin, hit.point, Color.red, 1f);
    }

    if (bulletTrail != null)
    {
        bulletTrail.SetPosition(0, origin);
        bulletTrail.SetPosition(1, origin + direction * rifleRange);
    }
}
	public void ScanForPlayer()
{
    scanTimer += Time.deltaTime;

    // každou sekundu se náhodně otočí
    if(scanTimer >= scanInterval && playerDetected == false)
    {
        float randomAngle = Random.Range(-scanAngle, scanAngle);
        transform.Rotate(0f, randomAngle, 0f);

        scanTimer = 0f;
    }

    // Raycast dopředu
    Ray ray = new Ray(FirePoint.transform.position, FirePoint.transform.forward);
    RaycastHit hit;

    if(Physics.Raycast(ray, out hit, 100f))
    {
        if(hit.collider.CompareTag("Player"))
        {
            playerDetected = true;
        }
    }

    // když už hráče našel → zamkne pozici a začne střílet
    if(playerDetected == true)
    {
        LockUpPosition();
        ShootToPlayer();
    }
}

public void LockUpPosition()
{
    Vector3 direction = player.transform.position - transform.position;
    direction.y = 0f;

    Quaternion targetRotation = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
}
	public void ShootToPlayer()
	{
		RaycastForward();//raycastuje hrace
	}
	public void PlayAnimationBackwards()
{
	Animator anim = enemy.transform.GetComponent<Animator>();
	anim.Play("walk_backwards");
}

public void PlayAnimationForwards()
{
	Animator anim = enemy.transform.GetComponent<Animator>();
	anim.Play("TalibCrouching");
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
	
}

void FireRifle()
{
    // jednoduchý Raycast střelby
    Ray ray = new Ray(FirePoint.transform.position, FirePoint.transform.forward);
    if (Physics.Raycast(ray, out RaycastHit hit, rifleRange))
    {
        Debug.DrawLine(FirePoint.transform.position, hit.point, Color.red, 1f);
 
    }

    if (bulletTrail != null)
    {
        bulletTrail.SetPosition(0, FirePoint.transform.position);
        bulletTrail.SetPosition(1, FirePoint.transform.position + FirePoint.transform.forward * rifleRange);
    }
}
	public void Shoot()
	{
		// střelba
        shootTimer += Time.deltaTime;
        if (shootTimer >= ShootingCooldown)
        {
            FireRifle();
            shootTimer = 0f;
        }
		RaycastForward();
		
	}

}