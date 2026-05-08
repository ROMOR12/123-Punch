using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Referencias")]
    public TMP_Text textoNombreUsuario;

    void Start()
    {
        MostrarNombre();
    }

    public void MostrarNombre()
    {
        // Verificamos que el SessionManager tenga un usuario cargado
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            // Accedemos a la propiedad 'username' de la clase Usuario
            string nombre = SessionManager.shared.currentUser.username;

            // Lo mostramos en pantalla
            if (textoNombreUsuario != null)
            {
                textoNombreUsuario.text = nombre;
            }
        }
        else
        {
            // Si por algún motivo no hay sesión, ponemos un nombre genérico
            if (textoNombreUsuario != null) textoNombreUsuario.text = "Jugador";
        }
    }
}
