using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
public class HumveeMachinegunBehaviour : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
	public GameObject humvee, machinegun, turret, nearestPlayer, fireObject, fireMuzzlePrefab, generatedProjectile;
	public GameObject playerSeatObject, cursor, nearestPlayer2, generatedMuzzleObject, nearestEnemy;
	public string cursor_name, nearest_player_name = "IsofSoldier";
	public GameObject projectilePrefab01;
	public string fire_prefab_name = "WFX_MF 4P RIFLE2";
	public float nearestEnemyDistance, shoot_distance = 6000.0f, distance_bullet_from_machinegun;
	public float fireAndForgetDistance = 500.0f;
	public float projectile_speed = 300.0f;
	public float rotation_speed = 100.0f;
	public float currentDistance, activationDistance = 530.0f;
	public int projectiles_counter;
	public float time, angle, prepona_distance;
	public TMP_Text text, infoText;
	public GameObject MachinegunButton;
	public string player_shooting_anim = "PlayerShooting";
	public string player_shooting_anim_param = "PlayerShootingParam";
	public bool HasEnteredMachinegun, CanShootMachinegun, CanEmitNewBullet;
	public GameObject enterMachinegunButton;
public GameObject exitMachinegunButton;
public float enterDistance = 20.0f;
public float cursor_dist, cursor_angle;
	/*
	fireDelay - 
	0.10f → 10 ran za sekundu (600 RPM)
	0.08f → 12,5 ran/s (750 RPM)
	0.06f → 16,7 ran/s (1000 RPM)
	*/
	public float fireDelay = 0.1f;
	private float nextFireTime = 0.0f;
	
    // Start is called before the first frame update
    void Start()
    {
        humvee = GameObject.Find(this.name);
		turret = FindGameObjectInChildren(humvee, "machinegun_turret_cage");
		machinegun = FindGameObjectInChildren(turret, "machinegun");
		fireObject = FindGameObjectInChildren(machinegun, "FireObject");
		playerSeatObject = FindGameObjectInChildren(machinegun, "PlayerSeat");
		projectilePrefab01 = FindGameObjectInChildren(machinegun, "machinegun_projectile_01");//have 01 - 06 named projectiles
		if(nearestPlayer2==null)
			nearestPlayer2 = GameObject.Find(nearest_player_name);
    }
	
		public Color normalColor = Color.white;
		public Color hoverColor = Color.yellow;

		public void OnPointerEnter(PointerEventData eventData)
		{
			text.color = hoverColor;
			}

		public void OnPointerExit(PointerEventData eventData)
		{
			text.color = normalColor;
		}

    // Update is called once per frame
    void Update()
    {
		fireMuzzlePrefab = GameObject.Find("WFX_MF 4P RIFLE2");
		if(generatedProjectile!=null && machinegun!=null)
		{
			distance_bullet_from_machinegun = Vector3.Distance(generatedProjectile.transform.position, machinegun.transform.position);
			
			if(distance_bullet_from_machinegun>projectile_speed)
			{
				CanEmitNewBullet = true;
				generatedProjectile = null;
				generatedMuzzleObject = null;
			}
		}
		if(machinegun!=null)
		{
			cursor = GameObject.Find("Cursor_X (1)");
			
			cursor_dist = Vector3.Distance(machinegun.transform.position, cursor.transform.position);
			float z_dist = Mathf.Abs(machinegun.transform.position.z - cursor.transform.position.z);
			float x_dist = Mathf.Abs(machinegun.transform.position.x - cursor.transform.position.x);
			cursor_angle = Mathf.Asin(z_dist/cursor_dist) * Mathf.Rad2Deg;
			float pos_x = cursor.transform.position.x;
			float pos_z = cursor.transform.position.z;
			if(fireObject!=null)
			{
				if(pos_x>humvee.transform.position.x)
				{
					if(pos_z<humvee.transform.position.z)
					{
						fireObject.transform.rotation = Quaternion.Euler(0,-cursor_angle+180,0);
					}
					if(pos_z>humvee.transform.position.z)
					{
						fireObject.transform.rotation = Quaternion.Euler(0,cursor_angle+180,0);
					}
				}
				//fireObject.transform.rotation = Quaternion.Euler(0,cursor_angle,0);
				Debug.DrawRay(
				fireObject.transform.position,
				fireObject.transform.forward * shoot_distance,
				Color.green
				);
				
				machinegun.transform.rotation = Quaternion.Euler(0, fireObject.transform.rotation.y + 180, 0);
			}
		}
		if(nearestPlayer==null)
		{
			nearestPlayer = GameObject.Find("WomanSniper");
		}
		if(nearestEnemy==null)
			nearestEnemy = FindNearestEnemy();
		if(nearestPlayer2==null)
			nearestPlayer2 = GameObject.Find(nearest_player_name);
		else if(nearestPlayer2!=null && machinegun!=null)
		{
			currentDistance = Vector3.Distance(nearestPlayer2.transform.position,machinegun.transform.position);
			machinegun = FindGameObjectInChildren(turret, "machinegun");
						fireObject = FindGameObjectInChildren(machinegun, "FireObject");
						playerSeatObject = FindGameObjectInChildren(turret, "PlayerSeat");
						projectilePrefab01 = FindGameObjectInChildren(turret, "machinegun_projectile_01");//have 01 - 06 named projectiles
			if(CanShootMachinegun==true && Input.GetKey(KeyCode.Space))
			{				
				//MachinegunAimsToTarget();
				MachinegunShooting();
				if (projectilePrefab01 != null && generatedProjectile == null && fireObject != null)
				{
					ShootProjectile();
				}
			}
			if(currentDistance<activationDistance)
				MachinegunButton.SetActive(true);//show button
			else
				MachinegunButton.SetActive(false);//hide button
			
						
			
			
		}
		else if(nearestPlayer!=null)
		{
			
			
			if(Input.GetKey(KeyCode.Return) && HasEnteredMachinegun==false)//player entering machinegun
			{
				HasEnteredMachinegun = true;
				PressTheEnterButton();
			}
			else if(HasEnteredMachinegun && Input.GetKey(KeyCode.Space))//player shooting animation
			{
				PlayAnimation(player_shooting_anim, player_shooting_anim_param, 1);
				PlayMachinegunSoundEffect();
			}
			
			float distance =
			Vector3.Distance(nearestPlayer.transform.position,
                     playerSeatObject.transform.position);

				if (!HasEnteredMachinegun && distance <= enterDistance)
						enterMachinegunButton.SetActive(true);
					else
				enterMachinegunButton.SetActive(false);
		}
		if(turret==null)
				turret = FindGameObjectInChildren(humvee, "machinegun_turret_cage");
			if(machinegun==null)
				machinegun = FindGameObjectInChildren(turret, "machinegun");
			if(fireObject==null)
				fireObject = FindGameObjectInChildren(machinegun, "FireObject");
			if(playerSeatObject==null)
						playerSeatObject = FindGameObjectInChildren(machinegun, "PlayerSeat");
			if(projectilePrefab01==null)
				projectilePrefab01 = FindGameObjectInChildren(machinegun, "machinegun_projectile_01");//have 01 - 06 named projectiles
        if(fire_prefab_name!=null && fireMuzzlePrefab==null)
			fireMuzzlePrefab = GameObject.Find(fire_prefab_name);
		else if(fireObject!=null && fireMuzzlePrefab!=null)//fireObject is null object, Vector3, fireMuzzlePrefab is muzzle fire prefab with particle systems
		{
			if (Input.GetKey(KeyCode.Space) && Time.time >= nextFireTime)
			{
			MachinegunShooting();
			}
		}
		if(generatedProjectile!=null && generatedProjectile.transform.GetComponent<Projectile_Behaviour>().IsTriggered==true)
		{
			Vector3 target_position = generatedProjectile.transform.GetComponent<Projectile_Behaviour>().targetPoint;
			
			generatedProjectile.transform.position = Vector3.MoveTowards(generatedProjectile.transform.position, target_position, projectile_speed*Time.deltaTime);
		}
		//boolean assignment example: bool canShoot = ammo > 0 && !isReloading;
		if(playerSeatObject != null)
		{
		HasEnteredMachinegun =
        playerSeatObject.transform.childCount > 0;
		if(cursor==null)
		cursor = GameObject.Find(cursor_name);
		bool AreParametersCorrect = cursor != null && Input.GetKey(KeyCode.LeftControl) && machinegun!=null;
		if (AreParametersCorrect)
	{
    MachinegunAimsToTarget();
	}
		}
    }
	
	public void ShootProjectile()
	{
		generatedProjectile = Instantiate(
					projectilePrefab01,
					fireObject.transform.position,
					fireObject.transform.rotation);

					// Velikost projektilu
					generatedProjectile.transform.localScale = Vector3.one * 60.0f;

					Projectile_Behaviour pb = generatedProjectile.GetComponent<Projectile_Behaviour>();
						if(cursor_angle!=null)//we see if the green line helps me determine the direction
						{
						//	fireObject.transform.rotation = Quaternion.Euler(0,cursor_angle,0);//here are projectiles created so must be directed in correct way
						}
						if (pb != null)
						{
						pb.IsTriggered = true;

						shoot_distance = 6000.0f;

						// Bod 3000 jednotek před hlavní kulometu
						Vector3 direction =
						Quaternion.Euler(0f, 90f+180.0f, 0f) * (fireObject.transform.up);//this is shooting angle

pb.targetPoint = fireObject.transform.position +
                 direction * shoot_distance;
						 
						 CanEmitNewBullet = false;
						}
	}
	
	public GameObject FindNearestEnemy()
	{
		GameObject nearest_enemy = null;
		
		GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
		float nearest_distance = Mathf.Infinity;
		foreach(GameObject go in allEnemies)
		{
			if(Vector3.Distance(go.transform.position, humvee.transform.position)<nearest_distance)
			{
				nearest_distance = Vector3.Distance(go.transform.position, humvee.transform.position);
				nearestEnemy = go;
				nearest_enemy = go;
			}
		}
		
		return nearest_enemy;
	}
	
	public void MachinegunAimsToTarget()
	{
		Vector3 dir = cursor.transform.position - machinegun.transform.position;

    // ignorujeme výšku
    dir.y = 0f;

    float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

    Quaternion rot = Quaternion.Euler(0f, cursor_angle, 0f);

    machinegun.transform.rotation =
        Quaternion.Lerp(machinegun.transform.rotation,
                        rot,
                        rotation_speed * Time.deltaTime);
	}
	
	public void MachinegunShooting()
	{
		//
			Quaternion shooting_direction = Quaternion.Euler(0, cursor_angle,0);
			 generatedMuzzleObject =
				Instantiate(fireMuzzlePrefab,
                    fireObject.transform.position,
                    shooting_direction);

			ParticleSystem ps = generatedMuzzleObject.GetComponent<ParticleSystem>();
			if (ps != null) //play muzzle effect
				ps.Play();
				
			nextFireTime = Time.time + fireDelay; //time delay
			ShootProjectile(fireObject); //shoot projectile

			

			//Destroy(generatedMuzzleObject, 3.0f);//destroy after time
	}
	
	public void AddPlayerToMachinegun()
	{
		if(machinegun!=null && nearestPlayer2!=null)
		{
			nearestPlayer2.transform.position = machinegun.transform.position;
			infoText.text = "Player is attached to machinegun and can shoot";
			CanShootMachinegun = true;
		}
	}
	
	public AudioSource machinegunAudio;
		public AudioClip machinegunClip;
	public void PlayMachinegunSoundEffect()
	{
    if(machinegunAudio != null && machinegunClip != null)
    {
        machinegunAudio.PlayOneShot(machinegunClip);
    }
	}
	public void PressTheEnterButton()
{
    nearestPlayer.transform.SetParent(playerSeatObject.transform);
    nearestPlayer.transform.localPosition = Vector3.zero;
    nearestPlayer.transform.localRotation = Quaternion.identity;

    HasEnteredMachinegun = true;

   if (exitMachinegunButton != null)
    exitMachinegunButton.SetActive(true);

if (enterMachinegunButton != null)
    enterMachinegunButton.SetActive(false);
}

