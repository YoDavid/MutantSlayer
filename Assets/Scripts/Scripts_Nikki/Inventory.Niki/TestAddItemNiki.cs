using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAddItemNiki : MonoBehaviour
{
    public InventotyManager inventotyManager;
    public ItemNiki[] itemToPickUp;

    public void PickUpItem(int id)
    {
        bool result = inventotyManager.AddItem(itemToPickUp[id]);
        if (result == true)
        {
            print("Item add");
        }
        else
        {
            print("Item Not Add");
        }
    }

    public void GetSelectedItem()
    {
        ItemNiki receivedItem = inventotyManager.GetSelectedItem(false);
        if (receivedItem != null)
        {
            print("Recive Item" + receivedItem);
        }
        else { print("No Item recived"); }
    }
    public void UseGetSelectedItem()
    {
        ItemNiki receivedItem = inventotyManager.GetSelectedItem(true);
        if (receivedItem != null)
        {
            print("USE Item" + receivedItem);
        }
        else { print("No item useed"); }
    }
}
