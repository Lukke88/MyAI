using System.Collections.Generic;

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
//Low-poly tactical urban warfare with predictive enemy AI.
//„Shit! Enemy IFV! Take cover!“ - pri prevzeti mardera
public class ArabWarriorTacticalBehaviour : MonoBehaviour
{
    [Header("References")]
    public GameObject enemy;
    public GameObject nearestPlayer;
    public GameObject currentThreat;
    public string threatName = "Player";
    public GameObject[] allBarricades;

    [Header("Threat / Alertness")]
    [Range(0f, 100f)] public float alertness = 0f;
    public float vehicleAlertStartDistance = 500f;
    public float vehicleAlertMaximumDistance = 200f;
    public float playerVehicleIntentDistance = 1500f;
    [Range(0f, 100f)] public float intentAlertness = 60f;
    public float alertnessRiseSpeed = 45f;
    public float alertnessFallSpeed = 12f;
    public float coverAlertnessThreshold = 50f;
    public float vehicleTargetSwitchDistance = 500f;
    public float vehicleSearchInterval = 1f;
    private GameObject nearestDetectedVehicle;
    private float nextVehicleSearchTime;

    [Header("Movement & Combat Settings")]
    public float max_speed = 8.0f;
    public float min_speed = 3.5f;
    public float shooting_distance = 30.0f;
    public float fieldOfViewAngle = 120.0f;


    [Header("Prediction Settings")]
    public float turnAngleThreshold = 30.0f;
    public float predictionHorizon = 10.0f;
    public float barricadeProximityRadius = 30.0f;
    public float coverStopDistance = 2.0f;


    [Header("Trajectory Visualization")]
    public bool drawTrajectoryGizmos = true;
    public float playerSampleInterval = 0.15f;
    public int maxTrajectorySamples = 40;
    public int splineSubdivisions = 8;
    public float predictedPathStep = 0.5f;
    public float obstacleCheckDistance = 8.0f;
    public float obstacleSideOffset = 1.5f;
    public float obstacleProbeHeight = 0.8f;
    public float wallClearanceProbeDistance = 5.0f;
    public int clearanceRayCount = 12;

    [Header("Player Wall Clearance Learning")]
    public float averageWallClearance = 1.25f;
    public float minWallClearance = 0.35f;
    public float maxWallClearance = 3.0f;
    public int maxClearanceSamples = 30;

    public enum EnemyState { Intercept, Ambush, RetreatToCover, Shooting }
    public EnemyState currentState;
    public bool PlayerDetected;


    // Movement history and estimates
    private Vector3 lastPlayerPosition;
    private Vector3 lastMoveDirection;
    private float lastRecordTime;
    private float nextSampleTime;
    private bool hasPreviousRecord;

    private readonly Queue<float> recentSpeeds = new Queue<float>();
    private const int maxSpeedSamples = 20;
    private readonly Queue<float> clearanceSamples = new Queue<float>();
    private readonly List<Vector3> playerTrajectorySamples = new List<Vector3>();

    public List<Vector3> trajectoryTurnPoints = new List<Vector3>();
    public List<Vector3> predictedFuturePoints = new List<Vector3>();
    public List<GameObject> probablePlayerBarricades = new List<GameObject>();
    public List<GameObject> probableWalls = new List<GameObject>();

    private readonly List<Vector3> yellowSplinePoints = new List<Vector3>();
    private readonly List<Vector3> purplePredictionPoints = new List<Vector3>();

    public float delta_average_speed;
    public float time_to_reach_destination_player;
    public Vector3 possible_target_destination;
    private GameObject currentCoverBarricade;

    // Peek behavior
    private float peekTimer;
    private float nextPeekInterval = 2.0f;
    private bool isPeekingOut;


    private void Start()
    {
        enemy = gameObject;
        nearestPlayer = GameObject.Find("HeroSoldier");
        RefreshBarricades();
    }

