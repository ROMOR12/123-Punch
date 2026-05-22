using UnityEngine;

// Clase padre para crear personajes y enemigos f�cilmente desde el editor
public class EntityBase : ScriptableObject
{
    // Identificaci�n visual y de sistema
    public string id;
    public string entityName;
    public Sprite sprite;

    [Header("Stats Base")]
    // Estad�sticas principales de combate
    public int life;      // Vida m�xima
    public int energy;    // Estamina para acciones
    public int force;     // Da�o base de los ataques
    public int recovery;  // Rapidez para recuperar energia

    [Header("Desbloqueo")]
    public string unlockCondition = "coins"; // "coins", "default", "historia_1", etc.
    public int price = 1000;
}