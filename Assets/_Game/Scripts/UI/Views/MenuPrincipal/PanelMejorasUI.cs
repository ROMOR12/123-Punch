using UnityEngine;
using TMPro;
using UnityEngine.UI;

// esta clase controla la interfaz visual de mejoras, la simulacion del coste acumulado y la confirmacion en firebase
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

    private Usuario usuarioReal;
    private Personaje personajeReal;

    private Personaje pjTemp;
    private int costeAcumulado = 0;

    private bool estaInicializado = false;
    private bool estaDesbloqueado = false;

    private void Start()
    {
        // esta funcion inicializa el gestor de mejoras y descarga los datos actuales del personaje
        if (mejoraManager == null)
        {
            mejoraManager = FindFirstObjectByType<MejoraStatsManager>();
            if (mejoraManager == null)
            {
                mejoraManager = gameObject.AddComponent<MejoraStatsManager>();
                Debug.LogWarning("[PanelMejorasUI] No se encontró MejoraStatsManager en la escena. Se ha creado e instanciado uno automáticamente en este GameObject.");
            }
        }
        estaInicializado = true;
        CargarDatosAutomaticamente();
    }

    private void OnEnable()
    {
        // esta funcion recarga los datos del panel al activar el componente
        if (estaInicializado)
        {
            CargarDatosAutomaticamente();
        }
    }

    public async void CargarDatosAutomaticamente()
    {
        // esta funcion descarga los datos de las estadisticas del personaje y su estado de desbloqueo
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            usuarioReal = SessionManager.shared.currentUser;

            string idPersonaje = GameManager.Instance.idPersonajeSeleccionado;
            
            MostrarPersonajes mp = Object.FindFirstObjectByType<MostrarPersonajes>();
            if (mp != null) idPersonaje = mp.ObtenerIdPersonajeVisible();

            Debug.Log($"Usuario {usuarioReal.username} encontrado. Descargando stats de {idPersonaje}...");

            personajeReal = await usuarioService.ObtenerPersonajeDeUsuario(usuarioReal.id, idPersonaje);

            if (this == null) return;

            if (personajeReal == null)
            {
                Debug.LogWarning($"El personaje '{idPersonaje}' no está en la base de datos del usuario. Cargando stats base...");
                PersonajeService pjServiceGlobal = new PersonajeService();
                personajeReal = await pjServiceGlobal.ObtenerPersonaje(idPersonaje);
                
                if (this == null) return;
                
                if (personajeReal != null)
                {
                    personajeReal.id = idPersonaje;
                }
            }

            estaDesbloqueado = false;
            if (mp != null)
            {
                estaDesbloqueado = mp.EstaPersonajeVisibleDesbloqueado();
            }
            else if (personajeReal != null && idPersonaje.Equals("personaje_james", System.StringComparison.OrdinalIgnoreCase))
            {
                estaDesbloqueado = true;
            }

            if (personajeReal != null)
            {
                Debug.Log($"Todo listo. Monedas: {usuarioReal.free_coin}, Nivel del personaje: {personajeReal.NivelTotal}");
                ReiniciarCarrito();
            }
            else
            {
                Debug.LogError($"Error: No se encontró el personaje '{idPersonaje}' ni en el usuario ni en las stats base.");
            }
        }
        else
        {
            Debug.LogError("Error: El SessionManager está vacío.");
        }
    }

    public void AbrirPanel(Usuario user, Personaje pj)
    {
        // esta funcion abre la ventana de mejoras asociando el usuario y personaje dados
        if (user == null || pj == null)
        {
            Debug.LogError("Error: Se intenta abrir el panel con usuario o personaje nulos.");
            return;
        }

        usuarioReal = user;
        personajeReal = pj;

        Debug.Log($"Abriendo panel para {pj.name}. Monedas actuales del usuario: {user.free_coin}");

        ReiniciarCarrito();
        gameObject.SetActive(true);
    }

    public void BtnAumentarVida() { SimularMejora(StatType.Life); }
    public void BtnAumentarEnergia() { SimularMejora(StatType.Energy); }
    public void BtnAumentarFuerza() { SimularMejora(StatType.Force); }
    public void BtnAumentarRecuperacion() { SimularMejora(StatType.Recovery); }

    private void SimularMejora(StatType stat)
    {
        // esta funcion simula la compra de una mejora incrementando el coste acumulado y el nivel del atributo temporal
        if (!estaDesbloqueado)
        {
            Debug.LogWarning("No puedes mejorar un personaje que esta bloqueado.");
            return;
        }

        if (usuarioReal == null || pjTemp == null)
        {
            Debug.LogError("No puedes mejorar porque no se ha cargado ningún usuario o personaje.");
            return;
        }

        if (mejoraManager == null)
        {
            Debug.LogError("Falta la referencia a MejoraStatsManager.");
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

    public async void BotonConfirmar()
    {
        // esta funcion aplica de forma definitiva las mejoras en la base de datos y descuenta las monedas del usuario
        if (costeAcumulado == 0)
        {
            Debug.Log("No hay ninguna mejora en el carrito para confirmar.");
            return;
        }

        Debug.Log($"Confirmando compra. Gastando {costeAcumulado} monedas de {usuarioReal.free_coin} totales.");

        if (btnConfirmar != null) btnConfirmar.interactable = false;
        if (btnDescartar != null) btnDescartar.interactable = false;

        usuarioReal.free_coin -= costeAcumulado;

        bool userOk = await usuarioService.ActualizarUsuario(usuarioReal);
        if (this == null) return;
        bool personajeOk = await usuarioService.ActualizarPersonaje(usuarioReal.id, pjTemp);
        if (this == null) return;

        if (userOk && personajeOk)
        {
            Debug.Log("Firebase actualizado correctamente.");
            personajeReal = pjTemp;
            ReiniciarCarrito();
        }
        else
        {
            Debug.LogError("Error de red al guardar en Firebase. Devolviendo monedas...");
            usuarioReal.free_coin += costeAcumulado;
            ActualizarTextos();
        }

        if (btnConfirmar != null) btnConfirmar.interactable = true;
        if (btnDescartar != null) btnDescartar.interactable = true;
    }

    public void BotonDescartar()
    {
        // esta funcion cancela las mejoras seleccionadas y limpia el carro de compra simulado
        Debug.Log("Mejoras descartadas por el usuario.");
        ReiniciarCarrito();
    }

    private void ReiniciarCarrito()
    {
        // esta funcion reestablece los valores de las estadisticas y costes acumulados al estado inicial
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
        // esta funcion refresca los textos del panel y colorea de verde los atributos aumentados o de rojo si no hay fondos
        if (txtVida == null || txtEnergia == null || txtFuerza == null || txtRecuperacion == null || txtMonedasJugador == null || txtCosteTotal == null)
        {
            Debug.LogError("Error: Faltan referencias a componentes TextMeshProUGUI en el inspector.");
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

        int monedasDisponibles = usuarioReal.free_coin - costeAcumulado;

        if (txtCosteVidaProx != null && mejoraManager != null) 
        {
            int cost = mejoraManager.CalcularCoste(StatType.Life, pjTemp);
            txtCosteVidaProx.text = cost.ToString();
            txtCosteVidaProx.color = monedasDisponibles >= cost ? Color.white : Color.red;
        }
        if (txtCosteEnergiaProx != null && mejoraManager != null) 
        {
            int cost = mejoraManager.CalcularCoste(StatType.Energy, pjTemp);
            txtCosteEnergiaProx.text = cost.ToString();
            txtCosteEnergiaProx.color = monedasDisponibles >= cost ? Color.white : Color.red;
        }
        if (txtCosteFuerzaProx != null && mejoraManager != null) 
        {
            int cost = mejoraManager.CalcularCoste(StatType.Force, pjTemp);
            txtCosteFuerzaProx.text = cost.ToString();
            txtCosteFuerzaProx.color = monedasDisponibles >= cost ? Color.white : Color.red;
        }
        if (txtCosteRecuperacionProx != null && mejoraManager != null) 
        {
            int cost = mejoraManager.CalcularCoste(StatType.Recovery, pjTemp);
            txtCosteRecuperacionProx.text = cost.ToString();
            txtCosteRecuperacionProx.color = monedasDisponibles >= cost ? Color.white : Color.red;
        }

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
