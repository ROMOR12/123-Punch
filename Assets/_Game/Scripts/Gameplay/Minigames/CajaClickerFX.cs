using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;

public class CajaClickerFx : MonoBehaviour
{
    [Header("--- Configuración del Giro ---")]
    [Tooltip("Velocidad normal cuando no tocas")]
    public float velocidadBase = 1f;
    [Tooltip("Velocidad máxima a la que puede llegar")]
    public float velocidadMaxima = 30f;
    [Tooltip("Cuánto acelera con cada click")]
    public float aceleracionPorClick = 2f;
    [Tooltip("Qué tan rápido frena si dejas de tocar")]
    public float frenadoPorSegundo = 5f;

    [Header("--- Configuración del POP ---")]
    [Tooltip("Cuánto crece la caja al tocar")]
    public float escalaPop = 1.2f;
    [Tooltip("Cuánto dura el efecto de estar grande")]
    public float duracionPop = 0.05f;

    [Header("--- Explosión ---")]
    public GameObject panelRecompensa;
    public GameObject efectoParticulas;
    [Tooltip("Cuánto se hincha justo antes de explotar")]
    public float escalaHinchadoFinal = 2.5f;
    [Tooltip("Tiempo que se queda hinchada antes de reventar")]
    public float tiempoHinchado = 0.2f;
    [Tooltip("Tiempo que tarda en salir el panel después de la explosión")]
    public float retrasoPanelFinal = 0.5f;
    [Tooltip("Cuánto tarda el panel en subir desde abajo")]
    public float duracionAnimacionPanel = 0.5f;

    private float velocidadActual;
    private Animator miAnimator;
    private RectTransform miRectTransform;
    private Vector3 escalaOriginal;
    private Coroutine rutinaPopActual;
    private bool cajaAbierta = false;
    private Image miImagen;

    private RectTransform rectPanelRecompensa;
    private Vector2 posicionOriginalPanel;

    void Start()
    {
        // Componentes de la caja
        miAnimator = GetComponent<Animator>();
        miRectTransform = GetComponent<RectTransform>();

        // Recogemos el componente de la imagen
        miImagen = GetComponent<Image>();

        escalaOriginal = miRectTransform.localScale;
        velocidadActual = velocidadBase;

        // Aseguramos que el panel y las paticulas de cuando se habra la caja esten desactivos al principio
        if (panelRecompensa != null)
        {
            rectPanelRecompensa = panelRecompensa.GetComponent<RectTransform>();

            // 1. Guardamos donde has colocado el panel en el editor (el centro de la pantalla)
            posicionOriginalPanel = rectPanelRecompensa.anchoredPosition;

            // 2. Lo desactivamos
            panelRecompensa.SetActive(false);
        }
        if (efectoParticulas != null) efectoParticulas.SetActive(false);
    }

    void Update()
    {
        // Si la caja ya a sido abierta no hacemos nada
        if (cajaAbierta) return;

        // Detectamos si se a pulsado
        if (SeHaPulsado())
        {
            ProcesarTap();
        }

        // Gestionamos el frenado de la caja
        if (velocidadActual > velocidadBase)
        {
            // Reducimos la velocidad multiplicado por delta para los FPS
            velocidadActual -= frenadoPorSegundo * Time.deltaTime;
        }

        // Si la velocidad de la caja llega a su maximo la abrimos
        if (velocidadActual >= velocidadMaxima)
        {
            // CAMBIO: Iniciamos la secuencia de animación en vez de abrir de golpe
            StartCoroutine(SecuenciaExplosion());
        }
        else
        {
            // Calculamos que la velocidad actual no pueda ser menor que la base pero tampoco puede ser mayor que la maxima
            velocidadActual = Mathf.Clamp(velocidadActual, velocidadBase, velocidadMaxima);
        }

        // Decimos que la velocidad de la animacion sea la velocidad actual de la caja
        if (miAnimator != null) miAnimator.speed = velocidadActual;
    }

    // Metodo para comprobar el pulsado
    bool SeHaPulsado()
    {
        // Si pulsamos con el raton
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        // Si pulsamos en una pantalla tactil
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        // No pulsamos
        return false;
    }

    void ProcesarTap()
    {
        // Aumentamos la velocidad
        velocidadActual += aceleracionPorClick;

        // reiniciamos la corutina del pop para que si el usuario pulsa muy rapido, la caja no se haga gigante
        if (rutinaPopActual != null) StopCoroutine(rutinaPopActual);
        rutinaPopActual = StartCoroutine(HacerPop());
    }

    // Efecto de explosion
    IEnumerator SecuenciaExplosion()
    {
        // Ponemos a true para saber que la caja esta abierta
        cajaAbierta = true;

        // Paramos el pop anterior y el animator para controlar la escala manualmente
        if (rutinaPopActual != null) StopCoroutine(rutinaPopActual);
        if (miAnimator != null) miAnimator.enabled = false;

        // Hacemos la caja mas grande para dar impresion de exlosion
        miRectTransform.localScale = escalaOriginal * escalaHinchadoFinal;

        // Esperamos ese instante de tensión
        yield return new WaitForSeconds(tiempoHinchado);

        // Ocultamos la caja visualmente para que las particulas aun sigan en escena
        if (miImagen != null) miImagen.enabled = false;

        // Activamos las particulas
        if (efectoParticulas != null) efectoParticulas.SetActive(true);

        // Esperamos un poco para mostrar el panel de recompensas
        yield return new WaitForSeconds(retrasoPanelFinal);

        // Activamos el panel de la recompensa
        if (panelRecompensa != null)
        {
            yield return StartCoroutine(AnimarEntradaPanel());
        }
    }
    // Animación de entrada del panel de recompensas
    IEnumerator AnimarEntradaPanel()
    {
        panelRecompensa.SetActive(true);

        // Caulculamos la posicion de inicio restando restando la Y de la pantalla
        Vector2 posicionAbajo = posicionOriginalPanel;
        posicionAbajo.y -= Screen.height;

        // Ponemos el panel en la posicion inicial calculad, que es abajo
        rectPanelRecompensa.anchoredPosition = posicionAbajo;

        // Bucle de la animacion
        float tiempoPasado = 0f;
        while (tiempoPasado < duracionAnimacionPanel)
        {
            tiempoPasado += Time.deltaTime;
            // Calculamos el porcentaje completado
            float porcentaje = tiempoPasado / duracionAnimacionPanel;

            // SmoothStep hace que empiece y termine suave, en vez de lineal
            porcentaje = Mathf.SmoothStep(0f, 1f, porcentaje);

            // Movemos el panel
            rectPanelRecompensa.anchoredPosition = Vector2.Lerp(posicionAbajo, posicionOriginalPanel, porcentaje);

            // Esperamos al siguiente frame
            yield return null;
        }

        // Aseguramos que termine en su posicion original
        rectPanelRecompensa.anchoredPosition = posicionOriginalPanel;
    }

    // Animación de pop
    IEnumerator HacerPop()
    {
        // Escalamos la caja y la hacemos un pelin mas grande
        miRectTransform.localScale = escalaOriginal * escalaPop;

        // Metemos un pelin de couldown para que se veo por un moemnto mas grande
        yield return new WaitForSeconds(duracionPop);

        // Volvemos al tamaño original si la caja aun no a sido abierta
        if (!cajaAbierta)
        {
            miRectTransform.localScale = escalaOriginal;
        }
        rutinaPopActual = null;
    }
}