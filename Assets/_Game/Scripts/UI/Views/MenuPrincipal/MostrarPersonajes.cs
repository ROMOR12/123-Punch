using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MostrarPersonajes : MonoBehaviour
{
    private GameManager gameManager;

    [Header("Referencias de Personaje")]
    [SerializeField] private Image imagenPersonajeSeleccionado;
    [SerializeField] private Image imagenSeleccionPersonaje;
    [SerializeField] private TextMeshProUGUI textoVida, textoStam, textoDanyo, textoRecuperacion, textoNombrePersonaje;

    [Header("UI del Botón Seleccionar")]
    [SerializeField] private Button botonSeleccionar;
    [SerializeField] private TextMeshProUGUI textoBotonSeleccionar;
    [SerializeField] private Color colorSeleccionado = new Color(0.5f, 0.5f, 0.5f); // Color más oscuro
    private Color colorOriginalBoton;

    private int indicePersonajeSeleccionado = 0;
    private Dictionary<string, Personaje> cachePersonajes = new Dictionary<string, Personaje>();

    void Start()
    {
        gameManager = GameManager.Instance;
        if (botonSeleccionar != null) colorOriginalBoton = botonSeleccionar.image.color;

        // Aquí el GameManager ya habrá leído de PlayerPrefs automáticamente
        string idGuardado = gameManager.idPersonajeSeleccionado;

        int indice = gameManager.listaPersonajes.FindIndex(p => p.id == idGuardado);
        if (indice != -1)
        {
            indicePersonajeSeleccionado = indice;
        }

        ActualizarPersonaje();
    }

    private async void ActualizarPersonaje()
    {
        var personajeLocal = gameManager.listaPersonajes[indicePersonajeSeleccionado];

        // Actualizar imágenes
        imagenSeleccionPersonaje.sprite = personajeLocal.sprite != null ? personajeLocal.sprite : gameManager.imageDefault;
        imagenPersonajeSeleccionado.sprite = personajeLocal.sprite != null ? personajeLocal.sprite : gameManager.imageDefault;

        string idPersonaje = personajeLocal.id;

        // --- LÓGICA DEL BOTÓN ---
        // Comprobamos si este personaje es el que ya está guardado en el GameManager
        if (gameManager.idPersonajeSeleccionado == idPersonaje)
        {
            ConfigurarBotonComoSeleccionado(true);
        }
        else
        {
            ConfigurarBotonComoSeleccionado(false);
        }

        // Carga de stats (Caché + Firebase)
        if (cachePersonajes.ContainsKey(idPersonaje))
        {
            MostrarDatosEnPantalla(cachePersonajes[idPersonaje]);
        }
        else
        {
            SetTextosCargando();
            PersonajeService pjService = new PersonajeService();
            Personaje datosNube = await pjService.ObtenerPersonaje(idPersonaje);

            if (datosNube != null)
            {
                cachePersonajes[idPersonaje] = datosNube;
                if (gameManager.listaPersonajes[indicePersonajeSeleccionado].id == idPersonaje)
                {
                    MostrarDatosEnPantalla(datosNube);
                }
            }
        }
    }

    public void SeleccionarPersonajeBoton()
    {
        string idElegido = gameManager.listaPersonajes[indicePersonajeSeleccionado].id;
        gameManager.idPersonajeSeleccionado = idElegido;

        // Cambiamos el aspecto visual del botón inmediatamente
        ConfigurarBotonComoSeleccionado(true);

        Debug.Log($"Personaje {idElegido} seleccionado y guardado.");
    }

    private void ConfigurarBotonComoSeleccionado(bool esSeleccionado)
    {
        if (botonSeleccionar == null || textoBotonSeleccionar == null) return;

        if (esSeleccionado)
        {
            textoBotonSeleccionar.text = "Seleccionado";
            botonSeleccionar.image.color = colorSeleccionado;
            botonSeleccionar.interactable = false;
        }
        else
        {
            textoBotonSeleccionar.text = "Seleccionar";
            botonSeleccionar.image.color = colorOriginalBoton;
            botonSeleccionar.interactable = true;
        }
    }

    private void MostrarDatosEnPantalla(Personaje datos)
    {
        textoVida.text = datos.life.ToString();
        textoStam.text = datos.energy.ToString();
        textoDanyo.text = datos.force.ToString();
        textoRecuperacion.text = datos.recovery.ToString();
        textoNombrePersonaje.text = datos.name;
    }

    private void SetTextosCargando()
    {
        textoVida.text = "..."; textoStam.text = "..."; textoDanyo.text = "...";
        textoRecuperacion.text = "..."; textoNombrePersonaje.text = "Cargando...";
    }

    public void CambiarPersonajeBotonIzquierdo()
    {
        indicePersonajeSeleccionado = (indicePersonajeSeleccionado == 0) ? gameManager.listaPersonajes.Count - 1 : indicePersonajeSeleccionado - 1;
        ActualizarPersonaje();
    }

    public void CambiarPersonajeBotonDerecho()
    {
        indicePersonajeSeleccionado = (indicePersonajeSeleccionado == gameManager.listaPersonajes.Count - 1) ? 0 : indicePersonajeSeleccionado + 1;
        ActualizarPersonaje();
    }
}