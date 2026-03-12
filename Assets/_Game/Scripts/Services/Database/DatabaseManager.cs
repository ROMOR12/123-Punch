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
    public async Task<bool> NuevoUsuario(string userId, string nombre, string email)
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
            id_level = 1
        };

        // Guardamos los datos del jugador en la base de datos en la nube
        await docRef.SetAsync(usuarioNuevo, SetOptions.MergeAll);

        //TODO Crear un personaje predeterminado para todos los usuarios
        await MandarPersonaje(userId,new PersonajeDatos());
        //TODO Crear un objeto predeterminado para todos los usuarios
        await MandarObjeto(userId,new ReferenciaObjeto());

        return true;
       
    }

    public async Task<bool> MandarPersonaje(string userId,PersonajeDatos personajeNuevo)
    {
        DocumentReference docRef = db.Collection("usuarios").Document(userId).Collection("personajes").Document(personajeNuevo.name);

        await docRef.SetAsync(personajeNuevo, SetOptions.MergeAll);

        return true;

    }

    public async Task<bool> MandarObjeto(string userId, ReferenciaObjeto referenciaObjeto)
    {
        DocumentReference docRef = db.Collection("usuarios").Document(userId).Collection("objetos").Document(referenciaObjeto.name);

        await docRef.SetAsync(referenciaObjeto, SetOptions.MergeAll);

        return true;

    }


}
