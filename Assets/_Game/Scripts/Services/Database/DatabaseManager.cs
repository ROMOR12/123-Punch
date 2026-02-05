using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public async Task<bool> CrearUsuarioNuevo(string userId, string nombre, string email)
    {
        DocumentReference docRef = db.Collection("usuarios").Document(userId);

        Usuario usuarioNuevo = new Usuario()
        {
            id = userId,
            username = nombre,
            email = email,
            first_log = Timestamp.GetCurrentTimestamp(),
            last_log = Timestamp.GetCurrentTimestamp(),
            free_coin = 100,
            premium_coin = 0,
            is_admin = false,
            xp = 0,
            id_world = 1,
            id_level = 1,
            referenciaObjetos = new List<ReferenciaObjeto>()
            { new ReferenciaObjeto(){id_Objeto = "",cantidad = 1}},
            refereniaPersonajes = new List<string>()
            {""}

        };

        // Guardamos los datos del jugador en la base de datos en la nube
        await docRef.SetAsync(usuarioNuevo, SetOptions.MergeAll);

        return true;
       
    }
}
