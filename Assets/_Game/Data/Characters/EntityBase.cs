using UnityEngine;

// Clase padre para crear personajes y enemigos fácilmente desde el editor
public class EntityBase : ScriptableObject
{
    // Identificación visual y de sistema
    public int id;
    public string entityName;
    public Sprite sprite;

    [Header("Stats Base")]
    // Estadísticas principales de combate
    public int life;      // Vida máxima
    public int energy;    // Estamina para acciones
    public int force;     // Daño base de los ataques
    public int recovery;  // Rapidez para recuperar energía
}