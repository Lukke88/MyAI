using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
public class HeroController : MonoBehaviour
{
    public Camera mainCamera;            // Hlavní kamera
    public Transform firePoint;          // Místo odkud hrdina střílí
	public GameObject player;
    public bool HeroFiresFromStand = false;
    public Text infoText;                // UI Text pro výpis informací
    public float rotationSpeed = 2f;     // Jak rychle se otáčí
    public float maxRotationAngle = 30f; // Maximální natočení doleva/
	public TMP_Text infoTextTMP;
	private PickupBase currentTarget;
	
	[Header("Inventar")]
	public Transform frontHandle;                 // Null objekt na ruce

	public GameObject currentWeapon;
	public TMP_Dropdown inventoryDropdown;

	public List<WeaponData> inventory;
	public int bulletCount = 0;
	
	private Animator animator;
	public AudioSource audioSource;
	public AudioClip pickupClickSound;
	public GameObject inventoryWindow;
	
	public GameObject itemIconPrefab;
	public Transform inventoryGridUI;   // pro UI parent
	public InventoryGrid inventoryGrid; // logický grid

public GameObject inventoryItemPrefab;


	/*
	Když hráč sebere zbraň:

Přidáš WeaponData do inventory

Zavoláš CreateInventoryIcon(weapon)

Vytvoříš UI objekt (Image) uvnitř inventoryWindow
	*/
	public void Start()
	{
		player = GameObject.Find(this.name);
		animator = GetComponent<Animator>();

	}
    void Update()
    {
		DetectPickupTarget();
		if (currentWeapon != null)
		{
			currentWeapon.transform.localPosition = Vector3.zero;
			currentWeapon.transform.localRotation = Quaternion.identity;
		}

        HandleCtrlRotation();
        DetectEnemies();
		
		
		
		if (Input.GetKeyDown(KeyCode.I))
{
    inventoryWindow.SetActive(!inventoryWindow.activeSelf);
}

    }
	
	public void AddItemToGrid(WeaponData item)
{
    for (int y = 0; y < inventoryGrid.gridHeight; y++)
    {
        for (int x = 0; x < inventoryGrid.gridWidth; x++)
        {
            if (inventoryGrid.CanPlaceItem(item, x, y))
            {
                inventoryGrid.PlaceItem(item, x, y);
                CreateUIItem(item, x, y);
                return;
            }
        }
    }

    Debug.Log("No space in inventory!");
}

void CreateUIItem(WeaponData item, int x, int y)
{
    GameObject obj = Instantiate(inventoryItemPrefab, inventoryGridUI);

    InventoryUIItem uiItem = obj.GetComponent<InventoryUIItem>();
    uiItem.Setup(item, inventoryGrid.cellSize);

    RectTransform rt = obj.GetComponent<RectTransform>();
    rt.anchoredPosition = new Vector2(
        x * inventoryGrid.cellSize,
        -y * inventoryGrid.cellSize
    );
}
	
	void DetectPickupTarget()
{
    Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
    RaycastHit hit;

    Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.blue);

    if (Physics.Raycast(ray, out hit, 1000f))
    {
		if(!hit.collider.name.Contains("Ground"))
        Debug.Log("Ray hit: " + hit.collider.name);
	}

    if (Physics.Raycast(ray, out hit, 3000f))
    {
        PickupBase pickup = hit.collider.GetComponent<PickupBase>();
		Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red);
        if (pickup != null)
        {
            if (currentTarget != pickup)
            {
                ClearTarget();
                currentTarget = pickup;
                currentTarget.Highlight(true);
            }

         //   infoTextTMP.text = "Do you want to pick up " + pickup.itemName + "?";

            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(MoveToPickup(pickup));
            }

            return;
        }
    }

    ClearTarget();
}
void ClearTarget()
{
    if (currentTarget != null)
    {
        currentTarget.Highlight(false);
        currentTarget = null;
    }

    infoTextTMP.text = "";
}
IEnumerator MoveToPickup(PickupBase pickup)
{
    while (Vector3.Distance(transform.position, pickup.transform.position) > 1f)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            pickup.transform.position,
            3f * Time.deltaTime
        );

        yield return null;
    }

    yield return StartCoroutine(ProcessPickupWithAnimation(pickup));

}
IEnumerator ProcessPickupWithAnimation(PickupBase pickup)
{
    animator.SetInteger("IsMiaGrabItem", 1);

    yield return new WaitForSeconds(0.8f); // délka animace

    animator.SetInteger("IsMiaGrabItem", 0);

    WeaponPickup weapon = pickup.GetComponent<WeaponPickup>();
    AmmoPickup ammo = pickup.GetComponent<AmmoPickup>();

    if (weapon != null)
    {
       AddItemToGrid(weapon.weaponData);

    }

    if (ammo != null)
    {
        bulletCount += ammo.amount;
        Debug.Log("Ammo increased: " + bulletCount);
    }

    audioSource.PlayOneShot(pickupClickSound);

    Destroy(pickup.gameObject);
}


	public void AddItem(WeaponData weapon)
	{
    inventory.Add(weapon);
	CreateInventoryIcon(weapon);
    inventoryDropdown.ClearOptions();

    List<string> options = new List<string>();

    foreach (WeaponData item in inventory)
    {
        options.Add(item.weaponName);
    }

    inventoryDropdown.AddOptions(options);

    inventoryDropdown.RefreshShownValue();
	}



	void CreateInventoryIcon(WeaponData weapon)
{
    GameObject iconObj = Instantiate(itemIconPrefab, inventoryGridUI);


    Image img = iconObj.GetComponent<Image>();
    img.sprite = weapon.icon;

    DragItem dragItem = iconObj.GetComponent<DragItem>();
    dragItem.icon = img;
}

