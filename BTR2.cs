using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BTR2 : MonoBehaviour
{
    [Header("References")]
    public GameObject armored_car;
    public GameObject main_hero;
    public GameObject cursor_square;
    public GameObject turret;
    public Transform cannonMuzzle;
    public Transform target;

    [Header("Movement")]
    public float forwardSpeed = 8f;
    public float backwardSpeed = 5f;
    public float turnSpeed = 90f;

    [Header("Ballistics")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float ballisticHeight = 8f;
    public int ballisticResolution = 30;

    [Header("FX")]
    public GameObject muzzleFlashPrefab;
    public GameObject impactExplosionPrefab;
    public AudioClip fireSound;

    private Camera mainCamera;
    private AudioSource audioSource;
    private Vector3 hitPoint;
    private bool isActivated;

    private List<Transform> wheels = new List<Transform>();

    // ================= UNITY =================

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        mainCamera = Camera.main;
        cursor_square = GameObject.Find("cursor_square");

        if (armored_car != null)
            FindAndAssignWheels(armored_car.transform);
    }

    void Update()
    {
        CheckActivation();
        HandleVehicleMovement();
        UpdateCursorOnTerrain();

        if (Input.GetKeyDown(KeyCode.Space) && target != null)
            FireProjectile(target.position);
    }

    // ================= VEHICLE =================

    void HandleVehicleMovement()
    {
        float move = 0f;
        float turn = 0f;

        if (Input.GetKey(KeyCode.W)) move = -1f;
        if (Input.GetKey(KeyCode.S)) move = 1f;
        if (Input.GetKey(KeyCode.A)) turn = -1f;
        if (Input.GetKey(KeyCode.D)) turn = 1f;

        float speed = move >= 0 ? forwardSpeed : backwardSpeed;

        transform.Translate(Vector3.right * move * speed * Time.deltaTime);
        transform.Rotate(Vector3.forward, turn * turnSpeed * Time.deltaTime);

       // RotateWheels(move, speed);
    }

    // ================= SHOOTING =================

    void FireProjectile(Vector3 targetPos)
    {
        if (projectilePrefab == null || cannonMuzzle == null) return;

        GameObject proj = Instantiate(projectilePrefab, cannonMuzzle.position, cannonMuzzle.rotation);

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, cannonMuzzle.position, cannonMuzzle.rotation);
            Destroy(flash, 2f);
        }

        if (fireSound != null)
            audioSource.PlayOneShot(fireSound);

        StartCoroutine(MoveProjectileAlongBallistic(proj, cannonMuzzle.position, targetPos));
    }

    IEnumerator MoveProjectileAlongBallistic(GameObject projectile, Vector3 start, Vector3 end)
    {
        float t = 0f;
        Vector3 prev = start;

        while (t < 1f)
        {
            t += Time.deltaTime * projectileSpeed / Vector3.Distance(start, end);
            Vector3 next = GetBallisticPoint(start, end, t);

            projectile.transform.position = next;

            Vector3 dir = (next - prev).normalized;
            if (dir != Vector3.zero)
                projectile.transform.rotation = Quaternion.LookRotation(dir);

            prev = next;
            yield return null;
        }

        if (impactExplosionPrefab != null)
            Instantiate(impactExplosionPrefab, projectile.transform.position, Quaternion.identity);

        Destroy(projectile);
    }

    Vector3 GetBallisticPoint(Vector3 start, Vector3 end, float t)
    {
        Vector3 pos = Vector3.Lerp(start, end, t);
        pos.y += Mathf.Sin(Mathf.PI * t) * ballisticHeight;
        return pos;
    }

    // ================= HELPERS =================

    void UpdateCursorOnTerrain()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 3000f))
        {
            hitPoint = hit.point;
            if (cursor_square != null)
                cursor_square.transform.position = hitPoint + Vector3.up * 0.05f;

            target = cursor_square.transform;
        }
    }

    void CheckActivation()
    {
        if (main_hero == null || armored_car == null) return;

        if (Vector3.Distance(armored_car.transform.position, main_hero.transform.position) < 5f)
            isActivated = true;
    }

    void FindAndAssignWheels(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains("wheel"))
                wheels.Add(child);

            FindAndAssignWheels(child);
        }
    }

    void RotateWheels(float input, float speed)
    {
        float rot = input * speed * Time.deltaTime * 360f;
        foreach (Transform w in wheels)
            w.Rotate(Vector3.up, rot);
    }
}
