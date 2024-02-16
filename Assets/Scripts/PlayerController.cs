using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float walkSpeed = 100f;
    [SerializeField]
    private bool carrying;
    [SerializeField]
    private bool working;
    [SerializeField]
    private bool running;

    [SerializeField]
    private Vector2 movement = Vector2.zero;

    [SerializeField]
    private List<WorldItem> interactableItems = new List<WorldItem>();
    [SerializeField]
    private List<GameObject> interactableObjects = new List<GameObject>();
    [SerializeField]
    private WorldItem myItems = null;

    Rigidbody2D rb;
    Animator animator;

    private float keyPressDelay;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKey(KeyCode.R) && !carrying)
            ActivateButton();
        else {
            working = false;
            if (animator.GetBool("Working"))
                animator.SetBool("Working", false);
        }


        if (working) {
            movement = Vector2.zero;
            return;
        } else if (Input.GetKeyDown(KeyCode.E))
            Grab();

        // Movement
        float horizontal = Input.GetAxis("Horizontal");
        animator.SetFloat("x", horizontal);
        float vertical = Input.GetAxis("Vertical");
        animator.SetFloat("y", vertical);
        running = Input.GetKey(KeyCode.LeftShift);
        animator.SetFloat("Running", running ? 2 : 1);
        if (horizontal > 0.1f || vertical > 0.1f || horizontal < -0.1f || vertical < -0.1f) {
            movement.x = horizontal > 0.1f || horizontal < -0.1f ? walkSpeed * (horizontal < -0.1f ? -1 : 1) : 0;
            movement.y = vertical > 0.1f || vertical < -0.1f ? walkSpeed * (vertical < -0.1f ? -1 : 1) : 0;
            if (running) {
                movement *= 2;
            }

            //rb.velocity = movement;
            if (horizontal > 0.1f)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (horizontal < -0.1f)
                transform.localScale = new Vector3(1, 1, 1);
        } else {
            movement = Vector2.zero;
        }
    }

    private void FixedUpdate() {
        rb.velocity = movement * Time.deltaTime;
    }

    public void ActivateButton() {
        if (!carrying) {
            if (interactableObjects.Count > 0) {
                bool successful = false;
                switch (interactableObjects[interactableObjects.Count - 1].tag.Split("/")[1]) {
                    case "AssemblyPlatform":
                        successful = interactableObjects[interactableObjects.Count - 1].GetComponent<AssemblyPlatform>().Craft();
                        if (successful) {
                            if (animator != null) {
                                if (!working)
                                    animator.SetTrigger("Start Working");
                                if (!animator.GetBool("Working"))
                                    animator.SetBool("Working", true);
                            }

                            working = true;
                        } else {
                            working = false;
                            if (animator.GetBool("Working"))
                                animator.SetBool("Working", false);
                        }
                        break;
                    case "Customer":
                        interactableObjects[interactableObjects.Count - 1].GetComponent<Customer>().StartQuest();
                        break;
                    case "Input":
                    case "Output":
                        break;
                    case "Workbench":
                        successful = interactableObjects[interactableObjects.Count - 1].GetComponent<WorkTable>().Craft();
                        if (successful) {
                            if (animator != null) {
                                if (!working)
                                    animator.SetTrigger("Start Working");
                                if (!animator.GetBool("Working"))
                                    animator.SetBool("Working", true);
                            }

                            working = true;
                        } else {
                            working = false;
                            if (animator.GetBool("Working"))
                                animator.SetBool("Working", false);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void Grab() {
        if (carrying) {
            // Drop what we are holding, or pick up more if there is more
            if (interactableObjects.Count > 0) {
                // Drop items on work object
                WorldItem excessItems;
                switch (interactableObjects[interactableObjects.Count - 1].tag.Split('/')[1]) {
                    case "Input":
                        // Add held item to input and get given back the excess
                        excessItems = interactableObjects[interactableObjects.Count - 1].GetComponent<IOPort>().AddToSlot(myItems);
                        if (excessItems == null) {
                            myItems = null;
                            carrying = false;
                        }
                        break;
                    case "Output":
                        // Adding the item in the output to the item we are holding if it is matching
                        interactableObjects[interactableObjects.Count - 1].GetComponent<IOPort>().RemoveFromSlot(myItems);
                        break;
                    case "AssemblyPlatform":
                        excessItems = interactableObjects[interactableObjects.Count - 1].GetComponent<AssemblyPlatform>().AddItem(myItems);
                        if (excessItems == null) {
                            myItems = null;
                            carrying = false;
                        }
                        break;
                    case "Customer":
                        bool giveSuccess = interactableObjects[interactableObjects.Count - 1].GetComponent<Customer>().GiveItem(myItems);
                        if (giveSuccess) {
                            myItems = null;
                            carrying = false;
                        }
                        break;
                    case "Workbench":
                        break;
                    default:
                        break;
                }
            } else if (interactableItems.Count > 0 && myItems.GetItem().GetItemQuantity() < myItems.GetItem().GetItemData().stackCap) {
                bool foundStack = false;
                int desiredAmount = 1;
                // Hold shift to pick up stack (maybe inverse this, i.e. hold shift to grab 1
                if (Input.GetKey(KeyCode.LeftShift))
                    desiredAmount = interactableItems[interactableItems.Count - 1].GetItem().GetItemQuantity();
                // Check if we want to pick up a matching item from a near by stack
                for (int i = interactableItems.Count - 1; i >= 0; i--) {
                    if (interactableItems[i].GetItem().GetItemData() == myItems.GetItem().GetItemData()) {
                        if (interactableItems[i].GetItem().GetItemQuantity() < myItems.GetItem().GetItemData().stackCap) {
                            foundStack = true;
                            Items takenItem = interactableItems[i].TakeItem(desiredAmount);
                            int excess = myItems.GiveItem(takenItem);
                            interactableItems[i].GiveItem(new Items(takenItem.GetItemData(), excess));
                            Debug.Log("Picking up more " + myItems.GetItem().GetItemData().name);
                            break;
                        }
                    }
                }

                if (!foundStack) {
                    DropStack();
                }
            } else {
                DropStack();
            }

            void DropStack() {
                // Then drop the whole stack on the floor
                Debug.Log("Dropping " + myItems.GetItem().GetItemData().name + " x" + myItems.GetItem().GetItemQuantity());
                myItems.transform.SetParent(null, true);
                myItems.transform.position = transform.position - new Vector3(0, 1f, 0);
                myItems.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                myItems = null;
                carrying = false;
            }
        } else {
            // Not carrying anything
            if (interactableObjects.Count > 0) {
                // Interact with work object
                if (interactableObjects[interactableObjects.Count - 1].tag.Contains("Input") || interactableObjects[interactableObjects.Count - 1].tag.Contains("Output")) {
                    myItems = interactableObjects[interactableObjects.Count - 1].GetComponent<IOPort>().RemoveFromSlot();
                    if (myItems != null ) {
                        myItems.transform.SetParent(transform, true);
                        myItems.transform.localPosition = new Vector3(0, 0.1f, 0);
                        carrying = true;
                    }
                } else if (interactableObjects[interactableObjects.Count - 1].tag.Contains("AssemblyPlatform")) {
                    myItems = interactableObjects[interactableObjects.Count - 1].GetComponent<AssemblyPlatform>().RemoveItem();
                    if (myItems != null) {
                        myItems.transform.SetParent(transform, true);
                        myItems.transform.localPosition = new Vector3(0, 0.1f, 0);
                        carrying = true;
                    }
                } /*else {
                    // Workbench
                    float workTime = interactableObjects[interactableObjects.Count - 1].GetComponent<WorkTable>().StartCrafting();
                    Debug.Log("Working at " + interactableObjects[interactableObjects.Count - 1].name + " for " + workTime + "s");
                    if (workTime > 0)
                        StartCoroutine(Work(workTime));
                }*/
            } else if (!working && interactableItems.Count > 0) {
                // Pick up new item
                if (interactableItems[interactableItems.Count -1].GetItem().GetItemQuantity() == 1 || Input.GetKey(KeyCode.LeftShift)) {
                    // Just take the existing item
                    myItems = interactableItems[interactableItems.Count - 1];
                    myItems.transform.SetParent(transform, true);
                } else {
                    // Create a copy of the item in your hand and take 1 from the other object and give it to me
                    myItems = Instantiate(interactableItems[interactableItems.Count - 1].GetItem().GetItemData().worldItemPrefab, transform).GetComponent<WorldItem>();
                    myItems.InitialiseWorldItem(interactableItems[interactableItems.Count - 1].TakeItem(1), interactableItems[interactableItems.Count - 1].GetItem().GetColor());
                }
                myItems.transform.localPosition = new Vector3(0, 0.1f, 0);
                myItems.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                carrying = true;
            }
        }

        if (myItems != null) {
            if (myItems.GetItem().GetItemQuantity() == 0) {
                Destroy(myItems.gameObject);
                myItems = null;
                carrying = false;
            }
        } else {
            carrying = false;
        }

        animator.SetBool("Carrying", carrying);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        //Debug.Log("Entered " + collision.name + "'s trigger");
        if (collision.GetComponent<WorldItem>() != null)
            interactableItems.Add(collision.GetComponent<WorldItem>());
        else if (collision.tag.Contains("Interactable"))
            interactableObjects.Add(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<WorldItem>() != null)
            interactableItems.Remove(collision.GetComponent<WorldItem>());
        else if (collision.tag.Contains("Interactable")) {
            if (interactableObjects.Contains(collision.gameObject))
                interactableObjects.Remove(collision.gameObject);
        }
    }
}