using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    [SerializeField]
    private ItemLibrary[] itemLibraries;

    [Header("UI")]
    [SerializeField]
    private int money = 20;
    [SerializeField]
    private TMP_Text coinBag;
    [SerializeField]
    private TMP_Text customerDisplay;

    [Header("Customers")]
    [SerializeField]
    private GameObject[] customers;
    [SerializeField]
    private List<GameObject> existingCustomers;
    [SerializeField]
    private int customersToday = 0;
    [SerializeField]
    private float nextCustomerInterval = 90f;
    [SerializeField]
    private float nextInterval = 75f;
    [SerializeField]
    private float intervalDecreaseRate = 10f;
    [SerializeField]
    private float xSpawnPosition = 10f;
    [SerializeField]
    private Vector2Int ySpawnRegion = new Vector2Int(-3, 4);

    [Header("Clock")]
    [SerializeField]
    private float dayLength = 300f; // 5minute games? highest Money wins?
    [SerializeField]
    private float dayTime = 0;
    [SerializeField]
    private Slider clock;


    [Header("Game Over")]
    [SerializeField]
    private GameObject gameOverScreen;
    [SerializeField]
    private TMP_Text scoreBoard;

    private void Start() {
        if (instance == null )
            instance = this;

        coinBag.text = "x" + money;
        customerDisplay.text = "";
    }

    private void Update() {
        dayTime += Time.deltaTime;

        if (dayTime > nextCustomerInterval && dayLength - dayTime > 20f)
            BirthCustomer();

        clock.value = Mathf.Min(dayTime / dayLength, 1f);
        if (dayTime > dayLength && existingCustomers.Count <= 0) {
            // Day over
            EndDay();
        }
    }

    public void StartDay() {
        customersToday = 0;
        BirthCustomer();
    }

    public void EndDay() {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    public void BirthCustomer() {
        ++customersToday;
        customerDisplay.text = "x" + customersToday;
        existingCustomers.Add(Instantiate(customers[Random.Range(0, customers.Length)]));
        existingCustomers[existingCustomers.Count - 1].GetComponent<Customer>().InitialiseCustomer(new Vector3(xSpawnPosition, Random.Range((int)ySpawnRegion.x, (int)ySpawnRegion.y + 1)));
        nextCustomerInterval += nextInterval;
        nextInterval -= intervalDecreaseRate;
        if (nextInterval < 15f)
            nextInterval = 15f;
    }

    public void UnBirthCustomer(GameObject outGoingCustomer) {
        existingCustomers.Remove(outGoingCustomer);
        Destroy(outGoingCustomer);
    }

    public Items GetItemFromLibrary(int libraryIndex) {
        int randomInt = Random.Range(0, itemLibraries[libraryIndex].GetLibraryLength());
        Debug.Log(randomInt);
        return itemLibraries[libraryIndex].GetItem(randomInt);
    }

    public void Pay(int payment) {
        money += payment;
        coinBag.text = "x" + money;
    }

    public void Buy(int amount) {
        money -= amount;
        coinBag.text = "x" + money;
    }
}
