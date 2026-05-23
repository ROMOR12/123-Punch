using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Events;
using UnityEngine.Events;
using TMPro;

public class CreateAudioPanelScript : EditorWindow
{
    [MenuItem("Tools/Generar Panel de Audio")]
    public static void GenerarPanel()
    {
        MenuOpciones menuOpciones = Object.FindFirstObjectByType<MenuOpciones>();
        if (menuOpciones == null) return;

        // Limpiar el PanelAudio anterior si existe para no duplicar
        if (menuOpciones.PanelAudio != null)
        {
            DestroyImmediate(menuOpciones.PanelAudio);
        }
        else
        {
            Transform viejoPanel = menuOpciones.transform.Find("PanelAudio");
            if (viejoPanel != null) DestroyImmediate(viejoPanel.gameObject);
        }

        // Clonar PanelOpciones
        GameObject panelAudio = Instantiate(menuOpciones.PanelOpciones, menuOpciones.PanelOpciones.transform.parent);
        panelAudio.name = "PanelAudio";
        panelAudio.SetActive(false);
        panelAudio.transform.SetAsLastSibling();

        // 1. Gestionar el Botón de Volver usando uno de los botones originales como plantilla
        Button[] botonesAntiguos = panelAudio.GetComponentsInChildren<Button>(true);
        Button botonPlantilla = null;
        
        foreach (Button btn in botonesAntiguos)
        {
            if (botonPlantilla == null) 
            {
                botonPlantilla = btn; // Guardamos el primero para usarlo de plantilla (así heredamos los colores y fuentes bonitos)
            }
            else 
            {
                DestroyImmediate(btn.gameObject); // Destruimos los demás
            }
        }

        if (botonPlantilla != null)
        {
            botonPlantilla.name = "BotonVolver";
            RectTransform rtBoton = botonPlantilla.GetComponent<RectTransform>();
            
            // Colocar el botón abajo del todo, centrado
            rtBoton.anchorMin = new Vector2(0.5f, 0f);
            rtBoton.anchorMax = new Vector2(0.5f, 0f);
            rtBoton.pivot = new Vector2(0.5f, 0f);
            rtBoton.anchoredPosition = new Vector2(0, 50); // 50 pixels desde abajo
            rtBoton.sizeDelta = new Vector2(300, 80); // Botón grande

            // Cambiar el texto a "VOLVER"
            TextMeshProUGUI txtBtnTMP = botonPlantilla.GetComponentInChildren<TextMeshProUGUI>();
            if (txtBtnTMP != null) 
            {
                txtBtnTMP.text = "VOLVER";
                txtBtnTMP.fontSize = 45;
                txtBtnTMP.enableAutoSizing = false;
            }
            else
            {
                Text txtBtn = botonPlantilla.GetComponentInChildren<Text>();
                if (txtBtn != null) 
                {
                    txtBtn.text = "VOLVER";
                    txtBtn.fontSize = 40;
                }
            }

            // Cambiar la acción para que cierre el panel
            for (int i = 0; i < botonPlantilla.onClick.GetPersistentEventCount(); i++)
            {
                UnityEventTools.RemovePersistentListener(botonPlantilla.onClick, i);
            }
            UnityEventTools.AddVoidPersistentListener(botonPlantilla.onClick, new UnityAction(menuOpciones.CerrarPanelAudio));
        }

        // 2. Título principal
        TextMeshProUGUI[] textosAntiguos = panelAudio.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI txt in textosAntiguos)
        {
            string nombreTxt = txt.gameObject.name.ToLower();
            if (nombreTxt.Contains("title") || nombreTxt.Contains("titulo") || txt.fontSize >= 50)
            {
                txt.text = "OPCIONES DE AUDIO";
                txt.fontSize = 60;
                txt.alignment = TextAlignmentOptions.Center;
                txt.color = new Color(0.9f, 0.9f, 0.1f); // Amarillo chulo
                break; // Solo cambiar el primero que parezca un título
            }
        }

        // 3. Crear Sliders Bonitos
        DefaultControls.Resources uiResources = new DefaultControls.Resources();
        uiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

        // Crear Slider Música
        GameObject sliderMusicObj = CrearSliderBonito("MÚSICA", uiResources, panelAudio.transform, new Vector2(0, 100));
        // Crear Slider SFX
        GameObject sliderSFXObj = CrearSliderBonito("EFECTOS", uiResources, panelAudio.transform, new Vector2(0, -100));

        // 4. Asignar en MenuOpciones
        menuOpciones.PanelAudio = panelAudio;
        menuOpciones.sliderMusic = sliderMusicObj.GetComponent<Slider>();
        menuOpciones.sliderSFX = sliderSFXObj.GetComponent<Slider>();

        EditorUtility.SetDirty(menuOpciones);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("¡Panel de Audio rediseñado con éxito!");
    }

    private static GameObject CrearSliderBonito(string titulo, DefaultControls.Resources uiResources, Transform parent, Vector2 position)
    {
        // Crear un contenedor vacio para mantener el texto y el slider juntos
        GameObject container = new GameObject("ContenedorSlider_" + titulo);
        container.transform.SetParent(parent, false);
        RectTransform rtContainer = container.AddComponent<RectTransform>();
        rtContainer.anchoredPosition = position;
        rtContainer.sizeDelta = new Vector2(500, 120);

        // Crear Texto (Usaremos un Text normal, pero grande y en negrita)
        GameObject textObj = DefaultControls.CreateText(uiResources);
        textObj.transform.SetParent(container.transform, false);
        RectTransform rtTxt = textObj.GetComponent<RectTransform>();
        rtTxt.anchoredPosition = new Vector2(0, 40); // Arriba del slider
        rtTxt.sizeDelta = new Vector2(500, 50);
        Text txt = textObj.GetComponent<Text>();
        txt.text = titulo;
        txt.fontSize = 45;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        // Añadir una sombra para que resalte
        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0,0,0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        // Crear Slider
        GameObject sliderObj = DefaultControls.CreateSlider(uiResources);
        sliderObj.transform.SetParent(container.transform, false);
        RectTransform rtSlider = sliderObj.GetComponent<RectTransform>();
        rtSlider.anchoredPosition = new Vector2(0, -20);
        rtSlider.sizeDelta = new Vector2(400, 40); // Más grueso y ancho

        Slider slider = sliderObj.GetComponent<Slider>();
        
        // Estilizar el fondo del slider
        Transform bg = sliderObj.transform.Find("Background");
        if (bg != null)
        {
            Image bgImg = bg.GetComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Gris oscuro
        }

        // Estilizar el relleno (Fill)
        Transform fillArea = sliderObj.transform.Find("Fill Area");
        if (fillArea != null)
        {
            Transform fill = fillArea.Find("Fill");
            if (fill != null)
            {
                Image fillImg = fill.GetComponent<Image>();
                fillImg.color = new Color(1f, 0.4f, 0f); // Naranja vibrante
            }
        }

        // Estilizar el tirador (Handle)
        Transform handleArea = sliderObj.transform.Find("Handle Slide Area");
        if (handleArea != null)
        {
            Transform handle = handleArea.Find("Handle");
            if (handle != null)
            {
                Image handleImg = handle.GetComponent<Image>();
                handleImg.color = Color.white;
                RectTransform rtHandle = handle.GetComponent<RectTransform>();
                rtHandle.sizeDelta = new Vector2(40, 0); // Tirador más grande
            }
        }

        return sliderObj;
    }
}
