using UnityEngine;
using System.Collections.Generic;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public string weaponsFolder = "Guns/Models";
    public int numberOfWeapons = 200;
    public float minDistance = 200f;
    public float maxDistance = 300f;
    public float groundY = 0f; // výška nad zemí, pokud nemá Terrain
    public LayerMask avoidLayers; // vrstvy, na kterých se nesmí zbraň generovat

    private List<GameObject> weaponPrefabs = new List<GameObject>();

    void Start()
    {
        LoadWeaponPrefabs();
        SpawnWeaponsRandomly();
    }

    void LoadWeaponPrefabs()
    {
        // Načti všechny prefaby z Resources/Guns/Models/
        GameObject[] prefabs = Resources.LoadAll<GameObject>(weaponsFolder);

        foreach (GameObject prefab in prefabs)
        {
            weaponPrefabs.Add(prefab);
        }

        if (weaponPrefabs.Count == 0)
            Debug.LogWarning("WeaponSpawner: Žádné prefaby nebyly nalezeny ve složce " + weaponsFolder);
    }

    void SpawnWeaponsRandomly()
{
    int attempts = 0;
    int spawned = 0;
    List<Vector3> spawnedPositions = new List<Vector3>(); // pozice již umístěných zbraní
    float minDistanceBetweenWeapons = 10f; // minimální vzdálenost mezi zbraněmi

    while (spawned < numberOfWeapons && attempts < numberOfWeapons * 20)
    {
        attempts++;

        float x = Random.Range(-maxDistance, maxDistance);
        float z = Random.Range(-maxDistance, maxDistance);
        Vector3 spawnPos = new Vector3(x, groundY + 2f, z);

        // Kontrola kolize s objekty
        if (Physics.CheckSphere(spawnPos, 1f, avoidLayers))
            continue;

        // Kontrola vzdálenosti od ostatních zbraní
        bool tooClose = false;
        foreach (Vector3 pos in spawnedPositions)
        {
            if (Vector3.Distance(spawnPos, pos) < minDistanceBetweenWeapons)
            {
                tooClose = true;
                break;
            }
        }
        if (tooClose) continue;

        // Vyber náhodnou zbraň
        GameObject prefab = weaponPrefabs[Random.Range(0, weaponPrefabs.Count)];
        GameObject weaponInstance = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f,360f), 0));

        // Přidání WeaponItem a ItemDescription pokud chybí
        WeaponItem weaponItem = weaponInstance.GetComponent<WeaponItem>();
        if (weaponItem == null)
        {
            weaponItem = weaponInstance.AddComponent<WeaponItem>();
            if (weaponItem.itemDescription == null)
            {
                ItemDescription desc = ScriptableObject.CreateInstance<ItemDescription>();
                desc.itemName = prefab.name;
                desc.description = "Generic weapon description."; // později můžeš doplnit z Wikipedie
                desc.maxAmmo = 30;
                desc.icon = null;
                weaponItem.itemDescription = desc;
            }
        }

        spawnedPositions.Add(spawnPos);
        spawned++;
    }

    Debug.Log("WeaponSpawner: Naskládáno " + spawned + " zbraní po mapě.");
}

}
