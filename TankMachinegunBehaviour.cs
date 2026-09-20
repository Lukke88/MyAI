using System.Collections;
using UnityEngine;

public class TankMachinegunBehaviour : MonoBehaviour
{
    [Header("References")]
    public GameObject machinegun, enemy;
    public GameObject terrain;
	[Header("Enemy animation parameters")]
	public string running_animation = "";
	public string running_anim_param = "";//for enemy
	public ParticleSystem bloodSplatter;
	public string pa6rt_system_name = "WFX_Bloodsplatter";

    [Header("Weapon states")]
    public bool IsMachinegunActivated;
    public bool IsCannonActivated;
    public bool IsAllowedToShoot;
	public AudioClip aargh_enemy;
    [Header("Machinegun range")]
    public float mg_min_range = 300.0f;
    public float mg_max_range = 4000.0f;

    [Header("Machinegun fan")]
    public float vejir_size = 45.0f;

    // Number of individual shots in one fan.
    public int numberOfHitPoints = 20;

    // Distance between machinegun and each hit point.
    public float range = 4000.0f;

    [Header("Timing")]
    public float delayBetweenShots = 0.05f;

    [Header("Effects")]
    public GameObject dustEffect;

    [Header("Enemy reaction")]
    public string enemyHitTrigger = "EnemyHit";

    [Header("Runtime information")]
    public float terrain_height;
    public Vector3 hit_point;

    public Vector3[] ground_hit_points;

    public int hitpoint_index;
    public int currentHitPoint;

    private bool isShooting;
    private Coroutine shootingCoroutine;

	public GameObject[] allEnemies, allBarricades, allWalls;
    void Start()
    {
        machinegun = this.gameObject;

        terrain = GameObject.Find("Plane");

        if (terrain != null)
        {
            terrain_height = terrain.transform.position.y;
        }

        ground_hit_points = new Vector3[numberOfHitPoints];
		
		allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
		allBarricades = GameObject.FindGameObjectsWithTag("Barricade");
		allWalls = GameObject.FindGameObjectsWithTag("Wall");
    }


    void Update()
    {
        // --------------------------------------------------
        // WEAPON SWITCHING
        // --------------------------------------------------

        if (Input.GetKeyDown(KeyCode.M))
        {
            IsMachinegunActivated = true;
            IsCannonActivated = false;

            Debug.Log("Machinegun activated.");
        }

        else if (Input.GetKeyDown(KeyCode.K))
        {
            IsCannonActivated = true;
            IsMachinegunActivated = false;

            Debug.Log("Cannon activated.");
        }


        // --------------------------------------------------
        // MACHINEGUN AIMING
        // --------------------------------------------------

        if (IsMachinegunActivated == false)
        {
            IsAllowedToShoot = false;
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 6000.0f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                hit_point = hit.point;
            }
        }


        // --------------------------------------------------
        // RANGE CHECK
        // --------------------------------------------------

        float Distance_from_machinegun =
            Vector3.Distance(machinegun.transform.position, hit_point);

        IsAllowedToShoot =
            Distance_from_machinegun > mg_min_range &&
            Distance_from_machinegun < mg_max_range;


        // --------------------------------------------------
        // FIRE MACHINEGUN FAN
        // --------------------------------------------------

        // Left mouse button = fire one complete fan.
        if (Input.GetMouseButtonDown(0))
        {
            if (IsAllowedToShoot == true && isShooting == false)
            {
                CreateMachinegunFan();
            }
        }
		
