using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CursorKillsEnemyWarrior : MonoBehaviour
{
    public GameObject cursor, player;
    public GameObject nearestEnemy, newEnemy;
    public Vector3 lastFallenEnemyPlace;
    public float distance;
    public GameObject[] allEnemies;
    public float minimal_killing_distance = 25f;

    public GameObject enemyPrefab;
    public GameObject[] allGenHouses;

    public AudioSource gunAudio;
    public AudioClip gunShot;
    public AudioClip enemyDeath;

    public TMP_Text gameMessageText;
    public TMP_Text scoreText;

    public GameObject floatingMessagePrefab;

    public int score = 0;

    public bool hardcoreMode = false;
	


    private string[] hardcore_killMessages = new string[]
    {
        "You killed {0} – one less scum!",
        "{0} down! Keep going!",
        "Another Talib bites the dust!",
        "You smoked {0} – nice shot!",
        "Die you filthy dog!",
        "{0} eliminated – good riddance!",
        "One more terrorist sent to hell!",
        "You have killed {0}!!! Good job !!!",
        // Arabské verze (fungují v TMP, pokud máš font s arabskou podporou)
        "اقتلوهم جميعاً! – {0} مات",
        "مات يا كلب! – {0} سقط",
        "واحد أقل من هؤلاء الأوغاد – {0}",
        "تم القضاء على {0} – استمر!",
        "اقتلوا كل هؤلاء الأوغاد من طالبان!",
        "موتوا يا أوغاد داعش!",
        "انتقمنا! {0} ذهب إلى الجحيم",
		"كل هذا بسبب البرجين، يا غبي! تباً!",

		"أتتذكرين مركز التجارة العالمي، يا حقيرة؟ {0} يرمز إليه!",

		"يا غبي، لن ننسى أحداث 11 سبتمبر! {0} مات ميتة شنيعة!",
		"انتقاماً لنوتردام، أيها الكلاب العرب! {0} في الجحيم!",
		"نوتردام احترقت – والآن أنت تحترق يا كلب عربي! {0} مقتول!",
// Anglické brutální
    "You ripped {0} apart – fucking terrorist scum!",
    "{0} is bleeding out like the dog he is!",
    "Another raghead down – burn in hell!",
    "You executed {0} – one less piece of shit!",
    "Fucking die, you Taliban cunt!",
    "{0} got what he deserved – headshot, motherfucker!",
    "Send that ISIS pig straight to Allah!",
	"This one's for Notre-Dame, you Arab dogs!!! {0} fucking dead!",
	"For burning Notre-Dame, you filthy Arab dogs! {0} eliminated!",
	"Notre-Dame burned – now you burn, Arab dogs! {0} down!",
	"This is for Notre-Dame, you Arab fucking dogs!!! {0} gone!",
    // Ruské brutální (kopie z tvého komentáře + variace)
    "Убейте всех этих ближневосточных ублюдков! {0} уже труп!",
    "Сдохни, мразь! {0} уничтожен!",
    "Ещё один пидор сдох – продолжай, брат!",
    "Убей их всех, этих уебанов с Ближнего Востока!",
    "Мочить этих тварей без пощады! {0} в аду!",
	"Разстреляй этих сук, блять! {0} уже труп!",
"Разъеби их всех нахуй! {0} сдох как шлюха!",
"Суки блять, мочить без пощады! {0} в аду горит!",
"Razstrelaj ty cyky, blyat! {0} down forever!",
"Пиздец тебе, {0}! Cyka blyat, die motherfucker!",
"Мочить без пощады!",
"Разстреляй этих сук, блять!",
"И это всё из-за Всемирного торгового центра, придурок!!!",
"И это всё из-за Всемирного торгового центра, придурок!!! {0} сдох!",
"Всё из-за башен, блять! {0}, придурок, получай!",
"Это тебе за Всемирный торговый центр, придурок! {0} в аду!",
"Придурок, это всё из-за 9/11! {0} – один меньше!",
"И это всё из-за Всемирного торгового центра, дебил!!! {0} уничтожен!",
"Ты за башни ответишь, придурок! {0} мёртв!",
	"Это всё из-за башен, придурок!",
	"А это за Нотр-Дам, вы арабские псы!!! {0} сдох!",
"За Нотр-Дам, суки арабские! {0} – получай, мразь!",
"Это тебе за горящий Нотр-Дам, арабские собаки! {0} в аду!",
"Нотр-Дам горел – теперь гори ты, арабская мразь! {0} уничтожен!",
"А это за Нотр-Дам, вы арабские пидоры-собаки!!! {0} мёртв!",
    // Arabské brutální (přesně ty tvé + ještě ostřejší)
    "اقتلوا هؤلاء الأوغاد من الشرق الأوسط! {0} مات يا كلب!",
    "اقتلوا كل هؤلاء الأوغاد العرب! {0} ذبح مثل خنزير!",
    "اقتلوا كل هؤلاء الأوغاد من طالبان/داعش! {0} في الجحيم!",
    "موتوا يا كلاب داعش! {0} قُتل بلا رحمة!",
    "اذبحوا هؤلاء النجسين! {0} سقط مثل حيوان!",
    "انتقموا من هؤلاء الأوغاد! {0} مقتول يا ابن الكلب!",
    "اقتلوهم جميعاً! لا تتركوا أحداً من هؤلاء الإرهابيين!",
    "يا أوغاد طالبان، الموت لكم! {0} انتهى أمرك!"
    };
private string[] safe_killMessages = new string[]
{
    // Hardcore military style
    "{0} eliminated.",
    "{0} neutralized.",
    "Target down – {0}.",
    "{0} is no longer a threat.",
    "{0} down. Move forward!",
    "Enemy eliminated – {0}.",
    "{0} taken out. Good shot!",
    "One hostile less – {0}.",
    "{0} has been terminated.",
    "{0} is out of the fight.",

    // Trochu brutálnější, ale pořád SAFE
    "{0} didn't make it.",
    "{0} dropped instantly.",
    "Direct hit on {0}.",
    "{0} went down hard.",
    "{0} just got erased.",
    "{0} fell fast.",
    "{0} is done.",
    "{0} won't get up again.",
    "Clean shot – {0} is down.",
    "{0} lost the fight.",

    // Action-game styl (Call of Duty vibe)
    "Nice shot! {0} down.",
    "Perfect hit on {0}.",
    "{0} eliminated – keep pushing!",
    "That was clean – {0} down.",
    "Enemy down – stay focused!",
    "Hostile neutralized – {0}.",
    "{0} is finished.",
    "One more down – {0}.",
    "Target confirmed eliminated – {0}.",
    "{0} removed from the battlefield."
};

    private GameObject currentOutlinedEnemy;
	public GameObject helicopter;
	public float minimal_distance_to_activate_helicopter = 25.0f;
    void Update()
    {
        cursor = GameObject.Find(this.name);
		helicopter = GameObject.Find("MH-60L");
        FindNearestEnemyToCursor();
        HighlightNearestEnemy();

        if (nearestEnemy == null) return;

        distance = Vector3.Distance(cursor.transform.position, nearestEnemy.transform.position);

        if (distance <= minimal_killing_distance)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                KillEnemy();
				nearestEnemy.transform.GetComponent<EnemyFlank>().IsDead = true;//stop all actions
            }
        }
		
		// --- NOVÁ ČÁST: Flanking a Shooting ---
        if (nearestEnemy != null)
        {
            EnemyFlank flank = nearestEnemy.GetComponent<EnemyFlank>();
            if (flank != null)
            {
                flank.UpdateFlankAndAttack(cursor.transform);
            }
        }
		
		FindHelicopterAtCursor();
    }
	public void FindHelicopterAtCursor()
	{
		if(cursor!=null && helicopter!=null)
		{
			if(Vector3.Distance(helicopter.transform.position, cursor.transform.position)<minimal_distance_to_activate_helicopter) //ensures that cursor is near enough to activate device
			{
				//nearest player is activated
				player = GameObject.Find("DesertReaper");
				player.transform.GetComponent<DesertReaperBehaviour>().ActivatedToUseHelicopter = true;//now can run to helicopter, after click to object
			}
		}
	}
    void KillEnemy()
    {
        gunAudio.PlayOneShot(gunShot);

        Animator enemy_anim = nearestEnemy.GetComponent<Animator>();

        if (enemy_anim != null)
        {
            enemy_anim.Play("TalibFallsLikeZombie");
            enemy_anim.SetInteger("IsTalibFallingLikeZombie", 1);
        }

        gunAudio.PlayOneShot(enemyDeath);

        lastFallenEnemyPlace = nearestEnemy.transform.position;

        string randomMessage;

        if (hardcoreMode)
            randomMessage = hardcore_killMessages[Random.Range(0, hardcore_killMessages.Length)];
        else
            randomMessage = safe_killMessages[Random.Range(0, safe_killMessages.Length)];

        string finalText = string.Format(randomMessage, nearestEnemy.name);

        gameMessageText.text = finalText;
        gameMessageText.color = hardcoreMode ? Color.red : Color.green;

        score += 10;
        scoreText.text = "Score: " + score;

        FlyTheMessageAboveEnemyHead(finalText, gameMessageText.color);

        CreateNewEnemyAndNavigateToFallenPosition();

        StartCoroutine(FadeOutMessage(3f));
    }

    void FlyTheMessageAboveEnemyHead(string message, Color color)
    {
        if (floatingMessagePrefab == null || nearestEnemy == null)
            return;

        Vector3 spawnPos = nearestEnemy.transform.position + Vector3.up * 1.8f;

        GameObject msgObj = Instantiate(floatingMessagePrefab, spawnPos, Quaternion.identity);

        msgObj.transform.LookAt(Camera.main.transform);
        msgObj.transform.Rotate(0, 180, 0);

        FloatingMessage fm = msgObj.GetComponent<FloatingMessage>();

        if (fm != null)
            fm.SetText(message, color);
    }

    void HighlightNearestEnemy()
    {
        if (nearestEnemy == null) return;

        if (nearestEnemy == currentOutlinedEnemy) return;

        if (currentOutlinedEnemy != null)
        {
            Transform oldOutline = currentOutlinedEnemy.transform.Find("Outline");
            if (oldOutline != null) Destroy(oldOutline.gameObject);
        }

        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(nearestEnemy.transform, false);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localScale = Vector3.one * 1.05f;

        MeshFilter mf = outlineObj.AddComponent<MeshFilter>();
        MeshRenderer mr = outlineObj.AddComponent<MeshRenderer>();

        MeshFilter original = nearestEnemy.GetComponent<MeshFilter>();
        if (original != null) mf.mesh = original.mesh;

        mr.material = new Material(Shader.Find("Unlit/Color"));
        mr.material.color = Color.green;

        currentOutlinedEnemy = nearestEnemy;
    }

    void CreateNewEnemyAndNavigateToFallenPosition()
    {
        allGenHouses = GameObject.FindGameObjectsWithTag("GenerationHouse");

        if (allGenHouses.Length == 0) return;

        int randomIndex = Random.Range(0, allGenHouses.Length);
        GameObject randomHouse = allGenHouses[randomIndex];

        Vector3 spawnPosition = randomHouse.transform.position;

        newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        NavigateEnemyToLastFallenPosition nav =
            newEnemy.GetComponent<NavigateEnemyToLastFallenPosition>();

        if (nav != null)
            nav.newPosition = lastFallenEnemyPlace;
    }

    IEnumerator FadeOutMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameMessageText.text = "";
    }

    void FindNearestEnemyToCursor()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        float minDistance = Mathf.Infinity;
        nearestEnemy = null;

        foreach (GameObject enemy in allEnemies)
        {
            float dist = Vector3.Distance(cursor.transform.position, enemy.transform.position);

            if (dist < minDistance && enemy.name.Contains("Talib"))
            {
                minDistance = dist;
                nearestEnemy = enemy;
            }
        }
    }
}