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
    public float shakeAmount = 0.1f;
    public float shakeDuration = 0.2f;
    public GameObject deathEffectPrefab;
    public GameObject AlertIcon;
    public SpriteRenderer alertaSpriteRenderer;
    public Sprite iconoAtaqueNormal;
    public Sprite iconoAtaqueFuerte;
    public Sprite iconoFinta;


    private Color originalColor;
    private float currentLife;
    private Coroutine currentAttackRoutine;
    private int countSpecialAttacks = 0;

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

    IEnumerator ShowEnemyAttackVisuals(bool esAtaqueFuerte)
    {
        if (spriteRenderer != null && enemyData != null)
        {
            if (esAtaqueFuerte)
            {
                // Si es fuerte, ponemos el sprite fuerte
                if (enemyData.HardAttackSprite != null)
                    spriteRenderer.sprite = enemyData.HardAttackSprite;
            }
            else
            {
                // Si NO es fuerte, ponemos el normal
                if (enemyData.AttackSprite != null)
                    spriteRenderer.sprite = enemyData.AttackSprite;
            }
        }

        yield return new WaitForSeconds(0.2f); // Tiempo que se ve el golpe

        // Volvemos a la pose normal (Idle)
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
    }

    IEnumerator MostrarAlerta(Sprite nuevoSprite, Color colorAviso, float duracion)
    {
        if (AlertIcon != null && alertaSpriteRenderer != null)
        {
            // Configurar Sprite y activar
            if (nuevoSprite != null) alertaSpriteRenderer.sprite = nuevoSprite;
            AlertIcon.SetActive(true);

            // Un pequeño "pop" inicial de tamaño
            AlertIcon.transform.localScale = Vector3.one * 1.5f;

            //BUCLE DE PARPADEO
            float tiempoPasado = 0f;
            float velocidadParpadeo = 5f;

            while (tiempoPasado < duracion)
            {
                // Calculamos la transparencia
                // PingPong hace que el valor vaya de 0 a 1 y de 1 a 0 repetidamente
                float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);

                //Que no desaparezca del todo (mínimo 0.3 de opacidad) para que siempre se vea algo
                alpha = Mathf.Clamp(alpha, 0.4f, 1f);

                // Creamos el color nuevo respetando el color original (blanco, rojo, etc)
                Color colorFinal = colorAviso;
                colorFinal.a = alpha; // Solo cambiamos la "a" (transparencia)

                alertaSpriteRenderer.color = colorFinal;

                tiempoPasado += Time.deltaTime;
                yield return null; // Esperamos al siguiente frame
            }

            // Apagar
            AlertIcon.SetActive(false);

            // Restauramos el color con opacidad al 100% por si acaso para la próxima
            Color restaurar = colorAviso;
            restaurar.a = 1f;
            alertaSpriteRenderer.color = restaurar;
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
        int ladoAtaque = (Random.Range(0, 2) == 0) ? -1 : 1;

        // AVISO VISUAL
        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        StartCoroutine(MostrarAlerta(iconoFinta, Color.white, 0.7f));

        Vector3 originalPos = transform.position;
        transform.position = originalPos + new Vector3(ladoAtaque * 1.5f, 0, 0);

        Debug.Log($"¡FINTA CIAN! Viene por la {(ladoAtaque == -1 ? "IZQUIERDA" : "DERECHA")}");

        // 2. TIEMPO DE REACCIÓN
        yield return new WaitForSeconds(1f);

        // 3. GOLPE
        transform.position = originalPos;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null)
        {
            bool esquivaExitosa = false;
            if (playerCombat.dodgeDirection != 0 && playerCombat.dodgeDirection != ladoAtaque)
            {
                esquivaExitosa = true;
            }

            if (esquivaExitosa)
            {
                Debug.Log("¡Esquiva perfecta!");
            }
            else
            {
                Debug.Log("¡Te comiste la finta!");
                float baseDamage = enemyData.force * damageMultiplier;
                int damageFinta = Mathf.RoundToInt(baseDamage * 3f);
                playerCombat.ReceiveTrueDamage(damageFinta);
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

        StartCoroutine(MostrarAlerta(iconoAtaqueNormal, Color.white, tiempoAviso));

        yield return new WaitForSeconds(tiempoAviso);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        StartCoroutine(ShowEnemyAttackVisuals(false));
        

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
        // 1. AVISO VISUAL (Color + Icono)
        if (spriteRenderer != null) spriteRenderer.color = Color.magenta;

        // --- CORRECCIÓN: ESTA LÍNEA AHORA ESTÁ ANTES DEL WAIT ---
        StartCoroutine(MostrarAlerta(iconoAtaqueFuerte, Color.white, 0.7f));
        // --------------------------------------------------------

        // 2. ESPERA (Ahora el jugador ve el aviso mientras espera)
        yield return new WaitForSeconds(tiempoAviso);


        // 3. GOLPE
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null && enemyData != null)
        {
            // Lógica de cansancio
            if (countSpecialAttacks >= 2)
            {
                Debug.Log("¡El enemigo está agotado!");
                spriteRenderer.color = Color.grey;
                yield return new WaitForSeconds(1f);
                countSpecialAttacks = 0;
                playerCombat.recuperarEnergia(20);
                yield break;
            }

            StartCoroutine(ShowEnemyAttackVisuals(true)); // Sprite de ataque fuerte

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