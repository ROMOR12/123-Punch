using UnityEngine;

// Tipos de estadísticas que el sistema de items puede modificar
public enum StatType
{
    Life,
    Energy,
    Force,
    Recovery
}

// Categorías de rareza de los items
public enum ItemRarity
{
    Comun,
    Raro,
    Epico,
    Legendario,
    Ilegal
}

// Estructura auxiliar para agrupar qué estadística cambia y con qué valor
[System.Serializable]
public struct StatModifier
{
    public StatType statType;  // Que estadística de modifica
    public int amount;         // Cantidad que suma o resta
}
