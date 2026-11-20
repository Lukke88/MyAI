
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountEnemiesAndScore : MonoBehaviour
{
    GameObject[] allEnemies, allPlayers, allGenHouses;

    public int players_count, enemies_count, dead_enemies_count, sum_score;

    public TMP_Text playersCountText, enemiesCountText, deadEnemiesText, sumScoreText;
	
	    [Header("Item Pickup System")]
    public GameObject player;
    public Animator playerAnimator;
    public Transform backpackPoint;
	public int maximum_generated_enemies_during_game = 200, currentCountOfGenEnemies;//we can change parameters to generate more or less enemies during the game
    public float pickupMoveSpeed = 320.0f;
    public float pickupStoppingDistance = 15.0f;
    public float pickupRotationSpeed = 8.0f;
    public float doubleClickInterval = 0.35f;
    public float pickupAnimationDuration = 1.2f;

    public string walkingAnimationParameter = "IsWalking";
    public string pickupAnimationTrigger = "Pick_up";

    private float lastClickTime;
    private GameObject lastClickedItem;
    private bool isPickingUp;
    private bool weaponsSpawned;

    public List<GameObject> inventory = new List<GameObject>();
	
	/*
	4. Nastavení v Unity Inspectoru

	BackpackPoint

	Vytvořte Empty GameObject na zádech vojáka. Pojmenujte jej BackpackPoint. Zde se budou připojovat sebrané předměty.

	Animator

	Přidejte Bool parametr IsWalking a Trigger parametr Pick_up. Propojte je se stavy Walk a Pick_up.


	Pickup points

	Vytvořte Empty objekty pick_point_1, pick_point_2 atd. Nastavte jim tag Item. Umístěte je po mapě.
	*/

    // Start is called before the first frame update
    void Start()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        allPlayers = GameObject.FindGameObjectsWithTag("Player");
		allGenHouses = GameObject.FindGameObjectsWithTag("GenerationHouse");
        enemies_count = allEnemies.Length;
        players_count = allPlayers.Length;

        playersCountText.text = players_count.ToString();
        enemiesCountText.text = enemies_count.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        allPlayers = GameObject.FindGameObjectsWithTag("Player");

        players_count = allPlayers.Length;

        playersCountText.text = players_count.ToString();
        enemiesCountText.text = enemies_count.ToString();

        CountDeadEnemies();

        //SpreadRandomlyWeapons("Guns", "pick_point_", "Item");
    }

    public void CountDeadEnemies()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject go in allEnemies)
        {
            EnemyHealth eh = go.transform.GetComponent<EnemyHealth>();
            ArabWarriorAttacks ac = go.transform.GetComponent<ArabWarriorAttacks>();

            if (eh != null && ac != null)
            {
                if (eh.isDead == true)//check the status of enemy
                {
                    if (ac.WasDeadEnemyCounted == false)
                    {
                        sum_score += ac.points_for_dead_enemy;
                        dead_enemies_count++;//add count of dead enemies
                        enemies_count--;//decrease living enemies 

                        ac.WasDeadEnemyCounted = true;

                        playersCountText.text = "Players:" + players_count.ToString();
                        enemiesCountText.text = "Enemies alive:" + enemies_count.ToString();
                        deadEnemiesText.text = "Deads:" + dead_enemies_count.ToString();
                        sumScoreText.text = "Game Score:" + sum_score.ToString();
						
						SubstituteDeadEnemy(go, go.transform.position);
                    }
                }
            }
        }
    }
	public GameObject nearestGenHouse, newEnemy;
	public bool NewEnemyInstantiated;
	public void SubstituteDeadEnemy(GameObject enemy_to_substitute, Vector3 old_enemy_position)
	{
		float nearestHouseDistance = Mathf.Infinity;
		if(nearestGenHouse==null)
		{
			foreach(GameObject go in allGenHouses)
			{
				//look for nearest gen house
				
				if(Vector3.Distance(go.transform.position, enemy_to_substitute.transform.position)<nearestHouseDistance)
				{
					nearestGenHouse = go;
					nearestHouseDistance = Vector3.Distance(go.transform.position, enemy_to_substitute.transform.position);
				}
			}
		}
		else if(nearestGenHouse!=null && NewEnemyInstantiated==false && newEnemy==null && currentCountOfGenEnemies<maximum_generated_enemies_during_game)
		{
			newEnemy = Instantiate(enemy_to_substitute, old_enemy_position, Quaternion.identity);
			ArabEnemyRunsToFinalPosition a = newEnemy.transform.GetComponent<ArabEnemyRunsToFinalPosition>();
			a.target_object = enemy_to_substitute;//newly generated enemy from generation house runs to position of old, dead enemy, this ensures enemy waves from city
			a.enabled = true;//enable navigation script when is new enemy created
			NewEnemyInstantiated = true;//no more instances
			currentCountOfGenEnemies++;//overrall enemies count from start of the game
		}
		else if(newEnemy!=null)//we have created a monster
		{
			if(Vector3.Distance(newEnemy.transform.position, enemy_to_substitute.transform.position)<=20.0f)
			{
				//get all back to defaults while new enemy is on the place
				nearestGenHouse = null;
				NewEnemyInstantiated = false;
				newEnemy = null;
				Destroy(enemy_to_substitute);//Destroys old figure
			}
		}
	}

    
