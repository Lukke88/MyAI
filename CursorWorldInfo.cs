using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CursorWorldInfo : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform jenny;
    public Transform armoredCar;
    public TMP_Text infoText;
	public TMP_Text CursorInfo;
    public Button enterVehicleButton;

    [Header("Settings")]
    public float enterDistance = 21f;
    public float moveSpeed = 5f;

    private Transform currentTarget;
    private bool moveToVehicle, isRaycastingTheVehicle, IsHeroInVehicle;
	
	[Header("Animation")]
public Animator jennyAnimator;

public string animationRunState = "JennyRun";
public string animationIdleState = "JennyIdle (1)";

public string paramIsRunning = "IsJennyRunning";
public string paramIsIdle = "IsJennyIdle";

[Header("Movement Audio")]
public AudioSource footstepSource;
public AudioClip runClip;

[Header("UI")]
public TMP_Text cursorInfo;

[Header("Units")]
public Transform btr82;
public bool IsAllowedRunToVehicle;

[Header("Speed Calculation")]
public Vector3 lastJennyPosition;
public float jennySpeedMS;
public float currentCharacterVehicleDistANCE;
public bool StickToVehicle;


    [Header("Timing")]
    public float displayDelay = 1f; // čas mezi CZ a EN
    public float glitchIntensity = 1f; // posun textu pro glitch efekt

    [Header("Lines")]
    [TextArea]
    public List<string> czechLines = new List<string>
    {
        "Ty vole, kdo to tu tak zprasil?",
        "Sakra, tohle nešlo podle plánu!",
        "No fuj, zase bordel!"
    };

    [TextArea]
    public List<string> englishLines = new List<string>
    {
        "Dude, who messed this up?",
        "Damn it, that didn't go as planned!",
        "Yuck, what a mess!"
    };
    void Start()
    {
		 lastJennyPosition = jenny.position;
        enterVehicleButton.gameObject.SetActive(false);
        enterVehicleButton.onClick.AddListener(StartEnterVehicle);
		
	//	if (cursorInfo != null)
      //      StartCoroutine(ShowRandomLines());
    }

    void Update()
    {
		armoredCar = btr82;
        HandleRaycast();
		if(currentTarget!=null)
			infoText.text = "It seems that we have contact with " + currentTarget.name +". Double click to vehicle to get in." ;
        if ((moveToVehicle==true||IsAllowedRunToVehicle==true) && armoredCar != null)
        {
            MoveJennyToVehicle();
        }
		
		UpdateCursorInfo();
				currentCharacterVehicleDistANCE = Vector3.Distance(jenny.position,armoredCar.position);

		if(StickToVehicle==true)
			jenny.position = armoredCar.position;
    }
	
	public void LateUpdate()
	{
		mainCamera = Camera.main;
		//pridat glitch ze stare VHS
		/*StartCoroutine(ShowBilingualLine(
    "Ty vole, kdo to tu tak zprasil?",
    "Dude, who messed this up?"
));*/
CameraFollowsHero cam = mainCamera.GetComponent<CameraFollowsHero>();
        if (cam != null && Vector3.Distance(jenny.position,armoredCar.position)<=(enterDistance+1))
        {
			cam.mainHero = armoredCar;
            cam.target = armoredCar;
			jenny.transform.GetComponent<Renderer>().enabled = false;
			IsHeroInVehicle=true;
			IsAllowedRunToVehicle = false;
        }
		else
		{
			
		}
		
		currentCharacterVehicleDistANCE = Vector3.Distance(jenny.position,armoredCar.position);
	}
	
	IEnumerator ShowRandomLines()
    {
        while (true)
        {
            int idx = Random.Range(0, Mathf.Min(czechLines.Count, englishLines.Count));

            // CZ
            cursorInfo.text = czechLines[idx];
            ApplyGlitch();
            yield return new WaitForSeconds(displayDelay);

            // EN
            cursorInfo.text = englishLines[idx];
            ApplyGlitch();
            yield return new WaitForSeconds(displayDelay);
        }
    }

    void ApplyGlitch()
    {
        // jednoduchý jitter / glitch efekt na pozici textu
        if (cursorInfo != null)
        {
            Vector3 originalPos = cursorInfo.transform.localPosition;
            float jitterX = Random.Range(-glitchIntensity, glitchIntensity);
            float jitterY = Random.Range(-glitchIntensity, glitchIntensity);
            cursorInfo.transform.localPosition = originalPos + new Vector3(jitterX, jitterY, 0);
        }
    }
