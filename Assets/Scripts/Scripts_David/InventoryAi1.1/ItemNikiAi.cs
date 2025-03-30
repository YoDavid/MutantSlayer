using UnityEngine;

// [CreateAssetMenu] יוצר אפשרות ליצור את האובייקט דרך התפריט
[CreateAssetMenu(menuName = "Scriptable object/ItemNikiAi")]
public class ItemNikiAi : ScriptableObject
{
    [Header("Only gameplay")] // כותרת ב-Inspector
    public ItemTypeAi type; // סוג הפריט (enum)
    public ActionTypeAi actionType; // סוג הפעולה (enum)

    [Header("Only UI")] // כותרת ב-Inspector
    public bool stackableAi = true; // האם ניתן לערום את הפריט
    public Sprite image; // תמונת הפריט

    [Header("Item Information")] // כותרת ב-Inspector
    public string itemName; // שם הפריט
    [TextArea(3, 5)] // הוספת שדה טקסט גדול ב-Inspector
    public string description; // תיאור הפריט
}

// רשימת סוגי פריטים אפשריים
public enum ItemTypeAi
{
    HealStoneMini,
    HealStoneMedium,
    HealStoneBig,
    PowerUpStone,
}

// רשימת סוגי פעולות אפשריות
public enum ActionTypeAi
{
    Heal,
    PowerUp,
}