using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject TalibanWarrior;

    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int MaxEnemyCounter = 5;

    private int CurrentEnemyCounter = 0;
    private int DeadEnemyCounter = 0;
    private int nameCounter = 0;

    [Header("UI")]
    public TMP_Text score_text;
    public TMP_Text enemy_counter_text;
    public TMP_Text dead_enemies_text;

    void Start()
    {
        AddCollisionBoxesToGenerationHouses();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (CurrentEnemyCounter < MaxEnemyCounter)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // ================= SPAWN =================

    void SpawnEnemy()
    {
        GameObject[] houses = GameObject.FindGameObjectsWithTag("GenerationHouse");
        if (houses.Length == 0) return;

        GameObject randomHouse = houses[Random.Range(0, houses.Length)];

        GameObject enemy = Instantiate(
            TalibanWarrior,
            randomHouse.transform.position,
            Quaternion.identity
        );

        nameCounter++;
        enemy.name = "generated_" + TalibanWarrior.name + "_" + nameCounter;

        CurrentEnemyCounter++;

        UpdateUI();

        StartCoroutine(PlaceEnemyBehindWall(enemy));
    }

    // ================= PLACEMENT =================

    IEnumerator PlaceEnemyBehindWall(GameObject enemy)
    {
        yield return new WaitForSeconds(0.2f);

        GameObject player = FindNearestPlayer(enemy.transform.position);
        if (player == null) yield break;

        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

        GameObject nearestWall = walls
            .OrderBy(w => Vector3.Distance(player.transform.position, w.transform.position))
            .FirstOrDefault();

        if (nearestWall == null) yield break;

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tiles");

        if (tiles.Length == 0) yield break;

        GameObject randomTile = tiles[Random.Range(0, tiles.Length)];

        enemy.transform.position = randomTile.transform.position;

        enemy.transform.LookAt(player.transform);

        BaseTalibEnemyAI ai = enemy.GetComponent<BaseTalibEnemyAI>();
        if (ai != null)
        {
            ai.enabled = true;
        }

        EnemyDeathWatcher watcher = enemy.AddComponent<EnemyDeathWatcher>();
        watcher.manager = this;
    }

    // ================= COLLISION BOX ADD =================

    void AddCollisionBoxesToGenerationHouses()
    {
        GameObject[] houses = GameObject.FindGameObjectsWithTag("GenerationHouse");

        foreach (GameObject house in houses)
        {
            if (house.GetComponent<BoxCollider>() == null)
            {
                house.AddComponent<BoxCollider>();
            }
        }
    }

    // ================= PLAYER =================

    GameObject FindNearestPlayer(Vector3 position)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p;
            }
        }

        return nearest;
    }

    // ================= ENEMY DEATH CALLBACK =================

    public void OnEnemyKilled()
    {
        CurrentEnemyCounter--;
        DeadEnemyCounter++;

        UpdateUI();
    }

    // ================= UI =================

    void UpdateUI()
    {
        if (score_text != null)
            score_text.text = "Score: " + DeadEnemyCounter * 100;

        if (enemy_counter_text != null)
            enemy_counter_text.text = "Enemies: " + CurrentEnemyCounter + "/" + MaxEnemyCounter;

        if (dead_enemies_text != null)
            dead_enemies_text.text = "Dead: " + DeadEnemyCounter;
    }
}
