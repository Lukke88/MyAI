using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class HoneyCombHex : MonoBehaviour
{
    public bool occupied;

    [HideInInspector]
    public List<Transform> freeStickPoints = new List<Transform>();

    void Awake()
    {
        for(int i=1;i<=6;i++)
        {
            Transform p = transform.Find("stick_point" + i);

            if(p!=null)
                freeStickPoints.Add(p);
        }
    }
}
