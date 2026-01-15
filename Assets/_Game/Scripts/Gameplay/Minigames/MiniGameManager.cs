using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MiniGameMana : MonoBehaviour
{
    [Header("Configuración")]
    public int clicksObjetivo = 50;
    public float tiempoLimite = 10f;

    [Header("Referencia UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI clickText;

    [Header("Efecto Sopa")]
    public Image imagenSopa;
    public Sprite[] estadosSopa;
    public float fuerzaSalto = 20f;
    public float duracionSalto = 0.1f;
    public ParticleSystem particulasSopa;

    private int clicksActuales = 0;
    private int clicksSopa = 0;
    private float tiempoRestante;
    private bool juegoActivo = true;
    private Vector3 posicionOriginalSopa;

    private void Start()
    {
        tiempoRestante = tiempoLimite;
        posicionOriginalSopa = imagenSopa.rectTransform.anchoredPosition;
        ActualizarSopa();
        ActualizarInterfaz();
    }

    private void Update()
    {
        if (juegoActivo) 
        {
            if(tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                ActualizarInterfaz();
            }
            else
            {
                FinalizarJuego(false);
            }
        }
    }

    public void RegistrarClick()
    {
        if (!juegoActivo) return;
        
        clicksActuales++;
        clicksSopa++;
        ActualizarInterfaz();
        ActualizarSopa();
        StopAllCoroutines();
        StartCoroutine(EfectoSaltarSopa());

        if(particulasSopa != null)
        {
            particulasSopa.Play();
        }

        if(clicksActuales >= clicksObjetivo)
        {
            FinalizarJuego(true);
        }
    }

    IEnumerator EfectoSaltarSopa()
    {
        RectTransform rt = imagenSopa.rectTransform;

        rt.anchoredPosition = posicionOriginalSopa + new Vector3(0, fuerzaSalto);
        yield return new WaitForSeconds(duracionSalto);

        rt.anchoredPosition = posicionOriginalSopa;
    }

    public void ActualizarSopa()
    {
        if (estadosSopa.Length == 0) return;

        int indice = clicksSopa % estadosSopa.Length;
        imagenSopa.sprite = estadosSopa[indice];
    }

    public void ActualizarInterfaz()
    {
        clickText.text = "Clics: " + clicksActuales + " / " + clicksObjetivo;
        timerText.text = "Tiempo: " + tiempoRestante.ToString("F1") + "s";
    }

    public void FinalizarJuego(bool ganado)
    {
        juegoActivo = false;
        if (ganado)
        {
            Debug.Log("Ganaste!");
            timerText.text = "¡CONSEGUIDO!";
        }
        else
        {
            Debug.Log("Se acabó el tiempo...");
            timerText.text = "¡Tiempo Agotado!";
        }
    }
}
