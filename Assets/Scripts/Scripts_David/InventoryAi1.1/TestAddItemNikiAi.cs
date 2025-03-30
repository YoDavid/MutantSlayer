using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAddItemNikiAi : MonoBehaviour
{
    public InventoryManagerAi inventotyManagerAi;
    public ItemNikiAi[] itemToPickUp;

    public void PickUpItem(int id)
    {
        // áãé÷ä àí äîæää ú÷éï
        if (id >= 0 && id < itemToPickUp.Length)
        {
            bool result = inventotyManagerAi.AddItemAi(itemToPickUp[id]);
            if (result)
            {
                print("Item added: " + itemToPickUp[id].itemName); // äãôñú ùí äôøéè
            }
            else
            {
                print("Item not added: Inventory full or item not stackable"); // äåãòä éåúø àéğôåøîèéáéú
            }
        }
        else
        {
            print("Invalid item ID");
        }
    }

    public void GetSelectedItem()
    {
        ItemNikiAi receivedItem = inventotyManagerAi.GetSelectedItem(false);
        if (receivedItem != null)
        {
            print("Received Item: " + receivedItem.itemName); // äãôñú ùí äôøéè
        }
        else
        {
            print("No item received");
        }
    }

    public void UseGetSelectedItem()
    {
        ItemNikiAi receivedItem = inventotyManagerAi.GetSelectedItem(true);
        if (receivedItem != null)
        {
            print("Used Item: " + receivedItem.itemName); // äãôñú ùí äôøéè
        }
        else
        {
            print("No item used");
        }
    }
}