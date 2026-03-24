using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedAI_FlankingHelicopterBehaviour : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform crashedHelicopter;
    public GameObject weaponPrefab;

    [Header("AI Settings")]
    public float moveSpeed = 12f;
    public float flankDistance = 12f;    // vzdálenost od vrtulníku
    public float shootingRange = 200f;
    public float shootingCooldown = 2f;
    public int flankAngle = 45;          // vejíř ±45°
    
    [Header("Manager Settings")]
    public int enemyIndex = 0;           // index nepřítele v manageru
    public int totalEnemies = 1;         // celkový počet nepřátel, aby se flank správně rozprostřel

    private Vector3 targetFlankPoint;
    private float shootTimer = 0f;

    private enum AIState { Idle, ApproachingHelicopter, Flanking, AttackPlayer }
    private AIState currentState = AIState.Idle;

    void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
        currentState = AIState.ApproachingHelicopter;
        AssignFlankPoint();
    }

    void Update()
    {
        // Dynamické flankování kolem vrtulníku podle indexu a celkového počtu
        AssignFlankPoint();

        switch (currentState)
        {
            case AIState.ApproachingHelicopter:
                MoveTo(crashedHelicopter.position);
                if (Vector3.Distance(transform.position, crashedHelicopter.position) <= flankDistance + 2f)
                {
                    currentState = AIState.Flanking;
                }
                break;

            case AIState.Flanking:
                MoveTo(targetFlankPoint);
                if (Vector3.Distance(transform.position, targetFlankPoint) < 1f)
                {
                    if (Vector3.Distance(transform.position, player.position) <= shootingRange)
                        currentState = AIState.AttackPlayer;
                }
                break;

            case AIState.AttackPlayer:
                FaceTarget(player.position);
                shootTimer += Time.deltaTime;
                if (shootTimer >= shootingCooldown)
                {
                    FireWeapon();
                    shootTimer = 0f;
                }

                // pokud hráč uteče, vrátíme se na flank pozici
                if (Vector3.Distance(transform.position, player.position) > shootingRange)
                {
                    currentState = AIState.Flanking;
                }
                break;

            case AIState.Idle:
                break;
        }
    }

    void MoveTo(Vector3 destination)
    {
        Vector3 nextPoint = CalculateAvoidancePosition(destination);
        transform.position = Vector3.MoveTowards(transform.position, nextPoint, moveSpeed * Time.deltaTime);
        FaceTarget(nextPoint);
    }

    void FaceTarget(Vector3 point)
    {
        Vector3 dir = point - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    void FireWeapon()
    {
        if (weaponPrefab != null)
        {
            Instantiate(weaponPrefab, transform.position + transform.forward * 1f, transform.rotation);
        }
    }

    void AssignFlankPoint()
    {
        if (totalEnemies <= 1)
        {
            // pokud je jen jeden enemy, vezmeme náhodný úhel ±flankAngle
            float angle = Random.Range(-flankAngle, flankAngle);
            Vector3 forward = (player.position - crashedHelicopter.position).normalized;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            targetFlankPoint = crashedHelicopter.position + rot * forward * flankDistance;
        }
        else
        {
            // dynamický vejíř: každý enemy dostane svůj úhel
            float totalAngle = flankAngle * 2f;
            float angleStep = totalAngle / (totalEnemies - 1);
            float angle = -flankAngle + angleStep * enemyIndex;

            Vector3 forward = (player.position - crashedHelicopter.position).normalized;
            Quaternion rot = Quaternion.Euler(0, angle, 0);
            targetFlankPoint = crashedHelicopter.position + rot * forward * flankDistance;
        }
    }

    Vector3 CalculateAvoidancePosition(Vector3 currentPos, Vector3 targetPos)
    {
        Vector3 dir = targetPos - currentPos;
        RaycastHit hit;
        if (Physics.Raycast(currentPos, dir.normalized, out hit, dir.magnitude))
        {
            Vector3 hitNormal = hit.normal;
            float offset = 5f;
            Vector3 pointA = hit.point + Vector3.Cross(Vector3.up, hitNormal).normalized * offset;
            Vector3 pointB = hit.point - Vector3.Cross(Vector3.up, hitNormal).normalized * offset;
            return (Vector3.Distance(currentPos, pointA) < Vector3.Distance(currentPos, pointB)) ? pointA : pointB;
        }
        return targetPos;
    }

    // Přetížení pro volání jen s cílem
    Vector3 CalculateAvoidancePosition(Vector3 targetPos)
    {
        return CalculateAvoidancePosition(transform.position, targetPos);
    }
}