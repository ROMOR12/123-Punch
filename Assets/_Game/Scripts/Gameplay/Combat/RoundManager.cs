using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Firebase.Functions;
using System.Threading.Tasks;

public class RoundManager : MonoBehaviour
{
    [Header("Configuraci�n Visual")]
    public Image[] circulosRonda;
    public Sprite spriteVacio;
    public Sprite spriteJugador;
    public Sprite spriteEnemigo;

    [Header("Referencias")]
    public CombatController playerCombat;
    public EnemyBot enemyBot;
    public GameObject pantallaVictoriaFinal;
    public GameObject pantallaDerrotaFinal;

    [Header("Estad�sticas Finales")]
    public ResultScreenUI pantallaResultados;
    private float tiempoInicioCombate;

    private int victoriasJugador = 0;
    private int victoriasEnemigo = 0;
    private int rondaActual = 0;

    private FirebaseFunctions functions;

    void Start()
    {
        functions = FirebaseFunctions.DefaultInstance;
        // inicializamos los circulos que vamos a usar para mostrar las rondas
        foreach (var img in circulosRonda)
        {
            if (img != null) img.sprite = spriteVacio;
        }
    }
    public void RegistrarFinDeRonda(bool ganoJugador)
    {
        // si la ronda actual es mayor o igual al numero de circulos, no hacemos nada
        if (rondaActual >= circulosRonda.Length) return;

        // si gano el jugador, ponemos el sprite del jugador en el circulo de la ronda actual
        if (ganoJugador)
        {
            // asignamos el sprite del jugador al circulo de la ronda actual
            circulosRonda[rondaActual].sprite = spriteJugador;
            // sumamos una victoria al jugador
            victoriasJugador++;
        }
        // si gano el enemigo, ponemos el sprite del enemigo en el circulo de la ronda actual
        else
        {
            circulosRonda[rondaActual].sprite = spriteEnemigo;
            victoriasEnemigo++;
        }

        // incrementamos la ronda actual
        rondaActual++;

        // comprobamos si el jugador tiene 2 victorias
        if (victoriasJugador >= 2)
        {
            // llamamos al metodo de fin del juego con el parametro true
            FinDelJuego(true);
        }
        else if (victoriasEnemigo >= 2)
        {
            // llamamos al metodo de fin del juego con el parametro false
            FinDelJuego(false);
        }
        else
        {
            // reiniciamos la ronda despues de un tiempo
            StartCoroutine(ReiniciarRonda());
        }
    }

    IEnumerator ReiniciarRonda()
    {
        // esperamos 3 segundos antes de reiniciar la ronda
        yield return new WaitForSeconds(3f);

        // reiniciamos los combatientes para la siguiente ronda
        SoundManager.PlayBackgroundMusic();
        playerCombat.ReiniciarParaRonda();
        enemyBot.ReiniciarParaRonda();
    }
    void FinDelJuego(bool ganoJugador)
    {
        if (enemyBot != null)
        {
            enemyBot.StopAllCoroutines();
            enemyBot.enabled = false;
        }

        // Calcula cu�nto ha durado la pelea
        float duracionCombate = Time.time - tiempoInicioCombate;
        SoundManager.StopMusic();

        // mostramos la pantalla de victoria o derrota segun corresponda
        if (ganoJugador)
        {
            if (pantallaVictoriaFinal != null) pantallaVictoriaFinal.SetActive(true);
            SoundManager.PlaySound(SoundType.Win);
            if (playerCombat != null) playerCombat.CelebrarVictoria();

            _ = ReclamarRecompensaServidor();

            // Trigger que salta cuando el jugador gana, para los logros
            GameEvents.TriggerFightWon();

            if (pantallaResultados != null)
            {
                pantallaResultados.MostrarResultados(
                    ganoJugador = true,
                    playerCombat.contadorGolpes,
                    duracionCombate,
                    playerCombat.contadorTotalDamage
                );

                GameManager.Instance.numCajas++;
                if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
                    SessionManager.shared.currentUser.lootboxes = GameManager.Instance.numCajas;

            }
        }
                else
        {
            if (pantallaDerrotaFinal != null) pantallaDerrotaFinal.SetActive(true);
            GameEvents.TriggerFightLost();

            if (pantallaResultados != null)
            {
                pantallaResultados.MostrarResultados(
                    ganoJugador = false,
                    playerCombat.contadorGolpes,
                    duracionCombate,
                    playerCombat.contadorTotalDamage
                );
            }
        }
    }
    private async Task ReclamarRecompensaServidor()
    {
        try
        {
            Debug.Log("Pidiendo recompensa al servidor...");

            // Llamamos a la funci�n con el nombre exacto que le pusimos en Node.js
            HttpsCallableReference callable = functions.GetHttpsCallable("recompensarCombate");
            HttpsCallableResult result = await callable.CallAsync();

            Debug.Log("�Recompensa validada! El servidor ha sumado 100 monedas a la BD.");

            // Actualizamos la memoria local para que la UI o la tienda lo sepan inmediatamente
            if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
            {
                SessionManager.shared.currentUser.free_coin += 100;
                Debug.Log($"Monedas totales en memoria: {SessionManager.shared.currentUser.free_coin}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error en el servidor al reclamar recompensa: {e.Message}");
        }
    }
}
