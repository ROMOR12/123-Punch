using UnityEngine;
using TMPro;
using System;

public class ResultScreenUI : MonoBehaviour
{
    [Header("Textos de Estadísticas")]
    public GameObject ventanaResultados;
    public TMP_Text tituloTexto; 
    public TMP_Text hitsTexto;
    public TMP_Text tiempoTexto;
    public TMP_Text damageTexto;

    public void MostrarResultados(bool victoria, int golpes, float tiempoJugado, int damageTotal)
    {
        //Activar el panel
        ventanaResultados.SetActive(true);

        //Poner título y color
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

        hitsTexto.text = golpes.ToString();
        damageTexto.text = damageTotal.ToString();

        TimeSpan t = TimeSpan.FromSeconds(tiempoJugado);
        tiempoTexto.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
