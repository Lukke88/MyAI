using UnityEngine;

public class EnemyNavigator_new : MonoBehaviour
{
    public GameObject tank;

    public float speed = 5f;
    public float detectDistance = 80f;
    public float avoidDistance = 20f;

    public bool IsAvoidingBuilding = false;
    public bool IsRunningThroughCity = false;

    private Vector3 targetPos;
    private Vector3 pointA, pointB;
    private int avoidState = 0;

    void Update()
    {
        if (tank == null) return;

        if (IsRunningThroughCity)
        {
            CityMovement();
        }
        else
        {
            MoveDirectlyToTank();
        }
    }

    void CityMovement()
    {
        RaycastHit hit;

        // detekce budovy před sebou
        if (!IsAvoidingBuilding &&
            Physics.Raycast(transform.position, transform.forward, out hit, detectDistance))
        {
            if (hit.collider.CompareTag("Building"))
            {
                float dist = hit.distance;

                if (dist < avoidDistance)
                {
                    StartAvoid(hit);
                    return;
                }
            }
        }

        // avoidance logika
        if (IsAvoidingBuilding)
        {
            AvoidMovement();
            return;
        }

        // pohyb po "ulicích" (90°)
        Vector3 dir = (tank.transform.position - transform.position);

        // preferuj X osu
        if (Mathf.Abs(dir.x) > 5f)
        {
            targetPos = new Vector3(tank.transform.position.x, transform.position.y, transform.position.z);
        }
        else
        {
            targetPos = new Vector3(transform.position.x, transform.position.y, tank.transform.position.z);
        }

        MoveTo(targetPos);
    }

    void StartAvoid(RaycastHit hit)
    {
        IsAvoidingBuilding = true;
        avoidState = 0;

        Vector3 normal = hit.normal;

        // body kolem budovy
        pointA = hit.point + Vector3.Cross(Vector3.up, normal).normalized * 5f;
        pointB = hit.point - Vector3.Cross(Vector3.up, normal).normalized * 5f;
    }

    void AvoidMovement()
    {
        if (avoidState == 0)
        {
            MoveTo(pointA);

            if (Vector3.Distance(transform.position, pointA) < 1f)
                avoidState = 1;
        }
        else if (avoidState == 1)
        {
            MoveTo(pointB);

            if (Vector3.Distance(transform.position, pointB) < 1f)
            {
                IsAvoidingBuilding = false;
            }
        }
    }

    void MoveDirectlyToTank()
    {
        MoveTo(tank.transform.position);
    }

    void MoveTo(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position).normalized;

        transform.position += dir * speed * Time.deltaTime;

        // 🔥 rotace jen po 90°
        Vector3 snappedDir = new Vector3(
            Mathf.Round(dir.x),
            0,
            Mathf.Round(dir.z)
        );

        if (snappedDir != Vector3.zero)
            transform.forward = snappedDir;
    }
}