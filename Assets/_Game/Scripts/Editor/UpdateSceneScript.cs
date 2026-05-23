using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class UpdateSceneScript
{
    [MenuItem("Tools/Update VSbotLvl1 Scene")]
    public static void UpdateScene()
    {
        string vsScenePath = "Assets/_Game/Scenes/Combat/Bots/VSbotLvl1.unity";
        string histScenePath = "Assets/_Game/Scenes/Combat/History/CombateHistoria1.unity";

        if (!File.Exists(vsScenePath) || !File.Exists(histScenePath))
        {
            Debug.LogError("Error: No se encontraron las escenas en las rutas especificadas.");
            return;
        }

        string tempEnemyPrefabPath = "Assets/temp_enemy_vsbot1.prefab";
        string tempMapPrefabPath = "Assets/temp_map_vsbot1.prefab";

        try
        {
            // 1. Abrimos VSbotLvl1 para guardar el enemigo y el mapa temporalmente
            Scene vsScene = EditorSceneManager.OpenScene(vsScenePath, OpenSceneMode.Single);
            
            GameObject enemyToKeep = GameObject.Find("enemy");
            GameObject mapToKeep = GameObject.Find("ringv2_0");

            if (enemyToKeep == null || mapToKeep == null)
            {
                Debug.LogError("No se encontro 'enemy' o 'ringv2_0' en la escena VSbotLvl1. Comprueba los nombres de los GameObjects.");
                return;
            }

            // Los guardamos como prefabs temporales
            PrefabUtility.SaveAsPrefabAsset(enemyToKeep, tempEnemyPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(mapToKeep, tempMapPrefabPath);

            // 2. Abrimos la escena actualizada (CombateHistoria1)
            Scene histScene = EditorSceneManager.OpenScene(histScenePath, OpenSceneMode.Single);

            // Borramos su enemigo y mapa actual
            GameObject histEnemy = GameObject.Find("enemy");
            GameObject histMap = GameObject.Find("ringv2_0");

            if (histEnemy != null) GameObject.DestroyImmediate(histEnemy);
            if (histMap != null) GameObject.DestroyImmediate(histMap);

            // 3. Instanciamos los objetos guardados de VSbotLvl1
            GameObject newEnemyObj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(tempEnemyPrefabPath));
            GameObject newMapObj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(tempMapPrefabPath));

            // Desempaquetamos los prefabs para que no queden vinculados a los temporales
            PrefabUtility.UnpackPrefabInstance(newEnemyObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            PrefabUtility.UnpackPrefabInstance(newMapObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            // 4. Conectamos el enemigo instanciado a los managers actualizados
            EnemyBot enemyBotComponent = newEnemyObj.GetComponent<EnemyBot>();
            CombatController cc = GameObject.FindObjectOfType<CombatController>();
            RoundManager rm = GameObject.FindObjectOfType<RoundManager>();
            GameLoader gl = GameObject.FindObjectOfType<GameLoader>();

            if (enemyBotComponent != null)
            {
                if (cc != null) 
                {
                    cc.currentEnemy = enemyBotComponent;
                    enemyBotComponent.playerCombat = cc;
                    EditorUtility.SetDirty(cc);
                }
                if (rm != null) 
                {
                    rm.enemyBot = enemyBotComponent;
                    EditorUtility.SetDirty(rm);
                }
                if (gl != null)
                {
                    gl.enemyBot = enemyBotComponent;
                    EditorUtility.SetDirty(gl);
                }
                
                EditorUtility.SetDirty(enemyBotComponent);
            }
            else
            {
                Debug.LogWarning("El GameObject 'enemy' no tiene el script 'EnemyBot' adjunto.");
            }

            // 5. Guardamos como VSbotLvl1 (sobrescribimos)
            EditorSceneManager.SaveScene(histScene, vsScenePath);

            Debug.Log("<color=green>¡Exito! La escena VSbotLvl1 ha sido actualizada con los sistemas de CombateHistoria1, conservando su enemigo y mapa.</color>");
        }
        finally
        {
            // 6. Limpieza
            if (File.Exists(tempEnemyPrefabPath))
            {
                AssetDatabase.DeleteAsset(tempEnemyPrefabPath);
            }
            if (File.Exists(tempMapPrefabPath))
            {
                AssetDatabase.DeleteAsset(tempMapPrefabPath);
            }
            AssetDatabase.Refresh();
        }
    }
}
