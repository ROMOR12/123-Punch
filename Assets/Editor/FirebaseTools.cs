#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Firebase.Firestore;
using System.Collections.Generic;

public class FirebaseTools
{
    [MenuItem("Tools/Firebase/Generar Base de Datos Recompensas")]
    public static async void GenerarRecompensas()
    {
        Debug.Log("Generando base de datos en Firebase...");
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        
        var dias = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object> { { "dia", 1 }, { "tipo", "free_coin" }, { "cantidad", 50 }, { "id_item", "" } },
            new Dictionary<string, object> { { "dia", 2 }, { "tipo", "free_coin" }, { "cantidad", 100 }, { "id_item", "" } },
            new Dictionary<string, object> { { "dia", 3 }, { "tipo", "premium_coin" }, { "cantidad", 10 }, { "id_item", "" } },
            new Dictionary<string, object> { { "dia", 4 }, { "tipo", "free_coin" }, { "cantidad", 150 }, { "id_item", "" } },
            new Dictionary<string, object> { { "dia", 5 }, { "tipo", "lootbox" }, { "cantidad", 1 }, { "id_item", "" } },
            new Dictionary<string, object> { { "dia", 6 }, { "tipo", "premium_coin" }, { "cantidad", 20 }, { "id_item", "" } },
            new Dictionary<string, object> { { "dia", 7 }, { "tipo", "lootbox" }, { "cantidad", 3 }, { "id_item", "" } }
        };

        var datos = new Dictionary<string, object>
        {
            { "dias", dias }
        };

        try
        {
            await db.Collection("ConfiguracionGlobal").Document("RecompensasDiarias").SetAsync(datos);
            Debug.Log("✅ ¡Colección de Recompensas Diarias creada con éxito en Firebase!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al crear las recompensas: {e.Message}. Si falla por permisos, asegúrate de tener las reglas de Firestore abiertas para lectura/escritura mientras configuras o iniciar sesión.");
        }
    }
}
#endif
