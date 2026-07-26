using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Projectile_Behaviour : MonoBehaviour
{
	public GameObject projectile, muzzleObject2, turret, generatedFlash, generatedProjectile, generatedExplosion, flameThrowerTail;
	public bool IsTriggered;
	 public TMP_Text infoText, pointsCounter;
	public Vector3 targetPoint;   
	public float projectileSpeed = 500f;
	public float starting_height = 170.0f;
	public float dist_from_source;
	public float max_distance_from_source = 3000.0f;
	public float hit_distance = 10.0f;
	public Vector3 hit_point;
	public string hit_animation = "RibHit";
	public string hit_anim_parameter = "RibHitArab";
	public string wall_explosion_prefab_name = "WFX_ExplosiveSmoke Small";
	public string flamethrower_tail_name = "WFX_FlameThrower Big Alt";
	public GameObject hit_object;
	public float projectile_speed = 500.0f;
	public static float time;
	
		public string rib_hit_animation = "RibHit";
	public string kill_shot_animation = "KillShot";
	
		public string rib_hit_param = "RibHitArab";
	public string kill_param = "KilledArab";
	
	public string crater_texture_name = "Crater";

	private Vector3 lastProjectilePosition;
	private bool wallHit;
	

	public LineRenderer projectileTrail;
	public TrailRenderer projectileTrail_;
	
	public GameObject mother_object;
	public float height_mother_object;
	
    void Start()
    {
        projectile = GameObject.Find(this.name);
		turret = GameObject.Find("SquareTurret");
			generatedProjectile = projectile;
		targetPoint = turret.transform.position
            + turret.transform.forward * 5000f;

		if(this.name.Contains("(Clone)"))
			IsTriggered = true;
		
		CreateFlash();//muzzle flash at the birth of the bullet
		if(this.name.Contains("mg_"))
		{
			mother_object = GameObject.Find("HumWeeDestroyed");
			height_mother_object = mother_object.transform.position.y;
		}
		projectileTrail_ = GetComponent<TrailRenderer>();

			if(projectileTrail == null)
				{
				projectileTrail_ = gameObject.AddComponent<TrailRenderer>();
				}

			CreateProjectileTrail();
			AddFlamethrowerTailToProjectile();
			lastProjectilePosition = transform.position;
    }


   void Update()
{
	if(projectile.transform.position.y<starting_height)
		projectile.transform.position = new Vector3(projectile.transform.position.x, starting_height, projectile.transform.position.z);
	GameObject cursor = GameObject.Find("Cursor_X (1)");//name can be changed later
	if(cursor!=null)//this is for testing purposes - when it's simple, then it works, better than calculate angles.
	{
		targetPoint = new Vector3(cursor.transform.position.x, height_mother_object, cursor.transform.position.z); //take direction of the cursor and extend it beyond the cursor, targetPoint = direction*3000.0f etc.
		
		if(Vector3.Distance(cursor.transform.position, mother_object.transform.position)<projectile_speed)
		{
			Vector3 direction =
		(cursor.transform.position - fireObject.transform.position).normalized;

		targetPoint =
		fireObject.transform.position +
		direction * 3000.0f;

		targetPoint.y = height_mother_object;
		}
	}
	if(IsTriggered == true)
	{
		lastProjectilePosition = transform.position;
		if(generatedProjectile==null)
			generatedProjectile = projectile;
		
		if(this.name.Contains("mg_") && targetPoint!=Vector3.zero)//humwee machinegun generated projectile
		{
			generatedProjectile.transform.position = Vector3.MoveTowards(generatedProjectile.transform.position, targetPoint, projectile_speed*Time.deltaTime);
			
			transform.localScale = Vector3.one * 60.0f;
			//rotate projectile to target
			Vector3 direction = (targetPoint - generatedProjectile.transform.position).normalized;

			if (direction != Vector3.zero)
			{
				generatedProjectile.transform.rotation = Quaternion.LookRotation(direction);
			}
			ProjectileRaycastsForward(projectile);
			
			CreateProjectileTrail();
		}
		else // generic projectile
		{
		transform.Translate(
			0,
			-projectileSpeed * Time.deltaTime,
			0.0f
		);
		}

		DetectWallHit();
	}
	
	if(projectile!=null && turret!=null)
	{
		dist_from_source = Vector3.Distance(
			projectile.transform.position,
			turret.transform.position
		);
	}
		ProjectileRaycastsForward(projectile);

	if(dist_from_source >= max_distance_from_source)
		Destroy(projectile);

	if(targetPoint.y < starting_height)
	{
		targetPoint = new Vector3(
			targetPoint.x,
			starting_height,
			targetPoint.z
		);
	}
	
	

	if(flameThrowerTail!=null) //instead of trail, we use flamethrower to mark the way of projectile
	{
		ParticleSystem ps = flameThrowerTail.transform.GetComponent<ParticleSystem>();
		ps.Play();
	}

	//DetectEnemyInRange(30.0f);
}


public void AddFlamethrowerTailToProjectile()
{
	GameObject flameThrowerTailPrefab = GameObject.Find(flamethrower_tail_name);
	if(projectile!=null && flameThrowerTail==null)
	{
		flameThrowerTail = Instantiate(flameThrowerTailPrefab, projectile.transform.position, projectile.transform.rotation);
		flameThrowerTail.transform.parent = projectile.transform;
	}
}
public float damageImpact;
public GameObject last_hit_object;
public void ProjectileRaycastsForward(GameObject newGeneratedProjectile)
{
	/*if(newGeneratedProjectile == null)
		return;*/

	float raycast_distance = 200.0f;

	Vector3 direction =
		newGeneratedProjectile.transform.up;
	if(projectile.name.Contains("mg_") && projectile.transform.rotation.eulerAngles.x!=90.0f)
	{
		projectile.transform.rotation = Quaternion.Euler(
		90.0f,
		projectile.transform.eulerAngles.y,
		0.0f
		);
		direction =
		newGeneratedProjectile.transform.up;
	}
		else if(!projectile.name.Contains("mg_") )
		{
			direction =
		-newGeneratedProjectile.transform.up;
		}
	hit_object = null;

	Ray ray = new Ray(
		newGeneratedProjectile.transform.position,
		direction
	);

	RaycastHit hit;

	if(Physics.Raycast(
		ray,
		out hit,
		raycast_distance
	))
	{
		hit_point = hit.point;
		if(last_hit_object==null || last_hit_object!=hit_object)
		{
		hit_object =
			hit.collider.gameObject;
			last_hit_object = hit_object;
		}
	}
	
	if(last_hit_object!=null)
	{
		Explode(hit_point, "WFX_Nuke");
	}
	
	Debug.DrawLine(
		newGeneratedProjectile.transform.position,
		newGeneratedProjectile.transform.position +
		direction * raycast_distance,
		Color.red
	);

	if(
		hit_object != null &&
		hit_object.name.Contains("Arab")
	)
	{
		EnemyHealth eh =
			hit_object.GetComponent<EnemyHealth>();

		if(eh != null && hit_object!=null)
		{
			damageImpact = 5.0f;
			eh.TakeDamage(damageImpact);
			
			if(eh.currentHealth <= 1)
		{
			EnemyProvideDeathSalto(hit_object);
		}
		else
		{
			EnemyGetsHit(hit_object);
		}
		last_hit_object = hit_object;
		hit_object = null;
		}
		
	}
	else if(hit_object.name.Contains("Wall") || hit_object.name.Contains("wall") ||hit_object.name.Contains("Building"))
	{
		if(hit_point!=Vector3.zero)
		{
			Explode(hit_point, "WFX_Nuke");
			infoText.text = "Projectile hit " + hit_object.name;
		}
	}

	
}

public void Explode(Vector3 impact_point, string explosion_name)
{
	GameObject explosionPrefab = GameObject.Find(explosion_name);
			if(generatedExplosion==null)
			generatedExplosion = Instantiate(
    explosionPrefab,
    impact_point,
    Quaternion.identity
);
			else
			{
				ParticleSystem ps = generatedExplosion.transform.GetComponent<ParticleSystem>();
				ps.Play();
				Destroy(generatedExplosion, 3.0f);
				Destroy(projectile);
			}
}


public int SumPoints;
public void AddPointsToCounter(int points_count)
{
	string scoreText =
	pointsCounter.text.Replace(
		"Your Score: ",
		""
	);

	int currentScore =
	int.Parse(scoreText);
	SumPoints = currentScore;
	
	SumPoints += points_count;
	pointsCounter.text = "Your Score: " + SumPoints.ToString();
}
public void EnemyProvideDeathSalto(GameObject enemy_figure)
{
	Animator animator = enemy_figure.transform.GetComponent<Animator>();
	animator.SetInteger(kill_param,1);
	animator.Play(kill_shot_animation);
	infoText.text = "Enemy was killed and lost " + damageImpact + " % of life. Congratulations to good kill !!!";
	AddPointsToCounter(50);
}

public void EnemyGetsHit(GameObject enemy_figure)
{
	Animator animator = enemy_figure.transform.GetComponent<Animator>();
	animator.SetInteger(rib_hit_param,1);
	animator.Play(hit_animation);
	infoText.text = "Enemy was hit and lost " + damageImpact + " % of life.";
}
	
	public void DetectWallHit()
{
	if(wallHit)
		return;

	Vector3 direction =
		transform.position - lastProjectilePosition;

	float distance = direction.magnitude;

	if(distance <= 0.0f)
		return;

	RaycastHit hit;

	if(Physics.Raycast(
		lastProjectilePosition,
		direction.normalized,
		out hit,
		distance
	))
	{
		GameObject wallObject = hit.collider.gameObject;

		if(
			wallObject.CompareTag("Wall") ||
			(hit.collider.transform.parent != null &&
			 hit.collider.transform.parent.CompareTag("Wall"))
		)
		{
			wallHit = true;

			CreateWallExplosion(hit.point);

			CreateCrater(
				hit.point,
				hit.normal
			);

			Destroy(projectile);
		}
	}

	lastProjectilePosition = transform.position;
}

void CreateWallExplosion(Vector3 hitPoint)
{
	GameObject explosionPrefab =
		GameObject.Find(wall_explosion_prefab_name);

	if(explosionPrefab == null)
	{
		Debug.LogWarning(
			"Explosion prefab not found: " +
			wall_explosion_prefab_name
		);

		return;
	}

	GameObject explosion = Instantiate(
		explosionPrefab,
		hitPoint,
		Quaternion.identity
	);

	ParticleSystem ps =
		explosion.GetComponent<ParticleSystem>();

	if(ps != null)
	{
		ps.Play();
	}

	Destroy(explosion, 5.0f);
}

void CreateCrater(
	Vector3 hitPoint,
	Vector3 hitNormal
)
{
	GameObject crater =
		GameObject.CreatePrimitive(
			PrimitiveType.Quad
		);

	crater.transform.position =
		hitPoint + hitNormal * 0.03f;

	crater.transform.rotation =
		Quaternion.FromToRotation(
			Vector3.forward,
			hitNormal
		);

	crater.transform.localScale =
		new Vector3(4.0f, 4.0f, 4.0f);

	Destroy(
		crater.GetComponent<Collider>()
	);

	Material craterMaterial =
		new Material(
			Shader.Find(
				"Universal Render Pipeline/Unlit"
			)
		);

	Texture craterTexture =
		Resources.Load<Texture>(
			crater_texture_name
		);

	if(craterTexture != null)
	{
		craterMaterial.mainTexture =
			craterTexture;
	}
	else
	{
		Debug.LogWarning(
			"Crater texture not found in Resources: " +
			crater_texture_name
		);
	}

	crater.GetComponent<Renderer>().material =
		craterMaterial;
}
	public void DetectEnemyInRange(float distance_from_bullet)
{
	GameObject[] allEnemies =
		GameObject.FindGameObjectsWithTag("Enemy");

	foreach(GameObject go in allEnemies)
	{
		if(Vector3.Distance(
			go.transform.position,
			transform.position
		) <= distance_from_bullet)
		{
			PlayEnemyHitAnimation(
				go,
				hit_animation,
				hit_anim_parameter
			);

			return;
		}
	}
}
	
	
	void PlayEnemyHitAnimation(
		GameObject enemy_,
		string anim,
		string hit_param
	)
	{
		Animator animator = enemy_.transform.GetComponent<Animator>();

		animator.SetInteger(hit_param,1);
		animator.Play(anim);
		
		Destroy(projectile, 2.0f);
	}
	
	
	public void CreateFlash()
	{
		GameObject flash = GameObject.Find("WFX_MF 4P RIFLE2");
		
		if(turret!=null && generatedFlash==null)
		{
			generatedFlash = Instantiate(
				flash,
				muzzleObject2.transform.position,
				turret.transform.rotation
			);

			ParticleSystem ps =
				generatedFlash.transform.GetComponent<ParticleSystem>();

			ps.Play();
		}

		if(generatedFlash!=null)
		{
			Destroy(generatedFlash, 5.0f);
		}
	}


	void CreateProjectileTrail()
{
	projectileTrail.positionCount = 2;

	projectileTrail.useWorldSpace = false;

	projectileTrail.SetPosition(0, Vector3.zero);
	projectileTrail.SetPosition(1, new Vector3(0f, 50f, 0f));

	projectileTrail.startColor = Color.green;
	projectileTrail.endColor = Color.green;

	projectileTrail.startWidth = 2.415f;
	projectileTrail.endWidth = 2.405f;

	projectileTrail.material = new Material(
		Shader.Find("Universal Render Pipeline/Unlit")
	);
}
}