using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PanelInventarioUI : MonoBehaviour
{
    [Header("Referencias a Huecos de Equipamiento")]
    public Image iconoPasivo;
    public Image iconoActivo1;
    public Image iconoActivo2;

    [Header("Grid de Inventario (Mochila)")]
    public Transform gridInventario;
    public GameObject prefabItemInventario;

    [Header("Popup de Equipamiento")]
    public GameObject panelEquiparConfirmacion;
    public TextMeshProUGUI txtNombreEquipar;
    public TextMeshProUGUI txtDescripcionEquipar;
    public Image iconoEquipar;
    public Button btnEquipar;
    public Button btnCancelarEquipar;

    [Header("Popup de Desequipar")]
    public GameObject panelDesequiparConfirmacion;
    public GameObject panelDesequiparConfirmacionFade;
    public TextMeshProUGUI txtNombreDesequipar;
    public Button btnDesequipar;
    public Button btnCancelarDesequipar;

    [Header("Servicios")]
    private UsuarioService usuarioService;
    private ItemBase itemSeleccionado;
    private int slotADesequipar = -1;
    private ItemBase itemADesequipar;

    private void Awake()
    {
        usuarioService = new UsuarioService();
    }

    private void Start()
    {
        if (btnEquipar != null) btnEquipar.onClick.AddListener(ConfirmarEquipar);
        if (btnCancelarEquipar != null) btnCancelarEquipar.onClick.AddListener(CerrarPopupEquipar);
        if (panelEquiparConfirmacion != null) panelEquiparConfirmacion.SetActive(false);

        if (btnDesequipar != null) btnDesequipar.onClick.AddListener(ConfirmarDesequipar);
        if (btnCancelarDesequipar != null) btnCancelarDesequipar.onClick.AddListener(CerrarPopupDesequipar);
        if (panelDesequiparConfirmacion != null) panelDesequiparConfirmacion.SetActive(false);
    }

    private void OnEnable()
    {
        if (panelEquiparConfirmacion != null) panelEquiparConfirmacion.SetActive(false);
        if (panelDesequiparConfirmacion != null) panelDesequiparConfirmacion.SetActive(false);

        // 0. Sincronizar los datos en memoria con los de la base de datos (SessionManager)
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            Usuario user = SessionManager.shared.currentUser;
            GameManager.Instance.inventarioIDs = user.inventario ?? new List<string>();
            GameManager.Instance.pasivoEquipadoID = user.pasivo_equipado ?? "";
            
            if (user.objetos_equipados != null && user.objetos_equipados.Count >= 2)
            {
                GameManager.Instance.activosEquipadosIDs = user.objetos_equipados;
            }
        }

        CargarInventario();
        ActualizarUIEquipamiento();
    }

    // --- 1. CARGAR INVENTARIO VISUAL ---
    private void CargarInventario()
    {
        // Limpiar botones anteriores
        foreach (Transform child in gridInventario)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null || GameManager.Instance.inventarioIDs == null) 
        {
            Debug.LogWarning("Inventario nulo en el GameManager.");
            return;
        }

        Debug.Log($"Tienes {GameManager.Instance.inventarioIDs.Count} objetos en tu inventario de Firebase.");

        // 1. Agrupar items (contar cantidades)
        Dictionary<string, int> conteoItems = new Dictionary<string, int>();
        foreach (string itemID in GameManager.Instance.inventarioIDs)
        {
            if (string.IsNullOrEmpty(itemID)) continue; // Ignorar IDs vacíos o nulos

            if (conteoItems.ContainsKey(itemID))
            {
                conteoItems[itemID]++;
            }
            else
            {
                conteoItems[itemID] = 1;
            }
        }

        // 2. Crear las filas visuales
        foreach (var kvp in conteoItems)
        {
            string itemID = kvp.Key;
            int cantidad = kvp.Value;

            ItemBase itemBase = ObtenerItemPorID(itemID);
            if (itemBase != null)
            {
                CrearBotonItem(itemBase, cantidad);
            }
            else
            {
                Debug.LogError($"El ID '{itemID}' está en tu Firebase pero NO se encontró en el GameManager.");
            }
        }
    }

    private void CrearBotonItem(ItemBase item, int cantidad)
    {
        GameObject rowObj = Instantiate(prefabItemInventario, gridInventario, false);
        rowObj.SetActive(true);
        
        RectTransform rt = rowObj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.anchoredPosition3D = Vector3.zero;
        }

        Debug.Log($"[UI] Fila creada para: {item.itemBaseName} (x{cantidad}). Si no la ves en pantalla, es problema del RectTransform/Layout.");
        InventarioRowUI rowUI = rowObj.GetComponent<InventarioRowUI>();

        if (rowUI != null)
        {
            // Rellenar datos
            if (rowUI.textCantidad != null) rowUI.textCantidad.text = $"x{cantidad}";
            if (rowUI.iconoItem != null) rowUI.iconoItem.sprite = item.icon;
            if (rowUI.textNombre != null) rowUI.textNombre.text = item.itemBaseName;
            if (rowUI.textRareza != null) rowUI.textRareza.text = item.rarity.ToString();
            
            if (rowUI.textTipo != null)
            {
                rowUI.textTipo.text = (item is Pasivo) ? "Pasivo" : "Activo";
            }

            // Configurar botón equipar
            if (rowUI.botonEquipar != null)
            {
                rowUI.botonEquipar.onClick.AddListener(() => AbrirPopupEquipar(item));
            }
        }
        else
        {
            // Si el usuario aún no ha puesto el script, hacemos fallback para que no pete
            Image img = rowObj.GetComponent<Image>();
            Button btn = rowObj.GetComponent<Button>();
            if (img != null) img.sprite = item.icon;
            if (btn != null) btn.onClick.AddListener(() => AbrirPopupEquipar(item));
        }
    }

    // --- 2. LÓGICA DE EQUIPAMIENTO ---
    private void EquiparItem(ItemBase item)
    {
        if (item is Pasivo)
        {
            GameManager.Instance.pasivoEquipadoID = item.id;
        }
        else if (item is Consumible)
        {
            // Asegurarnos de que la lista tiene al menos 2 huecos para evitar errores "Out Of Bounds"
            while (GameManager.Instance.activosEquipadosIDs.Count < 2)
            {
                GameManager.Instance.activosEquipadosIDs.Add("");
            }

            // Comprobamos si ya está equipado para no duplicarlo
            if (GameManager.Instance.activosEquipadosIDs.Contains(item.id)) return;

            // Lógica para alternar entre el hueco 1 y 2
            if (string.IsNullOrEmpty(GameManager.Instance.activosEquipadosIDs[0]))
            {
                GameManager.Instance.activosEquipadosIDs[0] = item.id;
            }
            else if (string.IsNullOrEmpty(GameManager.Instance.activosEquipadosIDs[1]))
            {
                GameManager.Instance.activosEquipadosIDs[1] = item.id;
            }
            else
            {
                // Si ambos están llenos, sobrescribimos el primero por defecto
                GameManager.Instance.activosEquipadosIDs[0] = item.id;
            }
        }

        ActualizarUIEquipamiento();
        GuardarEquipamientoEnFirebase();
    }

    // --- 3. DESEQUIPAR DESDE LA UI ---
    // Puedes asignar estas funciones al evento OnClick de los iconos equipados
    public void DesequiparPasivo()
    {
        string idPasivo = GameManager.Instance.pasivoEquipadoID;
        if (string.IsNullOrEmpty(idPasivo)) return;

        ItemBase item = GameManager.Instance.GetPasivoPorID(idPasivo);
        if (item != null)
        {
            AbrirPopupDesequipar(item, 0);
        }
    }

    public void DesequiparActivo1()
    {
        if (GameManager.Instance.activosEquipadosIDs.Count > 0)
        {
            string idActivo1 = GameManager.Instance.activosEquipadosIDs[0];
            if (string.IsNullOrEmpty(idActivo1)) return;

            ItemBase item = GameManager.Instance.GetActivoPorID(idActivo1);
            if (item != null)
            {
                AbrirPopupDesequipar(item, 1);
            }
        }
    }

    public void DesequiparActivo2()
    {
        if (GameManager.Instance.activosEquipadosIDs.Count > 1)
        {
            string idActivo2 = GameManager.Instance.activosEquipadosIDs[1];
            if (string.IsNullOrEmpty(idActivo2)) return;

            ItemBase item = GameManager.Instance.GetActivoPorID(idActivo2);
            if (item != null)
            {
                AbrirPopupDesequipar(item, 2);
            }
        }
    }

    // --- 4. ACTUALIZAR VISUALES Y GUARDAR ---
    private void ActualizarUIEquipamiento()
    {
        // Pasivo
        string idPasivo = GameManager.Instance.pasivoEquipadoID;
        if (!string.IsNullOrEmpty(idPasivo))
        {
            Pasivo p = GameManager.Instance.GetPasivoPorID(idPasivo);
            if (p != null)
            {
                iconoPasivo.sprite = p.icon;
                iconoPasivo.preserveAspect = true;
                iconoPasivo.enabled = true;
            }
        }
        else
        {
            iconoPasivo.enabled = false;
        }

        // Activo 1
        string idActivo1 = GameManager.Instance.activosEquipadosIDs[0];
        if (!string.IsNullOrEmpty(idActivo1))
        {
            Consumible c1 = GameManager.Instance.GetActivoPorID(idActivo1);
            if (c1 != null)
            {
                iconoActivo1.sprite = c1.icon;
                iconoActivo1.preserveAspect = true;
                iconoActivo1.enabled = true;
            }
        }
        else
        {
            iconoActivo1.enabled = false;
        }

        // Activo 2
        string idActivo2 = GameManager.Instance.activosEquipadosIDs[1];
        if (!string.IsNullOrEmpty(idActivo2))
        {
            Consumible c2 = GameManager.Instance.GetActivoPorID(idActivo2);
            if (c2 != null)
            {
                iconoActivo2.sprite = c2.icon;
                iconoActivo2.preserveAspect = true;
                iconoActivo2.enabled = true;
            }
        }
        else
        {
            iconoActivo2.enabled = false;
        }
    }

    private async void GuardarEquipamientoEnFirebase()
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            Usuario user = SessionManager.shared.currentUser;
            user.pasivo_equipado = GameManager.Instance.pasivoEquipadoID;
            user.objetos_equipados = GameManager.Instance.activosEquipadosIDs;

            if (usuarioService == null) usuarioService = new UsuarioService();

            bool ok = await usuarioService.ActualizarUsuario(user);
            if (ok)
            {
                Debug.Log("Equipamiento guardado en Firebase correctamente.");
            }
            else
            {
                Debug.LogError("Error al guardar el equipamiento en Firebase.");
            }
        }
    }

    // --- UTILIDADES ---
    private ItemBase ObtenerItemPorID(string id)
    {
        Pasivo pasivo = GameManager.Instance.GetPasivoPorID(id);
        if (pasivo != null) return pasivo;

        Consumible consumible = GameManager.Instance.GetActivoPorID(id);
        if (consumible != null) return consumible;

        return null;
    }

    // --- POPUP DE EQUIPAMIENTO ---
    private void AbrirPopupEquipar(ItemBase item)
    {
        if (item == null) return;
        itemSeleccionado = item;

        if (panelEquiparConfirmacion != null)
        {
            panelEquiparConfirmacion.SetActive(true);

            if (txtNombreEquipar != null) txtNombreEquipar.text = item.itemBaseName;
            if (txtDescripcionEquipar != null) txtDescripcionEquipar.text = item.description;
            if (iconoEquipar != null) iconoEquipar.sprite = item.icon;
        }
        else
        {
            ConfirmarEquipar();
        }
    }

    private void ConfirmarEquipar()
    {
        if (itemSeleccionado != null)
        {
            EquiparItem(itemSeleccionado);
        }
        CerrarPopupEquipar();
    }

    private void CerrarPopupEquipar()
    {
        itemSeleccionado = null;
        if (panelEquiparConfirmacion != null)
        {
            panelEquiparConfirmacion.SetActive(false);
        }
    }

    // --- POPUP DE DESEQUIPAR ---
    private void AbrirPopupDesequipar(ItemBase item, int slot)
    {
        itemADesequipar = item;
        slotADesequipar = slot;

        if (panelDesequiparConfirmacion != null)
        {
            panelDesequiparConfirmacion.SetActive(true);
            panelDesequiparConfirmacionFade.SetActive(false);
            if (txtNombreDesequipar != null) txtNombreDesequipar.text = item.itemBaseName;
        }
        else
        {
            ConfirmarDesequipar();
        }
    }

    private void ConfirmarDesequipar()
    {
        if (slotADesequipar == 0)
        {
            GameManager.Instance.pasivoEquipadoID = "";
        }
        else if (slotADesequipar == 1)
        {
            if (GameManager.Instance.activosEquipadosIDs.Count > 0)
                GameManager.Instance.activosEquipadosIDs[0] = "";
        }
        else if (slotADesequipar == 2)
        {
            if (GameManager.Instance.activosEquipadosIDs.Count > 1)
                GameManager.Instance.activosEquipadosIDs[1] = "";
        }

        ActualizarUIEquipamiento();
        GuardarEquipamientoEnFirebase();
        CerrarPopupDesequipar();
    }

    private void CerrarPopupDesequipar()
    {
        itemADesequipar = null;
        slotADesequipar = -1;
        if (panelDesequiparConfirmacion != null)
        {
            panelDesequiparConfirmacion.SetActive(false);
            panelDesequiparConfirmacionFade.SetActive(false);
        }
    }

    // --- TRUCO PARA PROBAR EL INVENTARIO ---
    public void BotonTruco_DarObjetos()
    {
        // Añadimos TODOS los objetos pasivos disponibles
        foreach (var pasivo in GameManager.Instance.todosLosPasivos)
        {
            GameManager.Instance.inventarioIDs.Add(pasivo.id);
        }
        
        // Añadimos TODOS los objetos activos disponibles
        foreach (var activo in GameManager.Instance.todosLosActivos)
        {
            GameManager.Instance.inventarioIDs.Add(activo.id);
        }

        Debug.Log("¡Todos los objetos regalados con éxito! Actualizando mochila...");
        
        // Recargamos visualmente
        CargarInventario();
        
        // Guardamos en Firebase para que se queden para siempre
        GuardarEquipamientoEnFirebase();
    }
}
