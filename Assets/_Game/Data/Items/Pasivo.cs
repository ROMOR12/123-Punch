using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Pasivo")]
public class Pasivo : ItemBase
{
    public List<StatModifier> bonificaciones;

    public void Equipar(CombatController stats)
    {
        foreach (var bono in bonificaciones)
        {
            // 'false' significa que no es un consumible, es item pasivo
            stats.AplicarCambioEstadistica(bono.statType, bono.amount, false);
        }
    }
}
