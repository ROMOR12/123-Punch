using UnityEngine;

// Clase base para cualquier objeto del juego. Define la estructura general.
public class ItemBase : ScriptableObject
{
    public int id;                 // Identificador único
    public string itemBaseName;    // Nombre visible en el juego

    [Header("Item Stats")]
    [TextArea(2, 5)]
    public string description;     // Descripción

    public Sprite icon;            // Icono del inventario
    public bool isSkin;            // Define si el objeto es una skin

    [Header("Rareza")]
    public ItemRarity rarity;      // Nivel de rareza
}
