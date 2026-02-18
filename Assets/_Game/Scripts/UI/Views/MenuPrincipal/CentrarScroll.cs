using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Añadimos las interfaces IBeginDragHandler y IEndDragHandler
public class MenuDeslizanteConIman : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect miScroll;

    // Las 3 posiciones exactas de tus paneles: Izquierda (0), Centro (0.5), Derecha (1)
    private float[] posicionesPaneles = new float[] { 0f, 0.5f, 1f };

    private float posicionDestino;
    private bool estaArrastrando = false;

    [Header("Configuración del Imán")]
    public float velocidadIman = 10f; // Ajusta esto en el Inspector para que sea más rápido o lento

    void Start()
    {
        miScroll = GetComponent<ScrollRect>();

        if (miScroll != null)
        {
            Canvas.ForceUpdateCanvases();

            // 1. Forzamos a que empiece en el panel central (0.5)
            miScroll.horizontalNormalizedPosition = 0.5f;
            posicionDestino = 0.5f;
        }
    }

    void Update()
    {
        // 2. Si el jugador NO está tocando la pantalla, el imán mueve el panel suavemente
        if (!estaArrastrando && miScroll != null)
        {
            miScroll.horizontalNormalizedPosition = Mathf.Lerp(
                miScroll.horizontalNormalizedPosition,
                posicionDestino,
                Time.deltaTime * velocidadIman
            );
        }
    }

    // 3. Se dispara automáticamente cuando el jugador empieza a deslizar
    public void OnBeginDrag(PointerEventData eventData)
    {
        estaArrastrando = true;
    }

    // 4. Se dispara automáticamente cuando el jugador suelta la pantalla
    public void OnEndDrag(PointerEventData eventData)
    {
        estaArrastrando = false;
        EncontrarPanelMasCercano();
    }

    // 5. Función matemática sencilla para ver qué panel está más cerca
    private void EncontrarPanelMasCercano()
    {
        float posicionActual = miScroll.horizontalNormalizedPosition;
        float distanciaMinima = float.MaxValue;

        for (int i = 0; i < posicionesPaneles.Length; i++)
        {
            // Medimos la distancia entre donde soltaste y cada panel
            float distancia = Mathf.Abs(posicionActual - posicionesPaneles[i]);

            // Nos quedamos con el más cercano
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                posicionDestino = posicionesPaneles[i];
            }
        }
    }
}