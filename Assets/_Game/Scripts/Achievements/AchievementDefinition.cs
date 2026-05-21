using UnityEngine;

[CreateAssetMenu(fileName = "New Achievement Definition", menuName = "Achievements/Achievement Definition")]
public class AchievementDefinition : ScriptableObject
{
    public string id;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public AchievementType type;
    public int targetValue;
    public int reward; // Assuming coins or XP as reward
    public bool hidden; // If true, description/title might be hidden until unlocked
}
