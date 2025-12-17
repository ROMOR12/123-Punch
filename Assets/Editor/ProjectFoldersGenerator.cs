using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PunchProjectSetup
{
    [MenuItem("Tools/123 PUNCH!/Generar Estructura")]
    public static void CreateFolderStructure()
    {
        List<string> folders = new List<string>()
        {
            // --- CARPETAS DE TERCEROS (SDKs) ---
            "ThirdParty/Photon",        // Multiplayer
            "ThirdParty/Firebase",      // Backend y Base de datos
            "ThirdParty/GooglePlay",    // Login y Billing
            "Plugins/Android",
            "Plugins/iOS",
            "Editor",

            // --- TU JUEGO ---
            "_Game",
            
            // ART & ASSETS
            "_Game/Art/UI/Icons",
            "_Game/Art/UI/Menus",
            "_Game/Art/Characters",
            "_Game/Art/Animations",
            "_Game/Art/Materials",
            "_Game/Art/Shaders",
            "_Game/Audio/SFX",
            "_Game/Audio/Music",

            // DATOS (ScriptableObjects)
            "_Game/Data/Characters",    // Stats: Vida, Energía, Fuerza
            "_Game/Data/Items",         // Objetos de tienda
            "_Game/Data/Enemies",
            "_Game/Data/Levels",        

            // PREFABS
            "_Game/Prefabs/Characters",
            "_Game/Prefabs/UI",
            "_Game/Prefabs/Environment",

            // SCENES
            "_Game/Scenes/Boot",        // Inicialización
            "_Game/Scenes/Menu",
            "_Game/Scenes/Combat",
            "_Game/Scenes/Minigames",   

            // CÓDIGO (Arquitectura)
            "_Game/Scripts/App",        // Main Entry Point
            
            // Gameplay
            "_Game/Scripts/Gameplay/Combat",    // Lógica de ataque/defensa/stamina
            "_Game/Scripts/Gameplay/Minigames",
            "_Game/Scripts/Gameplay/Characters",
            
            // UI
            "_Game/Scripts/UI/Controllers",
            "_Game/Scripts/UI/Views",           
            
            // Servicios (Backend)
            "_Game/Scripts/Services/Auth",      // Login
            "_Game/Scripts/Services/Database",  // Guardado Cloud
            "_Game/Scripts/Services/Network",   // Photon
            "_Game/Scripts/Services/IAP",       // Compras
            
            // Modelos de Datos (Clases C# puras)
            "_Game/Scripts/Models"
        };

        foreach (string folder in folders)
        {
            string path = Path.Combine(Application.dataPath, folder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Estructura de '1 2 3 PUNCH!' generada correctamente.");
    }
}
