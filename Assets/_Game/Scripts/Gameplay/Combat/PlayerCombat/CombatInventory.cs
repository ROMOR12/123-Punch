using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatInventory : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image imagenBotonConsumible;
    public GameObject flechaConsumible;

    [Header("Datos de Inventario")]
    private List<Consumible> mochilaConsumibles = new List<Consumible>();
    private CombatController playerCombat; // Para decirle a quién aplicar el efecto

    public void Inicializar(CombatController player, List<Consumible> itemsIniciales)
    {
        playerCombat = player;
        mochilaConsumibles.Clear();

        if (itemsIniciales != null)
        {
            foreach (var item in itemsIniciales)
            {
                if (item != null) mochilaConsumibles.Add(item);
            }
        }
        ActualizarInterfazObjeto();
    }

    public void UsarPrimerConsumible()
    {
        // Validación de estado: Si el jugador no puede actuar, no usar el item.
        if (playerCombat.IsUnableToAct()) return;

        if (mochilaConsumibles.Count > 0)
        {
            Consumible item = mochilaConsumibles[0];
            item.Usar(playerCombat);
            SoundManager.PlaySound(SoundType.Consumable);

            mochilaConsumibles.RemoveAt(0);
            ActualizarInterfazObjeto();
        }
    }

    public void SiguienteConsumible()
    {
        if (mochilaConsumibles.Count <= 1) return;

        Consumible objetoActual = mochilaConsumibles[0];
        mochilaConsumibles.RemoveAt(0);
        mochilaConsumibles.Add(objetoActual);

        ActualizarInterfazObjeto();
    }

    private void ActualizarInterfazObjeto()
    {
        if (imagenBotonConsumible == null) return;

        if (mochilaConsumibles != null && mochilaConsumibles.Count > 0)
        {
            Consumible primerItem = mochilaConsumibles[0];
            if (primerItem != null && primerItem.icon != null)
            {
                imagenBotonConsumible.sprite = primerItem.icon;
                imagenBotonConsumible.enabled = true;
            }
            else
            {
                imagenBotonConsumible.enabled = false;
            }
        }
        else
        {
            imagenBotonConsumible.enabled = false;
        }

        if (flechaConsumible != null)
        {
            flechaConsumible.SetActive(mochilaConsumibles != null && mochilaConsumibles.Count > 1);
        }
    }
}
