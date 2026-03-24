using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//created by Lucas Juricka, 2026, maddex88@gmail.com

public class GenerateEnemies : MonoBehaviour
{
    public GameObject[] allEnemies;
    public GameObject[] generated_enemies;
    public GameObject[] allGenHouses;
    public GameObject[] allWalls;
    public GameObject[] chosenHideout;
	public GameObject enemy;
public string runAnimationState = "Run";
public string runParameter = "MoveState";

private Animator[] enemyAnimators;

    public bool[] IsAbleToShoot;

    public int enemies_count;
    public int max_count = 20;

    public GameObject selectedGenHouse;
    public GameObject selectedHideout;

    public Vector3 selectedVector3_waypoint;

    public bool enemy_instantiated;

    public GameObject enemy_prefab;

    public GameObject SelectedWall_for_navigation;

    public int gen_enemies_index;

    public float[] distance_to_target_destination;

    public bool IsGoingToTargetDestination;
    public bool IsAvoidingWall;
    public bool IsAtFinalObject;

    public int control_count_hideouts;

    public Vector3 PointA;
    public Vector3 PointB;

    public bool passedA;
    public bool passedB;
	public int gen_enemies_index2;
    void Start()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        allGenHouses = GameObject.FindGameObjectsWithTag("GenerationHouse");
        allWalls = GameObject.FindGameObjectsWithTag("Wall");

        enemies_count = allEnemies.Length;

        generated_enemies = new GameObject[max_count];
		foreach(GameObject go in allEnemies)
		{
			if(go.name.Contains("BlueEnemy"))
			{
				generated_enemies[gen_enemies_index2] = go;
				gen_enemies_index2++;
			}
		}
        chosenHideout = new GameObject[max_count];
        IsAbleToShoot = new bool[max_count];
        distance_to_target_destination = new float[max_count];

