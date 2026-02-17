using UnityEngine;
using System.Collections.Generic;

/*
//=========================
// Detekce zdi
//=========================
public static class WallDetector
{
    /// <summary>
    /// Kontroluje, zda je mezi start a target objekt se zdí (tag "Wall")
    /// </summary>
    public static bool IsWallBetween(Vector3 start, Vector3 target)
    {
        Vector3 direction = (target - start).normalized;
        float distance = Vector3.Distance(start, target);

        if (Physics.Raycast(start + Vector3.up * 1.5f, direction, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag("Wall"))
                return true;
        }
        return false;
    }
}*/

//=========================
// Třída pro správu zranění
//=========================
[System.Serializable]
public class EnemyDamageClass
{
    public BaseTalibEnemyAI baseAI; // odkaz na AI
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Animace při zranění")]
    public Dictionary<string, int> damageParameters = new Dictionary<string, int>(); // string = název animace, int = index parametru
    public string hitAnimation;       // TalibKnockedOut / TalibGetsHitKneeling / TalibHitShoulder
    public string standUpAnimation;   // TalibStandingUpFromDeath
    public string deathAnimation;     // TalibFallingLikeZombie

    [Header("UI")]
    public UnityEngine.UI.Slider healthSlider;

    public EnemyDamageClass(BaseTalibEnemyAI ai)
    {
        baseAI = ai;
        currentHealth = maxHealth;

        // Naplníme dictionary podle BaseTalibEnemyAI enumů
        foreach (BaseTalibEnemyAI.Animations anim in System.Enum.GetValues(typeof(BaseTalibEnemyAI.Animations)))
        {
            damageParameters[anim.ToString()] = (int)anim;
        }

        // Default animace
        hitAnimation = "TalibGetsHitKneeling";
        standUpAnimation = "TalibStandingUpFromDeath";
        deathAnimation = "TalibFallingLikeZombie";
    }

    /// <summary>
    /// Zpracuje zranění
    /// </summary>
    public void ApplyDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth -= amount;
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;

        if (currentHealth <= 0f)
        {
            // Smrt
            PlayAnimation(deathAnimation);
            DisableAI();
            AwardPlayerPoints(50);
        }
        else
        {
            // Zranění, ale žije
            PlayAnimation(hitAnimation);
        }
    }

    private void PlayAnimation(string animationName)
    {
        if (baseAI == null) return;

        if (damageParameters.TryGetValue(animationName, out int index))
        {
            // Nastavíme parameter Animatoru
            baseAI.animator.SetInteger(baseAI.parameterNames[index], 1);
            baseAI.currentAnimation = (BaseTalibEnemyAI.Animations)index;
        }
    }

    private void DisableAI()
    {
        /*if (baseAI == null) return;

        // Deaktivujeme všechny skripty AI
        foreach (var comp in baseAI.GetComponents<MonoBehaviour>())
        {
            if (comp != this)
                comp.enabled = false;
        }*/
    }

    private void AwardPlayerPoints(int points)
    {
        // TODO: napojit na herní skóre
        Debug.Log($"Player awarded {points} points!");
    }
}
