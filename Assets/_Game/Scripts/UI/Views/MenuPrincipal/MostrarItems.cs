using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MostrarItems : MonoBehaviour
{
    private TiendaService tiendaService;

    [Header("Referencias de CuadrÃ­cula DinÃ¡mica (Tienda)")]
    [Tooltip("El contenedor (Grid Layout Group) donde se instanciarÃ¡n los artÃ­culos.")]
    public Transform gridTienda;
    [Tooltip("El prefab de la tarjeta de artÃ­culo que contiene el script TiendaItemUI.")]
    public GameObject prefabItemTienda;

    [Header("UI General de la Tienda")]
    [Tooltip("Texto para mostrar las monedas actuales del jugador en la interfaz de la tienda.")]
    public TextMeshProUGUI txtMonedasTienda;
    [Tooltip("Texto opcional para mostrar mensajes de Ã©xito o error al comprar.")]
    public TextMeshProUGUI txtMensajeFeedback;

    [Header("Panel de ConfirmaciÃ³n de Compra (Popup)")]
    public GameObject panelConfirmacion;
    public GameObject panelConfirmacionFade;
    public TextMeshProUGUI txtNombreConfirmacion;
    public TextMeshProUGUI txtDescripcionConfirmacion;
    public TextMeshProUGUI txtPrecioConfirmacion;
    public Image iconoConfirmacion;
    public Button btnConfirmarCompra;
    public Button btnCancelarCompra;

    private TiendaItem ofertaSeleccionada;
    private Objeto objetoSeleccionado;

    private void Awake()
    {
        tiendaService = new TiendaService();
    }

    private void Start()
    {
        CargarMonedasUsuario();
        RefrescarTienda();
    }

    private void OnEnable()
    {
        CargarMonedasUsuario();
        RefrescarTienda();
    }

    private void CargarMonedasUsuario()
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            if (txtMonedasTienda != null)
            {
                txtMonedasTienda.text = SessionManager.shared.currentUser.free_coin.ToString();
            }
        }
    }

    private bool isRefreshing = false;

    public async void RefrescarTienda()
    {
        if (isRefreshing) return;
        isRefreshing = true;

        try
        {
            if (gridTienda == null || prefabItemTienda == null)
            {
                Debug.LogWarning("Faltan referencias crÃ­ticas en MostrarItems.");
                return;
            }

            foreach (Transform child in gridTienda)
            {
                Destroy(child.gameObject);
            }

            if (GameManager.Instance == null) return;

            GlobalDataService globalData = new GlobalDataService();
            await globalData.CargarObjetosGlobales();

            List<TiendaItem> ofertas = await tiendaService.ObtenerCatalogoActivo();
            
            int itemsMostrados = 0;

            foreach (TiendaItem oferta in ofertas)
            {
                if (oferta == null) continue;

                if (!GlobalDataService.cacheObjetos.TryGetValue(oferta.id_Objeto, out Objeto datosFirebase))
                {
                    Debug.LogWarning($"No se encontrÃ³ objeto con ID: {oferta.id_Objeto} en la cachÃ©.");
                    continue;
                }

                Sprite icono = GameManager.Instance.imageDefault;
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.items != null)
                    {
                        foreach (var itemLocal in GameManager.Instance.items)
                            if (itemLocal != null && itemLocal.id == oferta.id_Objeto) { icono = itemLocal.icon; break; }
                    }
                    if (icono == GameManager.Instance.imageDefault && GameManager.Instance.todosLosActivos != null)
                    {
                        foreach (var itemLocal in GameManager.Instance.todosLosActivos)
                            if (itemLocal != null && itemLocal.id == oferta.id_Objeto) { icono = itemLocal.icon; break; }
                    }
                    if (icono == GameManager.Instance.imageDefault && GameManager.Instance.todosLosPasivos != null)
                    {
                        foreach (var itemLocal in GameManager.Instance.todosLosPasivos)
                            if (itemLocal != null && itemLocal.id == oferta.id_Objeto) { icono = itemLocal.icon; break; }
                    }
                }

                CrearTarjetaOferta(oferta, datosFirebase, icono);
                itemsMostrados++;
            }
            Debug.Log($"Tienda cargada desde Firebase: se muestran {itemsMostrados} ofertas.");
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private void CrearTarjetaOferta(TiendaItem oferta, Objeto datosFirebase, Sprite icono)
    {
        GameObject cardObj = Instantiate(prefabItemTienda, gridTienda, false);
        cardObj.SetActive(true);

        RectTransform rt = cardObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
        }

        TiendaItemUI itemUI = cardObj.GetComponent<TiendaItemUI>();
        if (itemUI != null)
        {
            if (itemUI.textNombre != null) itemUI.textNombre.text = datosFirebase.name;
            if (itemUI.textPrecio != null) itemUI.textPrecio.text = oferta.precio_monedas.ToString();
            if (itemUI.iconoItem != null) itemUI.iconoItem.sprite = icono;

            if (itemUI.botonComprar != null)
            {
                itemUI.botonComprar.interactable = true;
                itemUI.botonComprar.onClick.RemoveAllListeners();
                itemUI.botonComprar.onClick.AddListener(() => AbrirConfirmacionCompra(oferta, datosFirebase, icono));
            }
        }
        else
        {
            Debug.LogError($"El prefabItemTienda no tiene el script adjunto 'TiendaItemUI'.");
        }
    }

    private void AbrirConfirmacionCompra(TiendaItem oferta, Objeto datosFirebase, Sprite icono)
    {
        ofertaSeleccionada = oferta;
        objetoSeleccionado = datosFirebase;

        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(true);
            panelConfirmacionFade.SetActive(true);

            if (txtNombreConfirmacion != null) txtNombreConfirmacion.text = datosFirebase.name;
            if (txtDescripcionConfirmacion != null) txtDescripcionConfirmacion.text = datosFirebase.description;
            if (txtPrecioConfirmacion != null) txtPrecioConfirmacion.text = oferta.precio_monedas.ToString();
            if (iconoConfirmacion != null) iconoConfirmacion.sprite = icono;

            if (btnConfirmarCompra != null)
            {
                btnConfirmarCompra.onClick.RemoveAllListeners();
                btnConfirmarCompra.onClick.AddListener(ConfirmarCompra);
            }

            if (btnCancelarCompra != null)
            {
                btnCancelarCompra.onClick.RemoveAllListeners();
                btnCancelarCompra.onClick.AddListener(CerrarConfirmacionCompra);
            }
        }
        else
        {
            ConfirmarCompra();
        }
    }

    public void CerrarConfirmacionCompra()
    {
        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(false);
            panelConfirmacionFade.SetActive(false);
        }
        ofertaSeleccionada = null;
        objetoSeleccionado = null;
    }

    private async void ConfirmarCompra()
    {
        if (ofertaSeleccionada == null) return;

        if (SessionManager.shared == null || SessionManager.shared.currentUser == null)
        {
            MostrarFeedback("Error: SesiÃ³n de usuario no vÃ¡lida.");
            CerrarConfirmacionCompra();
            return;
        }

        Usuario user = SessionManager.shared.currentUser;

        if (btnConfirmarCompra != null) btnConfirmarCompra.interactable = false;
        MostrarFeedback("Procesando compra en Firebase...");

        bool ok = await tiendaService.Comprar(user, ofertaSeleccionada);

        if (ok)
        {
            if (GameManager.Instance != null && user.inventario != null)
            {
                GameManager.Instance.inventarioIDs = user.inventario;
            }

            MostrarFeedback($"Â¡Has comprado {objetoSeleccionado.name}!");
        }
        else
        {
            MostrarFeedback("Error: Monedas insuficientes o fallo de red.");
        }

        CargarMonedasUsuario();

        if (btnConfirmarCompra != null) btnConfirmarCompra.interactable = true;
        CerrarConfirmacionCompra();
    }

    private void MostrarFeedback(string mensaje)
    {
        Debug.Log($"[TIENDA] {mensaje}");
        if (txtMensajeFeedback != null)
        {
            txtMensajeFeedback.text = mensaje;
            CancelInvoke("LimpiarFeedback");
            Invoke("LimpiarFeedback", 4f);
        }
    }

    private void LimpiarFeedback()
    {
        if (txtMensajeFeedback != null)
        {
            txtMensajeFeedback.text = "";
        }
    }
    
}
