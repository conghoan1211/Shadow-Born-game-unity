using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UI : MonoBehaviour
{
    public static UI instance;
    public TextMeshProUGUI dragonballText;
    public TextMeshProUGUI CoinText;

    public TextMeshProUGUI dragonballTextFinish;
    public TextMeshProUGUI CoinTextFinish;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (dragonballText == null)
            dragonballText = GameObject.Find("DragonballText").GetComponent<TextMeshProUGUI>();
        if (CoinText == null)
            CoinText = GameObject.Find("CoinText").GetComponent<TextMeshProUGUI>();
        UpdateInventoryUI("Dragonball", 0);
        UpdateInventoryUI("Coin", 0);
    }
    private void Update()
    {
        SetScoreFinish();
    }
    public void UpdateInventoryUI(string itemName, int quantity)
    {
        if (itemName == "Dragonball")  
            dragonballText.text = $"x{quantity}";
        else if (itemName == "Coin") 
            CoinText.text = $"x{quantity}";
    }

    public void SetScoreFinish()
    {
        dragonballTextFinish.text = "x"+ Inventory.instance.GetItemQuantity("Dragonball").ToString();
        CoinTextFinish.text = "x"+ Inventory.instance.GetItemQuantity("Coin").ToString();
    }
}
