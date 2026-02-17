using UnityEngine;
using UnityEngine.Events;
using System;

public class BaseTalibEnemyAI : MonoBehaviour
{
    [Header("Animator Setup")]
    [SerializeField] public Animator animator;

    [Header("Current State")]
    [SerializeField] public Animations currentAnimation = Animations.TalibRunning;

    [Header("Events")]
    public UnityEvent<Animations> OnStateChanged;
	
	public int IndexedAction;
	public GameObject TargetedPlayer;

    // Seznam všech parametrů Animatoru (pořadí shora dolů z tvých screenshotů - kombinace obou)
    // Každý index odpovídá přesně indexu v Animations enumu
    public enum Parameters
    {
        IsTalibRunning,
        IsTalibShooting,
        IsTalibHiding,
        IsTalibJumping,
        IsTalibFalling,
        IsTalibThrowing,
        IsTalibDancing,
        IsTalibWalkingBackwards,
        IsTalibCrawlingForwards,
        IsTalibStandingUp,
        IsTalibDoingBackflip,
        IsTalibKneelDown,
        IsTalibKnockedOut,
        IsTalibPicksLWeapon,
        IsTalibProneForward,
        IsTalibShootingCrouch,
        IsTalibTurnsWithRifle,
        IsTalibStrafeLeft,
        IsTalibHitShoulder,
        IsTalibJumpAside,
        IsTalibJumpingAside,
        IsTalibStabbing,
        IsTalibFallingLikeZombie,
        IsTalibCallingOthers,
        IsTalibCrouchRunning,
        IsTalibWalkingRightSide,
        IsTalibDeathFallBack,
        IsTalibTossingGrenadeFromSt,
        IsTalibPickingObject,
        IsTalibFallingForward,
        IsTalibStandingUpFromDeath,
		IsTalibGetsHitKneeling,
		IsTalibStandingUp0
    }

    // Seznam všech animací (názvy stavů z pravé strany Animatoru - Entry -> [název])
    // INDEXY SÚ SYNCHRONIZOVANÉ S Parameters (změna indexu v jednom = změna v druhém)
    // Rozšířeno o VŠECHNY viditelné z tvých screenshotů (+ pár logických pro kompletnost)
    public enum Animations
    {
        TalibRunning,
        TalibShooting,
        TalibHiding,
        TalibJumping,
        TalibFalling,
        TalibThrowing,
        TalibDancing,
        TalibWalkingBackwards,
        TalibCrawlingForwards,
        TalibStandingUp,
        TalibDoingBackflip,
        TalibKneelDown,
        TalibKnockedOut,
        TalibPicksLWeapon,
        TalibProneForward,
        TalibShootingCrouch,
        TalibTurnsWithRifle,
        TalibStrafeLeft,
        TalibHitShoulder,
        TalibJumpAside,
        TalibJumpingAside,
        TalibStabbing,
        TalibFallingLikeZombie,
        TalibCallingOthers,
        TalibCrouchRunning,
        TalibWalkingRightSide,
        TalibDeathFallBack,
        TalibTossingGrenadeFromSt,
        TalibPickingObject,
        TalibFallingForward,
        TalibStandingUpFromDeath,
		TalibGetsHitKneeling,
		TalibStandsUp
    }

    public string[] parameterNames;
	public float Health = 100f;
	public UnityEngine.UI.Slider healthSlider;

	public void TakeDamage(float damage)
	{
    Health -= damage;
    Health = Mathf.Clamp(Health, 0f, 100f);

    if (healthSlider != null)
        healthSlider.value = Health / 100f;

    if (animator != null)
        animator.SetTrigger("Hit");

    if (Health <= 0)
        Die();
	}

