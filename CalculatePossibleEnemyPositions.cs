using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculatePossibleEnemyPositions : MonoBehaviour
{
	public GameObject player_character, wayPointPrefab, enemy;
	public GameObject nearest_wall_to_player;
	public float nearest_wall_z_position;
	public GameObject[] allWalls;
	public Vector3[] corners;
	float offset = 100f;
	public Vector3 enemyWaypoint;//verify if enemy wayoint is in "forbidden" zone
	public float minimal_x_dist_from_player = 120.0f;
	public Dictionary<int, GameObject> selectedWalls = new Dictionary< int, GameObject>();
	int selected_walls_counter;
	Vector3 lastPlayerPosition, waypoint, playerPos;
	Vector3 farPoint1;
	Vector3 farPoint2;
	RaycastHit hit1;
	RaycastHit hit2;
	
	public float enemySpeed = 5f;
public float tickRate = 0.2f;

private float tickTimer;
private Vector3 currentEnemyTarget;
private bool hasEnemyTarget = false;
	//available position, actuated after each frame
	public List<Vector3> availableWaypoints = new List<Vector3>();
	//swich on/off showing each waipont position
	public bool visualizeRandomEnemyWaypoints;
	
	List<GameObject> candidateWalls;

	List<Vector3> forbiddenWaypoints;
    // Start is called before the first frame update
    void Start()
    {
        player_character = GameObject.Find("WomanSniper");//target object
		enemy = GameObject.Find("ArabWarrior");
    }

    // Update is called once per frame
    void Update()
    {
		if(player_character==null)
			player_character = GameObject.Find("WomanSniper");//target object, can be changed to vehicle
        if(Vector3.Distance(lastPlayerPosition,
                    player_character.transform.position) > 2f)
					{
					lastPlayerPosition = player_character.transform.position;

					FindAppPossibleWalls();
					TriangleDetectEdgesFunction();
					//zjistime, ktere Waypointy(Vector3) jsou v "zakazane zone"
					//pomoci farPoint1 a farPoint2 vyloucime ty, ktere lezi mezi temito dvema krajnimi Vectory
					CreateWaypointsAtWalls(farPoint1, farPoint2);

					}
					
					if(enemy!=null && availableWaypoints.Count>1)
					{
						MoveEnemyToRandomPoint();
					}
    }
	
	public void MoveEnemyToRandomPoint()
{
    tickTimer += Time.deltaTime;

    if(tickTimer < tickRate)
        return;

    tickTimer = 0f;


    // pokud nemá cíl, vyber waypoint
    if(!hasEnemyTarget)
    {
        int randomIndex = Random.Range(0, availableWaypoints.Count);

        currentEnemyTarget = availableWaypoints[randomIndex];

        hasEnemyTarget = true;
    }


    // pohyb po ticku
    enemy.transform.position = Vector3.MoveTowards(
        enemy.transform.position,
        currentEnemyTarget,
        enemySpeed * tickRate
    );


    // otočení nepřítele
    Vector3 direction = currentEnemyTarget - enemy.transform.position;

    if(direction != Vector3.zero)
    {
        enemy.transform.rotation =
            Quaternion.LookRotation(direction);
    }


    // dosažení cíle
    if(Vector3.Distance(enemy.transform.position,
                       currentEnemyTarget) < 1f)
    {
        hasEnemyTarget = false;
    }
}
	
	public void CreateWaypointsAtWalls(Vector3 edgePoint1, Vector3 edgePoint2)
{
    availableWaypoints.Clear();

    foreach(GameObject wall in selectedWalls.Values)
    {
        BoxCollider wallCollider = wall.GetComponent<BoxCollider>();

        Bounds b = wallCollider.bounds;

        // šest waypointů podél délky zdi
        for(int i = 0; i < 6; i++)
        {
            float t = i / 5f;

            // bod na zdi od levého kraje k pravému kraji
            Vector3 pointOnWall = Vector3.Lerp(
                new Vector3(b.min.x, b.center.y, b.center.z),
                new Vector3(b.max.x, b.center.y, b.center.z),
                t
            );


            // posun od zdi směrem ven
            waypoint = pointOnWall + wall.transform.forward * offset;


            // kontrola, jestli waypoint není v zakázané zóně
            if(!PointInsideTriangle(
                waypoint,
                playerPos,
                hit1.point,
                hit2.point))
            {
                availableWaypoints.Add(waypoint);
            }


            // vizualizace
            if(visualizeRandomEnemyWaypoints)
            {
                Debug.DrawRay(
                    waypoint,
                    Vector3.up * 5f,
                    Color.green
                );
            }
        }
    }
}

/*
ZEĎ

|--------------------------------|
0        1        2        3  4  5
*        *        *        *  *  *

          |
          | offset 100
          v

        waypointy
*/
	

	
	
	
	public void TriangleDetectEdgesFunction()
	{
		//finds edge points of wall nearest to player
		BoxCollider wallCollider = nearest_wall_to_player.GetComponent<BoxCollider>();

corners = new Vector3[4];

corners[0] = wallCollider.bounds.min;

corners[1] = new Vector3(
    wallCollider.bounds.min.x,
    wallCollider.bounds.center.y,
    wallCollider.bounds.max.z
);

corners[2] = wallCollider.bounds.max;

corners[3] = new Vector3(
    wallCollider.bounds.max.x,
    wallCollider.bounds.center.y,
    wallCollider.bounds.min.z
);

Vector3 center = wallCollider.bounds.center;
Vector3 extents = wallCollider.bounds.extents;

Vector3 leftPoint = new Vector3(
    center.x - extents.x,
    center.y,
    center.z
);

Vector3 rightPoint = new Vector3(
    center.x + extents.x,
    center.y,
    center.z
);
//calculate distance
playerPos = player_character.transform.position;

float min1 = Mathf.Infinity;
float min2 = Mathf.Infinity;

Vector3 edge1 = Vector3.zero;
Vector3 edge2 = Vector3.zero;


foreach(Vector3 c in corners)
{
    float dist = Vector3.Distance(playerPos,c);

    if(dist < min1)
    {
        min2=min1;
        edge2=edge1;

        min1=dist;
        edge1=c;
    }
    else if(dist < min2)
    {
        min2=dist;
        edge2=c;
    }
	
}
//Teď máte dvě hrany z pohledu hráče.
//Vytvoření paprsků

//Směr od hráče k hraně:
Vector3 dir1 = (edge1 - playerPos).normalized;
Vector3 dir2 = (edge2 - playerPos).normalized;
//Prodloužení:

farPoint1 = playerPos + dir1 * 500f;
farPoint2 = playerPos + dir2 * 500f;
//Draw line
Debug.DrawLine(
playerPos,
farPoint1,
Color.red
);
//prunik hrana 1
Debug.DrawLine(
playerPos,
farPoint2,
Color.red
);

//RaycastHit hit;

if(Physics.Raycast(
    playerPos,
    dir1,
    out hit1,
    500f))
{
    Debug.DrawLine(
        playerPos,
        hit1.point,
        Color.red
    );
}
//prunik hrana 2
//RaycastHit hit2;

if(Physics.Raycast(
    playerPos,
    dir2,
    out hit2,
    500f))
{
    Debug.DrawLine(
        playerPos,
        hit2.point,
        Color.red
    );
}

if(PointInsideTriangle(
    enemyWaypoint,
    playerPos,
    hit1.point,
    hit2.point
))
{
   // continue;
}
else if(!PointInsideTriangle(
    enemyWaypoint,
    playerPos,
    hit1.point,
    hit2.point
))
{
    availableWaypoints.Add(enemyWaypoint);
}
/*
//univrsal variety
Vector3[] corners = new Vector3[4];

corners[0] = wallCollider.bounds.min;
corners[1] = new Vector3(
    wallCollider.bounds.min.x,
    0,
    wallCollider.bounds.max.z
);

corners[2] = wallCollider.bounds.max;

corners[3] = new Vector3(
    wallCollider.bounds.max.x,
    0,
    wallCollider.bounds.min.z
);
*/
	}
	
	public bool PointInsideTriangle(Vector3 point, Vector3 A, Vector3 B, Vector3 C)
{
    Vector3 v0 = C - A;
    Vector3 v1 = B - A;
    Vector3 v2 = point - A;

    float dot00 = Vector3.Dot(v0, v0);
    float dot01 = Vector3.Dot(v0, v1);
    float dot02 = Vector3.Dot(v0, v2);
    float dot11 = Vector3.Dot(v1, v1);
    float dot12 = Vector3.Dot(v1, v2);

    float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);

    float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
    float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

    return (u >= 0) && (v >= 0) && (u + v <= 1);
}
	
	public void FindAppPossibleWalls()
	{
		selectedWalls.Clear();
		selected_walls_counter = 0;
		allWalls = GameObject.FindGameObjectsWithTag("Wall");
		if(selected_walls_counter<allWalls.Length)
		{

		foreach(GameObject go in allWalls)
		{
			if(Mathf.Abs(go.transform.position.x - player_character.transform.position.x)>=minimal_x_dist_from_player)//if wall is behind this borderline, then is acceptable
			{
				
				selectedWalls.Add(selected_walls_counter, go);//gets all possible walls
				selected_walls_counter++;
			}
		}
		}
	}
}
