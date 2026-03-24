using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameButtons : MonoBehaviour
{
    // ── MAIN MENU ─────────────────────────
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject aboutPanel;
    public GameObject loadingPanel;

    public Slider loadingSlider;
    public Text loadingText;

    // ── GAME UI ───────────────────────────
    [Header("Game UI")]
    public GameObject minimapPanel;
    public GameObject formationsPanel;
    public GameObject infoBoxPanel;
    public GameObject weaponPanel;
    public GameObject scorePanel;
    public GameObject coordsPanel;

    public string gameSceneName = "GameScene";

    // ── SAVE UI ───────────────────────────
    [Header("Save UI")]
    public GameObject saveGamePanel;
    public TMP_InputField inputFieldSave;
    public Button buttonSave;
    public TextMeshProUGUI savedGamesText;

    // ── LOAD UI ───────────────────────────
    [Header("Load UI")]
    public GameObject loadGamePanel;
    public Transform loadButtonsContainer;
    public GameObject loadButtonPrefab;
    public Button buttonBackLoad;

    private List<GameObject> spawnedButtons = new List<GameObject>();

    // ─────────────────────────────────────
    void Start()
    {
        ShowMainMenu();

        if (buttonSave != null)
            buttonSave.onClick.AddListener(OnSaveClicked);

        if (buttonBackLoad != null)
            buttonBackLoad.onClick.AddListener(CloseLoadPanel);
    }

    // ── MENU ─────────────────────────────
    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);
        aboutPanel.SetActive(false);
        loadingPanel.SetActive(false);
        saveGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);

        DisableGameUI();
    }

    public void Options() => SwitchPanel(optionsPanel);
    public void About() => SwitchPanel(aboutPanel);

    public void BackToMenu() => ShowMainMenu();

    void SwitchPanel(GameObject target)
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        aboutPanel.SetActive(false);

        target.SetActive(true);
    }

    public void Exit() => Application.Quit();

    // ── GAME UI ──────────────────────────
    void EnableGameUI(bool state)
    {
        minimapPanel.SetActive(state);
        formationsPanel.SetActive(state);
        infoBoxPanel.SetActive(state);
        weaponPanel.SetActive(state);
        scorePanel.SetActive(state);
        coordsPanel.SetActive(state);
    }

    void DisableGameUI() => EnableGameUI(false);

    // ── NEW GAME ─────────────────────────
    public void NewGame()
    {
        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAsync(null));
    }

   IEnumerator LoadSceneAsync(string saveToLoad)
{
    AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);

    while (!op.isDone)
    {
        float progress = Mathf.Clamp01(op.progress / 0.9f);
        loadingSlider.value = progress;
        loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
        yield return null;
    }

    loadingPanel.SetActive(false);
    EnableGameUI(true);

    if (!string.IsNullOrEmpty(saveToLoad))
        ApplyLoadedData(saveToLoad);

    // --- Zobrazíme MissionTitle a MissionSubtitle ---
    ShowMissionTitle();
}

void ShowMissionTitle()
{
    GameObject missionTitleGO = GameObject.Find("MissionTitle");
    if (missionTitleGO != null)
    {
        missionTitleGO.SetActive(true); // aktivujeme text
        StartCoroutine(HideMissionTitleAfterSeconds(3f)); // skryjeme po 3 sekundách
    }
}

IEnumerator HideMissionTitleAfterSeconds(float seconds)
{
    yield return new WaitForSeconds(seconds);

    GameObject missionTitleGO = GameObject.Find("MissionTitle");
    if (missionTitleGO != null)
    {
        missionTitleGO.SetActive(false);
    }
}

    // ── SAVE ─────────────────────────────
    public void OpenSavePanel()
    {
        saveGamePanel.SetActive(true);
        RefreshSaveList();
    }

    void OnSaveClicked()
    {
        string name = string.IsNullOrWhiteSpace(inputFieldSave.text)
            ? "save1"
            : Sanitize(inputFieldSave.text);

        SaveGame(name);
        RefreshSaveList();
        inputFieldSave.text = "";
    }

    void SaveGame(string name)
    {
        GameData data = GetPlayerData();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(name), json);

        Debug.Log("Saved: " + name);
    }

    void RefreshSaveList()
    {
        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        if (files.Length == 0)
        {
            savedGamesText.text = "No saves";
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>Saves:</b>\n");

        foreach (var file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            DateTime date = File.GetLastWriteTime(file);
            sb.AppendLine($"• {name} ({date:dd.MM HH:mm})");
        }

        savedGamesText.text = sb.ToString();
    }

    public void CloseSavePanel() => saveGamePanel.SetActive(false);

    // ── LOAD ─────────────────────────────
    public void OpenLoadPanel()
    {
        loadGamePanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        RefreshLoadButtons();
    }

    public void CloseLoadPanel()
    {
        loadGamePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        ClearButtons();
    }

    void RefreshLoadButtons()
    {
        ClearButtons();

        string[] files = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (var file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            GameObject btn = Instantiate(loadButtonPrefab, loadButtonsContainer);
            spawnedButtons.Add(btn);

            btn.GetComponentInChildren<TMP_Text>().text = name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                StartCoroutine(LoadSceneAsync(name));
            });
        }
    }

    void ClearButtons()
    {
        foreach (var b in spawnedButtons)
            Destroy(b);

        spawnedButtons.Clear();
    }

    // ── DATA ─────────────────────────────
    GameData GetPlayerData()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        return new GameData
        {
            playerX = player.transform.position.x,
            playerY = player.transform.position.y,
            playerZ = player.transform.position.z,
            saveDate = DateTime.Now.ToString()
        };
    }

    void ApplyLoadedData(string name)
    {
        string path = GetPath(name);

        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        GameData data = JsonUtility.FromJson<GameData>(json);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
    }

    string GetPath(string name) =>
        Path.Combine(Application.persistentDataPath, name + ".json");

    string Sanitize(string input) =>
        System.Text.RegularExpressions.Regex.Replace(input, @"[^a-zA-Z0-9_-]", "_");
}

// ── DATA CLASS ─────────────────────────
[Serializable]
public class GameData
{
    public float playerX;
    public float playerY;
    public float playerZ;
    public string saveDate;
}