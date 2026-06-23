using System.Collections.Generic;
using UnityEngine;

public class TankProjectileBehaviour : MonoBehaviour
{
	public GameObject tank_projectile;
    private Dictionary<int, Vector3> trajectoryPoints;
    private int currentIndex = 0;
	public Vector3 lastPosition;
    public float speed = 300f; // jak rychle se pohybuje mezi body
    public float reachThreshold = 0.1f;

    private bool isInitialized = false;
	public GameObject prefab_particle_system, generated_particle_system;
	
	public float explosionRadius = 5f;
public int damage = 100;
	[Header("Blast Explosion")]
	public AudioClip explosionSound;
	public float soundVolume = 1f;

	[Header("Shrapnels")]
	public int shardCount = 30;
    public float shardSize = 0.2f;
    public float explosionForce = 400f;
    public float upwardModifier = 0.2f;
    public float lifeTime = 4f;

    public Material shardMaterial;
	public ExplosionDecal explosionDecal;
    // 🔥 inicializace z tanku
    public void Init(Dictionary<int, Vector3> points)
    {
        trajectoryPoints = points;
        currentIndex = 0;

        // nastavíme startovní pozici
        if (trajectoryPoints != null && trajectoryPoints.Count > 0)
        {
            transform.position = trajectoryPoints[0];
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || trajectoryPoints == null || trajectoryPoints.Count == 0)
            return;

        MoveAlongTrajectory();
    }

    void MoveAlongTrajectory()
    {
        if (currentIndex >= trajectoryPoints.Count)
        {
			Vector3 explosionPosition = transform.position;
            Explode();
            return;
        }

        Vector3 target = trajectoryPoints[currentIndex];

        // pohyb směrem k bodu
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        // rotace směrem k dalšímu bodu (optional, ale vypadá to dobře)
        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            RotateTowards(target);
        }

        // dosažení bodu
        if (Vector3.Distance(transform.position, target) < reachThreshold)
        {
            currentIndex++;
        }
    }
	public GameObject prefab_explosion;
    void Explode()
{
    Vector3 explosionPosition = transform.position;

    // particle
    //prefab_explosion = Instantiate(prefab_particle_system, explosionPosition, Quaternion.identity); //destroy after 2 seconds

    // střepiny
   // GenerateShrapnel(explosionPosition);

    // decal
    if (explosionDecal != null)
        explosionDecal.SpawnDecal(explosionPosition);

    // sound
    if (explosionSound != null)
        AudioSource.PlayClipAtPoint(explosionSound, explosionPosition, soundVolume);

    Collider[] hits = Physics.OverlapSphere(explosionPosition, explosionRadius);

    foreach (Collider hit in hits)
    {
        float distance = Vector3.Distance(explosionPosition, hit.transform.position);
        float finalDamage = damage * (1 - distance / explosionRadius);
        finalDamage = Mathf.Clamp(finalDamage, 0, damage);

        if (hit.CompareTag("Enemy"))
        {
            /*var dmg = hit.GetComponent<EnemyHealth>();
            if (dmg != null)
                dmg.TakeDamage((int)finalDamage);*/
        }

        /*if (hit.CompareTag("Barrel"))
        {
            var barrel = hit.GetComponent<ExplosiveBarrel>();
            if (barrel != null)
                barrel.Explode();
        }*/
    }

    Destroy(gameObject);
}
void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, explosionRadius);
}

public void GenerateShrapnel(Vector3 position)
    {
        for (int i = 0; i < shardCount; i++)
        {
            GameObject shard = new GameObject("Shard_" + i);
			shard.AddComponent<ShardDamage>();
            shard.transform.position = position;
            shard.transform.rotation = Random.rotation;

            MeshFilter mf = shard.AddComponent<MeshFilter>();
            MeshRenderer mr = shard.AddComponent<MeshRenderer>();

            mr.material = shardMaterial;
			MeshCollider mc = shard.AddComponent<MeshCollider>();
			mc.convex = true;

            mf.mesh = CreateTriangleMesh();

            Rigidbody rb = shard.AddComponent<Rigidbody>();

            Vector3 randomDir = Random.onUnitSphere;

            rb.AddForce(randomDir * explosionForce);
            rb.AddForce(Vector3.up * explosionForce * upwardModifier);

            rb.angularVelocity = Random.insideUnitSphere * 10f;
			
			rb.drag = 0.5f;
			rb.angularDrag = 0.2f;
			rb.mass = Random.Range(0.1f, 0.5f);

			float randomForce = Random.Range(explosionForce * 0.5f, explosionForce * 1.5f);
			rb.AddForce((Random.onUnitSphere + Vector3.up * 0.5f).normalized * randomForce);

            Destroy(shard, lifeTime);
        }
    }
	
	void RotateTowards(Vector3 targetPosition)
	{
    Vector3 direction = (targetPosition - transform.position).normalized;

    if (direction != Vector3.zero)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 10f // rychlost otáčení
        );
    }
	}
	
	void OnCollisionEnter(Collision col)
{
    /*if (col.collider.CompareTag("Enemy"))
    {
        var dmg = col.collider.GetComponent<EnemyHealth>();
        if (dmg != null)
            dmg.TakeDamage(10);
    }*/
}

    Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[3];

        vertices[0] = Vector3.zero;
        vertices[1] = new Vector3(Random.Range(0.1f, shardSize), 0, 0);
        vertices[2] = new Vector3(0, Random.Range(0.1f, shardSize), 0);

        mesh.vertices = vertices;

        mesh.triangles = new int[] { 0, 1, 2 };

        mesh.RecalculateNormals();

        return mesh;
    }
}



public class ExplosionDecal : MonoBehaviour
{
    public GameObject decalPrefab; // plane s texture kráteru
    public float lifeTime = 20f;

    public void SpawnDecal(Vector3 position)
    {
        RaycastHit hit;

        // hodíme ray dolů, aby to sedělo na terén
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 10f))
        {
            GameObject decal = Instantiate(
                decalPrefab,
                hit.point + Vector3.up * 0.01f,
                Quaternion.LookRotation(hit.normal)
            );

            // random rotace pro variaci
            decal.transform.Rotate(0, Random.Range(0, 360), 0);

            Destroy(decal, lifeTime);
        }
    }
}

public class ShardDamage : MonoBehaviour
{
    public int damage = 10;

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Enemy"))
        {
            /*var dmg = col.collider.GetComponent<EnemyHealth>();
            if (dmg != null)
                dmg.TakeDamage(damage);*/
        }
    }
}