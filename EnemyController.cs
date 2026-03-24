using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Transform player;
    public Transform coverPoint;

    public float moveSpeed = 3f;
    public float shootDistance = 20f;
    public float fireCooldown = 2f;

    public float circleSegmentAngle = 35f;
    public float circleRadiusOffset = 1.5f;

    public GameObject bulletPrefab;
    public Transform muzzlePoint;

    private float fireTimer;
    private Vector3 circleTargetPos;

    private enum EnemyState
    {
        Hidden,
        MovingOut,
        Shooting,
        Returning
    }

    private EnemyState state = EnemyState.Hidden;

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null || coverPoint == null) return;

        fireTimer += Time.deltaTime;

        switch (state)
        {
            case EnemyState.Hidden:
                HiddenState();
                break;

            case EnemyState.MovingOut:
                MoveOutState();
                break;

            case EnemyState.Shooting:
                ShootState();
                break;

            case EnemyState.Returning:
                ReturnState();
                break;
        }
    }

    // ===== STATES =====

    void HiddenState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < shootDistance && fireTimer > fireCooldown)
        {
            CalculateCircleAttackPosition();
            state = EnemyState.MovingOut;
        }
    }

    void MoveOutState()
    {
        MoveTowards(circleTargetPos);

        if (Vector3.Distance(transform.position, circleTargetPos) < 0.5f)
        {
            state = EnemyState.Shooting;
        }
    }

    void ShootState()
    {
        Shoot();

        fireTimer = 0f;
        state = EnemyState.Returning;
    }

    void ReturnState()
    {
        MoveTowards(coverPoint.position);

        if (Vector3.Distance(transform.position, coverPoint.position) < 0.5f)
        {
            state = EnemyState.Hidden;
        }
    }

    // ===== CORE FUNCTIONS =====

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;

        // Bullet avoidance
        Vector3 avoidance = BulletAvoidance();
        dir += avoidance * 0.7f;

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(player.position - transform.position),
            Time.deltaTime * 5f
        );
    }

    Vector3 BulletAvoidance()
    {
        Vector3 avoidVector = Vector3.zero;

        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");

        foreach (var b in bullets)
        {
            float dist = Vector3.Distance(transform.position, b.transform.position);

            if (dist < 5f)
            {
                avoidVector -= (b.transform.position - transform.position).normalized;
            }
        }

        return avoidVector;
    }

    void CalculateCircleAttackPosition()
    {
        float radius = Vector3.Distance(player.position, coverPoint.position);

        float baseAngle = Mathf.Atan2(
            player.position.z - coverPoint.position.z,
            player.position.x - coverPoint.position.x
        );

        float offset = Random.Range(-circleSegmentAngle, circleSegmentAngle) * Mathf.Deg2Rad;

        float finalAngle = baseAngle + offset;

        circleTargetPos = player.position +
            new Vector3(
                Mathf.Cos(finalAngle),
                0,
                Mathf.Sin(finalAngle)
            ) * radius;
    }

    void Shoot()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            muzzlePoint.position,
            Quaternion.LookRotation(player.position - transform.position)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 dir = (player.position - muzzlePoint.position).normalized;
            rb.velocity = dir * 30f;
        }
    }
}