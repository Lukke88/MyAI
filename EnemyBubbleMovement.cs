using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateHelicopter : MonoBehaviour
{
	public GameObject player;
	public GameObject[] allEnemies;
	public float range = 50.0f, distance, maxDistance, minDistance, shooting_dist = 30.0f;
	public float detection_range = 20.0f;
	
	public void Start()
	{
		allEnemies = GameObject.FindGameObjectsOfType("Enemy");
	}
	
	public void Update()
    {
		minDistance = range;
		foreach(GameObject go in allEnemies)
		{
			if (Vector3.Distance(go.transform.position, player.transform.position) < range)
			{
				Vector3 moveDirection;

				// hráč je moc blízko → enemy chce ustoupit
					if(distance < minDistance)
					{
					moveDirection = (enemy.transform.position - player.transform.position).normalized;
					// pohyb směrem od hráče
					enemy.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
					Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
					enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, targetRotation, 0.05f);

					// zkontrolujeme, jestli za ním není zeď
					if(Physics.Raycast(enemy.transform.position, moveDirection, detection_range))
					{
					// místo dozadu se pohne do strany
					moveDirection = enemy.transform.right;
					}
			}
				
			}
			
			if (distance > maxDistance && distance > shooting_dist)
			{
				// směr k hráči
				moveDirection = (player.position - enemy.position).normalized;

				// pohyb směrem k hráči
				enemy.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

				// plynulé otočení k hráči
					Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
					enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, targetRotation, 0.05f);
				}				
    
		}
		
		float offset_z = 10.0f;

RaycastHit hit;

// ray z enemy směrem k hráči
Vector3 directionToPlayer = (player.position - enemy.position).normalized;

if (Physics.Raycast(enemy.position, directionToPlayer, out hit, detection_range))
{
    // pokud je mezi nimi zeď
    if (hit.collider.CompareTag("Wall"))
    {
        // pozice ZA zdí (na opačné straně od hráče)
        Vector3 hidePosition = hit.point - directionToPlayer * offset_z;

        // směr k místu za zdí
        moveDirection = (hidePosition - enemy.position).normalized;
		
		enemy.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
//Kód s otočením na hráče za zdí
if (Physics.Raycast(enemy.transform.position, moveDirection, out hit, detection_range))
{
    if (hit.collider.CompareTag("Wall"))
    {
        float offset_z = 10.0f;

        // směr k hráči
        Vector3 directionToPlayer = (player.position - enemy.position).normalized;

        // pozice ZA zdí
        Vector3 hidePosition = hit.point - directionToPlayer * offset_z;

        // pohyb k místu za zdí
        moveDirection = (hidePosition - enemy.position).normalized;

        enemy.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // ===== OTOČENÍ NA HRÁČE =====
        Vector3 lookDirection = player.position - enemy.position;
        lookDirection.y = 0f;

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, targetRotation, 0.05f);
			
			//Shoot
			Shoot();
        }
    }
}
// kontrola překážky před nepřítelem
if (Physics.Raycast(enemy.transform.position, moveDirection, out hit, detection_range))
{
    if (hit.collider.CompareTag("Wall"))
    {
		/*
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
        }*/
		
		
    }
}
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
	if(Input.GetKey(KeyCode.Space))//az po vystrelu
	{
		   HitEnemy(lastColliderName);
	}
}

void FireRifle()
{
    // jednoduchý Raycast střelby
    Ray ray = new Ray(FirePoint.position, FirePoint.forward);
    if (Physics.Raycast(ray, out RaycastHit hit, rifleRange))
    {
        Debug.DrawLine(FirePoint.position, hit.point, Color.red, 1f);
        // případně poškodit enemy
    }

    if (bulletTrail != null)
    {
        bulletTrail.SetPosition(0, FirePoint.position);
        bulletTrail.SetPosition(1, FirePoint.position + FirePoint.forward * rifleRange);
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