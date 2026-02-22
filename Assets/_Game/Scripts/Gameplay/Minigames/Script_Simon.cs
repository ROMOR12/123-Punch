using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/*
 * public class Script_Simon : MonoBehaviour
{
    [Header("UI y Objetos")]
    public Button[] botonesObjetos;
    public GameObject acierto;
    public GameObject error;
    public TextMeshProUGUI Tiempo;
    public TextMeshProUGUI textoPuntos; // <--- NUEVO: Arrastra aquí un texto para los puntos

    [Header("Configuración")]
    public float tiempoInicial = 30f;
    private float tiempoRestante;
    private int puntosActuales = 0; // <--- NUEVO: Contador de puntos

    private List<int> secuenciaJuego = new List<int>();
    private int pasoActual = 0;
    private bool puedePresionar = false;
    private bool juegoActivo = false;

    void Start()
    {
        if (acierto) acierto.SetActive(false);
        if (error) error.SetActive(false);
        tiempoRestante = tiempoInicial;
        IniciarJuegoDesdeCero();
    }

    public void IniciarJuegoDesdeCero()
    {
        secuenciaJuego.Clear();
        pasoActual = 0;
        puntosActuales = 0; // Reiniciamos puntos al fallar o empezar
        ActualizarTextoPuntos();

        //No se si queremos que se reinicie el tiempo
        //tiempoRestante = tiempoInicial;
        juegoActivo = true;
        StartCoroutine(SiguienteRonda());
    }

    void Update()
    {
        if (juegoActivo && tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            Tiempo.text = string.Format("{0:00}:{1:00}", 0, Mathf.Max(0, tiempoRestante));

            if (tiempoRestante <= 0) FinalizarPorTiempo();
        }
    }

    IEnumerator SiguienteRonda()
    {
        puedePresionar = false;
        pasoActual = 0;

        // Añadimos un paso más a la secuencia infinita
        secuenciaJuego.Add(Random.Range(0, botonesObjetos.Length));

        yield return new WaitForSeconds(0.8f);

        foreach (int indice in secuenciaJuego)
        {
            yield return StartCoroutine(AnimarYBrillar(indice));
            yield return new WaitForSeconds(0.2f); // Un poco más rápido para dar ritmo
        }

        puedePresionar = true;
    }

    public void BotonPresionado(int id)
    {
        if (!puedePresionar || !juegoActivo) return;

        if (id == secuenciaJuego[pasoActual])
        {
            // ACIERTO INDIVIDUAL
            StartCoroutine(AnimarYBrillar(id));
            pasoActual++;

            // SI COMPLETA LA SECUENCIA ENTERA
            if (pasoActual >= secuenciaJuego.Count)
            {
                puntosActuales += 10; // Sumamos 10 puntos por ronda completada
                ActualizarTextoPuntos();
                StartCoroutine(MostrarFeedback(true)); // Círculo verde al completar ronda
                StartCoroutine(SiguienteRonda());
            }
        }
        else
        {
            // ERROR: REINICIO TOTAL
            StartCoroutine(ManejarError());
        }
    }

    void ActualizarTextoPuntos()
    {
        if (textoPuntos) textoPuntos.text = "PUNTOS: " + puntosActuales;
    }

    IEnumerator ManejarError()
    {
        puedePresionar = false;
        juegoActivo = false; // Paramos el tiempo al fallar
        StartCoroutine(MostrarFeedback(false)); // X Roja

        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate(); 
        #endif

        yield return new WaitForSeconds(1.5f);
        IniciarJuegoDesdeCero();
    }

    void FinalizarPorTiempo()
    {
        juegoActivo = false;
        // Aquí podrías mostrar un panel de "Puntuación Final"
        tiempoRestante = 0;
        Tiempo.text = "00:00";
        StopAllCoroutines();
        // Aquí el juego se detiene porque el tiempo llegó a cero
        Debug.Log("Fin de la partida. Puntos totales: " + puntosActuales);
    }

    IEnumerator MostrarFeedback(bool esAcierto)
    {
        GameObject fb = esAcierto ? acierto : error;
        if (fb)
        {
            fb.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            fb.SetActive(false);
        }
    }

    IEnumerator AnimarYBrillar(int indice)
    {
        Transform t = botonesObjetos[indice].transform;
        t.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        yield return new WaitForSeconds(0.15f);
        t.localScale = Vector3.one;
    }
}
*/
public class Script_Simon : MonoBehaviour
{
    [Header("UI y Objetos")]
    public Button[] botonesObjetos;
    public GameObject acierto;
    public GameObject error;
    public TextMeshProUGUI textoEstado;
    public GameObject resultScreen;
    public TextMeshProUGUI title;

