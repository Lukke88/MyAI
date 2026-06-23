using UnityEngine;
using UnityEngine.UI;

public class DesertReaper_manual_navigation : MonoBehaviour
{
    public float moveSpeed = 25.0f;
    public float rotationSpeed = 120f;

    public float detectDistance = 2000f;

    public Text guiText;   // připojíš Text z Canvasu
	public bool IsTankDetected;
    public bool IsPlayerInTank = false;
	public GameObject player, tank;
	public float distance_from_tank;//testing
	public Button TankEnterButton, TankExitButton; // přiřadíš v Inspectoru
	public bool IsPlayerLookingToTank, IsPlayerRunning;
	public float angle, targetAngle, playerAngle, delta;
	
	void Start()
{
    if (TankEnterButton != null)
    {
        TankEnterButton.gameObject.SetActive(false);
        TankEnterButton.onClick.AddListener(OnEnterTank);
    }
	
}
	void OnEnterTank()
{
    // Kamera se přepne na tank
    Camera.main.GetComponent<CameraFollowsHero>().target = tank.transform;
	tank.transform.GetComponent<TankBehaviour>().IsActivated = true;
    // Převzetí ovládání tanku
    IsPlayerInTank = true;
    Debug.Log("Hráč vstupuje do tanku!");
	
	 // deaktivace hráče
    player.SetActive(false);   // skryje model hráče
    // pokud máš pohybový skript zvlášť, lze ho také deaktivovat, např.:
    // this.enabled = false;

    // skryj button, protože už není potřeba
    if(TankEnterButton != null)
        TankEnterButton.gameObject.SetActive(false);
	}

	public void OnExitTank()
{
    if (player == null || tank == null)
        return;
	tank.transform.GetComponent<TankBehaviour>().IsActivated = false;
    // Umístíme hráče vedle tanku (např. 2 jednotky napravo od tanku)
    Vector3 exitOffset = tank.transform.right * 20f; // doprava od tanku
    player.transform.position = tank.transform.position + exitOffset;

    // Aktivace hráče
    player.SetActive(true);

    // Kamera se vrátí na hráče
    Camera.main.GetComponent<CameraFollowsHero>().target = player.transform;

    // Nastavíme stav
    IsPlayerInTank = false;

    // Debug
    Debug.Log("Hráč vystoupil z tanku!");
}
    void Update()
    {
		if(player==null)
		player = GameObject.Find(this.name);
		tank = GameObject.Find("Merkava Mk_lowpoly"); 	
		if(player!=null && tank!=null)distance_from_tank = Vector3.Distance(player.transform.position, tank.transform.position);
		if (distance_from_tank <= 20.0f) // player boarded the vehicle
		IsPlayerInTank = true;
		
		
        if (!IsPlayerInTank)
        {
			if(Camera.main.transform.GetComponent<CameraFollowsHero>().target==null) //basic setting
			Camera.main.transform.GetComponent<CameraFollowsHero>().target = player.transform;
			tank.transform.GetComponent<TankBehaviour>().IsActivated = false; //deactivates tank functions
            MovePlayer();
			DetectTank();
            
        }
		else if(IsPlayerInTank==true)
		{
			Camera.main.transform.GetComponent<CameraFollowsHero>().target = tank.transform; //switch it when is player in tank
			tank.transform.GetComponent<TankBehaviour>().IsActivated = true; //activates tank functions
    Debug.Log("Hráč vstupuje do tanku!");
	
	 // deaktivace hráče
    player.SetActive(false);   // skryje model hráče
		PlayIdleAnimation();
		Camera.main.transform.GetComponent<CameraFollowsHero>().sideAngle = 180.0f; //switch it when is player in tank
		}
		IsPlayerRunning = IsPlayerMoving();
		if (!IsPlayerRunning && !IsPlayerInTank)
		{
			Animator anim = player.transform.GetComponent<Animator>();
		
			anim.SetInteger("IsReaperRunsFast",0);
			PlayIdleAnimation();
		}
		DetectPlayerSight();
	
		
		//ButtonsManagement();
		
	
    }
	
