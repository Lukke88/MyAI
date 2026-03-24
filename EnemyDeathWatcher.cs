using UnityEngine;

public class EnemyDeathWatcher : MonoBehaviour
{
    public EnemySpawnManager manager;

    private BaseTalibEnemyAI ai;
    private bool reported = false;

    void Start()
    {
        ai = GetComponent<BaseTalibEnemyAI>();
    }

    void Update()
    {
        if (ai != null && ai.IsDead && !reported)
        {
            reported = true;
            manager.OnEnemyKilled();
            Destroy(gameObject, 2f);
        }
    }
}
