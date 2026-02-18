using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MiniGameMana : MonoBehaviour
{
    [Header("Configuración")]
    public int clicksObjetivo = 50;
    public float tiempoLimite = 10f;

    [Header("Referencia UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI clickText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultTitle;

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

    // Lo primero que se ejecuta cuando se inicia el script
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

    // Por cada click suma uno al contador y llama a los metodos para actualizar la interfaz
    public void RegistrarClick()
    {
        if (!juegoActivo) return;
        
        clicksActuales++;
        clicksSopa++;
        ActualizarInterfaz();
        ActualizarSopa();
        StopAllCoroutines();
        StartCoroutine(EfectoSaltarSopa());
        particulasSopa.Play();

        // Condición para finalizar el juego
        if (clicksActuales >= clicksObjetivo)
        {
            FinalizarJuego(true);
        }
    }

    // Efecto de salto para el sprite de la sopa
    IEnumerator EfectoSaltarSopa()
    {
        RectTransform rt = imagenSopa.rectTransform;

        rt.anchoredPosition = posicionOriginalSopa + new Vector3(0, fuerzaSalto);
        yield return new WaitForSeconds(duracionSalto);

        rt.anchoredPosition = posicionOriginalSopa;
    }

    // Actualiza la interfaz, los diferentes estados de la sopa
    public void ActualizarSopa()
    {
        if (estadosSopa.Length == 0) return;

        int indice = clicksSopa % estadosSopa.Length;
        imagenSopa.sprite = estadosSopa[indice];
    }

    // Actualiza la interfaz en tiempo real, tiempo restante y contador de clicks
    public void ActualizarInterfaz()
    {
        clickText.text = "Clics: " + clicksActuales + " / " + clicksObjetivo;
        timerText.text = "Tiempo: " + tiempoRestante.ToString("F1") + "s";
    }

    // Finaliza el juego y muestra un mensaje en funcion de si has ganado o perdido
    public async void FinalizarJuego(bool ganado)
    {
        juegoActivo = false;
        if (ganado)
        {
            Debug.Log("Ganaste!");
            timerText.text = "¡CONSEGUIDO!";
            resultTitle.text = "VICTORIA!";
            await Task.Delay(1000);
            resultPanel.SetActive(true);
            StopAllCoroutines();
        }
        else
        {
            Debug.Log("Se acabó el tiempo...");
            timerText.text = "¡Tiempo Agotado!";
            resultTitle.text = "Perdiste :(";
            await Task.Delay(1000);
            resultPanel.SetActive(true);
            StopAllCoroutines();
        }
    }

    public void btn_Salir()
    {
        CargaEscena.Cargar("Menu");
    }
}
