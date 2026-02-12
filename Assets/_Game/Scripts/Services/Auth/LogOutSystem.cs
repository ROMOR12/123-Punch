using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogOutSystem : MonoBehaviour
{
    public void cerrarSesion()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        CargaEscena.Cargar("LoginScene");

    }   
}
