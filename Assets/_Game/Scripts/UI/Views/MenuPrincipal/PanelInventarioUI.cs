using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PanelInventarioUI : MonoBehaviour
{
    [Header("Referencias a Huecos de Equipamiento")]
    public Image iconoPasivo;
    public Image iconoActivo1;
    public Image iconoActivo2;

    [Header("Grid de Inventario (Mochila)")]
    public Transform gridInventario;
    public GameObject prefabItemInventario; // Debe tener un Image y un Button

    [Header("Servicios")]
    private UsuarioService usuarioService;

    private void Awake()
    {
        usuarioService = new UsuarioService();
    }

    private void OnEnable()
    {
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
                rowUI.botonEquipar.onClick.AddListener(() => EquiparItem(item));
            }
        }
        else
        {
            // Si el usuario aún no ha puesto el script, hacemos fallback para que no pete
            Image img = rowObj.GetComponent<Image>();
            Button btn = rowObj.GetComponent<Button>();
            if (img != null) img.sprite = item.icon;
            if (btn != null) btn.onClick.AddListener(() => EquiparItem(item));
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
        GameManager.Instance.pasivoEquipadoID = "";
        ActualizarUIEquipamiento();
        GuardarEquipamientoEnFirebase();
    }

    public void DesequiparActivo1()
    {
        GameManager.Instance.activosEquipadosIDs[0] = "";
        ActualizarUIEquipamiento();
        GuardarEquipamientoEnFirebase();
    }

    public void DesequiparActivo2()
    {
        GameManager.Instance.activosEquipadosIDs[1] = "";
        ActualizarUIEquipamiento();
        GuardarEquipamientoEnFirebase();
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
