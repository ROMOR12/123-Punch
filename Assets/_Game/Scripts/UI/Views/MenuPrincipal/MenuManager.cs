using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI Referencias")]
    public TMP_Text textoNombreUsuario;

    void Start()
    {
        MostrarNombre();

        SincronizaNivel();
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

    public void SincronizaNivel()
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            int nivelBD = int.Parse(SessionManager.shared.currentUser.id_level);

            if (nivelBD > 0)
            {
                GameManager.Instance.nivelMaximoDesbloqueado = nivelBD;
            }
        }
    }
}
