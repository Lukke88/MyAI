using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    public HeroController hero; // hráč
    public string saveFileName = "savegame.json";

    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        // --- Uložíme hráče ---
        PlayerData playerData = new PlayerData();
        playerData.position = hero.transform.position;
        playerData.rotation = hero.transform.rotation;

        // inventory
        playerData.inventoryItems = new List<string>();
        foreach (var weapon in hero.inventory)
        {
          //  playerData.inventoryItems.Add(weapon.name); // nebo weapon.ID pokud WeaponData má ID
        }

        saveData.player = playerData;

        // --- Uložíme ostatní objekty ve scéně (interactable/important) ---
        List<GameObjectData> objectsData = new List<GameObjectData>();
        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            // filtrujeme jen tagy nebo vrstvy, které chceme ukládat
            if (obj.CompareTag("Selectable") || obj.CompareTag("Enemy"))
            {
                GameObjectData data = new GameObjectData();
                data.name = obj.name;
                data.tag = obj.tag;
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation;
                data.active = obj.activeSelf;

                objectsData.Add(data);
            }
        }
        saveData.sceneObjects = objectsData;

        // --- Serializace do JSON ---
        string json = JsonUtility.ToJson(saveData, true);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(path, json);

        Debug.Log("Game saved to: " + path);
    }
	
	public void LoadGame()
{
    string path = Path.Combine(Application.persistentDataPath, saveFileName);
    if (!File.Exists(path))
    {
        Debug.LogWarning("Save file not found!");
        return;
    }

    string json = File.ReadAllText(path);
    SaveData saveData = JsonUtility.FromJson<SaveData>(json);

    // --- Načtení hráče ---
    hero.transform.position = saveData.player.position;
    hero.transform.rotation = saveData.player.rotation;

    // inventory
    hero.inventory.Clear();
    foreach (string itemName in saveData.player.inventoryItems)
    {
     /*   WeaponData weapon = Resources.Load<WeaponData>("Weapons/" + itemName);
        if (weapon != null)
            hero.AddItem(weapon);*/
    }

    // --- Načtení objektů ve scéně ---
    foreach (var objData in saveData.sceneObjects)
    {
        GameObject obj = GameObject.Find(objData.name);
        if (obj != null)
        {
            obj.transform.position = objData.position;
            obj.transform.rotation = objData.rotation;
            obj.SetActive(objData.active);
        }
    }

    Debug.Log("Game loaded!");
}

}
// Stav jednoho herního objektu
[Serializable]
public class GameObjectData
{
    public string name;
    public string tag;
    public Vector3 position;
    public Quaternion rotation;
    public bool active;
}

// Stav hráče
[Serializable]
public class PlayerData
{
    public Vector3 position;
    public Quaternion rotation;
    public List<string> inventoryItems; // uložíme jen názvy itemů nebo WeaponData ID
}

// Celý save
[Serializable]
public class SaveData
{
    public PlayerData player;
    public List<GameObjectData> sceneObjects;
}