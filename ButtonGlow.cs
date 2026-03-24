using UnityEngine;
using TMPro;

public class ObjectGlow : MonoBehaviour
{
    private Renderer rend;
    private Material materialInstance;

    [Header("Glow Settings")]
    public Color emissionColor = Color.yellow;
    public float emissionStrength = 2f;

    [Header("UI")]
    public TextMeshProUGUI infoText;

    [Header("Player")]
    public GameObject player; // přetáhni Mia1 do inspectoru

    private float lastClickTime;
    private float doubleClickTime = 0.3f;

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (rend != null)
        {
            materialInstance = rend.material;
            materialInstance.EnableKeyword("_EMISSION");
        }

        if (player == null)
        {
            player = GameObject.Find("Mia1");
        }
    }

    void OnMouseEnter()
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor("_EmissionColor", emissionColor * emissionStrength);
        }

        if (infoText != null)
        {
            infoText.text = "Do you want to move to " + gameObject.name + "?";
            infoText.enabled = true;
        }
    }

    void OnMouseExit()
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor("_EmissionColor", Color.black);
        }

        if (infoText != null)
        {
            infoText.text = "";
            infoText.enabled = false;
        }
    }

    void OnMouseDown()
    {
        // Jednoduchý klik
        MovePlayer();

        // Detekce dvojkliku
        if (Time.time - lastClickTime < doubleClickTime)
        {
            MovePlayer();
        }

        lastClickTime = Time.time;
    }

    void MovePlayer()
    {
        if (player != null)
        {
            player.transform.position = transform.position;
        }
    }
}