using UnityEngine;
using System.Collections.Generic;

public class WallAttachmentManager : MonoBehaviour
{
    public bool IsActivatedAttachmentToWall;
    public RTSSelectionManager selectionManager;

    private List<GameObject> ghostUnits = new List<GameObject>();
    private float lastClickTime;
    private float doubleClickDelay = 0.25f;

    void Update()
    {
        if (!IsActivatedAttachmentToWall)
            return;

        HandleGhostPreview();

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickDelay)
            {
                ConfirmPlacement();
            }

            lastClickTime = Time.time;
        }
    }

    void HandleGhostPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (!hit.collider.CompareTag("Wall"))
                return;

            ClearGhosts();

            List<UnitController> selected = selectionManager.GetSelectedUnits();
            if (selected.Count == 0)
                return;

            float wallLength = hit.collider.bounds.size.x;
            int count = selected.Count;
            float segment = wallLength / count;
            Vector3 start = hit.collider.bounds.min;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = start + hit.collider.transform.right * segment * i;

                GameObject ghostPrefab = GetStancePrefab(hit.collider, selected[i]);
                GameObject ghost = Instantiate(ghostPrefab, pos, Quaternion.identity);

                SetTransparent(ghost, 0.2f); // 80% transparent

                ghostUnits.Add(ghost);
            }
        }
    }

    GameObject GetStancePrefab(Collider wall, UnitController unit)
    {
        float wallHeight = wall.bounds.size.y;
        float unitHeight = unit.GetComponent<Collider>().bounds.size.y;

        string stanceName = "standing_soldirer";

        if (wallHeight > unitHeight)
            stanceName = "snitching_soldier";
        else if (wallHeight > unitHeight * 0.5f)
            stanceName = "crouching_soldier";
        else if (wallHeight <= unitHeight * 0.3f)
            stanceName = "standing_soldirer";

        return Resources.Load<GameObject>(stanceName);
    }

    void ConfirmPlacement()
    {
        List<UnitController> selected = selectionManager.GetSelectedUnits();
        if (selected.Count == 0 || ghostUnits.Count == 0)
            return;

        for (int i = 0; i < selected.Count; i++)
        {
            selected[i].MoveTo(ghostUnits[i].transform.position);

            selected[i].SetFinalStance(ghostUnits[i].name.Replace("(Clone)", ""));
        }

        ClearGhosts();
        IsActivatedAttachmentToWall = false;
    }

    void ClearGhosts()
    {
        foreach (var ghost in ghostUnits)
            Destroy(ghost);

        ghostUnits.Clear();
    }

    void SetTransparent(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.SetFloat("_Mode", 3);
                m.color = new Color(m.color.r, m.color.g, m.color.b, alpha);
                m.EnableKeyword("_ALPHABLEND_ON");
                m.renderQueue = 3000;
            }
        }
    }
}