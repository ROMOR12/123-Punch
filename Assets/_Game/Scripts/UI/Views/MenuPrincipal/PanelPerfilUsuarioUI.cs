using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;

public class PanelPerfilUsuarioUI : MonoBehaviour
{
    [Header("--- UI Textos ---")]
    public TextMeshProUGUI txtNombreUsuario;
    public TextMeshProUGUI txtNivelActual;
    public TextMeshProUGUI txtProgresoXP; // Ejemplo: 150 / 300 XP

    [Header("--- UI Gráficos ---")]
    public Slider sliderXP;

    [Header("--- Botones de Acceso Rápido ---")]
    public Button btnLogros;
    public Button btnPaseBatalla;
    public Button btnCerrarSesion;

    private void Start()
    {
        // Suscribir eventos a los botones
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

        // Nombre
        if (txtNombreUsuario != null)
            txtNombreUsuario.text = user.username;

        // --- SISTEMA DE NIVELES (Usando LevelSystem) ---
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
            // Alternativamente puedes usar sliderXP.value = progreso si el min/max del slider es 0/1
        }
    }

    private void OnClickLogros()
    {
        Debug.Log("[Perfil] Abriendo panel de logros... (En construcción)");
        // TODO: Mostrar popup de "Próximamente" o abrir el panel de logros reales
    }

    private void OnClickPaseBatalla()
    {
        Debug.Log("[Perfil] Abriendo pase de batalla... (En construcción)");
        // TODO: Abrir el sistema de Battle Pass
    }

    private void OnClickCerrarSesion()
    {
        Debug.Log("[Perfil] Cerrando sesión...");
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }

        if (SessionManager.shared != null)
        {
            SessionManager.shared.LimpiarSesion();
        }

        // Cargar escena de Login (o recargar la actual si el AuthManager ya lo gestiona al inicio)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login"); 
        // Cambia "Login" por el nombre de tu escena inicial si se llama distinto.
    }
}
