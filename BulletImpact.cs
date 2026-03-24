using UnityEngine;

public class BulletImpact : MonoBehaviour
{
    public GameObject dustPrefab;       // malý prach při dopadu
    public GameObject hitTexturePrefab; // 2D textura dopadu
    public float maxDistance = 2800f;

    private Vector3 spawnPos;

    void Start()
    {
        spawnPos = transform.position;
    }

    void Update()
    {
        // Znič kulku pokud je moc daleko od hráče
        if (Vector3.Distance(spawnPos, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // ===== Dopad na zeď =====
        if (collision.collider.CompareTag("Wall"))
        {
            // malý prach
            if (dustPrefab != null)
            {
                Instantiate(dustPrefab, collision.contacts[0].point, Quaternion.identity);
            }

            // 2D textura dopadu
            if (hitTexturePrefab != null)
            {
                Instantiate(hitTexturePrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            }
        }

        // ===== Dopad na nepřítele =====
        if (collision.collider.CompareTag("Enemy"))
        {
            Animator anim = collision.collider.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetInteger("IsTalibDeathFallBack", 1);
                anim.Play("TalibDeathFall");
            }
        }

        // Znič kulku po dopadu
        Destroy(gameObject);
    }
}