	private void Die()
	{
    if (animator != null)
        animator.SetTrigger("Die");

    Destroy(gameObject, 3f);
	}
	
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // Automaticky načte názvy parametrů podle enumu Parameters (pořadí = indexy)
        parameterNames = Enum.GetNames(typeof(Parameters));
    }
	/*
    private void LateUpdate()
    {
        // SYNCHRONIZACE: Detekuje změnu v Animatoru a updatuje currentAnimation
        for (int i = 0; i < parameterNames.Length; i++)
        {
            if (animator.GetInteger(parameterNames[i]) > 0)
            {
                Animations newState = (Animations)i;
                if (currentAnimation != newState)
                {
                    currentAnimation = newState;
                    OnStateChanged?.Invoke(currentAnimation);
                }
                return; // První int parametr vyhrává (priorita podle pořadí v enumu)
            }
        }
    }*/
	
	public void UpdateIndexedAction(Animations state)
{
    // 1. Zjistíme index Animations
    int index = (int)state;

    // 2. Aktualizujeme IndexedAction
    IndexedAction = index;

    // 3. Aktualizujeme parametr v Animatoru
    // Váš enum Parameters je synchronizovaný s Animations
    string paramName = parameterNames[index];

    // Pokud je parametr typu int
    animator.SetInteger(paramName, 1);

    // Vypneme ostatní parametry (pokud používáte booly současně)
    for (int i = 0; i < parameterNames.Length; i++)
    {
        if (i != index)
            animator.SetInteger(parameterNames[i], 0); // nebo SetBool pokud používáte bool
    }

    // 4. Spustíme animaci přímo přes Play (pokud chcete)
    animator.Play(state.ToString());

    // 5. Aktualizujeme currentAnimation
    currentAnimation = state;

    // 6. Vyvoláme event
    OnStateChanged?.Invoke(state);
}


    // HLAVNÍ METODA: Nastaví animaci - automaticky true jen pro vybranou, false vše ostatní
    public void SetState(Animations state)
    {
        int targetIndex = (int)state;

        // Vypne VŠECHNY parametry
        for (int i = 0; i < parameterNames.Length; i++)
        {
            animator.SetInteger(parameterNames[i], 0);
        }

        currentAnimation = state;
        OnStateChanged?.Invoke(state);
    }

    // Pomocné metody pro AI logiku (příklady - rozšiř podle potřeby)
    public void PlayRunning() => SetState(Animations.TalibRunning);
    public void PlayShooting() => SetState(Animations.TalibShooting);
    public void PlayJumping() => SetState(Animations.TalibJumping);
    public void PlayCrawling() => SetState(Animations.TalibCrawlingForwards);
    public void PlayDeath() => SetState(Animations.TalibDeathFallBack);
    public void PlayGrenadeToss() => SetState(Animations.TalibTossingGrenadeFromSt);

    // Příklad jednoduché AI (volitelně - přidej do Update() podmínky)
    [Header("AI Settings (Example)")]
    [SerializeField] private Transform player;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float chaseRange = 10f;
	public Animations targetState; // nastaví se z TalibCoverAI
	
	[Header("Shooting")]
//[SerializeField] private Transform TargetedPlayer;
[SerializeField] private float fireCooldown = 2.5f;
[SerializeField] private float fireRange = 50f;
[SerializeField] private float beamDuration = 0.15f;

private float fireTimer;
public bool IsWeaponInHands;

[Header("Weapon")]
[SerializeField] private GameObject weaponPrefab;        // z projektu
[SerializeField] private Transform weaponHandSocket;     // bone ruky / empty
[SerializeField] private GameObject MuzzleFirePrefab;

[Header("Impact FX")]
[SerializeField] private GameObject DustParticleSystem;
[SerializeField] private GameObject BloodSplatter_ParticleSystem;

private GameObject currentWeapon;
private Transform muzzlePoint;
public bool IsDead;
//public bool IsWeaponInHands;

 private void Update()
{
    if (player == null) return;

    if (targetState != currentAnimation)
    {
        UpdateIndexedAction(targetState);
    }

    animator.SetInteger("IndexedAction", IndexedAction);

    if (targetState == Animations.TalibShooting)
    {
		 if (!IsWeaponInHands)
			IsWeaponInHands = AddWeaponToHands();
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireCooldown;
        }
    }
}
public bool AddWeaponToHands()
{
    if (weaponPrefab == null || weaponHandSocket == null)
    {
        Debug.LogWarning("Weapon prefab nebo HandSocket chybí");
        return false;
    }

    // Zbraň už existuje
    if (currentWeapon != null)
        return true;

    // 1. Instantiate zbraně
    currentWeapon = Instantiate(
        weaponPrefab,
        weaponHandSocket.position,
        weaponHandSocket.rotation,
        weaponHandSocket
    );

    currentWeapon.transform.localPosition = Vector3.zero;
    currentWeapon.transform.localRotation = Quaternion.identity;

    // 2. Vytvoření muzzle pointu
    CreateMuzzlePoint(currentWeapon.transform);

    return true;
}

