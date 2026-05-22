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

    [Header("UI del BotÃ³n Seleccionar")]
    [SerializeField] private Button botonSeleccionar;
    [SerializeField] private TextMeshProUGUI textoBotonSeleccionar;
    [SerializeField] private Color colorSeleccionado = new Color(0.5f, 0.5f, 0.5f);
    private Color colorOriginalBoton;

    private int indicePersonajeSeleccionado = 0;
    private Dictionary<string, Personaje> cachePersonajes = new Dictionary<string, Personaje>();
    private Dictionary<string, bool> cacheEstadoDesbloqueo = new Dictionary<string, bool>();

    private bool personajeActualDesbloqueado = false;

    void Start()
    {
        gameManager = GameManager.Instance;
        if (botonSeleccionar != null) colorOriginalBoton = botonSeleccionar.image.color;

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

        imagenSeleccionPersonaje.sprite = personajeLocal.sprite != null ? personajeLocal.sprite : gameManager.imageDefault;
        imagenPersonajeSeleccionado.sprite = personajeLocal.sprite != null ? personajeLocal.sprite : gameManager.imageDefault;

        string idPersonaje = personajeLocal.id;

        ConfigurarBotonComoBloqueado(personajeLocal);
        
        Personaje datosAMostrar = null;
        bool estaDesbloqueado = false;

        if (cachePersonajes.ContainsKey(idPersonaje) && cacheEstadoDesbloqueo.ContainsKey(idPersonaje))
        {
            datosAMostrar = cachePersonajes[idPersonaje];
            estaDesbloqueado = cacheEstadoDesbloqueo[idPersonaje];
        }
        else
        {
            SetTextosCargando();
            
            UsuarioService uService = new UsuarioService();
            PersonajeService pjServiceGlobal = new PersonajeService();
            Personaje datosUsuario = null;

            if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
            {
                string idUsuario = SessionManager.shared.currentUser.id;
                datosUsuario = await uService.ObtenerPersonajeDeUsuario(idUsuario, idPersonaje);
            }

            if (this == null) return;

            if (datosUsuario != null)
            {
                datosAMostrar = datosUsuario;
                estaDesbloqueado = true;
            }
            else
            {
                Personaje datosBase = await pjServiceGlobal.ObtenerPersonaje(idPersonaje);
                if (this == null) return;
                datosAMostrar = datosBase;
                estaDesbloqueado = false;
            }

            if (datosAMostrar != null)
            {
                cachePersonajes[idPersonaje] = datosAMostrar;
                cacheEstadoDesbloqueo[idPersonaje] = estaDesbloqueado;
            }
        }

        if (this == null || imagenSeleccionPersonaje == null || imagenPersonajeSeleccionado == null) return;

        if (datosAMostrar != null && gameManager.listaPersonajes[indicePersonajeSeleccionado].id == idPersonaje)
        {
            MostrarDatosEnPantalla(datosAMostrar);

            if (idPersonaje.Equals("personaje_james", System.StringComparison.OrdinalIgnoreCase))
            {
                estaDesbloqueado = true;
            }

            personajeActualDesbloqueado = estaDesbloqueado;

            if (estaDesbloqueado)
            {
                imagenSeleccionPersonaje.color = Color.white;
                imagenPersonajeSeleccionado.color = Color.white;
            }
            else
            {
                imagenSeleccionPersonaje.color = new Color(0.0f, 0.0f, 0.0f, 1f); 
                imagenPersonajeSeleccionado.color = new Color(0.0f, 0.0f, 0.0f, 1f);
            }

            if (!estaDesbloqueado)
            {
                ConfigurarBotonComoBloqueado(personajeLocal);
            }
            else if (gameManager.idPersonajeSeleccionado == idPersonaje)
            {
                ConfigurarBotonComoSeleccionado(true);
            }
            else
            {
                ConfigurarBotonComoSeleccionado(false);
            }
        }
    }

    public async void SeleccionarPersonajeBoton()
    {
        var personajeLocal = gameManager.listaPersonajes[indicePersonajeSeleccionado];
        string idElegido = personajeLocal.id;

        if (!personajeActualDesbloqueado)
        {
            if (personajeLocal.unlockCondition == "coins")
            {
                if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
                {
                    int monedas = SessionManager.shared.currentUser.free_coin;
                    if (monedas >= personajeLocal.price)
                    {
                        SessionManager.shared.currentUser.free_coin -= personajeLocal.price;
                        
                        UsuarioService uService = new UsuarioService();
                        await uService.ActualizarUsuario(SessionManager.shared.currentUser);
                        if (this == null) return;

                        Personaje datosBase = cachePersonajes[idElegido];
                        await uService.ActualizarPersonaje(SessionManager.shared.currentUser.id, datosBase);
                        if (this == null) return;

                        cacheEstadoDesbloqueo[idElegido] = true;
                        personajeActualDesbloqueado = true;

                        gameManager.idPersonajeSeleccionado = idElegido;
                        ConfigurarBotonComoSeleccionado(true);
                        if (imagenSeleccionPersonaje != null) imagenSeleccionPersonaje.color = Color.white;
                        if (imagenPersonajeSeleccionado != null) imagenPersonajeSeleccionado.color = Color.white;

                        GameEvents.TriggerCharacterUnlocked();
                        SoundManager.PlaySound(SoundType.Consumable);
                        Debug.Log($"Personaje {idElegido} comprado por {personajeLocal.price} monedas.");
                    }
                    else
                    {
                        Debug.Log("No tienes suficientes monedas.");
                    }
                }
            }
            return;
        }

        gameManager.idPersonajeSeleccionado = idElegido;

        ConfigurarBotonComoSeleccionado(true);

        Debug.Log($"Personaje {idElegido} seleccionado y guardado.");
    }

    private void ConfigurarBotonComoBloqueado(BaseCharacter personaje)
    {
        if (botonSeleccionar == null || textoBotonSeleccionar == null) return;

        if (personaje.unlockCondition == "coins")
        {
            textoBotonSeleccionar.text = $"Comprar ({personaje.price})";
            botonSeleccionar.image.color = colorOriginalBoton;
            botonSeleccionar.interactable = true;
        }
        else
        {
            textoBotonSeleccionar.text = $"Bloqueado ({personaje.unlockCondition})";
            botonSeleccionar.image.color = colorSeleccionado;
            botonSeleccionar.interactable = false;
        }
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
