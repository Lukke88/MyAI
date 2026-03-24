using UnityEngine;

public class SimpleHighlight : MonoBehaviour
{
    public Renderer[] renderers;
    public Color[] originalColors;

    void Update()
    {
        renderers = GetComponentsInChildren<Renderer>();

        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
    }

    public void SetHighlight(bool state)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (state)
                renderers[i].material.color = originalColors[i] * 0.6f + Color.green * 0.4f;
            else
                renderers[i].material.color = originalColors[i];
        }
    }
}
