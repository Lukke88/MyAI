using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab, tank, missile, selected_house;
    public GameObject[] houses, cloned_missiles;
    public WallSlots wall;

public TMP_Text missionText;

    public int maxEnemies = 10;
   public  List<GameObject> activeEnemies = new List<GameObject>();
	void Start()
	{
		houses = GameObject.FindGameObjectsWithTag("GenerationHouse");
		tank = GameObject.Find("Merkava Mk_lowpoly");
		
		string[] missionBriefings =
{
	"Don't move or I'll whistle one behind your ears.",//"Něcum nebo ti hvizdnu jednu za uši."
	"Move one more inch and you'll regret it.",
	"A sniper is hiding somewhere in the Syrian town. Find him before he drops you like a deer in an open field.",
	"An enemy sniper is concealed somewhere in the city. Locate and eliminate him before he picks you off like a deer in the open.",
	"Intelligence reports an enemy sniper operating somewhere in the city. Track him down and neutralize him before he turns you into his next target.",
	"Somewhere in this Syrian town, a sniper is hiding. Find him before he shoots you down like a deer grazing in a field.",
	"An enemy sniper is concealed somewhere in the city. Locate and eliminate him before he picks you off like a deer in the open.",
	"Somebody just started spreading jihad through 7.62mm bullets. Find him and eliminate.",
	"Intelligence reports an enemy sniper in the area. Intelligence also suggests you should avoid getting shot.",
"An enemy sniper is concealed somewhere in the city. Locate and eliminate him before he picks you off like a deer in the open.",
	"A sniper is hiding somewhere in the Syrian town. Find him before he drops you like a deer in an open field.",
"Somewhere in this Syrian town, a sniper is hiding. Find him before he shoots you down like a deer grazing in a field.",
    "Keep your head down. A sniper is operating in the area.",
	"Intelligence says the city is safe. Intelligence was last seen running in the opposite direction.",

"Watch the rooftops. There's a sniper out there.",

"Stay in cover. One sniper can stop an entire squad.",

"Move from cover to cover. Do not give him a clean shot.",

"Find the shooter before he finds you.",
};

missionText.text =
    missionBriefings[
        Random.Range(0, missionBriefings.Length)
    ];
	}
	public void LateUpdate()
	{
		cloned_missiles = GameObject.FindGameObjectsWithTag("EnemyMissile");
		if(tank!=null)
		foreach(GameObject go in cloned_missiles)
		{
			if(Vector3.Distance(tank.transform.position, go.transform.position)<=70.0f)
			{
				TrySpawnEnemy(go);//creates enemy
				
			}
		}
	}
	int max_spawned_enemies = 3;
	public int currentEnemiesCount;
	public void Update()
	{
		if(selected_house!=null)
		{
			if(currentEnemiesCount<maxEnemies)
		{
		Vector3 selected_position = new Vector3(selected_house.transform.position.x, tank.transform.position.y, selected_house.transform.position.z);
        GameObject enemy = Instantiate(enemyPrefab, selected_position, Quaternion.identity);
		currentEnemiesCount++;
		enemy.name = "Generated_enemy_" + currentEnemiesCount;
		
        EnemyUnit unit = enemy.GetComponent<EnemyUnit>();
        //unit.Init(slot, slotIndex, wall, this);

        activeEnemies.Add(enemy);
		
		selected_house = null;
		}
		}
	}
	
    public void TrySpawnEnemy(GameObject last_missile)
    {
        if(activeEnemies.Count >= maxEnemies) return;

		if(selected_house==null)
        selected_house = houses[Random.Range(0, houses.Length)];
		else
		{
        int slotIndex;
        //Transform slot = wall.GetFreeSlot(out slotIndex);

        //if(slot == null) return;
		if(max_spawned_enemies<currentEnemiesCount)
		{
        GameObject enemy = Instantiate(enemyPrefab, selected_house.transform.position, Quaternion.identity);
		currentEnemiesCount++;
		enemy.name = "Generated_enemy_" + currentEnemiesCount;
		
        EnemyUnit unit = enemy.GetComponent<EnemyUnit>();
        //unit.Init(slot, slotIndex, wall, this);

        activeEnemies.Add(enemy);
		Destroy(last_missile);//destroys missile
		selected_house = null;
		}
		}
    }
	
	public void TrySpawnEnemiesFromHouses(int minCount, int maxCount)
{
    if (houses == null || houses.Length == 0) return;

    int spawnCount = Random.Range(minCount, maxCount + 1);

    for (int i = 0; i < spawnCount; i++)
    {
        if (activeEnemies.Count >= maxEnemies) return;

        GameObject house = houses[Random.Range(0, houses.Length)];

        int slotIndex;
        Transform slot = wall.GetFreeSlot(out slotIndex);

        if (slot == null) return;

        GameObject enemy = Instantiate(enemyPrefab, house.transform.position, Quaternion.identity);

        EnemyUnit unit = enemy.GetComponent<EnemyUnit>();
        unit.Init(slot, slotIndex, wall, this);

        activeEnemies.Add(enemy);
    }
}

    public void OnEnemyDeath(GameObject enemy, int slotIndex)
    {
        activeEnemies.Remove(enemy);
        wall.FreeSlot(slotIndex);

     //   TrySpawnEnemy(); // doplnění
    }
}