using UnityEngine;

[CreateAssetMenu(fileName = "NuevoPersonaje", menuName = "ScriptableObjects/Personaje")]
public class BaseCharacter : EntityBase
{
    [Header("Sprites")]
    public Sprite AttackSprite;
    public Sprite DefetSprite;
   
}
