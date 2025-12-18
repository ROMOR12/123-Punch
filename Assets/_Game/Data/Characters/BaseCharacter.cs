using UnityEngine;

[CreateAssetMenu(fileName = "NuevoPersonaje", menuName = "ScriptableObjects/Personaje")]
public class BaseCharacter : ScriptableObject
{
    public int id;
    public string name;
    public Sprite sprite;
    public int life;
    public int energy;
    public int force;
    public int recovery;
}
