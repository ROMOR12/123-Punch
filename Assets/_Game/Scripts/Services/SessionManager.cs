using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private static SessionManager _shared;
    public static SessionManager shared
    {
        get
        {
            if (_shared == null)
            {
                _shared = FindFirstObjectByType<SessionManager>();
                if (_shared == null)
                {
                    GameObject go = new GameObject("SessionManager (Lazy)");
                    _shared = go.AddComponent<SessionManager>();
                    
                    #if UNITY_EDITOR
                    if (_shared.currentUser == null)
                    {
                        _shared.currentUser = new Usuario
                        {
                            id = "mock_user_id",
                            username = "EditorPlayer",
                            free_coin = 5000,
                            email = "editor@test.com"
                        };
                        Debug.LogWarning("[SessionManager] Creado SessionManager temporal con usuario ficticio para pruebas en el Editor.");
                    }
                    #endif
                }
            }
            return _shared;
        }
        set
        {
            _shared = value;
        }
    }

    public Usuario currentUser;

    void Awake()
    {
        if (_shared == null)
        {
            _shared = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_shared != this)
        {
            Destroy(gameObject);
        }
    }

    public void LimpiarSesion()
    {
        currentUser = null;
    }
}
