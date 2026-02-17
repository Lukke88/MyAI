using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class MiaBehaviour : MonoBehaviour
{
    public enum Animations
    {
        MiaIdle,
        MiaWalks,
        MiaRunBackwards,
        MiaRunWithRifle,
        MiaSprintForward,
        MiaRollForward,
        MiaCrouching,
        MiaCrouchForward,
        MiaProneForward,
        MiaShotCrawl,
        MiaThrowFromCrawl,
        MiaTossGrenadeFromStand,
        MiaGettingUp,
        MiaStandingUp,
        MiaFallOver,
        MiaDeath,
        MiaGrabItem,
        MiaPickItem,
        MiaGrabRifleFromBack,
        MiaFiringRifle,
        MiaBoxing
    }
	public string Crouch_animation = "MiaCrouching";
	public string Stand_animation = "MiaStandingUp";//running is MiaRunWithRifle or MiaSprintForward
	public string Crawl_animation = "MiaProneForward";
	public string Shoot_animation = "MiaFiringRifle";
	public string Idle_animation = "MiaIdle";
	public string Throw_stand_animation = "MiaTossGrenadeFromStand";
	public string Throw_crawl_animation = "MiaThrowFromCrawl";
	//public string SelectedAnimation;
public enum Inventory
{
    None,
    AKM,
    Pistol,
    Knife
    // ... další zbraně
}

// privátní proměnná pro Mia
private Inventory miaInventory = Inventory.None;
    [Header("Animation")]
    public Animations selectedAnimation;

   [Header("Activation")]
    public bool IsActivated = false;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 180f;

    private Vector3 finalDestination;
    private bool hasDestination = false;

    [Header("Audio")]
    public AudioClip runClip;
	public Camera mainCamera;
    [Header("UI")]
    public TMP_Text infoText;
    public TMP_Text infoCoordinates;

    private Animator animator;
    private CharacterController controller;
    private AudioSource audioSource;

    [Header("UI")]
    public Button activateButton;

    public Transform player;
	public float y_marker, y_player, height_player;
	 public Transform weapon;        // Drag & drop zbraň sem

    public float weaponHeightPercent = 0.8f; // výška rukou 80% postavy
    public string enemyTag = "Enemy";
	public GameObject marker_;
	
	private Vector3 lastPosition;
private float stillTimer = 0f;
private float idleDelay = 2f; // čas v sekundách, než přejde do idle

private GameObject targetWeapon;       // objekt na který míříme
private bool weaponClicked = false;    // bylo kliknuto na zbraň

[Header("Ammunition")]
public int maxAmmoAKM = 30;
public int maxAmmoPistol = 15;

private int currentAmmo = 0;      // aktuální počet nábojů pro aktivní zbraň

[Header("Weapon Slots")]
public Transform fireSlot;   // pozice ruky
public Transform backSlot;   // pozice na zádech

private GameObject activeWeapon = null;   // zbraň, která je právě v rukou

[Header("Weapon Info Display")]
public GameObject infoTextPrefab;   // prefab textu (např. TextMeshPro)
private GameObject currentInfoText;  // instance textu
private float hoverTime = 0f;        // čas, po který je kurzor nad zbraní
public float hoverDelay = 0.5f;      // po kolika sekundách se infotext objeví

private GameObject hoveredWeapon;     // zbraň, nad kterou je kurzor

[Header("Weapon Effects")]
public GameObject muzzleFlashPrefab;      // Prefab pro výbuch hlavně
public GameObject nullBulletPrefab;       // Prefab pro "bullet point" bez physics
public float bulletSpeed = 50f;           // rychlost pohybu nullBullet
public LineRenderer bulletTrailPrefab;    // LineRenderer prefab pro žlutý ohon
public AudioClip fireSound;               // zvuk střelby

[Header("Grenade System")]
public GameObject prefab_grenade, generated_grenade;
public float grenadeThrowForce = 15f;
public float grenadeArcHeight = 5f;
private Vector3 TargetDestination;
private bool targetLocked = false;

[Header("Explosion Settings")]
public GameObject ppfxExplosionFireball;
public float explosionRadius = 15.0f;
public float explosionForce = 800f;
public float maxDamage = 100f;
public AudioClip grenadeExplosionClip;
public float explosionSoundVolume = 1f;

[Header("Auto Aim & Shoot")]
public float aimMaxRotationAngle = 30f; // max natočení postavy při aimu
public float aimRotationSpeed = 5f;     // rychlost otáčení při aimu

[Header("Health System")]
public Slider healthSlider;     // přiřadit v Inspector
public float maxHealth = 100f;
private float currentHealth;
public bool IsHit;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }
 private void Start()
    {
        player = this.gameObject.transform;

    if (activateButton != null)
    {
        activateButton.onClick.AddListener(ActivateHero);
    }

    // Inicializace zdraví
    currentHealth = maxHealth;
    if (healthSlider != null)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }

    }
	
    private void Update()
    {
       
		if(IsActivated==true)
		{
			if(player==null)
			{
			player = GameObject.Find(this.name).transform;
			marker_ = GameObject.Find("marker");
			y_marker = marker_.transform.position.y;
			y_player = player.transform.position.y;
			mainCamera = Camera.main;
			}
        HandleKeyboardMovement();
        HandleMouseClickMovement();
		CheckIdleState();
        UpdateCoordinatesUI();
		HandleWeaponSwitchInput();
		PositionWeapon(); // aby se aktivní zbraň vždy správně nastavila
        DetectEnemies();
		HandleWeaponHover();
		HandleWeaponClick();
		HandleWeaponHoverInfo();
		HandleCombatInput();
		HandleAutoAimAndShoot();
		if(Input.GetKeyDown(KeyCode.LeftControl))
			FireWeapon();
		else
		{
			  animator.SetInteger("Is" + Shoot_animation, 0);
		}
		marker_.transform.position = new Vector3(player.transform.position.x, y_marker, player.transform.position.z);
		CameraFollowsHero cam = mainCamera.GetComponent<CameraFollowsHero>();
		cam.mainHero = player.transform;
            cam.target = player.transform;
			
		}
    }
	
	public void DamageHero(float damageAmount)
{
    if (currentHealth <= 0) return; // už mrtvá

    currentHealth -= damageAmount;
    currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

    // update slideru
    if (healthSlider != null)
        healthSlider.value = currentHealth;

    // Fallout hláška
    if (infoText != null)
        infoText.text = $"It seems that someone doesn't like you. Lost {damageAmount}% health.";

    // Animace podle zdraví
    if (currentHealth > 0f)
    {
        // pokud je zásah fatální? ne, jen pad
        selectedAnimation = Animations.MiaFallOver;
        ResetAllParameters();
        animator.Play(selectedAnimation.ToString());
    }
    else
    {
        // smrt
        selectedAnimation = Animations.MiaDeath;
        ResetAllParameters();
        animator.Play(selectedAnimation.ToString());
    }
}

	private void HandleAutoAimAndShoot()
{
    // Kontrola, zda držíme Ctrl
    if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
    {
        // 1. Aim – otočení postavy směrem k myši
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

        Vector3 direction = worldMousePos - transform.position;
        direction.y = 0; // ignorovat výšku
        if (direction != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            targetAngle = Mathf.Clamp(targetAngle, -aimMaxRotationAngle, aimMaxRotationAngle);

            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * aimRotationSpeed);
        }

        // 2. Raycast pro detekci nepřátel
        Ray ray = new Ray(weapon.position, weapon.forward);
        RaycastHit hit;
        float maxDistance = 100f;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.CompareTag(enemyTag))
            {
                // automatická střelba
                HeroFiresFromStand();
            }
        }
    }
}

	private void HandleCombatInput()
{
    // Zamknutí cíle Ctrl + klik
    if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            TargetDestination = hit.point;
            targetLocked = true;

            // otočení postavy směrem k cíli
            Vector3 dir = TargetDestination - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    if (!targetLocked) return;

    // Výběr akce podle postoje
    if (Input.GetKeyDown(KeyCode.G)) // hod granátu
    {
        if (selectedAnimation == Animations.MiaProneForward)
            HeroThrowsFromCrawl();
        else
            HeroThrowsFromStand();
    }

    if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetMouseButtonDown(0))
	{
    if (selectedAnimation == Animations.MiaProneForward)
        HeroFiresFromCrawl();
    else
        HeroFiresFromStand();
	}
	else if((!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) && Input.GetMouseButtonDown(0))
	{
		//run
		//SelectedAnimation = Animations.MiaRunWithRifle;
		selectedAnimation = Animations.MiaSprintForward;
		PlayAnimation(Animations.MiaRunWithRifle);
	}
}

	void PositionWeapon()
    {
        if (weapon == null) return;

        // Najdi výšku postavy
        float charHeight = GetComponent<Collider>().bounds.size.y;

        // Nastav pozici zbraně: uprostřed X osy, 80% výšky
        Vector3 newPos = transform.position;
        newPos.y += charHeight * weaponHeightPercent;
        newPos.x = transform.position.x; // doprostřed X osy postavy
        newPos.z = transform.position.z; // zachovat Z

		if(weapon!=null && activeWeapon==null)
        weapon.position = newPos;
		else if(activeWeapon!=null)
			PositionWeaponInSlots();

        // Natáčí zbraň hlavní směrem postavy
        weapon.rotation = transform.rotation;
    }
	
	void PositionWeaponInSlots()
{
    if (activeWeapon == null) return; // nic nenastavujeme, pokud není zbraň v ruce

    // Najdi výšku postavy
    float charHeight = GetComponent<Collider>().bounds.size.y;

    // Nastav pozici zbraně podle fireSlotu
    Vector3 newPos = fireSlot.position;
    newPos.y += charHeight * weaponHeightPercent; // offset výšky
    activeWeapon.transform.position = newPos;

    // Natáčení zbraně stejným směrem jako postava
    activeWeapon.transform.rotation = transform.rotation;
}

	private void FireWeapon()
{
    HeroFiring(); // tvoje animace střelby

    if (miaInventory == Inventory.None || currentAmmo <= 0)
    {
        infoText.text = "No ammo!";
        return;
    }

    currentAmmo--;
    infoText.text = miaInventory.ToString() + " fired. Ammo left: " + currentAmmo;

    // ======================
    // 1. Zvuk
    // ======================
    if (fireSound != null)
        audioSource.PlayOneShot(fireSound);

    // ======================
    // 2. Raycast dopředu
    // ======================
    Ray ray = new Ray(fireSlot.position, fireSlot.forward);
    RaycastHit hit;
    float maxDistance = 100f;

    Vector3 hitPoint = ray.GetPoint(maxDistance); // výchozí pozice, pokud nic nezasáhne

    if (Physics.Raycast(ray, out hit, maxDistance))
    {
        hitPoint = hit.point;

        if (hit.collider.CompareTag("Enemy"))
        {
            // enemy dostal zásah – aktivuj hit animaci
            Animator enemyAnimator = hit.collider.GetComponent<Animator>();
            if (enemyAnimator != null)
            {
                enemyAnimator.Play("hit_animation");
            }
        }
    }

    // ======================
    // 3. NullBullet + LineRenderer
    // ======================
    if (nullBulletPrefab != null && bulletTrailPrefab != null)
    {
        GameObject nullBullet = Instantiate(nullBulletPrefab, fireSlot.position, Quaternion.identity);
        LineRenderer trail = Instantiate(bulletTrailPrefab, fireSlot.position, Quaternion.identity);
        trail.positionCount = 2;
        trail.SetPosition(0, nullBullet.transform.position);
        trail.SetPosition(1, nullBullet.transform.position + fireSlot.forward * 20f);
        trail.startColor = trail.endColor = Color.yellow;

        StartCoroutine(MoveBullet(nullBullet, trail, hitPoint));
    }

    // ======================
    // 4. MuzzleFlash
    // ======================
    if (muzzleFlashPrefab != null)
    {
        // počítáme muzzlePoint
        Collider gunCollider = fireSlot.GetComponent<Collider>();
        Vector3 muzzlePoint = fireSlot.position + fireSlot.forward * 1f; // default
        if (gunCollider != null)
        {
            Bounds bounds = gunCollider.bounds;
            muzzlePoint = bounds.center + fireSlot.forward * (bounds.extents.z); // nejvzdálenější stěna
        }

        GameObject muzzle = Instantiate(muzzleFlashPrefab, muzzlePoint, fireSlot.rotation);
        Destroy(muzzle, 2f); // po 2 sekundách zmizí
    }
}

