using UnityEngine;

public class ItemBase : ScriptableObject
{
    public int id;
    public string itemBaseName;

    [Header("Item Stats")]
    public string description;
    public Sprite icon;
    public bool isSkin;
}
