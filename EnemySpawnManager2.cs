using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class EnemySpawnManager2 : MonoBehaviour
{
	[Header("Path Debug")]
	public bool showGeneratedPath = true;
	public float obstacleOffset = 20.0f;
    [Header("References")]
    public GameObject player;
    public GameObject enemyPrefab;
    public GameObject tilePrefab;
	public string enemy_prefab_name = "hezbollahTerrorist_gun_animated 1";
	public string player_name = "WomanSniper";
	public string coverPointName = "CoverPoint";
	public GameObject[] genHouses;
	public GameObject newWall, currentWall, nearest_gen_house;
	public Vector3 dir;
    [Header("Settings")]
    public int numberCoverPoints = 6;
    public int requiredAttackers = 6;
	public int count_generated_enemies;
    public float moveSpeed = 25.0f;
    public float coverOffset = 8.0f;
	public float default_offset = 30.0f;
	public int current_count_Generated_tiles;
    [Header("Debug")]
    public TMP_Text countText;

    public GameObject nearestGenerationHouse;

    public Vector3[] coverPoints;
	public int count_cover_points;
    public List<GameObject> coverPositionObjects =
        new List<GameObject>();

    public List<GameObject> generatedEnemies =
        new List<GameObject>();

    public int enemiesReady;
	
	public void Start()
	{
		enemyPrefab = GameObject.Find(enemy_prefab_name);
		player = GameObject.Find(player_name);
		tilePrefab = GameObject.Find(coverPointName);
		genHouses = GameObject.FindGameObjectsWithTag("GenerationHouse");
	}

    void Update()
    {
        if (player == null)
		{
            enemyPrefab = GameObject.Find(enemy_prefab_name);
		player = GameObject.Find(player_name);
		tilePrefab = GameObject.Find(coverPointName);
		}
		
		
       newWall =
            FindNearestWall(player);

        if (newWall != currentWall)
        {
            currentWall = newWall;
			if(lastWall!=currentWall)
            RegenerateCoverPoints();
			else if(count_cover_points<numberCoverPoints)
			{
				currentWall =
            FindNearestWall(player);
            nearestGenerationHouse =
                FindNearestGenerationHouse(currentWall);
				     CreateCoverPointsAtWall(
					currentWall,
					numberCoverPoints);
			}
        }
		
		foreach(GameObject go in coverPositionObjects)
		{
    if(go == null)
        continue;

    Renderer rend =
        go.GetComponentInChildren<Renderer>();

    if(rend != null)
        rend.enabled = true;
	
	
		}
		if(count_generated_enemies<requiredAttackers)
		{
			FindNearestGenHouse(player, "GenerationHouse");//searches house by the tag
			
			if(nearestGenerationHouse != null)
			{
			Debug.DrawLine(
            player.transform.position,
            nearestGenerationHouse.transform.position,
            Color.yellow);
			}
			
			
		}
		else if(count_generated_enemies>=requiredAttackers)
        MoveEnemies();
    }
	
	public List<Vector3> GeneratePathToCoverPoint(
    Vector3 startPos,
    Vector3 endPos)
{
    List<Vector3> path =
        new List<Vector3>();

    path.Add(startPos);

    Vector3 dir =
        (endPos - startPos).normalized;

    float distance =
        Vector3.Distance(
            startPos,
            endPos);

    RaycastHit hit;

    if(Physics.Raycast(
        startPos,
        dir,
        out hit,
        distance))
    {
        if(hit.collider.CompareTag("building") ||
           hit.collider.CompareTag("wreck"))
        {
            Collider col =
                hit.collider;

            Bounds b =
                col.bounds;

            Vector3 contact =
                hit.point;

            Vector3 cornerA =
                new Vector3(
                    b.min.x - obstacleOffset,
                    contact.y,
                    b.min.z - obstacleOffset);

            Vector3 cornerB =
                new Vector3(
                    b.max.x + obstacleOffset,
                    contact.y,
                    b.max.z + obstacleOffset);

            Vector3 escapePoint =
                new Vector3(
                    b.max.x + obstacleOffset,
                    contact.y,
                    endPos.z);

            path.Add(contact);
            path.Add(cornerA);
            path.Add(cornerB);
            path.Add(escapePoint);
        }
    }

    path.Add(endPos);

    return path;
}

public void DrawPath(
    List<Vector3> path)
{
    if(!showGeneratedPath)
        return;

    for(int i = 0;
        i < path.Count - 1;
        i++)
    {
        Debug.DrawLine(
            path[i],
            path[i + 1],
            Color.cyan);
    }
}

    void MoveEnemies()
    {
        foreach (GameObject enemy in generatedEnemies)
        {
            if (enemy == null)
                continue;

        

		if(dir != Vector3.zero)
		{
    enemy.transform.rotation =
        Quaternion.Slerp(
            enemy.transform.rotation,
            Quaternion.LookRotation(dir),
            5.0f * Time.deltaTime);
}

            /*enemy.transform.position =
                Vector3.MoveTowards(
                    enemy.transform.position,
                    data.targetWaypoint.position,
                    moveSpeed * Time.deltaTime
                );

            float distance =
                Vector3.Distance(
                    enemy.transform.position,
                    data.targetWaypoint.position);

            if (distance < 1.0f &&
                !data.attackPrepared)
            {
                data.attackPrepared = true;

                PrepareForAttack(enemy);

                enemiesReady++;

                if (countText != null)
                {
                    countText.text =
                        "Prepared enemies: " +
                        enemiesReady;
                }
            }*/
        }
    }

    public void SpawnEnemy()
    {
        if(nearestGenerationHouse == null)
		{
		nearestGenerationHouse =
        FindNearestGenHouse(
            player,
            "GenerationHouse");
		}
		
		if(nearestGenerationHouse == null)
		return;

     //   if (coverPositionObjects.Count == 0)
     //       return;

        int waypointIndex =
            generatedEnemies.Count %
            coverPositionObjects.Count;
			
			Transform coverPoint =
			coverPositionObjects[
			waypointIndex].transform;

			List<Vector3> generatedPath =
			GeneratePathToCoverPoint(
			nearestGenerationHouse.transform.position,
			coverPoint.transform.position);

			DrawPath(generatedPath);

        GameObject enemy =
            Instantiate(
                enemyPrefab,
                nearestGenerationHouse.transform.position,
                nearestGenerationHouse.transform.rotation);

        enemy.name =
            "gen_enemy_" +
            generatedEnemies.Count;

        HezbollahTerroristBehaviour data =
            enemy.AddComponent<HezbollahTerroristBehaviour>();
			
			

        data.finalCoverPoint =
            coverPositionObjects[
                waypointIndex].transform;
				
				HezbollahTerroristBehaviour ai = enemy.GetComponent<HezbollahTerroristBehaviour>();

				if (ai != null)
					{
					coverPoint = coverPositionObjects[waypointIndex].transform;

					ai.finalCoverPoint = coverPoint;
					ai.IsNewlyGenerated = true;

					if (Vector3.Distance(coverPoint.position, enemy.transform.position) <= 10.0f)
					ai.IsDeployedAtWall = true;
					}

        generatedEnemies.Add(enemy);
    }

    void RegenerateCoverPoints()
    {
		current_count_Generated_tiles = 0;
        foreach(GameObject go in coverPositionObjects)
		{
            if (go != null)
                Destroy(go);
        }

        coverPositionObjects.Clear();

      //  if (currentWall == null)
       //     return;
		float yRotation =
			currentWall.transform.eulerAngles.y;

		if (Mathf.Abs(
        Mathf.DeltaAngle(
            yRotation,
            180.0f)) <= 2.0f)
			{
		CreateCoverPointsAtWall(
        currentWall,
        numberCoverPoints);
		}
		
		for(int i = 0; i < numberCoverPoints; i++)
		{
			SpawnEnemy();
		}
    }
public GameObject lastWall;
    void CreateCoverPointsAtWall(
        GameObject wall,
        int pointCount)
    {
        Collider col =
            wall.GetComponent<Collider>();

		float x_offset = default_offset;

        coverPoints =
            new Vector3[numberCoverPoints];

        float wallLength_z =
            col.bounds.size.z;

        float step_z =
            wallLength_z / numberCoverPoints;

        for (int i = 0;
             i < numberCoverPoints;
             i++)
        {
			float x_pos = wall.transform.position.x - x_offset;
			float y_pos = 18.0f;
			float z_pos = wall.transform.position.z - wallLength_z/2 + i*step_z;
            Vector3 pos = new Vector3(x_pos, y_pos, z_pos);

       
			if(current_count_Generated_tiles<numberCoverPoints)
			{
            coverPoints[i] = pos;
			
            GameObject tile =
                Instantiate(
                    tilePrefab,
                    pos,
                    Quaternion.identity);
					
					tile.transform.localScale =
				new Vector3(
					21.998f,
					21.998f,
					21.998f);

            tile.name =
                "CoverPointTile2_" + i;

            coverPositionObjects.Add(tile);
			current_count_Generated_tiles++;
			lastWall = wall;
			}
			
			GameObject[] generated_tiles = GameObject.FindGameObjectsWithTag("tiles");
			
			foreach(GameObject go in generated_tiles)
				{
				Renderer rend =
				go.GetComponentInChildren<Renderer>();

				if(rend != null)
				rend.enabled = true;
				}
			
        }
    }

    GameObject FindNearestWall(
        GameObject selectedPlayer)
    {
        GameObject[] walls =
            GameObject.FindGameObjectsWithTag(
                "Wall");

        float closestDistance =
            Mathf.Infinity;

        GameObject closestWall =
            null;

        foreach (GameObject wall in walls)
        {
            float dist =
                Vector3.Distance(
                    wall.transform.position,
                    selectedPlayer.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestWall = wall;
            }
        }

        return closestWall;
    }
	
	GameObject FindNearestGenHouse(
        GameObject selectedPlayer, string searched_tag)
    {
        GameObject[] genHouses =
            GameObject.FindGameObjectsWithTag(
                searched_tag);

        float closestDistance =
            Mathf.Infinity;

        GameObject nearestGenerationHouse =
            null;

        foreach (GameObject go in genHouses)
        {
            float dist =
                Vector3.Distance(
                    go.transform.position,
                    selectedPlayer.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                nearestGenerationHouse = go;
            }
        }

        return nearestGenerationHouse;
    }

    GameObject FindNearestGenerationHouse(
        GameObject wall)
    {
        GameObject[] houses =
            GameObject.FindGameObjectsWithTag(
                "GenerationHouse");

        float closestDistance =
            Mathf.Infinity;

        GameObject closest =
            null;

        foreach (GameObject house in houses)
        {
            float dist =
                Vector3.Distance(
                    house.transform.position,
                    wall.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = house;
            }
        }

        return closest;
    }

    void PrepareForAttack(
        GameObject enemy)
    {
        enemy.transform.LookAt(
            player.transform);

        Animator anim =
            enemy.GetComponent<Animator>();

        if (anim != null)
        {
            anim.Play("Attack");
        }
    }
}