// --------------------------
// Coroutine pro pohyb nullBullet
// --------------------------
private System.Collections.IEnumerator MoveBullet(GameObject bullet, LineRenderer trail, Vector3 targetPoint)
{
    Vector3 startPos = player.position; // výchozí pozice hráče
    while (bullet != null)
    {
        Vector3 dir = (targetPoint - bullet.transform.position).normalized;
        float step = bulletSpeed * Time.deltaTime;

        // pohyb bulletu
        bullet.transform.position += dir * step;

        // update trail
        if (trail != null)
        {
            trail.SetPosition(0, bullet.transform.position);
            trail.SetPosition(1, bullet.transform.position + dir * 20f);
        }

        // ---------- 1. Samodestrukce po vzdálenosti ----------
        if (Vector3.Distance(startPos, bullet.transform.position) > 900f)
        {
            Destroy(bullet);
            if (trail != null) Destroy(trail.gameObject);
            yield break;
        }

        // ---------- 2. Kontrola kolize ----------
        RaycastHit hit;
        if (Physics.Raycast(bullet.transform.position, dir, out hit, step))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                Animator enemyAnimator = hit.collider.GetComponent<Animator>();
                if (enemyAnimator != null) enemyAnimator.Play("hit_animation");

                Destroy(bullet);
                if (trail != null) Destroy(trail.gameObject);
                yield break;
            }
            else if (hit.collider.CompareTag("Wall"))
            {
                // vytvoří efekt exploze prachu
                if (muzzleFlashPrefab != null)
                {
                    GameObject dust = Instantiate(muzzleFlashPrefab, hit.point, Quaternion.identity);
                    Destroy(dust, 2f);
                }

                // vytvoření plane s texturou díry ve zdi
                GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Quad);
                hole.transform.position = hit.point + hit.normal * 0.01f; // lehce nad stěnou
                hole.transform.rotation = Quaternion.LookRotation(hit.normal);
                hole.transform.localScale = Vector3.one * 0.5f; // velikost díry
                if (hole.GetComponent<Collider>() != null)
                    Destroy(hole.GetComponent<Collider>()); // nechceme kolize
                // přiřadíme texturu díry (musí být materiál s transparentností)
                Material holeMat = new Material(Shader.Find("Standard"));
                // tady si doplníš svůj texture resource
                // holeMat.mainTexture = holeTexture;
                hole.GetComponent<MeshRenderer>().material = holeMat;

                Destroy(bullet);
                if (trail != null) Destroy(trail.gameObject);
                yield break;
            }
        }

        yield return null;
    }
}


