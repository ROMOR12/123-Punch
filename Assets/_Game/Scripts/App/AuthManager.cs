using UnityEngine;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    [Header("--- PANELES UI ---")]
    public GameObject panelLoginUI;
    public GameObject panelRegistroUI;

    [Header("--- UI LOGIN ---")]
    public TMP_InputField emailLoginInput;
    public TMP_InputField passLoginInput;

    [Header("--- UI REGISTRO ---")]
    public TMP_InputField usernameRegisterInput;
    public TMP_InputField emailRegisterInput;
    public TMP_InputField passRegisterInput;
    public TMP_InputField passRegisterConfirmInput;

    [Header("--- FEEDBACK ---")]
    public TMP_Text feedbackText;

    private FirebaseAuth auth;
    private bool cambiarDeEscena = false;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    void Start()
    {
        // iniciamos en el panel de login
        IrALogin();

        // si ya hay una sesión iniciada, vamos al menú principal
        if (auth.CurrentUser != null)
        {
            Debug.Log("Sesión encontrada: " + auth.CurrentUser.UserId);
            if (feedbackText != null) feedbackText.text = "Iniciando sesión...";
            cambiarDeEscena = true;
        }
    }

    void Update()
    {
        // Unity solo permite cambiar de escena desde el hilo principal (Update)
        if (cambiarDeEscena)
        {
            cambiarDeEscena = false;
            //SceneManager.LoadScene(""); aqui hay que poner el nombre de la escena del menu principal
        }
    }

    // navegación entre paneles 
    public void IrARegistro()
    {
        panelLoginUI.SetActive(false);
        panelRegistroUI.SetActive(true);
        LimpiarFeedback();
    }

    public void IrALogin()
    {
        panelRegistroUI.SetActive(false);
        panelLoginUI.SetActive(true);
        LimpiarFeedback();
    }

    // limpiar mensajes de feedback
    void LimpiarFeedback()
    {
        if (feedbackText != null) feedbackText.text = "";
    }

    // login con email y contraseña
    public void LoginConEmail()
    {
        string email = emailLoginInput.text;
        string pass = passLoginInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            feedbackText.text = "Rellena correo y contraseña";
            return;
        }

        feedbackText.text = "Verificando...";

        auth.SignInWithEmailAndPasswordAsync(email, pass).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error Login: " + task.Exception);
                // Usamos un dispatcher simple o esperamos al usuario para ver el error en consola
                // Nota: No se puede tocar UI compleja desde aquí sin Dispatcher, 
                // pero cambiar escenas con el bool sí funciona.
                return;
            }

            // Usuario logueado
            cambiarDeEscena = true;
        });
    }

    // iniciar sesion como invitado
    public void LoginInvitado()
    {
        feedbackText.text = "Entrando como invitado...";

        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error Invitado: " + task.Exception);
                return;
            }
            cambiarDeEscena = true;
        });
    }

    // registro con email y contraseña
    public void Registrarse()
    {
        string usuario = usernameRegisterInput.text;
        string email = emailRegisterInput.text;
        string pass = passRegisterInput.text;
        string passConf = passRegisterConfirmInput.text;

        // Validaciones
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            feedbackText.text = "Rellena todos los campos";
            return;
        }

        if (pass != passConf)
        {
            feedbackText.text = "Las contraseñas no coinciden";
            return;
        }

        feedbackText.text = "Creando cuenta...";

        auth.CreateUserWithEmailAndPasswordAsync(email, pass).ContinueWith((Task<AuthResult> task) =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Error Registro: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;

            Debug.Log("Usuario creado: " + newUser.UserId);

            // AQUI PONDREMOS LA BASE DE DATOS LUEGO

            // Usuario registrado y logueado
            cambiarDeEscena = true;
        });
    }
}
