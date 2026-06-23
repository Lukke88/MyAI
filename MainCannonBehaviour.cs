using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCannonBehaviour : MonoBehaviour
{
	public GameObject mainCannon, turret, tank, muzzlePoint_new, prefabFireball, muzzlePoint, projectilePrefab, currentProjectile;
	public float turn_angle_y, turn_angle_y_test, koefficient, unityCannonLength, impactDistance;
	public float tilt_angle = 10.0f;
	public int projectileIndex;
	public float muzzleVelocity = 1500f; // m/s (tank kanon)
	public float gravity = 9.81f;
	public float realCannonLength = 6.5f; // metry
	public int projectilesCounter;
	float metersPerUnit;
	public Vector3 startPosition;
    // Start is called before the first frame update
    void Start()
    {
        tank = GameObject.Find("Merkava Mk_lowpoly");
		turret = tank.transform.GetChild(1).gameObject;
		projectilePrefab = GameObject.Find("TankProjectile");
    }

    // Update is called once per frame
    void Update()
    {
		 // změna tilt podle kolečka myši
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        tilt_angle += scroll * 2f; // citlivost
        tilt_angle = Mathf.Clamp(tilt_angle, -5f, 15f); // omezení
		
		 metersPerUnit = realCannonLength / unityCannonLength;
		 muzzleVelocity = 200f;
		 if(muzzlePoint_new!=muzzlePoint)
			 muzzlePoint_new = muzzlePoint;
        if(mainCannon==null)mainCannon=GameObject.Find(this.name);
		else
		{
			
			muzzlePoint = mainCannon.transform.GetChild(0).gameObject;
			tank = GameObject.Find("Merkava Mk_lowpoly");
			
			if(Input.GetKey(KeyCode.Space))
		{
			IsProjectileSpawned = false;
		//	ActivateFireBall();
			// Fire_ball(turret.transform.position, 200f, projectilePrefab);
			SpawnProjectile();
			 DrawBallisticCurve();
			 if(currentProjectile!=null)
			 {
			 }
		}
			turret = tank.transform.GetChild(1).gameObject;
			turn_angle_y = (turret.transform.localRotation.y)*100+6.0f;
			turn_angle_y_test = turret.transform.localEulerAngles.y;
			koefficient = turn_angle_y_test/turn_angle_y;
		//	mainCannon.transform.rotation = Quaternion.Euler(tilt_angle, turret.transform.localEulerAngles.y+46.0f, 0);
			float cannon_length = mainCannon.transform.GetComponent<Collider>().bounds.size.z;
			if(muzzlePoint_new==null)
			{
				AddMuzzlePointToCannon();
				muzzlePoint_new = new GameObject();
				muzzlePoint_new.transform.position = new Vector3(mainCannon.transform.position.x + cannon_length*Mathf.Sin(turn_angle_y)*180/Mathf.PI,mainCannon.transform.position.y,mainCannon.transform.position.z + cannon_length*Mathf.Cos(turn_angle_y)*180/Mathf.PI);
			}
			else if(muzzlePoint_new!=null && currentProjectile==null)
			{
				currentProjectile = Instantiate(projectilePrefab, muzzlePoint_new.transform.position, muzzlePoint_new.transform.rotation);
			}
			
		}
		
		
		
		// Pohyb projektilu po bodech
    if (currentProjectile != null && allPoints.Length > 0)
    {
        if (projectileIndex < allPoints.Length)
        {
            currentProjectile.transform.position = Vector3.Lerp(currentProjectile.transform.position, allPoints[projectileIndex], 0.3f);
            if (Vector3.Distance(currentProjectile.transform.position, allPoints[projectileIndex]) < 0.1f)
                projectileIndex++;
        }
        else
        {
            Destroy(currentProjectile, 2f); // projektil po dopadu zničíme
            currentProjectile = null;
        }
    }
		
		
    }
	public bool IsProjectileSpawned;
	void SpawnProjectile()
{
    if (projectilePrefab == null || muzzlePoint_new == null)
        return;
	if(IsProjectileSpawned==false)
	{
    currentProjectile = Instantiate(projectilePrefab, muzzlePoint_new.transform.position, muzzlePoint_new.transform.rotation);
	IsProjectileSpawned = true;
	}
    currentProjectile.name = "TankProjectile_" + projectilesCounter.ToString();
    projectileIndex = 0;
    projectilesCounter++;
}
	
	public void Fire_ball(Vector3 starting_position, float starting_speed, GameObject projectilePrefab)
{
    if(projectilePrefab == null)
    {
        Debug.LogWarning("Projectile prefab není přiřazen!");
        return;
    }

    if(muzzlePoint_new == null)
    {
        Debug.LogWarning("MuzzlePoint není přiřazen!");
        return;
    }

    // vytvoření projektilu
    GameObject projectile = Instantiate(projectilePrefab, turret.transform.position, muzzlePoint_new.transform.rotation);
    projectile.name = "TankProjectile_" + projectilesCounter.ToString();

    // přidání Rigidbody (pokud není)
    Rigidbody rb = projectile.GetComponent<Rigidbody>();	
	
    if(rb == null)
        rb = projectile.AddComponent<Rigidbody>();
	rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
	rb.interpolation = RigidbodyInterpolation.Interpolate;
    // vypneme gravity (tankový projektil)
    rb.useGravity = false;

    // směr střely (dopředu z kanonu)
    Vector3 direction = turret.transform.right;

    // aplikace impulsu
    rb.AddForce(direction * starting_speed, ForceMode.Impulse);
	projectilesCounter++;
}
	
	// Funkce pro aktivaci fireballu
public void ActivateFireBall()
{
	
    if (prefabFireball == null || muzzlePoint == null)
    {
      // prefabFireball = GameObject.Find("ppfxExplosionFireball02");
	  prefabFireball = GameObject.Find("WFX_Nuke");
	  muzzlePoint = muzzlePoint_new;
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
	void AddMuzzlePointToCannon()
{
    if(mainCannon==null)
        mainCannon = GameObject.Find(this.name);
    else
    {
        if(muzzlePoint_new == null)
        {
            muzzlePoint_new = new GameObject("MuzzlePoint_generated");
        }

        // délka kanonu
        float cannon_length = mainCannon.GetComponent<Collider>().bounds.size.z;

        // směr dopředu (Z osa kanonu)
        Vector3 forward = mainCannon.transform.forward;
		Vector3 right_direction = mainCannon.transform.right;
        // pozice na konci hlavně
        muzzlePoint_new.transform.position = mainCannon.transform.position - right_direction * cannon_length;
		muzzlePoint_new.transform.parent = mainCannon.transform;

        // rotace stejná jako kanon
        muzzlePoint_new.transform.rotation = mainCannon.transform.rotation;
    }
}
public Vector3[] allPoints = new Vector3[360];
public Vector3 point;
public GameObject projectileGO;//already contains currentProjectile
public Dictionary<int, Vector3> generated_projectile_positions = new Dictionary<int, Vector3>();
void DrawBallisticCurve()
{
    if (muzzlePoint_new == null) 
        muzzlePoint_new = mainCannon.transform.GetChild(1).gameObject;
	//tilt_angle = 2.0f;
    turn_angle_y = tank.transform.localEulerAngles.y + 180f;

    float unityLength = mainCannon.GetComponent<Collider>().bounds.size.z;
    metersPerUnit = realCannonLength / unityLength;

    muzzleVelocity = 200f;

    float angleRad = tilt_angle * Mathf.Deg2Rad;     // tilt (výška)
    float angleRad2 = turn_angle_y * Mathf.Deg2Rad;  // yaw (směr)

    Vector3 startPos = muzzlePoint_new.transform.position;

    float timeStep = 0.05f;
    float maxTime = 5f;
	
	

    Vector3 previousPoint = startPos;
	List<Vector3> pointsList = new List<Vector3>(); // pro navádění projektilu
    pointsList.Add(startPos);

    for(float t = 0; t < maxTime; t += timeStep)
    {
        // horizontální vzdálenost (rovnoměrný pohyb)
        float horizontalDistance = muzzleVelocity * Mathf.Cos(angleRad) * t;

        // X/Z z tvého raycast směru
        float x = startPos.x + horizontalDistance * Mathf.Sin(angleRad2);
        float z = startPos.z + horizontalDistance * Mathf.Cos(angleRad2);

        // Y z fyziky (tilt)
        float y = startPos.y + (muzzleVelocity * Mathf.Sin(angleRad) * t) 
                               - (0.5f * gravity * t * t);

        Vector3 point = new Vector3(x, y, z);

        Debug.DrawLine(previousPoint, point, Color.blue);
		
		pointsList.Add(point);

        previousPoint = point;

        // stop při dopadu
        if (y < 0f)
            {
                // spočítání horizontální vzdálenosti dopadu
                impactDistance = Vector3.Distance(new Vector3(startPos.x, 0f, startPos.z), new Vector3(x, 0f, z));
                break;
            }
			
			
			generated_projectile_positions[projectileIndex] = point;//kazda pozice se zapise do dictionary a pak odesle do souradnic projektilu
			projectileIndex++;
			
			projectileGO = Instantiate(currentProjectile, startPosition, Quaternion.identity);

			TankProjectileBehaviour proj = projectileGO.GetComponent<TankProjectileBehaviour>();

			proj.Init(generated_projectile_positions);
    }
	
}

  void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        style.normal.textColor = Color.cyan; // modrá barva, lze změnit na Color.green
        GUI.Label(new Rect(Screen.width - 150, 10, 140, 20), "Dopad: " + impactDistance.ToString("F1") + " m", style);
    }
}
