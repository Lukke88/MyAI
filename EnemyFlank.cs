using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlank : MonoBehaviour
{
    public enum State { Idle, Flanking, Shooting }
    public State currentState = State.Idle;

    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    public Transform player;
    public GameObject wallContainer, enemy; // Wall(4)
    private Transform targetTile;

    private Animator anim;
	public bool IsDead;
    void Start()
    {
		enemy = GameObject.Find(this.name);
        if (wallContainer == null)
        {
            wallContainer = GameObject.Find("Wall (4)");
        }

        anim = GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogWarning("Animator component not found on Enemy!");
        }
    }

    void Update()
    {
        if(player != null)
        {
            UpdateFlankAndAttack(player);
        }
    }

    public void UpdateFlankAndAttack(Transform playerTransform)
    {
        player = playerTransform;

        switch (currentState)
        {
            case State.Idle:
                // Idle animace
                if (anim != null) anim.SetInteger("State", 0);

                // Pokud je enemy „označen“ (např. ve vaší logice cursor) -> začne flanking
                RaycastHit hit;
                Vector3 dir = (player.position - transform.position).normalized;
                if (Physics.Raycast(transform.position, dir, out hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        FindTileBehindWall();
                        currentState = State.Flanking;
                    }
                }
                break;

            case State.Flanking:
                if (anim != null) anim.SetInteger("State", 2); // běžící/flank animace

                if (targetTile != null)
                {
                    // Pohyb směrem k tile
					if(IsDead==false)
					{
                    MoveToFlankingPosition();

                    // Pokud jsme dostatečně blízko tile -> střelba
                    if (Vector3.Distance(transform.position, targetTile.position) < 0.5f)
                    {
                        currentState = State.Shooting;
                    }
                }}
                break;

            case State.Shooting:
                if (anim != null) anim.SetInteger("State", 1); // střelba animace

                // Sleduj hráče během střelby
                Quaternion lookRotation = Quaternion.LookRotation(player.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);

                // Můžete zde volat metodu střelby
                ShootAtPlayer();
                break;
        }
    }
	public Vector3 PointA, PointB, PointC;
	public bool passedA, passedB, passedC;
	
	//public float moveSpeed = 25f;
   // public float rotateSpeed = 25f;
	public void MoveToFlankingPosition()
	{
					float z_offset_from_tile = 10.0f;//jak daleko za dlazdici se bude enemy pohybovat
					float x_offset = 5.0f;
					float enemy_height = enemy.transform.position.y;
					if(PointA==Vector3.zero || PointB==Vector3.zero || PointC==Vector3.zero)
					{
						//bod nad pozici hrace
						PointA = new Vector3(enemy.transform.position.x, enemy_height, enemy.transform.position.z + z_offset_from_tile);
						//bod nad cilovou pozici, lehce doleva
						if(enemy.transform.position.x<targetTile.transform.position.x)
						{
						PointB = new Vector3(targetTile.transform.position.x - x_offset, enemy_height, enemy.transform.position.z + z_offset_from_tile);
							PointC = new Vector3(targetTile.transform.position.x, enemy_height, enemy.transform.position.z + z_offset_from_tile - x_offset);
						}
						else
							if(enemy.transform.position.x>=targetTile.transform.position.x)
						{
						PointB = new Vector3(targetTile.transform.position.x + x_offset, enemy_height, enemy.transform.position.z + z_offset_from_tile);
							PointC = new Vector3(targetTile.transform.position.x, enemy_height, enemy.transform.position.z + z_offset_from_tile - x_offset);
						}
					}
					else if(PointA!=Vector3.zero && PointB!=Vector3.zero && PointC!=Vector3.zero)
					{
						MoveAlongPath();
					}
					
                    // Rotace směrem k hráči
                    Quaternion lookRot = Quaternion.LookRotation(player.position - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);
					
					
	}
	
	void MoveAlongPath()
    {
        Vector3 target = Vector3.zero;

        if (!passedA)
            target = PointA;
        else if (!passedB)
            target = PointB;
        else if (!passedC)
            target = PointC;
        else if (targetTile != null)
            target = targetTile.position;

        // Pohyb k cíli
        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target, moveSpeed * Time.deltaTime);

        // Kontrola, zda jsme dosáhli bodu
        if (!passedA && Vector3.Distance(enemy.transform.position, PointA) < 0.1f) passedA = true;
        else if (!passedB && Vector3.Distance(enemy.transform.position, PointB) < 0.1f) passedB = true;
        else if (!passedC && Vector3.Distance(enemy.transform.position, PointC) < 0.1f) passedC = true;
    }

    // Vykreslení cesty v editoru
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        if (PointA != Vector3.zero)
        {
            Gizmos.DrawSphere(PointA, 0.3f);
            if (PointB != Vector3.zero) Gizmos.DrawLine(PointA, PointB);
        }

        if (PointB != Vector3.zero)
        {
            Gizmos.DrawSphere(PointB, 0.3f);
            if (PointC != Vector3.zero) Gizmos.DrawLine(PointB, PointC);
        }

        if (PointC != Vector3.zero)
        {
            Gizmos.DrawSphere(PointC, 0.3f);
            if (targetTile != null) Gizmos.DrawLine(PointC, targetTile.position);
        }

        if (targetTile != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f); // oranžová pro targetTile
            Gizmos.DrawSphere(targetTile.position, 0.4f);
        }
    }
	public int rand_number_tile;
    void FindTileBehindWall()
    {
        if (wallContainer == null) return;//Wall (4)

        Transform[] tiles = new Transform[wallContainer.transform.childCount];
        for (int i = 0; i < wallContainer.transform.childCount; i++)
            tiles[i] = wallContainer.transform.GetChild(i);

        float closestDistance = Mathf.Infinity;
        Transform chosenTile = null;

        foreach (Transform tile in tiles)
        {
			if(rand_number_tile==0) //nahodne cislo
			rand_number_tile = Random.Range(1,4);
            Vector3 toTile = tile.position - transform.position;
            Vector3 toPlayer = player.position - transform.position;
            float angle = Vector3.Angle(toPlayer, toTile);

            // Vyber tile z nahodneho cisla
            if (rand_number_tile>0 && tile.name.Contains("_" + rand_number_tile.ToString()))
            {
               
                {
                    chosenTile = tile;
                }
            }
        }

        targetTile = chosenTile;
    }

    void ShootAtPlayer()
    {
        // Placeholder pro logiku střelby
        // Např. Instantiate(bullet, muzzle.position, Quaternion.LookRotation(player.position - transform.position));
    }
}