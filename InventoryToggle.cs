using UnityEngine;
using UnityEngine.UI;

public class InventoryToggle : MonoBehaviour
{
    [Header("Inventory Settings")]
    public GameObject inventoryPanel; // Přetáhněte sem ShowInventory

    public Button toggleButton; // Automaticky najde Button na tomto GameObjectu

    void Start()
    {
        // Najdi Button komponentu
        toggleButton = GetComponent<Button>();
        if (toggleButton != null)
        {
            // Přidej listener pro klik
            toggleButton.onClick.AddListener(ToggleInventory);
        }

        // Inicializuj inventář jako skrytý
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }

    // Volitelně: Toggle na klávesu I (pro testování)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
		
    }
}