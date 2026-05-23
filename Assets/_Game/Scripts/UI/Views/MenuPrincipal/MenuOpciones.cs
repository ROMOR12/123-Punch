using UnityEngine;
using UnityEngine.UI;

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

    public void MostrarPanelAudio()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        if (PanelAudio != null) PanelAudio.SetActive(true);
        if (PanelOpciones != null) PanelOpciones.SetActive(false);
    }

    public void CerrarPanelAudio()
    {
        SoundManager.PlaySound(SoundType.Consumable);
        if (PanelAudio != null) PanelAudio.SetActive(false);
        if (PanelOpciones != null) PanelOpciones.SetActive(true);
    }

    public void CambiarVolumenMusica(float volumen)
    {
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null) sm.musicVolume = volumen;
        PlayerPrefs.SetFloat("MusicVolume", volumen);
        PlayerPrefs.Save();
    }

    public void CambiarVolumenSFX(float volumen)
    {
        SoundManager sm = FindFirstObjectByType<SoundManager>();
        if (sm != null) sm.sfxVolume = volumen;
        PlayerPrefs.SetFloat("SFXVolume", volumen);
        PlayerPrefs.Save();
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
