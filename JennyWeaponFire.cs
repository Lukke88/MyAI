using UnityEngine;
using System.Collections.Generic;
public class JennyWeaponFire : MonoBehaviour
{
    public GameObject jenny;

   public  GameObject muzzle_fire_point, gun;

   public  GameObject beam_original, muzzleShotOriginal, beam_instantiated;
    public ParticleSystem muzzleShotInstantiated, activeMuzzleFlash;

    public float fireRange = 200f;
    public float beamSpeed = 120f;
	public Animator animator;
	public bool beam_fired;
	// 1. Enum pro animation parametry (bool parametry podle tvého Animatoru)
    private enum AnimationParameter
    {
        IsRunning,
        IsJennyShootingStand,
        IsJennyShootingCrouch,
        IsJennyWalkingCrouch,
        IsJennyStandingUp,
        IsJennyIdle,
        IsJennyFalls,
        IsJennyCreepWalking,
        // případně další, které ještě přidáš později
    }

    // 2. Enum pro názvy animací (jen pro přehlednost a prevenci překlepů)
    private enum AnimationClip
    {
        JennyRun,
        JennyShootsStand,
        JennyShoot,             // pokud máš i tuto
        JennyWalk,
        JennyIdle,
        JennyIdle1,
        JennyHides,
        JennyHidesCrouch,
        JennyFalls,
        JennyFallsToGround,
        JennyStandsUp,
        JennyCreepWalk,
        JennyHitStand,
        // přidej další podle potřeby
    }

    void Start()
    {
        gun = jenny.transform
            .GetChild(0)
            .GetChild(0)
            .GetChild(0)
            .GetChild(16)
            .gameObject;

        muzzle_fire_point = gun.transform.GetChild(0).gameObject;

     //   
    }

    void Update()
    {
		beam_original = GameObject.Find("ppfxBeamElectric");
		muzzleShotOriginal = GameObject.Find("WFX_MF 4P RIFLE1");
		jenny = GameObject.Find(this.name);
		muzzle_fire_point = gun.transform.GetChild(0).gameObject;
		animator = jenny.transform.GetComponent<Animator>();
        HandleAimingRotation();
	//	CleanExtraRifleClones();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
		if(beam_fired==true && beam_instantiated!=null)
			beam_instantiated.transform.Translate(0,0,10.1f);
    }
	public GameObject muzzleGO;
	public Vector3 targetPoint;
	[Header("Beam Rotation Debug")]
[SerializeField] private bool rotateX90 = true;
[SerializeField] private bool rotateXMinus90 = false;
[SerializeField] private bool rotateY90 = false;
[SerializeField] private float customX = 0f;
[SerializeField] private float customY = 0f;
[SerializeField] private float customZ = 0f;

private Quaternion GetBeamRotation(Vector3 direction)
{
    Quaternion lookRotation = Quaternion.LookRotation(direction);
    
    Quaternion localRotation = Quaternion.Euler(
        rotateX90 ? 90f : (rotateXMinus90 ? -90f : customX),
        rotateY90 ? 90f : customY,
        customZ
    );
    
    return lookRotation * localRotation;
}


    void Fire()
    {
		
		beam_fired = true;
		// 🔴 raycast
        Ray ray = new Ray(
            muzzle_fire_point.transform.position,
            muzzle_fire_point.transform.forward
        );
       RaycastHit hit;
        
		muzzleGO = Instantiate(
            muzzleShotOriginal,
            muzzle_fire_point.transform.position,
            muzzle_fire_point.transform.rotation
        );
		// 🔥 muzzle flash
		activeMuzzleFlash = muzzleGO.GetComponent<ParticleSystem>();
		activeMuzzleFlash.Play();
         // ⚡ beam
        /*beam_instantiated = Instantiate(
            beam_original,
            muzzle_fire_point.transform.position,
            Quaternion.LookRotation(targetPoint - muzzle_fire_point.transform.position)
        );*/
		Rigidbody rb = beam_instantiated.GetComponent<Rigidbody>();
        // Pak v Fire() metodě:
		Vector3 direction = targetPoint - muzzle_fire_point.transform.position;
		Quaternion beamRotation = GetBeamRotation(direction);

        beam_instantiated = Instantiate(beam_original, muzzle_fire_point.transform.position, beamRotation);

        if (Physics.Raycast(ray, out hit, fireRange))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 0.5f);

            targetPoint = hit.point;

