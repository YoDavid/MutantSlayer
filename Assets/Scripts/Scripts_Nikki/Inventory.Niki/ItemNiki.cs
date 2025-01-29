using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable object/Item")]
public class ItemNiki : ScriptableObject
{
    [Header("Only gameplay")]
    public TileBase Tile;
    public ItemType type;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);

    [Header("Only UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;
}
public enum ItemType
{
    BuildingBlok,
    Tool
}
public enum ActionType
{
    Dig,
    Mine
}
