using UnityEngine;
using UnityEngine.UI;           // pokud bys potřeboval Image apod.
/*
Vytvoř prázdný GameObject Compass_round (pokud ještě nemáš)
Pod něj dej child Arrow (Image s šipkou)
Na Compass_round (nebo samostatný manager) přidej tento skript
Do pole player přetáhni hráče nebo Main Camera
Do pole arrowRect přetáhni RectTransform šipky
*/
public class CompassArrow : MonoBehaviour
{
    [SerializeField] private Transform player;           // tvůj hráč / kamera
    [SerializeField] private RectTransform arrowRect;    // RectTransform šipky (Arrow)

    // Možnost 1: ukazovat na konkrétní GameObject (chest, enemy_artillery, ...)
    public void PointToTarget(Transform target)
    {
        if (target == null || player == null) return;

        Vector3 toTarget = target.position - player.position;
        toTarget.y = 0; // ← důležité – ignorujeme výšku (jen horizontální směr)

        if (toTarget.sqrMagnitude < 0.1f) 
        {
            arrowRect.localEulerAngles = Vector3.zero; // blízko cíle → schováme / reset
            return;
        }

        float angle = Mathf.Atan2(toTarget.z, toTarget.x) * Mathf.Rad2Deg - 90f;
        // -90 protože šipka ve sprite obvykle ukazuje nahoru (Y+), ale Atan2 vrací 0° vpravo

        arrowRect.localEulerAngles = new Vector3(0, 0, angle);
    }
//kompas s naklonem
	void PointToTarget3D(Transform target)
{
    Vector3 dir = (target.position - player.position).normalized;
    Vector3 flatDir = new Vector3(dir.x, 0, dir.z).normalized;

    // horizontální úhel
    float horizontalAngle = Mathf.Atan2(flatDir.z, flatDir.x) * Mathf.Rad2Deg - 90f;

    // vertikální úhel (nahoru/dolů)
    float verticalAngle = Mathf.Asin(dir.y) * Mathf.Rad2Deg;

    arrowRect.localEulerAngles = new Vector3(0, 0, horizontalAngle);
    // případně pokud chceš naklonit i šipku:
    // arrowRect.localEulerAngles = new Vector3(-verticalAngle, 0, horizontalAngle);
}
    // Možnost 2: přímo na souřadnice (x,z)
    public void PointToPosition(Vector3 worldPosition)
    {
        if (player == null) return;

        Vector3 toTarget = worldPosition - player.position;
        toTarget.y = 0;

        if (toTarget.sqrMagnitude < 0.1f) 
        {
            arrowRect.localEulerAngles = Vector3.zero;
            return;
        }

        float angle = Mathf.Atan2(toTarget.z, toTarget.x) * Mathf.Rad2Deg - 90f;
        arrowRect.localEulerAngles = new Vector3(0, 0, angle);
    }


    // Bonus: Update verze – neustále na nejbližší cíl (např. tag)
    void Update()
    {
        // příklad: hledá první objekt s tagem "Chest"
        GameObject chest = GameObject.FindWithTag("Chest");
        if (chest != null)
        {
            PointToTarget(chest.transform);
        }
		var target = GameObject.Find("Chest")?.transform;
    if (target)
    {
        Vector3 flat = (target.position - player.position);
        flat.y = 0;
        arrowRect.up = flat.normalized;   // ← nejkratší zápis (funguje, když šipka ukazuje nahoru)
    }
        // nebo na konkrétní pozici
        // PointToPosition(new Vector3(150, 0, -80));
    }
}