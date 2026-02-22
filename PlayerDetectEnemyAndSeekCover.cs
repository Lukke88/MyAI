using System.Collections;
using UnityEngine;

public class PlayerDetectEnemyAndSeekCover : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;

    public float detectDistance = 50f;
    public float rotationTime = 1.5f;
    public float movementSpeed = 5f;

    public Vector3 hidePointA;
    public Vector3 hidePointB;
	public Vector3 hidePointC;
	public Vector3 PlayerStartingPosition;

    bool isRotatingToEnemy;
    bool isSeekingCover;
	void Start()
	{
		player = GameObject.Find(this.name);
		PlayerStartingPosition = player.transform.position;
		enemy = GameObject.Find("Enemy1");
	}
    void Update()
    {
        if(player == null || enemy == null) return;

        float dist = Vector3.Distance(player.transform.position, enemy.transform.position);

        // -------------------------
        // Detection phase
        // -------------------------
        if(dist < detectDistance && !IsWallBetween())
        {
            if(!isRotatingToEnemy && !isSeekingCover)
                StartCoroutine(RotateToEnemyThenSeekCover());
        }

        // -------------------------
        // Seek cover movement
        // -------------------------
        if(isSeekingCover)
        {
            MoveToCover();
        }
    }

    // -------------------------
    // Raycast check for wall
    // -------------------------
    bool IsWallBetween()
    {
        Vector3 dir = enemy.transform.position - player.transform.position;
        Ray ray = new Ray(player.transform.position, dir.normalized);

        if(Physics.Raycast(ray, out RaycastHit hit, detectDistance))
        {
            return hit.collider.gameObject != enemy;
        }

        return false;
    }

    // -------------------------
    // Rotation coroutine
    // -------------------------
    IEnumerator RotateToEnemyThenSeekCover()
    {
        isRotatingToEnemy = true;

        float t = 0;
        Quaternion startRot = player.transform.rotation;

        Vector3 dir = (enemy.transform.position - player.transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);
		if(dir != Vector3.zero)
		{
			player.transform.rotation = 
				Quaternion.LookRotation(dir) * Quaternion.Euler(0,180f,0);
		}

        while(t < rotationTime)
        {
            t += Time.deltaTime;

            player.transform.rotation =
                Quaternion.Slerp(startRot, targetRot, t / rotationTime);

            yield return null;
        }

        FindCoverPoints();
        isSeekingCover = true;
        isRotatingToEnemy = false;
    }

    // -------------------------
    // Cover search logic
    // -------------------------
    void FindCoverPoints()
{
    Vector3 playerPos = player.transform.position;

    Collider[] walls = Physics.OverlapSphere(playerPos, 60f);

    GameObject bestWall = null;
    float bestDistance = float.MaxValue;

    // -------------------------
    // Find wall behind player (Z smaller than starting position)
    // -------------------------
    foreach(Collider col in walls)
    {
        if(!col.CompareTag("Wall")) continue;

        float wallZ = col.transform.position.z;

        if(wallZ < PlayerStartingPosition.z)
        {
            float dist = Vector3.Distance(playerPos, col.transform.position);

            if(dist < bestDistance)
            {
                bestDistance = dist;
                bestWall = col.gameObject;
            }
        }
    }

    if(bestWall == null) return;

    Vector3 wallPos = bestWall.transform.position;
    Collider wallCollider = bestWall.GetComponent<Collider>();

    float sizeX = wallCollider.bounds.size.x;
    float sizeZ = wallCollider.bounds.size.z;

    float z_offset = 1.2f;

    // -------------------------
    // HidePointA (closest to player, on wall edge)
    // -------------------------
    hidePointA = new Vector3(
        wallPos.x + sizeX/2,
        playerPos.y,
        wallPos.z + z_offset * sizeZ
    );

    // -------------------------
    // HidePointB (same X, opposite Z offset)
    // -------------------------
    hidePointB = new Vector3(
        hidePointA.x + sizeX/2,
        playerPos.y,
        wallPos.z - z_offset * sizeZ
    );

    // -------------------------
    // HidePointC (behind wall)
    // -------------------------
    hidePointC = new Vector3(
        wallPos.x,
        playerPos.y,
        wallPos.z - sizeZ * 1.5f
    );

    Debug.DrawLine(playerPos, hidePointA, Color.magenta, 3f);
    Debug.DrawLine(hidePointA, hidePointB, Color.magenta, 3f);
    Debug.DrawLine(hidePointB, hidePointC, Color.magenta, 3f);
}

    // -------------------------
    // Cover movement
    // -------------------------
    void MoveToCover()
    {
        Vector3 playerPos = player.transform.position;

        float distA = Vector3.Distance(playerPos, hidePointA);
        float distB = Vector3.Distance(playerPos, hidePointB);

        Vector3 target = distA < distB ? hidePointA : hidePointB;

        player.transform.position =
            Vector3.MoveTowards(
                player.transform.position,
                target,
                movementSpeed * Time.deltaTime);

        Debug.DrawLine(player.transform.position, target, Color.magenta);

        if(Vector3.Distance(playerPos, target) < 0.5f)
        {
            isSeekingCover = false;
        }
    }
}
