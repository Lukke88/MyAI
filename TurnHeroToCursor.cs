using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnHeroToCursor : MonoBehaviour
{
    public Transform cursorMarker;
    public Transform weaponHolder;
    public GameObject gunPrefab, muzzlePoint, generated_projectile;

    public GameObject projectilePrefab;
	
	public ParticleSystem muzzleFlashPrefab;
public Light muzzleLight;
public AudioClip fireSound;
public AudioSource audioSource;



    GameObject currentGun;

    void Start()
    {
        projectilePrefab = GameObject.Find("Bullet_original");

        // vytvoření zbraně
        if (weaponHolder.childCount == 0)
        {
            currentGun = Instantiate(gunPrefab, weaponHolder);
            currentGun.transform.localPosition = Vector3.zero;
            currentGun.transform.localRotation = Quaternion.identity;
        }
		// nastavení transformu FiringHolder
    weaponHolder.localPosition = new Vector3(0.732f, 2.4025f, 0.875f);
    weaponHolder.localScale = new Vector3(0.5422f, 0.5422f, 0.5422f);

    // vytvoření zbraně
	/*
    if (weaponHolder.childCount == 0)
    {
        currentGun = Instantiate(gunPrefab, weaponHolder);
        currentGun.transform.localPosition = Vector3.zero;
        currentGun.transform.localRotation = Quaternion.identity;
    }*/
    }

    void Update()
    {
        TurnToCursor();
		projectilePrefab = GameObject.Find("Bullet_original");
		weaponHolder.localPosition = new Vector3(1.25f, 2.4025f, 0.875f);
		muzzlePoint = weaponHolder.GetChild(0).GetChild(1).gameObject;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    void TurnToCursor()
    {
        if (cursorMarker == null) return;

        Vector3 direction = cursorMarker.position - transform.position;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * 10f
        );
    }
	public float speed_ak47  = 200.0f;
	public float speed_sniper_gun = 400.0f;
int bullet_counter;
  void Fire()
{
    if (weaponHolder == null) return;

    Vector3 spawnPos = muzzlePoint.transform.position;
    Vector3 dir = muzzlePoint.transform.forward;

    GameObject generated_projectile =
        Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));

    generated_projectile.name = "gen_bullet_" + bullet_counter.ToString();

    // ===== TrailRenderer (Glow + kometový ohon) =====
    TrailRenderer tr = generated_projectile.AddComponent<TrailRenderer>();
    tr.time = 2.4f;
    tr.startWidth = 1.18f;
    tr.endWidth = 0.2f;

    Material glowMat = new Material(Shader.Find("Sprites/Default"));
    tr.material = glowMat;

    tr.startColor = new Color(1f, 1f, 0.2f, 1f);
    tr.endColor = new Color(1f, 1f, 0f, 0f);

    // ===== Rigidbody =====
    Rigidbody rb = generated_projectile.GetComponent<Rigidbody>();

    if (rb != null)
    {
        rb.useGravity = false;
        rb.velocity = dir.normalized * speed_ak47;
    }

    // ===== MUZZLE FLASH VFX =====
    if (muzzleFlashPrefab != null)
    {
        ParticleSystem flash =
            Instantiate(muzzleFlashPrefab, spawnPos, Quaternion.LookRotation(dir));

        flash.Play();
        Destroy(flash.gameObject, 1f);
    }

    // ===== Light flash =====
    if (muzzleLight != null)
    {
        StartCoroutine(MuzzleLightFlash());
    }

    // ===== Sound =====
    if (audioSource != null && fireSound != null)
    {
        audioSource.PlayOneShot(fireSound);
    }

    StartCoroutine(DestroyWhenFar(generated_projectile, transform.position));

    bullet_counter++;
}

IEnumerator MuzzleLightFlash()
{
    muzzleLight.enabled = true;
    yield return new WaitForSeconds(0.05f);
    muzzleLight.enabled = false;
}

IEnumerator DestroyWhenFar(GameObject proj, Vector3 playerPos)
{
    while (proj != null)
    {
        if (Vector3.Distance(playerPos, proj.transform.position) > 2800f)
        {
            Destroy(proj);
            yield break;
        }

        yield return null;
    }
}

    IEnumerator MoveProjectile(GameObject proj, Vector3 dir)
    {
        float speed = speed_ak47;

        while (proj != null)
        {
            proj.transform.position += dir * speed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator DestroyShot(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }
}