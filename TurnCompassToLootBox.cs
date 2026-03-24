using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TurnCompassToLootBox : MonoBehaviour
{
    [Header("Compass")]
    public RectTransform compassUI;
    public Transform player;

    [Header("UI Texts")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI distanceText;

    [Header("Loot")]
    public string lootTag = "LootBox";
    private GameObject nearestLootBox;
    private Renderer lootRenderer;

    [Header("Spawn Loot Item")]
    public Transform lootSpawnPoint;

    private float lastClickTime;
    private float doubleClickThreshold = 0.3f;

    void Update()
    {
        FindNearestLootBox();
        RotateCompass();

        HandleLootHover();
        HandleLootClick();
        UpdateDistanceDisplay();
    }

    // ================================
    // FIND NEAREST LOOTBOX
    // ================================
    void FindNearestLootBox()
    {
        GameObject[] lootBoxes = GameObject.FindGameObjectsWithTag(lootTag);

        float minDistance = Mathf.Infinity;
        nearestLootBox = null;

        foreach (GameObject loot in lootBoxes)
        {
            float dist = Vector3.Distance(player.position, loot.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                nearestLootBox = loot;
            }
        }
    }

    // ================================
    // ROTATE COMPASS
    // ================================
    void RotateCompass()
    {
        if (nearestLootBox == null || player == null || compassUI == null)
            return;

        Vector3 dir = nearestLootBox.transform.position - player.position;
        dir.y = 0;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        compassUI.rotation = Quaternion.Euler(0, 0, -angle);
    }

    // ================================
    // HOVER HIGHLIGHT
    // ================================
    void HandleLootHover()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f))
        {
            if (hit.transform.CompareTag(lootTag))
            {
                lootRenderer = hit.transform.GetComponent<Renderer>();

                if (lootRenderer != null)
                {
                    lootRenderer.material.EnableKeyword("_EMISSION");
                    lootRenderer.material.SetColor("_EmissionColor", Color.yellow * 0.8f);

                    if (infoText != null)
                        infoText.text = "Do you want grab this lootbox item ?";
                }
            }
        }
        else
        {
            ClearLootHighlight();
        }
    }

    void ClearLootHighlight()
    {
        if (lootRenderer != null)
        {
            lootRenderer.material.SetColor("_EmissionColor", Color.black);
            lootRenderer = null;
        }
    }

    // ================================
    // CLICK LOOTBOX
    // ================================
    void HandleLootClick()
    {
        if (nearestLootBox == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f))
        {
            if (hit.transform.CompareTag(lootTag))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (Time.time - lastClickTime < doubleClickThreshold)
                    {
                        SpawnRandomLoot(hit.transform.position);
                    }

                    lastClickTime = Time.time;
                }
            }
        }
    }

    // ================================
    // SPAWN RANDOM LOOT
    // ================================
    void SpawnRandomLoot(Vector3 position)
    {
        GameObject[] items = Resources.LoadAll<GameObject>("Guns/models");

        if (items.Length == 0) return;

        int randomIndex = Random.Range(0, items.Length);

        Instantiate(items[randomIndex], position + Vector3.up * 1f, Quaternion.identity);
    }

    // ================================
    // DISTANCE DISPLAY
    // ================================
    void UpdateDistanceDisplay()
    {
        if (player == null || nearestLootBox == null || distanceText == null)
            return;

        float distance = Vector3.Distance(player.position, nearestLootBox.transform.position);

        distanceText.text = "Distance: " + distance.ToString("F1");
    }
}