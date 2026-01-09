using UnityEngine;

public class ItemBase : ScriptableObject
{
    public int id;
    public string itemBaseName;

    [Header("Item Stats")]
    [TextArea(2, 5)]
    public string description;
    
    public Sprite icon;
    public bool isSkin;

    [Header("Rareza")]
    public ItemRarity rarity;
}