public Transform exitPoint;

public void ExitMachinegun()
{
    nearestPlayer.transform.SetParent(null);

    nearestPlayer.transform.position = exitPoint.position;
    nearestPlayer.transform.rotation = exitPoint.rotation;

    HasEnteredMachinegun = false;

    enterMachinegunButton.SetActive(true);
    exitMachinegunButton.SetActive(false);
}
	
	public void PlayAnimation(string animation_name, string paranm_name, int param_value)//shooting player animation behind "knipl"
	{
		Animator animator = nearestPlayer.transform.GetComponent<Animator>();
		animator.SetInteger(paranm_name, param_value);
		animator.Play(animation_name);
	}
	
	public static Transform FindChildRecursive(Transform parent, string childName)
{
    foreach (Transform child in parent)
    {
        if (child.name == childName)
            return child;

        Transform result = FindChildRecursive(child, childName);

        if (result != null)
            return result;
    }

    return null;
}

public static GameObject FindGameObjectInChildren(GameObject parent, string childName)
{
    if (parent == null)
        return null;

    Transform result = FindChildRecursive(parent.transform, childName);

    return result != null ? result.gameObject : null;
}
	
	public void ShootProjectile(GameObject fire_object)
	{
		GameObject projectilePrefab = projectilePrefab01;
		float shoot_distance = 800.0f;
		if(generatedProjectile==null)
		{
			generatedProjectile =	Instantiate(projectilePrefab, fire_object.transform.position, fire_object.transform.rotation);
			generatedProjectile.name = "mg_projectile_" + projectiles_counter.ToString();
			Vector3 projectileTarget =
			generatedProjectile.transform.position +
			(-fire_object.transform.up * (shoot_distance));//this one does nothing
			
			//if(generatedMuzzleObject!=null)//rotates fire flame to good direction
				//generatedMuzzleObject.transform.rotation = Vector3.RotateTowards(generatedMuzzleObject.transform.position, projectileTarget, 100.0f*Time.deltaTime);
			
			Projectile_Behaviour projectile =
			generatedProjectile.GetComponent<Projectile_Behaviour>();

			projectile.targetPoint = projectileTarget;
			projectile.IsTriggered = true;
			projectiles_counter++;
		}
		else if(generatedProjectile!=null)
		{
			float distance = Vector3.Distance(machinegun.transform.position, generatedProjectile.transform.position);
			
			if(distance>fireAndForgetDistance)
				generatedProjectile = null;
			
			if(distance>2000.0f)
				Destroy(generatedProjectile);
		}
	}
}
