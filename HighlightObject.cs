using UnityEngine;

public class HighlightObject : MonoBehaviour
{
	private Renderer lastRenderer;
	private Material originalMaterial;
	private Texture originalTexture;

    public float rayDistance = 5000f;

	private Color originalColor;
	private Color originalEmission;

	public Texture ghostTexture;

    private OutlineScript currentOutline;

    void Update()
    {
        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition);

        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, rayDistance))
        {
            OutlineScript outline =
                hit.collider.GetComponent<OutlineScript>();

            if(outline != currentOutline)
            {
                ClearCurrent();

                if(outline != null)
                {
                    outline.IsHighlighted = true;

                    SetColorByTag(
                        outline,
                        hit.collider.tag);

                    currentOutline = outline;
                }
            }
        }
        else
        {
            ClearCurrent();
        }
    }
	
	void ResetMaterial(Renderer rend)
{
    if (rend == null) return;

    Material mat = rend.material;

    if (mat.HasProperty("_MainTex") && originalTexture != null)
        mat.mainTexture = originalTexture;

    if (mat.HasProperty("_Color"))
        mat.color = Color.white;

    if (mat.HasProperty("_EmissionColor"))
        mat.SetColor("_EmissionColor", Color.black);

    mat.DisableKeyword("_EMISSION");
}

   void ClearCurrent()
{
    if(currentOutline != null)
    {
        currentOutline.IsHighlighted = false;

        if(lastRenderer != null)
        {
            ResetMaterial(lastRenderer);
            lastRenderer = null;
        }

        currentOutline = null;
    }
}

void ApplyHighlight(Renderer rend, Color tint)
{
    if (rend == null) return;

    if (lastRenderer != null && lastRenderer != rend)
    {
        ResetMaterial(lastRenderer);
    }

    lastRenderer = rend;

    Material mat = rend.material;

    if (originalMaterial != mat)
    {
        originalMaterial = mat;

        if (mat.HasProperty("_MainTex"))
            originalTexture = mat.mainTexture;
    }

    if (mat.HasProperty("_Color"))
        mat.color = Color.Lerp(mat.color, tint, 0.35f);

    if (ghostTexture != null && mat.HasProperty("_MainTex"))
    {
        mat.SetTexture("_MainTex", ghostTexture);
        mat.SetColor("_Color", tint);
    }

    if (mat.HasProperty("_EmissionColor"))
    {
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", tint * 0.25f);
    }
}
    void SetColorByTag(OutlineScript outline, string tagName)
{
    Renderer rend = outline.GetComponent<Renderer>();

    switch(tagName)
    {
        case "Enemy":
            ApplyHighlight(rend, Color.red);
            break;

        case "tiles":
            ApplyHighlight(rend, Color.green);
            break;

        case "Casket":
            ApplyHighlight(rend, new Color(1f, 0.5f, 0f));
            break;

        case "building":
            ApplyHighlight(rend, Color.green);
            break;

        default:
            ApplyHighlight(rend, Color.white);
            break;
    }
}
}