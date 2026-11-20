using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArabEnemyRunsToFinalPosition : MonoBehaviour
{
	
    public GameObject enemy, playerTarget;
	public string player_tag = "Player";//alternatively "PlayerVehicle" etc.
	public GameObject[] allEnemies;
	public bool IsEnemyBehindTank;
	public string selected_hideout_object = "";
    public GameObject target_object, nearest_player, selectedBarricade;
	public float nearestTankDistance;
	public GameObject detected_obstruction;
	public GameObject slot;
	public float shooting_distance = 1500.0f;
    public float enemy_speed = 150.0f;
    public float rotationSpeed = 10.0f;
	public float minimal_distance_to_detect_end_wall = 130.0f;//enemy stops, detects target object
    private Animator anim;
    private int currentAnimation = 0;
	public bool stop_movement, IsGoingToAttackWaypoint, IsInAttackWaypoint;
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
	
	public float tankEscapeDistance = 50.0f;
	public string tankTag = "PlayerVehicle";
	
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
	
	public GameObject firePoint, gunslot;
	public string slot_name = "gunslot";
	public string fireppint_name = "gunFirePoint";
	
	public AudioClip shot_audio;
public AudioSource audio_source;
public float shot_volume = 1.0f;

private bool isPeeking = false;
private float nextPeekTime;
private Coroutine peekCoroutine;

public float minPeekDelay = 3.0f;
public float maxPeekDelay = 8.0f;

public string gun_name = "Gun";//prefab
public string gun_muzzle_name = "WFX_MF 4P RIFLE1";
public string canvas_name = "CanvasArabWarrior";

public GameObject inter_house1, inter_house2;
public bool passed_inter_house1, passed_inter_house2;

	int number_attack_positions = 5, control_count_waypoints;//6 positions, control_count_waypoints checks if are all created
	public Vector3[] attackWaypoints;
	public Vector3 selectedAttackPosition;
	
	public Vector3 startPoint;
	public Vector3[] intermediateHouse;
	public float intermediate_distance;
	public float intermediate_angle = 15.0f;
	public string target_object_name = "Side_wall_left";
    // Start is called before the first frame update
    void Start()
    {
		attackWaypoints = new Vector3[number_attack_positions];
        enemy = GameObject.Find(this.name);
		target_object = GameObject.Find(target_object_name);
		startPoint = enemy.transform.position;	
		 nextPeekTime =
        Time.time +
        Random.Range(minPeekDelay, maxPeekDelay);
		
		intermediate_distance = Vector3.Distance(enemy.transform.position, target_object.transform.position)/3;
		
		FindIntermediateHouses();
    }
	public bool IsAtNearestWall;
	public void LateUpdate()
	{
		float enemy_speed = 30.0f;
		allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
		selectedEnemy = null;//put all to default, to check if new one is on the same place like old one - this is just for control test in CheckDeadEnemies()
		if(target_object==null)
			target_object = GameObject.Find(selected_hideout_object);//this activates movement to certain object, target_object can be wall nearest to player_object
		//Jak aktivovat funkci při zaslechnutí hráče - hearing function to more advanced AI enemy
		EnemyDetectsBestAttackAngle tacticalAI =
			GetComponent<EnemyDetectsBestAttackAngle>();

			if (tacticalAI != null)//hears player behind wall, searches best attack angle(best hideout with direct raycast to player) and moves to new spot
				{
					tacticalAI.HearPlayerSound(playerTarget);
				}
				
				/*
				Každá barikáda, kterou chceš použít jako střeleckou pozici, musí mít následující strukturu:

				Barricade_01 (Tag: Barricade)

				Barricade_Model

				FirePoint (Required)

				FirePoint musí být:
				- Umístěný tam, kde může voják skutečně střílet.
				- Natočený směrem k potenciálnímu cíli.
				- Umístěný mimo geometrii barikády.
				*/

		if(Vector3.Distance(enemy.transform.position, nearest_player.transform.position)<shooting_distance || IsAtNearestWall==true)//it can be tank too
		{
			ShootToPlayerObject();
		}
		else
		{
			FindNearestWall();
			
			if(Vector3.Distance(enemy.transform.position, target_object.transform.position)<minimal_distance_to_detect_end_wall)
				IsAtNearestWall = true;//this activates second script for shooting 
		}
		if(target_object!=null && enemy!=null)
		{
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target_object.transform.position, enemy_speed*Time.deltaTime);
			
			Animator anim = enemy.transform.GetComponent<Animator>();
			if(Vector3.Distance(target_object.transform.position, enemy.transform.position)>10.0f)//enemy runs to final position, to the place of last dead enemy via CountEnemiesAndScore -> SubstituteDeadEnemy(go, go.transform.position);
			{
			anim.SetInteger("ShootingArab",0);
			anim.SetInteger("RunArab",1);
			anim.Play("ArabDrunkRun");
			}
			else
			{
				if(enemy.name.Contains("ArabWarrior"))
				{
					CountEnemiesAndScore c = enemy.transform.GetComponent<CountEnemiesAndScore>();
					//find nearest dead enemy
					CheckDeadEnemies(20.0f);
					if(selectedEnemy!=null)//if new generated enemy reached point of the old one, substitute the old one
						c.SubstituteDeadEnemy(selectedEnemy, selectedEnemy.transform.position);
				}
					anim.SetInteger("ShootingArab",1); //otherwise shoots to player
					anim.SetInteger("RunArab",0);
					anim.Play("ArabGunPlay");
			}
		}
	}
	public GameObject selectedEnemy;
	public void CheckDeadEnemies(float minimal_distance)
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject go in allEnemies)
        {
			if(Vector3.Distance(enemy.transform.position, go.transform.position)<=minimal_distance)
			{
            EnemyHealth eh = go.transform.GetComponent<EnemyHealth>();
            ArabWarriorAttacks ac = go.transform.GetComponent<ArabWarriorAttacks>();

            if (eh != null && ac != null)
            {
                if (eh.isDead == true)//check the status of enemy
                {
                    selectedEnemy = go;
                }
            }
			}
        }
    }
	
	public void ShootToPlayerObject()
	{
		if(enemy!=null && enemy.name.Contains("Challenger"))//prefabricated shooting function
		{
			ArabWarriorShootsToTank ab = enemy.transform.GetComponent<ArabWarriorShootsToTank>();
			ab.enabled = true;
			ab.nearestTank = nearest_player;
			ab.gunPrefabName = "AKM";//this adds gun prefab to enemy
			ab.destination_point = nearest_player.transform.position;//shoot straight to tank
			/*//already at line 65 of that script
			FindGunslotInGameObject(
					"gunslot",
					arabWarrior,
					"AKM",
					"firePoint"
				);
				ab.StootProjectile();
				*/
		}
		else
		{
			ArabWarriorAttacks ab = enemy.transform.GetComponent<ArabWarriorAttacks>();
			ab.enabled = true;
			ab.nearestPlayer = nearest_player;
		}
	}
	
	public void FindNearestWall()
	{
		GameObject[] allWalls = GameObject.FindGameObjectsWithTag("Barricade");
		float maximal_distance = Mathf.Infinity;
		foreach(GameObject go in allWalls)
		{
			if(Vector3.Distance(go.transform.position, nearest_player.transform.position)<maximal_distance)
			{
				maximal_distance = Vector3.Distance(go.transform.position, nearest_player.transform.position);
				
				selectedBarricade = go;
				
			}
		}
	}
    // Update is called once per frame
    public void Update()
    {
		//float enemy_speed = 30.0f;
		 // TANK TOO CLOSE -> RUN AWAY
    if (enemy != null && RunAwayFromTank())//priority
        return;

    if(target_object==null)
        target_object = GameObject.Find(selected_hideout_object);
		
		if(selectedBarricade!=null)//for the case that player object(tank) goes to some object(barricade) and enemy needs to fast change a plan
			target_object = selectedBarricade;
		else if(selectedBarricade==null)
			target_object = GameObject.Find(selected_hideout_object);//otherwise find default object
        
		Vector3 direction =
            target_object.transform.position -
            enemy.transform.position;

        direction.y = 0f;
		if(detected_obstruction!=null)//detected building in way ->stop what he is doing
		{
			stop_movement = true;
			PlayAnimation(idle_animation,idle_param,1);
		}
		if(inter_house1!=null && passed_inter_house1==false) //redirect to inter_target 1
		{
			target_object = inter_house1;
			passed_inter_house2 = VerifyIfPassedAroundObject(inter_house1);
		}
		else if(inter_house2!=null && passed_inter_house1==true && passed_inter_house2==false)//redirect to inter target 2
		{
			target_object = inter_house2;
			passed_inter_house2 = VerifyIfPassedAroundObject(inter_house2);
		}
		else if((inter_house1==null && inter_house2==null)||(passed_inter_house1==true && passed_inter_house2==true))
		{
			target_object = GameObject.Find(target_object_name);//go back to default target
		}
        if(target_object!=null && enemy!=null)
		{
			if(stop_movement==false && IsGoingToAttackWaypoint==false)
			enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target_object.transform.position, enemy_speed*Time.deltaTime);
			
			 RotateTowardsDirection(direction);
			 
			if(Vector3.Distance(enemy.transform.position,target_object.transform.position)>5.0f)
						PlayAnimation(run_animation,run_param,1);
			else
			{
						PlayAnimation(idle_animation,idle_param,1);
			}
			CanvasLookAtCamera();
			if(detected_obstruction==null)
			RaycastObjectInFront();
			
			if(stop_movement==true)
				AvoidObstacle();
			
			if(Vector3.Distance(target_object.transform.position, enemy.transform.position)<=minimal_distance_to_detect_end_wall)//enemy stops, detect object in front and move to random waypoint
			{
				FindWaypointForAttack();
			}
		}
    }
	public GameObject nearestTank;

	public bool isRunningFromTank = false;
	public Vector3 tankEscapeDirection;

