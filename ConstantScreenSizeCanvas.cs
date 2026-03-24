using UnityEngine;

public class ConstantScreenSizeCanvas : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;           // přetáhni Main Camera
    [SerializeField] private float referenceDistance = 10f; // v jaké vzdálenosti má mít „původní“ velikost (nastav si podle potřeby)
    [SerializeField] private bool billboard = true;         // otočit se ke kameře?

    private Canvas canvas;
    private Vector3 initialScale;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        initialScale = transform.localScale;
    }

    void LateUpdate()   // LateUpdate → pohyb kamery už proběhl
    {
        if (targetCamera == null) return;

        // 1. Billboard (vždy otočený ke kameře) – velmi časté u jmen/HP barů
        if (billboard)
        {
            transform.rotation = targetCamera.transform.rotation;
            // případně lepší varianta: transform.LookAt(transform.position + targetCamera.transform.forward);
        }

        // 2. Konstantní velikost na obrazovce
        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
        
        // škálujeme proporcionálně ke vzdálenosti
        float scaleFactor = distance / referenceDistance;
        
        transform.localScale = initialScale * scaleFactor;
    }
}