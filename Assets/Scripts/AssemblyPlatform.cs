using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssemblyPlatform : MonoBehaviour {
    [SerializeField]
    private WorldItem output;

    [Header("Necessity")]
    [SerializeField]
    private WorldItem door;
    [SerializeField]
    private WorldItem frame;

    [Header("Optional")]
    [SerializeField]
    private WorldItem handle;
    [SerializeField]
    private List<WorldItem> details;

    [SerializeField]
    private Transform handlePosition;

    [Header("IO")]
    [SerializeField]
    private List<WorldItem> recencyList;

    [Header("Crafting")]
    [SerializeField]
    private GameObject doorPrefab;
    [SerializeField]
    private ItemData completeDoorItemData;
    [SerializeField]
    private bool readyForCrafting = false;
    [SerializeField]
    private float progress = 0f;
    [SerializeField]
    private float craftingTime = 5f;
    [SerializeField]
    private bool crafting = false;
    [SerializeField]
    private Image readyIndicator;
    [SerializeField]
    private Image progressBar;
    [SerializeField]
    private Animator animator;

    private void Awake() {
        readyForCrafting = output == null && door != null && frame != null;
        readyIndicator.color = !readyForCrafting ? Color.red : Color.green;
    }

    #region Crafting
    private void LateUpdate() {
        if (crafting) {

            progress += Time.deltaTime;
            if (progress >= craftingTime) {
                CreateItem();
            }

            if (animator != null) {
                if (!animator.GetBool("On"))
                    animator.SetBool("On", true);
            }
        } else {
            if (animator != null) {
                if (animator.GetBool("On"))
                    animator.SetBool("On", false);
            }
        }

        crafting = false;
        if (progressBar != null) {
            if (readyForCrafting) {
                progressBar.fillAmount = progress / craftingTime;
                if (progressBar.color != Color.red)
                    progressBar.color = Color.red;
            } else if (output != null && progressBar.fillAmount != 1) {
                progressBar.fillAmount = 1;
                if (progressBar.color != Color.green)
                    progressBar.color = Color.green;
            } else if (output == null && progressBar.fillAmount != 0)
                progressBar.fillAmount = 0;
        }
    }

    public bool Craft() {
        if (!readyForCrafting)
            crafting = false;
        else
            crafting = true;

        return crafting;
    }

    public bool CreateItem() {
        if (door == null || frame == null || output != null)
            return false;

        WorldItem completedDoor = Instantiate(doorPrefab).GetComponent<WorldItem>();
        completedDoor.transform.SetParent(transform.GetChild(0), true);
        //completedDoor.transform.localScale = Vector3.one;
        completedDoor.transform.position = transform.position;
        completedDoor.InitialiseWorldItem(new CombinedItems(completeDoorItemData, 1));

        // Get all the components of this combined item
        List<Items> myComponents = new List<Items>();
        myComponents.AddRange(new Items[2] { door.GetItem(), frame.GetItem() });
        if (handle != null)
            myComponents.Add(handle.GetItem());

        foreach (WorldItem detail in details) {
            myComponents.Add(detail.GetItem());
        }
        Debug.Log(myComponents);
        completedDoor.GetItem().SetItemComponents(myComponents.ToArray());
        completedDoor.UpdateVisuals();
        if (completedDoor.transform.childCount > myComponents.Count) {
            for (int i = transform.GetChild(0).childCount - 1; i >= myComponents.Count; i--) {
                completedDoor.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        output = completedDoor;

        // Delete the components
        Destroy(door.gameObject);
        door = null;
        Destroy(frame.gameObject);
        frame = null;
        if (handle != null)
            Destroy(handle.gameObject);
        handle = null;
        foreach (WorldItem item in details) {
            if (item != null)
                Destroy(item.gameObject);
        }
        details.Clear();

        recencyList.Clear();

        // Reset Crafting
        CancelCrafting();
        readyForCrafting = output == null && door != null && frame != null;
        readyIndicator.color = !readyForCrafting ? Color.red : Color.green;

        Debug.Log("Created " + completedDoor.name);

        return true;
    }

    public void CancelCrafting() {
        crafting = false;
        progress = 0f;
        if (animator != null)
            animator.SetBool("On", false);
    }
    #endregion

    public WorldItem AddItem(WorldItem item) {
        WorldItem outGoingItem = null;
        if (output != null) {
            outGoingItem = output;
            output = null;
            //return outGoingItem;
        }

        switch (item.GetItem().GetItemData().type) {
            case ItemData.ItemType.Door:
                if (door != null)
                    outGoingItem = door;
                door = item;
                door.transform.SetParent(transform.GetChild(0), true);
                door.transform.position = transform.position;
                break;
            case ItemData.ItemType.Frame:
                if (frame != null)
                    outGoingItem = frame;
                frame = item;
                frame.transform.SetParent(transform.GetChild(0), true);
                frame.transform.position = transform.position;
                break;
            case ItemData.ItemType.Handle:
                if (handle != null)
                    outGoingItem = handle;
                handle = item;
                handle.transform.SetParent(transform.GetChild(0), true);
                handle.transform.position = handlePosition.position;
                break;
            default:
                return item;
        }

        recencyList.Add(item);

        readyForCrafting = output == null && door != null && frame != null;
        readyIndicator.color = !readyForCrafting ? Color.red : Color.green;
        if (!readyForCrafting)
            CancelCrafting();

        return outGoingItem;
    }

    public WorldItem RemoveItem(ItemData.ItemType itemType) {
        WorldItem outGoingItem = null;
        if (output != null) {
            outGoingItem = output;
            output = null;

            readyForCrafting = output == null && door != null && frame != null;
            readyIndicator.color = !readyForCrafting ? Color.red : Color.green;
            return outGoingItem;
        }

        switch (itemType) {
            case ItemData.ItemType.Door:
                outGoingItem = door;
                door = null;
                break;
            case ItemData.ItemType.Frame:
                outGoingItem = frame;
                frame = null;
                break;
            case ItemData.ItemType.Handle:
                outGoingItem = handle;
                handle = null;
                break;
            default:
                return null;
        }

        readyForCrafting = output == null && door != null && frame != null;
        readyIndicator.color = !readyForCrafting ? Color.red : Color.green;
        if (!readyForCrafting)
            CancelCrafting();

        return outGoingItem;
    }
    public WorldItem RemoveItem() {
        WorldItem outGoingItem;
        if (output != null) 
            outGoingItem = output;
        else
            outGoingItem = recencyList[recencyList.Count - 1];
        return RemoveItem(outGoingItem.GetItem().GetItemData().type);
    }

    public void UndoAll() {

    }
}