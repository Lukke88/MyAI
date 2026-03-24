using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI_protectiveBehaviour : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;
    public GameObject nearestHelicopter;
    public GameObject instantiated_gun;

    public string gun_prefab_name = "AKM_";
    public float shoot_distance = 200.0f;
    public float escape_distance = 250.0f;
    public float current_helicopter_distance;
    public bool IsShootingAllowed = false;
    public float raycast_distance_forward = 500.0f;

    [Header("Shooting")]
    public LineRenderer bulletBeam;
    public GameObject muzzleFlash_prefab;
    public Transform null_muzzle;

    public float minShootDelay = 0.15f;
    public float maxShootDelay = 0.6f;

    private float shootTimer = 0f;
    private float currentShootDelay = 0.3f;

    GameObject targetWall;
    Vector3 hidePosition;
    bool hidePositionFound = false;
    public float hideOffset = 10.0f;

    [Header("Animation")]
    public Animator enemyAnimator;
    private string running_animation = "Running";
    private string shooting_animation = "Shooting";
    private string currentIntegralParam = "IsRunning";

    public enum ActionState
    {
        Escaping, 
        SearchingHideout,
        Shooting
    }

    public ActionState selectedAction;
    public float moveSpeed = 26f;

    // ===== START =====
    void Start()
    {
        enemy = gameObject;
        player = GameObject.FindGameObjectWithTag("Player");

        // ================== PROXY KAPSLE A CHILD MODEL ==================
        // nastavíme velikost kapsle
        enemy.transform.localScale = Vector3.one * 5.69f;

        // extrahujeme číslo kapsle
        string enemyName = enemy.name; // např. "RedEnemy (3)"
        int id = 0;
        int startIdx = enemyName.IndexOf('(');
        int endIdx = enemyName.IndexOf(')');
        if(startIdx >= 0 && endIdx > startIdx)
        {
            string numberStr = enemyName.Substring(startIdx + 1, endIdx - startIdx - 1);
            int.TryParse(numberStr, out id);
        }

        // najdeme child objekt podle čísla
        string childName = "TalibanWarrior (" + id + ")";
        Transform modelChild = enemy.transform.Find(childName);
        if(modelChild != null)
        {
            // připevníme model na kapsli
            modelChild.localPosition = Vector3.zero;
            modelChild.localRotation = Quaternion.identity;
            modelChild.gameObject.SetActive(false); // zůstane skrytý na startu

            // získáme animator
            enemyAnimator = modelChild.GetComponent<Animator>();

            // nastavíme názvy animací podle typu postavy
            if(modelChild.name.Contains("TalibanWarrior"))
            {
                running_animation = "TalibRunning";
                shooting_animation = "TalibShooting";
                currentIntegralParam = "IsRunning";
            }
        }
    }

    // ===== UPDATE =====
    void Update()
    {
        nearestHelicopter = FindNearestHelicopter();
        if(nearestHelicopter == null)
            nearestHelicopter = GameObject.Find("MH-60L");

        if(player == null)
            player = GameObject.Find("DesertReaper");

        current_helicopter_distance = Vector3.Distance(enemy.transform.position,nearestHelicopter.transform.position);
        float enemyDist = Vector3.Distance(enemy.transform.position, nearestHelicopter.transform.position);
        float playerDist = Vector3.Distance(player.transform.position, nearestHelicopter.transform.position);

        // pokud je enemy blíž než hráč
        if (enemyDist < playerDist)
        {
            ProtectHelicopter();
        }

        // změna stavu po útěku
        if (selectedAction == ActionState.Escaping && current_helicopter_distance > escape_distance)
        {
            selectedAction = ActionState.SearchingHideout;
            PlayAnimation(running_animation, true); // zapneme běh
        }

        if (selectedAction == ActionState.SearchingHideout)
        {
            SearchForWall();
            PlayAnimation(running_animation, true); // stále běh
        }

        if (selectedAction == ActionState.Shooting)
        {
            IsShootingAllowed = true;
            PlayAnimation(shooting_animation, true); // přepneme na střelbu
            ShootingBehaviour();
        }
    }

    // ===== ANIMATION HELPER =====
    void PlayAnimation(string animName, bool isActive)
    {
        if (enemyAnimator == null) return;

        // zviditelníme model při animaci
        if (!enemyAnimator.gameObject.activeSelf)
            enemyAnimator.gameObject.SetActive(true);

        // přepneme parametr "IsRunning" / "IsShooting"
        enemyAnimator.SetBool(currentIntegralParam, isActive);

        // pokud měníme stav, upravíme parametr pro střelbu
        if(animName == shooting_animation)
            currentIntegralParam = "IsShooting";
        else if(animName == running_animation)
            currentIntegralParam = "IsRunning";

        // přímo přehrajeme trigger pro animaci (pokud je trigger)
        enemyAnimator.Play(animName);
    }

    // ===== SHOOTING =====
    void ShootingBehaviour()
    {
        if (player == null) return;

        // NATÁČENÍ NA HRÁČE
        Vector3 dir = player.transform.position - enemy.transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, rot, Time.deltaTime * 6f);
        }

        // TIMER NEPRAVIDELNÉ STŘELBY
        shootTimer += Time.deltaTime;
        if (shootTimer >= currentShootDelay)
        {
            FireBeam();
            shootTimer = 0f;
            currentShootDelay = Random.Range(minShootDelay, maxShootDelay);
        }
    }

    void FireBeam()
    {
        if (null_muzzle == null || bulletBeam == null) return;

        Ray ray = new Ray(null_muzzle.position, null_muzzle.forward);
        RaycastHit hit;

        Vector3 endPoint;

        if (Physics.Raycast(ray, out hit, shoot_distance))
        {
            endPoint = hit.point;
        }
        else
        {
            endPoint = null_muzzle.position + null_muzzle.forward * shoot_distance;
        }

        // LINE RENDERER (ŽLUTÝ BEAM)
        bulletBeam.SetPosition(0, null_muzzle.position);
        bulletBeam.SetPosition(1, endPoint);

        // MUZZLE FLASH
        GameObject flash = Instantiate(muzzleFlash_prefab, null_muzzle.position, null_muzzle.rotation);
        Destroy(flash, 0.3f);
    }

    // ===== WALL SEARCH =====
    void SearchForWall()
    {
        if (!hidePositionFound)
        {
            targetWall = FindNearestWall();

            // pokud žádná zeď → rovnou střelba
            if (targetWall == null)
            {
                selectedAction = ActionState.Shooting;
                return;
            }

            Vector3 wallForward = targetWall.transform.forward;
            hidePosition = targetWall.transform.position - wallForward * hideOffset;

            // rozestavení enemy podle počtu u zdi
            int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
            hidePosition.x += enemyCount * 2.5f;

            hidePositionFound = true;
        }

        // pohyb k úkrytu
        enemy.transform.position = Vector3.MoveTowards(
            enemy.transform.position,
            hidePosition,
            moveSpeed * Time.deltaTime
        );

        // pokud už tam je → střelba
        if (Vector3.Distance(enemy.transform.position, hidePosition) < 2f)
        {
            selectedAction = ActionState.Shooting;
            PlayAnimation(shooting_animation, true);
        }
    }

    GameObject FindNearestWall()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

        float minDist = 200f;
        GameObject nearest = null;

        foreach (GameObject wall in walls)
        {
            float dist = Vector3.Distance(enemy.transform.position, wall.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = wall;
            }
        }

        return nearest;
    }

    GameObject FindNearestHelicopter()
    {
        GameObject[] helicopters = GameObject.FindGameObjectsWithTag("Helicopter");

        float minDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject heli in helicopters)
        {
            float dist = Vector3.Distance(enemy.transform.position, heli.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = heli;
            }
        }

        return nearest;
    }


