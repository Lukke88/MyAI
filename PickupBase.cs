using UnityEngine;

public class PickupBase : MonoBehaviour
{
    public Renderer rend;
    public Color originalColor;

    Renderer[] renderers;

void Awake()
{
    renderers = GetComponentsInChildren<Renderer>();
}




    public void Highlight(bool on)
    {
		 Debug.Log("Highlight called: " + on);
        if (on)
        {
            rend.material.color = Color.green;
        }
        else
        {
            rend.material.color = originalColor;
        }
		foreach (Renderer r in renderers)
    {
        foreach (Material mat in r.materials)
        {
            mat.color = on ? Color.green : Color.white;
        }
    }
    }
}