public bool RunAwayFromTank()
{
    GameObject[] tanks =
        GameObject.FindGameObjectsWithTag(tankTag);

    if (tanks.Length == 0)
        return false;

    // Najdi nejbližší tank
    if (nearestTank == null)
    {
        float closestDistance = Mathf.Infinity;

        foreach (GameObject tank in tanks)
        {
            float distance =
                Vector3.Distance(
                    enemy.transform.position,
                    tank.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTank = tank;
            }
        }
    }

    if (nearestTank == null)
        return false;
	tankEscapeDistance = 1150.0f;
    // Vzdálenost od tanku se musí kontrolovat pokaždé
    nearestTankDistance =
        Vector3.Distance(
            enemy.transform.position,
            nearestTank.transform.position);

    // Pokud je tank méně než 50.0f od enemy,
    // enemy radikálně změní směr o 90 stupňů,
    // aby utekl z dráhy tanku
    if (nearestTankDistance < tankEscapeDistance)
    {
        if (isRunningFromTank == false)
        {
            Vector3 directionToTank2 =
                nearestTank.transform.position -
                enemy.transform.position;

            directionToTank2.y = 0f;

            if (directionToTank2 == Vector3.zero)
                return false;

            directionToTank2.Normalize();

            // Vytvoříme směr kolmo k tanku - 90 stupňů
            Vector3 escapeDirectionA =
                new Vector3(
                    -directionToTank2.z,
                    0f,
                    directionToTank2.x);

            Vector3 escapeDirectionB =
                new Vector3(
                    directionToTank2.z,
                    0f,
                    -directionToTank2.x);

            // Vybereme jednu ze dvou stran
            // aby enemy nešel přímo proti tanku
            if (Random.Range(0, 2) == 0)
                tankEscapeDirection = escapeDirectionA;
            else
                tankEscapeDirection = escapeDirectionB;

            tankEscapeDirection.Normalize();

            isRunningFromTank = true;
        }

        // Enemy utíká kolmo z dráhy tanku
		if(Vector3.Distance(nearestTank.transform.position, enemy.transform.position)<tankEscapeDistance)
		{
			isRunningFromTank = true;
        enemy.transform.position =
            Vector3.MoveTowards(
                enemy.transform.position,
                enemy.transform.position +
                tankEscapeDirection * 100.0f,
                enemy_speed * Time.deltaTime);

        RotateTowardsDirection(tankEscapeDirection);

        PlayAnimation(
            run_animation,
            run_param,
            1);

        Debug.DrawRay(
            enemy.transform.position,
            tankEscapeDirection * 50.0f,
            Color.magenta);
		}
		else
		{
			// Tank se začal vzdalovat
				if (isRunningFromTank == true)
				{
					isRunningFromTank = false;
				}
		}

        return true;
    }

    

    // Pokud je target_object blíže než tank,
    // enemy se vrátí automaticky ke svému target_object
    if (target_object != null && isRunningFromTank==false)
    {
        float distanceToTarget =
            Vector3.Distance(
                enemy.transform.position,
                target_object.transform.position);

        if (distanceToTarget < nearestTankDistance)
        {
            // Pokud je méně než 20.0f od targetu,
            // enemy se zastaví
            if (distanceToTarget <= 20.0f)
            {
                PlayAnimation(
                    idle_animation,
                    idle_param,
                    1);

                return true;
            }

            // Enemy jde zpět k target_object
            Vector3 directionToTarget =
                target_object.transform.position -
                enemy.transform.position;

            directionToTarget.y = 0f;

            RotateTowardsDirection(directionToTarget);

            enemy.transform.position =
                Vector3.MoveTowards(
                    enemy.transform.position,
                    target_object.transform.position,
                    enemy_speed * Time.deltaTime);

            PlayAnimation(
                run_animation,
                run_param,
                1);

            return true;
        }
    }

    // Pokud target není blíže než tank,
    // enemy se začne přibližovat zpět k tanku
	if(isRunningFromTank==false)
	{
    Vector3 directionToTank =
        nearestTank.transform.position -
        enemy.transform.position;

    directionToTank.y = 0f;

    if (directionToTank == Vector3.zero)
        return false;

    directionToTank.Normalize();

    enemy.transform.position =
        Vector3.MoveTowards(
            enemy.transform.position,
            enemy.transform.position +
            directionToTank * 100.0f,
            enemy_speed * Time.deltaTime);

    RotateTowardsDirection(directionToTank);

    PlayAnimation(
        run_animation,
        run_param,
        1);

    Debug.DrawRay(
        enemy.transform.position,
        directionToTank * 50.0f,
        Color.cyan);
	}

    return true;
}
	
	public bool VerifyIfPassedAroundObject(GameObject researched_object)
	{
		 bool passedAroundObject = false;
		GameObject default_target_object = GameObject.Find(target_object_name);
		float distance1 =
		Vector3.Distance(
        enemy.transform.position,
        researched_object.transform.position);

		float distance2 =
		Vector3.Distance(
        enemy.transform.position,
        default_target_object.transform.position);
		if(distance2<distance1)
			passedAroundObject= true;
		else
			passedAroundObject = false;
		
		return passedAroundObject;
	}
	
	public void FindIntermediateHouses()//looks for houses on the way from start to target destination
	{
		GameObject gen_house = FindNearestGenHouse();
		
		if(gen_house!=null)
		{
			float size_wall_x = target_object.transform.GetComponent<Collider>().bounds.size.x;
			float tretinka = Vector3.Distance(gen_house.transform.position, target_object.transform.position)/3;
			float angle_between_objects = Mathf.Asin(Mathf.Abs(gen_house.transform.position.z - target_object.transform.position.z)/Vector3.Distance(gen_house.transform.position, target_object.transform.position))*180/Mathf.PI;
			float angleRad =
			angle_between_objects * Mathf.PI / 180f;

		Vector3 direction =
    (target_object.transform.position -
     gen_house.transform.position).normalized;

			Vector3 point1 =
			gen_house.transform.position +
			direction * tretinka;

			Vector3 point2 =
			gen_house.transform.position +
			direction * (2f * tretinka);
			
			inter_house1 = FindRandomIntermediateHouse(point1, size_wall_x);
			inter_house2 = FindRandomIntermediateHouse(point2, size_wall_x);
		}
	}
	
	public GameObject FindRandomIntermediateHouse(Vector3 start_point_position, float horizontal_range_x)
	{
		GameObject selectedHouse = null;
		GameObject[] allHouses = GameObject.FindGameObjectsWithTag("building");
		
		foreach(GameObject go in allHouses)
		{
			if(Mathf.Abs(go.transform.position.x - start_point_position.x)<=5.0f && Vector3.Distance(go.transform.position,start_point_position)<=horizontal_range_x)
				selectedHouse = go;
		}
		
		return selectedHouse;
	}
	public GameObject FindNearestGenHouse()
{
    GameObject[] houses =
        GameObject.FindGameObjectsWithTag("GenerationHouse");

    GameObject nearestHouse = null;

    float closestDistance = Mathf.Infinity;

    foreach (GameObject house in houses)
    {
        float distance =
            Vector3.Distance(
                enemy.transform.position,
                house.transform.position);

        if (distance < closestDistance)
        {
            closestDistance = distance;
            nearestHouse = house;
        }
    }

    return nearestHouse;
}
	
	public void UpdatePlayerTag()
{
    GameObject[] players =
        GameObject.FindGameObjectsWithTag("Player");

    GameObject closestPlayer = null;
    float closestDistance = Mathf.Infinity;

    foreach (GameObject go in players)
    {
        float distance =
            Vector3.Distance(
                enemy.transform.position,
                go.transform.position);

        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestPlayer = go;
        }
    }

    if (closestPlayer == null)
        return;

    Renderer playerRenderer =
        closestPlayer.GetComponentInChildren<Renderer>();

    if (playerRenderer != null &&
        playerRenderer.enabled == false)
    {
        player_tag = "PlayerVehicle";
    }
    else
    {
        player_tag = "Player";
    }
}
	public int selectedRandPosition;
	public void FindWaypointForAttack()
	{
		//number_attack_positions = 5;
		IsGoingToAttackWaypoint = true;
		
			float movement_speed = enemy_speed;
			float offset_x = 40.0f;
			float offset_z = 40.0f;
			float size_x = detected_obstruction.transform.GetComponent<Collider>().bounds.size.x;
			float size_z = detected_obstruction.transform.GetComponent<Collider>().bounds.size.z;
			float gap_z = size_z/number_attack_positions; //z size > x size
			if(control_count_waypoints<=number_attack_positions )
			{
			if(Vector3.Distance(enemy.transform.position,target_object.transform.position)<=minimal_distance_to_detect_end_wall)
			{
				for(int i=1; i<number_attack_positions; i++)
				{
					float p_z = target_object.transform.position.z - size_z/2 + i*gap_z;
					float p_y = enemy.transform.position.y;
					float p_x = target_object.transform.position.x + offset_x;
					
					attackWaypoints[i] = new Vector3(p_x, p_y, p_z);
					control_count_waypoints++;
				}
			}
			}
			else if(control_count_waypoints>=number_attack_positions && IsGoingToAttackWaypoint==false)
			{
				if(selectedRandPosition==0)
					selectedRandPosition = Random.Range(1,number_attack_positions);
				if(selectedRandPosition>0)
				{
					selectedAttackPosition = attackWaypoints[selectedRandPosition];
				}
				if(selectedAttackPosition!=Vector3.zero && IsInAttackWaypoint==false)
				{
					//enemy goes there
					enemy.transform.position = Vector3.MoveTowards(enemy.transform.position,selectedAttackPosition,enemy_speed*Time.deltaTime);
					
					Debug.DrawLine(enemy.transform.position, selectedAttackPosition, Color.yellow);
					
					if(Vector3.Distance(enemy.transform.position, selectedAttackPosition)<=5.0f)
						IsInAttackWaypoint=true;
				}
			}
			else if(IsInAttackWaypoint == true)
			{
				FindNearestPlayer();
				FireToPlayer();
			}
	}
	public GameObject last_player_object;
	public void FindNearestPlayer()
{
    GameObject[] allPlayers;

    GameObject player =
        GameObject.FindGameObjectWithTag("Player");

    if (player != null)
    {
        Renderer playerRenderer =
            player.GetComponent<Renderer>();

        if (playerRenderer != null &&
            playerRenderer.enabled == false)
        {
            player_tag = "PlayerVehicle";
        }
        else
        {
            player_tag = "Player";
        }
    }

    allPlayers =
        GameObject.FindGameObjectsWithTag(player_tag);

    float closestDistance = Mathf.Infinity;
    nearest_player = null;

    foreach (GameObject go in allPlayers)
    {
        float distance =
            Vector3.Distance(
                enemy.transform.position,
                go.transform.position);

        if (distance < closestDistance)
        {
            closestDistance = distance;
            nearest_player = go;
        }
    }
}
	
	public void FireToPlayer()
{
    if (nearest_player != null)
    {
        Vector3 direction =
            nearest_player.transform.position -
            enemy.transform.position;

        direction.y = 0f;

        RotateTowardsDirection(direction);

        // enemy is currently hiding/peeking
        if (isPeeking)
            return;

        // time to hide
        if (Time.time >= nextPeekTime)
        {
            if (peekCoroutine == null)
            {
                peekCoroutine =
                    StartCoroutine(PeekAndHide());
            }

            return;
        }

        // normal shooting
        PlayAnimation(
            shoot_animation,
            shoot_param,
            1);

        if (firePoint == null || gunslot == null)
        {
            FindFirePointAndGunSlot();
        }
        else
        {
            CreateFireAndSoundEffect(
                firePoint.transform.position);

            FireInTheHole(
                firePoint.transform.position,
                firePoint);
        }
    }
}