private void HandleWeaponHoverInfo()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 100f))
    {
        GameObject hitObj = hit.collider.gameObject;

        if (hitObj.name == "AKM" || hitObj.name == "Pistol") // zbraně
        {
            // pokud kurzor na nové zbrani, reset timer
            if (hoveredWeapon != hitObj)
            {
                hoveredWeapon = hitObj;
                hoverTime = 0f;
                DestroyCurrentInfoText();
            }

            hoverTime += Time.deltaTime;

            // pokud kurzor držíme dost dlouho, ukaž infotext
            if (hoverTime >= hoverDelay && currentInfoText == null)
            {
                ShowWeaponInfo(hoveredWeapon);
            }
        }
        else
        {
            hoveredWeapon = null;
            hoverTime = 0f;
            DestroyCurrentInfoText();
        }
    }
    else
    {
        hoveredWeapon = null;
        hoverTime = 0f;
        DestroyCurrentInfoText();
    }

    // Pokud máme infotext, nechat ho sledovat zbraň
    if (currentInfoText != null && hoveredWeapon != null)
    {
        Vector3 infoPos = hoveredWeapon.transform.position + Vector3.up * 1.5f; // nad zbraní
        currentInfoText.transform.position = infoPos;
        currentInfoText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}


private void DestroyCurrentInfoText()
{
    if (currentInfoText != null)
    {
        Destroy(currentInfoText);
        currentInfoText = null;
    }
}
private void ShowWeaponInfo(GameObject weapon)
{
    WeaponItem weaponItem = weapon.GetComponent<WeaponItem>();
    if (weaponItem == null || weaponItem.itemDescription == null) return;

    ItemDescription desc = weaponItem.itemDescription;

    currentInfoText = Instantiate(infoTextPrefab, weapon.transform.position + Vector3.up * 1.5f, Quaternion.identity);

    // Text + obrázek
    TextMeshPro tmp = currentInfoText.GetComponent<TextMeshPro>();
    if (tmp != null)
    {
        tmp.text = $"{desc.itemName}\nAmmo: {desc.maxAmmo}\n{desc.description}";
    }

    // Pokud prefab obsahuje Image (UI), můžeme nastavit ikonku
    // currentInfoText může být canvas s Text + Image
    UnityEngine.UI.Image img = currentInfoText.GetComponentInChildren<UnityEngine.UI.Image>();
    if (img != null && desc.icon != null)
    {
        img.sprite = desc.icon;
        img.enabled = true;
    }
}

