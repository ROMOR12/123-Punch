using UnityEngine;
using UnityEngine.UI;

// esta clase controla el menu de opciones, la configuracion de volumenes, logros y el control de sesion
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
    public GameObject PanelAudio;
    public Slider sliderMusic;
    public Slider sliderSFX;

    private void Start()
    {
        // esta funcion inicializa el valor de los sliders con el volumen guardado y les asigna sus eventos
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null)
        {
            if (sliderMusic != null) 
            {
                sliderMusic.value = sm.musicVolume;
                sliderMusic.onValueChanged.AddListener(CambiarVolumenMusica);
                AgregarTextoASlider(sliderMusic, "Música");
            }
            if (sliderSFX != null) 
            {
                sliderSFX.value = sm.sfxVolume;
                sliderSFX.onValueChanged.AddListener(CambiarVolumenSFX);
                AgregarTextoASlider(sliderSFX, "SFX");
            }
        }
    }

    private void AgregarTextoASlider(Slider slider, string texto)
    {
        // esta funcion crea un label de texto flotante en la parte superior del slider si no existe ya
        if (slider.transform.Find("Label_Generado") == null)
        {
            GameObject txtObj = new GameObject("Label_Generado");
            txtObj.transform.SetParent(slider.transform, false);
            var rect = txtObj.AddComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 30);
            rect.sizeDelta = new Vector2(200, 30);
            var tmp = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = texto;
            tmp.fontSize = 24;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }

    public void MostrarPanelMenu()
    {
        // esta funcion activa el panel de opciones principal y muestra el fondo de atenuacion
        SoundManager.PlaySound(SoundType.Consumable);
        PanelOpciones.SetActive(true);
        BotonMenu.SetActive(false);
        FadePanel.SetActive(true);
    }

    public void CerrarPanelMenu()
    {
        // esta funcion oculta el panel de opciones principal y restaura el boton del menu
        SoundManager.PlaySound(SoundType.Consumable);
        PanelOpciones.SetActive(false);
        BotonMenu.SetActive(true);
        FadePanel.SetActive(false);
    }

    public void MostrarPanelLogros()
    {
        // esta funcion muestra el panel de logros y oculta el menu de opciones
        SoundManager.PlaySound(SoundType.Consumable);
        PanelLogros.SetActive(true);
        PanelOpciones.SetActive(false);
    }

    public void CerrarPanelLogros()
    {
        // esta funcion oculta el panel de logros y vuelve a mostrar el menu de opciones
        SoundManager.PlaySound(SoundType.Consumable);
        PanelLogros.SetActive(false);
        PanelOpciones.SetActive(true);
    }

    public void MostrarPanelAudio()
    {
        // esta funcion muestra el panel de ajustes de audio y oculta el menu de opciones
        SoundManager.PlaySound(SoundType.Consumable);
        if (PanelAudio != null) PanelAudio.SetActive(true);
        if (PanelOpciones != null) PanelOpciones.SetActive(false);
    }

    public void CerrarPanelAudio()
    {
        // esta funcion oculta el panel de ajustes de audio y vuelve al menu de opciones
        SoundManager.PlaySound(SoundType.Consumable);
        if (PanelAudio != null) PanelAudio.SetActive(false);
        if (PanelOpciones != null) PanelOpciones.SetActive(true);
    }

    public void CambiarVolumenMusica(float volumen)
    {
        // esta funcion actualiza el volumen de la musica en el gestor y lo guarda en las preferencias del jugador
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null) sm.musicVolume = volumen;
        PlayerPrefs.SetFloat("MusicVolume", volumen);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenSFX(float volumen)
    {
        // esta funcion actualiza el volumen de los efectos en el gestor y lo guarda en las preferencias del jugador
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null) sm.sfxVolume = volumen;
        PlayerPrefs.SetFloat("SFXVolume", volumen);
        PlayerPrefs.Save();
    }

    public void CerrarJuego()
    {
        // esta funcion cierra la aplicacion de forma definitiva
        SoundManager.PlaySound(SoundType.Consumable);
        Application.Quit();
    }

    public void CerrarSesion()
    {
        // esta funcion cierra la sesion del usuario actual en firebase y carga la pantalla de login
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
