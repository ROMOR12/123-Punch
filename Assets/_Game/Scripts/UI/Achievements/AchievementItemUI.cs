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
        titleText.text = def.hidden && !data.unlocked ? "???" : def.title;
        descriptionText.text = def.hidden && !data.unlocked ? "Sigue jugando para descubrirlo." : def.description;
        rewardText.text = $"+{def.reward} Monedas";

        // Progress
        progressBar.maxValue = def.targetValue;
        progressBar.value = data.progress;
        progressText.text = $"{data.progress}/{def.targetValue}";

        // States
        lockedOverlay.SetActive(!data.unlocked);
        unlockedOverlay.SetActive(data.unlocked);
    }
}
