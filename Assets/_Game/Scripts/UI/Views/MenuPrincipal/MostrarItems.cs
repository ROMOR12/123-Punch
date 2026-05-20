using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MostrarItems : MonoBehaviour
{
    private GameManager gameManager;
    private UsuarioService usuarioService;

    [Header("Referencias de Cuadrícula Dinámica (Tienda)")]
    [Tooltip("El contenedor (Grid Layout Group) donde se instanciarán los artículos.")]
    public Transform gridTienda;
    [Tooltip("El prefab de la tarjeta de artículo que contiene el script TiendaItemUI.")]
    public GameObject prefabItemTienda;

    [Header("UI General de la Tienda")]
    [Tooltip("Texto para mostrar las monedas actuales del jugador en la interfaz de la tienda.")]
    public TextMeshProUGUI txtMonedasTienda;
    [Tooltip("Texto opcional para mostrar mensajes de éxito o error al comprar.")]
    public TextMeshProUGUI txtMensajeFeedback;

    [Header("Panel de Confirmación de Compra (Popup)")]
    public GameObject panelConfirmacion;
    public GameObject panelConfirmacionFade;
    public TextMeshProUGUI txtNombreConfirmacion;
    public TextMeshProUGUI txtDescripcionConfirmacion;
    public TextMeshProUGUI txtPrecioConfirmacion;
    public Image iconoConfirmacion;
    public Button btnConfirmarCompra;
    public Button btnCancelarCompra;

    private ItemBase itemSeleccionado;

    private void Awake()
    {
        usuarioService = new UsuarioService();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        
        // Inicializar vistas al arrancar si el panel está activo
        CargarMonedasUsuario();
        RefrescarTienda();
    }

    private void OnEnable()
    {
        CargarMonedasUsuario();
        RefrescarTienda();
    }

    /// <summary>
    /// Muestra la cantidad de monedas actuales del usuario de la sesión.
    /// </summary>
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

    /// <summary>
    /// Limpia el grid e instancia dinámicamente solo los objetos comunes del catálogo.
    /// </summary>
    public void RefrescarTienda()
    {
        if (gridTienda == null || prefabItemTienda == null)
        {
            Debug.LogWarning("Faltan referencias críticas (gridTienda o prefabItemTienda) en MostrarItems.");
            return;
        }

        // Limpiar elementos previos en el grid
        foreach (Transform child in gridTienda)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null) return;

        // Cargar todos los ítems disponibles del catálogo
        List<ItemBase> catalogo = new List<ItemBase>();
        if (GameManager.Instance.items != null && GameManager.Instance.items.Count > 0)
        {
            catalogo.AddRange(GameManager.Instance.items);
        }
        else
        {
            // Fallback: Si la lista principal en GameManager está vacía, usamos los activos
            if (GameManager.Instance.todosLosActivos != null) catalogo.AddRange(GameManager.Instance.todosLosActivos);
        }

        int itemsMostrados = 0;

        foreach (ItemBase item in catalogo)
        {
            if (item == null) continue;

            // REQUISITO: Filtrar para mostrar únicamente ítems con rareza COMÚN o RARA y de tipo ACTIVO (Consumible)
            if (item.rarity != ItemRarity.Comun && item.rarity != ItemRarity.Raro) continue;
            if (item is not Consumible) continue;

            CrearTarjetaItem(item);
            itemsMostrados++;
        }
        Debug.Log($"Tienda cargada: se muestran {itemsMostrados} artículos comunes o raros.");
    }

    /// <summary>
    /// Crea e inicializa visualmente la tarjeta de un artículo en el grid.
    /// </summary>
    private void CrearTarjetaItem(ItemBase item)
    {
        GameObject cardObj = Instantiate(prefabItemTienda, gridTienda, false);
        cardObj.SetActive(true);

        // Resetear escala y posición para evitar fallos de Canvas/Layout
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
            // Asignar los campos solicitados: Nombre, Imagen/Icono y Precio
            if (itemUI.textNombre != null) itemUI.textNombre.text = item.itemBaseName;
            if (itemUI.textPrecio != null) itemUI.textPrecio.text = item.precio.ToString();
            if (itemUI.iconoItem != null)
            {
                itemUI.iconoItem.sprite = (item.icon != null) ? item.icon : (GameManager.Instance != null ? GameManager.Instance.imageDefault : null);
            }

            // Configurar botón de compra
            if (itemUI.botonComprar != null)
            {
                itemUI.botonComprar.interactable = true;
                itemUI.botonComprar.onClick.RemoveAllListeners();
                itemUI.botonComprar.onClick.AddListener(() => AbrirConfirmacionCompra(item));
            }
        }
        else
        {
            Debug.LogError($"El prefabItemTienda no tiene el script adjunto 'TiendaItemUI'.");
        }
    }

    /// <summary>
    /// Abre el panel de confirmación (Popup) con los datos del artículo seleccionado.
    /// </summary>
    private void AbrirConfirmacionCompra(ItemBase item)
    {
        itemSeleccionado = item;

        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(true);
            panelConfirmacionFade.SetActive(true);

            if (txtNombreConfirmacion != null) txtNombreConfirmacion.text = item.itemBaseName;
            if (txtDescripcionConfirmacion != null) txtDescripcionConfirmacion.text = item.description;
            if (txtPrecioConfirmacion != null) txtPrecioConfirmacion.text = item.precio.ToString();
            if (iconoConfirmacion != null)
            {
                iconoConfirmacion.sprite = (item.icon != null) ? item.icon : (GameManager.Instance != null ? GameManager.Instance.imageDefault : null);
            }

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
            // Compra directa si no hay popup configurado
            ConfirmarCompra();
        }
    }

    /// <summary>
    /// Cierra el popup de confirmación.
    /// </summary>
    public void CerrarConfirmacionCompra()
    {
        if (panelConfirmacion != null)
        {
            panelConfirmacion.SetActive(false);
            panelConfirmacionFade.SetActive(false);
        }
        itemSeleccionado = null;
    }

    /// <summary>
    /// Procesa la compra del artículo, restando las monedas, actualizando el inventario y guardando en Firebase.
    /// </summary>
    private async void ConfirmarCompra()
    {
        if (itemSeleccionado == null) return;

        if (SessionManager.shared == null || SessionManager.shared.currentUser == null)
        {
            MostrarFeedback("Error: Sesión de usuario no válida.");
            CerrarConfirmacionCompra();
            return;
        }

        Usuario user = SessionManager.shared.currentUser;

        // 1. Validar monedas suficientes
        if (user.free_coin < itemSeleccionado.precio)
        {
            MostrarFeedback("Monedas insuficientes.");
            CerrarConfirmacionCompra();
            return;
        }

        // 2. Transacción en memoria local
        user.free_coin -= itemSeleccionado.precio;
        if (user.inventario == null) user.inventario = new List<string>();
        user.inventario.Add(itemSeleccionado.id);

        // Sincronizar variables locales
        if (GameManager.Instance != null)
        {
            GameManager.Instance.inventarioIDs = user.inventario;
        }

        MostrarFeedback("Procesando compra en la base de datos...");

        // 3. Guardar en Firebase Firestore
        bool ok = await usuarioService.ActualizarUsuario(user);

        if (ok)
        {
            MostrarFeedback($"¡Has comprado {itemSeleccionado.itemBaseName}!");
            CargarMonedasUsuario();
            RefrescarTienda();
        }
        else
        {
            // Revertir cambios locales si falla la red
            user.free_coin += itemSeleccionado.precio;
            user.inventario.Remove(itemSeleccionado.id);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.inventarioIDs = user.inventario;
            }

            MostrarFeedback("Error de red al guardar. Monedas devueltas.");
            CargarMonedasUsuario();
        }

        CerrarConfirmacionCompra();
    }

    /// <summary>
    /// Muestra un mensaje temporal en la interfaz para retroalimentación.
    /// </summary>
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
