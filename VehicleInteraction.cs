using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class VehicleInteraction : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;

    [Header("Raycast Settings")]
    public float rayDistance = 50f;
    public LineRenderer rayLine;
    public LayerMask vehicleLayer;

    [Header("UI Settings")]
    public Button EnterTheVehicle_button;
    public Button ExitTheVehicle_button;
    public TMP_Text InfoText;
	public TMP_Text InfoCoordinates;
    [Header("Camera")]
    public Camera mainCamera;
    public CameraFollowsHero cameraScript;

    private Transform nearestVehicle;
    private bool inVehicle = false;

    [Header("Mouse Hover Vehicle")]
    public float autoMoveSpeed = 5f;
    public float autoRotateSpeed = 180f;
    private Transform hoveredVehicle;
    private bool movingToVehicle = false;

    [Header("Vehicle Wheels")]
    public float wheelRotationSpeed = 360f; // stupně za sekundu

    [Header("Navigation Settings")]
public Vector3 DestinationPosition;
public bool movingToDestination = false;
public float destinationStopDistance = 1.0f;
public float avoidDistance = 5.0f;
public float avoidAngle = 30f; // jak moc se natočit při překážce
public LayerMask obstacleLayer; // pro building/wall

void Start()
    {
        EnterTheVehicle_button.gameObject.SetActive(false);
        ExitTheVehicle_button.gameObject.SetActive(false);
        InfoText.text = "";
    }
void Update()
{
    if (!inVehicle)
    {
        HandleMovement();
        CheckVehicleRaycast();
        HandleVehicleHover();
    }
    else if (inVehicle)
    {
        // Priorita: Destination > manuální Move
        if (movingToDestination)
            MoveTowardsDestination();
        else
            MoveTheVehicle();
    }
}

// Kliknutí myší na vozidlo -> nastavuje DestinationPosition
void HandleVehicleDestination()
{
    if (!inVehicle) return;

    if (Input.GetMouseButtonDown(0))
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            DestinationPosition = hit.point;
            movingToDestination = true;
        }
    }
}

