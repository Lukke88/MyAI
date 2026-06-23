using UnityEngine;

public class TankBehaviour : MonoBehaviour
{
    public bool IsActivated;
	public GameObject tank, turret, cannon, cannon_child, muzzlePoint_cannon;
    [Header("Movement Settings")]
    public float forwardSpeed = 20f;
    public float backwardSpeed = 5f;
    public float turnSpeed = 60f;
    public float highSpeedThreshold = 8f; // rychlost, při které začne smyk
    public float swayAmount = 5f; // kývání při prudkém zastavení

    [Header("Particle Systems")]
    public ParticleSystem dustLeft;
    public ParticleSystem dustRight;
    public float windForceMultiplier = 2f; // síla odfouknutí prachu
	public bool IsMovingForward, IsMovingBackward;
	public float moveInput, turn_speed;
	float moveTimer = 0f;
	public float turretInput;
	float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
	public float moveDuration = 2f; // kolik sekund pojede po stisku
	public GameObject cursor;
    private Rigidbody rb;
	public GameObject cannonParent;
	public GameObject prefabFireball, generatedFireball, muzzlePoint;
	float gameLength;
	
	public Vector3 lastPosition;
	float speed, speedMeters, metersPerUnit, realRange, gameRange;

    void Start()
    {
         if (tank != null)
		{
        gameLength = tank.GetComponent<Renderer>().bounds.size.z;
        metersPerUnit = 9.04f / gameLength;
		}
    }

