using UnityEngine;

public class BeamBehaviour2 : MonoBehaviour
{
    public GameObject beam, explosionPrefab, generatedExplosion;
    public bool IsTriggered;
    public float travelling_speed = 120.0f;
	public GameObject[] allBuildings;
    public GameObject craterPrefab;
    public AudioClip wall_destruction_audio;
	public Vector3 hitPoint, hitNormal;
    public ParticleSystem blood_splatter;
    public AudioClip enemy_hit_audio;

    private AudioSource audioSource;
	public float damage = 5f;
	public float ContactDistance = 30.0f;
    public void Start()
    {
        beam = GameObject.Find(this.name);
        IsTriggered = true;
        audioSource = gameObject.AddComponent<AudioSource>();
		
		explosionPrefab =
                GameObject.Find("WFX_Nuke");
		allBuildings = GameObject.FindGameObjectsWithTag("building");
    }

    public void Update()
    {
        if (IsTriggered == true && this.name.Contains("Clone"))
        {
            beam.transform.Translate(0, travelling_speed, 0);
        }
		if(explosionPrefab==null)
			explosionPrefab =
                GameObject.Find("WFX_Nuke");
				
		foreach(GameObject go in allBuildings)
		{
			if(beam!=null && Vector3.Distance(go.transform.position, beam.transform.position)<=ContactDistance)
			{
				if(hitPoint!=Vector3.zero)
				{
					generatedExplosion = Instantiate(explosionPrefab,hitPoint,Quaternion.identity);
					ParticleSystem ps = generatedExplosion.transform.GetComponent<ParticleSystem>();
					ps.Play();
					Destroy(generatedExplosion, ps.main.duration);
				}
			}
		}
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];

        hitPoint = contact.point;
        hitNormal = contact.normal;

        // =========================
        // ENEMY HIT
        // =========================
        // =========================
// ENEMY HIT
// =========================
if (collision.gameObject.CompareTag("Enemy"))
{
    // BLOOD EFFECT
    if (blood_splatter != null)
    {
        ParticleSystem blood =
            Instantiate(
                blood_splatter,
                hitPoint,
                Quaternion.LookRotation(hitNormal)
            );

        blood.Play();

        Destroy(blood.gameObject, 5f);
    }

    // SOUND
    if (enemy_hit_audio != null)
    {
        AudioSource.PlayClipAtPoint(
            enemy_hit_audio,
            hitPoint,
            1.0f
        );
    }

    // DAMAGE
    EnemyHealth enemyHealth =
        collision.gameObject.GetComponent<EnemyHealth>();

    if (enemyHealth != null)
    {
        enemyHealth.TakeDamage(damage);
    }

    Destroy(gameObject);

    return;
}

        // =========================
        // WALL / BUILDING
        // =========================
        if (collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("building"))
        {
            

            if (explosionPrefab == null)
            {
                explosionPrefab =
                    GameObject.Find("WFX_Nuke");
            }

            if (explosionPrefab != null)
            {
                GameObject exp =
                    Instantiate(explosionPrefab, hitPoint, Quaternion.identity);

                exp.transform.localScale = Vector3.one * 5.18f;
                Destroy(exp, 8f);
            }

            if (craterPrefab != null)
            {
                Quaternion craterRotation =
                    Quaternion.LookRotation(hitNormal);

                craterRotation *= Quaternion.Euler(90f, 0f, 0f);

                GameObject crater =
                    Instantiate(
                        craterPrefab,
                        hitPoint + hitNormal * 0.02f,
                        craterRotation
                    );

                crater.transform.localScale = new Vector3(1.8f, 1.8f, 1.8f);
                crater.transform.SetParent(collision.transform);
            }

            if (wall_destruction_audio != null)
            {
                AudioSource.PlayClipAtPoint(wall_destruction_audio, hitPoint, 1.0f);
            }

            Destroy(gameObject);
        }
    }
}