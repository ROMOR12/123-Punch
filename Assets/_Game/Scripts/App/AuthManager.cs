using Firebase;
using Firebase.Auth;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class AuthManager : MonoBehaviour
{
    [Header("--- PANELES UI ---")]
    public GameObject panelLoginUI;
    public GameObject panelRegistroUI;
    public GameObject panelOlvidoUI;
    public GameObject panelVerificacionUI;

    [Header("--- UI LOGIN ---")]
    public TMP_InputField emailLoginInput;
    public TMP_InputField passLoginInput;

    [Header("--- UI REGISTRO ---")]
    public TMP_InputField usernameRegisterInput;
    public TMP_InputField emailRegisterInput;
    public TMP_InputField passRegisterInput;
    public TMP_InputField passRegisterConfirmInput;

    [Header("--- UI OLVIDO ---")]
    public TMP_InputField emailOlvidoInput;

    [Header("--- FEEDBACK ---")]
    public TMP_Text feedbackText;

    private FirebaseAuth auth;
    private bool irAlJuego = false;
    private bool recargarEscena = false;

    // REGEX PATTERNS
    private const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
    private const string MatchPasswordPattern = @"^(?=.*[a-zA-Z])(?=.*\d).{6,}$";

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    void Start()
    {
        CerrarTodosPaneles();
        panelLoginUI.SetActive(true);

        if (auth.CurrentUser != null)
        {
            auth.CurrentUser.ReloadAsync().ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    auth.SignOut();
                    return;
                }

                FirebaseUser user = auth.CurrentUser;
                if (user.IsEmailVerified || user.IsAnonymous) irAlJuego = true;
                else recargarEscena = true;
            });
        }
    }

    void Update()
    {
        if (irAlJuego)
        {
            irAlJuego = false;
            SceneManager.LoadScene("Assets/_Game/Scenes/Combat/CombateDePrueba.unity");
        }

        if (recargarEscena)
        {
            recargarEscena = false;
            if (auth.CurrentUser != null && !auth.CurrentUser.IsEmailVerified)
            {
                CerrarTodosPaneles();
                panelVerificacionUI.SetActive(true);
                if (feedbackText) feedbackText.text = "Verifica tu correo para continuar.";
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    // --- VALIDACIONES ---
    bool EsEmailValido(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return Regex.IsMatch(email, MatchEmailPattern);
    }

    bool EsPasswordValida(string pass)
    {
        if (string.IsNullOrEmpty(pass)) return false;
        return Regex.IsMatch(pass, MatchPasswordPattern);
    }

    public void IrARegistro() 
    {
        CerrarTodosPaneles(); 
        panelRegistroUI.SetActive(true); 
    }
    public void IrALogin() 
    { 
        CerrarTodosPaneles(); 
        panelLoginUI.SetActive(true); 
    }
    public void IrAOlvido() 
    { 
        CerrarTodosPaneles(); 
        panelOlvidoUI.SetActive(true); 
    }

    public void CerrarTodosPaneles()
    {
        panelLoginUI.SetActive(false);
        panelRegistroUI.SetActive(false);
        panelOlvidoUI.SetActive(false);
        panelVerificacionUI.SetActive(false);

        LimpiarFeedback();
        LimpiarInputs();
    }

    void LimpiarInputs()
    {
        if (emailLoginInput) emailLoginInput.text = "";
        if (passLoginInput) passLoginInput.text = "";

        if (usernameRegisterInput) usernameRegisterInput.text = "";
        if (emailRegisterInput) emailRegisterInput.text = "";
        if (passRegisterInput) passRegisterInput.text = "";
        if (passRegisterConfirmInput) passRegisterConfirmInput.text = "";

        if (emailOlvidoInput) emailOlvidoInput.text = "";
    }

    void LimpiarFeedback() 
    { 
        if (feedbackText != null) feedbackText.text = ""; 
    }

    public void Boton_ChequearVerificacion() 
    { 
        StartCoroutine(ChequearEmailCorrutina()); 
    }

    IEnumerator ChequearEmailCorrutina()
    {
        feedbackText.text = "Comprobando...";
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            var task = user.ReloadAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (user.IsEmailVerified)
            {
                feedbackText.text = "¡Verificado!";
                yield return new WaitForSeconds(1f);
                irAlJuego = true;
            }
            else feedbackText.text = "Aún no verificado. Revisa tu correo.";
        }
    }

    public void Boton_ReenviarCorreo()
    {
        if (auth.CurrentUser != null)
        {
            auth.CurrentUser.SendEmailVerificationAsync();
            feedbackText.text = "Correo reenviado.";
        }
    }

    public void Boton_CancelarVerificacion()
    {
        auth.SignOut();
        IrALogin();
    }

    // --- LOGIN ---
    public void LoginConEmail()
    {
        string email = emailLoginInput.text;
        string pass = passLoginInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            feedbackText.text = "Rellena correo y contraseña";
            return;
        }

        if (!EsEmailValido(email))
        {
            feedbackText.text = "El formato del correo no es válido.";
            return;
        }

        feedbackText.text = "Verificando...";

        auth.SignInWithEmailAndPasswordAsync(email, pass).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string errorMsg = ObtenerMensajeError(task.Exception);
                UnityMainThreadDispatcher.Instance().Enqueue(() => { feedbackText.text = errorMsg; });
                return;
            }

            FirebaseUser user = task.Result.User;
            if (user.IsEmailVerified) irAlJuego = true;
            else recargarEscena = true;
        });
    }

    public void LoginInvitado()
    {
        feedbackText.text = "Entrando como invitado...";
        auth.SignInAnonymouslyAsync().ContinueWith(task => { if (!task.IsFaulted) irAlJuego = true; });
    }

    // --- REGISTRO ---
    public void Registrarse()
    {
        string usuario = usernameRegisterInput.text;
        string email = emailRegisterInput.text;
        string pass = passRegisterInput.text;
        string passConf = passRegisterConfirmInput.text;

        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            feedbackText.text = "Rellena todos los campos";
            return;
        }

        if (usuario.Length < 3)
        {
            feedbackText.text = "El usuario debe tener mínimo 3 caracteres.";
            return;
        }

        if (!EsEmailValido(email))
        {
            feedbackText.text = "El formato del correo no es válido.";
            return;
        }

        if (!EsPasswordValida(pass))
        {
            feedbackText.text = "La contraseña necesita 6 caracteres, 1 número y 1 letra.";
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
                string errorMsg = ObtenerMensajeError(task.Exception);
                UnityMainThreadDispatcher.Instance().Enqueue(() => { feedbackText.text = errorMsg; });
                return;
            }

            FirebaseUser newUser = task.Result.User;
            newUser.SendEmailVerificationAsync();
            recargarEscena = true;
        });
    }

    // --- OLVIDO ---
    public void EnviarCorreoRecuperacion()
    {
        string email = emailOlvidoInput.text;

        if (string.IsNullOrEmpty(email))
        {
            feedbackText.text = "Escribe tu correo primero.";
            return;
        }

        if (!EsEmailValido(email))
        {
            feedbackText.text = "El formato del correo no es válido.";
            return;
        }

        feedbackText.text = "Enviando...";
        auth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string errorMsg = ObtenerMensajeError(task.Exception);
                UnityMainThreadDispatcher.Instance().Enqueue(() => { feedbackText.text = errorMsg; });
                return;
            }

            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                feedbackText.text = "¡Correo enviado! Revisa tu bandeja.";
            });
        });
    }

    // --- ERRORES ---
    string ObtenerMensajeError(System.AggregateException exception)
    {
        if (exception == null) return "Error desconocido.";
        FirebaseException firebaseEx = null;
        foreach (System.Exception e in exception.Flatten().InnerExceptions)
        {
            if (e is FirebaseException) { firebaseEx = (FirebaseException)e; break; }
        }

        if (firebaseEx != null)
        {
            var codigoError = (AuthError)firebaseEx.ErrorCode;
            switch (codigoError)
            {
                case AuthError.EmailAlreadyInUse: return "Ese correo ya está registrado.";
                case AuthError.WrongPassword: return "Contraseña incorrecta.";
                case AuthError.UserNotFound: return "Esa cuenta no existe.";
                default: return "Error: " + codigoError.ToString();
            }
        }
        return "Error de conexión o desconocido.";
    }
}
