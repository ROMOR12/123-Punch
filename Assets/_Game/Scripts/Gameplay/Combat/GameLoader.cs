using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GameLoader : MonoBehaviour
{
    [Header("Referencias del Combate")]
    public CombatController playerCombat;
    public EnemyBot enemyBot;

    [Header("Configuraci�n del Enemigo")]
    [Tooltip("El ID del enemigo en Firebase si no se carga din�micamente")]
    public string idEnemigo = "bot_facil";

    async void Start()
    {
        // Verificamos que existan las instancias necesarias
        if (GameManager.Instance == null)
        {
            Debug.LogError("No se encontr� el GameManager en la escena.");
            return;
        }

        // Obtenemos el ID del personaje seleccionado en el men�
        string idPersonaje = GameManager.Instance.idPersonajeSeleccionado;

        // Si entramos directo a la escena sin pasar por el men�, usamos uno por defecto
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
        UsuarioService uService = new UsuarioService();
        EnemyService enemyService = new EnemyService();

        // Tarea del enemigo
        var tareaEnemigo = enemyService.ObtenerEnemigo(idEnemigo);

        // Tarea del jugador (buscamos primero en su BD personal)
        Personaje personajeJugador = null;
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            string idUsuario = SessionManager.shared.currentUser.id;
            personajeJugador = await uService.ObtenerPersonajeDeUsuario(idUsuario, idPersonaje);
        }

        // Si no tiene el personaje guardado en su perfil, bajamos las stats base globales
        if (personajeJugador == null)
        {
            personajeJugador = await pjService.ObtenerPersonaje(idPersonaje);
        }

        await tareaEnemigo;

        // APLICAR DATOS AL JUGADOR
        if (personajeJugador != null)
        {
            Personaje p = personajeJugador;
            playerCombat.SobrescribirStatsDeFirebase(p.life, p.energy, p.force, p.recovery);
            Debug.Log($"Stats de {p.name} (Nube) inyectados correctamente.");

            CargarInventarioEnCombate();
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

            // --- CAMBIAR LOS SPRITES DEL ENEMIGO ---
            EnemyBase visualesEnemigo = GameManager.Instance.listaEnemigos.Find(p => p.id == idEnemigo);
            if (visualesEnemigo != null)
            {
                enemyBot.CambiarVisualesEnemigo(visualesEnemigo);
            }
            // ---------------------------------------

            Debug.Log($"Stats del enemigo {e.name} inyectados correctamente.");
        }
    }

    private void CargarInventarioEnCombate()
    {
        if (GameManager.Instance == null) return;

        // 1. Cargar Pasivo
        string idPasivo = GameManager.Instance.pasivoEquipadoID;
        if (!string.IsNullOrEmpty(idPasivo))
        {
            Pasivo pasivoLocal = GameManager.Instance.GetPasivoPorID(idPasivo);
            if (pasivoLocal != null)
            {
                Pasivo pasivo = Instantiate(pasivoLocal); // Clonamos para no sobreescribir el archivo original
                
                if (GlobalDataService.cacheObjetos.TryGetValue(idPasivo, out Objeto datosBD))
                {
                    pasivo.bonificaciones = new List<StatModifier>();
                    if (datosBD.life != 0) pasivo.bonificaciones.Add(new StatModifier { statType = StatType.Life, amount = datosBD.life });
                    if (datosBD.energy != 0) pasivo.bonificaciones.Add(new StatModifier { statType = StatType.Energy, amount = datosBD.energy });
                    if (datosBD.force != 0) pasivo.bonificaciones.Add(new StatModifier { statType = StatType.Force, amount = datosBD.force });
                    if (datosBD.recovery != 0) pasivo.bonificaciones.Add(new StatModifier { statType = StatType.Recovery, amount = datosBD.recovery });
                    pasivo.itemBaseName = datosBD.name;
                    pasivo.description = datosBD.description;
                }

                if (playerCombat.pasivosEquipados == null) playerCombat.pasivosEquipados = new List<Pasivo>();
                playerCombat.pasivosEquipados.Add(pasivo);
                pasivo.Equipar(playerCombat); // Aplica los efectos base
                Debug.Log($"Pasivo equipado (Stats DB): {pasivo.itemBaseName}");
            }
        }

        // 2. Cargar Consumibles (Activos)
        List<Consumible> consumiblesParaCombate = new List<Consumible>();
        if (GameManager.Instance.activosEquipadosIDs != null)
        {
            foreach (string idActivo in GameManager.Instance.activosEquipadosIDs)
            {
                if (!string.IsNullOrEmpty(idActivo))
                {
                    Consumible activoLocal = GameManager.Instance.GetActivoPorID(idActivo);
                    if (activoLocal != null) 
                    {
                        Consumible activo = Instantiate(activoLocal); // Clonamos

                        if (GlobalDataService.cacheObjetos.TryGetValue(idActivo, out Objeto datosBD))
                        {
                            activo.recuperacion = new List<StatModifier>();
                            if (datosBD.life != 0) activo.recuperacion.Add(new StatModifier { statType = StatType.Life, amount = datosBD.life });
                            if (datosBD.energy != 0) activo.recuperacion.Add(new StatModifier { statType = StatType.Energy, amount = datosBD.energy });
                            if (datosBD.force != 0) activo.recuperacion.Add(new StatModifier { statType = StatType.Force, amount = datosBD.force });
                            if (datosBD.recovery != 0) activo.recuperacion.Add(new StatModifier { statType = StatType.Recovery, amount = datosBD.recovery });
                            activo.itemBaseName = datosBD.name;
                            activo.description = datosBD.description;
                        }
                        
                        consumiblesParaCombate.Add(activo);
                    }
                }
            }
        }

        if (playerCombat.inventory != null)
        {
            playerCombat.inventory.Inicializar(playerCombat, consumiblesParaCombate);
            Debug.Log($"Consumibles inicializados (Stats DB): {consumiblesParaCombate.Count}");
        }
    }
}