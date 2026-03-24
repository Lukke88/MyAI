using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorForceEnable : MonoBehaviour
{
    [Header("UI Cursor")]
    public RectTransform cursorArrow;
    public string selectableTag = "Enemy";
    public string tileTag = "tiles";

    [Header("UI Text Info")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI enemyDescriptionText;

    [Header("Raycast")]
    public Camera mainCamera;
    public LayerMask groundMask;

    public Vector3 finalDestinationForPlayer;
    public GameObject mainPlayer;

    public static Vector3 CurrentCursorWorldPosition;

    // Highlight system
    private GameObject lastHighlightedTile;
    private float lastClickTime;
    private float doubleClickThreshold = 0.3f;

[Header("Weapon System")]
public string currentWeaponType; // nastavíš z dropdownu např. "rifle"
public Transform weaponHolder;   // bone ruky Mii
private GameObject currentWeaponInstance;
public Animator miaAnimator;
/*
string itemName = "AKM_rifle";
string weaponName = itemName.Split('_')[0];
*/
[Header("Enemy Aim UI")]
public TextMeshProUGUI aimingText;

[Header("Weapon Dropdown")]
public TMP_Dropdown weaponDropdown;

[Header("WeaponHolder")]
public GameObject Hand_Gun_Holder;

[Header("Shooting System")]
public Transform muzzlePoint;                // konec hlavně
public GameObject null_projectile;          // prázdný objekt co se bude pohybovat
public LineRenderer bulletLine;
public ParticleSystem muzzle_effect;
public AudioSource weaponAudioSource;
public AudioClip shootClip;

private GameObject currentTarget;

/*
Hand_Gun_Holder

nastav jako weaponHolder

Weapon prefab

musí mít správnou orientaci (hlaveň dopředu Z+)

MuzzlePoint

vytvoř Empty objekt na konci hlavně

přetáhni do muzzlePoint

null_projectile

prázdný objekt (malá sphere)

nastav jako inactive default

LineRenderer

material → Unlit/Color

width 0.02

disabled default

AudioSource

přidej na zbraň nebo Miu

přetáhni do weaponAudioSource
*/
private bool isAimingAtEnemy;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (mainCamera == null)
            mainCamera = Camera.main;
		
		if (weaponDropdown != null)
		{
			weaponDropdown.onValueChanged.AddListener(OnWeaponChanged);
		}
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        FollowMouse();
        RaycastFromCursorTip();

        if (mainPlayer != null && finalDestinationForPlayer != Vector3.zero)
        {
            mainPlayer.GetComponent<MiaBehaviour>().FinalDestination = finalDestinationForPlayer;
        }
		
		HandleWeaponChange();
		HandleEnemyAiming();
    }
	
	void HandleWeaponChange()
{
    if (currentWeaponType == "rifle" && currentWeaponInstance == null)
    {
        GameObject weaponPrefab = Resources.Load<GameObject>("Guns/models/AKM");

        if (weaponPrefab != null && weaponHolder != null)
        {
            currentWeaponInstance = Instantiate(weaponPrefab, weaponHolder);
            currentWeaponInstance.transform.localPosition = Vector3.zero;
            currentWeaponInstance.transform.localRotation = Quaternion.identity;
        }

        if (miaAnimator != null)
        {
            miaAnimator.SetInteger("IsMiaFiringRifle", 1);
			miaAnimator.Play("MiaFiringRifle");
        }
    }
}
void OnWeaponChanged(int index)
{
    string selectedOption = weaponDropdown.options[index].text.ToLower();

    // např. item je AKM_rifle
    string itemName = "AKM_" + selectedOption;

    // odstraníme "_rifle"
    string weaponName = itemName.Split('_')[0];

    // smažeme starou zbraň
    if (currentWeaponInstance != null)
    {
        Destroy(currentWeaponInstance);
    }

    // načteme prefab
    GameObject weaponPrefab = Resources.Load<GameObject>("Guns/models/" + weaponName);

    if (weaponPrefab != null && weaponHolder != null)
    {
        currentWeaponInstance = Instantiate(weaponPrefab, weaponHolder);
        currentWeaponInstance.transform.localPosition = Vector3.zero;
        currentWeaponInstance.transform.localRotation = Quaternion.identity;
    }
}
void HandleEnemyAiming()
{
    if (mainCamera == null) return;

    Vector3 cursorPos = cursorArrow.position;
    Ray ray = mainCamera.ScreenPointToRay(cursorPos);

    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 500f))
    {
       if (hit.transform.CompareTag(selectableTag))
{
    currentTarget = hit.transform.gameObject;
    isAimingAtEnemy = true;

    Renderer rend = currentTarget.GetComponent<Renderer>();

    if (rend != null && rend.material.HasProperty("_EmissionColor"))
    {
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.red * 0.8f);
    }

    if (aimingText != null)
    {
        aimingText.text = "You are aiming to enemy " + currentTarget.name;
        aimingText.enabled = true;
    }

    RotateWeaponToTarget();

    if (Input.GetMouseButtonDown(0))
    {
        Shoot();
    }

    return;
}
    }
	
	if (currentTarget != null)
	{
    Renderer rend = currentTarget.GetComponent<Renderer>();

    if (rend != null)
    {
        rend.material.SetColor("_EmissionColor", Color.black);
    }
	}

    isAimingAtEnemy = false;
    currentTarget = null;

    if (aimingText != null)
    {
        aimingText.text = "";
        aimingText.enabled = false;
    }
}

