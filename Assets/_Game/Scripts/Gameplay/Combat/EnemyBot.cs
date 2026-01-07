using UnityEngine;
using System.Collections;

public class EnemyBot : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyBase enemyData; // <-- Arrastra aquí tu ScriptableObject de Enemigo
    public SpriteRenderer spriteRenderer;

    [Header("Feedback Visual")]
    public Color hitColor = Color.red;
    private Color originalColor;

    // Variables de estado
    private float currentLife;

    void Start()
    {
        // 1. Inicializamos al enemigo con los datos del archivo
        if (enemyData != null)
        {
            currentLife = enemyData.life;
            name = enemyData.name; // Cambia el nombre en la jerarquía

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = enemyData.sprite;
                originalColor = spriteRenderer.color;
            }
        }
        else
        {
            Debug.LogError("¡Falta asignar el EnemyData en el inspector!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        Debug.Log($"El enemigo recibió {damage} de daño. Vida restante: {currentLife}");

        // Efecto visual de golpe
        if (spriteRenderer != null) StartCoroutine(FlashEffect());

        if (currentLife <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("¡K.O.! Has ganado.");
        // Aquí dispararíamos la animación de caer o la ventana de victoria
        gameObject.SetActive(false);
    }
}