float lastClickTime = 0f;
float doubleClickDelay = 0.3f; // 300 ms – můžeš doladit
Transform lastClickedTarget = null;

    void HandleRaycast()
{
    isRaycastingTheVehicle = false;

    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(ray, out RaycastHit hit, 2000))
    {
        if (hit.transform.CompareTag("car") || hit.transform.CompareTag("armored_car"))
        {
            currentTarget = hit.transform;

            float distance = Vector3.Distance(jenny.position, currentTarget.position);

            infoText.text =
                $"{currentTarget.name} is in distance {distance:F1} from main player.\n" +
                "Double-click to enter vehicle";
			IsAllowedRunToVehicle = true;
            isRaycastingTheVehicle = true;
            enterVehicleButton.gameObject.SetActive(true);

            // ⬇️ DETEKCE DOUBLE CLICKU
            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastClickTime <= doubleClickDelay &&
                    lastClickedTarget == currentTarget)
                {
                    MoveJennyToVehicle();
                    lastClickTime = 0f;
                    lastClickedTarget = null;
                    return;
                }

                lastClickTime = Time.time;
                lastClickedTarget = currentTarget;
            }

            return;
        }
    }

    ClearUI();
}
IEnumerator ShowBilingualLine(string czech, string english, float delay = 1f)
{
    cursorInfo.text = czech;
    yield return new WaitForSeconds(delay);
    cursorInfo.text = english;
}

void UpdateCursorInfo()
{
    // ▶️ VÝŠKA JENNY (v metrech)
    float jennyHeight = 0f;
    Renderer rend = jenny.GetComponentInChildren<Renderer>();
    if (rend != null)
        jennyHeight = rend.bounds.size.y;

    // ▶️ VZDÁLENOST K BTR-82
    float distance = Vector3.Distance(jenny.position, btr82.position);

    // ▶️ RYCHLOST JENNY
    jennySpeedMS = Vector3.Distance(jenny.position, lastJennyPosition) / Time.deltaTime;
    float jennySpeedKMH = jennySpeedMS * 3.6f;

    lastJennyPosition = jenny.position;

    // ▶️ UI OUTPUT (sloupeček)
    cursorInfo.text =
        $"Height: {jennyHeight:F2} m\n" +
        $"Distance: {distance:F1} m\n" +
        $"Speed: {jennySpeedMS:F2} m/s\n" +
        $"Speed: {jennySpeedKMH:F1} km/h";
}

    // --------------------------------------------------

    void StartEnterVehicle()
    {
        if (currentTarget == null) return;

        armoredCar = currentTarget;
        moveToVehicle = true;
    }

void MoveJennyToVehicle()
{
    Vector3 targetPos = armoredCar.position;
    targetPos.y = jenny.position.y;

    float dist = Vector3.Distance(jenny.position, targetPos);

    // 👉 POHYB
    if (dist > enterDistance)
    {
        // pohyb
        jenny.position = Vector3.MoveTowards(
            jenny.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // ▶️ ANIMACE BĚHU
        jennyAnimator.SetFloat(paramIsRunning, 1.0f);
        jennyAnimator.SetFloat(paramIsIdle, 0.0f);

        // ▶️ ZVUK BĚHU
        if (!footstepSource.isPlaying)
        {
            footstepSource.clip = runClip;
            footstepSource.loop = true;
            footstepSource.Play();
        }
    }
    else
    {
        // ⛔ ZASTAVENÍ
        jennyAnimator.SetFloat(paramIsRunning, 0.0f);
        jennyAnimator.SetFloat(paramIsIdle, 1.0f);

        // 🔇 STOP ZVUK
        if (footstepSource.isPlaying)
            footstepSource.Stop();
//jennyAnimator.CrossFade(animationRunState, 0.15f);

        EnterVehicle();
    }
}


    void EnterVehicle()
    {
        moveToVehicle = false;
		StickToVehicle = true;
		armoredCar.transform.GetComponent<BTR_behaviour>().isActivated = true;
        // vypni Jenny render
        Renderer[] renderers = jenny.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;

        // přepni kameru
        CameraFollowsHero cam = mainCamera.GetComponent<CameraFollowsHero>();
        if (cam != null && Vector3.Distance(jenny.position,armoredCar.position)<=(enterDistance+1))
        {
			cam.mainHero = armoredCar;
            cam.target = armoredCar;
			IsAllowedRunToVehicle=false;
			IsHeroInVehicle = true;
        }

        ClearUI();
    }

    void ClearUI()
    {
        currentTarget = null;
        infoText.text = "";
        enterVehicleButton.gameObject.SetActive(false);
    }
}