    private void Update()
    {
        if (enemy == null || nearestPlayer == null) return;

        RaycastToPlayer(1500.0f);
        float currentAlertness = CalculateAlertness();
        if (currentAlertness >= coverAlertnessThreshold && currentState != EnemyState.Shooting)
        {
            currentCoverBarricade = null;
            currentState = EnemyState.RetreatToCover;
        }
        if (PlayerDetected)
        {
            BuildYellowTrajectory();
            MeasurePlayerWallClearance();
            PredictPlayerMovementAroundObstacles();
            PredictPlayerPathAndBarricades();
            EvaluateTacticalState();
        }

        ExecuteCurrentState();
    }

    // Returns a 0-100 value suitable for a UI Slider (Min=0, Max=100).
    // Combines approaching vehicle proximity with the player's apparent intent to reach a vehicle.
    public float CalculateAlertness()
    {
        if (nearestPlayer == null) return alertness;

        if (Time.time >= nextVehicleSearchTime)
        {
            nextVehicleSearchTime = Time.time + Mathf.Max(0.2f, vehicleSearchInterval);
            GameObject[] vehicles = GameObject.FindGameObjectsWithTag("PlayerVehicle");
            nearestDetectedVehicle = null;
            float nearestDistance = Mathf.Infinity;
            foreach (GameObject vehicle in vehicles)
            {
                if (vehicle == null) continue;
                float distance = Vector3.Distance(enemy.transform.position, vehicle.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestDetectedVehicle = vehicle;
                }
            }
        }

        float vehicleScore = 0f;
        bool vehicleIsCloseThreat = false;
        if (nearestDetectedVehicle != null)
        {
            float vehicleDistance = Vector3.Distance(enemy.transform.position, nearestDetectedVehicle.transform.position);
            vehicleScore = Mathf.InverseLerp(vehicleAlertStartDistance, vehicleAlertMaximumDistance, vehicleDistance) * 100f;
            vehicleScore = Mathf.Clamp(vehicleScore, 0f, 100f);
            vehicleIsCloseThreat = vehicleDistance <= vehicleTargetSwitchDistance;
        }

        float intentScore = 0f;
        if (nearestDetectedVehicle != null)
        {
            float playerToVehicle = Vector3.Distance(nearestPlayer.transform.position, nearestDetectedVehicle.transform.position);
            Vector3 towardVehicle = nearestDetectedVehicle.transform.position - nearestPlayer.transform.position;
            towardVehicle.y = 0f;
            Vector3 playerDirection = lastMoveDirection;
            playerDirection.y = 0f;
            if (playerToVehicle <= playerVehicleIntentDistance && playerDirection.sqrMagnitude > 0.01f && towardVehicle.sqrMagnitude > 0.01f)
            {
                float alignment = Vector3.Dot(playerDirection.normalized, towardVehicle.normalized);
                if (alignment > 0.35f)
                {
                    float proximityFactor = 1f - Mathf.Clamp01(playerToVehicle / playerVehicleIntentDistance);
                    float directionFactor = Mathf.InverseLerp(0.35f, 1f, alignment);
                    intentScore = intentAlertness * proximityFactor * directionFactor;
                }
            }
        }

        float targetAlertness = Mathf.Max(vehicleScore, intentScore);
        float changeSpeed = targetAlertness > alertness ? alertnessRiseSpeed : alertnessFallSpeed;
        alertness = Mathf.MoveTowards(alertness, targetAlertness, changeSpeed * Time.deltaTime);

        // Preserve the pedestrian reference; switch only the tactical target.
        if (vehicleIsCloseThreat)
        {
            currentThreat = nearestDetectedVehicle;
            threatName = nearestDetectedVehicle.name;
        }
        else
        {
            currentThreat = nearestPlayer;
            threatName = nearestPlayer.name;
        }

        return Mathf.Clamp(alertness, 0f, 100f);
    }

    private void RefreshBarricades()
    {
        allBarricades = GameObject.FindGameObjectsWithTag("Barricade");
    }

