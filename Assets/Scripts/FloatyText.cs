using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatyText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text valueText;
    [SerializeField]
    private float killTimer = 2;
    [SerializeField]
    private Vector3 direction = Vector3.up;
    [SerializeField]
    private float speed = 1f;

    private void Update() {
        killTimer -= Time.deltaTime;
        transform.position += direction * speed * Time.deltaTime;
        if (killTimer < 0)
            Destroy(gameObject);
    }

    public void Initialise(int value) {
        valueText.text = (value > 0 ? "+" : "") + value.ToString();
    }
}
