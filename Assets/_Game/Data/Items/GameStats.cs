using UnityEngine;

public enum StatType
{
    Life,
    Energy,
    Force,
    Recovery
}

[System.Serializable]
public struct StatModifier
{
    public StatType statType;
    public int amount;
}
