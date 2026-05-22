using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public class FixAchievementPrefab
{
    static FixAchievementPrefab()
    {
        if (!EditorPrefs.GetBool("AchievementPrefabFixed_Antigravity_V3", false))
        {
            EditorApplication.delayCall += () => {
                DoFix();
                EditorPrefs.SetBool("AchievementPrefabFixed_Antigravity_V3", true);
            };
        }
    }

    [MenuItem("Tools/Antigravity/Arreglar Prefab Logros")]
    public static void DoFix()
    {
        string[] guids = AssetDatabase.FindAssets("PrefabPanelLogros t:Prefab");
        if (guids.Length == 0) return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        RectTransform rt = instance.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(650, 190);

        Color darkBrown = new Color(0.20f, 0.12f, 0.06f); 
        
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
            titulo.rectTransform.anchoredPosition = new Vector2(25, -20);
            titulo.rectTransform.sizeDelta = new Vector2(400, 40);
        }

        TextMeshProUGUI descripcion = instance.transform.Find("Descripcion")?.GetComponent<TextMeshProUGUI>();
        if (descripcion != null)
        {
            descripcion.color = darkBrown;
            descripcion.fontSize = 22;
            descripcion.alignment = TextAlignmentOptions.TopLeft;
            descripcion.enableWordWrapping = true;
            descripcion.rectTransform.anchorMin = new Vector2(0, 1);
            descripcion.rectTransform.anchorMax = new Vector2(0, 1);
            descripcion.rectTransform.pivot = new Vector2(0, 1);
            descripcion.rectTransform.anchoredPosition = new Vector2(25, -60);
            descripcion.rectTransform.sizeDelta = new Vector2(600, 80);
        }

        TextMeshProUGUI progreso = instance.transform.Find("Progreso")?.GetComponent<TextMeshProUGUI>();
        if (progreso != null)
        {
            progreso.color = darkBrown;
            progreso.fontStyle = FontStyles.Bold;
            progreso.fontSize = 28;
            progreso.alignment = TextAlignmentOptions.TopRight;
            progreso.rectTransform.anchorMin = new Vector2(1, 1);
            progreso.rectTransform.anchorMax = new Vector2(1, 1);
            progreso.rectTransform.pivot = new Vector2(1, 1);
            progreso.rectTransform.anchoredPosition = new Vector2(-25, -20);
            progreso.rectTransform.sizeDelta = new Vector2(150, 40);
        }

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
        rewardText.fontSize = 28;
        rewardText.alignment = TextAlignmentOptions.BottomRight;
        
        RectTransform rRt = rewardText.GetComponent<RectTransform>();
        rRt.anchorMin = new Vector2(1, 0);
        rRt.anchorMax = new Vector2(1, 0);
        rRt.pivot = new Vector2(1, 0);
        rRt.anchoredPosition = new Vector2(-25, 20);
        rRt.sizeDelta = new Vector2(250, 40);

        Transform sliderTransform = instance.transform.Find("BarraProgreso");
        Slider progressBar = null;
        if (sliderTransform == null)
        {
            GameObject sliderObj = new GameObject("BarraProgreso");
            sliderObj.transform.SetParent(instance.transform, false);
            progressBar = sliderObj.AddComponent<Slider>();
            
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.3f, 0.2f, 0.1f); 
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillAreaObj.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = new Vector2(-10, 0);

            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.8f, 0.2f); 
            RectTransform fRt = fillObj.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.sizeDelta = Vector2.zero;

            progressBar.targetGraphic = bgImg;
            progressBar.fillRect = fRt;
            progressBar.value = 0.5f;
            progressBar.interactable = false;
        }
        else
        {
            progressBar = sliderTransform.GetComponent<Slider>();
        }
        
        RectTransform sRt = progressBar.GetComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 0);
        sRt.anchorMax = new Vector2(0, 0);
        sRt.pivot = new Vector2(0, 0);
        sRt.anchoredPosition = new Vector2(25, 25);
        sRt.sizeDelta = new Vector2(300, 20);

        Transform lockedOverlay = instance.transform.Find("LockedOverlay");
        if (lockedOverlay != null)
        {
            Image img = lockedOverlay.GetComponent<Image>();
            if (img == null) img = lockedOverlay.gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.70f); 

            RectTransform lRt = lockedOverlay.GetComponent<RectTransform>();
            lRt.anchorMin = Vector2.zero;
            lRt.anchorMax = Vector2.one;
            lRt.sizeDelta = Vector2.zero;
            lRt.anchoredPosition = Vector2.zero;
            
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

        PrefabUtility.SaveAsPrefabAsset(instance, path);
        GameObject.DestroyImmediate(instance);

        Debug.Log("<color=cyan>[Antigravity]</color> Prefab restablecido a diseo balanceado.");
    }
}