using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NuevoPersonaje", menuName = "ScriptableObjects/Personaje")]
public class BaseCharacter : EntityBase
{
    [Header("Sprites")]
    public Sprite AttackSprite;
    public Sprite DefetSprite;
    public Sprite HardAttackSprite;

    [Header("Inventario Inicial")]
    public List<Pasivo> objetosPasivos;
    public List<Consumible> objetosConsumibles;
}
