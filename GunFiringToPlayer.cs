using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunFiringToPlayer : MonoBehaviour
{
	public GameObject gun, target, projectile, generatedProjectile, gun_muzzle, generatedMuzzleFire, muzzle_fire_prefab;

	public float starting_height = 170.0f;
	public float fire_interval = 0.5f;
	public float fire_timer;

    void Start()
    {
        gun = GameObject.Find(this.name);
		target = GameObject.Find("btr80-cma");
		projectile = GameObject.Find("projectile");

		fire_timer = fire_interval;
    }

    void Update()
    {
		float projectile_speed = 500.0f;

		fire_timer -= Time.deltaTime;

		if(gun != null)
		{
			gun_muzzle = gun.transform.GetChild(0).gameObject;
		}
		else
		{
			gun = GameObject.Find(this.name);
			target = GameObject.Find("btr80-cma");
			projectile = GameObject.Find("projectile");
			
		}
		muzzle_fire_prefab =
					GameObject.Find("WFX_MF 4P RIFLE2");

		if(generatedProjectile == null /*&& fire_timer <= 0.0f*/)
		{
			Debug.DrawLine(
				gun.transform.position,
				target.transform.position,
				Color.red
			);
			//if(fire_timer<=0.0f)
			{
			generatedProjectile = Instantiate(
				projectile,
				gun_muzzle.transform.position,
				gun_muzzle.transform.rotation
			);
			}

			if(gun_muzzle != null)
			{
				 

				if(generatedMuzzleFire == null)
				{
					Vector3 direction =
						target.transform.position -
						gun_muzzle.transform.position;

					Quaternion rotation =
						Quaternion.LookRotation(direction);

					rotation *= Quaternion.Euler(0, 90.0f, 0);

					generatedMuzzleFire = Instantiate(
						muzzle_fire_prefab,
						gun_muzzle.transform.position,
						rotation
					);
					
					generatedMuzzleFire.transform.parent =
						gun_muzzle.transform;

					if(generatedMuzzleFire.transform.localScale != Vector3.one * 2f)
					{
						generatedMuzzleFire.transform.localScale =
							Vector3.one * 2f;
					}

					ParticleSystem ps =
						generatedMuzzleFire
						.transform
						.GetComponent<ParticleSystem>();

					

					ps.Play();

					Destroy(generatedMuzzleFire, 3.0f);
				}
			}

			// RESET TIMERU PO VÝSTŘELU
			fire_timer = fire_interval;
		}
		else if(generatedProjectile != null)
		{
			Vector3 target_position = new Vector3(
				target.transform.position.x,
				starting_height,
				target.transform.position.z
			);

			generatedProjectile.transform.position =
				Vector3.MoveTowards(
					generatedProjectile.transform.position,
					target_position,
					projectile_speed * Time.deltaTime
				);

			if(Vector3.Distance(
				generatedProjectile.transform.position,
				target.transform.position
			) <= 10.0f)
			{
				Destroy(generatedProjectile);
			}
		}
    }
}