using UnityEngine;

public class ControladorEscenas : MonoBehaviour
{
    public void IrAEscenaCajas()
    {
        // Dejamos que el LootBoxManager dentro de la escena gestione las cantidades y compras
        CargaEscena.Cargar("LootBoxScene");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
