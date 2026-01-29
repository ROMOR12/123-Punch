using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Items/Pasivo")]
public class Pasivo : ItemBase
{
    public List<StatModifier> bonificaciones;

    public void Equipar(CharacterStats stats)
    {
        foreach (var bono in bonificaciones)
        {
            stats.AplicarModificador(bono.statType, bono.amount, false);
        }
    }
}
