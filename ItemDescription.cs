using UnityEngine;

[CreateAssetMenu(fileName = "NewItemDescription", menuName = "Items/ItemDescription")]
public class ItemDescription : ScriptableObject
{
    public string itemName;        // název zbraně
    [TextArea] public string description; // podrobnosti, popis
    public int maxAmmo;            // maximální počet nábojů
    public Sprite icon;            // obrázek pro infotext/UI
}

