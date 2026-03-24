using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class EnemyAIMoveAndShoot : MonoBehaviour
{
    [Header("Movement")]
    public float moveRadius = 30f;
    public float stopTime = 1.0f;

    [Header("Combat")]
    public float fireRate = 1.0f;
    public float projectileSpeed = 20f;
    public float spreadAngle = 5f;

    [Header("Health")]
    public float health = 100f;

    [Header("Animation")]
    public string hideStateName = "TalibCrouching";
    public string shootStateName = "TalibShooting";
	public string runStateName = "TalibRunning";
    public string animatorParameter = "State"; // můžeš změnit

    [Header("UI")]
    public Canvas enemyCanvas;
    public TextMeshProUGUI descriptionText;

   public NavMeshAgent agent;
    private Animator animator;
    private GameObject nearestPlayer;

    private float stateTimer;
    private float fireTimer;

    private bool isHiding;
	
	public Transform weaponSocket;
	public GameObject weaponPrefab;
	public ParticleSystem muzzleFlash;
	
	[Header("Line Flash")]
	public float lineDistance = 20f;
	public float lineLifeTime = 0.15f;
	public float lineSpawnRandomTick = 0.05f;
	public LineRenderer lineFlash;

    public enum State { Moving, Shooting }
    public State currentState;
	
	public enum Parameters
{
    IsTalibRunning,
    IsTalibShooting,
    IsTalibHiding,
    IsTalibJumping,
    IsTalibFalling,
    IsTalibThrowing,
    IsTalibDancing,
    IsTalibPicking,
    IsTalibWalkingBackwards,
    IsTalibCrawlingForwards,
    IsTalibStandingUp,
    IsTalibDoingKormelec,
    IsTalibStartsSprinting,
    IsTalibCrawlingBackwards,
    IsTalibDoingBackflip,
    IsTalibJumpingUp,
    IsTalibKneelingDown,
    IsTalibKnockedOut,
    IsTalibJumpsLeft,
    IsTalibPickingWeapon,
    IsTalibProningForward,
    IsTalibShootingCrouch
}

public enum Animations
{
    TalibRunning,
    TalibShooting,
    TalibHiding,
    TalibJumping,
    TalibFalling,
    TalibThrowing,
    TalibDancing,
    TalibPicking,
    TalibWalkingBackwards,
    TalibCrawlingForwards,
    TalibStandingUp,
    TalibDoingKormelec,
    TalibStartsSprinting,
    TalibCrawlingBackwards,
    TalibDoingBackflip,
    TalibJumpingUp,
    TalibKneelingDown,
    TalibKnockedOut,
    TalibJumpsLeft,
    TalibPickingWeapon,
    TalibProningForward,
    TalibShootingCrouch
}

    Transform muzzleGenerated = null;

void Start()
{
    agent = GetComponent<NavMeshAgent>();
    animator = GetComponent<Animator>();

    if (enemyCanvas != null)
        enemyCanvas.enabled = false;

    PickNewPosition();
    currentState = State.Moving;

    if (weaponPrefab != null && weaponSocket != null)
    {
        GameObject weapon = Instantiate(weaponPrefab, weaponSocket);

        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;

        // --- vytvoření muzzle_generated ---
        GameObject muzzleObj = new GameObject("muzzle_generated");
        muzzleObj.transform.SetParent(weapon.transform);
        muzzleObj.transform.localPosition = Vector3.zero;

        muzzleGenerated = muzzleObj.transform;
    }
}

    void Update()
    {
        FindNearestPlayer();

        if (currentState == State.Moving)
		{
			if (animator != null)
			{
				animator.SetInteger(animatorParameter, (int)Parameters.IsTalibRunning);
				animator.Play(runStateName);
			}

			agent.isStopped = false;

		if (!agent.pathPending && agent.remainingDistance < 0.5f)
			{
        agent.isStopped = true;
        stateTimer = stopTime;

			if (animator != null)
			{
				animator.SetInteger(animatorParameter, (int)Parameters.IsTalibShooting);
				animator.Play(shootStateName);
			}

        currentState = State.Shooting;
			}
		}
        else if (currentState == State.Shooting)
        {
            if (nearestPlayer != null && !isHiding)
                RotateToTarget();

            fireTimer += Time.deltaTime;

            if (fireTimer >= fireRate && !isHiding)
            {
                fireTimer = 0f;
                Shoot();
            }

            // Náhodné krytí pokud je u zdi
            if (IsNearWall(out _) && Random.value < 0.01f)
            {
                ToggleHide(true);
            }

            stateTimer -= Time.deltaTime;

            if (stateTimer <= 0)
            {
                ToggleHide(false);
                agent.isStopped = false;
                PickNewPosition();
                currentState = State.Moving;
            }
        }
    }

    // -------------------------
    // KRYTÍ
    // -------------------------
    void ToggleHide(bool hide)
    {
        isHiding = hide;

        if (animator != null)
        {
            if (hide)
                animator.Play(hideStateName);
            else
                animator.Play(shootStateName);
        }
    }

    // -------------------------
    // HEALTH / DAMAGE
    // -------------------------
    public void TakeDamage(float amount)
    {
        health -= amount;

        ShowCanvas("enemy lost 5%...");

        if (health <= 0)
        {
            ShowCanvas("Bonus !!! 100 points !!!");
            Destroy(gameObject, 1.5f);
        }
    }

    void ShowCanvas(string message)
    {
        if (enemyCanvas == null || descriptionText == null)
            return;

        enemyCanvas.enabled = true;

        descriptionText.text = message;
        descriptionText.color = Color.green;

        CancelInvoke(nameof(HideCanvas));
        Invoke(nameof(HideCanvas), 1.5f);
    }

    void HideCanvas()
    {
        if (enemyCanvas != null)
            enemyCanvas.enabled = false;
    }

    // -------------------------
    // TARGET
    // -------------------------
    void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float shortest = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < shortest)
            {
                shortest = dist;
                closest = p;
            }
        }

        nearestPlayer = closest;
    }

    void RotateToTarget()
    {
        Vector3 dir = (nearestPlayer.transform.position - transform.position).normalized;
        dir.y = 0;
		if (animator != null)
		{
			
			animator.SetInteger(animatorParameter, 1);
			animator.Play(shootStateName);
		}
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    // -------------------------
    // STŘELBA
    // -------------------------
    void Shoot()
{
    if (nearestPlayer == null) return;
	
	if (animator != null)
	{
		animator.SetInteger(animatorParameter, 1);
		animator.Play(shootStateName);
	}

    if (muzzleFlash != null)
        muzzleFlash.Play();

    Vector3 shootOrigin = muzzleGenerated != null
    ? muzzleGenerated.position
    : weaponSocket.position;
	
    Vector3 dir = (nearestPlayer.transform.position - shootOrigin).normalized;

    float randomAngle = Random.Range(-spreadAngle, spreadAngle);
    dir = Quaternion.Euler(0, randomAngle, 0) * dir;

    // --- Raycast hit ---
    RaycastHit hit;
    Vector3 hitPoint = shootOrigin + dir * lineDistance;

    bool hitPlayer = false;

    if (Physics.Raycast(shootOrigin, dir, out hit, lineDistance))
    {
        hitPoint = hit.point;

        if (hit.collider.CompareTag("Player"))
        {
            hitPlayer = true;
        }
    }
	
	if (Application.isEditor && nearestPlayer != null && muzzleGenerated != null)
	{
    Debug.DrawLine(
        muzzleGenerated.position,
        nearestPlayer.transform.position,
        Color.red
    );
	}

    // Spawn tracer beam
    StartCoroutine(SpawnLineFlash(shootOrigin, hitPoint, hitPlayer));
	
	StartCoroutine(ResetShootAnim());
}
System.Collections.IEnumerator ResetShootAnim()
{
    yield return new WaitForSeconds(0.2f);

    if (animator != null)
        animator.SetInteger(animatorParameter, 0);
}
System.Collections.IEnumerator SpawnLineFlash(Vector3 start, Vector3 end, bool hitPlayer)
{
    // náhodný tick delay
    yield return new WaitForSeconds(Random.Range(0f, lineSpawnRandomTick));

    GameObject lineObj = new GameObject("LineFlash");

    LineRenderer lr = lineObj.AddComponent<LineRenderer>();

    lr.positionCount = 2;
    lr.startWidth = 0.1f;
    lr.endWidth = 0.05f;

    lr.material = new Material(Shader.Find("Sprites/Default"));

    lr.startColor = hitPlayer ? Color.red : Color.yellow;
    lr.endColor = hitPlayer ? Color.red : Color.yellow;

    lr.SetPosition(0, start);
    lr.SetPosition(1, end);

    Destroy(lineObj, lineLifeTime);
}

    // -------------------------
    // POHYB
    // -------------------------
    void PickNewPosition()
    {
        Vector3 newPos = transform.position;

        if (IsNearWall(out Vector3 wallSize))
        {
            if (wallSize.z > wallSize.x)
            {
                float randomZ = Random.Range(-moveRadius, moveRadius);
                newPos += new Vector3(0, 0, randomZ);
            }
            else
            {
                float randomX = Random.Range(-moveRadius, moveRadius);
                newPos += new Vector3(randomX, 0, 0);
            }
        }
        else
        {
            Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
            newPos += new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPos, out hit, moveRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    bool IsNearWall(out Vector3 wallSize)
    {
        wallSize = Vector3.zero;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                wallSize = hit.collider.bounds.size;
                return true;
            }
        }

        return false;
    }
}