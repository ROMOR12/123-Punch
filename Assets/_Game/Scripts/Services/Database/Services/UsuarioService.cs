using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class UsuarioService
{
    public async Task<bool> NuevoUsuario(string userId, string nombre, string email)
    {
        try{
            FirebaseFirestore _context = DatabaseManager.shared.db;

            DocumentReference docRef = _context.Collection("usuarios").Document(userId);



            Usuario usuarioNuevo = new Usuario()
            {
                id = userId,
                username = nombre,
                email = email,
                first_log = Timestamp.GetCurrentTimestamp(),
                last_log = Timestamp.GetCurrentTimestamp(),
                free_coin = 100,
                premium_coin = 0,
                lootboxes = 0,
                is_admin = false,
                xp = 0,
                id_world = "1",
                id_level = "1"
            };

            // Guardamos los datos del jugador en la base de datos en la nube
            await docRef.SetAsync(usuarioNuevo, SetOptions.MergeAll);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Usuario?> ObtenerUsuario(string userId)
    {   
        try{
            FirebaseFirestore _context = DatabaseManager.shared.db;

            DocumentSnapshot snapshot = await _context.Collection("usuarios").Document(userId).GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    Usuario usuario = snapshot.ConvertTo<Usuario>();
                    return usuario;
                }
                else
                {
                    return null;
                }
        }catch { return null; }

    }

    public async Task<bool> ActualizarUsuario(Usuario usuario)
    {
        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;
            
            DocumentReference docRef = _context.Collection("usuarios").Document(usuario.id);

            
                
            await docRef.SetAsync(usuario, SetOptions.MergeAll);

            return true;
            
        }
        catch { return false; }

    }



    public async Task<bool> ActualizarPersonaje(string userId, Personaje personajeNuevo)
    {
        try
        {
          
            FirebaseFirestore _context = DatabaseManager.shared.db;

            DocumentReference docRef = _context.Collection("usuarios").Document(userId).Collection("personajes").Document(personajeNuevo.name);

            await docRef.SetAsync(personajeNuevo, SetOptions.MergeAll);

            return true;
        }catch { return false; }

    }

    // subcoleccion de personajes del usuario
    public async Task<Personaje> ObtenerPersonajeDeUsuario(string userId, string nombrePersonaje)
    {
        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;
            DocumentSnapshot snapshot = await _context.Collection("usuarios").Document(userId).Collection("personajes").Document(nombrePersonaje).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Personaje>();
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> ActualizarObjeto(string userId, Objeto objeto)
    {
        try 
        { 
            FirebaseFirestore _context = DatabaseManager.shared.db;

            DocumentReference docRef = _context.Collection("usuarios").Document(userId).Collection("objetos").Document(objeto.name);

            await docRef.SetAsync(objeto, SetOptions.MergeAll);

            return true;
        }
        catch { return false; }

    }

    public async Task<Usuario> ObtenerObjetosUsuario(string userId)
    {
        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;
            DocumentReference docRef = _context.Collection("usuarios").Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // Convertimos el documento al modelo Usuario
                Usuario user = snapshot.ConvertTo<Usuario>();

                // Sincronizamos con el GameManager usando los nombres exactos de tus clases
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.inventarioIDs = user.inventario;
                    GameManager.Instance.pasivoEquipadoID = user.pasivo_equipado;
                    GameManager.Instance.activosEquipadosIDs = user.objetos_equipados;
                }

                return user;
            }
            else
            {
                Debug.LogWarning($"El usuario con ID {userId} no existe en la base de datos.");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al obtener objetos del usuario: {e.Message}");
            return null;
        }
    }

}
