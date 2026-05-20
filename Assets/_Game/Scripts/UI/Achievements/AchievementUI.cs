using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform achievementListContainer; // Asegúrate de que este sea el objeto 'Content' del Scroll View
    public GameObject achievementPrefab; // El prefab que modificamos con el anclaje Stretch Horizontal

    private List<GameObject> activeItems = new List<GameObject>();
    private AchievementType? currentFilter = null;

    private void Start()
    {
        // Ejemplo: Refresh on start
        // Invoke("RefreshUI", 0.5f); // wait for manager to initialize
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void SetFilter(int typeIndex)
    {
        // 0 = All, 1 = Combat, 2 = Characters, 3 = Minigames
        if (typeIndex == 0) currentFilter = null;
        else if (typeIndex == 1) currentFilter = AchievementType.Wins; // Ejemplo de filtro de combate
        // Update this based on how you want to filter multiple types
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (AchievementManager.Instance == null) return;

        // Clear list
        foreach (var item in activeItems)
        {
            if (item != null) Destroy(item);
        }
        activeItems.Clear();

        List<AchievementData> allData = AchievementManager.Instance.GetAllAchievementsData();

        foreach (var data in allData)
        {
            AchievementDefinition def = AchievementManager.Instance.GetAchievementDefinition(data.id);

            if (def == null) continue;

            // Apply filter
            if (currentFilter != null && def.type != currentFilter)
                continue;

            // ==================== SOLUCIÓN DE LAYOUT Y ESCALA ====================
            // Instanciamos pasando el contenedor (el padre) directamente
            GameObject go = Instantiate(achievementPrefab, achievementListContainer);
            activeItems.Add(go);

            // 1. Forzamos a que la escala vuelva a ser 1x1x1 (evita el bug de la UI minúscula)
            go.transform.localScale = Vector3.one;

            // 2. Reseteamos la posición Z local por si acaso el Canvas tiene profundidad
            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector3 localPos = rectTransform.localPosition;
                localPos.z = 0f;
                rectTransform.localPosition = localPos;
            }
            // =====================================================================

            // Populate data
            AchievementItemUI ui = go.GetComponent<AchievementItemUI>();
            if (ui != null)
            {
                ui.Setup(def, data);
            }
        }
    }
}
