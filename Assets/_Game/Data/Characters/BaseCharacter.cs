using System.Collections.Generic;
using UnityEngine;

// Permite crear nuevos personajes directamente desde el menú de Unity
[CreateAssetMenu(fileName = "NuevoPersonaje", menuName = "ScriptableObjects/Personaje")]

// Hereda de EntityBase: Añade características exclusivas del jugador
public class BaseCharacter : EntityBase
{
    [Header("Sprites")]
    // Poses visuales para los diferentes estados del combate
    public Sprite AttackSprite;       // Al atacar normal
    public Sprite DefetSprite;        // Al bloquear/defender
    public Sprite HardAttackSprite;   // Al hacer el ataque fuerte

    [Header("Inventario Inicial")]
    // Listas de objetos con los que el personaje empieza la partida
    public List<Pasivo> objetosPasivos;
    public List<Consumible> objetosConsumibles;
}