private void SwitchWeapon(int slotNumber)
{
    // slotNumber = 1 → z backSlotu do ruky
    // slotNumber = 2 → jiná zbraň, případně další slot
    // zde ukázka jen pro backSlot → fireSlot

    if (backSlot.childCount > 0)
    {
        // vezmi první zbraň na zádech
        GameObject weaponOnBack = backSlot.GetChild(0).gameObject;

        // pokud už v ruce něco je, dej ji na záda
        if (activeWeapon != null)
        {
            activeWeapon.transform.SetParent(backSlot);
            activeWeapon.transform.localPosition = Vector3.zero;
            activeWeapon.transform.localRotation = Quaternion.identity;
        }

        // přesuň novou zbraň do ruky
        activeWeapon = weaponOnBack;
        activeWeapon.transform.SetParent(fireSlot);
        activeWeapon.transform.localPosition = Vector3.zero;
        activeWeapon.transform.localRotation = Quaternion.identity;

        infoText.text = "Switched to " + activeWeapon.name;
    }
}
private void HandleWeaponSwitchInput()
{
    if (Input.GetKeyDown(KeyCode.Keypad1))
    {
        SwitchWeapon(1);
    }
    else if (Input.GetKeyDown(KeyCode.Keypad2))
    {
        SwitchWeapon(2);
    }
    // ... další sloty podle potřeby
}

	private void HandleWeaponHover()
	{
    // Raycast z kamery k myši
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 100f))
    {
        GameObject hitObj = hit.collider.gameObject;

        if (hitObj.name == "AKM") // nebo podle tagu
        {
            targetWeapon = hitObj;
            infoText.text = "Do you want to grab " + targetWeapon.name + "?";
        }
        else
        {
            targetWeapon = null;
            infoText.text = ""; // nebo jiný text
        }
    }
}

