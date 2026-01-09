using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Consumible")]
public class Consumible : ItemBase
{
    public List<StatModifier> recuperacion;

    public void Usar(CombatController playerCombat)
    {
        foreach (var efecto in recuperacion)
        {
            //playerCombat.AplicarConsumible(efecto.statType, efecto.amount, true);
        }
        Debug.Log($"Usado consumible: {itemBaseName}");
    }
}
