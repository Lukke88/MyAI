using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    public Camera playerCamera;

    public float rotateSpeed = 5f;

    public void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray =
                playerCamera.ScreenPointToRay(
                    Input.mousePosition
                );

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 2000f))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    Vector3 targetPosition =
                        hit.collider.transform.position;

                    targetPosition.y =
                        transform.position.y;

                    Vector3 direction =
                        (targetPosition - transform.position).normalized;

                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    transform.rotation =
                        Quaternion.Slerp(
                            transform.rotation,
                            targetRotation,
                            rotateSpeed * Time.deltaTime
                        );
                }
            }
        }
    }
}