private void HandleWeaponClick()
{
    if (targetWeapon == null) return;

    if (Input.GetMouseButtonDown(0))
    {
        // nastavit cílovou pozici pro Miu
        finalDestination = targetWeapon.transform.position;
        hasDestination = true;
        weaponClicked = true;
    }

    // Pokud Mia dojde na zbraň
    if (weaponClicked && Vector3.Distance(transform.position, finalDestination) < 0.5f)
    {
        GrabWeapon(targetWeapon);
        weaponClicked = false;
    }
}
private void GrabWeapon(GameObject weaponObj)
{
    if (weaponObj == null) return;

    // Pokud už je v rukou zbraň, přesun na záda
    if (weapon != null && miaInventory != Inventory.None)
    {
        weapon.position = backSlot.position;
        weapon.SetParent(backSlot);
    }

    // Připoj novou zbraň do rukou
    weaponObj.transform.SetParent(weapon);   // weapon = transform rukou
    weaponObj.transform.localPosition = Vector3.zero; // doladit offset podle rukou
    weaponObj.transform.localRotation = Quaternion.identity;

    // Aktualizace inventáře a nastavení nábojů
    if (weaponObj.name.Contains("AKM"))
    {
        miaInventory = Inventory.AKM;
        currentAmmo = maxAmmoAKM; // při sebrání naplníme zásobník
    }
    else if (weaponObj.name.Contains("Pistol"))
    {
        miaInventory = Inventory.Pistol;
        currentAmmo = maxAmmoPistol;
    }
    else
    {
        miaInventory = Inventory.None;
        currentAmmo = 0;
    }

    // Info text
    infoText.text = "Picked up " + weaponObj.name + " (" + currentAmmo + " rounds)";
}


    void DetectEnemies()
    {
        // Raycast dopředu od zbraně
        Ray ray = new Ray(weapon.position, weapon.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f)) // 100 jednotek dopředu
        {
            if (hit.collider.CompareTag(enemyTag))
            {
                infoText.text = "Enemy detected: " + hit.collider.name;
            }
        }
    }
   
	// Přidej tyto privátní proměnné nahoře ve třídě

