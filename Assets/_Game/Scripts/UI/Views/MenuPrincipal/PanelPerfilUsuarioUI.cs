using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

public class PanelPerfilUsuarioUI : MonoBehaviour
{
    [Header("--- UI Textos ---")]
    public TextMeshProUGUI txtNombreUsuario;
    public TextMeshProUGUI txtNivelActual;
    public TextMeshProUGUI txtProgresoXP;

    [Header("--- UI GrÃ¡ficos ---")]
    public Slider sliderXP;

    [Header("--- Botones de Acceso RÃ¡pido ---")]
    public Button btnLogros;
    public Button btnPaseBatalla;
    public Button btnCerrarSesion;

    private void Start()
    {
        if (btnLogros != null) btnLogros.onClick.AddListener(OnClickLogros);
        if (btnPaseBatalla != null) btnPaseBatalla.onClick.AddListener(OnClickPaseBatalla);
        if (btnCerrarSesion != null) btnCerrarSesion.onClick.AddListener(OnClickCerrarSesion);
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
        float progreso = LevelSystem.ProgresoNivel(xpTotal);

        if (txtNivelActual != null)
            txtNivelActual.text = "Nvl. " + nivelActual;

        if (txtProgresoXP != null)
            txtProgresoXP.text = $"{xpDentroDelNivel} / {xpNecesaria} XP";

        if (sliderXP != null)
        {
            sliderXP.minValue = 0;
            sliderXP.maxValue = xpNecesaria;
            sliderXP.value = xpDentroDelNivel;
        }
    }

    private void OnClickLogros()
    {
        Debug.Log("[Perfil] Abriendo panel de logros... (En construcciÃ³n)");
    }

    private void OnClickPaseBatalla()
    {
        Debug.Log("[Perfil] Abriendo pase de batalla... (En construcciÃ³n)");
    }

    private void OnClickCerrarSesion()
    {
        Debug.Log("[Perfil] Cerrando sesiÃ³n...");
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }

        if (SessionManager.shared != null)
        {
            SessionManager.shared.LimpiarSesion();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Login"); 
    }
}
