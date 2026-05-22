using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class PersonajeService
{
    // M�todo para obtener los datos de un personaje desde Firebase
    public async Task<Personaje> ObtenerPersonaje(string personajeId)
    {
        try
        {
            // Verificamos que la base de datos est� inicializada
            if (DatabaseManager.shared == null || DatabaseManager.shared.db == null)
            {
                Debug.LogError("Error: DatabaseManager no est� inicializado.");
                return null;
            }

            FirebaseFirestore db = DatabaseManager.shared.db;

            // Buscamos en la colecci�n "personajes" el documento con el ID espec�fico
            DocumentSnapshot snap = await db.Collection("personajes").Document(personajeId).GetSnapshotAsync();

            if (snap.Exists)
            {
                Personaje p = snap.ConvertTo<Personaje>();
                p.id = snap.Id;
                return p;
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