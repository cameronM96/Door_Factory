using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkTable : MonoBehaviour {
    [SerializeField]
    private Recipe[] recipes = new Recipe[1];

    [SerializeField]
    private InvSlot[] input = new InvSlot[1];
    [SerializeField] 
    private InvSlot[] output = new InvSlot[1];

    [Header("Crafting")]
    [SerializeField]
    private int recipeIndex = -1;
    [SerializeField]
    private int[] inputIndicies = new int[0];
    [SerializeField]
    private float progress = 0f;
    [SerializeField]
    private bool readyForCrafting = false;
    [SerializeField]
    private bool crafting = false;

    [Header("Usability")]
    [SerializeField]
    private bool automated;
    [SerializeField]
    private GameObject[] outputDisplays;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Image progressBar;

    [SerializeField]
    private GameObject recipesViewer;


    private void Awake() {
        animator = GetComponent<Animator>();
        CheckIfReadyToCraft();
        if (progressBar != null )
            progressBar.GetComponentInParent<Canvas>().worldCamera = Camera.main;
        InitialiseRecipeView();
        ToggleRecipes(false);
    }

    private void Update() {
        if (readyForCrafting && !crafting && automated)
            crafting = true;
    }

    private void LateUpdate() {
        if (crafting) {

            progress += Time.deltaTime;
            if (progress >= recipes[recipeIndex].craftingTime) {
                CompleteRecipe();
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
            if (recipeIndex >= 0)
                progressBar.fillAmount = progress / recipes[recipeIndex].craftingTime;
            else if (progressBar.fillAmount != 0)
                progressBar.fillAmount = 0;
        }
    }

    #region Crafting
    public bool Craft() {
        if (!readyForCrafting)
            crafting = false;
        else
            crafting = true;

        return crafting;
    }

    /*public float StartCrafting() {
        Debug.Log("Beginning to craft " + recipes[recipeIndex].name);
        crafting = true;

        if (animator != null)
            animator.SetBool("On", true);
        return recipes[recipeIndex].craftingTime;
    }*/

    public void CompleteRecipe() {

        for (int i = 0; i < recipes[recipeIndex].outputs.Length; i++) {
            if (output[i].item == null) {
                // Make a new one
                WorldItem outputItem = Instantiate(recipes[recipeIndex].outputs[i].GetItemData().worldItemPrefab).GetComponent<WorldItem>();
                outputItem.transform.SetParent(output[i].location, true);
                outputItem.transform.localPosition = Vector3.zero;
                outputItem.InitialiseWorldItem(recipes[recipeIndex].outputs[i], input[i].item.GetItem().GetColor());
                outputItem.GetComponent<BoxCollider2D>().enabled = false;
                output[i].item = outputItem;
            } else {
                // Add to existing
                output[i].item.GiveItem(recipes[recipeIndex].outputs[i]);
            }
        }

        for (int i = 0; i < inputIndicies.Length; i++) {
            for (int j = 0; j < recipes[recipeIndex].inputs.Length; j++) {
                if (input[inputIndicies[i]].item.GetItem().GetItemData() == recipes[recipeIndex].inputs[j].GetItemData()) {
                    input[inputIndicies[i]].item.TakeItem(recipes[recipeIndex].inputs[j].GetItemQuantity());
                }
            }
        }

        Debug.Log("Created " + recipes[recipeIndex].name);
        CancelCrafting();
        
        CheckIfReadyToCraft();
    }

    public void CancelCrafting() {
        crafting = false;
        progress = 0f;
        if (animator != null)
            animator.SetBool("On", false);
    }
    #endregion

    #region IOManager
    public enum SlotType { input, output };
    public WorldItem AddToSlot(WorldItem newItem, SlotType slotType, int index) {

        InvSlot invSlot = (slotType == SlotType.input ? input : output)[index];
        if (invSlot.item == null) {
            // Slap all of it down
            (slotType == SlotType.input ? input : output)[index].item = newItem;
            newItem.GetComponent<BoxCollider2D>().enabled = false;
            newItem.transform.SetParent(invSlot.location, true);
            newItem.transform.localPosition = Vector3.zero;
            CheckIfReadyToCraft();
            return null;
        } else if (invSlot.item.GetItem().GetItemData() == newItem.GetItem().GetItemData()) {
            // Slap as much as you can down and return the rest
            int spaceLeft = invSlot.item.GetItem().GetItemData().stackCap - invSlot.item.GetItem().GetItemQuantity();
            invSlot.item.GiveItem(newItem.TakeItem(Mathf.Min(spaceLeft, newItem.GetItem().GetItemQuantity())));
        } else {
            Debug.LogWarning("Input already has " + invSlot.item.GetItem().GetItemData().name + " in this space");
        }

        CheckIfReadyToCraft();
        return newItem;
    }

    public WorldItem RemoveFromSlot(SlotType slotType, int index, WorldItem currentItems = null) {

        WorldItem removedItem = null;
        WorldItem slotItem = (slotType == SlotType.input ? input : output)[index].item;
        if (slotItem == null) {
            return null;
        }

        if (currentItems == null) {
            removedItem = slotItem;
            (slotType == SlotType.input ? input : output)[index].item = null;
        } else if (currentItems.GetItem().GetItemData() == slotItem.GetItem().GetItemData()) {
            int spaceLeft = currentItems.GetItem().GetItemData().stackCap - currentItems.GetItem().GetItemQuantity();
            currentItems.GiveItem(slotItem.TakeItem(spaceLeft, true));
        }

        CheckIfReadyToCraft();
        return removedItem;
    }
    #endregion

    #region Visual Aids
    public void CreateOutputDisplay() {
        ClearOutputDisplay();

        outputDisplays = new GameObject[recipes[recipeIndex].outputs.Length];
        for (int i = 0; i < recipes[recipeIndex].outputs.Length; i++) {
            WorldItem outputItem = Instantiate(recipes[recipeIndex].outputs[i].GetItemData().worldItemPrefab).GetComponent<WorldItem>();
            outputItem.transform.SetParent(output[i].location, true);
            outputItem.transform.localPosition = Vector3.zero;
            outputItem.InitialiseWorldItem(recipes[recipeIndex].outputs[i]);
            outputItem.GetComponent<BoxCollider2D>().enabled = false;
            outputDisplays[i] = outputItem.gameObject;
            SpriteRenderer[] renderers = outputItem.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in renderers) {
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0.5f);
                string[] layerName = renderer.sortingLayerName.Split(" ");
                layerName[1] = (int.Parse(layerName[1]) - 1).ToString();
                renderer.sortingLayerName = layerName[0] + " " + layerName[1];
            }
        }
    }

    public void ClearOutputDisplay() {
        for (int i = outputDisplays.Length - 1; i >= 0; i--) {
            Destroy(outputDisplays[i]);
        }
    }

    public void InitialiseRecipeView() {
        for (int i = 0; i < recipesViewer.transform.childCount; i++) {
            if (i >= recipes.Length) {
                recipesViewer.transform.GetChild(i).gameObject.SetActive(false);
                continue;
            } else {
                Transform recipeView = recipesViewer.transform.GetChild(i).transform;
                // Input
                recipeView.GetChild(0).GetComponent<Image>().sprite = recipes[i].inputs[0].GetItemData().icon;
                //recipeView.GetChild(0).GetComponent<Image>().color = recipes[i].inputs[0].GetColor();
                recipeView.GetChild(0).GetChild(0).GetComponentInChildren<TMP_Text>().text = "x" + recipes[i].inputs[0].GetItemQuantity();


                // Output
                recipeView.GetChild(2).GetComponent<Image>().sprite = recipes[i].outputs[0].GetItemData().icon;
                //recipeView.GetChild(2).GetComponent<Image>().color = recipes[i].outputs[0].GetColor();
                recipeView.GetChild(2).GetChild(0).GetComponentInChildren<TMP_Text>().text = "x" + recipes[i].outputs[0].GetItemQuantity();

                recipesViewer.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void ToggleRecipes(bool toggle) {
        recipesViewer.gameObject.SetActive(toggle);
    }
    #endregion

    #region Validation
    public bool CheckIfReadyToCraft() {
        return readyForCrafting = FindValidRecipe() && (recipeIndex < 0 ? false : HasSpaceAtOutput());
    }

    public bool FindValidRecipe() {

        for (int i = 0; i < recipes.Length; i++) {
            int foundInputs = 0;
            int neededInput = recipes[i].inputs.Length;
            int[] recipeInputIndicies = new int[neededInput];
            int currentInputIndex = 0;

            for (int j = 0; j < recipes[i].inputs.Length; j++) {
                for (int k = 0; k < input.Length; k++) {
                    if (input[k].item == null)
                        continue;

                    if (recipes[i].inputs[j].GetItemData() == input[k].item.GetItem().GetItemData()) {
                        if (input[k].item.GetItem().GetItemQuantity() >= recipes[i].inputs[j].GetItemQuantity()) {
                            ++foundInputs;
                            recipeInputIndicies[currentInputIndex] = k;
                            ++currentInputIndex;
                            if (foundInputs == neededInput) {
                                bool recipeChange = recipeIndex != i;
                                recipeIndex = i;
                                inputIndicies = recipeInputIndicies;

                                if (recipeChange) {
                                    progress = 0;
                                    CreateOutputDisplay();
                                }
                                return true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        // Could not find a valid recipe given the input
        recipeIndex = -1;
        inputIndicies = new int[0];
        CancelCrafting();
        ClearOutputDisplay();
        return false;
    }

    public bool HasSpaceAtOutput() {
        int validSpaces = 0;
        // There is no recipe so who cares if the output is valid
        if (recipeIndex < 0) {
            crafting = false;
            progress = 0;
            return false;
        }

        for (int i = 0; i < recipes[recipeIndex].outputs.Length; i++) {
            for (int j = 0; j < output.Length; j++) {
                if (output[j].item == null) {
                    // Space is empty so go for it
                    ++validSpaces;
                    break;
                } else if (recipes[recipeIndex].outputs[i].GetItemData() == output[j].item.GetItem().GetItemData()) {
                    if (output[j].item.GetItem().GetItemData().stackCap - output[j].item.GetItem().GetItemQuantity() >= recipes[recipeIndex].outputs[i].GetItemQuantity()) {
                        // We have enough space here
                        ++validSpaces;
                        break;
                    } else {
                        // Not enough room
                        CancelCrafting();
                        return false;
                    }
                }
            }
        }

        if (validSpaces == recipes[recipeIndex].outputs.Length) {
            return true;
        } else {
            // None of the spaces were valid
            CancelCrafting();
            return false;
        }
    }
    #endregion

    [Serializable]
    public struct InvSlot {
        public WorldItem item;
        public Transform location;
    }
}