using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public static SessionManager shared;

    public Usuario currentUser;

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

    public void LimpiarSesion()
    {
        currentUser = null;
    }
}
