using UnityEngine;

public class BuildingUtility : MonoBehaviour
{
    void Start()
    {
        GameObject[] all = GameObject.FindObjectsOfType<GameObject>();

        foreach (var obj in all)
        {
            if (obj.name.Contains("Building") || obj.name.Contains("arab_house"))
            {
                if (!obj.GetComponent<Collider>())
                {
                    BoxCollider col = obj.AddComponent<BoxCollider>();
                    col.isTrigger = false;
                }

                obj.tag = "Building";
            }
        }
    }
}