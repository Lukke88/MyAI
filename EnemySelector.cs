using UnityEngine;

public class EnemySelector : MonoBehaviour
{
    public Camera cam;
    public Transform turret;

    private GameObject currentTarget;
  //  private Outline lastOutline;

    void Update()
    {
        HandleSelection();
        HandleRotation();
        HandleShoot();
    }

    void HandleSelection()
    {
        if (!Input.GetKey(KeyCode.LeftControl))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                SetTarget(hit.collider.gameObject);
            }
        }
    }

    void SetTarget(GameObject obj)
    {
        if (currentTarget == obj) return;

        // vypni starý outline
       /* if (lastOutline != null)
            lastOutline.enabled = false;*/

        currentTarget = obj;
      //  lastOutline = obj.GetComponent<Outline>();
/*
        if (lastOutline != null)
        {
            lastOutline.enabled = true;
            lastOutline.OutlineColor = Color.green;
            lastOutline.OutlineWidth = 5f;
        }*/
    }
	
	    void HandleRotation()//rotace veze tanku na oznaceneho enemy
    {
        if (currentTarget == null) return;

        Vector3 dir = currentTarget.transform.position - turret.position;
        dir.y = 0f;

        if (dir == Vector3.zero) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        turret.rotation = Quaternion.Slerp(turret.rotation, rot, Time.deltaTime * 5f);
    }
	
	    private float clickTime = 0f;
    private float doubleClickDelay = 0.3f;

    void HandleShoot()//strelba tanku na enemy
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - clickTime < doubleClickDelay)
            {
                Shoot();
            }

            clickTime = Time.time;
        }
    }

    void Shoot()
    {
        if (currentTarget == null) return;

        Debug.Log("FIRE at " + currentTarget.name);

        // sem napojíš svůj beam / damage systém
        // ShootToTarget(currentTarget);
    }
	/*
	⚠️ Co musíš mít v Unity
		Enemy objekty mají tag: "Enemy"
		Enemy mají Collider
		Enemy mají Outline komponentu (nebo shader)
		Camera je přiřazená v inspectoru
		turret transform je osa věže
	*/
}
