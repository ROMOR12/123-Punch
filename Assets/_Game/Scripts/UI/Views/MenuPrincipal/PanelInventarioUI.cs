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
    private string itemSeleccionadoID;
    private int slotADesequipar = -1;
    private string itemADesequiparID;

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

    private async void OnEnable()
    {
        if (panelEquiparConfirmacion != null) panelEquiparConfirmacion.SetActive(false);
        if (panelDesequiparConfirmacion != null) panelDesequiparConfirmacion.SetActive(false);

        GlobalDataService globalData = new GlobalDataService();
        await globalData.CargarObjetosGlobales();

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

    private void CargarInventario()
    {
        foreach (Transform child in gridInventario)
        {
            Destroy(child.gameObject);
        }

        if (GameManager.Instance == null || GameManager.Instance.inventarioIDs == null) return;

        Dictionary<string, int> conteoItems = new Dictionary<string, int>();
        foreach (string itemID in GameManager.Instance.inventarioIDs)
        {
            if (string.IsNullOrEmpty(itemID)) continue;
            if (itemID == "Item_LootBox") continue;

            if (conteoItems.ContainsKey(itemID)) conteoItems[itemID]++;
            else conteoItems[itemID] = 1;
        }

        foreach (var kvp in conteoItems)
        {
            string itemID = kvp.Key;
            int cantidad = kvp.Value;

            if (!GlobalDataService.cacheObjetos.TryGetValue(itemID, out Objeto datosFirebase))
            {
                continue;
            }

            Sprite icono = ObtenerIconoLocal(itemID);
            CrearBotonItem(itemID, datosFirebase, icono, cantidad);
        }
    }

    private Sprite ObtenerIconoLocal(string itemID)
    {
        Sprite icono = GameManager.Instance.imageDefault;
        if (GameManager.Instance.todosLosPasivos != null)
        {
            foreach (var p in GameManager.Instance.todosLosPasivos)
                if (p != null && p.id == itemID) return p.icon;
        }
        if (GameManager.Instance.todosLosActivos != null)
        {
            foreach (var a in GameManager.Instance.todosLosActivos)
                if (a != null && a.id == itemID) return a.icon;
        }
        if (GameManager.Instance.items != null)
        {
            foreach (var i in GameManager.Instance.items)
                if (i != null && i.id == itemID) return i.icon;
        }
        return icono;
    }

    private void CrearBotonItem(string itemID, Objeto datosFirebase, Sprite icono, int cantidad)
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

        InventarioRowUI rowUI = rowObj.GetComponent<InventarioRowUI>();

        if (rowUI != null)
        {
            if (rowUI.textCantidad != null) rowUI.textCantidad.text = $"x{cantidad}";
            if (rowUI.iconoItem != null) rowUI.iconoItem.sprite = icono;
            if (rowUI.textNombre != null) rowUI.textNombre.text = datosFirebase.name;
            if (rowUI.textRareza != null) rowUI.textRareza.text = datosFirebase.rarity;
            
            if (rowUI.textTipo != null)
            {
                rowUI.textTipo.text = datosFirebase.is_passive ? "Pasivo" : "Activo";
            }

            if (rowUI.botonEquipar != null)
            {
                rowUI.botonEquipar.onClick.AddListener(() => AbrirPopupEquipar(itemID, datosFirebase, icono));
            }
        }
        else
        {
            Image img = rowObj.GetComponent<Image>();
            Button btn = rowObj.GetComponent<Button>();
            if (img != null) img.sprite = icono;
            if (btn != null) btn.onClick.AddListener(() => AbrirPopupEquipar(itemID, datosFirebase, icono));
        }
    }

    private void EquiparItem(string itemID, Objeto datos)
    {
        if (datos.is_passive)
        {
            GameManager.Instance.pasivoEquipadoID = itemID;
        }
        else
        {
            while (GameManager.Instance.activosEquipadosIDs.Count < 2)
            {
                GameManager.Instance.activosEquipadosIDs.Add("");
            }

            if (GameManager.Instance.activosEquipadosIDs.Contains(itemID)) return;

            if (string.IsNullOrEmpty(GameManager.Instance.activosEquipadosIDs[0]))
            {
                GameManager.Instance.activosEquipadosIDs[0] = itemID;
            }
            else if (string.IsNullOrEmpty(GameManager.Instance.activosEquipadosIDs[1]))
            {
                GameManager.Instance.activosEquipadosIDs[1] = itemID;
            }
            else
            {
                GameManager.Instance.activosEquipadosIDs[0] = itemID;
            }
        }

        ActualizarUIEquipamiento();
        GuardarEquipamientoEnFirebase();
    }

    public void DesequiparPasivo()
    {
        string idPasivo = GameManager.Instance.pasivoEquipadoID;
        if (string.IsNullOrEmpty(idPasivo)) return;

        if (GlobalDataService.cacheObjetos.TryGetValue(idPasivo, out Objeto datos))
        {
            AbrirPopupDesequipar(idPasivo, datos, 0);
        }
    }

    public void DesequiparActivo1()
    {
        if (GameManager.Instance.activosEquipadosIDs.Count > 0)
        {
            string idActivo1 = GameManager.Instance.activosEquipadosIDs[0];
            if (string.IsNullOrEmpty(idActivo1)) return;

            if (GlobalDataService.cacheObjetos.TryGetValue(idActivo1, out Objeto datos))
            {
                AbrirPopupDesequipar(idActivo1, datos, 1);
            }
        }
    }

    public void DesequiparActivo2()
    {
        if (GameManager.Instance.activosEquipadosIDs.Count > 1)
        {
            string idActivo2 = GameManager.Instance.activosEquipadosIDs[1];
            if (string.IsNullOrEmpty(idActivo2)) return;

            if (GlobalDataService.cacheObjetos.TryGetValue(idActivo2, out Objeto datos))
            {
                AbrirPopupDesequipar(idActivo2, datos, 2);
            }
        }
    }

    private void ActualizarUIEquipamiento()
    {
        string idPasivo = GameManager.Instance.pasivoEquipadoID;
        if (!string.IsNullOrEmpty(idPasivo))
        {
            iconoPasivo.sprite = ObtenerIconoLocal(idPasivo);
            iconoPasivo.preserveAspect = true;
            iconoPasivo.enabled = true;
        }
        else
        {
            iconoPasivo.enabled = false;
        }

        string idActivo1 = GameManager.Instance.activosEquipadosIDs.Count > 0 ? GameManager.Instance.activosEquipadosIDs[0] : "";
        if (!string.IsNullOrEmpty(idActivo1))
        {
            iconoActivo1.sprite = ObtenerIconoLocal(idActivo1);
            iconoActivo1.preserveAspect = true;
            iconoActivo1.enabled = true;
        }
        else
        {
            iconoActivo1.enabled = false;
        }

        string idActivo2 = GameManager.Instance.activosEquipadosIDs.Count > 1 ? GameManager.Instance.activosEquipadosIDs[1] : "";
        if (!string.IsNullOrEmpty(idActivo2))
        {
            iconoActivo2.sprite = ObtenerIconoLocal(idActivo2);
            iconoActivo2.preserveAspect = true;
            iconoActivo2.enabled = true;
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

    private void AbrirPopupEquipar(string itemID, Objeto datos, Sprite icono)
    {
        itemSeleccionadoID = itemID;

        if (panelEquiparConfirmacion != null)
        {
            panelEquiparConfirmacion.SetActive(true);

            if (txtNombreEquipar != null) txtNombreEquipar.text = datos.name;
            if (txtDescripcionEquipar != null) txtDescripcionEquipar.text = datos.description;
            if (iconoEquipar != null) iconoEquipar.sprite = icono;
        }
        else
        {
            ConfirmarEquipar();
        }
    }

    private void ConfirmarEquipar()
    {
        if (!string.IsNullOrEmpty(itemSeleccionadoID) && GlobalDataService.cacheObjetos.TryGetValue(itemSeleccionadoID, out Objeto datos))
        {
            EquiparItem(itemSeleccionadoID, datos);
        }
        CerrarPopupEquipar();
    }

    private void CerrarPopupEquipar()
    {
        itemSeleccionadoID = "";
        if (panelEquiparConfirmacion != null)
        {
            panelEquiparConfirmacion.SetActive(false);
        }
    }

    private void AbrirPopupDesequipar(string itemID, Objeto datos, int slot)
    {
        itemADesequiparID = itemID;
        slotADesequipar = slot;

        if (panelDesequiparConfirmacion != null)
        {
            panelDesequiparConfirmacion.SetActive(true);
            if (panelDesequiparConfirmacionFade != null) panelDesequiparConfirmacionFade.SetActive(true);
            if (txtNombreDesequipar != null) txtNombreDesequipar.text = datos.name;
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
        itemADesequiparID = "";
        slotADesequipar = -1;
        if (panelDesequiparConfirmacion != null)
        {
            panelDesequiparConfirmacion.SetActive(false);
            if (panelDesequiparConfirmacionFade != null) panelDesequiparConfirmacionFade.SetActive(false);
        }
    }

    public void BotonTruco_DarObjetos()
    {
        foreach (var objeto in GlobalDataService.cacheObjetos.Values)
        {
            if (objeto.id_Objeto != "Item_LootBox")
            {
                GameManager.Instance.inventarioIDs.Add(objeto.id_Objeto);
            }
        }
        
        CargarInventario();
        GuardarEquipamientoEnFirebase();
    }
}
