using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Consumible")]
public class Consumible : ItemBase
{
    public List<StatModifier> recuperacion;

    public void Usar(CharacterStats stats)
    {
        foreach (var efecto in recuperacion)
        {
            stats.AplicarModificador(efecto.statType, efecto.amount, true);
        }
        Debug.Log($"Usado consumible: {itemBaseName}");
    }
}
