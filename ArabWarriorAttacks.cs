using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArabWarriorAttacks : MonoBehaviour
{
	[Header("Basic variables")]
	public GameObject enemy, generatedFlash, muzzleFlashPrefab;
	public GameObject nearestPlayer, lastPlayer, bullet, generatedBullet, gunMuzzleObject, gunslot, gunPrefab, gunGenerated, muzzleFirePrefab;
	public Transform gunWeaponPivot;
	public float nearest_player_distance, nearest_player_angle, x_dist, z_dist;
	public string enemy_bullet_name = "bullet_Nagant3_enemy";
	public float attack_distance = 7000.0f;//6748.0f was testing distance for attack, so 7000.0f far enemy should attack
	public Vector3 target_point;
	[Header("Pointing system variables")]
	public int points_for_dead_enemy = 50;
	public bool WasDeadEnemyCounted, WasEnemySubstituted;//if third is false, GenerationHouse can send there new enemy
	[Header("Names of Actions")]
	public string idle_animation = "ActionIdle";
	public string run_animation = "ArabDrunkRun";
	public string shoot_animation ="ArabGunplay1";
	public string shoot_peek_animation = "ArabShootAndHide";
	public string headshot_animation = "ArabHeadShot";
	public string dance_animation = "HipHopArab";
	public string jumping_animation = "JumpingInTheRun";
	public string rib_hit_animation = "RibHit";
	public string kill_shot_animation = "KillShot";
	public string idle3_animation = "Idle3";
	
	public string run_param = "RunArab";
	public string idle_param = "IdleArab";
	public string shoot_param = "ShootingArab";
	public string hide_param = "HidePeekArab";
	public string headshot_param = "HeadshotArab";
	public string dancing_param = "ArabDancing";
	public string jumping_param = "JumpingArab";
	public string rib_hit_param = "RibHitArab";
	public string kill_param = "KilledArab";
	public string idle3_param = "BoredArab";
	[Header("Time delay")]
	public float min_shoot_delay = 1.5f;
	public float max_shoot_delay = 4.0f;
	[Header("Shooting variables")]
	public GunEnemyBehaviour enemyGun;

	private float shoot_timer;
	private float next_shoot_time;
	
	public GameObject muzzleObject, fireMuzzlePoint;
	public AudioClip shootSound, impactSound;
	
	public enum ActionStates
	{
		Idle,
		Shooting,
		Chasing
	};
	[Header("name variablkes")]
	public string selectedTag = "Player";//searches nearest player in FindNearestPlayer(), tag can be "PlayerVehicle"
	public string projectile_name = "projectile";
	public string gun_fire_point_name = "gunFirePoint";
	public string gun_weapon_pivot_name = "gunWeaponPivot";
	public string gun_prefab_name = "AKM";
	public float generatedWeaponScale = 0.01f;
	public ActionStates currentState;
	public float actionStateSwitchDistance = 8000.0f;//over this distance will change to chase
	public float nearest_horizontal_distance = 50.0f;
	public float generated_muzzle_scale = 436.6558f;
	public bool IsSearchingAnotherCover;
	public GameObject[] allBarricades;//hide behind barricade
    // Start is called before the first frame update
    void Start()
	{
	enemy = GameObject.Find(this.name);
	allBarricades = GameObject.FindGameObjectsWithTag("Barricade");
	enemyGun = GetComponent<GunEnemyBehaviour>();
	muzzleFlashPrefab = GameObject.Find("Capsule");//eventually "FlashProjectilePlayer"

	if (enemyGun == null)
		enemyGun = gameObject.AddComponent<GunEnemyBehaviour>();

	enemyGun.Initialize(this);

	next_shoot_time =
		Random.Range(
			min_shoot_delay,
			max_shoot_delay
		);
		
		//Transform gunWeaponPivot = transform.Find("gunWeaponPivot");

if (gunWeaponPivot != null)
{
    InstantiateNewWeaponAtGunSlot(gun_prefab_name);
}
else
{
	InstantiateNewWeaponAtGunSlot(gun_prefab_name);//searches gun pivot first, then attaches weapon
    Debug.LogWarning(gun_weapon_pivot_name + " not found!");
}
	}
	/*
	- FlashProjectilePlayer přiřaďte do Projectile Prefab.
- WFX_MF 4P RIFLE1 přiřaďte do Muzzle Flash Prefab.
- Ověřte název child objektu firePoint.
- Zkontrolujte, že projektil letí lokálním směrem dopředu (+Z).
Pozor: GameObject.Find() najde pouze aktivní objekty ve scéně, nikoliv nepoužité prefab assety v Project. Proto doporučuji oba prefaby přiřadit přímo v Inspectoru.

Co očekávám po spuštění:
1. ArabWarrior získá cíl z CameraFollowsHero.
2. Najde svou zbraň Gun_ArabWarrior (12).
3. Vytvoří EnemyWeaponPivot s požadovanou rotací.
4. Najde firePoint a připojí muzzle flash.
5. V náhodných intervalech vystřeluje projektily na hráče.
6. Při každém výstřelu spustí animaci ArabGunplay1.
	*/
    // Update is called once per frame
    void Update()
    {
		//nearestPlayer = Camera.main.GetComponent<CameraFollowsHero>().target.gameObject;//turns automatically to player active in camera view
		
		if (Camera.main != null)
{
    CameraFollowsHero cameraFollow =
        Camera.main.GetComponent<CameraFollowsHero>();

    if (cameraFollow != null &&
        cameraFollow.target != null)
    {
        nearestPlayer = cameraFollow.target.gameObject;
    }
}
if(muzzleFlashPrefab==null)
	muzzleFlashPrefab = GameObject.Find("Capsule");//eventually "FlashProjectilePlayer"
if(muzzleFirePrefab==null)muzzleFirePrefab = GameObject.Find("WFX_MF 4P RIFLE1");

nearestPlayer = GameObject.Find("HeroSoldier");
if(nearestPlayer!=null)nearest_player_distance = Vector3.Distance(enemy.transform.position, nearestPlayer.transform.position);
if(Vector3.Distance(enemy.transform.position, nearestPlayer.transform.position)<actionStateSwitchDistance)
{
	currentState = ActionStates.Shooting;
}
else if(currentState==ActionStates.Shooting && Vector3.Distance(enemy.transform.position, nearestPlayer.transform.position)>actionStateSwitchDistance)
{
	currentState= ActionStates.Chasing;
}
else//if distance > actionStateSwitchDistance && currentState!=Shooting
{
	currentState = ActionStates.Idle;
}
if(gunWeaponPivot==null)
gunWeaponPivot = transform.Find(gun_weapon_pivot_name);//attach GunEnemyBehaviour to generated null object, containing all gun parts(therefore we can turn weapon in all axis)

if (gunWeaponPivot != null)
{
	InstantiateNewWeaponAtGunSlot(gun_prefab_name);
}
else
{
    Debug.LogWarning(gun_weapon_pivot_name + " not found!");
}

if (nearestPlayer == null)
{
    nearestPlayer = FindNearestPlayer(selectedTag);
}
if(currentState==ActionStates.Shooting)
{
if (nearestPlayer != null && enemyGun != null)
{
    enemyGun.SetTarget(nearestPlayer.transform);
}
		
		if(enemy==null)
			enemy = GameObject.Find(this.name);
		if(bullet==null)
			bullet = GameObject.Find(enemy_bullet_name);
        if(nearestPlayer==null)
		{
	nearestPlayer = FindNearestPlayer(selectedTag);
	
	}
	else if(nearestPlayer!=null)
	{
	TurnToNearestPlayer(nearestPlayer);
	
		shoot_timer += Time.deltaTime;

		if(shoot_timer >= next_shoot_time)
		{
			ShootAtNearestPlayer();

			shoot_timer = 0.0f;

			next_shoot_time =
				Random.Range(
					min_shoot_delay,
					max_shoot_delay
				);
		}
		
		if(target_point!=Vector3.zero && fireMuzzlePoint!=null)
				{
					//ShootToPlayer();
					SimpleShootingFunction();
				}


	}
	if(gunslot==null)
	{
		FindGunslotInChildren("gunslot", gun_prefab_name);//generates a gun
	}
	else if(gunslot!=null)
		{
			
				
			if(gunGenerated!=null)
			{
				
				if(nearestPlayer!=null && fireMuzzlePoint.transform.childCount<1)
				{
					nearest_player_distance = Vector3.Distance(nearestPlayer.transform.position, enemy.transform.position);
					x_dist = Mathf.Abs(nearestPlayer.transform.position.x - enemy.transform.position.x);
					z_dist = Mathf.Abs(nearestPlayer.transform.position.z - enemy.transform.position.z);
					nearest_player_angle = Mathf.Atan(z_dist/nearest_player_distance)*180/Mathf.PI;
					
					gunslot.transform.rotation = Quaternion.Euler(0, nearest_player_angle,0);
					gunGenerated.transform.rotation = Quaternion.Euler(0,enemy.transform.localRotation.y,0);
					fireMuzzlePoint = gunGenerated.transform.GetChild(0).gameObject;
					//add muzzle fire point to end of the gun and attach it like child
					if(muzzleObject==null)
					{
					muzzleObject =
					new GameObject(gun_fire_point_name);

					muzzleObject.transform.SetParent(
					fireMuzzlePoint.transform
					);

					muzzleObject.transform.localPosition =
					new Vector3(
					0.0f,
					0.0f,
					1.0f
					);

					muzzleObject.transform.localRotation =
					Quaternion.identity;

					gunMuzzleObject =
					muzzleObject;
					
					target_point = nearestPlayer.transform.position;
					}
				}
				
				if(target_point!=Vector3.zero && fireMuzzlePoint!=null)
				{
					ShootToPlayer();
				}
			}
			else if(gunGenerated==null)
			{
				if(bullet==null)
					bullet = GameObject.Find(enemy_bullet_name);
				if(muzzleFirePrefab==null)muzzleFirePrefab = GameObject.Find("WFX_MF 4P RIFLE1");
				else
				{
					SimpleShootingFunction();
				}
			}
		}
}//end of ActionStates.Shooting block
else if(currentState==ActionStates.Chasing)
{
	if(CheckIfEnemyIsOnGround())
    {
        SearchNearestBarricadeHideout();

        GoToNearestBarricade(
            320.0f,//speed
            run_animation,
            idle_animation,
            shoot_peek_animation
        );
    }
    else
    {
        // Enemy is inside apartment.
        // Different tactical behaviour here.
    }
}
		
    }//end of Update()
	
	public void InstantiateNewWeaponAtGunSlot(string weapon_prefab_name)
{
	string slot_name = gun_weapon_pivot_name;
	Transform[] children = enemy.GetComponentsInChildren<Transform>(true);
	foreach(Transform go in children)
	{
		if(go.name.Contains(slot_name))
		{
			gunWeaponPivot = go;//already is transform
		}
	}
    // Ideálně bys měl mít prefab přiřazený v inspectoru nebo načítat přes Resources/Addressables
    gunPrefab = GameObject.Find(weapon_prefab_name); // stále není ideální

    GunEnemyBehaviour gunBehaviour = null;
	GunFiringToPlayer gunFiringToPlayer = null;
    if (gunWeaponPivot.childCount == 0)
    {
		if(gunWeaponPivot!=null)
		{
        gunGenerated = Instantiate(gunPrefab, gunWeaponPivot.position, gunWeaponPivot.rotation, gunWeaponPivot);
	
		gunGenerated.transform.localScale =
					new Vector3(
					generatedWeaponScale,
					generatedWeaponScale,
					generatedWeaponScale
					);
		}
        // nebo jednodušeji:
        // gunGenerated = Instantiate(gunPrefab, gunWeaponPivot);

        gunBehaviour = gunGenerated.GetComponent<GunEnemyBehaviour>();
		gunFiringToPlayer = gunWeaponPivot.GetComponentInChildren<GunFiringToPlayer>();
    }
    else
    {
        // Pokud už zbraň existuje, vezmi komponentu z ní
        gunBehaviour = gunWeaponPivot.GetComponentInChildren<GunEnemyBehaviour>();
		gunFiringToPlayer = gunWeaponPivot.GetComponentInChildren<GunFiringToPlayer>();
    }

    // Fallback – kdyby komponenta neexistovala
    if (gunBehaviour == null)
    {
        gunBehaviour = gunWeaponPivot.gameObject.AddComponent<GunEnemyBehaviour>();
    }
	if(gunFiringToPlayer==null)
	{
		gunFiringToPlayer = gunWeaponPivot.GetComponentInChildren<GunFiringToPlayer>();
		
	}

    gunBehaviour.enabled = true;
    Debug.Log("GunEnemyBehaviour initialized on "+gun_weapon_pivot_name);
}
	
	public GameObject nearestGround;
public float enemyHeight = 2.0f;
public bool IsOnGround;

public bool CheckIfEnemyIsOnGround()
{
    GameObject[] grounds =
        GameObject.FindGameObjectsWithTag("Ground");

    float nearestDistance = Mathf.Infinity;
    nearestGround = null;

    foreach(GameObject go in grounds)
    {
        float distance = Vector3.Distance(
            enemy.transform.position,
            go.transform.position
        );

        if(distance < nearestDistance)
        {
            nearestDistance = distance;
            nearestGround = go;
        }
    }

    if(nearestGround == null)
        return false;

    IsOnGround = nearestDistance <= enemyHeight;

    return IsOnGround;
}
	
	public GameObject nearestBarricade;

	public void SearchNearestBarricadeHideout()
	{
		nearestBarricade = null;
		float nearestBarricadeDistance = Mathf.Infinity;
		foreach(GameObject go in allBarricades)
		{
			if(nearestBarricade==null)
			{
				if(Vector3.Distance(nearestPlayer.transform.position, go.transform.position)<actionStateSwitchDistance ||Vector3.Distance(nearestPlayer.transform.position, go.transform.position)<nearestBarricadeDistance)
				{
					nearestBarricade = go;
				}
			}
		}
	}	
	
	public void GoToNearestBarricade(float enemy_movement_speed, string movement_animation, string idling_animation, string shooting_animation)
	{
		float z_offset = 20.0f;
		if(nearestBarricade!=null)
		{
			Vector3 WaypointBehindBarricade = new Vector3(nearestBarricade.transform.position.x, enemy.transform.position.y, nearestBarricade.transform.position.z + z_offset);

				enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, WaypointBehindBarricade, enemy_movement_speed*Time.deltaTime);
				Animator animator = enemy.transform.GetComponent<Animator>();
				if(Vector3.Distance(enemy.transform.position, WaypointBehindBarricade)>20.0f)
				{
					animator.Play(movement_animation);
				}
				else if(Vector3.Distance(enemy.transform.position, WaypointBehindBarricade)<20.0f)
				{
					animator.Play(idle_animation);
					animator.Play(shooting_animation);
					TurnToNearestPlayer(nearestPlayer);
					currentState=ActionStates.Shooting;//turns back to normal mode, switch off movement mode
				}
					
		}
	}
	public Vector3 flashFinalPosition;
	public void SimpleShootingFunction()
	{
		float flash_speed = 320.0f;
		if(fireMuzzlePoint!=null && generatedMuzzleFire==null)
					{
						generatedMuzzleFire = Instantiate(muzzleFirePrefab, fireMuzzlePoint.transform.position, fireMuzzlePoint.transform.rotation);
						generatedMuzzleFire.transform.localScale = new Vector3(436.6558f, 436.6558f, 436.6558f);
						generatedMuzzleFire.transform.position = fireMuzzlePoint.transform.position;
						generatedMuzzleFire.transform.parent = fireMuzzlePoint.transform;
					}
					else if(generatedMuzzleFire!=null)
					{
						ParticleSystem ps = generatedMuzzleFire.transform.GetComponent<ParticleSystem>();//shooting effect
						ps.Play();
						
						flashFinalPosition = nearestPlayer.transform.position;
						
						if(generatedFlash==null)
						{
							generatedFlash = Instantiate(muzzleFlashPrefab, fireMuzzlePoint.transform.position, fireMuzzlePoint.transform.rotation);
						}
						
						if(bullet!=null && generatedBullet==null)
						{
							generatedBullet = Instantiate(bullet, fireMuzzlePoint.transform.position, fireMuzzlePoint.transform.rotation);
						}
						else if(generatedBullet!=null && nearestPlayer!=null)
						{
							float bullet_speed = 300.0f;
							generatedBullet.transform.position = Vector3.MoveTowards(generatedBullet.transform.position, nearestPlayer.transform.position, bullet_speed*Time.deltaTime);
							
							if(Vector3.Distance(generatedBullet.transform.position, nearestPlayer.transform.position)<=10.0f)//practically zero distance
							{
								Destroy(generatedBullet);
							}
							else
							{
								AddTrailBehindBullet();
							}
						}
					}
					
					if(generatedFlash!=null && flashFinalPosition!=Vector3.zero)//light rays travels from enemy to player
					{
						generatedFlash.transform.position = Vector3.MoveTowards(generatedFlash.transform.position, flashFinalPosition, flash_speed*Time.deltaTime);
					}
					if(Vector3.Distance(nearestPlayer.transform.position, generatedFlash.transform.position)<=20.0f ||
					Vector3.Distance(enemy.transform.position, generatedFlash.transform.position)>Vector3.Distance(enemy.transform.position, nearestPlayer.transform.position)*1.2f)
						Destroy(generatedFlash);//if it's too far(or hits), destroy light ray
				
	}
	public void AddTrailBehindBullet()
{
    if(generatedBullet == null)
        return;

    TrailRenderer trail = generatedBullet.GetComponent<TrailRenderer>();

    if(trail == null)
    {
        trail = generatedBullet.AddComponent<TrailRenderer>();

        trail.time = 0.4f;

        trail.startWidth = 2.0f;
        trail.endWidth = 0.0f;

        trail.material = new Material(Shader.Find("Sprites/Default"));

        trail.startColor = Color.yellow;
        trail.endColor = new Color(1.0f, 1.0f, 0.0f, 0.0f);
    }
}
	public void SendFlashLineToTarget(float flash_line_length)
{
    if(generatedProjectile == null)
        return;//flash line goes from fireMuzzlePoint to target, upon hit will reflect from surface of tank and be destroyed

    LineRenderer line = generatedProjectile.AddComponent<LineRenderer>();

    line.positionCount = 2;

    line.startWidth = 0.08f;
    line.endWidth = 0.02f;

    line.material = new Material(Shader.Find("Sprites/Default"));
    line.startColor = Color.yellow;
    line.endColor = Color.yellow;

    Vector3 start = generatedProjectile.transform.position;

    Vector3 end =
        start +
        generatedProjectile.transform.forward * flash_line_length;

    line.SetPosition(0, start);
    line.SetPosition(1, end);
}
	
	public GameObject generatedProjectile;
