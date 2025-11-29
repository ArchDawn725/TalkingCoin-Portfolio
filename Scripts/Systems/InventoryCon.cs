using RootMotion.FinalIK;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEditor.Progress;

public class InventoryCon : MonoBehaviour
{
    private Controller con;
    public readonly List<ItemData> MyItems = new List<ItemData>();
    private Transform myItemLocations;
    private Transform myItemsHolder;
    public Transform itemSpawnLocation;
    public Transform spawnedItemsHolder;
    public readonly List<ItemData> CartItems = new List<ItemData>();
    public int Gold {  get; private set; }

    public InventoryCon(Controller controller, Transform itemLocations, Transform itemsHolder)
    {
        this.con = controller;
        this.myItemLocations = itemLocations;
        this.myItemsHolder = itemsHolder;
    }
    public void AddItem(string name, int amount = 1) //to me
    {
        foreach (var itemSO in con.allItemsSO)
        {
            if (itemSO.itemName == name)
            {
                MyItems.Add(new ItemData(itemSO.itemName, itemSO.price, itemSO.description, amount));
                ItemBought(itemSO.itemName);
                return;
            }
        }
        Debug.LogWarning($"Item '{name}' not found in allItemsSO.");
    }

    public void RemoveItem(string name, int amountToRemove, CharacterSO character) //from me
    {
        for (int i = 0; i < MyItems.Count; i++)
        {
            if (MyItems[i].itemName == name)
            {
                MyItems[i].amount -= amountToRemove;
                if (MyItems[i].amount <= 0)
                {
                    MyItems.RemoveAt(i);
                    Debug.Log($"{name} removed from the inventory.");
                }
                else
                {
                    Debug.Log($"{name} reduced to {MyItems[i].amount}.");
                }
                return;
            }
        }
        ItemSold(name, character);
        Debug.LogWarning($"Item '{name}' not found in the inventory.");
    }
    public InteractionObject FindItem(string name)
    {
        // Loop through the children of spawnedItemsHolder to find the item
        for (int i = 0; i < myItemsHolder.childCount; i++)
        {
            Transform child = myItemsHolder.GetChild(i);
            InteractionObject interactionObject = child.GetComponent<InteractionObject>();

            if (interactionObject != null && interactionObject.itemName == name)
            {
                return interactionObject;
            }
        }

        Debug.LogWarning($"Item '{name}' not found in spawned items.");
        return null;
    }

    //spawn items on cart
    public List<ItemData> GenerateAIItems(int amount)
    {
        ClearItems();
        List<ItemData> generatedItems = new List<ItemData>();
        for (int i = 0; i < amount; i++)
        {
            int index = Random.Range(0, con.allItemsSO.Count);
            Instantiate(con.allItemsSO[index].prefab, itemSpawnLocation.GetChild(i).position, Quaternion.identity, spawnedItemsHolder);
            generatedItems.Add(new ItemData(con.allItemsSO[index].itemName, con.allItemsSO[index].price, con.allItemsSO[index].description, 1));
            CartItems.Add(new ItemData(con.allItemsSO[index].itemName, con.allItemsSO[index].price, con.allItemsSO[index].description, 1));
            Debug.Log(con.allItemsSO[index].itemName);
        }
        return generatedItems;
    }
    public void GenerateMyItems(List<string> itemsList)
    {
        for (int i = 0; i < itemsList.Count; i++)
        {
            foreach (var itemSO in con.allItemsSO)
            {
                if (itemSO.itemName == itemsList[i])
                {
                    Instantiate(itemSO.prefab, myItemLocations.GetChild(i).position, Quaternion.identity, myItemsHolder);
                    break;
                }
            }
        }
    }
    public string GetItems(List<ItemData> theseItems, bool buyer)
    {
        StringBuilder sb = new StringBuilder();



        foreach (ItemData item in theseItems)
        {
            string salePrice = "";
            if (buyer) { salePrice = item.price.ToString(); }
            else { salePrice = GetItemPrice(item.price).ToString(); }

            sb.AppendFormat("{0} - Price: {1}, Description: {2}, Amount: {3}\n",
                            item.itemName,
                            salePrice,
                            item.Description,
                            item.amount);
        }

        return sb.ToString();
    }
    private int GetItemPrice(int price)
    {
        return price + Random.Range(-price / 2, price);
    }
    public void GoldChange(int value)
    {
        Gold += value;
        UIController.Instance.UpdateGold(Gold);
    }

    private void ClearItems()
    {
        CartItems.Clear();

        if (spawnedItemsHolder.childCount > 0)
        {
            //destroy all cart items
            for (int i = spawnedItemsHolder.childCount - 1; i >= 0; i--)
            {
                Destroy(spawnedItemsHolder.GetChild(i).gameObject);
            }
        }
    }

    private void ItemBought(string itemName)
    {
        //if artifact, win objective
        if (string.Compare(itemName, "artifact") == 0)
        {
            if (con.sideQuest == Controller.SideQuests.The_Artifact) { con.CompletedSideObjective(); }
        }
    }
    private void ItemSold(string itemName, CharacterSO character)
    {
        if (string.Compare(itemName, "poison") == 0)
        {
            con.characters.Remove(character);
            if (string.Compare(character.occupation, "King") == 0)
            {
                if (con.sideQuest == Controller.SideQuests.Kill_The_King) { con.CompletedSideObjective(); }
            }
        }
    }

    [System.Serializable]
    public class ItemData
    {
        public int price;
        public int amount;
        public string itemName;
        public string Description;

        public ItemData(string name, int price, string description, int amount)
        {
            this.itemName = name;
            this.price = price;
            this.Description = description;
            this.amount = amount;
        }
    }
}
