using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
            return;

        transform.LookAt(
            transform.position +
            cam.transform.rotation * Vector3.forward,
            cam.transform.rotation * Vector3.up
        );
    }
}