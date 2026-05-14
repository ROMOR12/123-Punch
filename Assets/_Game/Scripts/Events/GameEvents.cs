using System;

public static class GameEvents
{
    // Gameplay events for achievements and stats
    public static Action OnFightWon;
    public static Action OnFightLost;
    public static Action OnKO;
    public static Action OnCharacterUnlocked;
    public static Action OnMinigamePlayed;
    public static Action<int> OnPunchThrown; // Can pass how many punches or just 1
    public static Action<int> OnCombo; // Pass the combo hit count
    public static Action OnDailyLogin;

    // Trigger methods
    public static void TriggerFightWon() => OnFightWon?.Invoke();
    public static void TriggerFightLost() => OnFightLost?.Invoke();
    public static void TriggerKO() => OnKO?.Invoke();
    public static void TriggerCharacterUnlocked() => OnCharacterUnlocked?.Invoke();
    public static void TriggerMinigamePlayed() => OnMinigamePlayed?.Invoke();
    public static void TriggerPunchThrown(int amount = 1) => OnPunchThrown?.Invoke(amount);
    public static void TriggerCombo(int comboHits) => OnCombo?.Invoke(comboHits);
    public static void TriggerDailyLogin() => OnDailyLogin?.Invoke();
}