private IEnumerator PeekAndHide()
{
    isPeeking = true;

    PlayAnimation(
        shoot_peek_animation,
        hide_param,
        1);

    Animator animator =
        enemy.GetComponent<Animator>();

    yield return null;

    AnimatorStateInfo state =
        animator.GetCurrentAnimatorStateInfo(0);

    float animationLength =
        state.length;

    yield return new WaitForSeconds(
        animationLength);

    isPeeking = false;

    nextPeekTime =
        Time.time +
        Random.Range(
            minPeekDelay,
            maxPeekDelay);

    peekCoroutine = null;
}
public GameObject generated_beam, generated_muzzle, generated_gun;
public void FireInTheHole(
    Vector3 beamGenPosition,
    GameObject NullFirePoint)
{
    GameObject firingBeam_prefab =
        GameObject.Find("lightBeam");

    if (generated_beam == null)
    {
        generated_beam =
            Instantiate(
                firingBeam_prefab,
                beamGenPosition,
                NullFirePoint.transform.rotation);
    }

    generated_beam.transform.position =
        beamGenPosition;

    generated_beam.transform.rotation =
        NullFirePoint.transform.rotation;

    lightBeam_travels_to_target beam =
        generated_beam.GetComponent<
            lightBeam_travels_to_target>();

    beam.target = nearest_player;
    beam.IsTriggered = true;
}

