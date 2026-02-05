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
            playerCombat.AplicarCambioEstadistica(efecto.statType, efecto.amount, true);
        }
        Debug.Log($"Usando objeto: {itemBaseName}");
    }
}
