using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using System.Text.RegularExpressions;
using Firebase;
using System.Collections;

public class PanelPerfilUsuarioUI : MonoBehaviour
{
    private const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";
    private const string MatchPasswordPattern = @"^(?=.*[a-zA-Z])(?=.*\d).{6,}$";
    [Header("--- UI Textos ---")]
    public TextMeshProUGUI txtNombreUsuario;
    public TextMeshProUGUI txtNivelActual;
    public TextMeshProUGUI txtProgresoXP;
    public TextMeshProUGUI txtXPRestante; // Nuevo texto para la XP faltante

    [Header("--- UI GrÃ¡ficos ---")]
    public Slider sliderXP;

    [Header("--- Botones de Acceso Rápido ---")]
    public Button btnLogros;

    [Header("--- Vinculación de Cuentas ---")]
    public Button btnAbrirVincular;
    public TextMeshProUGUI txtBtnVincular;
    public GameObject panelVincular;
    public TMP_InputField inputEmailVincular;
    public TMP_InputField inputPassVincular;
    public TMP_InputField inputPassConfirmVincular;
    public Button btnConfirmarVincular;
    public Button btnCerrarVincular;
    public TextMeshProUGUI txtFeedbackVincular;

    [Header("--- Verificación de Vinculación ---")]
    public GameObject panelVerificacionVincular;
    public Button btnChequearVerificacionVincular;
    public Button btnCerrarVerificacionVincular;
    public Button btnReenviarCorreoVincular;

    private void Start()
    {
        if (btnLogros != null) btnLogros.onClick.AddListener(OnClickLogros);

        if (btnAbrirVincular != null) btnAbrirVincular.onClick.AddListener(OnClickAbrirVincular);
        if (btnConfirmarVincular != null) btnConfirmarVincular.onClick.AddListener(OnClickConfirmarVincular);
        if (btnCerrarVincular != null) btnCerrarVincular.onClick.AddListener(OnClickCerrarVincular);
        if (btnChequearVerificacionVincular != null) btnChequearVerificacionVincular.onClick.AddListener(OnClickChequearVerificacionVincular);
        if (btnCerrarVerificacionVincular != null) btnCerrarVerificacionVincular.onClick.AddListener(OnClickCerrarVerificacionVincular);
        if (btnReenviarCorreoVincular != null) btnReenviarCorreoVincular.onClick.AddListener(OnClickReenviarCorreoVincular);
    }

    private void OnEnable()
    {
        ActualizarDatosPerfil();
    }

    public void ActualizarDatosPerfil()
    {
        if (SessionManager.shared == null || SessionManager.shared.currentUser == null)
            return;

        Usuario user = SessionManager.shared.currentUser;

        if (txtNombreUsuario != null)
            txtNombreUsuario.text = user.username;

        int xpTotal = user.xp;
        int nivelActual = LevelSystem.CalcularNivel(xpTotal);
        int xpDentroDelNivel = LevelSystem.XPEnNivelActual(xpTotal);
        int xpNecesaria = LevelSystem.XPNecesariaParaSiguienteNivel(xpTotal);
        int xpFaltante = xpNecesaria - xpDentroDelNivel;
        float progreso = LevelSystem.ProgresoNivel(xpTotal);

        if (txtNivelActual != null)
            txtNivelActual.text = "Nvl. " + nivelActual;

        if (txtProgresoXP != null)
            txtProgresoXP.text = $"{xpDentroDelNivel} / {xpNecesaria} XP";
            
        if (txtXPRestante != null)
            txtXPRestante.text = $"Faltan {xpFaltante} XP";

        if (sliderXP != null)
        {
            sliderXP.minValue = 0;
            sliderXP.maxValue = xpNecesaria;
            sliderXP.value = xpDentroDelNivel;
        }

        if (FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            if (FirebaseAuth.DefaultInstance.CurrentUser.IsAnonymous)
            {
                if (btnAbrirVincular != null) btnAbrirVincular.interactable = true;
                if (txtBtnVincular != null) txtBtnVincular.text = "Vincular Cuenta";
            }
            else
            {
                if (btnAbrirVincular != null) btnAbrirVincular.interactable = false;
                if (txtBtnVincular != null) txtBtnVincular.text = "Cuenta Vinculada";
            }
        }
    }

