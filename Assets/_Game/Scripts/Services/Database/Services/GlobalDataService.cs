using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;

public class GlobalDataService
{
    public static Dictionary<string, Objeto> cacheObjetos = new Dictionary<string, Objeto>();
    public static bool isLoaded = false;

    public async Task CargarObjetosGlobales()
    {
        if (isLoaded) return;

        Debug.Log("Descargando objetos globales de Firebase...");
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        
        try
        {
            QuerySnapshot snapshot = await db.Collection("objetos_globales").GetSnapshotAsync();
            cacheObjetos.Clear();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Objeto obj = document.ConvertTo<Objeto>();
                    cacheObjetos[obj.id_Objeto] = obj;
                }
            }

            isLoaded = true;
            Debug.Log($"Se han cargado {cacheObjetos.Count} objetos en caché.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error descargando objetos globales: {ex}");
        }
    }
}
