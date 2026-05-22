using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuDeslizanteConIman : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect miScroll;

    private float[] posicionesPaneles = new float[] { 0f, 0.5f, 1f };

    private float posicionDestino;
    private bool estaArrastrando = false;

    [Header("Configuración del Imán")]
    public float velocidadIman = 10f;

    void Start()
    {
        miScroll = GetComponent<ScrollRect>();

        if (miScroll != null)
        {
            Canvas.ForceUpdateCanvases();

            miScroll.horizontalNormalizedPosition = 0.5f;
            posicionDestino = 0.5f;
        }
    }

    void Update()
    {
        if (!estaArrastrando && miScroll != null)
        {
            miScroll.horizontalNormalizedPosition = Mathf.Lerp(
                miScroll.horizontalNormalizedPosition,
                posicionDestino,
                Time.deltaTime * velocidadIman
            );
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        estaArrastrando = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        estaArrastrando = false;
        EncontrarPanelMasCercano();
    }

    private void EncontrarPanelMasCercano()
    {
        float posicionActual = miScroll.horizontalNormalizedPosition;
        float distanciaMinima = float.MaxValue;

        for (int i = 0; i < posicionesPaneles.Length; i++)
        {
            float distancia = Mathf.Abs(posicionActual - posicionesPaneles[i]);

            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                posicionDestino = posicionesPaneles[i];
            }
        }
    }
}