		if(IsEnemyMovingToCover==true && Vector3.Distance(enemy.transform.position, selected_cover.transform.position)>20.0f)
		{
			//newPosition is global variable
			enemy_object.transform.position =
		Vector3.MoveTowards(
        enemy.transform.position,
        newPosition,
        enemy_speed * Time.deltaTime);
		}
		else if(Vector3.Distance(enemy.transform.position, selected_cover.transform.position)<=20.0f)
		{
			//can start shooting
			IsEnemyMovingToCover = false;
			selected_cover = null;
			newPosition = Vector3.zero;
		}
    }


    // ======================================================
    // CREATE FAN OF HIT POINTS
    // ======================================================

    public void CreateMachinegunFan()
    {
        hitpoint_index = 0;
        currentHitPoint = 0;

        float basic_angle = machinegun.transform.eulerAngles.y;

        float min_angle = basic_angle - vejir_size;
        float max_angle = basic_angle + vejir_size;

        float angle_step = 0.0f;

        if (numberOfHitPoints > 1)
        {
            angle_step =
                (max_angle - min_angle) / (numberOfHitPoints - 1);
        }

        for (int i = 0; i < numberOfHitPoints; i++)
        {
            float current_angle = min_angle + angle_step * i;

            float radians = current_angle * Mathf.Deg2Rad;

            Vector3 misto_zasahu = new Vector3(
                machinegun.transform.position.x +
                range * Mathf.Sin(radians),

                terrain_height,

                machinegun.transform.position.z +
                range * Mathf.Cos(radians)
            );

            ground_hit_points[hitpoint_index] = misto_zasahu;

            hitpoint_index++;
        }

        shootingCoroutine = StartCoroutine(ShootToHitPoints());
    }


    // ======================================================
    // SHOOT TO ALL HIT POINTS
    // ======================================================

    public IEnumerator ShootToHitPoints()
    {
        isShooting = true;

        currentHitPoint = 0;

        while (currentHitPoint < ground_hit_points.Length)
        {
            Vector3 targetPoint =
                ground_hit_points[currentHitPoint];

            Vector3 direction =
                (targetPoint - machinegun.transform.position).normalized;

            Ray ray = new Ray(
                machinegun.transform.position,
                direction
            );

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5000.0f))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    PlayEnemyHitAnimation(hit.collider.gameObject);
                }

                else if (hit.collider.CompareTag("Ground"))
                {
                    PlayDustHit(hit.point);
                }
            }

            currentHitPoint++;

            yield return new WaitForSeconds(delayBetweenShots);
        }

        isShooting = false;
        shootingCoroutine = null;
    }


    // ======================================================
    // ENEMY HIT
    // ======================================================
	public bool IsEnemyMovingToCover;
    public void PlayEnemyHitAnimation(GameObject enemy)
    {
        Animator animator = enemy.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger(enemyHitTrigger);
        }

        Debug.Log("Machinegun hit enemy: " + enemy.name);
		
		bloodSplatter = enemy.transform.GetComponent<ParticleSystem>();
		bloodSplatter.Play();
		
		AudioSource audioSource =
		enemy.transform.GetComponent<AudioSource>();

		if (audioSource != null)
		{
			audioSource.PlayOneShot("AArgh");
		}
		enemy.transform.GetComponent<EnemyHealth>().GetDamage(5.0f);//decreases enemy life
		if(IsEnemyMovingToCover==false) // enemy get hit -> search hideout
		MakeEnemySearchCover(enemy, running_animation, running_anim_param);
    }
	GameObject selected_cover;
	public Vector3 newPosition;//global variable
	public void MakeEnemySearchCover(GameObject enemy_object, string enemy_animation_name, string enemy_anim_param_name)
	{
		float enemy_speed =30.0f;
		allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
		allBarricades = GameObject.FindGameObjectsWithTag("Barricade");
		allWalls = GameObject.FindGameObjectsWithTag("Wall");
		
		float max_distance = 600.0f;
		GameObject selected_cover = null;
		float closest_distance = Mathf.Infinity;

		foreach (GameObject go in allBarricades)
	{
    float distance = Vector3.Distance(
        go.transform.position,
        enemy_object.transform.position
    );

    if (distance < max_distance && distance < closest_distance)
    {
        closest_distance = distance;
        selected_cover = go;
    }
}
		
		if(selected_cover!=null && enemy_object!=null)
		{
			float z_offset = 20.0f;
			newPosition = new Vector3(selected_cover.transform.position.x, enemy_object.transform.position.y, selected_cover.transform.position.z + z_offset);
			enemy_object.transform.position = Vector3.MoveTowards(enemy.transform.position, newPosition, enemy_speed*Time.deltaTime);
			
			Animator anim = enemy.transform.GetComponent<Animator>();
			anim.SetInteger(enemy_anim_param_name, 1);
			anim.Play(enemy_animation_name);
		}
	}


    // ======================================================
    // DUST HIT
    // ======================================================

    public void PlayDustHit(Vector3 position)
    {
        if (dustEffect != null)
        {
            Instantiate(
                dustEffect,
                position,
                Quaternion.identity
            );
        }
    }
}