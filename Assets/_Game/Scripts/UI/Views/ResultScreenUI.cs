using UnityEngine;
using TMPro;
using System;

// Controlador de la interfaz gráfica al finalizar el combate.
public class ResultScreenUI : MonoBehaviour
{
    [Header("Textos de Estadísticas")]
    public GameObject ventanaResultados; // El panel principal que se muestra
    public TMP_Text tituloTexto;         // Muestra "VICTORIA" o "DERROTA"
    public TMP_Text hitsTexto;           // Contador de golpes en pantalla
    public TMP_Text tiempoTexto;         // Duración del combate
    public TMP_Text damageTexto;         // Daño total infligido

    // Recibe los datos finales del RoundManager
    public void MostrarResultados(bool victoria, int golpes, float tiempoJugado, int damageTotal)
    {
        // 1. Mostrar el panel
        ventanaResultados.SetActive(true);

        // 2. Configurar el título y su color según el resultado
        if (victoria)
        {
            tituloTexto.text = "¡VICTORIA!";
            tituloTexto.color = Color.green;
        }
        else
        {
            tituloTexto.text = "DERROTA :(";
            tituloTexto.color = Color.red;
        }

        // 3. Asignar las estadísticas numéricas transformándolas a texto
        hitsTexto.text = golpes.ToString();
        damageTexto.text = damageTotal.ToString();

        // 4. Formatear los segundos a (Minutos:Segundos)
        TimeSpan t = TimeSpan.FromSeconds(tiempoJugado);
        tiempoTexto.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
