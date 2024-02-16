using UnityEngine;

public class IOPort : MonoBehaviour {

    [SerializeField]
    private WorkTable.SlotType slotType;
    [SerializeField]
    private int slotIndex;
    [SerializeField]
    private WorkTable myWorktable;

    private void Start() {
        if (myWorktable == null)
            myWorktable = GetComponentInParent<WorkTable>();
        if (gameObject.name == "Output")
            slotType = WorkTable.SlotType.output;
        else if (gameObject.name == "Input")
            slotType = WorkTable.SlotType.input;
    }

    public WorldItem AddToSlot(WorldItem item) {
        return myWorktable.AddToSlot(item, slotType, slotIndex);
    }

    public WorldItem RemoveFromSlot(WorldItem currentItems = null) {
        return myWorktable.RemoveFromSlot(slotType, slotIndex, currentItems);
    }
}