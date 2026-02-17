using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Image[] circulosRonda;
    public Sprite spriteVacio;
    public Sprite spriteJugador;
    public Sprite spriteEnemigo;

    [Header("Referencias")]
    public CombatController playerCombat;
    public EnemyBot enemyBot;
    public GameObject pantallaVictoriaFinal;
    public GameObject pantallaDerrotaFinal;

    [Header("Estadísticas Finales")]
    public ResultScreenUI pantallaResultados;
    private float tiempoInicioCombate;

    private int victoriasJugador = 0;
    private int victoriasEnemigo = 0;
    private int rondaActual = 0;

    void Start()
    {
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
            Debug.Log("gano el bot");
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
            enemyBot.StopAllCoroutines(); // Detiene ataques en curso
            enemyBot.enabled = false;     // Apaga su cerebro totalmente
        }

        //Calcula cuánto ha durado la pelea(Ahora - Inicio)
        float duracionCombate = Time.time - tiempoInicioCombate;
        SoundManager.StopMusic();

        // mostramos la pantalla de victoria o derrota segun corresponda
        if (ganoJugador)
        {
            if (pantallaVictoriaFinal != null) pantallaVictoriaFinal.SetActive(true);
            SoundManager.PlaySound(SoundType.Win);
            if (playerCombat != null) playerCombat.CelebrarVictoria();

            if (pantallaResultados != null)
            {
                // true = Ganaste
                // contadorGolpes = Los que sumamos con RegistrarEstadistica
                // duracionCombate = El tiempo calculado
                // contadorDañoTotal = El daño acumulado
                pantallaResultados.MostrarResultados(
                    ganoJugador = true,
                    playerCombat.contadorGolpes,
                    duracionCombate,
                    playerCombat.contadorDañoTotal
                );

                GameManager.Instance.numCajas++;

            }
        }
        else
        {
            if (pantallaResultados != null)
            {
                pantallaResultados.MostrarResultados(
                    ganoJugador = false,
                    playerCombat.contadorGolpes,
                    duracionCombate,
                    playerCombat.contadorDañoTotal
                );
            }
        }
    }
}
