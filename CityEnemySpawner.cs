using UnityEngine;
using System.Collections.Generic;

public class CityEnemySpawner : MonoBehaviour
{
    public GameObject tank;
    public GameObject enemyPrefab;
    public float triggerDistance = 200f;

    private int enemyCounter = 0;
    private List<GameObject> houses = new List<GameObject>();

    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GenerationHouse");

        foreach (var o in objs)
            houses.Add(o);
    }

    void Update()
    {
        foreach (var house in houses)
        {
            float dist = Vector3.Distance(tank.transform.position, house.transform.position);

            if (dist < triggerDistance && !house.GetComponent<HouseTriggered>())
            {
                house.AddComponent<HouseTriggered>(); // označíme jako aktivované
                SpawnEnemies(house.transform.position);
            }
        }
    }

    void SpawnEnemies(Vector3 pos)
    {
        int count = Random.Range(2, 5);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawn = pos + Random.insideUnitSphere * 5f;
            spawn.y = pos.y;

            GameObject e = Instantiate(enemyPrefab, spawn, Quaternion.identity);
            e.name = "generated_terrorist_" + enemyCounter++;

            EnemyNavigator_new nav = e.GetComponent<EnemyNavigator_new>();
            nav.tank = tank;
            nav.IsRunningThroughCity = true;
        }
    }
}

// marker komponenta
public class HouseTriggered : MonoBehaviour {}

/*
spawn z arab_house
reaguje na tank (200m)
běží „městem“ (X/Z logika)
otáčí se po 90°
obchází budovy (PointA → PointB)
pak pokračuje
*/