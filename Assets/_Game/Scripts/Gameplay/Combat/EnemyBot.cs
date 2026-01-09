using UnityEngine;
using System.Collections;

public class EnemyBot : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyBase enemyData;
    public SpriteRenderer spriteRenderer;
    public CombatController playerCombat;
    public float minWaitTime = 2f;
    public float maxWaitTime = 4f;

    [Header("Balance del Juego")]
    public float damageMultiplier = 5f;

    [Header("Feedback Visual")]
    public Color hitColor = Color.red;
    public float shakeAmount = 0.1f; // <--- NUEVO: Cuánto vibra (0.1 es suave, 0.5 es terremoto)
    public float shakeDuration = 0.2f; // <--- NUEVO: Cuánto dura el temblor
    private Color originalColor;

    private float currentLife;
    private Coroutine currentAttackRoutine;

    void Start()
    {
        if (enemyData != null)
        {
            currentLife = enemyData.life;
            name = enemyData.name;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = enemyData.sprite;
                originalColor = spriteRenderer.color;
            }
            currentAttackRoutine = StartCoroutine(EnemyAttackLoop());
        }
    }

    IEnumerator EnemyAttackLoop()
    {
        while (currentLife > 0)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
            // Ataque Normal
            yield return StartCoroutine(RealizarAtaque(0.5f, 1f));
        }
    }

    public void CastigarJugador()
    {
        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        currentAttackRoutine = StartCoroutine(RutinaCastigo());
    }

    IEnumerator RutinaCastigo()
    {
        Debug.Log("¡CASTIGO! Golpe Crítico.");

        yield return new WaitForSeconds(1f);
        // Ataque Rápido y Daño Doble
        yield return StartCoroutine(RealizarAtaque(0.4f, 2f));
        yield return new WaitForSeconds(1.5f);
        currentAttackRoutine = StartCoroutine(EnemyAttackLoop());
    }

    IEnumerator RealizarAtaque(float tiempoAviso, float criticalMultiplier)
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(tiempoAviso);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null && enemyData != null)
        {
            float baseDamage = enemyData.force * damageMultiplier;
            int finalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
            playerCombat.ReceiveDamage(finalDamage);
        }
        yield return new WaitForSeconds(0.5f);
    }

    public void TakeDamage(int damage)
    {
        currentLife -= damage;
        if (currentLife < 0) currentLife = 0;

        // --- EFECTOS VISUALES ---
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect()); // Parpadeo Rojo
            StartCoroutine(ShakeEffect()); // <--- NUEVO: TEMBLOR
        }

        if (currentLife <= 0)
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }

    // --- NUEVA RUTINA DE VIBRACIÓN ---
    private IEnumerator ShakeEffect()
    {
        // 1. Guardamos la posición original para no perderla
        Vector3 originalPos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // 2. Calculamos una posición aleatoria muy cerca de la original
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            // 3. Movemos al enemigo
            transform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null; // Esperamos al siguiente frame
        }

        // 4. IMPORTANTE: Devolvemos al enemigo a su sitio exacto
        transform.position = originalPos;
    }

    private IEnumerator FlashEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }
}