	public void StartMovementAnimation()
	{
		Animator anim = player.transform.GetComponent<Animator>();
		anim.Play("ReaperRunsFast");
		anim.SetInteger("IsReaperRunsFast",1);
	}
	
	public void PlayIdleAnimation()
{
	Animator anim = player.transform.GetComponent<Animator>();
    anim.Play("ReaperGunplayShooting");          // název tvé idle animace
    anim.SetInteger("IsReaperGunplayShooting", 0);
}
	void DetectPlayerSight()
	{
		Vector3 dir = tank.transform.position - player.transform.position;
dir.y = 0;

targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg; // úhel k tanku
playerAngle = player.transform.eulerAngles.y;             // kde hráč čumí

delta = Mathf.DeltaAngle(playerAngle, targetAngle);      // rozdíl mezi hráčem a tankem
		angle = delta;
		if(delta>0 && delta < 30f) // například ±30° považujeme za "otočen k tanku"
		{
			Debug.Log("Hráč je otočen k tanku");
			IsPlayerLookingToTank = true;
	}
	}
	bool IsPlayerMoving()
	{
    return Input.GetKey(KeyCode.W) ||
           Input.GetKey(KeyCode.S) ||
           Input.GetKey(KeyCode.A) ||
           Input.GetKey(KeyCode.D);
	}
	public void ButtonsManagement()
	{
		if (!TankEnterButton.gameObject.activeSelf)
		{
            TankEnterButton.gameObject.SetActive(true); // aktivace buttonu
		}
		else if(delta<0)
		{
			IsPlayerLookingToTank = false;
			
        if (TankEnterButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(false); // deaktivace buttonu
		}
		if(IsPlayerInTank==true)
		{
			if (TankEnterButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(false); // deaktivace buttonu pro vstup
			if (TankExitButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(true); // aktivace buttonu pro vystup
		}
		else if(IsPlayerInTank==false && IsPlayerLookingToTank==true)
		{
			if (TankEnterButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(true); // aktivace buttonu pro vstup
			if (TankExitButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(false); // deaktivace buttonu pro vystup
		}
		else
		{
			if (TankEnterButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(false); // deaktivace buttonu pro vstup
			if (TankExitButton.gameObject.activeSelf)
            TankEnterButton.gameObject.SetActive(false); // deaktivace buttonu pro vystup
		}
	}
    void MovePlayer()
    {
        if (Input.GetKey(KeyCode.W))
		{
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
			StartMovementAnimation();
		}
			else if(Input.GetKeyUp(KeyCode.W))
			{
				PlayIdleAnimation();
			}
        if (Input.GetKey(KeyCode.S))
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
	public GameObject child;
    void DetectTank()
    {
		float yRotation = player.transform.eulerAngles.y;
		child = player.transform.GetChild(2).gameObject;
		child.transform.rotation = Quaternion.Euler(-2.0f,yRotation,0);
        Ray ray = new Ray(child.transform.position, player.transform.forward);
	   //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		Debug.DrawLine(child.transform.position, player.transform.forward * detectDistance, Color.blue);
		
		 // vezmeme rotaci hráče
		
		
		Debug.DrawLine(child.transform.position, child.transform.forward * detectDistance, Color.yellow);
        if (Physics.Raycast(ray, out hit, detectDistance))
        {
            if (hit.collider.CompareTag("tank") || (hit.collider.name.Contains("Merkava Mk_lowpoly") || hit.collider.name.Contains("Body")))
            {
                guiText.text = "Do you want board the tank (press Enter)";
				IsTankDetected = true;
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    IsPlayerInTank = true;
                    guiText.text = "";
                }
            }
            else
            {
                guiText.text = "";
				
            }
			
        }
        
    }
}