void MoveTowardsDestination()
{
    if (nearestVehicle == null) return;

    Vector3 direction = DestinationPosition - nearestVehicle.position;
    direction.y = 0;

    // Raycast dopředu na překážku
    RaycastHit hit;
    if (Physics.Raycast(nearestVehicle.position + Vector3.up, nearestVehicle.forward, out hit, avoidDistance, obstacleLayer))
    {
        // Překážka před vozidlem, natočíme mírně doprava
        nearestVehicle.Rotate(Vector3.up, avoidAngle * Time.deltaTime);
    }
    else
    {
        // Otáčení k cíli
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            nearestVehicle.rotation = Quaternion.RotateTowards(nearestVehicle.rotation, targetRot, rotateSpeed * Time.deltaTime);

            // Pohyb vpřed
            nearestVehicle.position = Vector3.MoveTowards(nearestVehicle.position, DestinationPosition, moveSpeed * Time.deltaTime);
        }

        // Pokud jsme u cíle
        if (Vector3.Distance(nearestVehicle.position, DestinationPosition) <= destinationStopDistance)
            movingToDestination = false;
    }

    // Otáčení kol při pohybu
    List<Transform> wheels = new List<Transform>();
    GetWheelsRecursive(nearestVehicle, wheels);
    foreach (Transform wheel in wheels)
    {
        float rotationAmount = moveSpeed * wheelRotationSpeed * Time.deltaTime;
        wheel.Rotate(rotationAmount, 0f, 0f, Space.Self);
    }
}


    #region Jenny Movement & Raycast
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.forward * v * moveSpeed * Time.deltaTime;
        transform.position += move;
        transform.Rotate(Vector3.up, h * rotateSpeed * Time.deltaTime);
    }

    void CheckVehicleRaycast()
    {
        Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
        RaycastHit hit;

        bool hitVehicle = Physics.Raycast(ray, out hit, rayDistance, vehicleLayer);

        rayLine.SetPosition(0, transform.position + Vector3.up);

        if (hitVehicle)
        {
            rayLine.SetPosition(1, hit.point);
            rayLine.startColor = rayLine.endColor = Color.green;
            nearestVehicle = hit.transform;

            InfoText.text = nearestVehicle.name + ". Do you want to enter the vehicle?";

            float distance = Vector3.Distance(transform.position, nearestVehicle.position);
            EnterTheVehicle_button.gameObject.SetActive(distance <= 20f);
        }
        else
        {
            rayLine.SetPosition(1, transform.position + transform.forward * rayDistance);
            rayLine.startColor = rayLine.endColor = Color.red;
            EnterTheVehicle_button.gameObject.SetActive(false);
            InfoText.text = "";
            nearestVehicle = null;
        }
    }
    #endregion

    #region Hover & AutoMove
    void HandleVehicleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, vehicleLayer))
        {
            hoveredVehicle = hit.transform;
            InfoText.text = hoveredVehicle.name + ". Do you want to enter the vehicle?";
            EnterTheVehicle_button.gameObject.SetActive(true);

            if (Input.GetMouseButtonDown(0))
                movingToVehicle = true;
        }
        else
        {
            hoveredVehicle = null;
            if (!movingToVehicle)
                EnterTheVehicle_button.gameObject.SetActive(false);
            if (!movingToVehicle)
                InfoText.text = "";
        }

        if (movingToVehicle && hoveredVehicle != null)
        {
            Vector3 targetPos = hoveredVehicle.position - hoveredVehicle.forward * 2f;
            Vector3 direction = targetPos - transform.position;
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                transform.position += direction.normalized * autoMoveSpeed * Time.deltaTime;
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, autoRotateSpeed * Time.deltaTime);
            }
            else
            {
                movingToVehicle = false;
                EnterVehicleButton(); // po dorazeni automaticky nastoupí
            }
        }
    }
    #endregion

    #region Vehicle Enter/Exit
    public void EnterVehicleButton()
    {
        if (nearestVehicle == null) return;

        inVehicle = true;
        // Skryjeme jen renderery Jenny
        SetRenderersEnabled(false);

        cameraScript.target = nearestVehicle;

        EnterTheVehicle_button.gameObject.SetActive(false);
        ExitTheVehicle_button.gameObject.SetActive(true);
        InfoText.text = "";
    }

    public void ExitVehicleButton()
    {
        if (nearestVehicle == null) return;

        inVehicle = false;
        SetRenderersEnabled(true);

        Vector3 exitPosition = nearestVehicle.position + nearestVehicle.right * 3f;
        transform.position = exitPosition;

        cameraScript.target = transform;

        ExitTheVehicle_button.gameObject.SetActive(false);
    }

    private void SetRenderersEnabled(bool enabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = enabled;
        }
    }
    #endregion

    #region Vehicle Movement & Wheels
    public void MoveTheVehicle()
    {
        if (nearestVehicle == null) return;

        float v = Input.GetAxis("Vertical");   // W/S
        float h = Input.GetAxis("Horizontal"); // A/D

        // Pohyb dopředu/dozadu
        nearestVehicle.position += nearestVehicle.forward * v * moveSpeed * Time.deltaTime;

        // Natočení vozidla při zatáčení
        if (Mathf.Abs(h) > 0.01f)
        {
            float turn = h * rotateSpeed * Time.deltaTime * Mathf.Sign(v != 0 ? v : 1);
            nearestVehicle.Rotate(Vector3.up, turn);
        }

        // Otáčení kol
        List<Transform> wheels = new List<Transform>();
        GetWheelsRecursive(nearestVehicle, wheels);

        foreach (Transform wheel in wheels)
        {
            float rotationAmount = v * wheelRotationSpeed * Time.deltaTime;
            wheel.Rotate(rotationAmount, 0f, 0f, Space.Self);
        }
    }

    private void GetWheelsRecursive(Transform parent, List<Transform> wheels)
    {
        foreach (Transform child in parent)
        {
            if (child.name.ToLower().Contains("wheel"))
                wheels.Add(child);

            if (child.childCount > 0)
                GetWheelsRecursive(child, wheels);
        }
    }
    #endregion
}
