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
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            string nombre = SessionManager.shared.currentUser.username;

            if (textoNombreUsuario != null)
            {
                textoNombreUsuario.text = nombre;
            }
        }
        else
        {
            if (textoNombreUsuario != null) textoNombreUsuario.text = "Jugador";
        }
    }
}
