using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class EnemyBot : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyBase enemyData; // <-- Arrastra aquí tu ScriptableObject de Enemigo
    public SpriteRenderer spriteRenderer;
    public CombatController playerCombat; // Referencia para saber a quién pegar
    public float minWaitTime = 2f; // Tiempo mínimo entre golpes
    public float maxWaitTime = 4f; // Tiempo máximo

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

            StartCoroutine(EnemyAttackLoop());
        }
        else
        {
            Debug.LogError("¡Falta asignar el EnemyData en el inspector!");
        }
    }

    IEnumerator EnemyAttackLoop()
    {
        while (currentLife > 0) // Mientras esté vivo
        {
            // 1. ESPERAR: Tiempo aleatorio pensando
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // 2. AVISAR: Se pone AMARILLO (Prepara el golpe)
            if (spriteRenderer != null) spriteRenderer.color = Color.yellow;

            // Le damos 0.5 segundos al jugador para reaccionar
            yield return new WaitForSeconds(0.5f);

            // 3. ATACAR
            // Volvemos al color normal
            if (spriteRenderer != null) spriteRenderer.color = Color.white;

            // Llamamos a la función del jugador para hacerle daño
            if (playerCombat != null)
            {
                playerCombat.ReceiveDamage(enemyData.force * 15);
            }

            // 4. REPOSO: Espera un poquito después de pegar
            yield return new WaitForSeconds(1f);
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
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Debug.Log("¡K.O.! Has ganado.");
        // Aquí dispararíamos la animación de caer o la ventana de victoria
        gameObject.SetActive(false);
    }
}