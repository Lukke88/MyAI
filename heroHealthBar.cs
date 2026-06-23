using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroHealthBar : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI")]
    public Slider healthSlider;
    public TMP_Text healthText;


    public enum DamageAmount
{
    Damage5 = 5,
    Damage10 = 10,
    Damage25 = 25,
    Damage50 = 50
}

public DamageAmount damagePerLightBeam = DamageAmount.Damage5;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Debug.Log("Player dead");
            // sem pozdeji pride smrt hrace
        }
    }

    void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = "health: " + Mathf.RoundToInt(currentHealth) + "%";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("lightBeam"))
        {
            TakeDamage((float)damagePerLightBeam);

            Debug.Log("Player hit. Health = " + currentHealth);
        }
    }
}