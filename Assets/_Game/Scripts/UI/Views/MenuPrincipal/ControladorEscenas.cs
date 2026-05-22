using UnityEngine;

public class ControladorEscenas : MonoBehaviour
{
    public void IrAEscenaCajas()
    {
        CargaEscena.Cargar("LootBoxScene");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
