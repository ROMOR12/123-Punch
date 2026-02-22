using UnityEngine;

// Este script solo esta para que puedas volver al menu despues de abrir una caja
public class LootBoxManager : MonoBehaviour
{
    public void salirAlMenu()
    {
        CargaEscena.Cargar("Menu");
    }
}