public void GrabItem(GameObject item)
{
    WeaponData weapon = item.GetComponent<WeaponData>();
    if (weapon != null)
    {
        AddItem(weapon);
        item.SetActive(false); // skrytí objektu ve scéně
    }
    else
    {
        Debug.LogWarning("This GameObject has no WeaponData!");
    }
}
	public void EquipItem(int index)
	{
    if (index < 0 || index >= inventory.Count)
        return;

    if (currentWeapon != null)
        Destroy(currentWeapon);

    currentWeapon = Instantiate(inventory[index].prefab);

    currentWeapon.transform.SetParent(frontHandle);
    if (currentWeapon != null)
		{
			currentWeapon.transform.localPosition = Vector3.zero;
			currentWeapon.transform.localRotation = Quaternion.identity;
		}

	}

	

    void HandleCtrlRotation()
    {
        // Pokud je stisknuto Ctrl
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            HeroFiresFromStand = true;

            // Získat pozici myši ve světových souřadnicích
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z; // zachovat vzdálenost
            Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

            // Vektor od hrdiny k myši
            Vector3 direction = worldMousePos - transform.position;
            direction.y = 0; // ignorovat výšku

            // Vypočítat požadovaný úhel
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Omezení natočení na maxRotationAngle
            angle = Mathf.Clamp(angle, -maxRotationAngle, maxRotationAngle);

            // Plynulé otáčení
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            HeroFiresFromStand = false;
        }
    }

    void DetectEnemies()
    {
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 200f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                string enemyName = hit.collider.name;
                if (infoText != null)
                    infoText.text = "You have detected " + enemyName;
            }
        }
    }
}
[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public GameObject prefab;
    public Sprite icon;

    public int width = 1;
    public int height = 1;
}


public class InventoryGrid : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 20;
    public float cellSize = 50f;

    private WeaponData[,] grid;

    void Awake()
    {
        grid = new WeaponData[gridWidth, gridHeight];
    }

    public bool CanPlaceItem(WeaponData item, int startX, int startY)
    {
        for (int x = 0; x < item.width; x++)
        {
            for (int y = 0; y < item.height; y++)
            {
                int checkX = startX + x;
                int checkY = startY + y;

                if (checkX >= gridWidth || checkY >= gridHeight)
                    return false;

                if (grid[checkX, checkY] != null)
                    return false;
            }
        }

        return true;
    }

    public void PlaceItem(WeaponData item, int startX, int startY)
    {
        for (int x = 0; x < item.width; x++)
        {
            for (int y = 0; y < item.height; y++)
            {
                grid[startX + x, startY + y] = item;
            }
        }
    }
}

public class InventoryUIItem : MonoBehaviour
{
    public Image icon;
    public WeaponData itemData;

    public void Setup(WeaponData data, float cellSize)
    {
        itemData = data;
        icon.sprite = data.icon;

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(
            data.width * cellSize,
            data.height * cellSize
        );
    }
}

public class AmmoPickup : PickupBase
{
    public int amount = 30;
}
public class WeaponPickup : PickupBase
{
    public WeaponData weaponData;
}

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image icon;

    public void SetItem(Sprite sprite)
    {
        icon.sprite = sprite;
        icon.enabled = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        DragItem dragItem = eventData.pointerDrag.GetComponent<DragItem>();

        if (dragItem != null)
        {
            SetItem(dragItem.icon.sprite);
            dragItem.originalSlot.ClearSlot();
            Destroy(dragItem.gameObject);
        }
    }

    public void ClearSlot()
    {
        icon.sprite = null;
        icon.enabled = false;
    }
}
public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image icon;
    public Transform originalParent;
    public InventorySlot originalSlot;

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<InventorySlot>();

        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }
}