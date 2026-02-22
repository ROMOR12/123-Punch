using UnityEngine;
using System.Collections.Generic;

// Objetos de un solo uso en mitad de la partida
[CreateAssetMenu(menuName = "Items/Consumible")]
public class Consumible : ItemBase
{
    public List<StatModifier> recuperacion;

    // Se ejecuta al pulsar el botón: Aplica el efecto de curación instantánea
    public void Usar(CombatController playerCombat)
    {
        foreach (var efecto in recuperacion)
        {
            // 'true': indica que es una obtención de una bonificación de un consumible.
            playerCombat.AplicarCambioEstadistica(efecto.statType, efecto.amount, true);
        }
        Debug.Log($"Usando objeto: {itemBaseName}");
    }
}
