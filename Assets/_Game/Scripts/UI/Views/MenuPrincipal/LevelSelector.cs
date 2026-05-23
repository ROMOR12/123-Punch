using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    public void CargarNivel(string escena)
    {
        CargaEscena.Cargar(escena);
    }
}
