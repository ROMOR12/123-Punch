using UnityEngine;

[CreateAssetMenu(fileName = "NuevoEnemigo", menuName = "ScriptableObjects/Enemigo")]
public class EnemyBase : EntityBase
{
    [Header("Sprites")]
    public Sprite AttackSprite;
    public Sprite HardAttackSprite;
}
