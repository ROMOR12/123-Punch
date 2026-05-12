using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

// Script para cerrar sesion
public class LogOutSystem : MonoBehaviour
{
    public void cerrarSesion()
    {
        FirebaseAuth.DefaultInstance.SignOut();

        // Limpiamos los datos del jugador de la memoria
        if (SessionManager.shared != null)
        {
            SessionManager.shared.LimpiarSesion();
        }

        CargaEscena.Cargar("LoginScene");
    }
}
