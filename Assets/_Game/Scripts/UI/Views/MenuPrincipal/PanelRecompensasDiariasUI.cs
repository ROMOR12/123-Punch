using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PanelRecompensasDiariasUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public Transform contenedorTarjetas;
    public GameObject prefabTarjetaNormal;
    public Button btnReclamar;
    public TextMeshProUGUI txtBotonReclamar;

    [Header("Iconos de los Premios")]
    public Sprite iconFreeCoin;
    public Sprite iconPremiumCoin;
    public Sprite iconLootbox;

    private RecompensasService recompensasService;
    private ConfiguracionRecompensas configActual;
    private RecompensaDiaria recompensaDeHoy;

    private async void OnEnable()
    {
        if (recompensasService == null) recompensasService = new RecompensasService();
        
        // Bloqueamos el botón mientras comprobamos Firebase
        if (btnReclamar != null) btnReclamar.interactable = false;
        if (txtBotonReclamar != null) txtBotonReclamar.text = "Cargando...";

        // 1. Obtener la lista de los 7 días desde Firebase
        configActual = await recompensasService.ObtenerConfiguracion();

        // 2. Comprobar racha y dibujar tarjetas
        RefrescarUI();
    }

    private void RefrescarUI()
    {
        if (configActual == null || configActual.dias == null || configActual.dias.Count == 0)
        {
            if (txtBotonReclamar != null) txtBotonReclamar.text = "Error al cargar";
            return;
        }

        Usuario user = SessionManager.shared.currentUser;
        if (user == null) return;

        bool puedeReclamar = recompensasService.PuedeReclamar(user);
        
        int rachaYaCobrada = user.daily_reward_streak;
        int diaACobrarHoy = 0;

        if (puedeReclamar)
        {
            // Verificamos qué día le toca cobrar hoy si le da al botón
            diaACobrarHoy = recompensasService.ObtenerRachaActualizada(user);
            
            // Si le toca el día 1, es porque es nuevo o perdió la racha, visualmente no tiene nada cobrado.
            if (diaACobrarHoy == 1) rachaYaCobrada = 0; 
            else rachaYaCobrada = diaACobrarHoy - 1;
        }

        // Limpiamos los contenedores por si ya había tarjetas
        if (contenedorTarjetas != null)
        {
            foreach (Transform child in contenedorTarjetas) Destroy(child.gameObject);
        }

        // Creamos las tarjetas
        for (int i = 0; i < configActual.dias.Count; i++)
        {
            var recompensa = configActual.dias[i];
            
            if (prefabTarjetaNormal != null && contenedorTarjetas != null)
            {
                GameObject obj = Instantiate(prefabTarjetaNormal, contenedorTarjetas);
                TarjetaRecompensaUI tarjeta = obj.GetComponent<TarjetaRecompensaUI>();

                if (tarjeta != null)
                {
                    Sprite spriteIcono = GetIcono(recompensa.tipo);
                    string nombre = GetNombre(recompensa.tipo);
                    
                    bool esDiaActual = (puedeReclamar && recompensa.dia == diaACobrarHoy);
                    bool yaReclamado = recompensa.dia <= rachaYaCobrada;

                    tarjeta.Configurar(recompensa, spriteIcono, nombre, yaReclamado, esDiaActual);
                }
            }

            // Guardamos la referencia de lo que gana hoy para el botón
            if (puedeReclamar && recompensa.dia == diaACobrarHoy)
            {
                recompensaDeHoy = recompensa;
            }
        }

        // Configuramos el botón
        if (btnReclamar != null)
        {
            btnReclamar.interactable = puedeReclamar;
            btnReclamar.onClick.RemoveAllListeners();
            if (puedeReclamar)
            {
                btnReclamar.onClick.AddListener(OnBotonReclamar);
            }
        }

        if (txtBotonReclamar != null)
        {
            txtBotonReclamar.text = puedeReclamar ? "Reclamar" : "Reclamado";
        }
    }

    private async void OnBotonReclamar()
    {
        Usuario user = SessionManager.shared.currentUser;
        if (user == null || recompensaDeHoy == null) return;

        if (btnReclamar != null) btnReclamar.interactable = false;
        if (txtBotonReclamar != null) txtBotonReclamar.text = "Guardando...";

        bool ok = await recompensasService.Reclamar(user, recompensaDeHoy);
        
        if (ok)
        {
            Debug.Log("¡Recompensa diaria reclamada con éxito!");
            RefrescarUI(); // Refrescará para poner el "Tick verde" al día de hoy
            
            // Esperamos 1.5 segundos para que el jugador vea el Tick verde y cerramos
            Invoke(nameof(CerrarPanel), 1.5f);
        }
        else
        {
            Debug.LogError("Error al reclamar la recompensa diaria en Firebase.");
            if (btnReclamar != null) btnReclamar.interactable = true;
            if (txtBotonReclamar != null) txtBotonReclamar.text = "Reclamar";
        }
    }

    private Sprite GetIcono(string tipo)
    {
        if (tipo == "free_coin") return iconFreeCoin;
        if (tipo == "premium_coin") return iconPremiumCoin;
        if (tipo == "lootbox") return iconLootbox;
        return null;
    }

    private string GetNombre(string tipo)
    {
        if (tipo == "free_coin") return "Monedas";
        if (tipo == "premium_coin") return "Billetes";
        if (tipo == "lootbox") return "Cajas";
        return tipo;
    }

    private void CerrarPanel()
    {
        gameObject.SetActive(false);
    }
}
