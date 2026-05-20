using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public class FixAchievementPrefab
{
    static FixAchievementPrefab()
    {
        if (!EditorPrefs.GetBool("AchievementPrefabFixed_Antigravity", false))
        {
            EditorApplication.delayCall += () => {
                DoFix();
                EditorPrefs.SetBool("AchievementPrefabFixed_Antigravity", true);
            };
        }
    }

    [MenuItem("Tools/Antigravity/Arreglar Prefab Logros")]
    public static void DoFix()
    {
        string[] guids = AssetDatabase.FindAssets("PrefabPanelLogros t:Prefab");
        if (guids.Length == 0) 
        {
            Debug.LogWarning("No se encontró el prefab 'PrefabPanelLogros'.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        if (prefab == null) return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        // --- 1. Fix RectTransform (Width to 650, Height to 180) ---
        RectTransform rt = instance.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(650, 180);
        }

        // --- 2. Fix Text Colors & Outlines ---
        Color darkBrown = new Color(0.24f, 0.14f, 0.07f); // #3E2511
        
        TextMeshProUGUI titulo = instance.transform.Find("Titulo")?.GetComponent<TextMeshProUGUI>();
        if (titulo != null)
        {
            titulo.color = darkBrown;
            titulo.fontStyle = FontStyles.Bold;
            titulo.fontSize = 32;
            titulo.alignment = TextAlignmentOptions.TopLeft;
            titulo.rectTransform.anchorMin = new Vector2(0, 1);
            titulo.rectTransform.anchorMax = new Vector2(0, 1);
            titulo.rectTransform.pivot = new Vector2(0, 1);
            titulo.rectTransform.anchoredPosition = new Vector2(30, -20);
            titulo.rectTransform.sizeDelta = new Vector2(400, 40);
        }

        TextMeshProUGUI descripcion = instance.transform.Find("Descripcion")?.GetComponent<TextMeshProUGUI>();
        if (descripcion != null)
        {
            descripcion.color = darkBrown;
            descripcion.fontSize = 24;
            descripcion.alignment = TextAlignmentOptions.TopLeft;
            descripcion.enableWordWrapping = true;
            descripcion.rectTransform.anchorMin = new Vector2(0, 1);
            descripcion.rectTransform.anchorMax = new Vector2(0, 1);
            descripcion.rectTransform.pivot = new Vector2(0, 1);
            descripcion.rectTransform.anchoredPosition = new Vector2(30, -60);
            descripcion.rectTransform.sizeDelta = new Vector2(590, 80);
        }

        TextMeshProUGUI progreso = instance.transform.Find("Progreso")?.GetComponent<TextMeshProUGUI>();
        if (progreso != null)
        {
            progreso.color = darkBrown;
            progreso.fontStyle = FontStyles.Bold;
            progreso.fontSize = 26;
            progreso.alignment = TextAlignmentOptions.TopRight;
            progreso.rectTransform.anchorMin = new Vector2(1, 1);
            progreso.rectTransform.anchorMax = new Vector2(1, 1);
            progreso.rectTransform.pivot = new Vector2(1, 1);
            progreso.rectTransform.anchoredPosition = new Vector2(-30, -20);
            progreso.rectTransform.sizeDelta = new Vector2(150, 40);
        }

        // --- 3. Add Missing Components (Reward Text, Slider) ---
        Transform rewardTransform = instance.transform.Find("Recompensa");
        TextMeshProUGUI rewardText = null;
        if (rewardTransform == null)
        {
            GameObject rewardObj = new GameObject("Recompensa");
            rewardObj.transform.SetParent(instance.transform, false);
            rewardText = rewardObj.AddComponent<TextMeshProUGUI>();
        }
        else
        {
            rewardText = rewardTransform.GetComponent<TextMeshProUGUI>();
        }
        
        rewardText.text = "+100 Monedas";
        rewardText.color = new Color(0.85f, 0.65f, 0.1f); // Gold
        rewardText.fontStyle = FontStyles.Bold;
        rewardText.fontSize = 26;
        rewardText.alignment = TextAlignmentOptions.BottomRight;
        
        RectTransform rRt = rewardText.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(1, 0);
        rRt.anchorMax = new Vector2(1, 0);
        rRt.pivot = new Vector2(1, 0);
        rRt.anchoredPosition = new Vector2(-30, 20);
        rRt.sizeDelta = new Vector2(250, 40);

        Transform sliderTransform = instance.transform.Find("BarraProgreso");
        Slider progressBar = null;
        if (sliderTransform == null)
        {
            GameObject sliderObj = new GameObject("BarraProgreso");
            sliderObj.transform.SetParent(instance.transform, false);
            progressBar = sliderObj.AddComponent<Slider>();
            
            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.2f, 0.1f); // Dark wood bg
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillAreaObj.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = new Vector2(-10, 0); // Padding

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.8f, 0.2f); // Green
            RectTransform fRt = fillObj.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;

            progressBar.targetGraphic = bgImg;
            progressBar.fillRect = fRt;
            progressBar.value = 0.5f;
            progressBar.interactable = false; // It's just a display!
        }
        else
        {
            progressBar = sliderTransform.GetComponent<Slider>();
        }
        
        RectTransform sRt = progressBar.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 0);
        sRt.anchorMax = new Vector2(0, 0);
        sRt.pivot = new Vector2(0, 0);
        sRt.anchoredPosition = new Vector2(30, 25);
        sRt.sizeDelta = new Vector2(250, 20);

        // --- 4. Fix Locked Overlay ---
        Transform lockedOverlay = instance.transform.Find("LockedOverlay");
        if (lockedOverlay != null)
        {
            Image img = lockedOverlay.GetComponent<Image>();
            if (img == null) img = lockedOverlay.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.75f); // Dark overlay

            RectTransform lRt = lockedOverlay.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.sizeDelta = Vector2.zero;
            lRt.anchoredPosition = Vector2.zero;
            
            // Adjust padlock
            Transform padlock = lockedOverlay.Find("CandadoBloqueado");
            if (padlock != null)
            {
                RectTransform padRt = padlock.GetComponent<RectTransform>();
                padRt.anchorMin = new Vector2(0.5f, 0.5f);
                padRt.anchorMax = new Vector2(0.5f, 0.5f);
                padRt.pivot = new Vector2(0.5f, 0.5f);
                padRt.anchoredPosition = Vector2.zero;
                padRt.sizeDelta = new Vector2(60, 60);
            }
        }
        
        Transform unlockedOverlay = instance.transform.Find("UnlockedOverlay");
        if (unlockedOverlay != null)
        {
            RectTransform uRt = unlockedOverlay.GetComponent<RectTransform>();
            uRt.anchorMin = Vector2.zero;
            uRt.anchorMax = Vector2.one;
            uRt.sizeDelta = Vector2.zero;
            uRt.anchoredPosition = Vector2.zero;
        }

        // --- 5. Update Script References ---
        var script = instance.GetComponent<AchievementItemUI>();
        if (script != null)
        {
            script.rewardText = rewardText;
            script.progressBar = progressBar;
            script.titleText = titulo;
            script.descriptionText = descripcion;
            script.progressText = progreso;
            if (lockedOverlay != null) script.lockedOverlay = lockedOverlay.gameObject;
            if (unlockedOverlay != null) script.unlockedOverlay = unlockedOverlay.gameObject;
            
            EditorUtility.SetDirty(script);
        }

        // Apply back to prefab
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        GameObject.DestroyImmediate(instance);

        Debug.Log("<color=cyan>[Antigravity]</color> ¡Prefab de Logros (PrefabPanelLogros) ha sido re-diseñado y corregido automáticamente!");
    }
}