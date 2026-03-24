using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateEnemyToLastFallenPosition : MonoBehaviour
{
    public Vector3 newPosition;
    public float speed = 5f;

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            newPosition,
            speed * Time.deltaTime
        );
    }
}
