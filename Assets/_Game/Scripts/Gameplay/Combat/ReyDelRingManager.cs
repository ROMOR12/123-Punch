using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using Firebase.Functions;

public class ReyDelRingManager : MonoBehaviour
{
    public static ReyDelRingManager Instance;

    [Header("Configuración del Tiempo")]
    [Tooltip("Tiempo total en segundos")]
    public float tiempoTotal = 180f;

    [Header("Lista de Enemigos (ScriptableObjects)")]
    public List<EnemyBase> listaEnemigosData;

    [Header("Prefab del Enemigo Genérico")]
    public GameObject enemyPrefabBase;

    [Header("Referencias de Escena")]
    public CombatController combatController;
    public Transform puntoSpawnEnemigo;

    [Header("Referencias de UI")]
    public TMP_Text textoTemporizador;
    public TMP_Text textoOleada;

    public bool IsModoActivo { get; private set; } = false;
    private float tiempoRestante;
    private int indiceActual = 0;
    private GameObject enemigoActualInstanciado;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (combatController == null) combatController = FindObjectOfType<CombatController>();

        IsModoActivo = true;
        tiempoRestante = tiempoTotal;
        indiceActual = 0;

        // Desactivamos el RoundManager normal si existe en la escena para que no crear conflictos
        if (combatController.roundManager != null)
        {
            combatController.roundManager.enabled = false;
        }

        SpawnearEnemigo(indiceActual);
    }

    private void Update()
    {
        if (!IsModoActivo) return;

        // Cuenta atrás del tiempo
        tiempoRestante -= Time.deltaTime;
        ActualizarUI();

        if (tiempoRestante <= 0)
        {
            FinPorTiempo();
        }
    }

    private void ActualizarUI()
    {
        if (textoTemporizador != null)
        {
            float minutos = Mathf.FloorToInt(tiempoRestante / 60);
            float segundos = Mathf.FloorToInt(tiempoRestante % 60);
            textoTemporizador.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }

        if (textoOleada != null)
        {
            // Capamos el número para que nunca supere el total de la lista al ganar (evita el 5 / 4)
            int rivalMostrado = Mathf.Min(indiceActual + 1, listaEnemigosData.Count);
            textoOleada.text = $"RIVAL: {rivalMostrado} / {listaEnemigosData.Count}";
        }
    }

    private void SpawnearEnemigo(int index)
    {
        if (index >= listaEnemigosData.Count)
        {
            VictoriaTotal();
            return;
        }

        // Instanciamos el enemigo
        enemigoActualInstanciado = Instantiate(enemyPrefabBase, puntoSpawnEnemigo.position, puntoSpawnEnemigo.rotation);

        // Obtenemos su componente de combate
        EnemyBot enemyBotScript = enemigoActualInstanciado.GetComponent<EnemyBot>();

        // Inyectamos los datos y la referencia del jugador al clon antes de iniciar el combate
        if (enemyBotScript != null)
        {
            enemyBotScript.enemyData = listaEnemigosData[index];
            enemyBotScript.playerCombat = combatController;
        }

        // Vinculamos el nuevo enemigo al controlador
        combatController.SiguienteEnemigoReyDelRing(enemyBotScript);
    }

    public void AlDerrotarEnemigo()
    {
        indiceActual++;
        StartCoroutine(CambiarDeEnemigoRutina());
    }

    // Lógica para el cambio del enemigo
    private IEnumerator CambiarDeEnemigoRutina()
    {
        yield return new WaitForSeconds(1.5f);

        if (enemigoActualInstanciado != null)
        {
            Destroy(enemigoActualInstanciado);
        }

        yield return new WaitForSeconds(1.0f);

        if (IsModoActivo)
        {
            SpawnearEnemigo(indiceActual);
        }
    }

    public void AlMorirJugador()
    {
        IsModoActivo = false;
        Debug.Log("¡El jugador ha sido derrotado en el Rey del Ring!");

        // Evaluamos el progreso para dar la recompensa
        EvaluarRecompensaParcial();

        // Enviamos 'false' porque el jugador ha muerto
        MostrarPantallaResultados(false);
    }

    private void FinPorTiempo()
    {
        IsModoActivo = false;
        tiempoRestante = 0;
        ActualizarUI();
        combatController.DetenerCombate();
        Debug.Log("¡Se acabó el tiempo!");

        // Evaluamos el progreso para dar la recompensa
        EvaluarRecompensaParcial();

        // Enviamos 'false' porque se ha acabado el tiempo
        MostrarPantallaResultados(false);
    }

    private void VictoriaTotal()
    {
        IsModoActivo = false;
        combatController.CelebrarVictoria();
        Debug.Log("¡TE HAS CORONADO REY DEL RING!");

        // Llamada para reclamar monedas
        _ = ReclamarRecompensaServidor("recompensa200", 200);

        // Enviamos 'true' porque ha completado toda la lista con éxito
        MostrarPantallaResultados(true);
    }

    // Recoge las estadísticas del combate
    private void MostrarPantallaResultados(bool esVictoria)
    {
        if (combatController != null && combatController.pantallaResultados != null)
        {
            // Calculamos el tiempo real de juego
            float tiempoJugado = tiempoTotal - tiempoRestante;

            // Extraemos los contadores
            int golpesTotales = combatController.contadorGolpes;
            int danioTotal = combatController.contadorTotalDamage;

            // Rellenamos la tarjeta
            combatController.pantallaResultados.MostrarResultados(esVictoria, golpesTotales, tiempoJugado, danioTotal);
        }
    }

    // Comprueba cuántos enemigos hemos derrotado para asignar las monedas
    private void EvaluarRecompensaParcial()
    {
        if (indiceActual == 0)
        {
            Debug.Log("[Firebase] Cero rivales derrotados. No hay monedas.");
            return;
        }

        if (indiceActual >= 1 && indiceActual <= 2)
        {
            _ = ReclamarRecompensaServidor("recompensa25", 25);
        }
        else if (indiceActual >= 3)
        {
            _ = ReclamarRecompensaServidor("recompensa50", 50);
        }
    }

    // Conexión con el servidor para conseguir las monedas
    private async Task ReclamarRecompensaServidor(string nombreFuncion, int cantidadMonedas)
    {
        try
        {
            Debug.Log($"[Firebase] Solicitando {nombreFuncion} ({cantidadMonedas} monedas)...");

            FirebaseFunctions functions = FirebaseFunctions.DefaultInstance;
            HttpsCallableReference callable = functions.GetHttpsCallable(nombreFuncion);

            await callable.CallAsync();

            if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
            {
                SessionManager.shared.currentUser.free_coin += cantidadMonedas;
                Debug.Log($"[Firebase] ¡Éxito! {cantidadMonedas} monedas añadidas. Total local: {SessionManager.shared.currentUser.free_coin}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Firebase] Error al ejecutar {nombreFuncion}: {e.Message}");
        }
    }
}