public void CreateFireAndSoundEffect(Vector3 muzzle_position)
{
	GameObject muzzlePrefab = GameObject.Find(gun_muzzle_name);
	if(slot!=null)
	AddGunToHands(gun_name);//gives gun to hands of enemy soldier
	if(generated_muzzle==null)
	{
		generated_muzzle = Instantiate(muzzlePrefab, muzzle_position, firePoint.transform.rotation);
	}
	else if(generated_muzzle!=null)
	{
		ParticleSystem ps = generated_muzzle.transform.GetComponent<ParticleSystem>();
		ps.Play();
		Destroy(generated_muzzle, 3.0f);
	}
	PlayShotAudioEffect();
	
}
public void AddGunToHands(string prefabName)
{
	GameObject gunPrefab = GameObject.Find(prefabName);
	
	if(generated_gun==null && gunPrefab!=null && slot!=null)
	{
		generated_gun = Instantiate(gunPrefab, slot.transform.position, slot.transform.rotation);
		generated_gun.transform.parent = slot.transform;
	}
}
public void PlayShotAudioEffect()
{
    if (shot_audio == null)
        return;

    if (audio_source == null)
    {
        audio_source = enemy.GetComponent<AudioSource>();

        if (audio_source == null)
        {
            audio_source =
                enemy.AddComponent<AudioSource>();
        }
    }

    audio_source.PlayOneShot(
        shot_audio,
        shot_volume);
}

