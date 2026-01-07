using UnityEngine;

public class EntityBase : ScriptableObject
{
    public int id;
    public string entityName;
    public Sprite sprite;

    [Header("Stats")]
    public int life;
    public int energy;
    public int force;
    public int recovery;
}