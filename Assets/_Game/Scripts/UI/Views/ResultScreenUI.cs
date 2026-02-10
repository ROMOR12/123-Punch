using UnityEngine;
using TMPro;
using System;

public class ResultScreenUI : MonoBehaviour
{
    [Header("Textos de Estadísticas")]
    public GameObject ventanaResultados;
    public TMP_Text tituloTexto;       //"VICTORIA" o "DERROTA"
    public TMP_Text hitsTexto;         // "Golpes"
    public TMP_Text tiempoTexto;       // "Tiempo"
    public TMP_Text damageTexto;       // "Daño Total"

    // Esta función la llamará el CombatController al terminar
    public void MostrarResultados(bool victoria, int golpes, float tiempoJugado, int damageTotal)
    {
        //Activar el panel
        ventanaResultados.SetActive(true);

        //Poner título y color
        if (victoria)
        {
            tituloTexto.text = "¡VICTORIA!";
            tituloTexto.color = Color.green;
            //SoundManager.PlayUiSound(UiSoundType.CLICK); //cambiar
        }
        else
        {
            tituloTexto.text = "DERROTA :(";
            tituloTexto.color = Color.red;
            //SoundManager.PlayUiSound(UiSoundType.CLICK); //cambiar
        }

        //Rellenar datos
        hitsTexto.text = golpes.ToString();
        damageTexto.text = damageTotal.ToString();

        //Formatea el tiempo (Minutos:Segundos)
        TimeSpan t = TimeSpan.FromSeconds(tiempoJugado);
        tiempoTexto.text = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
