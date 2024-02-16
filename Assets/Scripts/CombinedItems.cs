using System;
using UnityEngine;

[Serializable]
public class CombinedItems : Items {
    [SerializeField]
    private Items[] itemComponents;

    public CombinedItems(ItemData newItemData = null, int newQuantity = 0) {
        itemData = newItemData;
        quantity = newQuantity;
    }

    public Items GetItemComponent(int index) { return itemComponents[index]; }

    public override Items[] GetItemComponents() {
        return itemComponents;
    }

    public override void SetItemComponents(Items[] newComponents) {
        itemComponents = newComponents;
    }

    public int GetCombinedValue() {
        int combinedValue = 0;
        foreach (Items item in itemComponents) {
            if (item == null)
                continue;
            if (item.GetItemData() == null)
                continue;

            combinedValue += item.GetItemData().value * item.GetItemQuantity();
        }

        return combinedValue;
    }
}