using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Datos de la Sesión (Firebase)")]
    public Usuario usuarioActual;
    public Personaje personajeActual;

    public int numCajas = 0;

    public static GameManager Instance;

    public List<ModosDeJuego> modosDeJuego;

    public List<ItemBase> items;

    public Sprite imageDefault;

    public List<BaseCharacter> listaPersonajes;
    public List<EnemyBase> listaEnemigos;

    [Header("Inventario y Equipamiento (Firebase)")]
    public List<string> inventarioIDs = new List<string>();
    public string pasivoEquipadoID = "";
    public List<string> activosEquipadosIDs = new List<string> { "", "" };

    [Header("Bases de Datos Maestras (ScriptableObjects)")]
    [Tooltip("Arrastra aquí todos los ScriptableObjects de objetos Pasivos")]
    public List<Pasivo> todosLosPasivos;
    [Tooltip("Arrastra aquí todos los ScriptableObjects de objetos Consumibles")]
    public List<Consumible> todosLosActivos;

    [Header("Progreso Actual")]
    private string _idPersonajeSeleccionado;

    [Header("Progreso de Niveles (Firebase)")]
    public int nivelMaximoDesbloqueado = 1;

    public string idPersonajeSeleccionado
    {
        get
        {
            if (string.IsNullOrEmpty(_idPersonajeSeleccionado))
            {
                _idPersonajeSeleccionado = PlayerPrefs.GetString("PersonajeEquipado", "personaje_ava");
            }
            return _idPersonajeSeleccionado;
        }
        set
        {
            _idPersonajeSeleccionado = value;
            PlayerPrefs.SetString("PersonajeEquipado", value);
            PlayerPrefs.Save();
        }
    }

    
    private bool notificacionesInicializadas = false;
    private void Update()
    {
        if (!notificacionesInicializadas && SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            new NotificacionesService().Inicializar();
            notificacionesInicializadas = true;
        }
    }

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameManager.Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Pasivo GetPasivoPorID(string id) => todosLosPasivos.Find(p => p.id == id);
    public Consumible GetActivoPorID(string id) => todosLosActivos.Find(a => a.id == id);

    public async void DesbloquearSiguienteNivel(int nivelActual)
    {
        if (nivelActual == nivelMaximoDesbloqueado)
        {
            nivelMaximoDesbloqueado++;

            if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
            {
                SessionManager.shared.currentUser.id_level = nivelMaximoDesbloqueado.ToString();

                string idUsuario = SessionManager.shared.currentUser.id;

                UsuarioService uService = new UsuarioService();
                await uService.ActualizarNivelUsuario(idUsuario, nivelMaximoDesbloqueado);

                Debug.Log($"Progreso actualizado en la Base de Datos para el usuario: Nivel {nivelMaximoDesbloqueado}");
            }
        }
    }
}