    // Player detection and history sampling
    public void RaycastToPlayer(float raycastDistance)
    {
        Vector3 toPlayer = nearestPlayer.transform.position - enemy.transform.position;
        Vector3 directionToPlayer = toPlayer.normalized;
        float angle = Vector3.Angle(enemy.transform.forward, directionToPlayer);
        if (angle > fieldOfViewAngle * 0.5f)
        {
            PlayerDetected = false;
            return;
        }

        Ray ray = new Ray(enemy.transform.position + Vector3.up * 1.5f, directionToPlayer);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.gameObject == nearestPlayer)
            {
                PlayerDetected = true;
                Vector3 currentPos = nearestPlayer.transform.position;
                float currentTime = Time.time;

                if (!hasPreviousRecord)
                {
                    lastPlayerPosition = currentPos;
                    lastRecordTime = currentTime;
                    lastMoveDirection = Vector3.zero;
                    hasPreviousRecord = true;
                    AddTrajectorySample(currentPos);
                }
                else
                {
                    float elapsed = currentTime - lastRecordTime;
                    Vector3 displacement = currentPos - lastPlayerPosition;
                    if (elapsed > 0.05f && displacement.magnitude > 0.1f)
                    {
                        float speed = displacement.magnitude / elapsed;
                        recentSpeeds.Enqueue(speed);
                        while (recentSpeeds.Count > maxSpeedSamples) recentSpeeds.Dequeue();

                        Vector3 currentDirection = displacement.normalized;
                        if (lastMoveDirection != Vector3.zero && Vector3.Angle(lastMoveDirection, currentDirection) >= turnAngleThreshold)
                        {
                            trajectoryTurnPoints.Add(currentPos);
                            if (trajectoryTurnPoints.Count > 10) trajectoryTurnPoints.RemoveAt(0);
                        }

                        lastMoveDirection = currentDirection;
                        lastPlayerPosition = currentPos;
                        lastRecordTime = currentTime;
                        if (Time.time >= nextSampleTime)
                        {
                            AddTrajectorySample(currentPos);
                            nextSampleTime = Time.time + playerSampleInterval;
                        }
                    }
                }
            }
            else PlayerDetected = false;
        }
        else PlayerDetected = false;


        float speedSum = 0f;
        foreach (float speed in recentSpeeds) speedSum += speed;
        delta_average_speed = recentSpeeds.Count > 0 ? speedSum / recentSpeeds.Count : 0f;
    }


    private void AddTrajectorySample(Vector3 position)
    {
        playerTrajectorySamples.Add(position);
        while (playerTrajectorySamples.Count > maxTrajectorySamples) playerTrajectorySamples.RemoveAt(0);
    }


    // Catmull-Rom spline interpolation. It smooths recorded points; it is not itself a future predictor.
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2f * p1) + (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private void BuildYellowTrajectory()
    {
        yellowSplinePoints.Clear();
        if (playerTrajectorySamples.Count < 2) return;

        for (int i = 0; i < playerTrajectorySamples.Count - 1; i++)
        {
            Vector3 p0 = playerTrajectorySamples[Mathf.Max(i - 1, 0)];
            Vector3 p1 = playerTrajectorySamples[i];
            Vector3 p2 = playerTrajectorySamples[i + 1];
            Vector3 p3 = playerTrajectorySamples[Mathf.Min(i + 2, playerTrajectorySamples.Count - 1)];

            for (int step = 0; step < splineSubdivisions; step++)
            {
                float t = step / (float)splineSubdivisions;
                yellowSplinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }
        yellowSplinePoints.Add(playerTrajectorySamples[playerTrajectorySamples.Count - 1]);
    }


    // Sample nearby walls/barriers in several directions; learn the player's typical clearance.
    private void MeasurePlayerWallClearance()
    {
        Vector3 origin = nearestPlayer.transform.position + Vector3.up * obstacleProbeHeight;
        float closest = wallClearanceProbeDistance;
        bool found = false;
        int rays = Mathf.Max(4, clearanceRayCount);

        for (int i = 0; i < rays; i++)
        {
            float angle = i * (360f / rays);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Ray ray = new Ray(origin, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, wallClearanceProbeDistance);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == nearestPlayer) continue;
                if (!IsWallOrBarrier(hit.collider)) continue;
                if (hit.distance < closest)
                {
                    closest = hit.distance;
                    found = true;
                }
            }
        }

        if (found)
        {
            clearanceSamples.Enqueue(closest);
            while (clearanceSamples.Count > maxClearanceSamples) clearanceSamples.Dequeue();
            float sum = 0f;
            foreach (float sample in clearanceSamples) sum += sample;
            averageWallClearance = Mathf.Clamp(sum / clearanceSamples.Count, minWallClearance, maxWallClearance);
        }
    }

    private bool IsWallOrBarrier(Collider collider)
    {
        return collider.CompareTag("Wall") || collider.CompareTag("Barrier");
    }

    private bool IsWallOrBarrier(RaycastHit hit)
    {
        return hit.collider != null && IsWallOrBarrier(hit.collider);
    }


    // Purple path: straight extrapolation, then a side waypoint when an obstacle blocks the path.
    private void PredictPlayerMovementAroundObstacles()
    {
        purplePredictionPoints.Clear();
        predictedFuturePoints.Clear();
        probablePlayerBarricades.Clear();
        probableWalls.Clear();

        Vector3 start = nearestPlayer.transform.position;
        Vector3 direction = lastMoveDirection;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f || delta_average_speed < 0.2f) return;
        direction.Normalize();

        float horizon = Mathf.Max(1f, predictionHorizon);
        float travelDistance = delta_average_speed * horizon;
        Vector3 directEnd = start + direction * travelDistance;
        Vector3 probeOrigin = start + Vector3.up * obstacleProbeHeight;

        if (Physics.Raycast(probeOrigin, direction, out RaycastHit obstacleHit, Mathf.Min(obstacleCheckDistance, travelDistance)) && IsWallOrBarrier(obstacleHit))
        {
            Vector3 obstacleCenter = obstacleHit.point;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            float clearance = Mathf.Clamp(averageWallClearance, minWallClearance, maxWallClearance);
            Vector3 leftCandidate = obstacleCenter + side * (obstacleSideOffset + clearance);
            Vector3 rightCandidate = obstacleCenter - side * (obstacleSideOffset + clearance);
            leftCandidate.y = start.y;
            rightCandidate.y = start.y;

            bool leftBlocked = IsCandidateBlocked(start, leftCandidate);
            bool rightBlocked = IsCandidateBlocked(start, rightCandidate);
            Vector3 waypoint;
            if (!leftBlocked && rightBlocked) waypoint = leftCandidate;
            else if (leftBlocked && !rightBlocked) waypoint = rightCandidate;
            else if (!leftBlocked && !rightBlocked)
            {
                // Tie-break using the player's last observed turning direction.
                waypoint = lastMoveDirection == Vector3.zero || Vector3.Dot(lastMoveDirection, side) >= 0f ? leftCandidate : rightCandidate;
            }
            else
            {
                // Both sides appear blocked: retain a cautious point just before the obstacle.
                waypoint = obstacleHit.point - direction * 0.75f;
                waypoint.y = start.y;
            }

            Vector3 beyondObstacle = waypoint + direction * Mathf.Max(2f, obstacleSideOffset * 2f);
            beyondObstacle.y = start.y;
            purplePredictionPoints.Add(start);
            purplePredictionPoints.Add(waypoint);
            purplePredictionPoints.Add(beyondObstacle);
            purplePredictionPoints.Add(directEnd);
            predictedFuturePoints.Add(waypoint);
            predictedFuturePoints.Add(beyondObstacle);
            possible_target_destination = beyondObstacle;

            if (obstacleHit.collider != null)
            {
                GameObject obstacleObject = obstacleHit.collider.gameObject;
                if (obstacleObject.CompareTag("Barrier"))
                {
                    if (!probablePlayerBarricades.Contains(obstacleObject)) probablePlayerBarricades.Add(obstacleObject);
                }
                else if (obstacleObject.CompareTag("Wall"))
                {
                    if (!probableWalls.Contains(obstacleObject)) probableWalls.Add(obstacleObject);
                }
            }
        }
        else
        {
            purplePredictionPoints.Add(start);
            purplePredictionPoints.Add(directEnd);
            predictedFuturePoints.Add(directEnd);
            possible_target_destination = directEnd;
        }

        // Add a curved display using Catmull-Rom over the prediction control points.
        if (purplePredictionPoints.Count >= 3)
        {
            List<Vector3> controls = new List<Vector3>(purplePredictionPoints);
            if (controls.Count == 3) controls.Insert(0, controls[0]);
            List<Vector3> smooth = new List<Vector3>();
            for (int i = 0; i < controls.Count - 1; i++)
            {
                Vector3 p0 = controls[Mathf.Max(0, i - 1)];
                Vector3 p1 = controls[i];
                Vector3 p2 = controls[i + 1];
                Vector3 p3 = controls[Mathf.Min(i + 2, controls.Count - 1)];
                for (int s = 0; s < splineSubdivisions; s++)
                    smooth.Add(CatmullRom(p0, p1, p2, p3, s / (float)splineSubdivisions));
            }
            smooth.Add(controls[controls.Count - 1]);
            purplePredictionPoints.Clear();
            purplePredictionPoints.AddRange(smooth);
        }
    }


    private bool IsCandidateBlocked(Vector3 from, Vector3 to)
    {
        Vector3 start = from + Vector3.up * obstacleProbeHeight;
        Vector3 delta = to - from;
        delta.y = 0f;
        float distance = delta.magnitude;
        if (distance <= 0.1f) return false;
        RaycastHit[] hits = Physics.RaycastAll(start, delta.normalized, distance);
        foreach (RaycastHit hit in hits)
        {
            if (IsWallOrBarrier(hit)) return true;
        }
        return false;
    }


    // Original barricade proximity scan retained for tactical destination context.
    public void PredictPlayerPathAndBarricades()
    {
        if (allBarricades == null || allBarricades.Length == 0) RefreshBarricades();
        if (delta_average_speed < 0.2f || lastMoveDirection == Vector3.zero) return;

        foreach (GameObject barricade in allBarricades)
        {
            if (barricade == null) continue;
            foreach (Vector3 point in predictedFuturePoints)
            {
                if (Vector3.Distance(point, barricade.transform.position) <= barricadeProximityRadius)
                {
                    if (!probablePlayerBarricades.Contains(barricade))
                        probablePlayerBarricades.Add(barricade);
                    break;
                }
            }
        }
    }


    public void EvaluateTacticalState()
    {
        Vector3 toEnemyDir = (enemy.transform.position - nearestPlayer.transform.position).normalized;
        float approachAngle = Vector3.Angle(lastMoveDirection, toEnemyDir);
        bool playerChargingDirectly = approachAngle < 35.0f && delta_average_speed > 1.0f;

        if (playerChargingDirectly)
        {
            if (currentState != EnemyState.Shooting) currentState = EnemyState.RetreatToCover;
            return;
        }

        float distEnemyToDest = Vector3.Distance(enemy.transform.position, possible_target_destination);
        float distPlayerToDest = Vector3.Distance(nearestPlayer.transform.position, possible_target_destination);
        if (delta_average_speed > 0.1f)
        {
            time_to_reach_destination_player = distPlayerToDest / delta_average_speed;
            if (time_to_reach_destination_player > 0.1f)
            {
                float necessarySpeed = distEnemyToDest / time_to_reach_destination_player;
                currentState = necessarySpeed > max_speed ? EnemyState.Ambush : EnemyState.Intercept;
            }
        }
    }


    public void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Intercept: InterceptPlayerPosition(); break;
            case EnemyState.RetreatToCover: RetreatBehindNearestBarricade(); break;
            case EnemyState.Ambush: SearchNearestHideout(); break;
            case EnemyState.Shooting: StartShooting(); break;
        }
    }


    public void InterceptPlayerPosition()
    {
        MoveEnemyTo(possible_target_destination, max_speed);
        if (Vector3.Distance(enemy.transform.position, nearestPlayer.transform.position) <= shooting_distance)
            currentState = EnemyState.Shooting;
    }


    public void RetreatBehindNearestBarricade()
    {
        if (currentCoverBarricade == null) currentCoverBarricade = FindBestCoverBarricade();
        if (currentCoverBarricade == null)
        {
            currentState = EnemyState.Shooting;
            return;
        }

        Vector3 awayFromPlayer = currentCoverBarricade.transform.position - nearestPlayer.transform.position;
        awayFromPlayer.y = 0f;
        if (awayFromPlayer.sqrMagnitude < 0.001f) awayFromPlayer = enemy.transform.forward;
        awayFromPlayer.Normalize();

        Vector3 retreatPosition = currentCoverBarricade.transform.position + awayFromPlayer * coverStopDistance;
        retreatPosition.y = enemy.transform.position.y;
        float distance = Vector3.Distance(enemy.transform.position, retreatPosition);
        if (distance > 0.5f)
        {
            MoveEnemyTo(retreatPosition, max_speed);
        }
        else
        {
            currentState = EnemyState.Shooting;
        }
    }


    public void SearchNearestHideout()
    {
        if (currentCoverBarricade == null) currentCoverBarricade = FindBestCoverBarricade();
        if (currentCoverBarricade != null)
        {
            Vector3 awayFromPlayer = currentCoverBarricade.transform.position - nearestPlayer.transform.position;
            awayFromPlayer.y = 0f;
            awayFromPlayer.Normalize();
            Vector3 hideSpot = currentCoverBarricade.transform.position + awayFromPlayer * coverStopDistance;
            hideSpot.y = enemy.transform.position.y;
            MoveEnemyTo(hideSpot, min_speed);
        }

        if (Vector3.Distance(enemy.transform.position, nearestPlayer.transform.position) < shooting_distance)
            currentState = EnemyState.Shooting;
    }


    private GameObject FindBestCoverBarricade()
    {
        GameObject best = null;
        float shortest = Mathf.Infinity;
        if (allBarricades == null) RefreshBarricades();
        foreach (GameObject barricade in allBarricades)
        {
            if (barricade == null) continue;
            float enemyDistance = Vector3.Distance(enemy.transform.position, barricade.transform.position);
            float playerDistance = Vector3.Distance(nearestPlayer.transform.position, barricade.transform.position);
            if (enemyDistance < playerDistance && enemyDistance < shortest)
            {
                shortest = enemyDistance;
                best = barricade;
            }
        }
        return best;
    }


    public void StartShooting()
    {
        if (currentCoverBarricade != null)
        {
            peekTimer += Time.deltaTime;
            if (peekTimer >= nextPeekInterval)
            {
                peekTimer = 0f;
                isPeekingOut = !isPeekingOut;
                nextPeekInterval = Random.Range(1.2f, 3.0f);
            }

            Vector3 awayFromPlayer = currentCoverBarricade.transform.position - nearestPlayer.transform.position;
            awayFromPlayer.y = 0f;
            awayFromPlayer.Normalize();
            Vector3 baseCoverSpot = currentCoverBarricade.transform.position + awayFromPlayer * coverStopDistance;
            Vector3 sideOffset = Vector3.Cross(Vector3.up, awayFromPlayer) * 1.5f;
            Vector3 targetCombatPos = isPeekingOut ? baseCoverSpot + sideOffset : baseCoverSpot;
            targetCombatPos.y = enemy.transform.position.y;
            MoveEnemyTo(targetCombatPos, min_speed);
        }

        GameObject aimTarget = currentThreat != null ? currentThreat : nearestPlayer;
        Vector3 lookDir = aimTarget.transform.position - enemy.transform.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 8f);

        if (isPeekingOut || currentCoverBarricade == null)
        {
            // Connect your existing enemy weapon/fire method here.
        }
    }


    // Movement now uses direct MoveTowards, not NavMeshAgent.
    private void MoveEnemyTo(Vector3 destination, float speed)
    {
        destination.y = enemy.transform.position.y;
        Vector3 direction = destination - enemy.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.01f) return;

        enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, destination, speed * Time.deltaTime);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, Quaternion.LookRotation(direction.normalized), Time.deltaTime * 10f);
    }


    private void OnDrawGizmos()
    {
        if (!drawTrajectoryGizmos) return;

        // Yellow: recorded player path smoothed with Catmull-Rom.
        Gizmos.color = Color.yellow;
        for (int i = 0; i < yellowSplinePoints.Count - 1; i++)
            Gizmos.DrawLine(yellowSplinePoints[i], yellowSplinePoints[i + 1]);
        foreach (Vector3 point in trajectoryTurnPoints) Gizmos.DrawSphere(point, 0.25f);

        // Purple: predicted path, including selected obstacle bypass waypoint.
        Gizmos.color = new Color(0.7f, 0.2f, 1f);
        for (int i = 0; i < purplePredictionPoints.Count - 1; i++)
            Gizmos.DrawLine(purplePredictionPoints[i], purplePredictionPoints[i + 1]);
        foreach (Vector3 point in predictedFuturePoints) Gizmos.DrawSphere(point, 0.25f);

        // Red: obstacles affecting the current prediction.
        Gizmos.color = Color.red;
        foreach (GameObject obstacle in probablePlayerBarricades)
            if (obstacle != null) Gizmos.DrawWireSphere(obstacle.transform.position, 1.0f);
        Gizmos.color = Color.magenta;
        foreach (GameObject wall in probableWalls)
            if (wall != null) Gizmos.DrawWireSphere(wall.transform.position, 1.0f);
    }
}



