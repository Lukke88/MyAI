using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Renderer))]
public class SelectableHighlighter : MonoBehaviour
{
    public Color highlightColor = Color.yellow; // Nastavitelná barva
    public TMP_Text infoText; // TMP text, kam vypíšeme info
    private Color originalColor;
    private Renderer rend;
	public string objectName;
    private float clickTime = 0f;
    private float clickDelay = 0.3f; // Dvojklik timeout

    private HeroController hero; // odkaz na HeroController

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        hero = FindObjectOfType<HeroController>();
		objectName = this.name;
    }

    void OnMouseEnter()
    {
        rend.material.color = highlightColor;

        string objectName = gameObject.name;

        if (CompareTag("Selectable"))
        {
            infoText.text = "You have selected " + objectName + ". Do you want to grab the item?";
        }
        else if (CompareTag("Enemy"))
        {
            float distance = Vector3.Distance(hero.transform.position, transform.position);
            infoText.text = objectName + " is in distance " + distance.ToString("F1") + ". Do you want to destroy him?";
        }
    }

    void OnMouseExit()
    {
        rend.material.color = originalColor;
        infoText.text = "";
    }

    void OnMouseDown()
    {
        // kontrola dvojkliku
        if (Time.time - clickTime < clickDelay)
        {
            // dvojklik detekován
            if (CompareTag("Selectable"))
            {
                hero.GrabItem(gameObject); // metoda ve vašem HeroControlleru
                infoText.text = objectName + " has been added to your inventory!";
            }
            else if (CompareTag("Enemy"))
            {
              //  hero.AttackEnemy(gameObject); // metoda pro útok na Enemy
                infoText.text = objectName + " is being attacked!";
            }
        }
        else
        {
            clickTime = Time.time; // uloží čas prvního kliknutí
        }
    }
}