    private void OnClickLogros()
    {
        Debug.Log("[Perfil] Abriendo panel de logros... (En construcciÃ³n)");
    }

    private void OnClickAbrirVincular()
    {
        if (panelVincular != null) panelVincular.SetActive(true);
        if (txtFeedbackVincular != null) txtFeedbackVincular.text = "";
        if (inputEmailVincular != null) inputEmailVincular.text = "";
        if (inputPassVincular != null) inputPassVincular.text = "";
        if (inputPassConfirmVincular != null) inputPassConfirmVincular.text = "";
    }

    private void OnClickCerrarVincular()
    {
        if (panelVincular != null) panelVincular.SetActive(false);
    }

    private async void OnClickConfirmarVincular()
    {
        if (inputEmailVincular == null || inputPassVincular == null || inputPassConfirmVincular == null) return;

        string email = inputEmailVincular.text;
        string pass = inputPassVincular.text;
        string passConf = inputPassConfirmVincular.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(passConf))
        {
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Rellena todos los campos.";
            return;
        }

        if (!EsEmailValido(email))
        {
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = "El formato del correo no es válido.";
            return;
        }

        if (!EsPasswordValida(pass))
        {
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = "La contraseña necesita 6 caracteres, 1 número y 1 letra.";
            return;
        }

        if (pass != passConf)
        {
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Las contraseñas no coinciden.";
            return;
        }

        if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Vinculando...";
        if (btnConfirmarVincular != null) btnConfirmarVincular.interactable = false;

        try
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            Credential credential = EmailAuthProvider.GetCredential(email, pass);
            
            await user.LinkWithCredentialAsync(credential);
            
            // Si funciona, actualizamos la base de datos
            UsuarioService usuarioService = new UsuarioService();
            await usuarioService.ActualizarEmail(user.UserId, email);

            // Mandar correo verificacion
            await user.SendEmailVerificationAsync();

            if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Comprueba tu correo y pulsa Verificar.";
            
            // Actualizar boton perfil
            ActualizarDatosPerfil();
            
            // Ocultar panel vincular y abrir panel verificacion
            if (panelVincular != null) panelVincular.SetActive(false);
            if (panelVerificacionVincular != null) panelVerificacionVincular.SetActive(true);
        }
        catch (System.AggregateException e)
        {
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = ObtenerMensajeError(e);
        }
        catch (System.Exception e)
        {
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = e.Message;
        }
        finally
        {
            if (btnConfirmarVincular != null) btnConfirmarVincular.interactable = true;
        }
    }

    private void OnClickChequearVerificacionVincular()
    {
        StartCoroutine(ChequearEmailCorrutina());
    }

    private void OnClickCerrarVerificacionVincular()
    {
        if (panelVerificacionVincular != null) panelVerificacionVincular.SetActive(false);
    }

    private void OnClickReenviarCorreoVincular()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            FirebaseAuth.DefaultInstance.CurrentUser.SendEmailVerificationAsync();
            if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Correo reenviado.";
        }
    }

    private IEnumerator ChequearEmailCorrutina()
    {
        if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Comprobando...";
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user != null)
        {
            var task = user.ReloadAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (user != null && user.IsEmailVerified)
            {
                if (txtFeedbackVincular != null) txtFeedbackVincular.text = "¡Verificado con éxito!";
                yield return new WaitForSeconds(1f);
                if (panelVerificacionVincular != null) panelVerificacionVincular.SetActive(false);
            }
            else
            {
                if (txtFeedbackVincular != null) txtFeedbackVincular.text = "Aún no verificado. Revisa tu correo.";
            }
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
                case AuthError.WrongPassword:
                case AuthError.UserNotFound:
                case AuthError.Failure:
                case AuthError.InvalidCredential: return "El correo o la contraseña son incorrectos.";
                case AuthError.InvalidEmail: return "El formato del correo está mal.";
                case AuthError.MissingEmail: return "Falta escribir el correo.";
                case AuthError.MissingPassword: return "Falta escribir la contraseña.";
                default: return $"Error: {firebaseEx.Message}";
            }
        }
        return "Error de conexión o desconocido: " + exception.Message;
    }
}