    void Update()
    {
		float selected_tilt_angle = 5.0f;
		if(tank==null)
			tank = GameObject.Find(this.name);
		if(turret==null)
			turret = tank.transform.GetChild(1).gameObject;
		if(cannon==null)
			cannon = turret.transform.GetChild(0).gameObject;
		if(cursor==null)
			cursor = GameObject.Find("cursor_marker");
		cannon_child = cannon.transform.GetChild(0).GetChild(0).gameObject;
		if(cannon_child!=null && cannon_child.transform.localRotation.y!=90)
		{
			cannon_child.transform.rotation = Quaternion.Euler(0,90,0);
			cannon_child.transform.position = cannon.transform.position;
		}
		else if(cannon_child!=null && cannon_child.transform.localRotation.y==90 && cannon_child.transform.localRotation.x!=selected_tilt_angle)
		{
			cannon_child.transform.rotation = Quaternion.Euler(selected_tilt_angle,90,0);
			
			//cannon_child.transform.position = cannon.transform.position;
		}
		/*else if(cannon_child!=null)
		{
			GameObject cannon_ = cannon_child.transform.GetChild(0).gameObject; 
			cannon_.transform.position = cannon.transform.position;
			cannon_.transform.rotation = turret.transform.localRotation;
		}*/
		turn_speed = 0.75f;
        if (!IsActivated) return;
		
		metersPerUnit = 9.04f / gameLength;
        moveInput = 0f;
        if (Input.GetKeyDown(KeyCode.W))

		{
			moveTimer = moveDuration;
			IsMovingForward = true; IsMovingBackward = false;
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			 moveTimer = moveDuration;
			 IsMovingForward = false; IsMovingBackward = true;
		}
		else
		{
			moveInput = 0;
		}
		
		 float turnInput = 0f;

		if (Input.GetKey(KeyCode.A)) turnInput = -1f;
		if (Input.GetKey(KeyCode.D)) turnInput = 1f;
		// odpočet času
		if (moveTimer > 0)
			{
				moveTimer -= Time.deltaTime;
				if(IsMovingForward==true)
				moveInput = 1f;
				else
					moveInput = -1f;
			}
			else
			{
				moveInput = 0f;
			}
        

  
        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        else if (Input.GetKey(KeyCode.D)) turnInput = 1f;
		if(IsActivated==true)
		{
        HandleMovement2(moveInput, turnInput);
       
		HandleTurret();
		HandleCannon();
		 speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
		lastPosition = transform.position;
		
		speedMeters = speed * metersPerUnit;
		
		float tilt = GetGunTilt();
		float realRange = CalculateRange(tilt);

		// převod do Unity jednotek
		gameRange = realRange / metersPerUnit; 
		
		//HandleDust(turnInput);
		
		if (Input.GetKey(KeyCode.LeftControl))
		{
    Vector3 dir = cursor.transform.position - turret.transform.position;

    dir.y = 0f; // nechceme naklánění nahoru/dolů

    // OPAČNÝ směr
   // dir = -dir;

		if (dir != Vector3.zero)
		{
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        turret.transform.rotation = Quaternion.Lerp(
            turret.transform.rotation,
            targetRotation,
            Time.deltaTime * 5f
        );
		}
		//SetupMuzzlePoint();//MainCannonBehaviour does this
		}
		
		if(Input.GetKey(KeyCode.Space))
		{
			
			ActivateFireBall();
		}
		}
    }//konec Update()
	


public void SetupMuzzlePoint()
{
    if(cannonParent == null)
    {
        cannonParent = cannon; // pokud cannonParent není přiřazen, vezmeme cannon
        if(cannonParent == null) return;
    }

    // Pokud muzzlePoint už existuje, nepotřebujeme ho znovu
    if(muzzlePoint == null)
    {
        // Hledáme child s názvem "MuzzlePoint_Merkava" přímo v cannonParent
        Transform mp = cannonParent.transform.Find("MuzzlePoint_Merkava");

        if(mp != null)
        {
            muzzlePoint = mp.gameObject;
            Debug.Log("MuzzlePoint nalezen v dětech cannonParent: " + muzzlePoint.name);
        }
        else
        {
            // Pokud child neexistuje, vytvoříme nový
            muzzlePoint = new GameObject("MuzzlePoint_Merkava");
            muzzlePoint.transform.parent = cannonParent.transform;
            Debug.Log("MuzzlePoint vytvořen nový: " + muzzlePoint.name);
        }

        // Získáme délku kanonu a umístíme muzzlePoint na konec kanonu
        Renderer cannonRenderer = cannonParent.GetComponent<Renderer>();
        float length = 1f;
        if(cannonRenderer != null)
            length = cannonRenderer.bounds.size.z;

        muzzlePoint.transform.localPosition = new Vector3(0f, 0f, length);
        muzzlePoint.transform.localRotation = Quaternion.identity;

        Debug.Log("MuzzlePoint nastaven na konci kanonu: " + muzzlePoint.transform.position);
    }
}



// Funkce pro aktivaci fireballu
public void ActivateFireBall()
{
	
    if (prefabFireball == null || muzzlePoint == null)
    {
      // prefabFireball = GameObject.Find("ppfxExplosionFireball02");
	  prefabFireball = GameObject.Find("WFX_Nuke");
    }

    // Vytvoření instance FireBallu na muzzlePointu
    GameObject fireballInstance = Instantiate(prefabFireball, muzzlePoint.transform.position, muzzlePoint.transform.rotation);

    // Volitelně, pokud chceme, aby se fireball pohyboval s kanonem (např. pro animace)
    fireballInstance.transform.SetParent(muzzlePoint.transform);

    // Aktivace a spuštění efektu
    fireballInstance.SetActive(true);
    ParticleSystem ps = fireballInstance.GetComponent<ParticleSystem>();
    if(ps != null)
    {
        ps.Play();
        Destroy(fireballInstance, ps.main.duration);
    }
    else
    {
        Destroy(fireballInstance, 2f);
    }
}
	public float cannonRotateSpeed = 30f;

	void HandleCannon()
	{
    if (cannonParent == null) return;

    float input = 0f;

    if (Input.GetKey(KeyCode.UpArrow)) input = 1f;
    if (Input.GetKey(KeyCode.DownArrow)) input = -1f;

    float currentX = cannonParent.transform.localEulerAngles.x;

    if (currentX > 180) currentX -= 360;

    currentX += input * cannonRotateSpeed * Time.deltaTime;

    // LIMITY
    currentX = Mathf.Clamp(currentX, -5f, 15f);

    cannonParent.transform.localEulerAngles = new Vector3(currentX, 0f, 0f);
	}
	public float muzzleVelocity = 1700f; // m/s
	public float gravity = 9.81f;

	float CalculateRange(float angleDeg)
	{
    float angleRad = angleDeg * Mathf.Deg2Rad;
    float range = (muzzleVelocity * muzzleVelocity * Mathf.Sin(2 * angleRad)) / gravity;
    return range;
	}
	
	float GetGunTilt()
	{
    if (cannonParent == null) return 0f;

    float angle = cannonParent.transform.localEulerAngles.x;

    if (angle > 180) angle -= 360;

    return Mathf.Clamp(angle, -5f, 15f);
	}
	public float turretRotateSpeed = 100f;

