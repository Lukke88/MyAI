using UnityEngine;
using TMPro;  // ← nutné pro TextMeshProUGUI

public class ConstantScreenSizeNameTag : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;           
    [SerializeField] private float referenceDistance = 10f; 
    [SerializeField] private bool billboard = true;         
    [SerializeField] private bool updateNameOnStart = true; // změna jména jen jednou při startu?

    private Canvas canvas;
    private TextMeshProUGUI nameText;          // ← reference na TMP text
    private Vector3 initialScale;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>();

        // Najdeme TextMeshProUGUI – buď přímo na tomto objektu, nebo v dětech
        nameText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (nameText == null)
        {
            Debug.LogWarning("ConstantScreenSizeNameTag: TextMeshProUGUI nenalezen v dětech Canvasu!", this);
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        initialScale = transform.localScale;

        // Nastavení jména podle rodiče
        if (updateNameOnStart && nameText != null && transform.parent != null)
        {
            nameText.text = transform.parent.name;
            // případně můžeš ještě upravit: 
            // nameText.text = transform.parent.name.Replace("(Clone)", "").Trim();
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 1. Billboard (otočení ke kameře)
        if (billboard)
        {
            // Varianta 1 – přesná orientace ke kameře
           // transform.rotation = targetCamera.transform.rotation;

            // nebo Varianta 2 – jen směr pohledu (častější u jmen nad hlavou)
             transform.LookAt(transform.position + targetCamera.transform.forward);
        }

        // 2. Konstantní velikost na obrazovce
        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
        float scaleFactor = distance / referenceDistance;
        
        transform.localScale = initialScale * scaleFactor;
    }

    // Volitelné: pokud by se jméno rodiče měnilo i za běhu hry
    public void UpdateName()
    {
        if (nameText != null && transform.parent != null)
        {
            nameText.text = transform.parent.name;
        }
    }
}