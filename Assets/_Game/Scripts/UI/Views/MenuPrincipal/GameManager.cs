using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int numCajas = 0;

    public static GameManager Instance;

    public List<ModosDeJuego> modosDeJuego;

    public List<ItemBase> items;

    public Sprite imageDefault;

    public List<BaseCharacter> listaPersonajes;

    // --- NUEVO: SISTEMA DE INVENTARIO Y EQUIPAMIENTO ---
    [Header("Inventario y Equipamiento (Firebase)")]
    public List<string> inventarioIDs = new List<string>();
    public string pasivoEquipadoID = "";
    public List<string> activosEquipadosIDs = new List<string> { "", "" };

    [Header("Bases de Datos Maestras (ScriptableObjects)")]
    [Tooltip("Arrastra aquí todos los ScriptableObjects de objetos Pasivos")]
    public List<Pasivo> todosLosPasivos;
    [Tooltip("Arrastra aquí todos los ScriptableObjects de objetos Consumibles")]
    public List<Consumible> todosLosActivos;
    // --------------------------------------------------

    [Header("Progreso Actual")]
    private string _idPersonajeSeleccionado;

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

    // Métodos de utilidad opcionales para buscar objetos rápidamente
    public Pasivo GetPasivoPorID(string id) => todosLosPasivos.Find(p => p.id == id);
    public Consumible GetActivoPorID(string id) => todosLosActivos.Find(a => a.id == id);
}