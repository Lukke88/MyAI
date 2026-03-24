using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIAttacksToPlayer : MonoBehaviour
{
    public GameObject nearestPlayer, enemy;

    public float detectionRadius = 100f;
    public float fireRate = 1.5f;
    public float projectileSpeed = 20f;
    public float spreadAngle = 5f;

    private float fireTimer;

    void Update()
    {
		enemy = GameObject.Find(this.name);
        FindNearestPlayer();

        if (nearestPlayer == null)
            return;

        RotateToTarget();

        fireTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot();
        }
    }

    void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float shortestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closest = player;
            }
        }

        if (shortestDistance <= detectionRadius)
            nearestPlayer = closest;
        else
            nearestPlayer = null;
    }

    void RotateToTarget()
    {
        Vector3 direction = (nearestPlayer.transform.position - transform.position).normalized;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    void Shoot()
    {
        Vector3 targetPosition = nearestPlayer.transform.position;

        Vector3 direction = (targetPosition - transform.position).normalized;

        // Spread ±5°
        float randomAngle = Random.Range(-spreadAngle, spreadAngle);
        direction = Quaternion.Euler(0, randomAngle, 0) * direction;

        // Create projectile (empty object)
        GameObject projectile = new GameObject("EnemyProjectile");
        projectile.transform.position = transform.position + transform.forward * 1.5f;

        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = direction * projectileSpeed;

        // Add LineRenderer
        LineRenderer lr = projectile.AddComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;

        lr.SetPosition(0, projectile.transform.position);
        lr.SetPosition(1, projectile.transform.position + direction * 20f);

        Destroy(projectile, 2f);
    }
}
