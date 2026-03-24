using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateHelicopter : MonoBehaviour
{
    public bool IsActivatedMachine = false;

    public GameObject  helicopter, helicopter_body;
    public float activationDistance = 5.0f;
    public float flight_altitude = 20.0f;
	public float distance_to_board_helicopter = 25.0f;
    public GameObject playerObject, player;

    public Camera playerCamera;
    public Camera helicopterCamera;

    public GameObject mainRotor;
    public GameObject tailRotor;

    // otáčky rotorů
    public float currentRPM = 0f;
    public float targetRPM = 1500f;
    public float accelerationSpeed = 0.6f;

    // vzlet
    public float liftSpeed = 2f;

    // zvuk
    public AudioSource engineSound;

    // vibrace kamery
    public float vibrationPower = 0.05f;
    private Vector3 originalCameraPosition;

    // naklonění při startu
    public float tiltAmount = 8f;
    public float tiltSpeed = 8f;

    public bool IsHelicopterActivated, IsInFlightLevel;
	
	float currentTiltX = 0f;   // dopředu / dozadu
	float currentTiltZ = 0f;   // doleva / doprava

	float maxTiltForward = 15f;
	float maxTiltBackward = 5f;
	float maxTiltSide = 5f;

	public GameObject rocket_prefab;
public Transform rocket_launcher1;
public Transform rocket_launcher2;

public GameObject heliportPrefab;
private GameObject generatedHeliport;

public Vector3 targetPosition;
public bool moveToTarget = false;
public bool landingMode = false;

public float horizontalSpeed = 20f;
public float landingSpeed = 5f;
public float landingDistance = 50f;

public float startHeight = 2.0f; // výška po přistání

    void Start()
    {
        helicopter = GameObject.Find(this.name);
        originalCameraPosition = helicopterCamera.transform.localPosition;
    }

    void Update()
    {
		player = GameObject.Find("DesertReaper");
		if(helicopter!=null && player!=null)
		{
			if(Vector3.Distance(helicopter.transform.position,player.transform.position)<=distance_to_board_helicopter)
			{
				IsActivatedMachine = true;
			}
		}
        if (IsActivatedMachine)
        {
			CameraFollowsHero cam = helicopter.GetComponent<CameraFollowsHero>();
			if (cam != null)
				{
					cam.target = helicopter.transform;
				}
            mainRotor = helicopter.transform.GetChild(1).GetChild(0).gameObject;
            tailRotor = helicopter.transform.GetChild(1).GetChild(1).gameObject;

            // postupné zvyšování otáček
            currentRPM = Mathf.Lerp(currentRPM, targetRPM, accelerationSpeed * Time.deltaTime);

            // rotace rotorů
            mainRotor.transform.Rotate(Vector3.up * currentRPM * Time.deltaTime);
            tailRotor.transform.Rotate(Vector3.right * currentRPM * Time.deltaTime);

            // zvuk podle otáček
            if (engineSound != null)
            {
                engineSound.pitch = 0.5f + (currentRPM / targetRPM);
                engineSound.volume = currentRPM / targetRPM;
            }

            // vibrace kamery
            if (currentRPM > 400f)
            {
                float vibration = vibrationPower * (currentRPM / targetRPM);
                helicopterCamera.transform.localPosition =
                    originalCameraPosition + Random.insideUnitSphere * vibration;
            }

            // naklonění při startu
            if (currentRPM > 700f)
            {
                helicopter.transform.rotation = Quaternion.Lerp(
                    helicopter.transform.rotation,
                    Quaternion.Euler(-tiltAmount, helicopter.transform.rotation.eulerAngles.y, 0),
                    tiltSpeed * Time.deltaTime
                );
            }

            // vzlet
            if (currentRPM > 900f)
            {
                helicopter.transform.position += Vector3.up * liftSpeed * Time.deltaTime;
            }

            // zastavení ve výšce
            if (helicopter.transform.position.y > flight_altitude )
            {
                helicopter.transform.position = new Vector3(
                    helicopter.transform.position.x,
                    flight_altitude,
                    helicopter.transform.position.z
                );
            }
			else if(helicopter.transform.position.y==flight_altitude)
			{
				IsInFlightLevel = true;
			}
			
			if(IsInFlightLevel==true)
			{
				helicopter_body = helicopter.transform.GetChild(1).gameObject;
				
				if (Input.GetKey(KeyCode.W))
				{
					currentTiltX = Mathf.Lerp(currentTiltX, -maxTiltForward, tiltSpeed * Time.deltaTime);
				}
				else
				{
					currentTiltX = Mathf.Lerp(currentTiltX, 0f, tiltSpeed * Time.deltaTime);
				}
				
				if (Input.GetKey(KeyCode.A))
				{
					currentTiltZ = Mathf.Lerp(currentTiltZ, maxTiltSide, tiltSpeed * Time.deltaTime);
				}
				else if (Input.GetKey(KeyCode.D))
				{
				currentTiltZ = Mathf.Lerp(currentTiltZ, -maxTiltSide, tiltSpeed * Time.deltaTime);
				}
				else
				{
					currentTiltZ = Mathf.Lerp(currentTiltZ, 0f, tiltSpeed * Time.deltaTime);
				}
				
				helicopter_body.transform.localRotation =
				Quaternion.Euler(currentTiltX, 0f, currentTiltZ);
				
				float rotationSpeed = 60f;

				if (Input.GetKey(KeyCode.A))
				{
					helicopter.transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime);
				}

				if (Input.GetKey(KeyCode.D))
				{
					helicopter.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
				}
				
				
			if (Input.GetKey(KeyCode.LeftControl))
			{
				GameObject cursor = GameObject.Find("Quad");

				if (cursor != null)
				{
				Vector3 direction = cursor.transform.position - helicopter.transform.position;

				direction.y = 0f; // nechceme naklánění nahoru/dolů

				if (direction != Vector3.zero)
				{
				Quaternion targetRotation = Quaternion.LookRotation(direction);

				helicopter.transform.rotation = Quaternion.Lerp(
                helicopter.transform.rotation,
                targetRotation,
                5f * Time.deltaTime
					);
				}
				}
			}
			}
			
			if (Input.GetKeyDown(KeyCode.Space))
			{
			FireRocket();
			}
			
			if (Input.GetKey(KeyCode.H) && Input.GetMouseButtonDown(0)) //aktivace heliportu
			{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

				if (Physics.Raycast(ray, out hit, 5000f))
				{
        if (generatedHeliport != null)
            Destroy(generatedHeliport);

        generatedHeliport = Instantiate(heliportPrefab, hit.point, Quaternion.identity);

        targetPosition = hit.point;
        moveToTarget = true;
        landingMode = true;
			}
			}
			
			
			if (moveToTarget == true) //let k cili
				{
				Vector3 horizontalTarget = new Vector3(
				targetPosition.x,
				helicopter.transform.position.y,
				targetPosition.z
				);

    helicopter.transform.position = Vector3.MoveTowards(
        helicopter.transform.position,
        horizontalTarget,
        horizontalSpeed * Time.deltaTime
    );

    helicopter.transform.LookAt(horizontalTarget);
			}
			
			
			if (landingMode == true && generatedHeliport != null) //navadeni na heliport
{
    float dist = Vector3.Distance(
        helicopter.transform.position,
        generatedHeliport.transform.position
    );

    if (dist < landingDistance)
    {
        helicopter.transform.position = Vector3.MoveTowards(
            helicopter.transform.position,
            generatedHeliport.transform.position,
            landingSpeed * Time.deltaTime
        );
		}
		}
		
		if (landingMode == true && generatedHeliport != null)
{
    if (Vector3.Distance(helicopter.transform.position, generatedHeliport.transform.position) < 2f)
    {
        IsActivatedMachine = false;

        // zpomalení rotorů
        currentRPM = Mathf.Lerp(currentRPM, 0f, 1.5f * Time.deltaTime);

        // návrat hráče
        playerObject.SetActive(true);

        playerObject.transform.position =
            helicopter.transform.position + helicopter.transform.right * 13f;

        // kamera zpět na hráče
        playerCamera.enabled = true;
        helicopterCamera.enabled = false;

        moveToTarget = false;
        landingMode = false;
    }
}
if (Input.GetMouseButtonDown(0)) //let k cili
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 5000f))
    {
        targetPosition = hit.point;
        targetPosition.y = helicopter.transform.position.y; // držíme výšku

        moveToTarget = true;
        landingMode = false;
    }
}
        }
    }
	
	
	void FireRocket()//vystreleni raket
	{
    GameObject cursor = GameObject.Find("Quad");

    if (cursor == null) return;

    Vector3 target_place = cursor.transform.position;

    // náhodný výběr raketometu
    Transform launcher;

    if (Random.Range(0, 2) == 0)
        launcher = rocket_launcher1;
    else
        launcher = rocket_launcher2;

    // vytvoření rakety
    GameObject rocket = Instantiate(
        rocket_prefab,
        launcher.position,
        launcher.rotation
    );

    // předáme cíl raketě
    rocket.GetComponent<RocketBehaviour>().targetPosition = target_place;
	}
    public void ActivateObject()
    {
        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance < activationDistance)
        {
            IsActivatedMachine = true;

            // vypnutí hráče
            playerObject.SetActive(false);

            // přepnutí kamery
            playerCamera.enabled = false;
            helicopterCamera.enabled = true;

            // spuštění zvuku
            if (engineSound != null)
                engineSound.Play();

            Debug.Log("Helicopter Activated");
            IsHelicopterActivated = true;
        }
        else
        {
            Debug.Log("Player is too far from helicopter");
        }
    }
}
//toto dat do rakety
public class RocketBehaviour : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed = 40f;

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        transform.LookAt(targetPosition);
    }
}