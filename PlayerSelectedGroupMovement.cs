using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSelectedGroupMovement : MonoBehaviour
{
    [Header("Players")]
    public GameObject Player_1;
    public GameObject Player_2;
    public GameObject Player_3;
    public GameObject Player_4;
    public GameObject Player_5;
    public GameObject Player_6;
    private List<GameObject> players = new List<GameObject>();
    private List<bool> playerActive = new List<bool>();

    [Header("Player Buttons")]
    public Button Btn_Player_1;
    public Button Btn_Player_2;
    public Button Btn_Player_3;
    public Button Btn_Player_4;
    public Button Btn_Player_5;
    public Button Btn_Player_6;

    [Header("Formations")]
    public Button Btn_Arrow;
    public Button Btn_Rectangle;
    public Button Btn_Pentagon;
    public Button Btn_Circle;
    public Button Btn_Line;

    private enum FormationType { Arrow, Rectangle, Pentagon, Circle, Line }
    private FormationType currentFormation = FormationType.Line;

  //  [Header("Movement")]
    public enum MoveMode { Walk, Crouch, Crawl }
    public MoveMode currentMoveMode = MoveMode.Walk;
    public float moveSpeed = 5f;

    [Header("Markers")]
    public GameObject MarkerPrefab;
    private List<GameObject> markers = new List<GameObject>();

    [Header("Camera")]
    public Camera mainCamera;


[Header("Selection Box")]
public RectTransform selectionBox;
private Vector2 startPos;
private Vector2 endPos;
private bool isSelecting = false;

void Start()
    {
        // Fill players list
        players.Add(Player_1);
        players.Add(Player_2);
        players.Add(Player_3);
        players.Add(Player_4);
        players.Add(Player_5);
        players.Add(Player_6);

        // All inactive at start
        for(int i=0; i<players.Count; i++)
            playerActive.Add(false);

        // Instantiate markers
        foreach(var p in players)
        {
            GameObject marker = Instantiate(MarkerPrefab);
            marker.SetActive(false);
            markers.Add(marker);
        }

        // Assign button listeners
        Btn_Player_1.onClick.AddListener(() => TogglePlayer(0));
        Btn_Player_2.onClick.AddListener(() => TogglePlayer(1));
        Btn_Player_3.onClick.AddListener(() => TogglePlayer(2));
        Btn_Player_4.onClick.AddListener(() => TogglePlayer(3));
        Btn_Player_5.onClick.AddListener(() => TogglePlayer(4));
        Btn_Player_6.onClick.AddListener(() => TogglePlayer(5));

        Btn_Arrow.onClick.AddListener(() => SetFormation(FormationType.Arrow));
        Btn_Rectangle.onClick.AddListener(() => SetFormation(FormationType.Rectangle));
        Btn_Pentagon.onClick.AddListener(() => SetFormation(FormationType.Pentagon));
        Btn_Circle.onClick.AddListener(() => SetFormation(FormationType.Circle));
        Btn_Line.onClick.AddListener(() => SetFormation(FormationType.Line));
    }
	
	void Update()
    {
        // Update marker positions
        for(int i=0; i<players.Count; i++)
        {
            if(playerActive[i])
            {
                Vector3 pos = players[i].transform.position;
                RaycastHit hit;
                if(Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out hit, 10f))
                    pos.y = hit.point.y + 0.1f;
                markers[i].transform.position = pos;
            }
        }

        HandleSelectionBox();
    HandleMovementInput();
   // UpdateMarkers();
    }


void HandleSelectionBox()
{
    if(Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl))
    {
        isSelecting = true;
        startPos = Input.mousePosition;
        selectionBox.gameObject.SetActive(true);
    }

    if(Input.GetMouseButtonUp(0) && isSelecting)
    {
        isSelecting = false;
        selectionBox.gameObject.SetActive(false);
        SelectPlayersInBox();
    }

    if(isSelecting)
    {
        endPos = Input.mousePosition;
        Vector2 boxStart = startPos;
        Vector2 boxSize = endPos - startPos;

        // Handle negative width/height
        if(boxSize.x < 0)
        {
            boxStart.x = endPos.x;
            boxSize.x = -boxSize.x;
        }
        if(boxSize.y < 0)
        {
            boxStart.y = endPos.y;
            boxSize.y = -boxSize.y;
        }

        selectionBox.anchoredPosition = boxStart;
        selectionBox.sizeDelta = boxSize;
    }
}

void SelectPlayersInBox()
{
    for(int i=0; i<players.Count; i++)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(players[i].transform.position);
        if(screenPos.z > 0) // před kamerou
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(selectionBox, screenPos, null))
            {
                playerActive[i] = true;
                markers[i].SetActive(true);
            }
            else
            {
                playerActive[i] = false;
                markers[i].SetActive(false);
            }
        }
    }
}

    

    void TogglePlayer(int index)
    {
        playerActive[index] = !playerActive[index];
        markers[index].SetActive(playerActive[index]);
    }

    void SetFormation(FormationType formation)
    {
        currentFormation = formation;
    }

    

    void HandleMovementInput()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 targetPos = GetMouseWorldPosition();

            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                // Ctrl + Click → rotate + shoot
                foreach(var i in players)
                {
                    if(i != null && playerActive[players.IndexOf(i)])
                        i.transform.LookAt(targetPos); // Rotate to target
                        // TODO: add shooting
                }
            }
            else
            {
                // Move formation
                MoveFormation(targetPos);
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit))
            return hit.point;
        return Vector3.zero;
    }

    void MoveFormation(Vector3 target)
    {
        List<GameObject> activePlayers = new List<GameObject>();
        for(int i=0; i<players.Count; i++)
            if(playerActive[i])
                activePlayers.Add(players[i]);

        Vector3[] offsets = GetFormationOffsets(activePlayers.Count, currentFormation);

        for(int i=0; i<activePlayers.Count; i++)
        {
            Vector3 dest = target + offsets[i];
            activePlayers[i].transform.position = Vector3.MoveTowards(activePlayers[i].transform.position, dest, moveSpeed * Time.deltaTime);
        }

        // Camera follows frontmost
        if(activePlayers.Count > 0)
            mainCamera.GetComponent<CameraFollowsHero>().target = activePlayers[0].transform;
    }

    Vector3[] GetFormationOffsets(int count, FormationType formation)
    {
        Vector3[] result = new Vector3[count];
        // TODO: Implement proper offsets for each formation type
        float spacing = 2f;

        switch(formation)
        {
            case FormationType.Line:
                for(int i=0; i<count; i++)
                    result[i] = new Vector3(i * spacing, 0, 0);
                break;
            case FormationType.Arrow:
                for(int i=0; i<count; i++)
                    result[i] = new Vector3((i/2) * spacing * ((i%2==0)? -1:1), 0, -i);
                break;
            // Other formations: Rectangle, Pentagon, Circle ...
        }

        return result;
    }
}