    [Header("Configuración del Desafío")]
    public int metaObjetos = 6;

    private List<int> secuenciaJuego = new List<int>();
    private int pasoActual = 0;
    private bool puedePresionar = false;
    private bool juegoActivo = false;
    

    void Start()
    {
        if (acierto) acierto.SetActive(false);
        if (error) error.SetActive(false);

        StartCoroutine(IniciarNuevoDesafio());
    }

    IEnumerator IniciarNuevoDesafio()
    {
        secuenciaJuego.Clear();
        pasoActual = 0;

        if (textoEstado) textoEstado.text = "Presta atención";

        yield return new WaitForSeconds(2.0f);

        if (textoEstado) textoEstado.text = "Pulsa en el orden que brillen!";
        yield return new WaitForSeconds(2.0f);

        if (textoEstado) textoEstado.text = "3";
        yield return new WaitForSeconds(1.0f);

        if (textoEstado) textoEstado.text = "2";
        yield return new WaitForSeconds(1.0f);

        if (textoEstado) textoEstado.text = "1";
        yield return new WaitForSeconds(1.0f);

        if (textoEstado) textoEstado.text = "YA!";

        juegoActivo = true;

        // Empezamos la ronda
        StartCoroutine(SiguienteRonda());
    }

    IEnumerator SiguienteRonda()
    {
        puedePresionar = false;
        pasoActual = 0;

        secuenciaJuego.Add(Random.Range(0, botonesObjetos.Length));

        yield return new WaitForSeconds(0.8f);

        foreach (int indice in secuenciaJuego)
        {
            yield return StartCoroutine(AnimarYBrillar(indice));
            yield return new WaitForSeconds(0.2f);
        }

        if (textoEstado) textoEstado.text = "RONDA ACTUAL (" + secuenciaJuego.Count + "/" + metaObjetos + ")";
        puedePresionar = true;
    }

    public void BotonPresionado(int id)
    {
        if (!puedePresionar || !juegoActivo) return;

        if (id == secuenciaJuego[pasoActual])
        {
            StartCoroutine(AnimarYBrillar(id));
            pasoActual++;

            if (pasoActual >= secuenciaJuego.Count)
            {
                if (secuenciaJuego.Count >= metaObjetos)
                {
                    GanarPartida();
                }
                else
                {
                    StartCoroutine(MostrarFeedback(true));
                    StartCoroutine(SiguienteRonda());
                }
            }
        }
        else
        {
            PerderPartida();
        }
    }

    void GanarPartida()
    {
        juegoActivo = false;
        puedePresionar = false;
        if (textoEstado) textoEstado.text = "¡DESAFÍO COMPLETADO!";
        StartCoroutine(MostrarFeedback(true));
        Debug.Log("El jugador ha ganado el minijuego.");

        title.text = "HAS GANADO!!";
        resultScreen.SetActive(true);
    }

    void PerderPartida()
    {
        juegoActivo = false;
        puedePresionar = false;
        StopAllCoroutines();

        if (textoEstado) textoEstado.text = "¡ERROR! INTÉNTALO DE NUEVO";
        StartCoroutine(MostrarFeedback(false));

        #if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
        #endif

        title.text = "HAS PERDIDO :(";
        resultScreen.SetActive(true);
    }

    IEnumerator MostrarFeedback(bool esAcierto)
    {
        GameObject fb = esAcierto ? acierto : error;
        if (fb)
        {
            fb.SetActive(true);
            yield return new WaitForSeconds(0.6f);
            fb.SetActive(false);
        }
    }

    IEnumerator AnimarYBrillar(int indice)
    {
        Transform t = botonesObjetos[indice].transform;
        t.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        yield return new WaitForSeconds(0.55f);
        t.localScale = Vector3.one;
    }

    public void btn_salir()
    {
        CargaEscena.Cargar("Menu");
    }
}