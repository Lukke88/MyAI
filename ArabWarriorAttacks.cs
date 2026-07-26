using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArabWarriorAttacks : MonoBehaviour
{
	public GameObject enemy;
	public GameObject nearestPlayer, lastPlayer, bullet, gunMuzzleObject, gunslot, gunPrefab, gunGenerated;
	public float nearest_player_distance, nearest_player_angle, x_dist, z_dist;
	public float attack_distance = 300.0f;
	public Vector3 target_point;
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
	
	public float min_shoot_delay = 1.5f;
	public float max_shoot_delay = 4.0f;

	private float shoot_timer;
	private float next_shoot_time;
	
	public GameObject muzzleObject, fireMuzzlePoint;
	public AudioClip shootSound, impactSound;
    // Start is called before the first frame update
    void Start()
	{
	enemy = GameObject.Find(this.name);

	next_shoot_time =
		Random.Range(
			min_shoot_delay,
			max_shoot_delay
		);
	}

    // Update is called once per frame
    void Update()
    {
		if(enemy==null)
			enemy = GameObject.Find(this.name);
        if(nearestPlayer==null)
		{
	nearestPlayer = FindNearestPlayer();
	
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

	}
	if(gunslot==null)
	{
		FindGunslotInChildren("gunslot", "AKM");
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
					new GameObject("gunFirePoint");

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
		}
    }//end of Update()
	public GameObject generatedProjectile;
public int bullet_counter;
public Vector3 hit_point;

public void ShootToPlayer()//shoots bullet to player
{
	GameObject projectilePrefab = GameObject.Find("projectile");

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
	
	public GameObject FindNearestPlayer()
{
	GameObject[] allPlayers =
		GameObject.FindGameObjectsWithTag("PlayerVehicle");

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
