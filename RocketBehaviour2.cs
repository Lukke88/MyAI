using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class RocketBehaviour2 : MonoBehaviour
{
    [Header("Ballistics")]
    public float launchAngle = 35f;   // základní úhel výstřelu
    public float gravity = 9.81f;

	public float dist_to_target;
    [Header("Flight")]
    public float lifeTime = 10f;

    [Header("FX")]
    public ParticleSystem ignitionFX;
    public ParticleSystem trailFX;
    public AudioSource audioSource;

    public GameObject craterPrefab;
	public float linearVelocity;

    public Transform target;
    public Rigidbody rb;
	
	public bool IsTriggered, IsMuzzleFireCreated, IsMissileInTarget;
	
	public AudioSource audio;
	public AudioClip missileStartClip;
	public AudioClip missileFlightClip;
	public AudioClip missileImpactClip;
	
	public GameObject missile,
	ImpactExplosion, MuzzleFirePrefab, muzzle_null_object, generated_muzzle_fire;
	public float minimal_impact_distance = 65.0f;
	private Vector3 customTarget;
	private bool useCustomTarget = false;
	public float roll;
	public GameObject[] houses;
	
	public bool hasExploded = false;

	public void Start()
	{	
	houses = GameObject.FindGameObjectsWithTag("GenerationHouse");
	}
	public void SetCustomTarget(Vector3 pos)
	{
    customTarget = pos;
    useCustomTarget = true;
	}
	public void Update()
	{
		minimal_impact_distance = 65.0f;
		if(missile==null)
				missile = GameObject.Find(this.name);
		if(target!=null && missile!=null)
		{
			dist_to_target = Vector3.Distance(missile.transform.position, target.position);
		}
		if(IsTriggered==true)
		{
			
			if(muzzle_null_object!=null && IsMuzzleFireCreated==false)
			{
				generated_muzzle_fire = Instantiate(MuzzleFirePrefab, muzzle_null_object.transform.position, muzzle_null_object.transform.rotation);
				generated_muzzle_fire.transform.parent = missile.transform;
				audio.PlayOneShot(missileStartClip);
				IsMuzzleFireCreated = true;
			}
			else if(IsMuzzleFireCreated==true && IsMissileInTarget==false)
			{
				if(generated_muzzle_fire!=null)
				{
					ParticleSystem ps = generated_muzzle_fire.GetComponent<ParticleSystem>();
					ps.Play();//plays engine thrust burning plasma during the flight
					
					if(missile!=null && target!=null)
					{
						float dist = Vector3.Distance(missile.transform.position, target.transform.position);
						if(dist<=minimal_impact_distance && !hasExploded)
						{
							
							if(IsTargetShielded())
							{
							float chance = Random.value;

							if(chance < 0.7f) // 70% že zeď pomůže
								{
									Explode();
									return;
								}
							}
							
							

							hasExploded = true;
							IsMissileInTarget = true;//dostavame se ke tretimu bodu
							
						}
						else
						{
							if(!audio.isPlaying)
							{
								audio.clip = missileFlightClip;
								audio.Play();
							}
						}
					}
				}
			}
			else if(IsMissileInTarget==true && hasExploded==true)
			{
				ParticleSystem ps = ImpactExplosion.transform.GetComponent<ParticleSystem>();
					ps.Play();//plays engine thrust burning plasma during the flight
					audio.clip = missileImpactClip;
					audio.Play();
					
					EnemyManager en = FindObjectOfType<EnemyManager>();
							if(en != null)
							{
								StartCoroutine(SpawnWithDelay(en));
							//	en.TrySpawnEnemiesFromHouses(1, 3);
							}
							Explode();
					
					/*GameObject GetRandomHouse()
					{
						int index = Random.Range(0, houses.Length);
						return houses[index];
					}*/
					Destroy(missile);
				
				
			}
		}
	}
	
	IEnumerator SpawnWithDelay(EnemyManager en)
	{
    yield return new WaitForSeconds(1.5f);
    en.TrySpawnEnemiesFromHouses(1, 3);
	}
	public GameObject nearestObstacle;
	bool IsTargetShielded()
{
    if (target == null) return false;

    Vector3 direction = (target.position - transform.position).normalized;
    float distance = Vector3.Distance(transform.position, target.position);

    RaycastHit hit;

    if (Physics.Raycast(transform.position, direction, out hit, distance))
    {
        if (hit.transform != target)
        {
            nearestObstacle = hit.transform.gameObject;

            float distToObstacle = Vector3.Distance(target.position, hit.point);

            if (distToObstacle <= 20f)
            {
                return true; // tank je krytý
            }
        }
    }

    return false;
}
    public void Init(Transform playerTarget)
    {
        target = playerTarget;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        if (ignitionFX != null) ignitionFX.Play();
        if (audioSource != null) audioSource.Play();

        LaunchBallistic();

        Destroy(gameObject, lifeTime);
    }

    void LaunchBallistic()
    {
        if (target == null) return;

		
		Vector3 targetPos = useCustomTarget ? customTarget : target.position;

		Vector3 toTarget = targetPos - transform.position;

        // horizontální vzdálenost
        Vector3 toTargetFlat = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = toTargetFlat.magnitude;

        float height = toTarget.y;

        // výpočet rychlosti pro balistiku
        float angleRad = launchAngle * Mathf.Deg2Rad;

        float speed = Mathf.Sqrt(
            (distance * gravity) /
            Mathf.Sin(2 * angleRad)
        );

        // směr vpřed + nahoru
        Vector3 dirFlat = toTargetFlat.normalized;

        Vector3 velocity =
            dirFlat * speed * Mathf.Cos(angleRad) +
            Vector3.up * speed * Mathf.Sin(angleRad);

        
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    
	
	void Explode()
	{
    if (target == null) return;

    float dist = Vector3.Distance(transform.position, target.position);
	
    TankHealth hp = target.GetComponent<TankHealth>();
	GameObject explosion_prefab = GameObject.Find("WFX_Nuke");
					GameObject generated_explosion = Instantiate(explosion_prefab, missile.transform.position, Quaternion.identity);
					ParticleSystem particles = generated_explosion.transform.GetComponent<ParticleSystem>();
					particles.Play();
					
	EnemyManager en = FindObjectOfType<EnemyManager>();
							if(en != null)
							{
								StartCoroutine(SpawnWithDelay(en));
							//	en.TrySpawnEnemiesFromHouses(1, 3);
							}			
    if (hp != null)
    {
        float damage = 0f;

        if (dist <= 30f) // přímý zásah
        {
            damage = 20f;
        }
        else if (dist>30 && dist <= 40f) // okraj
        {
            damage = 15f;
        }
        else if (dist > 45f) // vedle
        {
            damage = Random.Range(2f, 5f);
        }
		
		float damageMultiplier = 1f;

		if (roll <= 0.2f) damageMultiplier = 1.2f;   // crit
		else if (roll > 0.8f) damageMultiplier = 0.5f; // weak hit

        hp.ApplyDamage(damage);
    }
	
	if (trailFX != null) trailFX.Stop();

        if (craterPrefab != null)
        {
            Instantiate(craterPrefab, transform.position, Quaternion.identity);
        }

    Destroy(gameObject);
	}
}