using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour {

    [SerializeField]
    private CombinedItems desiredItems = new CombinedItems();

    [Header("Movement")]
    [SerializeField]
    private Vector3 startPoint;
    [SerializeField]
    private Vector3 endPoint;
    [SerializeField]
    private float enterDst = 2f;
    [SerializeField]
    private float walkSpeed = 1f;
    [SerializeField]
    private int customerState = 0;

    [Header("Waiting")]
    [SerializeField]
    private float preQuestWaitTimer = 30f;

    [Header("Quests")]
    [SerializeField]
    private int questReward;
    [SerializeField]
    private float rewardMarkUpPerc = 0.2f;//10%
    [SerializeField]
    private GameObject questIcon;
    [SerializeField]
    private GameObject questObjective;
    [SerializeField]
    private Image questTimer;
    [SerializeField]
    private Gradient questTimerGradient;
    [SerializeField]
    private float questTime = 60f;
    [SerializeField]
    private GameObject floatyText;

    [SerializeField]
    private bool questStart;
    [SerializeField]
    private float waitTime;

    [Header("ReadOnly")]
    [SerializeField]
    private float waitTimer = 0;

    private void Update() {
        Vector3 movement;
        switch (customerState) {
            case 0:
                movement = (endPoint - transform.position);
                if (movement.sqrMagnitude > walkSpeed * Time.deltaTime)
                    movement = movement.normalized * walkSpeed * Time.deltaTime;
                transform.position += movement;

                if ((endPoint - transform.position).sqrMagnitude <= 0.05f) {
                    customerState = 1;
                    GetComponent<Animator>().SetInteger("State", customerState);
                    waitTimer = 0f;
                    waitTime = preQuestWaitTimer;
                    questTimer.transform.parent.gameObject.SetActive(true);
                }
                break;
            case 1:
                waitTimer += Time.deltaTime;
                questTimer.fillAmount = waitTimer / waitTime;
                questTimer.color = questTimerGradient.Evaluate(waitTimer / waitTime);
                if (waitTimer >= waitTime) {
                    // Customer has waited long enough and is leaving
                    customerState = 2;
                    transform.localScale = new Vector3(-1, 1, 1);
                    GetComponent<Animator>().SetInteger("State", customerState);
                    StartCoroutine(FailedQuest(5f));

                    questIcon.SetActive(false);
                    questObjective.SetActive(false);
                    questTimer.transform.parent.gameObject.SetActive(false);
                }
                break;
            case 2:
                movement = startPoint - transform.position;
                if (movement.sqrMagnitude > walkSpeed * Time.deltaTime)
                    movement = movement.normalized * walkSpeed * Time.deltaTime;
                transform.position += movement;
                break;
            default:
                break;
        }
    }

    public void InitialiseCustomer(Vector3 startPos) {
        startPoint = startPos;
        endPoint = startPoint + new Vector3(enterDst, 0f, 0f);
        transform.position = startPoint;

        GetComponent<Animator>().enabled = true;
    }

    public void StartQuest() {
        if (questStart)
            return;

        Debug.Log("Starting Quest! " + waitTime + "s on the clock!");
        // Get quest objective
        Items[] desires = new Items[3];
        for (int i = 0; i < desires.Length; i++) {
            desires[i] = GameManager.instance.GetItemFromLibrary(i);
        }

        desiredItems.SetItemComponents(desires);
        for (int i = 0; i < desires.Length; i++) {
            if (desires[i].GetItemData() == null) {
                questObjective.transform.GetChild(i).gameObject.SetActive(false);
                continue;
            }

            Image image = questObjective.transform.GetChild(i).GetComponent<Image>();
            image.sprite = desires[i].GetItemData().icon;
            image.color = desires[i].GetColor();
            questObjective.transform.GetChild(i).gameObject.SetActive(true);
        }
        questReward = Mathf.CeilToInt(desiredItems.GetCombinedValue() * (1f + rewardMarkUpPerc));


        questIcon.SetActive(false);
        questObjective.SetActive(true);
        questTimer.transform.parent.gameObject.SetActive(true);
        questTimer.fillAmount = 0;
        questStart = true;
        waitTimer = 0f;
        waitTime = questTime;
    }

    public bool GiveItem(WorldItem item) {
        // Check if this thing matches what we want
        if (item.GetItem().GetItemData() != desiredItems.GetItemData() || customerState > 1 || !questStart)
            return false;   // Custom doesn't want this...

        Items[] recievedItems = item.GetItem().GetItemComponents();
        Items[] desiredItemComponents = desiredItems.GetItemComponents();
        int matchingItems = 0;
        for (int i = 0; i < desiredItemComponents.Length; i++) {
            ItemData desiredItemComponent = desiredItemComponents[i].GetItemData();

            if (desiredItemComponent == null)
                matchingItems += 2; // We wanted nothing here so...
            else if (i > recievedItems.Length)
                continue;
            else {
                if (recievedItems[i].GetItemData() == desiredItemComponent)
                    ++matchingItems; // We got a match!
                if (recievedItems[i].GetColor() == desiredItemComponents[i].GetColor())
                    ++matchingItems;
            }
        }

        // Take item
        item.transform.SetParent(transform, true);
        item.transform.localPosition = Vector3.zero;
        GetComponent<Animator>().SetBool("Carrying", true);

        CompleteQuest(matchingItems / (desiredItemComponents.Length * 2));
        return true;
    }

    private void CompleteQuest(float correctPercentage) {
        Debug.Log("Completed Quest with a " + (correctPercentage * 100f) + "% correctness");
        float reward = Mathf.CeilToInt(desiredItems.GetCombinedValue() * (1f + rewardMarkUpPerc));
        reward = Mathf.FloorToInt(reward * correctPercentage);
        GameManager.instance.Pay((int)reward);
        if ((int)reward == 0)
            Instantiate(floatyText, transform.position, transform.rotation).GetComponent<FloatyText>().Initialise((int)reward);
        
        questStart = false;
        customerState = 2;
        transform.localScale = new Vector3(-1, 1, 1);
        GetComponent<Animator>().SetInteger("State", customerState);

        questIcon.SetActive(false);
        questObjective.SetActive(false);
        questTimer.transform.parent.gameObject.SetActive(false);
        // Walk away
        StartCoroutine(DestroySelf(5f));
    }

    IEnumerator DestroySelf(float killTimer) {
        yield return new WaitForSeconds(killTimer);
        GameManager.instance.UnBirthCustomer(this.gameObject);
    }

    IEnumerator FailedQuest(float killTimer) {
        yield return new WaitForSeconds(killTimer);
        GameManager.instance.EndDay();
    }
}
