using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GameLoader : MonoBehaviour
{
    [Header("Referencias del Combate")]
    public CombatController playerCombat;
    public EnemyBot enemyBot;

    [Header("Configuración del Enemigo")]
    [Tooltip("El ID del enemigo en Firebase si no se carga dinámicamente")]
    public string idEnemigo = "bot_facil";

    async void Start()
    {
        // Verificamos que existan las instancias necesarias
        if (GameManager.Instance == null)
        {
            Debug.LogError("No se encontró el GameManager en la escena.");
            return;
        }

        // Obtenemos el ID del personaje seleccionado en el menú
        string idPersonaje = GameManager.Instance.idPersonajeSeleccionado;

        // Si entramos directo a la escena sin pasar por el menú, usamos uno por defecto
        if (string.IsNullOrEmpty(idPersonaje))
        {
            Debug.LogWarning("No hay personaje seleccionado. Usando 'ava' por defecto.");
            idPersonaje = "ava";
        }

        // BUSCAR VISUALES: Buscamos el ScriptableObject en la lista del GameManager
        // Usamos el id para encontrar el personaje que tiene los Sprites correctos
        BaseCharacter visualesPersonaje = GameManager.Instance.listaPersonajes.Find(p => p.id == idPersonaje);

        if (visualesPersonaje != null)
        {
            // Le pasamos el ScriptableObject al CombatController para que cambie el Sprite
            playerCombat.CambiarVisualesPersonaje(visualesPersonaje);
        }
        else
        {
            Debug.LogError($"No se encontraron los visuales para el ID: {idPersonaje}");
        }

        Debug.Log($"Cargando datos de Firebase: {idPersonaje} vs {idEnemigo}");

        // INICIALIZAR SERVICIOS Y DESCARGAR STATS
        PersonajeService pjService = new PersonajeService();
        EnemyService enemyService = new EnemyService();

        // Lanzamos ambas descargas en paralelo para que el juego cargue más rápido
        var tareaPJ = pjService.ObtenerPersonaje(idPersonaje);
        var tareaEnemigo = enemyService.ObtenerEnemigo(idEnemigo);

        await Task.WhenAll(tareaPJ, tareaEnemigo);

        // APLICAR DATOS AL JUGADOR
        if (tareaPJ.Result != null)
        {
            Personaje p = tareaPJ.Result;
            playerCombat.SobrescribirStatsDeFirebase(p.life, p.energy, p.force, p.recovery);
            Debug.Log($"Stats de {p.name} (Nube) inyectados correctamente.");
        }
        else
        {
            Debug.LogError($"Error: No se pudieron bajar los stats de {idPersonaje} de Firebase.");
        }

        // APLICAR DATOS AL ENEMIGO
        if (tareaEnemigo.Result != null)
        {
            Enemy e = tareaEnemigo.Result;
            enemyBot.SobrescribirStatsDeFirebase(e.life, e.energy, e.force, e.recovery, e.name);
            playerCombat.ActualizarUIEnemigo(e.life, e.name);
            Debug.Log($"Stats del enemigo {e.name} inyectados correctamente.");
        }

        // Cargar el objeto Pasivo
        playerCombat.pasivosEquipados.Clear(); // Limpiamos la lista actual
        Pasivo objetoPasivo = GameManager.Instance.GetPasivoPorID(GameManager.Instance.pasivoEquipadoID);
        if (objetoPasivo != null)
        {
            playerCombat.pasivosEquipados.Add(objetoPasivo);
            Debug.Log($"Pasivo equipado: {objetoPasivo.name}");
        }

        // Cargar los objetos Activos
        List<Consumible> activosParaPelea = new List<Consumible>();
        foreach (string id in GameManager.Instance.activosEquipadosIDs)
        {
            Consumible c = GameManager.Instance.GetActivoPorID(id);
            if (c != null) activosParaPelea.Add(c);
        }

        // Inicializar la mochila de consumibles en el combate
        if (playerCombat.inventory != null)
        {
            playerCombat.inventory.Inicializar(playerCombat, activosParaPelea);
        }
    }
}