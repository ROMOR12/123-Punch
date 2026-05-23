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
        GameEvents.OnFightWon += HandleFightWon;
        GameEvents.OnFightLost += HandleFightLost;
        GameEvents.OnKO += HandleKO;
        GameEvents.OnCharacterUnlocked += HandleCharacterUnlocked;
        GameEvents.OnMinigamePlayed += HandleMinigamePlayed;
        GameEvents.OnPunchThrown += HandlePunchThrown;
        GameEvents.OnCombo += HandleCombo;
        GameEvents.OnDailyLogin += HandleDailyLogin;
    }

    private void OnDisable()
    {
        // Unsubscribe from game events
        GameEvents.OnFightWon -= HandleFightWon;
        GameEvents.OnFightLost -= HandleFightLost;
        GameEvents.OnKO -= HandleKO;
        GameEvents.OnCharacterUnlocked -= HandleCharacterUnlocked;
        GameEvents.OnMinigamePlayed -= HandleMinigamePlayed;
        GameEvents.OnPunchThrown -= HandlePunchThrown;
        GameEvents.OnCombo -= HandleCombo;
        GameEvents.OnDailyLogin -= HandleDailyLogin;
    }

    private void HandleFightWon()
    {
        AddProgress(AchievementType.Wins, 1);
        AddProgress(AchievementType.TotalFights, 1);
    }
    private void HandleFightLost()
    {
        AddProgress(AchievementType.FightsLost, 1);
        AddProgress(AchievementType.TotalFights, 1);
    }
    private void HandleKO() => AddProgress(AchievementType.KO, 1);
    private void HandleCharacterUnlocked() => AddProgress(AchievementType.CharactersUnlocked, 1);
    private void HandleMinigamePlayed() => AddProgress(AchievementType.MinigamesPlayed, 1);
    private void HandlePunchThrown(int amount) => AddProgress(AchievementType.PunchesThrown, amount);
    private void HandleCombo(int combo) => AddProgress(AchievementType.Combo, combo);
    private void HandleDailyLogin() => AddProgress(AchievementType.DailyLogins, 1);

    private void Start()
    {
        LoadAchievementsData();
    }

    public async void LoadAchievementsData()
    {
        // Keep a reference to the active user to ensure we don't apply data for a user who logged out during the await
        string currentLoadingUserId = SessionManager.shared != null && SessionManager.shared.currentUser != null 
            ? SessionManager.shared.currentUser.id 
            : null;

        Dictionary<string, AchievementData> loadedAchievements = new Dictionary<string, AchievementData>();

        if (!string.IsNullOrEmpty(currentLoadingUserId))
        {
            AchievementService aService = new AchievementService();
            List<AchievementData> logsFirebase = await aService.ObtenerLogrosUsuario(currentLoadingUserId);
            
            // Check if the user is still the same after the async call
            string activeUserId = SessionManager.shared != null && SessionManager.shared.currentUser != null 
                ? SessionManager.shared.currentUser.id 
                : null;
                
            if (activeUserId != currentLoadingUserId)
            {
                Debug.LogWarning("User changed or logged out while loading achievements. Aborting load.");
                return;
            }

            foreach (var data in logsFirebase)
            {
                if (data != null)
                {
                    loadedAchievements[data.id] = data;
                }
            }
        }
        else
        {
            Debug.Log("Guest mode or no user logged in. Using local temporary achievements.");
        }

        // Fill missing achievements with default data
        foreach (var def in allAchievements)
        {
            if (def != null && !loadedAchievements.ContainsKey(def.id))
            {
                loadedAchievements[def.id] = new AchievementData(def.id);
            }
        }

        // Swap reference atomically
        userAchievements = loadedAchievements;
        isInitialized = true;

        if (!string.IsNullOrEmpty(currentLoadingUserId))
        {
            if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
            {
                var user = SessionManager.shared.currentUser;
                DateTime lastLogDate = user.last_log.ToDateTime().Date;
                bool esNuevoDia = lastLogDate < System.DateTime.UtcNow.Date;
                
                // Siempre actualizamos el last_log a la fecha exacta de ahora para tener el registro real
                user.last_log = Timestamp.GetCurrentTimestamp();
                UsuarioService uService = new UsuarioService();
                _ = uService.ActualizarUsuario(user);
                
                // Pero solo lanzamos el evento diario si ha pasado un dÃ­a entero
                if (esNuevoDia)
                {
                    GameEvents.TriggerDailyLogin();
                }
            }
        }
    }

    private void AddProgress(AchievementType type, int amount)
    {
        if (!isInitialized) return;

        bool anyProgressChanged = false;

        foreach (var def in allAchievements)
        {
            if (def != null && def.type == type)
            {
                if (!userAchievements.ContainsKey(def.id))
                {
                    userAchievements[def.id] = new AchievementData(def.id);
                }

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
        if (!userAchievements.ContainsKey(def.id))
        {
            userAchievements[def.id] = new AchievementData(def.id);
        }

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