// -----------------------
// CALL THIS IN Update()
// -----------------------
private void CheckIdleState()
{
    // Pokud nemáme cíl, ignoruj
    if (!hasDestination) return;

    // Spočítáme, jak moc se postava posunula od posledního frame
    float distanceMoved = Vector3.Distance(transform.position, lastPosition);

    // Pokud je skoro na místě, navyš čas stojící postavy
    if (distanceMoved < 0.01f)
    {
        stillTimer += Time.deltaTime;
    }
    else
    {
        stillTimer = 0f; // reset timeru
    }

    // Uložíme aktuální pozici pro další frame
    lastPosition = transform.position;

    // Pokud postava stojí 2 sekundy nebo je na finalDestination
    float distanceToDestination = Vector3.Distance(transform.position, finalDestination);
    if (stillTimer >= idleDelay || distanceToDestination < 0.2f)
    {
        // Přepnutí do MiaIdle
        ResetAllParameters();
        animator.SetInteger("IsMiaIdle", 1);
        animator.Play("MiaIdle");

        // Vypnutí původní animace
        hasDestination = false;
        StopRunSound();
        stillTimer = 0f; // reset pro další použití
    }
}

    // -----------------------
    // BUTTON FUNCTION
    // -----------------------

    public void ActivateHero()
    {
        IsActivated = true;
		if (infoText != null)
		infoText.text = "You activated " + this.name;
        // Nastavení kamery
        Camera.main.transform.GetComponent<CameraFollowsHero>().target = player;
       

        // Info text
        if (infoText != null)
        {
            infoText.text = "You activated " + this.name;
        }

        Debug.Log("Hero Activated: " + this.name);
    }
	 
	public void HeroStandUp()
{
    // Přepni animátor na Stand
    ResetAllParameters();
	selectedAnimation = Animations.MiaStandingUp;

    animator.SetInteger("Is" + Stand_animation, 1);
    animator.Play(Stand_animation);
    
    // Nastav rychlost pohybu pro stojící postoj
    moveSpeed = 25f;
    sprintSpeed = 7f;
    
    if (infoText != null)
        infoText.text = "Hero is standing.";
}
public void HeroFiring()
{
    // Přepni animátor na Stand
    ResetAllParameters();
	selectedAnimation = Animations.MiaFiringRifle;
    animator.SetInteger("Is" + Shoot_animation, 1);
    animator.Play(Shoot_animation);
    
    // Nastav rychlost pohybu pro stojící postoj
    moveSpeed = 4f;
    sprintSpeed = 7f;
    
    if (infoText != null)
        infoText.text = "Hero is shooting.";
}

