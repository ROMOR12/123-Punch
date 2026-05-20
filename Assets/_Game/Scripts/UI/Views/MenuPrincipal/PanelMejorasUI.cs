using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PanelMejorasUI : MonoBehaviour
{
    [Header("Referencias a Scripts")]
    public MejoraStatsManager mejoraManager;
    private UsuarioService usuarioService = new UsuarioService();

    [Header("Textos UI - Stats")]
    public TextMeshProUGUI txtVida;
    public TextMeshProUGUI txtEnergia;
    public TextMeshProUGUI txtFuerza;
    public TextMeshProUGUI txtRecuperacion;

    [Header("Textos UI - Precios y Monedas")]
    public TextMeshProUGUI txtMonedasJugador;
    public TextMeshProUGUI txtCosteTotal;
    public TextMeshProUGUI txtCosteVidaProx;
    public TextMeshProUGUI txtCosteEnergiaProx;
    public TextMeshProUGUI txtCosteFuerzaProx;
    public TextMeshProUGUI txtCosteRecuperacionProx;

    [Header("Botones Principales")]
    public GameObject contenedorBotonesConfirmacion;
    public Button btnConfirmar;
    public Button btnDescartar;

    // Variables internas
    private Usuario usuarioReal;
    private Personaje personajeReal;

    private Personaje pjTemp;
    private int costeAcumulado = 0;

    private bool estaInicializado = false;

    private void Start()
    {
        estaInicializado = true;
        CargarDatosAutomaticamente();
    }

    private void OnEnable()
    {
        // Evitamos que OnEnable se ejecute la primera vez antes que GameManager.Awake()
        if (estaInicializado)
        {
            CargarDatosAutomaticamente();
        }
    }

    public async void CargarDatosAutomaticamente()
    {
        // 1. Buscamos al usuario en tu SessionManager (como haces en AuthManager)
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            usuarioReal = SessionManager.shared.currentUser;

            // 2. Buscamos qué personaje tiene seleccionado en el GameManager
            string idPersonaje = GameManager.Instance.idPersonajeSeleccionado;

            Debug.Log($"Usuario {usuarioReal.username} encontrado. Descargando stats de {idPersonaje}...");

            // 3. Descargamos el personaje directamente de Firebase usando tu servicio
            personajeReal = await usuarioService.ObtenerPersonajeDeUsuario(usuarioReal.id, idPersonaje);

            // Si no existe en el usuario, cargamos las stats base globales
            if (personajeReal == null)
            {
                Debug.LogWarning($"El personaje '{idPersonaje}' no está en la base de datos del usuario. Cargando stats base...");
                PersonajeService pjServiceGlobal = new PersonajeService();
                personajeReal = await pjServiceGlobal.ObtenerPersonaje(idPersonaje);
                
                // IMPORTANTE: Aseguramos que el ID no sea nulo si Firebase no lo incluye en el documento base
                if (personajeReal != null)
                {
                    personajeReal.id = idPersonaje;
                }
            }

            if (personajeReal != null)
            {
                Debug.Log($"¡Todo listo! Monedas: {usuarioReal.free_coin}, Nivel del personaje: {personajeReal.NivelTotal}");
                ReiniciarCarrito();
            }
            else
            {
                Debug.LogError($"Error: No se encontró el personaje '{idPersonaje}' ni en el usuario ni en las stats base.");
            }
        }
        else
        {
            Debug.LogError("Error: El SessionManager está vacío. ¿Iniciaste sesión desde la pantalla de Login?");
        }
    }

    // --- 1. INICIALIZAR EL PANEL ---
    // IMPORTANTE: Tienes que llamar a esto desde tu menú de selección para pasarle los datos reales
    public void AbrirPanel(Usuario user, Personaje pj)
    {
        if (user == null || pj == null)
        {
            Debug.LogError("¡ERROR! Le estás intentando pasar un usuario o personaje VACÍO (null) al panel.");
            return;
        }

        usuarioReal = user;
        personajeReal = pj;

        Debug.Log($"Abriendo panel para {pj.name}. Monedas actuales del usuario: {user.free_coin}");

        ReiniciarCarrito();
        gameObject.SetActive(true);
    }

    // --- 2. LÓGICA DE LOS BOTONES DE "+" ---
    public void BtnAumentarVida() { SimularMejora(StatType.Life); }
    public void BtnAumentarEnergia() { SimularMejora(StatType.Energy); }
    public void BtnAumentarFuerza() { SimularMejora(StatType.Force); }
    public void BtnAumentarRecuperacion() { SimularMejora(StatType.Recovery); }

    private void SimularMejora(StatType stat)
    {
        if (usuarioReal == null || pjTemp == null)
        {
            Debug.LogError("No puedes mejorar porque no se ha cargado ningún usuario o personaje. ¿Llamaste a AbrirPanel()?");
            return;
        }

        if (mejoraManager == null)
        {
            Debug.LogError("¡Falta arrastrar el MejoraStatsManager al slot del Inspector!");
            return;
        }

        int costeProxNivel = mejoraManager.CalcularCoste(stat, pjTemp);
        Debug.Log($"Intentando mejorar {stat}. Coste próximo nivel: {costeProxNivel}. Monedas disponibles: {usuarioReal.free_coin - costeAcumulado}");

        if (usuarioReal.free_coin >= (costeAcumulado + costeProxNivel))
        {
            costeAcumulado += costeProxNivel;

            switch (stat)
            {
                case StatType.Life:
                    pjTemp.mejoras_vida++;
                    pjTemp.life += mejoraManager.aumentoVida;
                    break;
                case StatType.Energy:
                    pjTemp.mejoras_energia++;
                    pjTemp.energy += mejoraManager.aumentoEnergia;
                    break;
                case StatType.Force:
                    pjTemp.mejoras_fuerza++;
                    pjTemp.force += mejoraManager.aumentoFuerza;
                    break;
                case StatType.Recovery:
                    pjTemp.mejoras_recuperacion++;
                    pjTemp.recovery += mejoraManager.aumentoRecuperacion;
                    break;
            }

            Debug.Log($"Simulación de {stat} exitosa. Nuevo coste acumulado: {costeAcumulado}");
            ActualizarTextos();
        }
        else
        {
            Debug.LogWarning("Monedas insuficientes para comprar esta mejora.");
        }
    }

    // --- 3. CONFIRMAR Y DESCARTAR ---
    public async void BotonConfirmar()
    {
        if (costeAcumulado == 0)
        {
            Debug.Log("No hay ninguna mejora en el carrito para confirmar.");
            return;
        }

        Debug.Log($"Confirmando compra. Gastando {costeAcumulado} monedas de {usuarioReal.free_coin} totales.");

        btnConfirmar.interactable = false;
        btnDescartar.interactable = false;

        usuarioReal.free_coin -= costeAcumulado;

        bool userOk = await usuarioService.ActualizarUsuario(usuarioReal);
        bool personajeOk = await usuarioService.ActualizarPersonaje(usuarioReal.id, pjTemp);

        if (userOk && personajeOk)
        {
            Debug.Log("¡Firebase actualizado correctamente!");
            personajeReal = pjTemp;
            ReiniciarCarrito();
        }
        else
        {
            Debug.LogError("Error de red al guardar en Firebase. Devolviendo monedas...");
            usuarioReal.free_coin += costeAcumulado;
            ActualizarTextos();
        }

        btnConfirmar.interactable = true;
        btnDescartar.interactable = true;
    }

    public void BotonDescartar()
    {
        Debug.Log("Mejoras descartadas por el usuario.");
        ReiniciarCarrito();
    }

    // --- 4. UTILIDADES ---
    private void ReiniciarCarrito()
    {
        costeAcumulado = 0;

        pjTemp = new Personaje
        {
            id = personajeReal.id,
            name = personajeReal.name,
            life = personajeReal.life,
            energy = personajeReal.energy,
            force = personajeReal.force,
            recovery = personajeReal.recovery,
            mejoras_vida = personajeReal.mejoras_vida,
            mejoras_energia = personajeReal.mejoras_energia,
            mejoras_fuerza = personajeReal.mejoras_fuerza,
            mejoras_recuperacion = personajeReal.mejoras_recuperacion
        };

        ActualizarTextos();
    }

    private void ActualizarTextos()
    {
        // Comprobación de seguridad por si te olvidaste de arrastrar algún texto en el inspector
        if (txtVida == null || txtEnergia == null || txtFuerza == null || txtRecuperacion == null || txtMonedasJugador == null || txtCosteTotal == null)
        {
            Debug.LogError("¡ERROR GRAVE! Te has olvidado de arrastrar uno o varios Textos (TextMeshPro) al Inspector del script.");
            return;
        }

        txtVida.text = pjTemp.life.ToString();
        txtVida.color = pjTemp.life > personajeReal.life ? Color.green : Color.white;

        txtEnergia.text = pjTemp.energy.ToString();
        txtEnergia.color = pjTemp.energy > personajeReal.energy ? Color.green : Color.white;

        txtFuerza.text = pjTemp.force.ToString();
        txtFuerza.color = pjTemp.force > personajeReal.force ? Color.green : Color.white;

        txtRecuperacion.text = pjTemp.recovery.ToString();
        txtRecuperacion.color = pjTemp.recovery > personajeReal.recovery ? Color.green : Color.white;

        txtMonedasJugador.text = usuarioReal.free_coin.ToString();

        if (txtCosteVidaProx != null) txtCosteVidaProx.text = mejoraManager.CalcularCoste(StatType.Life, pjTemp).ToString();
        if (txtCosteEnergiaProx != null) txtCosteEnergiaProx.text = mejoraManager.CalcularCoste(StatType.Energy, pjTemp).ToString();
        if (txtCosteFuerzaProx != null) txtCosteFuerzaProx.text = mejoraManager.CalcularCoste(StatType.Force, pjTemp).ToString();
        if (txtCosteRecuperacionProx != null) txtCosteRecuperacionProx.text = mejoraManager.CalcularCoste(StatType.Recovery, pjTemp).ToString();

        if (costeAcumulado > 0)
        {
            txtCosteTotal.text = $"-{costeAcumulado}";
            txtCosteTotal.color = Color.red;
            if (contenedorBotonesConfirmacion != null) contenedorBotonesConfirmacion.SetActive(true);
        }
        else
        {
            txtCosteTotal.text = "0";
            txtCosteTotal.color = Color.white;
            if (contenedorBotonesConfirmacion != null) contenedorBotonesConfirmacion.SetActive(false);
        }
    }
}
