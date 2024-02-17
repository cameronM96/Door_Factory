using System.Collections;
using UnityEngine;

public class WorldItem : MonoBehaviour {
    [SerializeField]
    private Items myItems = new Items();

    public void InitialiseWorldItem(Items newItems) {
        InitialiseWorldItem(newItems.GetItemData(), newItems.GetItemQuantity(), Color.white);
    }

    public void InitialiseWorldItem(Items newItems, Color newColour) {
        InitialiseWorldItem(newItems.GetItemData(), newItems.GetItemQuantity(), newColour);
    }

    public void InitialiseWorldItem(ItemData newItemData, int newQuantity, Color newColour) { 
        myItems = new Items(newItemData, newQuantity);
        myItems.SetColor(newColour);
        UpdateVisuals();
    }

    public void InitialiseWorldItem(CombinedItems newCombinedItem) {
        myItems = newCombinedItem;
    }

    public Items GetItem() {
        return myItems;
    }

    public Items TakeItem(int takeQuantity = 1, bool takeAsMuchAsPossible = false) {
        Items takenItems = myItems.TakeItem(takeQuantity, takeAsMuchAsPossible);
        if (myItems.GetItemQuantity() <= 0)
            StartCoroutine(DestroyObject());
        else
            UpdateVisuals();
        return takenItems;
    }

    public int GiveItem(Items newItem) {
        int excessItems = myItems.GiveItem(newItem);
        UpdateVisuals();
        return excessItems;
    }

    public void UpdateVisuals() {
        if (myItems.GetType() == typeof(CombinedItems)) {
            Items[] components = myItems.GetItemComponents();
            for (int i = 0; i < components.Length; i++) {
                transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = components[i].GetItemData().icon;
                transform.GetChild(i).GetComponent<SpriteRenderer>().color = components[i].GetColor();
                
            }
        } else {
            for (int i = 0; i < myItems.GetItemData().stackCap; i++) {
                transform.GetChild(i).GetComponent<SpriteRenderer>().color = myItems.GetColor();
                if (i + 1 <= myItems.GetItemQuantity())
                    transform.GetChild(i).gameObject.SetActive(true);
                else
                    transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        gameObject.name = myItems.GetItemData().name + " x" + myItems.GetItemQuantity();
    }

    IEnumerator DestroyObject() {
        yield return null;
        Destroy(gameObject);
    }
}