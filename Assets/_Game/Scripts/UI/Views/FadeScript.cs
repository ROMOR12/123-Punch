using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup faderCanvasGroup; // Panel del fade

    [Header("Configuración")]
    public float velocidadFade = 1.0f; // Velocidad a la que va a ir el fade

    [Header("Automático (Intro/Logo)")]
    public bool esAutomatico = false; // Si el cambio de escenas no requere una acción del usuario como pulsar un boton
    public float tiempoEspera = 2.0f; // Tiempo que espera antes de cambiar de escena
    public string escenaDestino = ""; // Escena a la que va a ir

    void Start()
    {
        faderCanvasGroup.alpha = 1; // La transpariencia de la pantalla empieza en negro total
        faderCanvasGroup.blocksRaycasts = true; // Desactiva los toques en lapantalla

        StartCoroutine(HacerFadeIn()); // Iniciamos una corrutina para hacer la animacion de fade al entrar

        if (esAutomatico) // Si el cambio de escena es un proceso automatico y el jugador no interviene
        {
            StartCoroutine(ProcesoAutomatico()); // Iniciamos el cambio de escena
        }
    }

    public void CambiarEscena(string nombreEscena)
    {
        StopAllCoroutines();

        StartCoroutine(HacerFadeOut(nombreEscena)); // Llamamos al metodo que va a hacer el fade al salir
    }


    IEnumerator HacerFadeIn()
    {
        // Hacemos el fade empezado por negro total y añadiendo trasnparencia poco a poco
        while (faderCanvasGroup.alpha > 0)
        {
            // Time.deltaTime devuelve el tiempo que a pasado desde el ultimo frame, si este lo multiplicamos
            // por la velocidad del fade, el fade siempre durara lo mismo, da igual los FPS que tengas.
            // Si no usamos el Time.DeltaTime, lo que pasaria es que segun el dispositivo, la animacion
            // de fade vaya mas rapido y lento segun los FPS del dispotivo, asi nos aseguramos que dure lo
            // en todos los dispotivos
            faderCanvasGroup.alpha -= Time.deltaTime * velocidadFade;
            yield return null;
        }
        faderCanvasGroup.blocksRaycasts = false; // Desbloqueamos los toques
    }

    IEnumerator HacerFadeOut(string escena)
    {
        faderCanvasGroup.blocksRaycasts = true; // Bloqueamos los toques en pantalla durante el fade out

        // Hacemos el fade out aumentando la transparencia del panel poco a poco
        while (faderCanvasGroup.alpha < 1)
        {
            faderCanvasGroup.alpha += Time.deltaTime * velocidadFade;
            yield return null;
        }

        SceneManager.LoadScene(escena); // Cargamos la escena pasada por parametro
    }

    IEnumerator ProcesoAutomatico()
    {
        // Esperamos a que termine el fade in antes de iniciar la espera
        yield return new WaitForSeconds((1f / velocidadFade) + 0.5f);
        yield return new WaitForSeconds(tiempoEspera);

        CambiarEscena(escenaDestino); // LLamamos al metodo para cambiar de escena pasandole el nombre de la escena a la que vamos a ir
    }
}
