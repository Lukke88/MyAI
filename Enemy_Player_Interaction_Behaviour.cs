using UnityEngine;
using System.Collections;

public class Enemy_Player_Interaction_Behaviour : MonoBehaviour
{
    public enum Status
    {
        Searching,
        Pursuit
    }

    public Status currentStatus = Status.Searching;

    private GameObject enemy;
    private GameObject currentTarget;

    [Header("Radar")]
    public float scanDistance = 100f;
    public float scanAngle = 45f;
    public bool scanningEnabled = true;

    [Header("Combat")]
    public float fireDistance = 50f;
    public GameObject null_projectile;
    public float projectileSpeed = 40f;
    public float damage = 10f;

    [Header("Animations")]
    public string Enemy_hit_animation = "EnemyHit";
    public string player_hit_animation = "PlayerHit";

    private Animator animator;
    private BaseTalibEnemyAI baseAI;

    void Start()
    {
        enemy = this.gameObject;
        // Pokud jméno NEOBSAHUJE "_", skript se vypne
    if (!enemy.name.Contains("_"))
    {
        this.enabled = false;
        return;
    }

    animator = GetComponent<Animator>();
    baseAI = GetComponent<BaseTalibEnemyAI>();
    }

    void Update()
    {
        if (currentStatus == Status.Searching)
        {
            Scan();
        }
        else if (currentStatus == Status.Pursuit)
        {
            PursueTarget();
        }
    }

    // ================= RADAR =================

    void Scan()
    {
        if (!scanningEnabled) return;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, scanDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                scanningEnabled = false;
                return;
            }

            if (hit.collider.CompareTag("building"))
            {
                if (hit.distance <= 5.0f)
                {
                    HandleBuildingCorner(hit.collider.gameObject);
                    return;
                }
            }

            if (hit.collider.CompareTag("Player"))
            {
                currentTarget = hit.collider.gameObject;
                currentStatus = Status.Pursuit;
                return;
            }
        }
        else
        {
            // scan left-right if nothing detected
            float angle = Mathf.Sin(Time.time) * scanAngle;
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + angle * Time.deltaTime, 0);
        }
    }

    // ================= BUILDING CORNER LOGIC =================

    void HandleBuildingCorner(GameObject building)
    {
        GameObject nearestPlayer = FindNearestPlayer();
        if (nearestPlayer == null) return;

        Vector3 dirToPlayer = nearestPlayer.transform.position - transform.position;

        if (dirToPlayer.x > 0)
            transform.Rotate(0, 90f, 0);
        else
            transform.Rotate(0, -90f, 0);

        // návrat zpět po otočení kolem rohu
        StartCoroutine(ReturnFromCorner());
    }

    IEnumerator ReturnFromCorner()
    {
        yield return new WaitForSeconds(1.0f);
        transform.Rotate(0, -90f, 0);
    }

    // ================= PURSUIT =================

    void PursueTarget()
    {
        if (currentTarget == null)
        {
            currentStatus = Status.Searching;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

        transform.LookAt(currentTarget.transform);

        if (distance > fireDistance)
        {
            MoveTowards(currentTarget.transform.position);
        }
        else
        {
            FireAtTarget();
        }
    }

    // ================= FIRE =================

    void FireAtTarget()
    {
        baseAI.Fire();

        GameObject proj = Instantiate(null_projectile, transform.position + transform.forward, Quaternion.identity);

        LineRenderer lr = proj.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.SetPosition(0, proj.transform.position);
            lr.SetPosition(1, currentTarget.transform.position);
        }

        StartCoroutine(MoveProjectile(proj, currentTarget));
    }

    IEnumerator MoveProjectile(GameObject proj, GameObject target)
    {
        float timer = Random.Range(0.1f, 0.3f);

        while (proj != null && target != null)
        {
            proj.transform.position = Vector3.MoveTowards(
                proj.transform.position,
                target.transform.position,
                projectileSpeed * Time.deltaTime
            );

            if (Vector3.Distance(proj.transform.position, target.transform.position) < 0.5f)
            {
                HitTarget(target);
                Destroy(proj);
                break;
            }

            yield return new WaitForSeconds(timer);
        }
    }

    void HitTarget(GameObject target)
    {
        Animator targetAnimator = target.GetComponent<Animator>();

        if (target.CompareTag("Player"))
        {
            MiaBehaviour mia = target.GetComponent<MiaBehaviour>();
            if (mia != null)
            {
                mia.DamageHero(damage);
                targetAnimator.SetTrigger(player_hit_animation);
            }
        }
        else
        {
            targetAnimator.SetTrigger(Enemy_hit_animation);
        }
    }

    // ================= MOVE =================

    void MoveTowards(Vector3 destination)
    {
        float step = Random.Range(2f, 4f) * Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            step
        );
    }

    // ================= HELPERS =================

    GameObject FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p;
            }
        }

        return nearest;
    }
}
