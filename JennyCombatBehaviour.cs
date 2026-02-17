using UnityEngine;

public class JennyCombatBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject jenny;           // ← sem přiřadíš Jenny (nebo necháš this.gameObject)
    private Animator animator;
	public GameObject beam_original, new_beam_instantiated, muzzle_fire_point, gun;
	public ParticleSystem muzzleShotOriginal, muzzleShot_instantiated;
    // Pokud nechceš přetahovat Jenny v inspektoru, stačí použít this.gameObject
    // → pak můžeš řádek [SerializeField] private GameObject jenny; smazat

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

    void Awake()
    {
        // Nejčastější způsob – skript je přímo na Jenny → používáme this
        if (jenny == null)
        {
            jenny = this.gameObject;
        }

        animator = jenny.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator na Jenny nebyl nalezen!", jenny);
        }
    }

    void Update()
    {
		//gun = jenny.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(16).gameObject;
		//muzzle_fire_point = gun.transform.GetChild(0).gameObject;
		//beam_original = GameObject.Find("ppfxBeamElectric");
		//muzzleShotOriginal = GameObject.Find("WFX_MF 4P RIFLE1");
        HandleAimingRotation();
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