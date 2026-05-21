using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectorNivel : MonoBehaviour
{
    public void CargarNivel(string nombreEscena)
    {
        CargaEscena.Cargar(nombreEscena);
    }
}
