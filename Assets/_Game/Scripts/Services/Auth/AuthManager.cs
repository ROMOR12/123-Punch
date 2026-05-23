using Firebase;
using Firebase.Auth;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    [Header("--- PANELES UI ---")]
    public GameObject panelLoginUI;
    public GameObject panelRegistroUI;
    public GameObject panelOlvidoUI;
    public GameObject panelVerificacionUI;
    public GameObject panelInvitadoUI;
    public GameObject panelSinConexionUI;

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

    [Header("--- UI INVITADO ---")]
    public TMP_InputField usernameInvitadoInput;

    [Header("--- FEEDBACK ---")]
    public TMP_Text feedbackText;

    [Header("--- FADE ---")]
    public SceneFader faderScript;

    private FirebaseAuth _auth;
    private FirebaseAuth auth
    {
        get
        {
            if (_auth == null)
            {
                _auth = FirebaseAuth.DefaultInstance;
            }
            return _auth;
        }
    }

    private bool irAlJuego = false;
    private bool recargarEscena = false;

    // Regex para el correoy la contraseña
    private const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
    private const string MatchPasswordPattern = @"^(?=.*[a-zA-Z])(?=.*\d).{6,}$";

    void Awake()
    {
        _auth = FirebaseAuth.DefaultInstance;
    }

    void Start()
    {
        ChequearConexion();
    }

    public void ChequearConexion()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            CerrarTodosPaneles();
            if (panelSinConexionUI != null) panelSinConexionUI.SetActive(true);
            
            if (feedbackText != null) 
                feedbackText.text = "No hay conexión a internet.";
        }
        else
        {
            IniciarApp();
        }
    }

    void IniciarApp()
    {
        CerrarTodosPaneles();

        // Si ya inicio sesion anteriormente
        FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            if (user.IsEmailVerified || user.IsAnonymous)
            {
                // Cargar datos de forma asíncrona antes de ir al juego sin mostrar el panel de login
                CargarDatosSesion(user.UserId);
            }
            else
            {
                recargarEscena = true;
            }
        }
        else
        {
            // Si no hay usuario logueado, entonces sÃ­ mostramos el panel de login
            panelLoginUI.SetActive(true);
        }
    }

    void Update()
    {
        if (irAlJuego)
        {
            irAlJuego = false;
            CargaEscena.Cargar("Menu");
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
            else if (auth.CurrentUser != null) { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        }
    }
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

    public void IrAInvitado() 
    { 
        CerrarTodosPaneles(); 
        if (panelInvitadoUI != null) panelInvitadoUI.SetActive(true); 
    }

    public void CerrarTodosPaneles()
    {
        if (panelLoginUI != null) panelLoginUI.SetActive(false);
        if (panelRegistroUI != null) panelRegistroUI.SetActive(false);
        if (panelOlvidoUI != null) panelOlvidoUI.SetActive(false);
        if (panelVerificacionUI != null) panelVerificacionUI.SetActive(false);
        if (panelInvitadoUI != null) panelInvitadoUI.SetActive(false);
        if (panelSinConexionUI != null) panelSinConexionUI.SetActive(false);

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
        if (usernameInvitadoInput) usernameInvitadoInput.text = "";
    }

    void LimpiarFeedback() 
    { 
        if (feedbackText != null) feedbackText.text = ""; 
    }

    public void Boton_ChequearVerificacion() 
    { 
        StartCoroutine(ChequearEmailCorrutina()); 
    }

    //Corrutina para comprobar si el correo esta verificado
    IEnumerator ChequearEmailCorrutina()
    {
        feedbackText.text = "Comprobando...";
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            var task = user.ReloadAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            // Si esta verificado
            if (user != null && user.IsEmailVerified)
            {
                feedbackText.text = "¡Verificado!";
                yield return new WaitForSeconds(1f);
                irAlJuego = true;
            }
            else feedbackText.text = "Aún no verificado. Revisa tu correo.";
        }
    }

    private async void CargarDatosSesion(string userId)
    {
        UsuarioService usuarioService = new UsuarioService();
        Usuario datosUsuario = await usuarioService.ObtenerUsuario(userId);

        if (datosUsuario != null)
        {
            SessionManager.shared.currentUser = datosUsuario; if (AchievementManager.Instance != null) { AchievementManager.Instance.LoadAchievementsData(); }
            irAlJuego = true;
        }
        else
        {
            auth.SignOut(); // Si por algún motivo no existe en DB, lo deslogueamos
            CerrarTodosPaneles();
            panelLoginUI.SetActive(true);
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

    public async void LoginConEmail()
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

        try
        {
            // Iniciamos sesión en Firebase Auth
            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, pass);
            FirebaseUser user = result.User;

            if (user != null && user.IsEmailVerified)
            {
                feedbackText.text = "Cargando datos del jugador...";

                // Descargamos los datos desde Firestore
                UsuarioService usuarioService = new UsuarioService();
                Usuario datosUsuario = await usuarioService.ObtenerUsuario(user.UserId);

                if (datosUsuario != null)
                {
                    // Guardamos los datos en la sesión
                    SessionManager.shared.currentUser = datosUsuario; if (AchievementManager.Instance != null) { AchievementManager.Instance.LoadAchievementsData(); }
                    irAlJuego = true;
                }
                else
                {
                    feedbackText.text = "Error: No se encontraron datos del usuario.";
                }
            }
            else
            {
                recargarEscena = true;
            }
        }
        catch (System.AggregateException e)
        {
            feedbackText.text = ObtenerMensajeError(e);
        }
    }

    public async void LoginInvitado()
    {
        string username = usernameInvitadoInput != null ? usernameInvitadoInput.text : "";
        
        if (string.IsNullOrEmpty(username))
        {
            if (feedbackText) feedbackText.text = "Por favor, escribe un nombre de usuario.";
            return;
        }
        
        if (username.Length < 3)
        {
            if (feedbackText) feedbackText.text = "El usuario debe tener mínimo 3 caracteres.";
            return;
        }

        if (feedbackText) feedbackText.text = "Creando cuenta de invitado...";

        try
        {
            // Entramos como usuario anonimo en Firebase Auth
            AuthResult result = await auth.SignInAnonymouslyAsync();
            FirebaseUser newUser = result.User;

            if (newUser != null)
            {
                if (feedbackText) feedbackText.text = "Registrando perfil en la base de datos...";

                // Intentamos insertar el usuario anónimo en Firestore
                UsuarioService usuarioService = new UsuarioService();
                
                // Usamos un correo falso para que la BD no dé problemas de validación de nulos
                string fakeEmail = "invitado_" + newUser.UserId.Substring(0, 5) + "@anon.com";
                bool resultado = await usuarioService.NuevoUsuario(newUser.UserId, username, fakeEmail);

                if (resultado)
                {
                    if (feedbackText) feedbackText.text = "¡Cuenta creada! Entrando al juego...";
                    CargarDatosSesion(newUser.UserId);
                }
                else
                {
                    if (feedbackText) feedbackText.text = "Error al crear el perfil en la base de datos.";
                }
            }
        }
        catch (System.AggregateException e)
        {
            if (feedbackText) feedbackText.text = ObtenerMensajeError(e);
        }
        catch (System.Exception e)
        {
            if (feedbackText) feedbackText.text = e.Message;
        }
    }

    public async void Registrarse()
    {
        // datos
        string usuario = usernameRegisterInput.text;
        string email = emailRegisterInput.text;
        string pass = passRegisterInput.text;
        string passConf = passRegisterConfirmInput.text;

        // unas cuantas validaciones
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

        try
        {
            // Intentamos registrar el usuario en el servicio de autentificación
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, pass);
            // Recogemos el usuario para la base de datos
            FirebaseUser newUser = result.User;


            // Intentamos insertar el usuario en la base de datos
            UsuarioService usuarioService = new UsuarioService();
            bool resultado = await usuarioService.NuevoUsuario(newUser.UserId, usuario, email);

            // Mandamos el correo de veerificacino
            await newUser.SendEmailVerificationAsync();

            recargarEscena = true;
        }
        catch (System.AggregateException e)
        {
            feedbackText.text = ObtenerMensajeError(e);
        }
        catch (System.Exception e)
        {
            feedbackText.text = e.Message;
        }
    }

    public async void EnviarCorreoRecuperacion()
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

        if (auth == null)
        {
            feedbackText.text = "Error: Firebase Auth no está inicializado.";
            return;
        }

        feedbackText.text = "Enviando...";

        try
        {
            // Enviamos el correo de recuperacion de la contraseña
            await auth.SendPasswordResetEmailAsync(email);
            feedbackText.text = "¡Correo enviado! Revisa tu bandeja.";
        }
        catch (System.Exception e)
        {
            System.AggregateException aggregate = e as System.AggregateException;
            if (aggregate != null)
            {
                feedbackText.text = ObtenerMensajeError(aggregate);
            }
            else
            {
                feedbackText.text = "Error: " + e.Message;
            }
        }
    }

    string ObtenerMensajeError(System.AggregateException exception)
    {
        if (exception == null) return "Error desconocido.";

        FirebaseException firebaseEx = null;

        foreach (System.Exception e in exception.Flatten().InnerExceptions)
        {
            if (e is FirebaseException)
            {
                firebaseEx = (FirebaseException)e;
                break;
            }
        }

        // Errores posibles
        if (firebaseEx != null)
        {
            var codigoError = (AuthError)firebaseEx.ErrorCode;

            switch (codigoError)
            {
                case AuthError.EmailAlreadyInUse:
                    return "Ese correo ya está registrado.";

                case AuthError.WrongPassword:
                case AuthError.UserNotFound:
                case AuthError.Failure:
                case AuthError.InvalidCredential:
                    return "El correo o la contraseña son incorrectos.";

                case AuthError.InvalidEmail:
                    return "El formato del correo está mal.";

                case AuthError.MissingEmail:
                    return "Falta escribir el correo.";

                case AuthError.MissingPassword:
                    return "Falta escribir la contraseña.";

                default:
                    return $"Error: {firebaseEx.Message}";
            }
        }
        return "Error de conexión o desconocido: " + exception.Message;
    }
}

