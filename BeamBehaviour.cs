using UnityEngine;

public class BeamBehaviour : MonoBehaviour
{
    public float destroyDelay = 0.05f;

    [Header("Wall / Building Hit")]
    public GameObject bulletHoleDecal;
    public ParticleSystem dustImpactFX;

    [Header("Enemy Hit")]
    public GameObject bloodSplatterPrefab;
    public AudioClip enemyHitSound;
	public enum HitSurfaceType
	{
		Wall,
		Building,
		Car,
		Tank,
		Enemy
	};
    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;
        ContactPoint contact = collision.contacts[0];

        HitSurfaceType surfaceType = ResolveSurfaceType(hitObject);

        switch (surfaceType)
        {
            case HitSurfaceType.Wall:
            case HitSurfaceType.Building:
            case HitSurfaceType.Car:
            case HitSurfaceType.Tank:
                HandleWallImpact(contact);
                break;

            case HitSurfaceType.Enemy:
                HandleEnemyImpact(hitObject, contact);
                break;
        }

        Destroy(gameObject, destroyDelay);
    }

    HitSurfaceType ResolveSurfaceType(GameObject obj)
    {
        if (obj.CompareTag("Enemy")) return HitSurfaceType.Enemy;
        if (obj.CompareTag("wall")) return HitSurfaceType.Wall;
        if (obj.CompareTag("building")) return HitSurfaceType.Building;
        if (obj.CompareTag("car")) return HitSurfaceType.Car;
        if (obj.CompareTag("tank")) return HitSurfaceType.Tank;

        return HitSurfaceType.Wall;
    }

    void HandleWallImpact(ContactPoint contact)
    {
        // 🕳 bullet hole
        if (bulletHoleDecal != null)
        {
            Quaternion rot = Quaternion.LookRotation(contact.normal);
            Vector3 pos = contact.point + contact.normal * 0.01f;

            Instantiate(bulletHoleDecal, pos, rot);
        }

        // 🌫 prach
        if (dustImpactFX != null)
        {
            ParticleSystem dust = Instantiate(
                dustImpactFX,
                contact.point,
                Quaternion.LookRotation(contact.normal)
            );
            Destroy(dust.gameObject, 2f);
        }
    }

    void HandleEnemyImpact(GameObject enemy, ContactPoint contact)
    {
        // 🩸 krev (zadní strana)
        Transform bloodPoint = enemy.transform.Find("BloodPoint");

        if (bloodPoint != null && bloodSplatterPrefab != null)
        {
            Instantiate(
                bloodSplatterPrefab,
                bloodPoint.position,
                Quaternion.LookRotation(-contact.normal)
            );
        }

        // 🔊 zvuk
        AudioSource audio = enemy.GetComponent<AudioSource>();
        if (audio != null && enemyHitSound != null)
        {
            audio.PlayOneShot(enemyHitSound);
        }

        // 🎞 animace zásahu
        Animator anim = enemy.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        // 💀 (zatím instant kill – později damage system)
        Destroy(enemy, 0.1f);
    }
}