public void HeroCrouches()
{
    // Přepni animátor na Crouch
	selectedAnimation = Animations.MiaCrouching;
    ResetAllParameters();
    animator.SetInteger("Is" + Crouch_animation, 1);
    animator.Play(Crouch_animation);
    
    // Snížená rychlost při plížení
    moveSpeed = 15f;
    sprintSpeed = 3.5f;
    
    if (infoText != null)
        infoText.text = "Hero is crouching.";
}
public void HeroThrowsFromCrawl()
{
    selectedAnimation = Animations.MiaThrowFromCrawl;
    ResetAllParameters();
    animator.Play(selectedAnimation.ToString());

    ThrowGrenade();
}
public void HeroThrowsFromStand()
{
    selectedAnimation = Animations.MiaTossGrenadeFromStand;
    ResetAllParameters();
    animator.Play(selectedAnimation.ToString());

    ThrowGrenade();
}
private void ThrowGrenade()
{
    if (prefab_grenade == null || !targetLocked) return;

    GameObject grenade = Instantiate(prefab_grenade, fireSlot.position, Quaternion.identity);

    StartCoroutine(GrenadeArc(grenade, TargetDestination));
}
private System.Collections.IEnumerator GrenadeArc(GameObject grenade, Vector3 target)
{
    float duration = 1.2f;
    float elapsed = 0f;

    Vector3 start = grenade.transform.position;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;

        // lineární pozice
        Vector3 current = Vector3.Lerp(start, target, t);

        // přidání výšky (parabola)
        current.y += grenadeArcHeight * Mathf.Sin(Mathf.PI * t);

        grenade.transform.position = current;

        yield return null;
    }

    grenade.transform.position = target;

// 🔥 INSTANCOVÁNÍ EXPLOZE
GameObject instantiated_explosion = Instantiate(
    ppfxExplosionFireball,
    grenade.transform.position,
    Quaternion.identity
);

// 🔊 ZVUK
if (grenadeExplosionClip != null)
{
    AudioSource.PlayClipAtPoint(
        grenadeExplosionClip,
        grenade.transform.position,
        explosionSoundVolume
    );
}

// 💥 OVERLAP SPHERE
Collider[] hitColliders = Physics.OverlapSphere(
    grenade.transform.position,
    explosionRadius
);

foreach (Collider col in hitColliders)
{
    float distance = Vector3.Distance(
        grenade.transform.position,
        col.transform.position
    );

    float damagePercent = 1f - (distance / explosionRadius);
    damagePercent = Mathf.Clamp01(damagePercent);

    float finalDamage = maxDamage * damagePercent;

    // 🚫 IGNORE TAGY
    if (col.CompareTag("building") ||
        col.CompareTag("wall") ||
        col.CompareTag("ground"))
        continue;

    // 💥 RIGIDBODY FORCE
    Rigidbody rb = col.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.AddExplosionForce(
            explosionForce,
            grenade.transform.position,
            explosionRadius
        );
    }

    // 👤 DAMAGE NA ENEMY
    if (col.CompareTag("Enemy"))
	{
    BaseTalibEnemyAI enemyAI = col.GetComponent<BaseTalibEnemyAI>();

    if (enemyAI != null)
    {
        enemyAI.TakeDamage(finalDamage);
    }
	}
}

// odstranění granátu
Destroy(grenade);

}

public void HeroFiresFromStand()
{
    selectedAnimation = Animations.MiaFiringRifle;
    ResetAllParameters();
    animator.Play(selectedAnimation.ToString());

    HeroFiring();
    FireWeapon();
}

public void HeroFiresFromCrawl()
{
    selectedAnimation = Animations.MiaShotCrawl;
    ResetAllParameters();
    animator.Play(selectedAnimation.ToString());

    HeroFiring();
    FireWeapon();
}

