using UnityEngine;
using System.Collections.Generic;

public class ProjectileCollision : MonoBehaviour
{
    [Header("Flight")]
    public float flightSpeed = 25f;
    public float ballisticHeight = 8f;

    [Header("Explosion FX")]
    public ParticleSystem explosionFireball;
    public GameObject craterPrefab;
    public AudioClip explosionSound;

    [Header("Explosion Physics")]
    public float explosionRadius = 40f;
    public float explosionForce = 2500f;
    public float upwardsModifier = 1.2f;

    [Header("Cleanup")]
    public float fxLifetime = 5f;

    // interní
    Vector3 startPos;
    Vector3 targetPos;
    float totalDistance;
    float travelled;
    public bool exploded, IsShot;

    public GameObject target;
    public GameObject floor;
	
	[Header("Crater Object")]
public GameObject krater1;  // tady přiřadíš svůj kráter PNG v inspektoru

    Rigidbody rb;
    List<Vector3> pathPoints;
    int currentIndex = 0;
    float moveSpeed = 30f;

    public void Init(Vector3 start, Vector3 target)
    {
        startPos = start;
        targetPos = target;
        travelled = 0f;
        totalDistance = Vector3.Distance(
            new Vector3(start.x, 0, start.z),
            new Vector3(target.x, 0, target.z)
        );

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void InitBallisticPath(
        Transform turret,
        Transform cursor,
        int resolution,
        System.Func<Vector3, Vector3, float, Vector3> getBallisticPoint
    )
    {
        pathPoints = new List<Vector3>();

        Vector3 start = turret.GetChild(0).position;
        Vector3 end = cursor.position;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 p = getBallisticPoint(start, end, t);
            pathPoints.Add(p);
        }

        currentIndex = 0;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        transform.position = pathPoints[0];
    }

    void Update()
    {
        // najdi floor pokud není nastaven
        if (floor == null)
        {
            floor = GameObject.Find("Ground");
            if (floor == null)
                return; // nemáme podlahu, nepočítáme explozi
        }

        if (exploded || pathPoints == null || pathPoints.Count < 2)
            return;

        // pohyb po pathPoints
        Vector3 targetPoint = pathPoints[currentIndex];
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint,
            moveSpeed * Time.deltaTime
        );

        Vector3 dir = targetPoint - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        // postup na další bod
        if (Vector3.Distance(transform.position, targetPoint) < 0.05f)
        {
            currentIndex++;
            if (currentIndex >= pathPoints.Count)
            {
                Explode();
                return;
            }
        }

        // === NOVÉ: Exploze při dosažení podlahy ===
        if (transform.position.y <= floor.transform.position.y)
        {
            Explode();
        }
    }

 public GameObject bullet_hole; // nový prefab pro bullet hole

void OnCollisionEnter(Collision collision)
{
    if (exploded) return;

    Vector3 hitPos = collision.contacts[0].point;

    if (collision.gameObject.CompareTag("Ground"))
    {
        // podlaha – záleží na výšce dopadu
        float heightAboveGround = hitPos.y - floor.transform.position.y;

        if (heightAboveGround < 0.2f)
        {
            // velký kráter
            if (krater1)
            {
                GameObject craterInstance = Instantiate(
                    krater1,
                    hitPos,
                    Quaternion.Euler(0f, 0f, 0f)
                );

                float randomScale = 1f + Random.Range(0f, 0.2f);
                craterInstance.transform.localScale *= randomScale;

                Destroy(craterInstance, fxLifetime);
            }
        }
        else
        {
            // bullet hole
            if (bullet_hole)
            {
                GameObject bullet = Instantiate(
                    bullet_hole,
                    hitPos + collision.contacts[0].normal * 0.01f, // malý offset
                    Quaternion.Euler(0f, 0f, 0f) // otočení o 90° v Z
                );
                Destroy(bullet, fxLifetime);
            }
        }

        Explode();
    }
    else if (collision.gameObject.CompareTag("Wall"))
    {
        // bullet hole na zdi
        if (bullet_hole)
        {
            GameObject bullet = Instantiate(
                bullet_hole,
                hitPos + collision.contacts[0].normal * 0.01f,
                Quaternion.Euler(0f, 0f, 0f)
            );
            Destroy(bullet, fxLifetime);
        }

        Destroy(gameObject); // projektil zničíme
    }
}



    void Explode()
{
    if (exploded) return;
    exploded = true;
    Vector3 pos = transform.position;

    // FX: fireball
    if (explosionFireball)
    {
        ParticleSystem fx = Instantiate(explosionFireball, pos, Quaternion.identity);
        fx.Play();
        Destroy(fx.gameObject, fxLifetime);
    }

    // FX: prefab kráteru (původní)
    if (craterPrefab)
    {
        Instantiate(
            craterPrefab,
            pos + Vector3.up * 0.02f,
            Quaternion.Euler(0, 0, 0f)
        );
    }

    // === NOVÉ: instancování krater1 ===
    if (krater1)
{
    // získáme rotaci floor v X a Z
    Quaternion floorRotation = Quaternion.Euler(0, 0, 0);

    GameObject craterInstance = Instantiate(
        krater1,
        pos,                  // přesně na místo exploze
        floorRotation
    );

    // náhodná velikost 100% až 120%
    float randomScale = 1f + Random.Range(0f, 0.2f);
    craterInstance.transform.localScale *= randomScale;

    Destroy(craterInstance, fxLifetime);  // automatické odstranění po fxLifetime
}

    // Audio
    if (explosionSound)
        AudioSource.PlayClipAtPoint(explosionSound, pos, 1f);

    // tlaková vlna
    Collider[] hits = Physics.OverlapSphere(pos, explosionRadius);
    foreach (Collider hit in hits)
    {
        Rigidbody rb = hit.attachedRigidbody;
        if (rb != null)
        {
            rb.AddExplosionForce(
                explosionForce,
                pos,
                explosionRadius,
                upwardsModifier,
                ForceMode.Impulse
            );
        }
    }

    Destroy(gameObject);
}


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
