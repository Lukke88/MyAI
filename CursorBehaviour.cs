using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CursorBehaviour : MonoBehaviour
{
    public GameObject turret, enemy;

    public TMP_Text infoText;
    public TMP_Text enemy_position_TMP_text;
	public TMP_Text enemy_distance_TMP_text;
    public TMP_Text enemy_name_TMP_text;
    public TMP_Text angle_turret_TMP_text;
	public TMP_Text enemyName;

    public float enemyDetectionDistance = 10.0f;
	
	

    private Renderer cursorRenderer;
    private Color originalColor;

    private GameObject currentEnemy;


    void Start()
    {
        cursorRenderer = GetComponent<Renderer>();

        turret = GameObject.Find("SquareTurret");

        if (cursorRenderer != null)
        {
            originalColor = cursorRenderer.material.color;
        }
    }


    void Update()
    {
        FindEnemy();
		float distance = Vector3.Distance(turret.transform.position, enemy.transform.position);
		enemy_distance_TMP_text.text = "Distance: " + distance.ToString("F0") + " m";

        if (currentEnemy != null)
        {
            cursorRenderer.material.color = Color.red;
			enemyName.text = currentEnemy.name;
			GameObject actualVehicle = GameObject.Find("HumWeeDestroyed");
			if(actualVehicle!=null)
			{
				distance = Vector3.Distance(actualVehicle.transform.position, transform.position);
				enemy_distance_TMP_text.text = distance.ToString() + " m";
			}
			enemy_position_TMP_text.text = currentEnemy.transform.position.ToString();
            if (infoText != null)
            {
                infoText.text = "You are aiming to " + currentEnemy.name;
            }


            if (enemy_name_TMP_text != null)
            {
                enemy_name_TMP_text.text = currentEnemy.name;
            }


            if (enemy_position_TMP_text != null)
            {
                Vector3 enemyPosition = currentEnemy.transform.position;

                enemy_position_TMP_text.text =
                    "X: " + enemyPosition.x.ToString("F1") +
                    "   Y: " + enemyPosition.y.ToString("F1") +
                    "   Z: " + enemyPosition.z.ToString("F1");
            }


            RotateTurretToEnemy();
            DisplayTurretAngle();
        }
        else
        {
            cursorRenderer.material.color = originalColor;


            if (infoText != null)
            {
                infoText.text = "";
            }


            if (enemy_name_TMP_text != null)
            {
                enemy_name_TMP_text.text = "";
            }


            if (enemy_position_TMP_text != null)
            {
                enemy_position_TMP_text.text = "";
            }


            if (angle_turret_TMP_text != null)
            {
                angle_turret_TMP_text.text = "";
            }
        }
    }


    void FindEnemy()
    {
        currentEnemy = null;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        float closestDistance = enemyDetectionDistance;


        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );


            if (distance <= closestDistance)
            {
                closestDistance = distance;
                currentEnemy = enemy;
            }
        }
    }


    void RotateTurretToEnemy()
    {
        if (turret == null || currentEnemy == null)
            return;


        Vector3 direction =
            currentEnemy.transform.position - turret.transform.position;


        direction.y = 0.0f;


        if (direction != Vector3.zero)
        {
            turret.transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }


    void DisplayTurretAngle()
    {
        if (turret == null || currentEnemy == null)
            return;


        Vector3 direction =
            currentEnemy.transform.position - turret.transform.position;


        direction.y = 0.0f;


        float angle =
            Vector3.SignedAngle(
                turret.transform.forward,
                direction,
                Vector3.up
            );


        if (angle_turret_TMP_text != null)
        {
            angle_turret_TMP_text.text =
                "Turret angle: " + angle.ToString("F1") + "°";
        }
    }
}