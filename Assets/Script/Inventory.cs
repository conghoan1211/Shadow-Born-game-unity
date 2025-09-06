using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Inventory : MonoBehaviour
{
    public static Inventory instance; // Singleton để truy cập từ script khác
    public Dictionary<string, int> itemDictionary = new Dictionary<string, int>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }

    public void AddItem(Items item)
    {
        if (itemDictionary.ContainsKey(item.itemName))
        {
            itemDictionary[item.itemName] += item.quantity; // Cộng dồn số lượng
        }
        else
        {
            itemDictionary[item.itemName] = item.quantity; // Thêm vật phẩm mới
        }

        UI.instance.UpdateInventoryUI(item.itemName, itemDictionary[item.itemName]);
    }
    public int GetItemQuantity(string itemName)
    {
        if (itemDictionary.ContainsKey(itemName))
        {
            return itemDictionary[itemName];
        }
        else
        {
            return 0;  
        }
    }
}
