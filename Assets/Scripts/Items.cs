using System;
using UnityEngine;

[Serializable]
public class Items {

    [SerializeField]
    protected ItemData itemData;
    [SerializeField]
    protected int quantity;
    [SerializeField]
    protected Color color = Color.white;

    public Items(ItemData newItemData = null, int newQuantity = 0) {
        itemData = newItemData;
        quantity = newQuantity;
    }
    public ItemData GetItemData() {
        return itemData;
    }

    public int GetItemQuantity() {
        return quantity;
    }

    public Color GetColor() {
        return color;
    }

    public void SetColor(Color newColor) {
        color = newColor;
    }

    public virtual Items[] GetItemComponents() { return null; }

    public virtual void SetItemComponents(Items[] newComponents) { }

    public Items TakeItem(int takeQuantity = 1, bool takeAsMuchAsPossible = false) {
        if (takeQuantity > quantity && !takeAsMuchAsPossible) {
            return new Items();
        } else if (takeQuantity > quantity && takeAsMuchAsPossible) {
            Items takenItem = new Items(itemData, quantity);
            quantity = 0;
            return takenItem;
        } else if (takeQuantity <= quantity) {
            quantity -= takeQuantity;
            return new Items(itemData, takeQuantity);
        } else {
            return new Items();
        }
    }

    public int GiveItem(Items newItem) {
        if (newItem.itemData == itemData) {
            if (quantity == itemData.stackCap) {
                // Already full
                return newItem.quantity;
            } else {
                // Add items together and return excess
                int excess = Mathf.Max((quantity + newItem.quantity) - itemData.stackCap, 0);
                quantity += newItem.quantity - excess;
                return excess;
            }
        } else {
            // These can't stack
            return newItem.quantity;
        }
    }
}