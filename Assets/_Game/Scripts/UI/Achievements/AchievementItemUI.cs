using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI rewardText;
    public Slider progressBar;
    public GameObject lockedOverlay;
    public GameObject unlockedOverlay;

    public void Setup(AchievementDefinition def, AchievementData data)
    {
        if (def == null)
        {
            Debug.LogError("def (AchievementDefinition) is null in AchievementItemUI.Setup", this);
            return;
        }
        if (data == null)
        {
            Debug.LogError("data (AchievementData) is null in AchievementItemUI.Setup", this);
            return;
        }

        Debug.Log($"Logro: {def.title} | ¿Desbloqueado en Data?: {data.unlocked}");

        if (titleText != null)
        {
            titleText.text = def.hidden && !data.unlocked ? "???" : def.title;
        }
        else
        {
            Debug.LogWarning("titleText reference is missing on AchievementItemUI prefab!", this);
        }

        if (descriptionText != null)
        {
            descriptionText.text = def.hidden && !data.unlocked ? "Sigue jugando para descubrirlo." : def.description;
        }
        else
        {
            Debug.LogWarning("descriptionText reference is missing on AchievementItemUI prefab!", this);
        }

        if (rewardText != null)
        {
            rewardText.text = $"+{def.reward} Monedas";
        }
        else
        {
            Debug.LogWarning("rewardText reference is missing on AchievementItemUI prefab!", this);
        }

        // Progress
        if (progressBar != null)
        {
            progressBar.maxValue = def.targetValue;
            progressBar.value = data.progress;
        }
        else
        {
            Debug.LogWarning("progressBar reference is missing on AchievementItemUI prefab!", this);
        }

        if (progressText != null)
        {
            progressText.text = $"{data.progress}/{def.targetValue}";
        }
        else
        {
            Debug.LogWarning("progressText reference is missing on AchievementItemUI prefab!", this);
        }

        // States
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!data.unlocked);
        }
        else
        {
            Debug.LogWarning("lockedOverlay reference is missing on AchievementItemUI prefab!", this);
        }

        if (unlockedOverlay != null)
        {
            unlockedOverlay.SetActive(data.unlocked);
        }
        else
        {
            Debug.LogWarning("unlockedOverlay reference is missing on AchievementItemUI prefab!", this);
        }
    }
}