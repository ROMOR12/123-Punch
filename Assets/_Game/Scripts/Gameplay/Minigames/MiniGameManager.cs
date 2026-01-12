using TMPro;
using UnityEngine;

public class MiniGameMana : MonoBehaviour
{
    [Header("Configuración")]
    public int clicksObjetivo = 50;
    public float tiempoLimite = 10f;

    [Header("Referencia UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI clickText;

    private int clicksActuales = 0;
    private float tiempoRestante;
    private bool juegoActivo = true;

    private void Start()
    {
        tiempoRestante = tiempoLimite;
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
        ActualizarInterfaz();

        if(clicksActuales >= clicksObjetivo)
        {
            FinalizarJuego(true);
        }
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
