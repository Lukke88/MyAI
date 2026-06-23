using UnityEngine;
using UnityEngine.UI;

public class CombatGUI : MonoBehaviour
{
    public Text combatText;
	public GameObject enemy, player_character, gun, gun_muzzle, muzzle_prefab, generated_muzzle, beam_prefab, generated_beam;
    public float visibleTime = 2.0f;

    private float timer;
	
	public bool IsShootingBack, IsShootingToPlayer;
	
	public Animator animator, player_animator;
	
	public GameObject enemy_tile;
public GameObject nearestWall;

public GameObject[] generatedTiles;

public bool tilesGenerated;

public float moveSpeed = 4.0f;

private Vector3 targetTilePosition;
private bool isMovingToTile;
	
	public void Start()
	{
		enemy = GameObject.Find(this.name);
		gun = enemy.transform.GetChild(0).GetChild(1).gameObject;
		gun_muzzle = gun.transform.GetChild(0).gameObject;
		muzzle_prefab = GameObject.Find("WFX_MF 4P RIFLE1");
		beam_prefab = GameObject.Find("lightBeam");
		
		enemy_tile = GameObject.FindWithTag("tiles");
		generatedTiles = new GameObject[6];
		
	}

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            combatText.text = "";
        }
		
		if(IsShootingBack==true)
			EnemyShootsBack();
		
		if(isMovingToTile == true)
{
	enemy.transform.position =
		Vector3.MoveTowards(
			enemy.transform.position,
			targetTilePosition,
			moveSpeed * Time.deltaTime);

	Vector3 lookDirection =
		targetTilePosition -
		enemy.transform.position;

	lookDirection.y = 0;

	if(lookDirection != Vector3.zero)
	{
		enemy.transform.rotation =
			Quaternion.LookRotation(lookDirection);
	}

	float distance =
		Vector3.Distance(
			enemy.transform.position,
			targetTilePosition);

	if(distance < 0.5f || IsShootingToPlayer)
	{
		isMovingToTile = false;

		animator.Play("hezbollah_terrorist_shoot");

		IsShootingBack = true;
	}
}
    }

    public void ShowHitMessage(float damage)
    {
        combatText.text =
            "OUCH !!! Enemy get hit and lost " +
            damage +
            " % of life";
			
			animator = enemy.transform.GetComponent<Animator>();
			animator.SetInteger("IsShooting",1);//muze strilet
			animator.Play("hezbollah_terrorist_shoot");//prehrava animaci strileni

        timer = visibleTime;
		IsShootingBack = true;
    }
	
	/*public void EnemyShootsBack()
	{
		//enemy turns to nearest player with tag "Player"
		
		//enemy starts shooting, creates one generated muzzle as child of gun muzzle, then destroy generated_muzzle after shot
		
		//creates generated_beam, shoots beam to player, destroys generated_beam on impact
		
		//plays player reaction "RibHit" or "RifleHit" animation at player figure
		
		
	}*/
	public void GenerateTilesNearWall()
{
	if(tilesGenerated == true)
	{
		return;
	}

	GameObject[] walls =
		GameObject.FindGameObjectsWithTag("Main_wall");

	float closestDistance = Mathf.Infinity;

	foreach(GameObject wall in walls)
	{
		float distance =
			Vector3.Distance(
				enemy.transform.position,
				wall.transform.position);

		if(distance < closestDistance)
		{
			closestDistance = distance;
			nearestWall = wall;
		}
	}

	if(nearestWall == null)
	{
		return;
	}

	generatedTiles[0] = enemy_tile;

	Renderer wallRenderer =
		nearestWall.GetComponent<Renderer>();

	float wallLength =
		wallRenderer.bounds.size.z;

	float spacing =
		wallLength / 5.0f;

	for(int i = 1; i < 6; i++)
	{
		Vector3 newPos =
			enemy_tile.transform.position;

		newPos.z += spacing * i;

		generatedTiles[i] =
			Instantiate(
				enemy_tile,
				newPos,
				enemy_tile.transform.rotation);
	}

	tilesGenerated = true;
}

public void MoveEnemyToRandomTile()
{
	if(generatedTiles.Length == 0)
	{
		return;
	}

	int randomTile =
		Random.Range(0, generatedTiles.Length);

	if(generatedTiles[randomTile] == null)
	{
		return;
	}

	targetTilePosition =
		generatedTiles[randomTile].transform.position;

	isMovingToTile = true;
}
	public void EnemyShootsBack()
{
	//enemy turns to nearest player with tag "Player"
	
	GameObject nearestPlayer = null;

	GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

	float closestDistance = Mathf.Infinity;

	foreach(GameObject p in players)
	{
		float distance =
			Vector3.Distance(enemy.transform.position,
			p.transform.position);

		if(distance < closestDistance)
		{
			closestDistance = distance;
			nearestPlayer = p;
		}
	}

	if(nearestPlayer == null)
	{
		return;
	}

	player_character = nearestPlayer;

	// enemy rotate to player

	Vector3 targetDirection =
		player_character.transform.position -
		enemy.transform.position;

	targetDirection.y = 0;

	if(targetDirection != Vector3.zero)
	{
		Quaternion targetRotation =
			Quaternion.LookRotation(targetDirection);

		enemy.transform.rotation =
			Quaternion.Slerp(
				enemy.transform.rotation,
				targetRotation,
				Time.deltaTime * 8.0f);
	}

	//enemy starts shooting, creates one generated muzzle as child of gun muzzle, then destroy generated_muzzle after shot

	if(generated_muzzle == null)
	{
		generated_muzzle =
			Instantiate(
				muzzle_prefab,
				gun_muzzle.transform.position,
				gun_muzzle.transform.rotation);

		generated_muzzle.transform.parent =
			gun_muzzle.transform;

		Destroy(generated_muzzle,0.15f);
	}

	//creates generated_beam, shoots beam to player, destroys generated_beam on impact

	if(generated_beam == null)
	{
		generated_beam =
			Instantiate(
				beam_prefab,
				gun_muzzle.transform.position,
				gun_muzzle.transform.rotation);

		Rigidbody rb =
			generated_beam.GetComponent<Rigidbody>();

		if(rb != null)
		{
			Vector3 shootDirection =
				(player_character.transform.position +
				Vector3.up * 1.2f) -
				gun_muzzle.transform.position;

	///		rb.linearVelocity =
	///			shootDirection.normalized * 300.0f;
		}

		Destroy(generated_beam,1.5f);

		//plays player reaction "RibHit" or "RifleHit" animation at player figure

		player_animator =
			player_character.GetComponent<Animator>();

		if(player_animator != null)
		{
			int randomHit = Random.Range(0,2);

			if(randomHit == 0)
			{
				player_animator.Play("RibHit");
			}
			else
			{
				player_animator.Play("RifleHit");
			}
		}

		IsShootingBack = false;

		animator.SetInteger("IsShooting",0);
		
		GenerateTilesNearWall();

		MoveEnemyToRandomTile();
	}
}
}