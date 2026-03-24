using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectsCursorNearby : MonoBehaviour
{
    public float detectDistance = 3f;
    public float runSpeed = 6f;

    private Vector3 originalPosition;
    private Vector3 escapeTarget;

    private bool isEscaping = false;
    private bool returning = false;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        Vector3 cursorWorldPos = GetCursorWorldPosition();

        float dist = Vector3.Distance(transform.position, cursorWorldPos);

        // pokud je kurzor blízko enemy
        if(dist < detectDistance && !isEscaping && !returning)
        {
            StartEscape(cursorWorldPos);
        }

        if(isEscaping)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                escapeTarget,
                runSpeed * Time.deltaTime
            );

            if(Vector3.Distance(transform.position, escapeTarget) < 0.5f)
            {
                isEscaping = false;
                returning = true;
            }
        }

        if(returning)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                originalPosition,
                runSpeed * Time.deltaTime
            );

            if(Vector3.Distance(transform.position, originalPosition) < 0.5f)
            {
                returning = false;
            }
        }
    }

    Vector3 GetCursorWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1000))
        {
            return hit.point;
        }

        return transform.position;
    }

    void StartEscape(Vector3 cursorPos)
    {
        Vector3 dir = transform.position - cursorPos;
        dir.Normalize();

        float randomDistance = Random.Range(5f, 20f);

        escapeTarget = transform.position + dir * randomDistance;

        isEscaping = true;
    }
}