
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDetectWallInFront : MonoBehaviour
{
    public GameObject projectile;
    public GameObject[] allWalls;

    [Header("Impact Effects")]
    public GameObject craterPrefab;
    public GameObject dirtParticlePrefab;

    [Header("Impact Audio")]
    public AudioClip impactAudioClip;
    public float impactVolume = 1.0f;

    [Header("Raycast Settings")]
    public float ray_length = 200.0f;

    private bool impactDetected;

    void Start()
    {
        projectile = gameObject;

        allWalls = GameObject.FindGameObjectsWithTag("Wall");
		
		
    }

    void Update()
    {
        if (projectile != null && impactDetected == false)
        {
            CreateRaycastToWallDetection();
        }
    }

    // ------------------------------------------------
    // 1. DETECT WALL OR OTHER OBSTACLE
    // ------------------------------------------------

    public void CreateRaycastToWallDetection()
    {
        Vector3 direction = projectile.transform.forward;

        Vector3 origin = projectile.transform.position;

        Ray ray = new Ray(origin, direction);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, ray_length,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Wall") ||
                hit.collider.CompareTag("building") ||
                hit.collider.CompareTag("PlayerVehicle") ||
                hit.collider.CompareTag("Barricade"))
            {
                impactDetected = true;

                CreateCraterAtHitPoint(hit);

                FlushDirtOut(hit);

                PlayImpactAudioClip(hit.point);

                Destroy(projectile);
            }
        }
    }

    // ------------------------------------------------
    // 2. CREATE CRATER AT HIT POINT
    // ------------------------------------------------

    public void CreateCraterAtHitPoint(RaycastHit hit)
    {
        if (craterPrefab == null)
            return;

        Quaternion craterRotation =
            Quaternion.LookRotation(hit.normal);

        Vector3 craterPosition =
            hit.point + hit.normal * 0.02f;

        Instantiate(
            craterPrefab,
            craterPosition,
            craterRotation
        );
    }

    // ------------------------------------------------
    // 3. FLUSH DIRT OUT
    // ------------------------------------------------

    public void FlushDirtOut(RaycastHit hit)
    {
        if (dirtParticlePrefab == null)
            return;

        Quaternion dirtRotation =
            Quaternion.LookRotation(hit.normal);

        Vector3 dirtPosition =
            hit.point + hit.normal * 0.05f;

        GameObject dirt = Instantiate(
            dirtParticlePrefab,
            dirtPosition,
            dirtRotation
        );

        ParticleSystem particles =
            dirt.GetComponent<ParticleSystem>();

        if (particles != null)
        {
            particles.Play();

            float duration =
                particles.main.duration +
                particles.main.startLifetime.constantMax;

            Destroy(dirt, duration);
        }
        else
        {
            Destroy(dirt, 5.0f);
        }
    }

    // ------------------------------------------------
    // 4. PLAY IMPACT AUDIO
    // ------------------------------------------------

    public void PlayImpactAudioClip(Vector3 hitPosition)
    {
        if (impactAudioClip == null)
            return;

        AudioSource.PlayClipAtPoint(
            impactAudioClip,
            hitPosition,
            impactVolume
        );
    }
}
