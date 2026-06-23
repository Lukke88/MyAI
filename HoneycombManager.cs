using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HoneycombManager : MonoBehaviour
{
    public GameObject hexPrefab;

    public int width = 20;
    public int height = 20;

    public float hexRadius = 20.0f;

    public bool IsHoneycombVisible;
	
	//public float xOffset = Mathf.Sqrt(3) * hexRadius;
	//public float zOffset = 1.5f * hexRadius;
	
	//float xOffset = 1.5f * hexRadius;
	//float zOffset = Mathf.Sqrt(3) * hexRadius;
	
	public float spacingMultiplier = 1.1f;
	int hexCounter = 1;
	
	public int hexCount = 100;

	List<HoneyCombHex> generatedHexes =
	new List<HoneyCombHex>();

	HashSet<Vector3> occupiedPositions =
	new HashSet<Vector3>();

    List<GameObject> hexes = new List<GameObject>();
	
//	Dictionary<GameObject,HoneyCombCell2> cells =
//new Dictionary<GameObject,HoneyCombCell2>();

    void Start()
    {
        GenerateGrid();
        SetVisible(IsHoneycombVisible);
    }

    void Update()
    {
		//xOffset = hexRadius * 1.5f * spacingMultiplier;
		//zOffset = Mathf.Sqrt(3) * hexRadius * spacingMultiplier;
        SetVisible(IsHoneycombVisible);
    }

    void GenerateGrid()
{
    generatedHexes.Clear();

    GameObject firstHex =
    Instantiate(
        hexPrefab,
        Vector3.zero,
        Quaternion.identity);

    firstHex.name = "hex_1";

    HoneyCombHex root =
    firstHex.GetComponent<HoneyCombHex>();

    generatedHexes.Add(root);

    occupiedPositions.Add(firstHex.transform.position);

    int currentId = 2;

    while(currentId <= hexCount)
    {
        HoneyCombHex sourceHex =
        generatedHexes[
        Random.Range(0,generatedHexes.Count)];

        if(sourceHex.freeStickPoints.Count==0)
            continue;

        int pointIndex =
        Random.Range(0,
        sourceHex.freeStickPoints.Count);

        Transform stickPoint =
        sourceHex.freeStickPoints[pointIndex];

        Vector3 spawnPos = stickPoint.position;

        bool alreadyOccupied = false;

        foreach(Vector3 pos in occupiedPositions)
        {
            if(Vector3.Distance(pos,spawnPos)<0.1f)
            {
                alreadyOccupied = true;
                break;
            }
        }

        sourceHex.freeStickPoints.RemoveAt(pointIndex);

        if(alreadyOccupied)
            continue;

        GameObject newHex =
        Instantiate(
            hexPrefab,
            spawnPos,
            Quaternion.identity);
	//	HoneyCombCell2 cell =
		//	new HoneyCombCell2();

			//cell.hexObject = newHex;

			//cells.Add(newHex,cell);
        newHex.name = "hex_" + currentId;

        occupiedPositions.Add(spawnPos);

        HoneyCombHex hex =
        newHex.GetComponent<HoneyCombHex>();

        generatedHexes.Add(hex);

        currentId++;
    }
}
	

    void SetVisible(bool visible)
    {
        foreach(GameObject h in hexes)
        {
            Renderer r = h.GetComponent<Renderer>();

            if(r!=null)
                r.enabled = visible;
        }
    }
	
	public GameObject stimpackPrefab;

	public int stimpackCount = 15;

	public Vector2 spawnAreaMin;
	public Vector2 spawnAreaMax;
	
	void SpawnStimpacks()
	{
    for(int i=0;i<stimpackCount;i++)
    {
        Vector3 pos = new Vector3(
            Random.Range(spawnAreaMin.x,spawnAreaMax.x),
            1,
            Random.Range(spawnAreaMin.y,spawnAreaMax.y)
        );

        Instantiate(stimpackPrefab,pos,Quaternion.identity);
    }
	}
}