	void HandleTurret()
	{
    if (turret == null) return;

    turretInput = 0f;

    if (Input.GetKey(KeyCode.LeftArrow)) turretInput = -1f;
    if (Input.GetKey(KeyCode.RightArrow)) turretInput = 1f;

    if (turretInput != 0f)
    {
        float rotation = turretInput * turretRotateSpeed * Time.deltaTime;
        turret.transform.Rotate(0f, rotation, 0f);
    }
}
    void HandleMovement(float moveInput, float turnInput)
    {
        // Pohyb vpřed/dozadu
        float currentSpeed = moveInput > 0 ? forwardSpeed : backwardSpeed;
        Vector3 movement = transform.forward * moveInput * currentSpeed * Time.deltaTime;
		if(moveInput>0 || moveInput<0)
		{
			if(moveInput<0)
				tank.transform.Translate(0,0,currentSpeed);
			else if(moveInput>0)
				tank.transform.Translate(0,0,-currentSpeed);
		}
        // Rotace podle rychlosti
        float speedMagnitude = rb.velocity.magnitude;
        if (speedMagnitude < highSpeedThreshold)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnInput * turnSpeed * Time.deltaTime, 0f));
        }
        else
        {
            // prudký smyk: otočení až o 90 stupňů
            float smykAngle = turnInput * 90f * Time.deltaTime;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, smykAngle, 0f));
        }

        // Lehké kývání při prudkém zastavení
        if (moveInput == 0 && rb.velocity.magnitude > 0.1f)
        {
            float sway = Mathf.Sin(Time.time * 5f) * swayAmount;
            rb.rotation = rb.rotation * Quaternion.Euler(0f, 0f, sway * Time.deltaTime);
        }
    }
void HandleMovement2(float moveInput, float turnInput)
{
    float currentSpeed = moveInput > 0 ? forwardSpeed : backwardSpeed;

    // POHYB
    if (moveInput != 0)
    {
        tank.transform.Translate(0, 0, -moveInput * currentSpeed * Time.deltaTime);
    }

    // ROTACE
    if (turnInput != 0)
    {
        float rotation = turnInput * turnSpeed * Time.deltaTime;

        // pokud jede dopředu → normální zatáčení
        // pokud dozadu → otočené (realističtější)
        if (moveInput < 0)
            rotation *= -1f;

        tank.transform.Rotate(0, rotation, 0);
    }
}
    void HandleDust(float turnInput)
    {
        if (dustLeft == null || dustRight == null) return;

        // Pokud prudce zatáčíme, odfouknout prach na opačnou stranu
        float speedMagnitude = rb.velocity.magnitude;

        if (speedMagnitude > highSpeedThreshold && Mathf.Abs(turnInput) > 0.1f)
        {
            // směr větru: proti zatáčení
            Vector3 wind = transform.right * (-turnInput) * windForceMultiplier;

            var leftMain = dustLeft.main;
            var rightMain = dustRight.main;

            // upravit rychlost částic
            leftMain.startSpeed = Mathf.Abs(wind.x);
            rightMain.startSpeed = Mathf.Abs(wind.x);

            // natočení směru částic
            dustLeft.transform.rotation = Quaternion.LookRotation(wind);
            dustRight.transform.rotation = Quaternion.LookRotation(wind);

            // zapnout emisi
            if (!dustLeft.isPlaying) dustLeft.Play();
            if (!dustRight.isPlaying) dustRight.Play();
        }
        else
        {
            // normální jízda nebo zastavení – jen mírný prach
            if (!dustLeft.isPlaying) dustLeft.Play();
            if (!dustRight.isPlaying) dustRight.Play();
        }
    }
	
	void OnGUI()
	{
		float margin_from_right = 150.0f;
    float tilt = GetGunTilt();
    float realRange = CalculateRange(tilt);

    realRange = Mathf.Clamp(realRange, 0, 4000f);

    GUI.Label(new Rect(Screen.width - margin_from_right, 10, 240, 20), "Tilt: " + tilt.ToString("F1") + "°");
    GUI.Label(new Rect(Screen.width - margin_from_right, 30, 240, 20), "Speed: " + speedMeters.ToString("F1") + " m/s");
    GUI.Label(new Rect(Screen.width - margin_from_right, 50, 240, 20), "Range: " + realRange.ToString("F0") + " m");
	}
}