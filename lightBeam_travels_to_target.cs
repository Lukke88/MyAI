using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightBeam_travels_to_target : MonoBehaviour
{
    public GameObject lightBeam;
    public GameObject target;
    public string target_name = "Merkava Mk_lowpoly";
	public bool IsTriggered;

    public float speed_beam = 120.0f;
    public float hitDistance = 1.0f;
	public GameObject[] allWalls;

    public GameObject hitEffectPrefab; // ParticleSystem prefab

    void Start()
    {
        lightBeam = gameObject; // jednodušší než Find(this.name)

        if (!string.IsNullOrEmpty(target_name))
            target = GameObject.Find(target_name);	
		allWalls = GameObject.FindGameObjectsWithTag("Wall");
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }
	public RaycastHit hit;
    void Update()
    {
        if (lightBeam == null || target == null) return;
		
        // ===== SMĚR K CÍLI =====
        Vector3 direction = (target.transform.position - lightBeam.transform.position);
        direction.y = 0f; // jen Y rotace
		

		Vector3 moveDirection = (target.transform.position - transform.position).normalized;
		DetectCollision();

		float angle_to_player = GetAngleToPlayer();
        if (direction != Vector3.zero)
        {
			angle_to_player = GetAngleToPlayer();

			Quaternion rot = Quaternion.Euler(-90f, angle_to_player + 90f, 90f);
			lightBeam.transform.rotation = rot;
            
        }
		if(this.name.Contains("(Clone)"))
			IsTriggered = true;
		else
			IsTriggered = false;//we not sending original
        // ===== POHYB =====
		if(IsTriggered==true)
		{
        lightBeam.transform.position = Vector3.MoveTowards(
            lightBeam.transform.position,
            target.transform.position,
            speed_beam * Time.deltaTime
        );

        // ===== HIT DETEKCE =====
        if (Vector3.Distance(lightBeam.transform.position, target.transform.position) <= hitDistance)
        {
            OnHit();
        }
		}	
		
		
		
    }
	
	public float GetAngleToPlayer()
{
    Vector3 direction = target.transform.position - transform.position;

    // ignorujeme výšku (jen Y rotace)
    direction.y = 0f;

    if (direction == Vector3.zero)
        return 0f;

    Quaternion lookRot = Quaternion.LookRotation(direction);

    return lookRot.eulerAngles.y;
}
	public GameObject nearestWall;

public void DetectCollision()
{
    RaycastHit hit;

    Vector3 rayDirection =
        (target.transform.position - lightBeam.transform.position).normalized;

    if (Physics.Raycast(lightBeam.transform.position,
                        rayDirection,
                        out hit,
                        120.0f))
    {
        nearestWall = hit.collider.gameObject;

        
    }
}
	
	
	void OnTriggerEnter(Collider other)
{
    WomanSniperBehaviour sniper =
        other.GetComponent<WomanSniperBehaviour>();

    if(sniper != null)
    {
        if(Random.value > 0.5f)
            sniper.animator.Play(sniper.RibHit);
        else
            sniper.animator.Play(sniper.RifleHit);
    }

if(other.CompareTag("Wall") && other.bounds.size.y>50.0f)
    {
        OnWallHit(transform.position);
    }

    if(other.gameObject == target)
    {
        OnHit();
    }
}

    void OnHit()
    {
        // spawn particle efektu
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                hitEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            Destroy(effect, 2f); // smaže efekt po čase
        }

        Destroy(gameObject); // zničí beam
    }
	
	public AudioClip wallHitSound;
	public AudioSource audioSource;

	void OnWallHit(Vector3 hitPoint)
	{
	//Vector3 reflectDir = Vector3.Reflect(rayDirection, hit.normal);
	Vector3 hitNormal = hit.normal;
    if (hitEffectPrefab != null)
    {
        Quaternion rot = Quaternion.LookRotation(hitNormal);

        GameObject effect = Instantiate(
            hitEffectPrefab,
            hitPoint,
            rot
        );
		//effect.transform.forward = reflectDir;
        Destroy(effect, 2f);
    }

    if(audioSource != null && wallHitSound != null)
    {
        audioSource.PlayOneShot(wallHitSound);
    }

    Destroy(gameObject);
}
}
