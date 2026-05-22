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

    private Usuario usuarioReal;
    private Personaje personajeReal;

    private Personaje pjTemp;
    private int costeAcumulado = 0;

    private bool estaInicializado = false;

    private void Start()
    {
        if (mejoraManager == null)
        {
            mejoraManager = FindFirstObjectByType<MejoraStatsManager>();
            if (mejoraManager == null)
            {
                mejoraManager = gameObject.AddComponent<MejoraStatsManager>();
                Debug.LogWarning("[PanelMejorasUI] No se encontrÃ³ MejoraStatsManager en la escena. Se ha creado e instanciado uno automÃ¡ticamente en este GameObject.");
            }
        }
        estaInicializado = true;
        CargarDatosAutomaticamente();
    }

    private void OnEnable()
    {
        if (estaInicializado)
        {
            CargarDatosAutomaticamente();
        }
    }

    public async void CargarDatosAutomaticamente()
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            usuarioReal = SessionManager.shared.currentUser;

            string idPersonaje = GameManager.Instance.idPersonajeSeleccionado;

            Debug.Log($"Usuario {usuarioReal.username} encontrado. Descargando stats de {idPersonaje}...");

            personajeReal = await usuarioService.ObtenerPersonajeDeUsuario(usuarioReal.id, idPersonaje);

            if (this == null) return;

            if (personajeReal == null)
            {
                Debug.LogWarning($"El personaje '{idPersonaje}' no estÃ¡ en la base de datos del usuario. Cargando stats base...");
                PersonajeService pjServiceGlobal = new PersonajeService();
                personajeReal = await pjServiceGlobal.ObtenerPersonaje(idPersonaje);
                
                if (this == null) return;
                
                if (personajeReal != null)
                {
                    personajeReal.id = idPersonaje;
                }
            }

            if (personajeReal != null)
            {
                Debug.Log($"Â¡Todo listo! Monedas: {usuarioReal.free_coin}, Nivel del personaje: {personajeReal.NivelTotal}");
                ReiniciarCarrito();
            }
            else
            {
                Debug.LogError($"Error: No se encontrÃ³ el personaje '{idPersonaje}' ni en el usuario ni en las stats base.");
            }
        }
        else
        {
            Debug.LogError("Error: El SessionManager estÃ¡ vacÃ­o. Â¿Iniciaste sesiÃ³n desde la pantalla de Login?");
        }
    }

    public void AbrirPanel(Usuario user, Personaje pj)
    {
        if (user == null || pj == null)
        {
            Debug.LogError("Â¡ERROR! Le estÃ¡s intentando pasar un usuario o personaje VACÃO (null) al panel.");
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
        if (usuarioReal == null || pjTemp == null)
        {
            Debug.LogError("No puedes mejorar porque no se ha cargado ningÃºn usuario o personaje. Â¿Llamaste a AbrirPanel()?");
            return;
        }

        if (mejoraManager == null)
        {
            Debug.LogError("Â¡Falta arrastrar el MejoraStatsManager al slot del Inspector!");
            return;
        }

        int costeProxNivel = mejoraManager.CalcularCoste(stat, pjTemp);
        Debug.Log($"Intentando mejorar {stat}. Coste prÃ³ximo nivel: {costeProxNivel}. Monedas disponibles: {usuarioReal.free_coin - costeAcumulado}");

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

            Debug.Log($"SimulaciÃ³n de {stat} exitosa. Nuevo coste acumulado: {costeAcumulado}");
            ActualizarTextos();
        }
        else
        {
            Debug.LogWarning("Monedas insuficientes para comprar esta mejora.");
        }
    }

    public async void BotonConfirmar()
    {
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
            Debug.Log("Â¡Firebase actualizado correctamente!");
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
        Debug.Log("Mejoras descartadas por el usuario.");
        ReiniciarCarrito();
    }

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
        if (txtVida == null || txtEnergia == null || txtFuerza == null || txtRecuperacion == null || txtMonedasJugador == null || txtCosteTotal == null)
        {
            Debug.LogError("Â¡ERROR GRAVE! Te has olvidado de arrastrar uno o varios Textos (TextMeshPro) al Inspector del script.");
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

        if (txtCosteVidaProx != null && mejoraManager != null) txtCosteVidaProx.text = mejoraManager.CalcularCoste(StatType.Life, pjTemp).ToString();
        if (txtCosteEnergiaProx != null && mejoraManager != null) txtCosteEnergiaProx.text = mejoraManager.CalcularCoste(StatType.Energy, pjTemp).ToString();
        if (txtCosteFuerzaProx != null && mejoraManager != null) txtCosteFuerzaProx.text = mejoraManager.CalcularCoste(StatType.Force, pjTemp).ToString();
        if (txtCosteRecuperacionProx != null && mejoraManager != null) txtCosteRecuperacionProx.text = mejoraManager.CalcularCoste(StatType.Recovery, pjTemp).ToString();

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
