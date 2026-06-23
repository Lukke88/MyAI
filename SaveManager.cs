using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    public GameObject player;

    public List<GameObject> enemies =
        new List<GameObject>();

    public TMP_Dropdown dropdown;

    public GameObject LoadGamesPanel;

    void Start()
    {
        RefreshSaveDropdown();
    }

    public void SaveGame(string saveName = "save1")
    {
        SaveAllCharactersData data = new SaveAllCharactersData();

        data.playerPosX = player.transform.position.x;
        data.playerPosY = player.transform.position.y;
        data.playerPosZ = player.transform.position.z;

        foreach (GameObject enemy in enemies)
        {
            EnemySaveData e = new EnemySaveData();

            e.posX = enemy.transform.position.x;
            e.posY = enemy.transform.position.y;
            e.posZ = enemy.transform.position.z;

            data.enemies.Add(e);
        }

        string json = JsonUtility.ToJson(data, true);

        string path =
            Path.Combine(
                Application.persistentDataPath,
                saveName + ".json");

        File.WriteAllText(path, json);

        RefreshSaveDropdown();
    }

    public void LoadGame(string saveName)
    {
        string path =
            Path.Combine(
                Application.persistentDataPath,
                saveName + ".json");

        if (!File.Exists(path))
            return;

        string json =
            File.ReadAllText(path);

        SaveAllCharactersData data =
            JsonUtility.FromJson<SaveAllCharactersData>(json);

        player.transform.position =
            new Vector3(
                data.playerPosX,
                data.playerPosY,
                data.playerPosZ);
    }

    public List<string> GetAllSaveFiles()
    {
        string[] files =
            Directory.GetFiles(
                Application.persistentDataPath,
                "*.json");

        List<string> saves =
            new List<string>();

        foreach (string file in files)
        {
            saves.Add(
                Path.GetFileNameWithoutExtension(file));
        }

        return saves;
    }

    void RefreshSaveDropdown()
    {
        dropdown.ClearOptions();

        List<string> saves =
            GetAllSaveFiles();

        dropdown.AddOptions(saves);
    }

    public void OpenLoadPanel()
    {
        LoadGamesPanel.SetActive(true);

        RefreshSaveDropdown();
    }

    public void LoadSelectedSave()
    {
        if (dropdown.options.Count == 0)
            return;

        string selectedSave =
            dropdown.options[
                dropdown.value].text;

        LoadGame(selectedSave);
    }
}