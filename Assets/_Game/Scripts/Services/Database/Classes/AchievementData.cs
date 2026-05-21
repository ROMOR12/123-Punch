using Firebase.Firestore;
using System;

[FirestoreData]
[Serializable]
public class AchievementData
{
    [FirestoreProperty]
    public string id { get; set; }

    [FirestoreProperty]
    public int progress { get; set; }

    [FirestoreProperty]
    public bool unlocked { get; set; }

    [FirestoreProperty]
    public Timestamp unlockDate { get; set; }

    public AchievementData() { }

    public AchievementData(string id)
    {
        this.id = id;
        this.progress = 0;
        this.unlocked = false;
        // unlockDate is not set until unlocked
    }
}
