using UnityEngine;

public class ControladorEscenas : MonoBehaviour
{
    public void IrAEscenaCajas()
    {
        if(GameManager.Instance.numCajas >= 1)
        {
            GameManager.Instance.numCajas--;
            CargaEscena.Cargar("LootBoxScene");
        }
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
