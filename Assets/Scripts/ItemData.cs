using UnityEngine;

[CreateAssetMenu(fileName = "New_Item", menuName = "Item")]
public class ItemData : ScriptableObject {
    public int stackCap = 1;
    public int value = 1;

    public Sprite icon;
    public GameObject worldItemPrefab;
    public enum ItemType {
        Door,
        Frame,
        Handle,
        CompleteDoor,
        Other
    }
    public ItemType type = ItemType.Other;
}
