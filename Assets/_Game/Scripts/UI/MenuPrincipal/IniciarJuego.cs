using UnityEngine;
using UnityEngine.SceneManagement;

public class IniciarJuego : MonoBehaviour
{
    public void CambiarEscena()
    {
        Debug.Log("Boton pulsado por miguel");
        SceneManager.LoadScene(2);
    }
}