public int bullet_counter;
public Vector3 hit_point;

public void ShootToPlayer()//shoots bullet to player
{
	GameObject projectilePrefab = GameObject.Find(projectile_name);

	if(projectilePrefab == null)
		return;

	if(generatedProjectile == null)
	{
		
		generatedProjectile =
			Instantiate(
				projectilePrefab,
				fireMuzzlePoint.transform.position,
				fireMuzzlePoint.transform.rotation
			);
			CreateShootSound(shootSound);
			CreateMuzzleFlash("WFX_MF 4P RIFLE2",fireMuzzlePoint.transform.position);
			CreateProjectileTrail();
			SendFlashLineToTarget(50.5f);
		generatedProjectile.name =
			"enemy_bullet_" + bullet_counter;

		Projectile_Behaviour pb =
			generatedProjectile.GetComponent<Projectile_Behaviour>();

		if(pb != null)
		{
			pb.targetPoint = target_point;
		}

		bullet_counter++;
	}

	ProjectileRaycastsForward(generatedProjectile);
}
public GameObject generatedMuzzleFlash;
public void CreateMuzzleFlash(string muzzleName, Vector3 muzzle_transform_position)
{
	GameObject muzzleFlashPrefab = GameObject.Find(muzzleName);
	if(generatedMuzzleFlash==null)
	{
		generatedMuzzleFlash = Instantiate(muzzleFlashPrefab, muzzle_transform_position, enemy.transform.localRotation);
		ParticleSystem ps = generatedMuzzleFlash.transform.GetComponent<ParticleSystem>();
		ps.Play();
		
		Destroy(generatedMuzzleFlash, 2.0f);
	}
}
public void CreateProjectileTrail()
{
	if(generatedProjectile == null)
		return;

	GameObject trailPrefab =
		GameObject.Find("ProjectileTrail");

	if(trailPrefab == null)
		return;

	GameObject generatedTrail =
		Instantiate(
			trailPrefab,
			generatedProjectile.transform.position,
			generatedProjectile.transform.rotation,
			generatedProjectile.transform
		);
}

