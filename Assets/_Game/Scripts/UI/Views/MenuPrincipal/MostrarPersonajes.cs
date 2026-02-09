using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MostrarPersonajes : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField]
    private Image imagenPersonajeSeleccionado;

    [SerializeField]
    private Image imagenSeleccionPersonaje;

    [SerializeField]
    private TextMeshProUGUI textoVida;

    [SerializeField]
    private TextMeshProUGUI textoStam;

    [SerializeField]
    private TextMeshProUGUI textoDanyo;

    [SerializeField]
    private TextMeshProUGUI textoRecuperacion;

    [SerializeField]
    private TextMeshProUGUI textoNombrePersonaje;

    private int indicePersonajeSeleccionado = 0;

    void Start()
    {
        gameManager = GameManager.Instance;

        ActualizarPersonaje();
    }

    private void ActualizarPersonaje()
    {
        // imagenPersonajeSeleccionado.sprite = gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite !=null? gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite : gameManager.imageDefault;
        // imagenSeleccionPersonaje.sprite = gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite != null ? gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite : gameManager.imageDefault;

        imagenSeleccionPersonaje.sprite = gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite !=null? gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite : gameManager.imageDefault;
        imagenPersonajeSeleccionado.sprite = gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite != null ? gameManager.listaPersonajes[indicePersonajeSeleccionado].sprite : gameManager.imageDefault;


        textoVida.text = gameManager.listaPersonajes[indicePersonajeSeleccionado].life.ToString();
        textoStam.text = gameManager.listaPersonajes[indicePersonajeSeleccionado].energy.ToString();
        textoDanyo.text = gameManager.listaPersonajes[indicePersonajeSeleccionado].force.ToString();
        textoRecuperacion.text = gameManager.listaPersonajes[indicePersonajeSeleccionado].recovery.ToString();
        textoNombrePersonaje.text = gameManager.listaPersonajes[indicePersonajeSeleccionado].entityName;
    }

    public void CambiarPersonajeBotonIzquierdo()
    {
        if (indicePersonajeSeleccionado == 0)
            indicePersonajeSeleccionado = gameManager.listaPersonajes.Count - 1;
        else
            indicePersonajeSeleccionado--;
        ActualizarPersonaje();
    }
    public void CambiarPersonajeBotonDerecho()
    {
        if (indicePersonajeSeleccionado == gameManager.listaPersonajes.Count - 1)
            indicePersonajeSeleccionado = 0;
        else
            indicePersonajeSeleccionado++;
        ActualizarPersonaje();
    }
}