void RotateWeaponToTarget()
{
    if (currentWeaponInstance == null || currentTarget == null) return;

    Vector3 dir = currentTarget.transform.position - currentWeaponInstance.transform.position;
    dir.y = 0f;

    currentWeaponInstance.transform.rotation = Quaternion.LookRotation(dir);
}

void Shoot()
{
    if (muzzlePoint == null || currentTarget == null) return;

    // Zvuk
    if (weaponAudioSource != null && shootClip != null)
    {
        weaponAudioSource.PlayOneShot(shootClip);
    }

    // Particle
    if (muzzle_effect != null)
    {
        muzzle_effect.Play();
    }

    // Aktivace projectile objektu
    if (null_projectile != null)
    {
        null_projectile.transform.position = muzzlePoint.position;
        StartCoroutine(MoveProjectile(null_projectile.transform, currentTarget.transform.position));
    }

    // LineRenderer efekt
    if (bulletLine != null)
    {
        bulletLine.enabled = true;
        bulletLine.SetPosition(0, muzzlePoint.position);
        bulletLine.SetPosition(1, muzzlePoint.position + muzzlePoint.forward * 20f);

        Invoke(nameof(DisableLine), 0.05f);
    }

    // Hit info
    if (infoText != null)
    {
        infoText.text = "Enemy got hit and lost 5% of health";
    }
}

System.Collections.IEnumerator MoveProjectile(Transform proj, Vector3 targetPos)
{
    float speed = 40f;

    while (Vector3.Distance(proj.position, targetPos) > 0.1f)
    {
        proj.position = Vector3.MoveTowards(proj.position, targetPos, speed * Time.deltaTime);
        yield return null;
    }

    proj.gameObject.SetActive(false);
}

void DisableLine()
{
    if (bulletLine != null)
        bulletLine.enabled = false;
}

    // ================================
    // Move UI cursor
    // ================================
    void FollowMouse()
    {
        cursorArrow.position = Input.mousePosition;
    }

    // ================================
    // Raycast from cursor tip
    // ================================
    void RaycastFromCursorTip()
    {
        Vector3 cursorPos = cursorArrow.position;
        Vector3 tipScreenPos = cursorPos + cursorArrow.up * 20f;
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return; // UI je pod myší → neprováděj pohyb
		}
        Ray ray = mainCamera.ScreenPointToRay(tipScreenPos);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2000f, groundMask))
        {
            CurrentCursorWorldPosition = hit.point;

            float distance = Vector3.Distance(
                mainCamera.transform.position,
                hit.point
            );

            if (infoText != null)
            {
                infoText.text =
                    "x: " + hit.point.x.ToString("F1") +
                    " z: " + hit.point.z.ToString("F1") +
                    "\nDistance: " + distance.ToString("F1");
            }

            HandleTileHighlight(hit);
            HandleTileDoubleClick(hit);
        }
        else
        {
            ClearHighlight();
            SelectEnemyUnit();
        }
    }

    // ================================
    // TILE HIGHLIGHT (Emission)
    // ================================
    void HandleTileHighlight(RaycastHit hit)
    {
        GameObject hitObj = hit.transform.gameObject;

        if (hitObj.CompareTag(tileTag))
        {
            if (lastHighlightedTile != hitObj)
            {
                ClearHighlight();

                lastHighlightedTile = hitObj;

                Renderer rend = hitObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = rend.material;

                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.white * 0.6f);
                    }
                }
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    void ClearHighlight()
    {
        if (lastHighlightedTile != null)
        {
            Renderer rend = lastHighlightedTile.GetComponent<Renderer>();

            if (rend != null)
            {
                Material mat = rend.material;

                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }

            lastHighlightedTile = null;
        }
    }

    // ================================
    // DOUBLE CLICK MOVE TO TILE
    // ================================
    void HandleTileDoubleClick(RaycastHit hit)
    {
        if (!hit.transform.CompareTag(tileTag))
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickThreshold)
            {
                finalDestinationForPlayer = hit.point;
            }

            lastClickTime = Time.time;
        }
    }

    // ================================
    // Enemy selection (old system)
    // ================================
    public GameObject selectedEnemy;

    void SelectEnemyUnit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2000f))
        {
            GameObject obj = hit.transform.gameObject;

            if (obj.CompareTag(selectableTag))
            {
                selectedEnemy = obj;
            }
        }

        AttachEnemyToPlayerScripts();
    }

    public void AttachEnemyToPlayerScripts()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject go in allPlayers)
        {
            var movement = go.GetComponent<PlayerSelectedGroupMovement>();

            if (movement != null && movement.selectedEnemyUnit == null && selectedEnemy != null)
            {
                movement.selectedEnemyUnit = selectedEnemy;
            }
        }
    }
}