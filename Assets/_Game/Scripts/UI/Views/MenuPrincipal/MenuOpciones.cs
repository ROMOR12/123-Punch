using UnityEngine;

public class MenuOpciones : MonoBehaviour
{
    [Header("Referencias UI")]
    public GameObject BotonOpciones;
    public GameObject PanelOpciones;
    public GameObject FadePanel;
    public GameObject BotonReanudar;
    public GameObject BotonCerrarJuego;
    public GameObject BotonMenu;


    public void MostrarPanelMenu()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        PanelOpciones.SetActive(true);
        BotonMenu.SetActive(false);
        FadePanel.SetActive(true);
    }
    public void CerrarPanelMenu()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        PanelOpciones.SetActive(false);
        BotonMenu.SetActive(true);
        FadePanel.SetActive(false);
    }
    public void CerrarJuego()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        Application.Quit();
    }
    public void CerrarSesion()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        
        // Cerramos sesión en Firebase
        if (Firebase.Auth.FirebaseAuth.DefaultInstance != null)
        {
            Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        }

        // Limpiamos los datos del jugador de la memoria
        if (SessionManager.shared != null)
        {
            SessionManager.shared.LimpiarSesion();
        }

        // Cargamos la escena de Login (usando tu script CargaEscena)
        CargaEscena.Cargar("LoginScene");
    }
}
