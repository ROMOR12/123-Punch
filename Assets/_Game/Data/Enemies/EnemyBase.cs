using UnityEngine;

// Permite crear nuevos enemigos rápidamente desde el menú de Unity
[CreateAssetMenu(fileName = "NuevoEnemigo", menuName = "ScriptableObjects/Enemigo")]

// Hereda de EntityBase: Añade las características visuales exclusivas de los rivales
public class EnemyBase : EntityBase
{
    [Header("Sprites")]
    // Poses visuales que usará el bot enemigo al golpear
    public Sprite AttackSprite;       // Sprite para el ataque normal
    public Sprite HardAttackSprite;   // Sprite para el ataque fuerte/imparable
}