            if (hit.collider.CompareTag("Enemy"))
{
    GameObject enemy = hit.collider.gameObject;

    // 1. Zajistíme, že má Rigidbody (pokud ne, přidáme ho)
    rb = enemy.GetComponent<Rigidbody>();
    if (rb == null)
    {
        rb = enemy.AddComponent<Rigidbody>();
        rb.mass = 60f;                  // realistická hmotnost člověka
        rb.drag = 0.5f;                 // trochu zpomalení ve vzduchu
        rb.angularDrag = 0.5f;
        rb.useGravity = true;           // gravitace musí být zapnutá
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // proti propadání
    }

    // 2. Pokud máš beam (laserový paprsek) a je aktivní → posuneme nepřítele směrem k němu
    if (beam_instantiated != null)
    {
        // Směr od nepřítele k beamu
        Vector3 directionToBeam = (beam_instantiated.transform.position - enemy.transform.position).normalized;

        // Síla – uprav hodnotu podle potřeby (10–50 je často dobrý start)
        float knockbackForce = 25f;

        // Aplikujeme sílu – trochu nahoru + směrem k beamu, aby to vypadalo jako „odražení“
        Vector3 force = directionToBeam * knockbackForce + Vector3.up * 8f;
        rb.AddForce(force, ForceMode.Impulse);

        // Alternativa: plynulý pohyb směrem k beamu (méně fyzikální, ale kontrolovanější)
        // StartCoroutine(MoveEnemyTowardBeam(enemy.transform, beam_instantiated.transform.position));
    }
    else
    {
        // Pokud beam neexistuje → jen obyčejný knockback směrem od tebe (odraz od střely/laseru)
        Vector3 directionAway = (enemy.transform.position - transform.position).normalized;
        rb.AddForce(directionAway * 18f + Vector3.up * 6f, ForceMode.Impulse);
    }

    // Volitelné – další efekty
    // enemy.GetComponent<EnemyHealth>()?.TakeDamage(35f);
    // PlayHitSoundOrParticle(hit.point);
}
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * fireRange, Color.red, 0.5f);
            targetPoint = ray.origin + ray.direction * fireRange;
        }

       

        
        if (rb != null)
        {
            rb.velocity = beam_instantiated.transform.forward * beamSpeed;
        }
	//	CleanExtraRifleClones();
		Destroy(muzzleGO, 2f);
        Destroy(beam_instantiated, 2f);
    }

        private void HandleAimingRotation()
    {
        // Stisknutý levý Ctrl → míření vestoje
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // Spustíme střeleckou animaci vestoje
            SetFloat(AnimationParameter.IsJennyShootingStand, 1.0f);

            // Volitelně vypneme jiné konfliktní stavy (podle tvé logiky)
            // SetBool(AnimationParameter.IsRunning, false);
            // SetBool(AnimationParameter.IsJennyIdle, false);
            // atd.
        }

        // Puštění Left Ctrl → vracíme se do normálu (např. idle / run)
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            SetFloat(AnimationParameter.IsJennyShootingStand, 1.0f);

            // Případně zde můžeš rozhodnout, jestli přejde do Idle, Walk apod.
            // animator.SetTrigger("ToIdle");  // pokud bys používal triggery
        }

        // Bonus – natáčení postavy podle myši/směru kamery (během míření)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            RotateCharacterToMouse();
        }
    }
	[SerializeField] private Transform muzzleFirePoint;   // ← přetáhni sem muzzle_fire_point z inspektoru
    [SerializeField] private float maxDistanceToKeep = 0.5f;
	public List<GameObject> rifleClones = new List<GameObject>();
    // Zavolej tuto metodu např. v Start(), po Instantiate zbraně, nebo když chceš čistit
    public void CleanExtraRifleClones()
    {
        if (muzzleFirePoint == null)
        {
            Debug.LogWarning("Muzzle fire point není přiřazený!");
            return;
        }

        // Najdeme všechny objekty ve scéně
        GameObject[] allGOs = FindObjectsOfType<GameObject>();

        

        foreach (var go in allGOs)
        {
            if (go == null) continue;
            if (go.name.Contains("RIFLE1(Clone)"))   // nebo přesně "WFX_MF 4P RIFLE1(Clone)"
            {
                rifleClones.Add(go);
            }
        }

        if (rifleClones.Count <= 1)
        {
            Debug.Log("Nenalezeno více klonů → nic nemažu");
            return;
        }

        // Najdeme ten nejblíže k muzzle
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var clone in rifleClones)
        {
            float dist = Vector3.Distance(clone.transform.position, muzzleFirePoint.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = clone;
            }
        }

        // Mažeme všechny kromě nejbližšího (a jen pokud je blízko)
        int destroyed = 0;
        foreach (var clone in rifleClones)
        {
            if (clone == closest) continue;

            float dist = Vector3.Distance(clone.transform.position, muzzleFirePoint.position);
            if (dist > maxDistanceToKeep)
            {
                Destroy(clone);
                destroyed++;
            }
        }

        Debug.Log($"Zničeno {destroyed} extra klonů RIFLE1(Clone). Nejblíže zůstal: {closest?.name}");
    }
    private void RotateCharacterToMouse()
    {
        // Velmi jednoduchá verze – natáčí postavu podle směru kamery (plane y=0)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPoint = ray.GetPoint(distance);
            Vector3 direction = (targetPoint - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 12f   // rychlost otáčení – uprav podle potřeby
                );
            }
        }
    }

    // Pomocná metoda – čitelnější volání SetBool
    private void SetFloat(AnimationParameter param, float value)
    {
        animator.SetFloat(param.ToString(), value);
    }

    // Volitelně i pomocná pro triggery, pokud je budeš později potřebovat
    private void SetTrigger(AnimationClip clip)
    {
        animator.SetTrigger(clip.ToString());
    }
}
