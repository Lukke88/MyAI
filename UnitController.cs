using UnityEngine;
using UnityEngine.AI;

public class UnitController : MonoBehaviour
{
    public bool IsSelected;
    public string shooting_animation = "shooting_animation";
    public string shooting_anim_param = "shooting_anim_param";

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 targetPosition;
    private bool movingToTile;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void MoveTo(Vector3 pos)
    {
        movingToTile = true;
        targetPosition = pos;
        agent.SetDestination(pos);
    }

    void Update()
    {
        if (movingToTile && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            movingToTile = false;
            if (animator != null)
                animator.SetTrigger(shooting_animation);
        }
    }
public void SetFinalStance(string stanceName)
{
    GameObject stancePrefab = Resources.Load<GameObject>(stanceName);

    if (stancePrefab == null)
        return;

    foreach (Transform child in transform)
        Destroy(child.gameObject);

    GameObject newModel = Instantiate(stancePrefab, transform);
    newModel.transform.localPosition = Vector3.zero;
}
    public void LookAtPoint(Vector3 point)
    {
        Vector3 dir = (point - transform.position).normalized;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void Shoot()
    {
        if (animator != null)
            animator.SetTrigger(shooting_animation);
    }
}