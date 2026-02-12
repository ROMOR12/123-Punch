using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para detectar el toque

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnapFinal : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    public float velocidadSuavizado = 10f;

    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private Vector2 posicionObjetivo;
    private bool necesitoMoverme = false;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentPanel = scrollRect.content;
    }

    void Update()
    {
        // Solo nos movemos si hemos soltado el dedo y calculado el destino
        if (necesitoMoverme)
        {
            // Interpolación suave hacia el objetivo
            contentPanel.anchoredPosition = Vector2.Lerp(
                contentPanel.anchoredPosition,
                posicionObjetivo,
                velocidadSuavizado * Time.deltaTime
            );

            // Si ya estamos muy cerca (menos de 1 pixel), paramos para ahorrar CPU
            if (Vector2.Distance(contentPanel.anchoredPosition, posicionObjetivo) < 1f)
            {
                contentPanel.anchoredPosition = posicionObjetivo;
                necesitoMoverme = false;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Si tocamos la pantalla, cancelamos el movimiento automático
        necesitoMoverme = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. MATAMOS LA INERCIA DE UNITY (Esta es la clave para que no vibre)
        scrollRect.velocity = Vector2.zero;

        // 2. Calculamos dónde ir
        EncontrarPanelMasCercano();

        // 3. Activamos el movimiento
        necesitoMoverme = true;
    }

    void EncontrarPanelMasCercano()
    {
        float distanciaMinima = float.MaxValue;
        int indiceGanador = 0;

        // Buscamos cuál de los hijos está visualmente más cerca del centro del ScrollView
        for (int i = 0; i < contentPanel.childCount; i++)
        {
            RectTransform hijo = contentPanel.GetChild(i) as RectTransform;

            // Calculamos la distancia en el mundo real (World Space) para no liarnos con pivotes
            float distancia = Mathf.Abs(transform.position.x - hijo.position.x);

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                indiceGanador = i;
            }
        }

        // --- MATEMÁTICAS PARA CENTRAR ---
        // Queremos mover el Content para que el Hijo Ganador acabe en el centro del ScrollView.
        // La fórmula es: NuevaPosContent = PosContentActual + (CentroScrollView - CentroHijo)

        RectTransform ganador = contentPanel.GetChild(indiceGanador) as RectTransform;

        float diferenciaX = transform.position.x - ganador.position.x;

        posicionObjetivo = new Vector2(
            contentPanel.anchoredPosition.x + diferenciaX,
            contentPanel.anchoredPosition.y
        );
    }
}