using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingMessage : MonoBehaviour
{
    public float moveUpSpeed   = 1.8f;     // jak rychle stoupá
    public float fadeTime      = 2.2f;     // celková doba života
    public float growFactor    = 1.15f;    // mírné zvětšení (1.0 = bez změny)

    private TMP_Text tmpText;
    private float timer = 0;

    void Awake()
    {
        tmpText = GetComponentInChildren<TMP_Text>();
        if (tmpText == null)
        {
            Debug.LogError("FloatingMessage: nenašel TMP_Text v dětech!");
            Destroy(gameObject);
        }
    }

    public void SetText(string message, Color color)
    {
        if (tmpText != null)
        {
            tmpText.text = message;
            tmpText.color = color;
        }
    }
void LateUpdate()
{
    transform.rotation = Camera.main.transform.rotation;
}
    void Update()
    {
        timer += Time.deltaTime;

        // Pohyb nahoru
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        // Mírné zvětšení (volitelné)
        float scale = 1f + (growFactor - 1f) * (timer / fadeTime);
        transform.localScale = Vector3.one * scale;

        // Fade out
        if (timer >= fadeTime)
        {
            Destroy(gameObject);
        }
        else if (timer > fadeTime * 0.4f) // začíná blednout od ~40 % doby
        {
            float alpha = 1f - ((timer - fadeTime * 0.4f) / (fadeTime * 0.6f));
            Color c = tmpText.color;
            c.a = Mathf.Clamp01(alpha);
            tmpText.color = c;
        }
    }
}