public void FindFirePointAndGunSlot()
{
    Transform[] allChildren =
        enemy.GetComponentsInChildren<Transform>(true);

    foreach (Transform child in allChildren)
    {
        if (child.name == slot_name)
        {
            gunslot = child.gameObject;
        }

        if (child.name == fireppint_name)
        {
            firePoint = child.gameObject;
        }
    }

    if (gunslot != null)
    {
        Debug.Log("Gun slot found: " + gunslot.name);
    }
    else
    {
        Debug.LogWarning(
            "Gun slot not found: " + slot_name +
            " on enemy: " + enemy.name);
    }

    if (firePoint != null)
    {
        Debug.Log("Fire point found: " + firePoint.name);
    }
    else
    {
        Debug.LogWarning(
            "Fire point not found: " + fireppint_name +
            " on enemy: " + enemy.name);
    }
}
	public bool passA, passB, passObstruction;
	public void AvoidObstacle()
	{
		
		if(detected_obstruction!=null)
		{
			float movement_speed = enemy_speed;
			float offset_x = 40.0f;
			float offset_z = 40.0f;
			float size_x = detected_obstruction.transform.GetComponent<Collider>().bounds.size.x;
			float size_z = detected_obstruction.transform.GetComponent<Collider>().bounds.size.z;
			float coef_a = 1;
			if(enemy.transform.position.x>=detected_obstruction.transform.position.x)
				coef_a = 1;
			else
				coef_a = -1;
			Vector3 centre_object = detected_obstruction.transform.position;
			float x1 = centre_object.x + coef_a*(size_x/2 + offset_x);
			float z1 = centre_object.z - (size_z/2 + offset_z);
			float y1 = enemy.transform.position.y;
			Vector3 PointA = new Vector3(x1, y1, z1);
			
			float x2 = centre_object.x + coef_a*(size_x/2 + offset_x);
			float z2 = centre_object.z + (size_z/2 + offset_z);
			float y2 = enemy.transform.position.y;
			Vector3 PointB = new Vector3(x2, y2, z2);
			
			if(PointA!=Vector3.zero && passA==false)
			{
				enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, PointA, movement_speed*Time.deltaTime);
				
					if(Vector3.Distance(enemy.transform.position, PointA)<=5.0f)
						passA = true;
						else
						{
							PlayAnimation(run_animation,run_param,1);
						}
					
			}
			else if(PointB!=Vector3.zero && passA==true && passB==false)
			{
				enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, PointB, movement_speed*Time.deltaTime);

					if(Vector3.Distance(enemy.transform.position, PointB)<=5.0f)
						passB = true;
					else
					{
						PlayAnimation(run_animation,run_param,1);
					}
			}
			else if(passA==true && passB==true)//go back to normal regime
			{
				PlayAnimation(idle_animation,idle_param,1);
				detected_obstruction = null;
				stop_movement = false;
			}
			
			Debug.DrawLine(enemy.transform.position, PointA,Color.yellow);
			Debug.DrawLine(PointB, PointA,Color.yellow);
			Debug.DrawLine(target_object.transform.position, PointB,Color.yellow);
		}
	}
	public void RaycastObjectInFront()
{
    float ray_length = 60.0f;
	
	Vector3 rayOrigin =
    enemy.transform.position +
    Vector3.up * 1.0f;
    
	Ray ray =
        new Ray(
           rayOrigin,
            enemy.transform.forward);

    RaycastHit hit;
	
    if (Physics.Raycast(
        ray,
        out hit,
        ray_length))
    {
        if (hit.collider.CompareTag("building"))
        {
            detected_obstruction =
                hit.collider.gameObject;
        }
        else
        {
            detected_obstruction = null;
        }
    }
    else
    {
        detected_obstruction = null;
    }

    Debug.DrawRay(
        enemy.transform.position,
        enemy.transform.forward * ray_length,
        Color.red);
}
	void CanvasLookAtCamera()
{
    Transform canvas =
        enemy.transform.Find(canvas_name);

    if (canvas == null)
        return;

    Camera cam =
        Camera.main;

    if (cam == null)
        return;

    canvas.rotation =
        Quaternion.LookRotation(
            canvas.position - cam.transform.position);
}
	void RotateTowardsDirection(
        Vector3 direction)
    {
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        enemy.transform.rotation =
            Quaternion.Slerp(
                enemy.transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime);
    }
	
	public void PlayAnimation(string animation_string, string parameter_string, int num_param)
	{
		Animator anim = enemy.transform.GetComponent<Animator>();
		anim.SetInteger(parameter_string,num_param);
		anim.Play(animation_string);
		
		
	}
}