        StartCoroutine(CreateEnemyOnRandomTick());
    }

    void Update()
    {
		allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemies_count = GameObject.FindGameObjectsWithTag("Enemy").Length;
		foreach(GameObject go in allEnemies)
		{
			if(go.name.Contains("BlueEnemy"))
			{
				generated_enemies[gen_enemies_index2] = go;
				gen_enemies_index2++;
			}
		}
		if(enemy != null)
{
    SelectedWall_for_navigation = GameObject.Find("big_wall");

    // přiřadí každému enemy náhodný startovní dům
    for (int i = 0; i < gen_enemies_index; i++)
    {
        GameObject currentEnemy = generated_enemies[i];
        if(currentEnemy != null)
        {
            // vyber náhodný dům
            int randHouseIndex = Random.Range(0, allGenHouses.Length);
            GameObject randomHouse = allGenHouses[randHouseIndex];

            // přesuň enemy na dům
            currentEnemy.transform.position = randomHouse.transform.position;

            // označíme cíl pro navigaci
            distance_to_target_destination[i] = Vector3.Distance(currentEnemy.transform.position, SelectedWall_for_navigation.transform.position);

            // spustíme běhovou animaci
            PlayRunAnimation(i);
        }
    }

    // nastavíme flag, že enemy začíná jít k cíli
    IsAtFinalObject = false;
    IsGoingToTargetDestination = true;
}
        if (selectedGenHouse != null)
        {
            selectedVector3_waypoint = selectedGenHouse.transform.position;

            if (!enemy_instantiated && gen_enemies_index < max_count)
            {
                GameObject newEnemy = Instantiate(enemy_prefab, selectedVector3_waypoint, Quaternion.identity);
				
				Animator anim = newEnemy.GetComponent<Animator>();

				if(anim == null)
					{
						anim = newEnemy.AddComponent<Animator>();
					}

				if(enemyAnimators == null)
					enemyAnimators = new Animator[max_count];

					enemyAnimators[gen_enemies_index] = anim;
                generated_enemies[gen_enemies_index] = newEnemy;

                enemy_instantiated = true;
                gen_enemies_index++;
            }
        }

        if (!IsAtFinalObject)
        {
            NavigateEnemiesToTargetDestination();
        }
        else if (IsAtFinalObject && control_count_hideouts < enemies_count)
        {
            for (int i = 0; i < enemies_count; i++)
            {
                FindRandomHideout(SelectedWall_for_navigation, i);
            }
        }
        else if (IsAtFinalObject && control_count_hideouts >= enemies_count)
        {
            for (int i = 0; i < enemies_count; i++)
            {
                MoveEnemyToFinalHideout(i);
            }
        }
		
		SetupEnvironmentColliders();
    }
	
	void SetupEnvironmentColliders()
{
    GameObject[] allObjects = FindObjectsOfType<GameObject>();

    foreach(GameObject go in allObjects)
    {
        if(go.name.Contains("Building") || go.name.Contains("Pole"))
        {
            if(go.GetComponent<Collider>() == null)
            {
                BoxCollider bc = go.AddComponent<BoxCollider>();

                Renderer r = go.GetComponent<Renderer>();

                if(r != null)
                {
                    bc.center = r.bounds.center - go.transform.position;
                    bc.size = r.bounds.size;
                }
            }
        }
    }
}

    public void MoveEnemyToFinalHideout(int enemy_indexx)
    {
        float contact_distance = 2.0f;
        float speed_movement = 20.0f;

        GameObject go = generated_enemies[enemy_indexx];

        if (go != null && chosenHideout[enemy_indexx] != null)
        {
			Vector3 target = chosenHideout[enemy_indexx].transform.position;
			PlayRunAnimation(enemy_indexx);
            go.transform.position =
                Vector3.MoveTowards(
                    go.transform.position,
                    chosenHideout[enemy_indexx].transform.position,
                    speed_movement * Time.deltaTime
                );
				//plynule otaceni
				enemy.transform.rotation = Quaternion.Slerp(
				enemy.transform.rotation,
				Quaternion.LookRotation(target - enemy.transform.position),
				Time.deltaTime * 5f
				);

            if (Vector3.Distance(go.transform.position, chosenHideout[enemy_indexx].transform.position) <= contact_distance)
            {
                IsAbleToShoot[enemy_indexx] = true;
            }
        }
    }

    public void FindRandomHideout(GameObject starting_base, int enemy_index)
    {
        if (chosenHideout[enemy_index] != null) return;

        GameObject wall = FindRandomHideoutWall(enemy_index);

        if (wall != null && wall.transform.position.z > starting_base.transform.position.z)
        {
            chosenHideout[enemy_index] = wall;
            control_count_hideouts++;
        }
    }

    IEnumerator CreateEnemyOnRandomTick()
    {
        while (enemies_count < max_count)
        {
            yield return new WaitForSeconds(0.5f);

            int rand_number = Random.Range(1, 6);

            foreach (GameObject go in allGenHouses)
            {
                if (go.name.Contains("GenerationHouse_" + rand_number))
                {
                    selectedGenHouse = go;
                    enemy_instantiated = false;
                    break;
                }
            }
        }
    }

    public GameObject FindRandomHideoutWall(int enemy_index)
    {
        int rand_number = Random.Range(1, 6);

        foreach (GameObject go in allWalls)
        {
            if (go.name.Contains("Wall" + rand_number))
            {
                selectedHideout = go;
                break;
            }
        }

        return selectedHideout;
    }

    public void NavigateEnemiesToTargetDestination()
    {
        for (int i = 0; i < gen_enemies_index; i++)
        {
            NavigateEnemyToDestination(i);
			
        }
    }

    public void NavigateEnemyToDestination(int enemy_index)
    {
        float speed_movement = 20.0f;

        GameObject go = generated_enemies[enemy_index];

        if (go == null || SelectedWall_for_navigation == null) return;

        distance_to_target_destination[enemy_index] =
            Vector3.Distance(go.transform.position, SelectedWall_for_navigation.transform.position);
		
        if (distance_to_target_destination[enemy_index] >= 1.0f)
        {
			PlayRunAnimation(enemy_index);
            go.transform.position =
                Vector3.MoveTowards(
                    go.transform.position,
                    SelectedWall_for_navigation.transform.position,
                    speed_movement * Time.deltaTime
                );

            if (!IsAvoidingWall)
            {
                RaycastForward(go, 20.0f);
            }
        }
    }
	void PlayRunAnimation(int enemy_index)
{
    if(enemyAnimators == null) return;

    Animator anim = enemyAnimators[enemy_index];

    if(anim == null) return;

    anim.Play(runAnimationState);
    anim.SetInteger(runParameter, 1);
}
    public void RaycastForward(GameObject enemy, float raycast_length)
    {
        Ray ray = new Ray(enemy.transform.position + Vector3.forward * raycast_length, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 20f))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Building"))
            {
                AvoidWall(hit.collider.gameObject, enemy);
                IsAvoidingWall = true;

                if (hit.collider.gameObject.name == SelectedWall_for_navigation.name)
                    IsAtFinalObject = true;
            }
        }
    }

    public void AvoidWall(GameObject obstruction_object, GameObject selected_enemy)
    {
        float speed_movement = 20.0f;

        float size_x = obstruction_object.GetComponent<Collider>().bounds.size.x;
        float size_z = obstruction_object.GetComponent<Collider>().bounds.size.z;

        float offset_size = 5.0f;

        if (PointA == Vector3.zero || PointB == Vector3.zero)
        {
            if (selected_enemy.transform.position.x < obstruction_object.transform.position.x)
            {
                PointA = new Vector3(
                    obstruction_object.transform.position.x - size_x / 2 - offset_size,
                    selected_enemy.transform.position.y,
                    obstruction_object.transform.position.z - size_z / 2 - offset_size
                );

                PointB = new Vector3(
                    obstruction_object.transform.position.x - size_x / 2 - offset_size,
                    selected_enemy.transform.position.y,
                    obstruction_object.transform.position.z + size_z / 2 + offset_size
                );
            }
            else
            {
                PointA = new Vector3(
                    obstruction_object.transform.position.x + size_x / 2 + offset_size,
                    selected_enemy.transform.position.y,
                    obstruction_object.transform.position.z - size_z / 2 - offset_size
                );

                PointB = new Vector3(
                    obstruction_object.transform.position.x + size_x / 2 + offset_size,
                    selected_enemy.transform.position.y,
                    obstruction_object.transform.position.z + size_z / 2 + offset_size
                );
            }
        }
        else
        {
            if (!passedA && !passedB)
            {
                selected_enemy.transform.position =
                    Vector3.MoveTowards(selected_enemy.transform.position, PointA, speed_movement * Time.deltaTime);

                if (Vector3.Distance(selected_enemy.transform.position, PointA) <= 1f)
                    passedA = true;
            }
            else if (passedA && !passedB)
            {
                selected_enemy.transform.position =
                    Vector3.MoveTowards(selected_enemy.transform.position, PointB, speed_movement * Time.deltaTime);

                if (Vector3.Distance(selected_enemy.transform.position, PointB) <= 1f)
                    passedB = true;
            }

            if (passedA && passedB)
            {
                IsAvoidingWall = false;
                PointA = Vector3.zero;
                PointB = Vector3.zero;
                passedA = false;
                passedB = false;
            }
        }
    }
}