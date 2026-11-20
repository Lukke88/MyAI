
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectsBestAttackAngle : MonoBehaviour
{
    [Header("Target")]
    public GameObject playerTarget;
    public Vector3 lastKnownPlayerPosition;
    public bool playerPositionKnown;

    [Header("Barricades")]
    public GameObject[] allBarricades;
    public GameObject bestBarricade;
    public GameObject movementTarget;

    [Header("Fire Points")]
    public int numberOfFirePoints = 5;
    public float distanceBehindBarricade = 20.0f;
    public float randomSelection = 1.0f;

    [Header("Detection")]
    public float maxBarricadeDistance = 1500f;
    public float targetHeight = 1.0f;

    [Header("Tactical Movement")]
    public float arrivalDistance = 30f;
    public float searchInterval = 2f;
    public bool isSearchingForAttackPosition;
    public bool isMovingToBarricade;

    private ArabEnemyRunsToFinalPosition navigation;
    private float nextSearchTime;

    private List<Transform> triedFirePoints =
        new List<Transform>();

    private List<Transform> availableFirePoints =
        new List<Transform>();

    private Transform selectedFirePoint;

    void Start()
    {
        navigation = GetComponent<ArabEnemyRunsToFinalPosition>();

        FindAllBarricades();

        foreach (GameObject barricade in allBarricades)
        {
            if (barricade != null)
            {
                CreateFirePoints(
                    barricade,
                    barricade.transform.position,
                    "FirePoint_",
                    numberOfFirePoints
                );
            }
        }
    }

    void Update()
    {
        if (isSearchingForAttackPosition &&
            Time.time >= nextSearchTime &&
            !isMovingToBarricade)
        {
            nextSearchTime = Time.time + searchInterval;
            SearchAndMoveToBestBarricade();
        }

        if (isMovingToBarricade)
        {
            CheckBarricadeArrival();
        }
    }

    // CREATE FIVE FIRE POINTS AT EACH BARRICADE

    public void CreateFirePoints(
        GameObject barricade,
        Vector3 barricadePosition,
        string generalName,
        int numberOfPositions)
    {
        if (barricade == null)
            return;

        Collider barricadeCollider =
            barricade.GetComponent<Collider>();

        if (barricadeCollider == null)
        {
            Debug.LogWarning(
                "Barricade has no Collider: " + barricade.name
            );
            return;
        }

        float sizeX = barricadeCollider.bounds.size.x;
        float gapX = sizeX / numberOfPositions;

        for (int i = 0; i < numberOfPositions; i++)
        {
            string pointName = generalName + (i + 1).ToString("00");

            // Avoid creating duplicates if Start is called again.

            Transform existing =
                barricade.transform.Find(pointName);

            if (existing != null)
                continue;

            float x =
                -sizeX / 2f + gapX * (i + 0.5f);

            Vector3 localOffset =
                new Vector3(x, 0f, -distanceBehindBarricade);

            GameObject firePoint = new GameObject(pointName);

            firePoint.transform.SetParent(
                barricade.transform,
                false
            );

            firePoint.transform.localPosition = localOffset;
            firePoint.transform.localRotation = Quaternion.identity;
        }
    }

    // FIND ALL BARRICADES

    public void FindAllBarricades()
    {
        allBarricades =
            GameObject.FindGameObjectsWithTag("Barricade");
    }

    // ENEMY HEARS PLAYER SOUND

    public void HearPlayerSound(GameObject player)
    {
        if (player == null)
            return;

        playerTarget = player;

        lastKnownPlayerPosition = player.transform.position;
        playerPositionKnown = true;

        isSearchingForAttackPosition = true;
        isMovingToBarricade = false;

        triedFirePoints.Clear();
        selectedFirePoint = null;

        Debug.Log("Enemy heard player! Searching for firing position.");
    }

    // UPDATE LAST KNOWN PLAYER POSITION

    public void UpdateLastKnownPlayerPosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
        playerPositionKnown = true;
    }

    // CHECK LINE OF SIGHT FROM FIRE POINT

    public bool CanShootPlayerFromFirePoint(Transform firePoint)
    {
        if (firePoint == null || !playerPositionKnown)
            return false;

        Vector3 targetPosition =
            lastKnownPlayerPosition +
            Vector3.up * targetHeight;

        Vector3 direction =
            targetPosition - firePoint.position;

        RaycastHit hit;

        if (Physics.Raycast(
            firePoint.position,
            direction.normalized,
            out hit,
            direction.magnitude,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (playerTarget != null &&
                (hit.collider.gameObject == playerTarget ||
                 hit.collider.transform.IsChildOf(
                     playerTarget.transform)))
            {
                return true;
            }
        }

        return false;
    }

    // FIND BEST FIRE POINT

    public void FindBestBarricade()
    {
        bestBarricade = null;
        selectedFirePoint = null;

        availableFirePoints.Clear();

        List<Transform> visibleFirePoints =
            new List<Transform>();

        List<Transform> blockedFirePoints =
            new List<Transform>();

        float bestScore = Mathf.Infinity;

        foreach (GameObject barricade in allBarricades)
        {
            if (barricade == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                barricade.transform.position
            );

            if (distance > maxBarricadeDistance)
                continue;

            for (int i = 1; i <= numberOfFirePoints; i++)
            {
                string pointName =
                    "FirePoint_" + i.ToString("00");

                Transform firePoint =
                    barricade.transform.Find(pointName);

                if (firePoint == null)
                    continue;

                if (triedFirePoints.Contains(firePoint))
                    continue;

                if (CanShootPlayerFromFirePoint(firePoint))
                {
                    visibleFirePoints.Add(firePoint);
                }
                else
                {
                    blockedFirePoints.Add(firePoint);
                }
            }
        }

        // Prefer positions with direct line of sight.

        if (visibleFirePoints.Count > 0)
        {
            availableFirePoints = visibleFirePoints;
        }
        else
        {
            // If none has line of sight, inspect other positions.

            availableFirePoints = blockedFirePoints;
        }

        if (availableFirePoints.Count == 0)
            return;

        // Randomize selection between suitable positions.

        if (visibleFirePoints.Count > 0)
        {
            selectedFirePoint =
                availableFirePoints[
                    Random.Range(0, availableFirePoints.Count)
                ];
        }
        else
        {
            // Choose the nearest untested position.

            foreach (Transform point in availableFirePoints)
            {
                float score = Vector3.Distance(
                    transform.position,
                    point.position
                );

                if (score < bestScore)
                {
                    bestScore = score;
                    selectedFirePoint = point;
                }
            }
        }

        if (selectedFirePoint != null)
        {
            bestBarricade =
                selectedFirePoint.parent.gameObject;
        }
    }

    // SEARCH AND MOVE TO SELECTED FIRE POINT

    public void SearchAndMoveToBestBarricade()
    {
        if (!playerPositionKnown || isMovingToBarricade)
            return;

        FindAllBarricades();
        FindBestBarricade();

        if (selectedFirePoint == null)
        {
            Debug.Log("No untested FirePoints remaining.");
            return;
        }

        if (navigation == null)
        {
            navigation =
                GetComponent<ArabEnemyRunsToFinalPosition>();
        }

        if (navigation == null)
        {
            Debug.LogError(
                "ArabEnemyRunsToFinalPosition not found!"
            );
            return;
        }

        movementTarget = selectedFirePoint.gameObject;

        triedFirePoints.Add(selectedFirePoint);

        navigation.target_object = movementTarget;
        navigation.enabled = true;

        isMovingToBarricade = true;

        Debug.Log(
            "Enemy moving to: " + selectedFirePoint.name
        );
    }

    // CHECK ARRIVAL AT FIRE POINT

    public void CheckBarricadeArrival()
    {
        if (movementTarget == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            movementTarget.transform.position
        );

        if (distance <= arrivalDistance)
        {
            isMovingToBarricade = false;

            // Check visibility from the selected firing position.

            if (CanShootPlayerFromFirePoint(selectedFirePoint))
            {
                isSearchingForAttackPosition = false;

                Debug.Log(
                    "Player visible! Enemy can start attacking."
                );

                return;
            }

            Debug.Log(
                "Wall blocks the shot. Trying another FirePoint."
            );

            // Keep searching for another position.

            nextSearchTime = Time.time + searchInterval;
        }
    }
}