void CreateMuzzlePoint(Transform weaponRoot)
{
    Bounds bounds = new Bounds(weaponRoot.position, Vector3.zero);
    Renderer[] renderers = weaponRoot.GetComponentsInChildren<Renderer>();

    foreach (Renderer r in renderers)
        bounds.Encapsulate(r.bounds);

    Vector3 muzzleWorldPos =
        bounds.center + weaponRoot.forward * bounds.extents.z;

    GameObject muzzle = new GameObject("muzzle_point");
    muzzle.transform.position = muzzleWorldPos;
    muzzle.transform.rotation = weaponRoot.rotation;
    muzzle.transform.SetParent(weaponRoot);

    muzzlePoint = muzzle.transform;
}

void DrawBeam(Vector3 start, Vector3 end)
{
    GameObject beamGO = new GameObject("EnemyBeam");
    LineRenderer lr = beamGO.AddComponent<LineRenderer>();

    lr.positionCount = 2;
    lr.SetPosition(0, start);
    lr.SetPosition(1, end);

    lr.startWidth = 0.05f;
    lr.endWidth = 0.02f;
    lr.material = new Material(Shader.Find("Unlit/Color"));
    lr.material.color = Color.yellow;

    Destroy(beamGO, beamDuration);
}

	public void Fire()
{
    if (TargetedPlayer == null) return;

    Vector3 origin = transform.position + Vector3.up * 1.5f; // zhruba výška zbraně
    Vector3 direction = (TargetedPlayer.transform.position + Vector3.up) - origin;
	direction += UnityEngine.Random.insideUnitSphere * 0.2f;
	RaycastHit hit;
    if (Physics.Raycast(origin, direction, out hit, fireRange))
    {
		

        // Vytvoření paprsku
        GameObject beamGO = new GameObject("EnemyBeam");
        LineRenderer lr = beamGO.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, hit.point);

        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f;

        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = Color.yellow;

        // Zásah hráče
        if (hit.collider.transform == TargetedPlayer)
        {
            Animator playerAnimator = TargetedPlayer.transform.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Hit"); // musí existovat v Animatoru hráče
            }
        }

        Destroy(beamGO, beamDuration);
    }
	
	 if (TargetedPlayer == null || muzzlePoint == null) return;

    // 🔥 muzzle flash
    if (MuzzleFirePrefab != null)
    {
        GameObject flash = Instantiate(
            MuzzleFirePrefab,
            muzzlePoint.position,
            muzzlePoint.rotation
        );
        Destroy(flash, 1.5f);
    }

    origin = muzzlePoint.position;
	direction = (TargetedPlayer.transform.position + Vector3.up) - origin;
    direction += UnityEngine.Random.insideUnitSphere * 0.2f;

    if (Physics.Raycast(origin, direction, out hit, fireRange))
    {
        // paprsek
        GameObject beamGO = new GameObject("EnemyBeam");
        LineRenderer lr = beamGO.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, hit.point);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = Color.yellow;

        Destroy(beamGO, beamDuration);

        // 💥 impact FX
        SpawnImpactEffect(hit);

        // 👤 zásah hráče
        if (hit.collider.CompareTag("Player"))
        {
            Animator playerAnimator = hit.collider.GetComponent<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Hit");
        }
    }

    // 🔥 muzzle flash
    if (MuzzleFirePrefab != null)
    {
        GameObject flash = Instantiate(
            MuzzleFirePrefab,
            muzzlePoint.position,
            muzzlePoint.rotation
        );

        Destroy(flash, 1.5f);
    }

    origin = muzzlePoint.position;
	direction = (TargetedPlayer.transform.position + Vector3.up) - origin;

    if (Physics.Raycast(origin, direction, out hit, fireRange))
    {
       // DrawBeam(origin, hit.point);

        if (hit.collider.transform == TargetedPlayer.transform)
        {
            Animator playerAnimator = TargetedPlayer.transform.GetComponent<Animator>();
            if (playerAnimator != null)
                playerAnimator.SetTrigger("Hit");
        }
    }
}
void SpawnImpactEffect(RaycastHit hit)
{
    GameObject fxPrefab = null;

    if (hit.collider.CompareTag("Player"))
    {
        fxPrefab = BloodSplatter_ParticleSystem;
    }
    else if (
        hit.collider.CompareTag("Wall") ||
        hit.collider.CompareTag("car") ||
        hit.collider.CompareTag("vehicle") ||
        hit.collider.CompareTag("building")
    )
    {
        fxPrefab = DustParticleSystem;
    }

    if (fxPrefab == null)
        return;

    GameObject fx = Instantiate(
        fxPrefab,
        hit.point,
        Quaternion.LookRotation(hit.normal)
    );

    Destroy(fx, 3f);
}

    // Debug: Vypíše aktuální stav
    private void OnGUI()
    {
        GUILayout.Label($"Current Animation: {currentAnimation}");
    }
}