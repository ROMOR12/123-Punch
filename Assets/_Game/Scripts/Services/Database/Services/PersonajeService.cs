using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class PersonajeService
{
    // Método para obtener los datos de un personaje desde Firebase
    public async Task<Personaje> ObtenerPersonaje(string personajeId)
    {
        try
        {
            // Verificamos que la base de datos esté inicializada
            if (DatabaseManager.shared == null || DatabaseManager.shared.db == null)
            {
                Debug.LogError("Error: DatabaseManager no está inicializado.");
                return null;
            }

            FirebaseFirestore db = DatabaseManager.shared.db;

            // Buscamos en la colección "personajes" el documento con el ID específico
            DocumentSnapshot snap = await db.Collection("personajes").Document(personajeId).GetSnapshotAsync();

            if (snap.Exists)
            {
                // Si existe, Firebase convierte mágicamente el documento a nuestra clase Personaje
                return snap.ConvertTo<Personaje>();
            }
            else
            {
                Debug.LogWarning($"El personaje con ID '{personajeId}' no existe en la base de datos.");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al descargar el personaje {personajeId}: {e.Message}");
            return null;
        }
    }
}