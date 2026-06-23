using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WallSlots : MonoBehaviour
{
    public Transform[] slots;
    public bool[] occupied;
	
	public void Update()
	{
		GameObject[] tiles = GameObject.FindGameObjectsWithTag("tiles");
		foreach(GameObject go in tiles)
		{
			if(go.name.Contains("enemy_tile"))
				Destroy(go);
		}
	}

    void Awake()
    {
        int count = 10;
        slots = new Transform[count];
        occupied = new bool[count];

        float width = 20f; // délka zdi
        float gap = width / count;

        for(int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("Slot_" + i);

            float x = -width/2 + i * gap;

            p.transform.position = transform.position + new Vector3(x, 0, 0);
            p.transform.parent = transform;

            slots[i] = p.transform;
            occupied[i] = false;
        }
    }

    public Transform GetFreeSlot(out int index)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(!occupied[i])
            {
                occupied[i] = true;
                index = i;
                return slots[i];
            }
        }

        index = -1;
        return null;
    }

    public void FreeSlot(int index)
    {
        if(index >= 0 && index < occupied.Length)
            occupied[index] = false;
    }
}