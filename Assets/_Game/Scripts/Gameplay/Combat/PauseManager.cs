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
        SoundManager.PlaySound(SoundType.StrongHit);
        SoundManager.PauseMusic();
        // activar el menu de pausa y desactivar el boton de pausa
        menuPausaUI.SetActive(true);
        pauseButom.SetActive(false);
        Time.timeScale = 0f; // pausar el tiempo del juego
        JuegoPausado = true;
    }
    public void Reanudar()
    {
        SoundManager.ResumeMusic();
        // desactivar el menu de pausa y activar el boton de pausa
        menuPausaUI.SetActive(false);
        pauseButom.SetActive(true);
        Time.timeScale = 1f; // reanudar el tiempo del juego
        JuegoPausado = false;
    }
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        JuegoPausado = false;

        // Buscamos todos los objetos que tengan el combatController y los guardamos en un array
        CombatController[] scriptsJugador = FindObjectsByType<CombatController>(FindObjectsSortMode.None);

        // Recorremos el array para borrar todos los objetos
        foreach (CombatController script in scriptsJugador)
        {
            // Borramos los objetos para que no vaya entre escenas
            DestroyImmediate(script.gameObject);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void CerrarJuego()
    {
        Time.timeScale = 1f; // reanudar el tiempo del juego

        // Buscamos todos los objetos que tengan el combatController y los guardamos en un array
        CombatController[] scriptsJugador = FindObjectsByType<CombatController>(FindObjectsSortMode.None);

        // Recorremos el array para borrar todos los objetos
        foreach (CombatController script in scriptsJugador)
        {
            // Borramos los objetos para que no vaya entre escenas
            DestroyImmediate(script.gameObject);
        }
            SceneManager.LoadScene("Menu");
    }
}
