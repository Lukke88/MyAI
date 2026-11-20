using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FlashProjectileImpact : MonoBehaviour
{
    public float speed = 3200.0f;
    public Vector3 direction;
	public GameObject Splatter2D;
	public int random_number_splatter;
	public Texture2D blood_splatter1, blood_splatter2, blood_splatter3;
    public GameObject impactPrefab, player_target, nearest_enemy;
    public string impactPrefabName = "WFX_Nuke", player_hit_animation_name = "HeroHit", player_hit_param_name;
	public GameObject flashObject;
    private Vector3 previousPosition;
    private bool hasImpacted;
	public TMP_Text infoText; 
	public float minimal_hit_distance = 125.0f;
	
	public AudioClip aArghSound;
	public float aArghVolume = 1.0f;

    private void Start()
    {
		
        previousPosition = transform.position;
		
        if (direction.sqrMagnitude > 0.001f)
            direction.Normalize();
		flashObject = GameObject.Find(this.name);
		flashObject.transform.rotation = Quaternion.Euler(0, flashObject.transform.localRotation.y, 90.0f);
		
		if(nearest_enemy==null) //we find enemy which created this
		{
			GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
			
			foreach(GameObject go in allEnemies)
			{
				if(Vector3.Distance(go.transform.position, flashObject.transform.position)<=20.0f)
				{
					//parent of the flash
					nearest_enemy = go;
					this.name = "EnemyFlash1";
				}
			}
		}
		
		random_number_splatter = Random.Range(1,4);
		Renderer splatterRenderer =
		Splatter2D.GetComponent<Renderer>();

		if (splatterRenderer != null)
			{
		splatterRenderer.material.mainTexture =
        random_number_splatter == 1 ? blood_splatter1 :
        random_number_splatter == 2 ? blood_splatter2 :
        blood_splatter3;
		}
		
		Splatter2D.transform.GetComponent<Renderer>().enabled = false;
		
		}
		
		public void ShowUpSplatterPlayAarghClip(Vector3 hit_point, int number_splatter)
		{
			//at player hit will appear at his back Splatter2D
			Renderer splatterRenderer =
    Splatter2D.GetComponent<Renderer>();

		if (splatterRenderer != null)//blood behind player at impact
		{
		splatterRenderer.material.mainTexture =
        random_number_splatter == 1 ? blood_splatter1 :
        random_number_splatter == 2 ? blood_splatter2 :
        blood_splatter3;
		}
		
		Animator anim = player_target.transform.GetComponent<Animator>();
		anim.Play("HeroRibHit");//gets hit
		
		PlayAArghSound();//play scream
			
			//then plays aargh clip on impact
			//and player hit animation
		}

    private void Update()
    {
        if (hasImpacted)
            return;
		//rotates muzzle flash object to player in camera view
		player_target = Camera.main.GetComponent<CameraFollowsHero>().target.gameObject;
		if(player_target!=null && flashObject!=null)
		{
			flashObject.transform.position = Vector3.MoveTowards(
			flashObject.transform.position,
			player_target.transform.position,
			speed * Time.deltaTime);
			
			bool enemyShooter = nearest_enemy != null &&
                    nearest_enemy.CompareTag("Enemy");

			if (!enemyShooter &&
				Vector3.Distance(flashObject.transform.position, player_target.transform.position) <= minimal_hit_distance)
				{
				if (infoText != null)
					{
					infoText.text = "You have been hit by " +
					(nearest_enemy != null ? nearest_enemy.name : "Unknown enemy") +
					" and lost 5% of life.";
					}
					
					ShowUpSplatterPlayAarghClip(player_target.transform.position,random_number_splatter);

    DeductLifePointsFromPlayer(5.0f);

    Animator anim = player_target.transform.GetComponent<Animator>();

    if (anim != null)
        anim.Play(player_hit_animation_name);

    Destroy(flashObject, 1.0f);
}
		}
		Vector3 targetDirection =
    player_target.transform.position - transform.position;

if (targetDirection.sqrMagnitude > 0.001f)
{
    targetDirection.Normalize();
}

Vector3 nextPosition =
    transform.position + targetDirection * speed * Time.deltaTime;
		
        Vector3 travel = nextPosition - previousPosition;
        float distance = travel.magnitude;

        // Detect collisions along the entire travelled distance.
        if (distance > 0f)
        {
            RaycastHit hit;

            if (Physics.Raycast(
                previousPosition,
                travel.normalized,
                out hit,
                distance))
            {
                Transform hitTransform = hit.collider.transform;
                bool validImpact = false;

                while (hitTransform != null)
                {
                    if (hitTransform.CompareTag("Wall") ||
                        hitTransform.CompareTag("building") ||
                        hitTransform.CompareTag("PlayerVehicle") ||
                        hitTransform.CompareTag("Vehicle") ||
                        hitTransform.name.ToLower().Contains("fragment"))
                    {
                        validImpact = true;
                        break;
                    }
					else if(hitTransform.CompareTag("Enemy"))
					{
						if(hitTransform.gameObject.name.Contains("ArabWarrior"))
						{
						PlayEnemyAnimation(hitTransform.gameObject, "RibHit", "RibHitArab");//enemy gets hit, play animation
						PlayEnemyAnimation(hitTransform.gameObject, "ArabGunplay1", "ShootingArab");
						GameObject hit_enemy = hitTransform.gameObject;
						hit_enemy.transform.GetComponent<EnemyTriggeredByProjectile>().enabled = true;//enemy gets angry, starts attacking
						hit_enemy.transform.GetComponent<ArabWarriorShootsToTank>().enabled = true;//same case, just exchange tank for player figure
						}
					}

                    hitTransform = hitTransform.parent;
                }

                if (validImpact)
                {
                    CreateImpact(hit.point);
                    return;
                }
            }
        }

        transform.position = nextPosition;
        previousPosition = transform.position;
    }
	
	public void DeductLifePointsFromPlayer(float lifePoints)
	{
		// Creates damage, deducts the slider, later...
		hasImpacted = true;
		
		if(player_target!=null && Vector3.Distance(player_target.transform.position, flashObject.transform.position)<=20.0f)
		{
			string name_hit = "HeroRibHit";
			
			Animator anim = player_target.transform.GetComponent<Animator>();
			anim.Play(name_hit);
			
			PlayAArghSound();
			heroHealthBar hb = player_target.transform.GetComponent<heroHealthBar>();
			hb.TakeDamage(5.0f);//takes damage from enemy, update slider
		}
	}
	
	public void PlayAArghSound()
{
    if (aArghSound == null)
    {
        Debug.LogWarning("AArgh sound clip is missing!");
        return;
    }

    if (player_target != null)
    {
        AudioSource.PlayClipAtPoint(
            aArghSound,
            player_target.transform.position,
            aArghVolume
        );
    }
}
	
	public string previous_param_name;
	public void PlayEnemyAnimation(GameObject hit_enemy, string animation_name, string param_name)
	{
		Animator anim = hit_enemy.transform.GetComponent<Animator>();
		if(previous_param_name!="")
		anim.SetInteger(previous_param_name, 0);//set previous parameter to 0	
		anim.SetInteger(param_name, 1);//set up actual parameter
		anim.Play(animation_name);//play actual animation
		previous_param_name = param_name;
	}

    private void CreateImpact(Vector3 impactPosition)
    {
        hasImpacted = true;

        GameObject explosionPrefab = impactPrefab;

        if (explosionPrefab == null)
        {
            explosionPrefab =
                Resources.Load<GameObject>(impactPrefabName);
        }

        if (explosionPrefab == null)
        {
            explosionPrefab = GameObject.Find(impactPrefabName);
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(
                explosionPrefab,
                impactPosition,
                Quaternion.identity
            );

            ParticleSystem[] particles =
                explosion.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in particles)
                ps.Play();

            Destroy(explosion, 5f);
        }
        else
        {
            Debug.LogWarning(
                "Impact prefab not found: " + impactPrefabName);
        }

        Destroy(gameObject);
    }
}

