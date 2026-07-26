using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroFindsCover : MonoBehaviour
{
	public GameObject hero, nearest_wall, marker_wall, enemy;
	public GameObject[] allWalls, generatedMarkers;
	public GameObject waypointPrefab;
	public Vector3[] allWallWaypoints;
	public Vector3 hit_point;
	public float wall_offset_x = 100.0f;
	
	public bool coverWaypointsCreated = false;

	public GameObject selectedWaypoint;

	public float arriveDistance = 5f;

	public float moveSpeed = 120f;

	public Animator heroAnimator;

	private RaycastHit hit;
	
	public GameObject enemyTargetWall;
	public GameObject[] enemyCoverMarkers;

	public Vector3[] enemyWaypoints;
	Vector3[] coverPositions;

	public bool enemySearchingCover = false;

	public float enemyWaypointOffset = 100f;
    // Start is called before the first frame update
    void Start()
    {
        hero = GameObject.Find(this.name);
    }

    // Update is called once per frame
    void Update()
    {
        if(hero!=null)
			FindCoverNearWall();
		
		if(selectedWaypoint != null)
			{
				MoveHeroToCover();
			}
			if(hero!=null && nearest_wall!=null)
			{
				if(Vector3.Distance(hero.transform.position, selectedWaypoint.transform.position)<=20.0f)
				{
				Vector3 dir2 =
					nearest_wall.transform.position -
					hero.transform.position;

					dir2.y = 0;

					hero.transform.rotation =
					Quaternion.LookRotation(dir2);
				}
			}
			
			float distance =
    Vector3.Distance(hit_point,
                     nearest_wall.transform.position);
					 
					 //enemy detects player
					 Vector3 enemyEye = enemy.transform.position + Vector3.up * 80f;
						Vector3 heroChest = hero.transform.position + Vector3.up * 80f;

						Vector3 dir = heroChest - enemyEye;

						if(Physics.Raycast(enemyEye, dir.normalized, out hit, dir.magnitude))
						{
						if(hit.collider.CompareTag("Wall"))
							{
								// hráč je krytý, enemy nemuze strilet
								enemySearchingCover = true;

								FindEnemyFlankWall();//enemy hleda novy kryt pokud jeho raycast narazi na zed pred hracem
							}
						}
/*
if(distance < 100.0f)//for later, erasing waypoints by distance
{
    if(!coverWaypointsCreated)
    {
        CreateWaypointsAtWall(...);
        coverWaypointsCreated = true;
    }
}
else
{
    DestroyWaypoints();
    coverWaypointsCreated = false;
}*/
    }
	
	public void FindEnemyFlankWall()
{
    float minDistance = Mathf.Infinity;

    foreach(GameObject wall in allWalls)
    {
        if(wall == nearest_wall)
            continue;

        float d =
            Vector3.Distance(
                wall.transform.position,
                nearest_wall.transform.position);

        if(d < minDistance)
        {
            minDistance = d;
            enemyTargetWall = wall;
        }
    }

    if(enemyTargetWall != null)
    {
        CreateEnemyWaypoints(enemyTargetWall);
    }
}

public void CreateEnemyWaypoints(GameObject wall)//enemy creates waypoints on opposite side than player
{
    enemyWaypoints = new Vector3[6];

    float zsize =
        wall.GetComponent<Collider>().bounds.size.z;

    float gap = zsize / 6f;

    for(int i=0;i<6;i++)
    {
        enemyWaypoints[i]=new Vector3(
            wall.transform.position.x + enemyWaypointOffset,
            wall.transform.position.y,
            wall.transform.position.z
            -zsize/2
            +(i+0.5f)*gap);
    }

    MoveEnemyToRandomWaypoint();
}

void MoveEnemyToRandomWaypoint()
{
    int r = Random.Range(0, enemyWaypoints.Length);

    StartCoroutine(
        EnemyMoveRoutine(enemyWaypoints[r]));//enemy moves across newly created waypoints
}

IEnumerator EnemyMoveRoutine(Vector3 target)
{
    while(Vector3.Distance(enemy.transform.position,target)>5f)
    {
        enemy.transform.position=
            Vector3.MoveTowards(
                enemy.transform.position,
                target,
                80f*Time.deltaTime);

        yield return null;
    }

    enemySearchingCover=false;
}
	
	void MoveHeroToCover()
	{
    heroAnimator.Play("FastRun");

    hero.transform.position =
        Vector3.MoveTowards(
            hero.transform.position,
            selectedWaypoint.transform.position,
            moveSpeed * Time.deltaTime);

    if(Vector3.Distance(hero.transform.position,
        selectedWaypoint.transform.position) < arriveDistance)
    {
        hero.transform.position =
            selectedWaypoint.transform.position;

        selectedWaypoint = null;

        heroAnimator.Play("CrouchingIdleBehindWall");
    }
	}
	public void FindCoverNearWall()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if(Physics.Raycast(ray, out hit, 2000))
{
    hit_point = hit.point;
    FindNearestWallAtHitPoint(hit_point);
}
		if(hit_point!=null)FindNearestWallAtHitPoint(hit_point);
		
		if(nearest_wall!=null)
		{
			CreateWaypointsAtWall(6, "CoverPoint", wall_offset_x);
		}
	}
	
	public void FindNearestWallAtHitPoint(Vector3 cursor_point)
	{
		allWalls = GameObject.FindGameObjectsWithTag("Wall");
		float max_distance = Mathf.Infinity;
		foreach(GameObject go in allWalls)
		{
			if(Vector3.Distance(go.transform.position, cursor_point)<max_distance)
			{
				max_distance = Vector3.Distance(go.transform.position, hero.transform.position);
				nearest_wall = go;
			}
		}
	}
	
	public void CreateWaypointsAtWall(int number_waypoints, string waypointPrefabName, float offset_x_size)
	{
		generatedMarkers = new GameObject[number_waypoints];
		if(waypointPrefab==null)
		waypointPrefab = GameObject.Find(waypointPrefabName);
		allWallWaypoints = new Vector3[number_waypoints];
		foreach(GameObject go in generatedMarkers)
		{
			if(go != null)
				Destroy(go);
		}
		for(int i = 0; i < number_waypoints; i++)
		{
			float z_size = nearest_wall.transform.GetComponent<Collider>().bounds.size.z;
			float z_gap = z_size/number_waypoints;
			//create waypoint position
			float x = nearest_wall.transform.position.x + offset_x_size;
			float y = nearest_wall.transform.position.y; 
			float z = nearest_wall.transform.position.z
				- z_size/2
				+ (i + 0.5f) * z_gap;
			allWallWaypoints[i] = new Vector3(x, y, z);
			CoverWaypoint cp = generatedMarkers[i].AddComponent<CoverWaypoint>();
			cp.heroCover = this;
			generatedMarkers[i] = Instantiate(waypointPrefab, allWallWaypoints[i], Quaternion.identity);
		}
	}
}
