using UnityEngine;

public class TankTurret : MonoBehaviour
{
    [Header("Turret Settings")]
    public GameObject turret;       // hlavní objekt věže
    public GameObject barrel;       // hlavice kanónu
    public float rotateSpeed = 30f; // rychlost natáčení věže
    public float minBarrelAngle = -5f;
    public float maxBarrelAngle = 30f;

    [Header("Projectile Settings")]
    public GameObject prefab_Projectile;
    public Transform spawnPoint;    // místo, odkud se projektil spawnuje
    public float projectileSpeed = 50f;
    public float gravity = 9.81f;

    [Header("Range & Visualization")]
    public float minRange = 200f;
    public float maxRange = 2000f;
    public bool showTrajectory = true; // zapínání/vypínání trajektorie

    private float barrelAngle = 0f;
    private Vector3 lastTrajectoryTarget;

    void Update()
    {
        HandleTurretRotation();
        HandleBarrelAngle();
        if (showTrajectory) DrawTrajectory();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireProjectile();
        }
    }

    void HandleTurretRotation()
    {
        float turnInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) turnInput = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) turnInput = 1f;

        turret.transform.Rotate(0f, turnInput * rotateSpeed * Time.deltaTime, 0f);
    }

    void HandleBarrelAngle()
    {
        float angleInput = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) angleInput = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) angleInput = -1f;

        barrelAngle += angleInput * rotateSpeed * Time.deltaTime;
        barrelAngle = Mathf.Clamp(barrelAngle, minBarrelAngle, maxBarrelAngle);

        // natáčení barrelu lokálně kolem x
        Vector3 localEuler = barrel.transform.localEulerAngles;
        localEuler.x = barrelAngle;
        barrel.transform.localEulerAngles = localEuler;
    }

    void DrawTrajectory()
    {
        // základní odhad dopadové vzdálenosti podle úhlu hlavně
        float angleRad = Mathf.Deg2Rad * barrelAngle;
        float velocity = projectileSpeed;

        Vector3 startPos = spawnPoint.position;
        Vector3 direction = barrel.transform.forward;

        // počet bodů trajektorie
        int points = 50;
        float timeStep = 0.1f;

        Vector3 previousPoint = startPos;
        for (int i = 0; i < points; i++)
        {
            float t = i * timeStep;
            // jednoduchá balistika: x = vt, y = vt*sin - 0.5*g*t^2
            Vector3 nextPoint = startPos + direction * velocity * t;
            nextPoint.y = startPos.y + Mathf.Sin(angleRad) * velocity * t - 0.5f * gravity * t * t;

            Debug.DrawLine(previousPoint, nextPoint, Color.blue);
            previousPoint = nextPoint;

            // pokud dopadne na zem
            if (nextPoint.y <= 0f)
            {
                lastTrajectoryTarget = nextPoint;
                break;
            }
        }
    }

    void FireProjectile()
    {
        if (prefab_Projectile == null || spawnPoint == null) return;

        GameObject proj = Instantiate(prefab_Projectile, spawnPoint.position, Quaternion.identity);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        rb.useGravity = true;

        // směr a rychlost podle barrelu
        Vector3 launchDir = barrel.transform.forward;
        rb.velocity = launchDir * projectileSpeed;
    }
}