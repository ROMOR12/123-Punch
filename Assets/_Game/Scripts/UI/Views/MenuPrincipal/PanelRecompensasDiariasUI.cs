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
        
        if (btnReclamar != null) btnReclamar.interactable = false;
        if (txtBotonReclamar != null) txtBotonReclamar.text = "Cargando...";

        configActual = await recompensasService.ObtenerConfiguracion();

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
            diaACobrarHoy = recompensasService.ObtenerRachaActualizada(user);
            
            if (diaACobrarHoy == 1) rachaYaCobrada = 0; 
            else rachaYaCobrada = diaACobrarHoy - 1;
        }

        if (contenedorTarjetas != null)
        {
            foreach (Transform child in contenedorTarjetas) Destroy(child.gameObject);
        }

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

            if (puedeReclamar && recompensa.dia == diaACobrarHoy)
            {
                recompensaDeHoy = recompensa;
            }
        }

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
            RefrescarUI();
            
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
