using UnityEngine;

public class ControladorEscenas : MonoBehaviour
{
    public void IrAEscenaCajas()
    {
        if (SessionManager.shared.currentUser.lootboxes > 0)
        {
            CargaEscena.Cargar("LootBoxScene");
        }
        else
        {
            Debug.Log("No puedes entrar, tienes 0 cajas.");
        }
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
