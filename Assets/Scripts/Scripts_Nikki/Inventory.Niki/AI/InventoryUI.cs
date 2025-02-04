using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// InventoryUI.cs
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory inventory; // הפניה לסקריפט Inventory
    public Text StoneHealMini; // טקסט עבור אבן חיים
    public Image StoneHealMiniImage;
    public Text StonePowerUp; // טקסט עבור אבן כוח
    public Image StonePowerUPImage;

    void Start()
    {
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<Inventory>();
        // מציאת רכיבי הטקסט ב-UI
        StoneHealMini = GameObject.Find("StoneHealMiniText").GetComponent<Text>();
        StoneHealMiniImage = GameObject.Find("StoneHealMiniImage").GetComponent <Image>();
        StonePowerUp = GameObject.Find("StonePowerUpText").GetComponent<Text>();
        StonePowerUPImage = GameObject.Find("StonePowerUPImage").GetComponent<Image>();

    }

    public void UpdateInventoryUI()
    {
        if (inventory.HasItem("Potion"))
        {
            StoneHealMiniImage.sprite = Resources.Load<Sprite>("Path/To/Potion/Image"); // טען את התמונה
            StoneHealMini.text = inventory.GetItemQuantity("Potion").ToString();
        }
        else
        {
            StoneHealMiniImage.sprite = null; // הסר את התמונה
            StoneHealMini.text = "0";
        }

        if (inventory.HasItem("Sword"))
        {
            StonePowerUPImage.sprite = Resources.Load<Sprite>("Path/To/Sword/Image"); // טען את התמונה
            StonePowerUp.text = inventory.GetItemQuantity("Sword").ToString();
        }
        else
        {
            StonePowerUPImage.sprite = null; // הסר את התמונה
            StonePowerUp.text = "0";
        }

        // ... (עדכון פריטים נוספים)
    }
}
/*
// Inventory.cs
public void AddItem(string itemName, int itemQuantity)
{
    // ... (קוד קודם)

    // עדכון ה-UI
    InventoryUI inventoryUI = GameObject.Find("Canvas").GetComponent<InventoryUI>();
    inventoryUI.UpdateInventoryUI();
}

public void RemoveItem(string itemName, int itemQuantity)
{
    // ... (קוד קודם)

    // עדכון ה-UI
    InventoryUI inventoryUI = GameObject.Find("Canvas").GetComponent<InventoryUI>();
    inventoryUI.UpdateInventoryUI();
}
*/