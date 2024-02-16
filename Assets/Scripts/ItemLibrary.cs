using UnityEngine;

[CreateAssetMenu(fileName = "New_Library", menuName = "ItemLibrary")]
public class ItemLibrary : ScriptableObject {
    [SerializeField]
    private Items[] items;

    public Items GetItem(int index) {
        return items[index];
    }

    public int GetLibraryLength() {
        return items.Length;
    }
}