// Funkce pro přidání null muzzle objektu na konec hlavně
public void AddMuzzleObjectToBarrelEnd(GameObject gunPrefab, Vector3 spawnPos)
{
    // Instancujeme zbraň
    GameObject gunInstance = Instantiate(gunPrefab, spawnPos, Quaternion.identity);
    gunInstance.transform.parent = enemy.transform;

    // Zkusíme najít kostru nebo dummy pro hlaveň
    Transform barrelEnd = gunInstance.transform.Find("BarrelEnd");

    if (barrelEnd != null)
    {
        // Pokud existuje, použijeme jeho pozici
        null_muzzle = barrelEnd;
    }
    else
    {
        // Pokud neexistuje, vytvoříme nový prázdný objekt
        GameObject nullMuzzle = new GameObject("null_muzzle_object");
        nullMuzzle.transform.parent = gunInstance.transform;

        // Nastavení pozice na konec zbraně
        nullMuzzle.transform.localPosition = new Vector3(0, 0, 1.0f); // uprav podle délky hlavně
        nullMuzzle.transform.localRotation = Quaternion.identity;

        // Uložíme do globální proměnné
        null_muzzle = nullMuzzle.transform;
    }
}
	
    void ProtectHelicopter()
    {
        // pokud enemy ještě nemá zbraň
        if (enemy.transform.childCount == 0 && instantiated_gun == null)
        {
            GameObject gunPrefab = GameObject.Find(gun_prefab_name);

            if (gunPrefab != null)
            {
                Vector3 spawnPos = enemy.transform.position;
                spawnPos.y += 1.2f;

                instantiated_gun = Instantiate(gunPrefab, spawnPos, Quaternion.identity);
                instantiated_gun.transform.parent = enemy.transform;
				
				AddMuzzleObjectToBarrelEnd(gunPrefab, spawnPos);//prida instancovany muzzle objekt na konec hlavne
            }
			
			
        }

        if (instantiated_gun != null && player != null)
        {
            instantiated_gun.transform.localScale = new Vector3(0.3904918f, 0.3904918f, 0.3904918f);
            instantiated_gun.transform.position = enemy.transform.position +
                                               enemy.transform.forward * 0.803f*8 +
                                               enemy.transform.right * 0.56f*2 +
                                               enemy.transform.up * 0.069f;

            Vector3 dir = player.transform.position - enemy.transform.position;
            dir.y = 0;

            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                enemy.transform.rotation = Quaternion.Lerp(enemy.transform.rotation, rot, Time.deltaTime * 6f);
            }
			
			
        }
    }
}