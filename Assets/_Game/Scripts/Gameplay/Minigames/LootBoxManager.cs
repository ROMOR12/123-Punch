using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Gestiona la lógica de recompensa de la LootBox.
/// El flujo visual (clicks, explosión) lo maneja CajaClickerFX.
/// Este script calcula el premio al entrar en la escena y lo tiene listo
/// para cuando CajaClickerFX active el panelRecompensa.
/// </summary>
public class LootBoxManager : MonoBehaviour
{
    [Header("UI del Panel de Recompensa")]
    [Tooltip("El mismo panelRecompensa que tienes en CajaClickerFX")]
    public GameObject panelRecompensa; // Arrastra aquí el mismo panelRecompensa del CajaClickerFX
    public Image iconoPremio;
    public TMP_Text textoNombrePremio;
    public TMP_Text textoRarezaPremio;

    [Header("Botones de navegación")]
    public Button btnSalirAlMenu;
    public Button btnVolverAAbrir; // Visible si tiene más cajas
    public Button btnComprar;      // Visible si no tiene cajas (cuesta 1000 monedas)

    private Usuario user;
    private UsuarioService usuarioService;
    private bool botonesYaMostrados = false;

    private async void Start()
    {
        usuarioService = new UsuarioService();
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
            user = SessionManager.shared.currentUser;

        // Conectar botones
        if (btnSalirAlMenu != null) btnSalirAlMenu.onClick.AddListener(SalirAlMenu);
        if (btnVolverAAbrir != null) btnVolverAAbrir.onClick.AddListener(VolverAAbrir);
        if (btnComprar != null) btnComprar.onClick.AddListener(ComprarYVolver);

        // Ocultar botones hasta que explote la caja
        MostrarBotonesNavegacion(false);

        // Calcular y guardar el premio YA, para tenerlo listo cuando la caja explote
        await PrepararPremio();
    }

    private void Update()
    {
        // Vigilamos si CajaClickerFX ha activado el panelRecompensa
        // Cuando lo detectamos, mostramos los botones de navegación
        if (!botonesYaMostrados && panelRecompensa != null && panelRecompensa.activeInHierarchy)
        {
            botonesYaMostrados = true;
            MostrarBotonesNavegacion(true);
        }
    }

    /// <summary>
    /// Calcula el premio por RNG, lo guarda en Firebase y rellena la UI del panel.
    /// El panel sigue oculto; CajaClickerFX lo mostrará al explotar.
    /// </summary>
    private async Task PrepararPremio()
    {
        if (user == null)
        {
            Debug.LogError("LootBoxManager: No hay usuario de sesión.");
            return;
        }

        try
        {
            // 1. Restar 1 caja
            user.lootboxes = Mathf.Max(0, user.lootboxes - 1);

            // 2. Tirada de rareza — nombres en español para coincidir con el enum ItemRarity
            float roll = Random.Range(0f, 100f);
            string rareza = roll < 0.5f ? "Ilegal"      // 0.5% — ultra raro
                          : roll < 1.5f ? "Legendario"  // 1%
                          : roll < 10f  ? "Epico"       // 8.5%
                          : roll < 40f  ? "Raro"        // 30%
                          : "Comun";                    // 60%

            // 3. Filtrar items por rareza y tipo
            // Los pasivos son permanentes, por eso son muy poco probables (10%)
            List<ItemBase> candidatos = new List<ItemBase>();
            bool darPasivo = Random.Range(0f, 100f) < 10f;

            if (GameManager.Instance != null)
            {
                if (darPasivo && GameManager.Instance.todosLosPasivos != null)
                {
                    foreach (var p in GameManager.Instance.todosLosPasivos)
                    {
                        if (p != null && p.rarity.ToString() == rareza)
                        {
                            // Solo añadimos el pasivo si el jugador NO lo tiene en su inventario
                            bool yaLoTiene = user.inventario != null && user.inventario.Contains(p.id);
                            if (!yaLoTiene)
                            {
                                candidatos.Add(p);
                            }
                        }
                    }
                }

                // Si toca activo (90%), o si tocaba pasivo pero ya tenemos todos los de esa rareza, buscamos activos de esta rareza
                if (candidatos.Count == 0 && GameManager.Instance.todosLosActivos != null)
                {
                    foreach (var a in GameManager.Instance.todosLosActivos)
                    {
                        if (a != null && a.rarity.ToString() == rareza)
                        {
                            candidatos.Add(a);
                        }
                    }
                }
            }

            // 4. Dar el premio
            if (candidatos.Count > 0)
            {
                ItemBase premio = candidatos[Random.Range(0, candidatos.Count)];

                if (string.IsNullOrEmpty(premio.id))
                {
                    Debug.LogError($"¡ATENCIÓN! El item '{premio.itemBaseName}' en tu GameManager tiene su ID vacío. Asígnale un ID único en el Inspector de Unity.");
                }
                else
                {
                    if (user.inventario == null) user.inventario = new List<string>();
                    user.inventario.Add(premio.id);

                    // Sincronizar también la lista local del GameManager (si es una lista distinta)
                    if (GameManager.Instance?.inventarioIDs != null && GameManager.Instance.inventarioIDs != user.inventario)
                        GameManager.Instance.inventarioIDs.Add(premio.id);
                }

                // 5. Rellenar UI del panel (el panel sigue oculto, CajaClickerFX lo activará)
                if (iconoPremio != null)        iconoPremio.sprite = premio.icon;
                if (textoNombrePremio != null)  textoNombrePremio.text = premio.itemBaseName;
                if (textoRarezaPremio != null)  textoRarezaPremio.text = rareza;

                Debug.Log($"Premio preparado: {premio.itemBaseName} ({rareza})");
            }
            else
            {
                Debug.LogError("LootBoxManager: No hay items en el GameManager. Configura todosLosPasivos / todosLosActivos.");
            }

            // 6. Guardar cajas e inventario en Firebase
            await usuarioService.ActualizarUsuario(user);
            Debug.Log("Caja y premio guardados en Firebase.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LootBoxManager - Error preparando premio: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Llamado por CajaClickerFX justo después de mostrar el panel de recompensa.
    /// Activa los botones correctos según las cajas que quedan.
    /// </summary>
    public void OnPanelRecompensaVisible()
    {
        MostrarBotonesNavegacion(true);
    }

    private void MostrarBotonesNavegacion(bool mostrar)
    {
        bool tieneMasCapas = user != null && user.lootboxes > 0;
        bool puedComprar   = user != null && user.free_coin >= 1000;

        if (btnSalirAlMenu != null)   btnSalirAlMenu.gameObject.SetActive(mostrar);
        if (btnVolverAAbrir != null)  btnVolverAAbrir.gameObject.SetActive(mostrar && tieneMasCapas);
        if (btnComprar != null)       btnComprar.gameObject.SetActive(mostrar && !tieneMasCapas && puedComprar);
    }

    private void VolverAAbrir()
    {
        // Recargamos la misma escena para abrir otra caja
        CargaEscena.Cargar("LootBoxScene");
    }

    private async void ComprarYVolver()
    {
        if (user == null || user.free_coin < 1000) return;

        user.free_coin -= 1000;
        user.lootboxes += 1;
        await usuarioService.ActualizarUsuario(user);

        // Recargamos la escena para que salga la animación de nuevo
        CargaEscena.Cargar("LootBoxScene");
    }

    public void SalirAlMenu()
    {
        CargaEscena.Cargar("Menu");
    }
}
