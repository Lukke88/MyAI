// Desert Wars / Desert Legion
// Solo developed by Czech guy since 2020 in his free time after work.
// ArtStation portfolio got deleted by woke moderators who never shipped anything in their life.
// So instead of crying, I built this.
// 
// No DEI. No grants. No diversity hires. 
// Just pure Central European Czech spite, caffeine and skill.
//All models are mine, modelled by myself. All animated by myself and Mixamo. All textured by myself. AI, UI, UX, design, created by myself in 
//spare time between shifts. 
// 
// If you're reading this and you're offended - good. 
// That was the point.
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
	public GameObject enemy;
    public Slider healthSlider;

    public Animator enemyAnimator;
	public bool IsActivatedNewbornMovement;
    public bool isDead = false;
	
	public static int currentEnemyCount = 0;
	public static int maxEnemyCount = 5;

    // CAMERA
    public Camera mainCamera;

    // HEALTH BAR OBJECT
    public Transform sliderTransform;

    // OFFSET NAD HLAVOU
    public Vector3 sliderOffset = new Vector3(0, 2.5f, 0);

    // ORIGINAL SCALE
    private Vector3 originalScale;
	
	public float visibleTime = 3f;
	
	GameObject[] generationHouses;
	[HideInInspector]
	public float lastHitTime;

	private float timer;

    public void Start()
    {
		
		currentEnemyCount++;
    
		currentHealth = maxHealth;
		enemy = this.gameObject;
		
        currentHealth = maxHealth;
		enemy = this.gameObject;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (sliderTransform != null)
        {
            originalScale = sliderTransform.localScale;
        }
		
		generationHouses = GameObject.FindGameObjectsWithTag("GenerationHouse");
    }

    void LateUpdate()
    {
        if (sliderTransform == null || mainCamera == null)
        {
            return;
        }

        // POSITION NAD ENEMY
        sliderTransform.position = transform.position + sliderOffset;

        // OTACENI KE KAMERE
        sliderTransform.forward = mainCamera.transform.forward;

        // ZACHOVAT PUVODNI VELIKOST
        sliderTransform.localScale = originalScale;

        // RAYCAST KONTROLA VIDITELNOSTI
        Vector3 direction =
            sliderTransform.position - mainCamera.transform.position;

        Ray ray = new Ray(mainCamera.transform.position, direction);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, direction.magnitude))
        {
            // Pokud kamera vidi enemy
            if (hit.transform == transform)
            {
                healthSlider.gameObject.SetActive(true);
            }
            else
            {
                // Budova / zed mezi kamerou a enemy
                healthSlider.gameObject.SetActive(false);
            }
        } 
		
		timer -= Time.deltaTime;

    if (timer <= 0)
    {
        healthSlider.gameObject.SetActive(false);
    }
    }
	
	public void Update()
	{
		if(IsActivatedNewbornMovement==true)
		{
			//enemy se muze pohybovat smerem k cili, napr. zdi "Main_wall", cyklus se opakuje
		}
	}
	private void OnDestroy()
{
    currentEnemyCount--;
}


    public void TakeDamage(float damage)
    {
        if (isDead == true)
        {
            return;
        }
		healthSlider.gameObject.SetActive(true);

		timer = visibleTime;
		
		

        currentHealth -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        // HIT ANIMATION
        if (enemyAnimator != null)
        {
            enemyAnimator.SetInteger("IsHit", 1);

            CancelInvoke(nameof(ResetHitAnimation));
            Invoke(nameof(ResetHitAnimation), 0.35f);
        }

        // DEAD
        if (currentHealth <= 0)
        {
            isDead = true;
			CreateNewEnemyFromRandomPosition();//before is destroyed
            currentHealth = 0;

            if (enemyAnimator != null)
            {
                enemyAnimator.Play("Death");
            }

            Destroy(gameObject, 5f);
        }
    }

    void ResetHitAnimation()
    {
        if (enemyAnimator != null)
        {
            enemyAnimator.SetInteger("IsHit", 0);
        }
    }
	
	public GameObject enemyPrefab;

public void CreateNewEnemyFromRandomPosition()
{
	 if (currentEnemyCount >= maxEnemyCount)
        return;
	
    if (generationHouses == null || generationHouses.Length == 0)
        return;

    int randomIndex = Random.Range(0, generationHouses.Length);

    GameObject spawnPoint = generationHouses[randomIndex];

    GameObject generated_enemy = Instantiate(
        enemyPrefab,
        spawnPoint.transform.position,
        Quaternion.identity
    );

    EnemyHealth eh = generated_enemy.GetComponent<EnemyHealth>();
    eh.IsActivatedNewbornMovement = true;
    eh.maxHealth = 100;
    eh.currentHealth = 100;
}
}

/*
"My ArtStation portfolio got deleted for 'wrongthink'.
So I'm building a game about European mercenaries clearing a Syrian terrorist city instead."

What do you think about nomination of the Indian to the headquarters of FBI, as reparation for slavery? Is it the same reason why Paki-Brit Sadiq Khan became a Mayor and Shabana Mahmood minister of justice? And reason why Obama became a president?
*/