/*
1. Detekce a sledování hráče (Implementováno)
- RaycastToPlayer()
- Kontrola zorného pole 120°.
- Raycast na vzdálenost 150 jednotek.
- Záznam pozice a času pohybu.
  2. Analýza pohybu (Implementováno)
- recentSpeeds uchovává posledních 20 vzorků rychlosti.
- delta_average_speed počítá průměr.
- trajectoryTurnPoints ukládá změny směru při zatočení alespoň o 30°.
  3. Predikce budoucího pohybu (Implementováno)
- PredictPlayerPathAndBarricades()
- Predikce až 10 sekund dopředu.
- Vytváření budoucích bodů po jedné sekundě.
- Vyhledávání barikád do vzdálenosti 30 jednotek od predikované trasy.
  4. Taktické rozhodování (Implementováno**)
- EvaluateTacticalState()
- Rozpoznání přímého útoku hráče.
- Výpočet času potřebného k dosažení cíle.
- Výpočet rychlosti potřebné k předběhnutí hráče.
  5. Krytí a přepadové pozice (Částečně**)
- FindBestCoverBarricade()
- SearchNearestHideout()
- RetreatBehindNearestBarricade()
- StartShooting()
- Střídání schování a vykouknutí (Peek-a-Boo).
  6. Vizualizace predikce (Implementováno)
- OnDrawGizmos()
- Žluté body změny směru.
- Azurová predikovaná trajektorie.
- Červeně označené pravděpodobné barikády.
*/



public class EnemyRadioMessage : MonoBehaviour
{
    public Image mainText;
    public Sprite normalImage;
    public Sprite enemyWarningImage;

    public float displayDuration = 3f;

    private bool messageActive;

    public void ShowEnemyWarning()
    {
        if (!messageActive)
            StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        messageActive = true;

        // Krátký glitch efekt
        for (int i = 0; i < 4; i++)
        {
            mainText.sprite = enemyWarningImage;
            yield return new WaitForSeconds(0.08f);

            mainText.sprite = normalImage;
            yield return new WaitForSeconds(0.06f);
        }

        mainText.sprite = enemyWarningImage;

        yield return new WaitForSeconds(displayDuration);

        mainText.sprite = normalImage;

        messageActive = false;
    }
}