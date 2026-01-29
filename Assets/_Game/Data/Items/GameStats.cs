using UnityEngine;

public enum StatType
{
    Life,
    Energy,
    Force,
    Recovery
}

public enum ItemRarity
{
    Comun,      
    Raro,        
    Epico,        
    Legendario,   
    Ilegal
}

[System.Serializable]
public struct StatModifier
{
    public StatType statType;
    public int amount;
}
