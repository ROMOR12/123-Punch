using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform achievementListContainer;
    public GameObject achievementPrefab; // A prefab containing a script AchievementItemUI
    public TextMeshProUGUI filterText;

    private List<GameObject> activeItems = new List<GameObject>();
    private AchievementType? currentFilter = null;

    private void Start()
    {
        // Example: Refresh on start
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
        else if (typeIndex == 1) currentFilter = AchievementType.Wins; // Example of a combat filter, you'd probably need a broader filter list
        // Update this based on how you want to filter multiple types
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (AchievementManager.Instance == null) return;

        // Clear list
        foreach (var item in activeItems)
        {
            Destroy(item);
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

            // Instantiate
            GameObject go = Instantiate(achievementPrefab, achievementListContainer);
            activeItems.Add(go);

            // Populate data
            AchievementItemUI ui = go.GetComponent<AchievementItemUI>();
            if (ui != null)
            {
                ui.Setup(def, data);
            }
        }
    }
}
