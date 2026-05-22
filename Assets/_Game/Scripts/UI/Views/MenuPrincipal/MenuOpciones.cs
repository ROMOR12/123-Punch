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
    public GameObject PanelLogros;


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

    public void MostrarPanelLogros()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        PanelLogros.SetActive(true);
        PanelOpciones.SetActive(false);
    }

    public void CerrarPanelLogros()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        PanelLogros.SetActive(false);
        PanelOpciones.SetActive(true);
    }

    public void CerrarJuego()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        Application.Quit();
    }
    public void CerrarSesion()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        
        if (Firebase.Auth.FirebaseAuth.DefaultInstance != null)
        {
            Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
        }

        if (SessionManager.shared != null)
        {
            SessionManager.shared.LimpiarSesion();
        }

        CargaEscena.Cargar("LoginScene");
    }
}