void SpreadRandomlyWeaponsFromFile(string file_name, string pickpoint_general_name, string pickpoint_tag)
{
    //gets random weapon from file with file_name, puts to random pick_point where pickpoint_general_name is first part, followed by number of pickpoint(null object), and every pickpoint will have tag
    //double click->player will come->play pickup animation->weapon will be moved from ground to players hand null object or back null object and written to player's inventory

    // ---------------------------------------------------------
    // 1. INITIALIZE PLAYER AND ANIMATOR
    // ---------------------------------------------------------

    if (player == null)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 0)
            player = players[0];
    }

    if (player == null || backpackPoint == null)
        return;

    if (playerAnimator == null)
        playerAnimator = player.GetComponentInChildren<Animator>();

    if (playerAnimator == null)
        return;


    // ---------------------------------------------------------
    // 2. RANDOMLY SPAWN WEAPONS AT PICKUP POINTS
    // ---------------------------------------------------------

    if (weaponsSpawned == false)
    {
        // Load all weapon prefabs from Resources/file_name
        GameObject[] weaponPrefabs = Resources.LoadAll<GameObject>(file_name);

        // Find all objects with the specified pickup point tag
        GameObject[] allPickPoints = GameObject.FindGameObjectsWithTag(pickpoint_tag);

        List<GameObject> validPickPoints = new List<GameObject>();

        foreach (GameObject point in allPickPoints)
        {
            // Only use objects whose names start with the specified prefix
            if (point.name.StartsWith(pickpoint_general_name))
            {
                validPickPoints.Add(point);
            }
        }

        if (weaponPrefabs.Length == 0 || validPickPoints.Count == 0)
            return;

        foreach (GameObject point in validPickPoints)
        {
            // Select random weapon prefab
            GameObject randomWeapon = weaponPrefabs[Random.Range(0, weaponPrefabs.Length)];

            // Spawn weapon at the selected pickup point
            Instantiate(randomWeapon, point.transform.position, point.transform.rotation);
        }

        weaponsSpawned = true;
    }


    // ---------------------------------------------------------
    // 3. DETECT DOUBLE CLICK ON PICKUP ITEM
    // ---------------------------------------------------------

    if (Input.GetMouseButtonDown(0) && isPickingUp == false)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10000.0f))
        {
            // Find the Item tag on the hit object or its parent
            Transform itemTransform = hit.collider.transform;

            while (itemTransform != null && !itemTransform.CompareTag("Item"))
            {
                itemTransform = itemTransform.parent;
            }

            if (itemTransform != null)
            {
                GameObject clickedItem = itemTransform.gameObject;

                // Check whether the second click targets the same item
                if (clickedItem == lastClickedItem &&
                    Time.time - lastClickTime <= doubleClickInterval)
                {
                    // Prevent another pickup from starting
                    isPickingUp = true;

                    GameObject targetItem = clickedItem;

                    lastClickedItem = null;
                    lastClickTime = 0f;


                    // ---------------------------------------------------------
                    // 4. MOVE PLAYER TO THE PICKUP ITEM
                    // ---------------------------------------------------------

                    System.Collections.IEnumerator PickupRoutine(GameObject item)
                    {
                        Vector3 destination = new Vector3(
                            item.transform.position.x,
                            player.transform.position.y,
                            item.transform.position.z
                        );

                        // Disable walking animation before starting movement
                        playerAnimator.SetBool(walkingAnimationParameter, true);

                        // Move player towards the selected item
                        while (Vector3.Distance(
                            player.transform.position,
                            destination) > pickupStoppingDistance)
                        {
                            if (item == null)
                                break;

                            // Update destination in case item position changes
                            destination = new Vector3(
                                item.transform.position.x,
                                player.transform.position.y,
                                item.transform.position.z
                            );

                            Vector3 direction = (destination - player.transform.position).normalized;

                            // Rotate player towards the item
                            if (direction.sqrMagnitude > 0.001f)
                            {
                                Quaternion targetRotation = Quaternion.LookRotation(direction);

                                player.transform.rotation = Quaternion.Slerp(
                                    player.transform.rotation,
                                    targetRotation,
                                    pickupRotationSpeed * Time.deltaTime
                                );
                            }

                            // Move player at the specified speed
                            player.transform.position = Vector3.MoveTowards(
                                player.transform.position,
                                destination,
                                pickupMoveSpeed * Time.deltaTime
                            );

                            yield return null;
                        }

                        // Stop walking animation
                        playerAnimator.SetBool(walkingAnimationParameter, false);

                        if (item == null)
                        {
                            isPickingUp = false;
                            yield break;
                        }


                        // ---------------------------------------------------------
                        // 5. PLAY PICKUP ANIMATION
                        // ---------------------------------------------------------

                        playerAnimator.SetTrigger(pickupAnimationTrigger);

                        // Wait until pickup animation finishes
                        yield return new WaitForSeconds(pickupAnimationDuration);


                        // ---------------------------------------------------------
                        // 6. MOVE ITEM TO BACKPACK POINT
                        // ---------------------------------------------------------

                        if (item != null)
                        {
                            // Disable item colliders
                            Collider[] itemColliders =
                                item.GetComponentsInChildren<Collider>();

                            foreach (Collider col in itemColliders)
                                col.enabled = false;

                            // Disable physics
                            Rigidbody rb = item.GetComponent<Rigidbody>();

                            if (rb != null)
                            {
                                rb.isKinematic = true;
                                rb.useGravity = false;
                            }

                            // Attach weapon to player's backpack
                            item.transform.SetParent(backpackPoint);

                            item.transform.localPosition = Vector3.zero;
                            item.transform.localRotation = Quaternion.identity;


                            // ---------------------------------------------------------
                            // 7. ADD ITEM TO PLAYER INVENTORY
                            // ---------------------------------------------------------

                            if (!inventory.Contains(item))
                            {
                                inventory.Add(item);
                            }
                        }

                        isPickingUp = false;
                    }

                    // Start the complete pickup sequence
                    StartCoroutine(PickupRoutine(targetItem));
                }
                else
                {
                    // First click: remember selected item and click time
                    lastClickedItem = clickedItem;
                    lastClickTime = Time.time;
                }
            }
        }
    }
}

}
