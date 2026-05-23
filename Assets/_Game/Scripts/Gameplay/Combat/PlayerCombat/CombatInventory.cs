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
    private CombatController playerCombat;

    private void Awake()
    {
        ActualizarInterfazObjeto();
    }

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
        if (playerCombat.IsUnableToAct()) return;

        if (mochilaConsumibles.Count > 0)
        {
            Consumible item = mochilaConsumibles[0];
            item.Usar(playerCombat);
            SoundManager.PlaySound(SoundType.Consumable);

            mochilaConsumibles.RemoveAt(0);
            ActualizarInterfazObjeto();

            ConsumirObjetoDeFirebase(item.id);
        }
    }

    private async void ConsumirObjetoDeFirebase(string itemID)
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            Usuario user = SessionManager.shared.currentUser;
            
            if (user.inventario != null && user.inventario.Contains(itemID))
            {
                user.inventario.Remove(itemID);
                if (GameManager.Instance != null && GameManager.Instance.inventarioIDs != null && GameManager.Instance.inventarioIDs != user.inventario)
                {
                    GameManager.Instance.inventarioIDs.Remove(itemID);
                }
            }

            if (user.inventario == null || !user.inventario.Contains(itemID))
            {
                if (user.objetos_equipados != null)
                {
                    for (int i = 0; i < user.objetos_equipados.Count; i++)
                    {
                        if (user.objetos_equipados[i] == itemID)
                        {
                            user.objetos_equipados[i] = "";
                            if (GameManager.Instance != null && GameManager.Instance.activosEquipadosIDs != null)
                            {
                                GameManager.Instance.activosEquipadosIDs[i] = "";
                            }
                            break;
                        }
                    }
                }
            }

            UsuarioService usuarioService = new UsuarioService();
            await usuarioService.ActualizarUsuario(user);
            Debug.Log($"Objeto {itemID} consumido y actualizado en Firebase.");
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
                gameObject.SetActive(true); // Mostrar el objeto completo
            }
            else
            {
                imagenBotonConsumible.enabled = false;
                gameObject.SetActive(false); // Ocultar el objeto completo
            }
        }
        else
        {
            imagenBotonConsumible.enabled = false;
            gameObject.SetActive(false); // Ocultar el objeto completo
        }

        if (flechaConsumible != null)
        {
            flechaConsumible.SetActive(mochilaConsumibles != null && mochilaConsumibles.Count > 1);
        }
    }
}
