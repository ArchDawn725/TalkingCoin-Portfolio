using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemSO")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public int price;
    [TextArea]
    public string description;
    public GameObject prefab;
}
