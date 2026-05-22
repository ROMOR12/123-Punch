using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup faderCanvasGroup;

    [Header("Configuración")]
    public float velocidadFade = 1.0f;

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
        while (faderCanvasGroup.alpha > 0)
        {
            faderCanvasGroup.alpha -= Time.deltaTime * velocidadFade;
            yield return null;
        }
        faderCanvasGroup.blocksRaycasts = false; // Desbloqueamos los toques
    }

    IEnumerator HacerFadeOut(string escena)
    {
        faderCanvasGroup.blocksRaycasts = true; // Bloqueamos los toques en pantalla durante el fade out

        while (faderCanvasGroup.alpha < 1)
        {
            faderCanvasGroup.alpha += Time.deltaTime * velocidadFade;
            yield return null;
        }

        SceneManager.LoadScene(escena); // Cargamos la escena pasada por parametro
    }

    IEnumerator ProcesoAutomatico()
    {
        yield return new WaitForSeconds((1f / velocidadFade) + 0.5f);
        yield return new WaitForSeconds(tiempoEspera);

        CambiarEscena(escenaDestino); // LLamamos al metodo para cambiar de escena pasandole el nombre de la escena a la que vamos a ir
    }
}
