using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

// script para gestionar la pausa del juego
public class PauseManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject menuPausaUI;
    public GameObject pauseButom;

    public static bool JuegoPausado = false;

    public void Pausar()
    {
        // activar el menu de pausa y desactivar el boton de pausa
        menuPausaUI.SetActive(true);
        pauseButom.SetActive(false);
        Time.timeScale = 0f; // pausar el tiempo del juego
        JuegoPausado = true;
    }
    public void Reanudar()
    {
        // desactivar el menu de pausa y activar el boton de pausa
        menuPausaUI.SetActive(false);
        pauseButom.SetActive(true);
        Time.timeScale = 1f; // reanudar el tiempo del juego
        JuegoPausado = false;
    }
    public void ReiniciarNivel()
    {
        // reiniciar el nivel actual
        Time.timeScale = 1f;
        JuegoPausado = false;

        // llamamos a la escena actual para reiniciarla
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void CerrarJuego()
    {
        Time.timeScale = 1f; // reanudar el tiempo del juego
        // Cerramos la aplicación, por ahora, aqui hay que llamar a SceneManager para ir al menu principal
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();

            SceneManager.LoadScene("Assets/_Game/Scenes/Menu/LoginScene.unity");
        }
    }
}