public void CreateShootSound(AudioClip audioClip)
{
	if(audioClip == null)
		return;

	if(gunMuzzleObject == null)
		return;

	AudioSource.PlayClipAtPoint(
		audioClip,
		gunMuzzleObject.transform.position
	);
}

public void CreateImpactSound(
	Vector3 impact_point,
	AudioClip audioClip
)
{
	if(audioClip == null)
		return;

	AudioSource.PlayClipAtPoint(
		audioClip,
		impact_point
	);
}

public void ProjectileRaycastsForward(GameObject newGeneratedProjectile)
{
	if(newGeneratedProjectile == null)
		return;

	float raycast_distance = 20.0f;

	Ray ray =
		new Ray(
			newGeneratedProjectile.transform.position,
			newGeneratedProjectile.transform.forward
		);

	RaycastHit hit;

	if(Physics.Raycast(ray, out hit, raycast_distance))
	{
		hit_point = hit.point;

		CreateSmallExplosionOnImpact(
			hit_point,
			newGeneratedProjectile
		);
	}
}


public void CreateSmallExplosionOnImpact(
	Vector3 impact_point,
	GameObject genProjectile
)
{
	GameObject explosion_prefab =
		GameObject.Find("WFX_ExplosiveSmoke Small");

	if(explosion_prefab == null)
		return;

	GameObject generated_explosion =
		Instantiate(
			explosion_prefab,
			impact_point,
			Quaternion.identity
		);

	ParticleSystem ps =
		generated_explosion.GetComponent<ParticleSystem>();

	if(ps != null)
	{
		ps.Play();
	}
	CreateImpactSound(
	impact_point,
	impactSound
	);
	target_point = Vector3.zero;
	Destroy(generated_explosion, 2.0f);
	Destroy(genProjectile);
	
}

	public void FindGunslotInChildren(
	string gunslot_name,
	string gunPrefabName
)
{
	gunPrefab = GameObject.Find(gunPrefabName);

	Transform[] allChildren =
		enemy.GetComponentsInChildren<Transform>(true);

	foreach(Transform child in allChildren)
	{
		if(child.name == gunslot_name)
		{
			gunslot = child.gameObject;

			Debug.Log(
				enemy.name +
				" found gunslot: " +
				gunslot.name
			);

			if(gunslot.transform.childCount < 1)
			{
				Debug.Log(
					enemy.name +
					" gunslot is empty. Creating gun."
				);

				if(gunPrefab != null)
				{
					gunGenerated =
						Instantiate(
							gunPrefab,
							gunslot.transform.position,
							gunslot.transform.rotation,
							gunslot.transform
						);

					gunGenerated.name =
						"Gun_" + enemy.name;

					gunGenerated.transform.localScale =
						Vector3.one * 0.01f;

					Debug.Log(
						enemy.name +
						" gun created and scaled to 0.01"
					);

					Transform muzzle =
						gunGenerated.transform.Find(
							"gunFirePoint"
						);

					if(muzzle != null)
					{
						gunMuzzleObject =
							muzzle.gameObject;

						Debug.Log(
							enemy.name +
							" found gunFirePoint."
						);
					}
					else
					{
						Debug.LogWarning(
							enemy.name +
							" could not find gunFirePoint."
						);
					}
				}
			}

			return;
		}
	}

	Debug.LogWarning(
		enemy.name +
		" could not find gunslot: " +
		gunslot_name
	);
}
	public void ShootAtNearestPlayer()
{
	PlayShootingAnimation(
		nearestPlayer,
		shoot_animation,
		shoot_param,
		1
	);

	CreateShootAndTrail(
		gunMuzzleObject,
		bullet
	);

	CreateMuzzleFireAndAudioSound(
		gunMuzzleObject,
		"WFX_MF 4P RIFLE2",
		2.0f
	);
	
	DestroyLastBullet(3000.0f);//cleaning up
}
	public void DestroyLastBullet(float max_distance)
{
	GameObject[] allObjects =
		FindObjectsOfType<GameObject>();

	foreach(GameObject go in allObjects)
	{
		if(go.name.StartsWith("enemy_bullet_"))
		{
			float distance =
				Vector3.Distance(
					enemy.transform.position,
					go.transform.position
				);

			if(distance >= max_distance)
			{
				Destroy(go);
			}
		}
	}
}
	
	GameObject generatedMuzzleFire;
	public void CreateMuzzleFireAndAudioSound(GameObject gun_muzzle_null_object, string muzzlePrefabName, float muzzleFireDuration)
	{
		GameObject muzzleFirePrefab = GameObject.Find(muzzlePrefabName);
		
		if(muzzleFirePrefab!=null)
		{
			generatedMuzzleFire = Instantiate(muzzleFirePrefab, gun_muzzle_null_object.transform.position, gun_muzzle_null_object.transform.rotation);
			
			ParticleSystem ps = generatedMuzzleFire.transform.GetComponent<ParticleSystem>();
			ps.Play();
			
			Destroy(generatedMuzzleFire, muzzleFireDuration);
		}
	}

	public void CreateShootAndTrail(GameObject muzzle_null_gameobject, GameObject bullet_object)
	{
		if(bullet_object!=null)
		{
			GameObject generatedBullet = Instantiate(bullet_object, muzzle_null_gameobject.transform.position, muzzle_null_gameobject.transform.rotation);
			generatedBullet.name = "enemy_bullet_" + bullet_counter.ToString();
			if(generatedBullet!=null)
			{
				Projectile_Behaviour pb = generatedBullet.transform.GetComponent<Projectile_Behaviour>();
				bullet_counter++;
				pb.IsTriggered = true;
			}
			//use targetPoint to create target destination in front of the muzzle_null_gameobject, in attack distance
		}
	}
	
	public void PlayShootingAnimation(
	GameObject nearest_player,
	string shoot_animation_name,
	string shoot_param_name,
	int param_value
)
{
	if(nearest_player == null)
		return;

	Animator animator =
		enemy.transform.GetComponent<Animator>();

	if(animator == null)
		return;

	animator.SetInteger(
		shoot_param_name,
		param_value
	);

	animator.Play(
		shoot_animation_name
	);
}
	
	public void TurnToNearestPlayer(GameObject nearest_player)
{
	if(nearest_player == null)
		return;

	Vector3 direction =
		nearest_player.transform.position -
		enemy.transform.position;

	direction.y = 0.0f;

	if(direction != Vector3.zero)
	{
		Quaternion targetRotation =
			Quaternion.LookRotation(direction);

		enemy.transform.rotation =
			Quaternion.Slerp(
				enemy.transform.rotation,
				targetRotation,
				5.0f * Time.deltaTime
			);
	}
}
	
	public GameObject FindNearestPlayer(string current_player_tag)
{
	GameObject[] allPlayers =
		GameObject.FindGameObjectsWithTag(current_player_tag);

	float nearest_player_distance =
		Mathf.Infinity;

	GameObject nearest_player = null;

	foreach(GameObject go in allPlayers)
	{
		float distance =
			Vector3.Distance(
				go.transform.position,
				enemy.transform.position
			);

		if(distance < nearest_player_distance)
		{
			nearest_player_distance = distance;
			nearest_player = go;
		}
	}

	lastPlayer = nearest_player;

	return nearest_player;
}
}
