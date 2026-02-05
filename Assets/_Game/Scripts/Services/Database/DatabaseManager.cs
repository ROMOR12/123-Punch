using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager shared;

    private FirebaseFirestore db;

    void Awake()
    {
        if (shared == null)
        {
            shared = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    // Metodo para crear un nuevo usuario en la base de datos
    public void CrearUsuarioNuevo(string userId, string nombre, string email)
    {
        DocumentReference docRef = db.Collection("usuarios").Document(userId);

        // Preparamos los datos del jugador
        Dictionary<string, object> datosJugador = new Dictionary<string, object>
        {
            { "username", nombre },
            { "email", email },
            { "monedas", 100 },
            { "nivel", 1 },
            { "xp", 0 },
            { "fecha_creacion", FieldValue.ServerTimestamp }
        };

        // Guardamos los datos del jugador en la base de datos en la nube
        docRef.SetAsync(datosJugador).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error al guardar datos: " + task.Exception);
            }
            else
            {

                Debug.Log("¡Usuario guardado en la Base de Datos!");
            }
        });
    }
}
