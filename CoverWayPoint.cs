using UnityEngine;

public class CoverWaypoint : MonoBehaviour
{
    public HeroFindsCover heroCover;

    void OnMouseDown()
    {
        heroCover.selectedWaypoint = gameObject;
    }
}
