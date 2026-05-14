using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Firebase.Firestore;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private List<AchievementDefinition> allAchievements;
    
    // Runtime data cache
    private Dictionary<string, AchievementData> userAchievements = new Dictionary<string, AchievementData>();
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Subscribe to game events
        GameEvents.OnFightWon += () => AddProgress(AchievementType.Wins, 1);
        GameEvents.OnFightLost += () => AddProgress(AchievementType.FightsLost, 1);
        GameEvents.OnKO += () => AddProgress(AchievementType.KO, 1);
        GameEvents.OnCharacterUnlocked += () => AddProgress(AchievementType.CharactersUnlocked, 1);
        GameEvents.OnMinigamePlayed += () => AddProgress(AchievementType.MinigamesPlayed, 1);
        GameEvents.OnPunchThrown += (amount) => AddProgress(AchievementType.PunchesThrown, amount);
        GameEvents.OnCombo += (combo) => AddProgress(AchievementType.Combo, combo); // Combos are checked if combo > target
        GameEvents.OnDailyLogin += () => AddProgress(AchievementType.DailyLogins, 1);
    }

    private void OnDisable()
    {
        // Unsubscribe from game events
        GameEvents.OnFightWon -= () => AddProgress(AchievementType.Wins, 1);
        GameEvents.OnFightLost -= () => AddProgress(AchievementType.FightsLost, 1);
        GameEvents.OnKO -= () => AddProgress(AchievementType.KO, 1);
        GameEvents.OnCharacterUnlocked -= () => AddProgress(AchievementType.CharactersUnlocked, 1);
        GameEvents.OnMinigamePlayed -= () => AddProgress(AchievementType.MinigamesPlayed, 1);
        GameEvents.OnPunchThrown -= (amount) => AddProgress(AchievementType.PunchesThrown, amount);
        GameEvents.OnCombo -= (combo) => AddProgress(AchievementType.Combo, combo);
        GameEvents.OnDailyLogin -= () => AddProgress(AchievementType.DailyLogins, 1);
    }

    private void Start()
    {
        LoadAchievementsData();
    }

    public async void LoadAchievementsData()
    {
        userAchievements.Clear();

        // Check if there's a logged-in user
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            string userId = SessionManager.shared.currentUser.id;
            AchievementService aService = new AchievementService();
            
            List<AchievementData> logsFirebase = await aService.ObtenerLogrosUsuario(userId);
            
            foreach (var data in logsFirebase)
            {
                userAchievements[data.id] = data;
            }
        }
        else
        {
            Debug.Log("Guest mode or no user logged in. Using local temporary achievements.");
            // Guest mode logic - local temp storage only, initialized empty
        }

        // Fill missing achievements with default data
        foreach (var def in allAchievements)
        {
            if (!userAchievements.ContainsKey(def.id))
            {
                userAchievements[def.id] = new AchievementData(def.id);
            }
        }

        isInitialized = true;
    }

    private void AddProgress(AchievementType type, int amount)
    {
        if (!isInitialized) return;

        bool anyProgressChanged = false;

        foreach (var def in allAchievements)
        {
            if (def.type == type)
            {
                AchievementData data = userAchievements[def.id];

                if (!data.unlocked)
                {
                    if (type == AchievementType.Combo)
                    {
                        // For combos, we check if the single combo exceeded target
                        if (amount > data.progress)
                        {
                            data.progress = amount;
                            anyProgressChanged = true;
                        }
                    }
                    else
                    {
                        // Accumulate progress
                        data.progress += amount;
                        anyProgressChanged = true;
                    }

                    // Security check - Cap progress at target value to prevent impossible progress
                    if (data.progress >= def.targetValue)
                    {
                        data.progress = def.targetValue;
                        UnlockAchievement(def);
                    }
                    else if (anyProgressChanged)
                    {
                        // Save progress to firebase only when it changes but not unlocked (Unlock already saves)
                        SaveAchievementToFirebase(data);
                    }
                }
            }
        }
    }

    private void UnlockAchievement(AchievementDefinition def)
    {
        AchievementData data = userAchievements[def.id];
        
        if (data.unlocked) return; // Prevent double unlock

        data.unlocked = true;
        data.unlockDate = Timestamp.GetCurrentTimestamp();
        data.progress = def.targetValue;

        // Give Reward
        GiveReward(def.reward);

        // Show UI Popup
        ShowUnlockPopup(def);

        // Save to Firebase
        SaveAchievementToFirebase(data);
    }

    private async void SaveAchievementToFirebase(AchievementData data)
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            string userId = SessionManager.shared.currentUser.id;
            AchievementService aService = new AchievementService();
            await aService.GuardarLogroUsuario(userId, data);
        }
    }

    private void GiveReward(int amount)
    {
        if (amount <= 0) return;

        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            // Give reward logic
            SessionManager.shared.currentUser.free_coin += amount;
            
            // Should also call UsuarioService to update user in DB
            UpdateUserCoins();
        }
    }

    private async void UpdateUserCoins()
    {
        UsuarioService uService = new UsuarioService();
        await uService.ActualizarUsuario(SessionManager.shared.currentUser);
    }

    private void ShowUnlockPopup(AchievementDefinition def)
    {
        Debug.Log($"Achievement Unlocked! {def.title} - Reward: {def.reward}");
        // Here you would trigger an event or call an UI Manager to show the popup
        // AchievementUIManager.Instance.ShowPopup(def);
    }

    public List<AchievementData> GetAllAchievementsData()
    {
        return new List<AchievementData>(userAchievements.Values);
    }

    public AchievementDefinition GetAchievementDefinition(string id)
    {
        return allAchievements.Find(x => x.id == id);
    }
}
