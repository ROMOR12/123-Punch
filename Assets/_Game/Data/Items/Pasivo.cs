using UnityEngine;
using System.Collections.Generic;

// Objetos equipables que dan ventajas permanentes durante toda la partida
[CreateAssetMenu(menuName = "Items/Pasivo")]
public class Pasivo : ItemBase
{
    public List<StatModifier> bonificaciones;

    // Se ejecuta al equipar: Aplica mejoras a las estadísticas base
    public void Equipar(CombatController stats)
    {
        foreach (var bono in bonificaciones)
        {
            // 'false': indica que es una mejora pasiva de la estadística, no una bonificación de un consumible.
            stats.AplicarCambioEstadistica(bono.statType, bono.amount, false);
        }
    }
}
