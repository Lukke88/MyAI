using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CursorMakerBehaviour : MonoBehaviour
{
    public GameObject marker, selectedEnemy, player;
    public GameObject[] enemies;
    public TMP_Text infoText;
	public float groundHeight = 0.5f;
    public float minimal_dist_of_enemy = 35.0f;
    public Vector3 hit_point;
	public GameObject gunOriginalGunPrefab;
    public float rotationSpeed = 8f;

    private Renderer enemyRenderer;
    private Material originalMaterial;
	
	public Transform playerFiringSlot;
	
	public float playerMoveSpeed = 6f;
public float safeDistance = 25f;

private float lastClickTime;
private float doubleClickTime = 0.25f;

private Vector3 moveTarget;
private bool playerMoving = false;

public GameObject gunPrefab;

private GameObject currentGun;
	
	public LineRenderer lineRenderer;
public float rayDistance = 2000f;
public Material dottedGreenMaterial;

    private Animator playerAnimator;

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if(player != null)
            playerAnimator = player.GetComponent<Animator>();
    }

 void Update()
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    selectedEnemy = null;

    Vector3 endPoint = player.transform.position + player.transform.forward * rayDistance;

    if(Physics.Raycast(ray, out hit, rayDistance))
    {
        endPoint = hit.point;   
    }
	
	ShowEnemyInfo();
    if(selectedEnemy != null)
    {
        Debug.Log("You are aiming at " + selectedEnemy.name);
        TurnSelectedPlayerToSelectedEnemy();
     //   PlayPlayerFireAnimation();
    }
   /* else
    {
        if(infoText != null)
            infoText.text = "";
    }*/
	
	HandleDoubleClickMove(endPoint);
	MovePlayer();

	if(marker!=null)
		marker.transform.position =new Vector3( endPoint.x, groundHeight,endPoint.z);
	if(endPoint!=Vector3.zero)
		CheckAllEnemies(hit.point);
	if(Input.GetKey(KeyCode.LeftControl))
    DrawRayVisual(player.transform.position, endPoint);
}
void HandleDoubleClickMove(Vector3 targetPoint)
{
    if(Input.GetMouseButtonDown(0))
    {
        if(Time.time - lastClickTime < doubleClickTime)
        {
            TryMovePlayer(targetPoint);
        }

        lastClickTime = Time.time;
    }
}
void TryMovePlayer(Vector3 targetPoint)
{
    if(player == null) return;

    // nesmí být blízko hráče
    if(Vector3.Distance(player.transform.position, targetPoint) < safeDistance)
        return;

    // nesmí být blízko enemy
    foreach(GameObject e in enemies)
    {
        if(Vector3.Distance(e.transform.position, targetPoint) < safeDistance)
            return;
    }

    // kontrola zdi nebo budovy
    Ray ray = new Ray(targetPoint + Vector3.up * 10f, Vector3.down);
    RaycastHit hit;

    if(Physics.Raycast(ray, out hit, 20f))
    {
        if(hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Building"))
            return;
    }

    moveTarget = targetPoint;
    playerMoving = true;
}

void MovePlayer()
{
    if(!playerMoving) return;

    player.transform.position = Vector3.MoveTowards(
        player.transform.position,
        moveTarget,
        playerMoveSpeed * Time.deltaTime
    );

    if(Vector3.Distance(player.transform.position, moveTarget) < 0.5f)
    {
        playerMoving = false;
    }
}
void DrawRayVisual(Vector3 startPos, Vector3 endPos)
{
    if(lineRenderer == null) return;

    lineRenderer.positionCount = 2;
    lineRenderer.SetPosition(0, startPos);
    lineRenderer.SetPosition(1, endPos);

    // Dotted effect (UV tiling)
    if(dottedGreenMaterial != null)
        lineRenderer.material = dottedGreenMaterial;
}
    public void CheckAllEnemies(Vector3 Hit_point)
    {
        GameObject closestEnemy = null;
        float closestDist = minimal_dist_of_enemy;

        foreach(GameObject go in enemies)
        {
            float d = Vector3.Distance(go.transform.position, Hit_point);

            if(d < closestDist)
            {
                closestDist = d;
                closestEnemy = go;
            }
        }

        if(closestEnemy != selectedEnemy)
        {
            RemoveHighlight();
            selectedEnemy = closestEnemy;
            ApplyHighlight();
        }
    }

    void TurnSelectedPlayerToSelectedEnemy()
{
    if(selectedEnemy == null) return;

    // Rotace hráče
    Vector3 direction = selectedEnemy.transform.position - player.transform.position;
    Quaternion lookRotation = Quaternion.LookRotation(direction);

    player.transform.rotation = Quaternion.Lerp(
        player.transform.rotation,
        lookRotation,
        Time.deltaTime * rotationSpeed
    );

    // --- Weapon spawn logic ---
    if(playerFiringSlot == null)
    {
        playerFiringSlot = player.transform.GetChild(2);
    }

    if(currentGun == null && gunPrefab != null)
    {
        currentGun = Instantiate(
            gunPrefab,
            playerFiringSlot.position,
            playerFiringSlot.rotation,
            playerFiringSlot
        );
    }
}

    void ShowEnemyInfo()
    {
        if(selectedEnemy != null && infoText != null)
        {
            infoText.text = "You are aiming at " + selectedEnemy.name;
        }
    }

    void PlayPlayerFireAnimation()
    {
        if(playerAnimator != null)
        {
            playerAnimator.SetInteger("IsMiaFiringRifle", 1);
        }
    }

    void ApplyHighlight()
    {
        if(selectedEnemy == null) return;

        enemyRenderer = selectedEnemy.GetComponent<Renderer>();

        if(enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;

            enemyRenderer.material.EnableKeyword("_EMISSION");
            enemyRenderer.material.SetColor("_EmissionColor", Color.yellow * 0.8f);
        }
    }

    void RemoveHighlight()
    {
        if(enemyRenderer != null && originalMaterial != null)
        {
            enemyRenderer.material = originalMaterial;
        }
    }
}