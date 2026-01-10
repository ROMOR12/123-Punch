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
    public float shakeDuration = 0.2f;// <--- NUEVO: Cuánto dura el temblor
    public GameObject deathEffectPrefab;

    private Color originalColor;
    private float currentLife;
    private Coroutine currentAttackRoutine;
    private int countSpecialAttacks = 0;
    private bool isSpecialAttack = false;

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

    IEnumerator ShowEnemyAttackVisuals()
    {
        if (spriteRenderer != null && enemyData.AttackSprite != null && !isSpecialAttack)
        {
            spriteRenderer.sprite = enemyData.AttackSprite;
        }

        if (spriteRenderer != null && enemyData.AttackSprite != null && isSpecialAttack)
        {
            isSpecialAttack = false;
            spriteRenderer.sprite = enemyData.HardAttackSprite;
        }

        yield return new WaitForSeconds(0.2f);

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
    }

    IEnumerator EnemyAttackLoop()
    {
        while (currentLife > 0)
        {
            // Espera aleatoria
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            int dice = Random.Range(0, 100);

            // --- LÓGICA DE PROBABILIDADES ---
            if (dice < 30) // 30% Probabilidad de FINTA
            {
                yield return StartCoroutine(RealizarFinta());
            }
            else if (dice < 60) // 30% Ataque IMPARABLE
            {
                if (countSpecialAttacks >= 2)
                {
                    countSpecialAttacks = 0;
                    yield return StartCoroutine(RealizarAtaque(1f, 1f));
                }
                else
                {
                    yield return StartCoroutine(RealizarAtaqueInparable(1f, 1.5f));
                }
            }
            else // 40% Ataque NORMAL
            {
                countSpecialAttacks = 0;
                yield return StartCoroutine(RealizarAtaque(1f, 1f));
            }
        }
    }

    IEnumerator RealizarFinta()
    {
        // 1. ELEGIR LADO: -1 (Izquierda) o 1 (Derecha)
        int ladoAtaque = (Random.Range(0, 2) == 0) ? -1 : 1;

        // 2. AVISO VISUAL (Se pone CIAN y se mueve un poco a ese lado)
        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        Vector3 originalPos = transform.position;
        // Se mueve un poco para indicar de dónde viene el golpe
        transform.position = originalPos + new Vector3(ladoAtaque * 1.5f, 0, 0);

        Debug.Log($"¡FINTA CIAN! Viene por la {(ladoAtaque == -1 ? "IZQUIERDA" : "DERECHA")}. ¡Esquiva al otro lado!");

        // 3. TIEMPO DE REACCIÓN (El jugador debe deslizar ahora)
        yield return new WaitForSeconds(1f);

        // 4. EL GOLPE (Vuelve al sitio y comprueba)
        transform.position = originalPos;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null)
        {
            // LÓGICA MATEMÁTICA DE LA ESQUIVA:
            // Si viene por Izquierda (-1), jugador debe estar en Derecha (1). (-1 != 1) -> SAFE
            // Si viene por Derecha (1), jugador debe estar en Izquierda (-1). (1 != -1) -> SAFE
            // Si jugador está en 0 (Centro) o en el mismo lado -> DAÑO

            bool esquivaExitosa = false;

            // Si estamos en lados opuestos, nos salvamos
            if (playerCombat.dodgeDirection != 0 && playerCombat.dodgeDirection != ladoAtaque)
            {
                esquivaExitosa = true;
            }

            if (esquivaExitosa)
            {
                Debug.Log("¡Esquiva perfecta! 😎");
                // Opcional: Sonido de "Swish"
            }
            else
            {
                Debug.Log("¡Te pilló! No esquivaste al lado correcto.");
                // Daño crítico por fallar la esquiva (x3 daño)
                float baseDamage = enemyData.force * damageMultiplier;
                int damageFinta = Mathf.RoundToInt(baseDamage * 3f);

                // Usamos el daño imparable para que el bloqueo no sirva, TIENES que esquivar
                playerCombat.ReceiveUnstoppableDamage(damageFinta);
            }
        }
        yield return new WaitForSeconds(0.5f);
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
        StartCoroutine(ShowEnemyAttackVisuals());

        if (playerCombat != null && enemyData != null)
        {
            float baseDamage = enemyData.force * damageMultiplier;
            int finalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
            playerCombat.ReceiveDamage(finalDamage);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator RealizarAtaqueInparable(float tiempoAviso, float criticalMultiplier)
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.magenta;
        yield return new WaitForSeconds(tiempoAviso);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (playerCombat != null && enemyData != null)
        {
            if (countSpecialAttacks >= 2)
            {
                Debug.Log("¡El enemigo está agotado después de tantos ataques especiales!");
                spriteRenderer.color = Color.grey; // Cambiamos el color para indicar agotamiento
                yield return new WaitForSeconds(1f); // Pausa para el efecto visual
                countSpecialAttacks = 0; // Reiniciamos el contador
                playerCombat.recuperarEnergia(20); // El jugador recupera energía
                yield break; // Cancelamos el ataque especial
            }
            StartCoroutine(ShowEnemyAttackVisuals());
            isSpecialAttack = true;
            float baseDamage = enemyData.force * damageMultiplier;
            int finalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
            playerCombat.ReceiveUnstoppableDamage(finalDamage);
        }
        yield return new WaitForSeconds(0.5f);
        countSpecialAttacks++;
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
            EnemyDead();
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

    private void EnemyDead()
    {

        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
        playerCombat.Win();

    }
}