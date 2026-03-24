using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGoForwardToPlayer : MonoBehaviour
{
    [Header("Movement & Detection")]
    public GameObject enemy;
    public GameObject player, gunHolder;
    public GameObject[] walls;
    public GameObject nearest_wall_in_front;
    public float detectionRadius = 20f;
    public float moveSpeed = 3f;

    [Header("Gun & Shooting")]
    public GameObject gunPrefab;
    public GameObject projectilePrefab;
    public Transform muzzlePoint;          // konec hlavně
    public ParticleSystem muzzleFlashPrefab;
    public GameObject muzzlePrefab;        // WFX_MF 4P RIFLE1
    public AudioSource audioSource;
    public AudioClip fireSound;
    public float speed_ak47 = 200.0f;

    [Header("Fire Control")]
    public float fireCooldown = 0.4f;
    private float nextFireTime = 0f;
    private int bullet_counter;
    public bool canShoot = false;
public enum EnemyStateController
{
	Pursuit,
	Shoot
}
    void Start()
    {
        enemy = gameObject;
        player = GameObject.FindGameObjectWithTag("Player");
        walls = GameObject.FindGameObjectsWithTag("Wall");
    }

    void Update()
    {
        FindNearestWall();

        // pohyb AI pokud hráč v radiusu
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, player.transform.position);
        if (distanceToPlayer < detectionRadius)
        {
            Move30DegreesFromPlayer();
            AttachGunToEnemy();
        }

        AimGunAtPlayer();
        DetectPlayerAndLineOfSight();

        // střelba s cooldown
        if (Time.time > nextFireTime && canShoot)
        {
            Fire();
            SpawnMuzzleFlash();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void AttachGunToEnemy()
    {
        if (gunHolder == null) return;
        if (gunHolder.transform.childCount > 0) return;

        GameObject gunInstance = Instantiate(gunPrefab, gunHolder.transform);
        gunInstance.transform.localPosition = Vector3.zero;
        gunInstance.transform.localRotation = Quaternion.identity;
    }

    void AimGunAtPlayer()
    {
        if (gunHolder == null || player == null) return;

        Vector3 dir = player.transform.position - gunHolder.transform.position;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        gunHolder.transform.rotation = Quaternion.Slerp(gunHolder.transform.rotation, lookRot, Time.deltaTime * 6f);
    }

    void DetectPlayerAndLineOfSight()
    {
        if (player == null)
        {
            canShoot = false;
            return;
        }

        Vector3 dir = (player.transform.position - muzzlePoint.position).normalized;
        float distance = Vector3.Distance(muzzlePoint.position, player.transform.position);

        RaycastHit hit;
        if (Physics.Raycast(muzzlePoint.position, dir, out hit, distance))
        {
            // hráč je zakrytý překážkou
            if (hit.collider.CompareTag("Wall") ||
                hit.collider.CompareTag("building") ||
                hit.collider.CompareTag("vehicle") ||
                hit.collider.CompareTag("car"))
            {
                canShoot = false;
               // GetComponent<EnemyStateController>() = EnemyStateController.Pursuit;
                return;
            }
        }

        // nic nepřekáží → hráč viditelný
        canShoot = true;
    }

    void Fire()
    {
        if (gunHolder == null || projectilePrefab == null) return;

        Vector3 spawnPos = muzzlePoint.position;
        Vector3 dir = muzzlePoint.forward;

        GameObject generated_projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        generated_projectile.name = "enemy_bullet_" + bullet_counter;

        // Trail
        TrailRenderer tr = generated_projectile.AddComponent<TrailRenderer>();
        tr.time = 1.8f;
        tr.startWidth = 0.8f;
        tr.endWidth = 0.1f;
        Material glowMat = new Material(Shader.Find("Sprites/Default"));
        tr.material = glowMat;
        tr.startColor = new Color(1f, 0.6f, 0.1f, 1f);
        tr.endColor = new Color(1f, 0.4f, 0.1f, 0f);

        // Rigidbody
        Rigidbody rb = generated_projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = dir.normalized * speed_ak47;
        }

        // muzzle flash (na konci hlavně)
        if (muzzleFlashPrefab != null)
        {
            ParticleSystem flash = Instantiate(muzzleFlashPrefab, spawnPos, Quaternion.LookRotation(dir));
            flash.Play();
            Destroy(flash.gameObject, 1f);
        }

        // zvuk
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        bullet_counter++;
    }

    void SpawnMuzzleFlash()
    {
        if (muzzlePrefab == null || muzzlePoint == null) return;

        ParticleSystem flash = Instantiate(muzzlePrefab, muzzlePoint.position, muzzlePoint.rotation).GetComponent<ParticleSystem>();
        flash.Play();
        Destroy(flash.gameObject, 1f);
    }

    void FindNearestWall()
    {
        float minDist = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject wall in walls)
        {
            Vector3 dirToWall = wall.transform.position - enemy.transform.position;

            if (Vector3.Dot(enemy.transform.forward, dirToWall) > 0)
            {
                float dist = dirToWall.magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = wall;
                }
            }
        }

        nearest_wall_in_front = closest;
    }

    void Move30DegreesFromPlayer()
    {
        Vector3 dirToPlayer = (player.transform.position - enemy.transform.position).normalized;
        Vector3 cross = Vector3.Cross(enemy.transform.forward, dirToPlayer);

        float angle = 30f;
        if (cross.y > 0)
        {
            transform.Rotate(0, angle * Time.deltaTime, 0);
        }
        else
        {
            transform.Rotate(0, -angle * Time.deltaTime, 0);
        }

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}