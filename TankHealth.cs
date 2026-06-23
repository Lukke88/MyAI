using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class TankHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public TextMeshProUGUI healthText, name_text, InfoText;
    public Slider healthSlider;

    public GameObject gameOverPanel, healthBarPanel; // <-- NOVÉ

    public EnemyManager enemyManager;
	
	public Transform cam;
	
	public float baseScale = 0.01f;
	public float scaleMultiplier = 1f;

    void Start()
    {
        currentHealth = maxHealth;
        name_text.text = this.name;

        if(gameOverPanel != null)
            gameOverPanel.SetActive(false); // schovat na startu
		if (cam == null)
			cam = Camera.main.transform;
        UpdateHealthUI();
    }
	
	void LateUpdate()
	{
    if (healthBarPanel != null && cam != null)
    {
        // otočí panel směrem ke kameře
        healthBarPanel.transform.LookAt(cam);

        // otočení o 180°, aby nebyl zrcadlově
        healthBarPanel.transform.Rotate(0, 180f, 0);
		
		float distance = Vector3.Distance(cam.position, healthBarPanel.transform.position);
		float scale = distance * baseScale * scaleMultiplier;

		healthBarPanel.transform.localScale = new Vector3(scale, scale, scale);
    }
	}

    public void ApplyDamage(float dmg)
    {
        currentHealth -= dmg;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("Tank HP: " + currentHealth);

        InfoText.text = "You got hit and lost " + dmg.ToString() + "% of life. Take cover !!!";

        UpdateHealthUI();

        if(currentHealth <= 0f)
        {
            GameOver();
        }
    }

    void UpdateHealthUI()
    {
        float normalized = currentHealth / maxHealth;

        if(healthSlider != null)
        {
            healthSlider.value = normalized;
        }

        if(healthText != null)
        {
            int percent = Mathf.RoundToInt(normalized * 100f);
            healthText.text = percent + "%";
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");

        if(gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f; // zastaví hru

        Destroy(gameObject); // můžeš klidně zakomentovat pokud chceš tank nechat ve scéně
    }
}