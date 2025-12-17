using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ProjectFoldersGenerator
{
    [MenuItem("Tools/Generar Estructura Proyecto Móvil")]
    public static void CreateFolderStructure()
    {
        // 1. Definimos la lista de carpetas que queremos crear
        List<string> folders = new List<string>()
        {
            // Carpetas Raíz
            "ThirdParty",
            "Plugins",
            "Editor",
            
            // Carpeta principal de TU juego
            "_Game",
            
            // Subcarpetas de _Game
            "_Game/Animations",
            "_Game/Art",
            "_Game/Art/Materials",
            "_Game/Art/Models",
            "_Game/Art/Shaders",
            "_Game/Art/Sprites",
            "_Game/Art/UI",
            "_Game/Audio",
            "_Game/Data",
            "_Game/Prefabs",
            "_Game/Scenes",
            "_Game/Scripts",
            
            // Subcarpetas de Scripts (Arquitectura)
            "_Game/Scripts/Core",
            "_Game/Scripts/Gameplay",
            "_Game/Scripts/UI",
            "_Game/Scripts/Utils",
            "_Game/Scripts/Inputs",
            "_Game/Scripts/Integrations"
        };

        // 2. Recorremos la lista y creamos las carpetas
        foreach (string folder in folders)
        {
            // Application.dataPath apunta directamente a la carpeta "Assets"
            string path = Path.Combine(Application.dataPath, folder);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // 3. Refrescamos la base de datos de assets para que aparezcan en Unity al instante
        AssetDatabase.Refresh();

        Debug.Log("<color=green>¡Estructura de carpetas generada con éxito!</color>");
    }
}
