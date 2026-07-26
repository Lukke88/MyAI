using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class UIButtonHighlight :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public TextMeshProUGUI text;

    public void OnPointerEnter(PointerEventData eventData)
{
    text.outlineWidth = 0.2f;
}

public void OnPointerExit(PointerEventData eventData)
{
    text.outlineWidth = 0.0f;
}
}