public void HeroCrawls()
{
    // Přepni animátor na Crawl
    ResetAllParameters();
	selectedAnimation = Animations.MiaProneForward;
    animator.SetInteger("Is" + Crawl_animation, 1);
    animator.Play(Crawl_animation);
    
    // Ještě nižší rychlost při plazení
    moveSpeed = 1f;
    sprintSpeed = 2f;
    
    if (infoText != null)
        infoText.text = "Hero is crawling.";
}

    // =========================
    // KEYBOARD MOVEMENT
    // =========================

    private void HandleKeyboardMovement()
    {
        float move = 0f;
        float rotation = 0f;

        if (Input.GetKey(KeyCode.W)) move = 1f;
        if (Input.GetKey(KeyCode.S)) move = -1f;
        if (Input.GetKey(KeyCode.A)) rotation = -1f;
        if (Input.GetKey(KeyCode.D)) rotation = 1f;

        // Rotace
        transform.Rotate(Vector3.up * rotation * rotationSpeed * Time.deltaTime);

        // Pohyb
        Vector3 forwardMove = transform.forward * move * moveSpeed;
        controller.Move(forwardMove * Time.deltaTime);

        // Animace + zvuk
        if (move > 0)
        {
            PlayAnimation(Animations.MiaRunWithRifle);
            PlayRunSound();
        }
        else if (move < 0)
        {
            PlayAnimation(Animations.MiaRunWithRifle);
        }
        else
        {
            PlayIdle();
        }
    }

    // =========================
    // MOUSE CLICK MOVEMENT
    // =========================

    private void HandleMouseClickMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                finalDestination = hit.point;
                hasDestination = true;
            }
        }

        if (hasDestination)
        {
            Vector3 direction = finalDestination - transform.position;
            direction.y = 0;

            float distance = direction.magnitude;

            if (distance > 0.2f)
            {
                direction.Normalize();
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(direction),
                    Time.deltaTime * 5f);
			
				//parameterName = "Is"+selectedAnimation;
                controller.Move(direction * sprintSpeed * Time.deltaTime);
				if(selectedAnimation.ToString()==Stand_animation) //selectedAnimation je enum, ne string
                PlayAnimation(Animations.MiaSprintForward);
				else if(selectedAnimation.ToString()==Crouch_animation)
                PlayAnimation(Animations.MiaCrouching);
				else if(selectedAnimation.ToString()==Crawl_animation)
                PlayAnimation(Animations.MiaProneForward);
				else if(selectedAnimation.ToString()==Shoot_animation)
                PlayAnimation(Animations.MiaFiringRifle);
				else
                PlayAnimation(Animations.MiaIdle);
                PlayRunSound();
            }
            else
            {
                hasDestination = false;
                PlayIdle();
                StopRunSound();
            }
        }
    }

    // =========================
    // ANIMATION
    // =========================

    private void PlayAnimation(Animations anim)
    {
        ResetAllParameters();
        animator.SetInteger("Is" + anim.ToString(), 1);
		animator.Play(anim.ToString());
		
		if(animator.GetCurrentAnimatorStateInfo(0).IsName("MiaSprintForward"))
		{
			float z_speed = 0;
			transform.Translate(Vector3.forward * z_speed * Time.deltaTime);
		}
    }

    private void PlayIdle()
    {
        ResetAllParameters();
        animator.SetInteger("IsMiaIdle", 1);
        StopRunSound();
    }

    private void ResetAllParameters()
    {
        foreach (Animations anim in Enum.GetValues(typeof(Animations)))
        {
            animator.SetInteger("Is" + anim.ToString(), 0);
        }
    }

    // =========================
    // AUDIO
    // =========================

    private void PlayRunSound()
    {
        if (runClip == null) return;

        if (!audioSource.isPlaying)
        {
            audioSource.clip = runClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void StopRunSound()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    // =========================
    // COORDINATES UI
    // =========================

    private void UpdateCoordinatesUI()
    {
        if (infoCoordinates == null) return;

        float distance = hasDestination
            ? Vector3.Distance(transform.position, finalDestination)
            : 0f;

        infoCoordinates.text =
            "x: " + transform.position.x.ToString("F2") + "\n" +
            "y: " + transform.position.y.ToString("F2") + "\n" +
            "z: " + transform.position.z.ToString("F2") + "\n" +
            "angle.y: " + transform.eulerAngles.y.ToString("F2") + "\n" +
            "Distance: " + distance.ToString("F2");
    }
    // -----------------------
    // ANIMATION SYSTEM
    // -----------------------

    public void PlaySelectedAnimation()
    {
        ResetAllParameters();

        string parameterName = GetParameterName(selectedAnimation);
        animator.SetInteger(parameterName, 1);
		animator.Play(parameterName);
    }

    

    private string GetParameterName(Animations animation)
    {
        return "Is" + animation.ToString();
    }
}
