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
    private Vector3 startPosition;
    private bool isFinte = false;
    private int numDeath = 0;

    void Start()
    {
        if (enemyData != null)
        {
            currentLife = enemyData.life;
            name = enemyData.name;
            startPosition = transform.position;
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
            // espera aleatoria antes del siguiente ataque
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // caulculados la posibilidad del siguiente ataque
            int dice = Random.Range(0, 100);

            // 30% de probabilidad de que el ataque sea una finta
            if (dice < 30)
            {
                yield return StartCoroutine(RealizarFinta());
            }
            // 30% de probabilidad de que el ataque sea imparable
            else if (dice < 60)
            {
                // si ya ha hecho 2 ataques fuertes, hace uno normal
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
            // 40% de probabilidad de que el ataque sea normal
            else
            {
                countSpecialAttacks = 0;
                yield return StartCoroutine(RealizarAtaque(1f, 1f));
            }
        }
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
        // aviso visual
        if (spriteRenderer != null) spriteRenderer.color = Color.magenta;

        // mostrar alerta
        StartCoroutine(MostrarAlerta(iconoAtaqueFuerte, Color.white, 0.7f));

        // espera tiempo de reacción
        yield return new WaitForSeconds(tiempoAviso);


        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null && enemyData != null)
        {
            // logica de cansancio
            if (countSpecialAttacks >= 2)
            {
                Debug.Log("¡El enemigo está agotado!");
                spriteRenderer.color = Color.grey;
                yield return new WaitForSeconds(1f);
                countSpecialAttacks = 0;
                playerCombat.recuperarEnergia(20);
                yield break;
            }

            StartCoroutine(ShowEnemyAttackVisuals(true)); // sprite de ataque fuerte

            float baseDamage = enemyData.force * damageMultiplier;
            int finalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
            playerCombat.ReceiveUnstoppableDamage(finalDamage);
        }
        yield return new WaitForSeconds(0.5f);
        countSpecialAttacks++;
    }

    IEnumerator RealizarFinta()
    {
        int ladoAtaque = (Random.Range(0, 2) == 0) ? -1 : 1;
        isFinte = true;

        // aviso visual
        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        StartCoroutine(MostrarAlerta(iconoFinta, Color.white, 0.7f));

        Vector3 originalPos = transform.position;
        transform.position = originalPos + new Vector3(ladoAtaque * 1.5f, 0, 0);

        Debug.Log($"¡FINTA CIAN! Viene por la {(ladoAtaque == -1 ? "IZQUIERDA" : "DERECHA")}");

        // espera tiempo de reacción
        yield return new WaitForSeconds(1f);

        // golpe
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
                // no pasa nada
            }
            else
            {
                float baseDamage = enemyData.force * damageMultiplier;
                int damageFinta = Mathf.RoundToInt(baseDamage * 3f);
                playerCombat.ReceiveTrueDamage(damageFinta);
            }
        }
        yield return new WaitForSeconds(0.5f);
        isFinte = false;
    }

    public bool TakeDamage(int damage)
    {
        if (isFinte)
        {
            Debug.Log("¡MISS! El enemigo esquivó.");
            return false;
        }

        currentLife -= damage;

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
            StartCoroutine(ShakeEffect());
        }

        if (currentLife <= 0)
        {
            currentLife = 0;
            StopAllCoroutines();
            EnemyDead();
        }

        return true;
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

        yield return new WaitForSeconds(0.2f); // tiempo que se ve el golpe

        // volvemos a la pose normal
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = enemyData.sprite;
        }
    }

    IEnumerator MostrarAlerta(Sprite nuevoSprite, Color colorAviso, float duracion)
    {
        if (AlertIcon != null && alertaSpriteRenderer != null)
        {
            // configurar Sprite y activar
            if (nuevoSprite != null) alertaSpriteRenderer.sprite = nuevoSprite;
            AlertIcon.SetActive(true);

            // animacion de pop 
            AlertIcon.transform.localScale = Vector3.one * 1.5f;

            // bucle del parpadeo
            float tiempoPasado = 0f;
            float velocidadParpadeo = 5f;

            while (tiempoPasado < duracion)
            {
                // calculamos la transpariencia usando el metodo pingpong
                // pingpong hace que el valor vaya de 0 a 1 y de 1 a 0 repetidamente
                float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);

                // trapear el alpha entre 0.4 y 1 para que no desaparezca del todo
                alpha = Mathf.Clamp(alpha, 0.4f, 1f);

                // asignamos el color con la nueva alpha
                Color colorFinal = colorAviso;
                colorFinal.a = alpha; // Solo cambiamos la transparencia 

                alertaSpriteRenderer.color = colorFinal;

                tiempoPasado += Time.deltaTime;
                yield return null; // esperamos al siguiente frame
            }

            // apagamos el icono 
            AlertIcon.SetActive(false);

            // restauramos el color con opacidad al 100% por si acaso para la proxima
            Color restaurar = colorAviso;
            restaurar.a = 1f;
            alertaSpriteRenderer.color = restaurar;
        }
    }

    public void CastigarJugador()
    {
        if (currentAttackRoutine != null) StopCoroutine(currentAttackRoutine);
        currentAttackRoutine = StartCoroutine(RutinaCastigo());
    }

    IEnumerator RutinaCastigo()
    {
        yield return new WaitForSeconds(1f);

        // ataque Rapido y Daño Doble
        yield return StartCoroutine(RealizarAtaque(0.4f, 2f));
        yield return new WaitForSeconds(1.5f);
        currentAttackRoutine = StartCoroutine(EnemyAttackLoop());
    }

    private IEnumerator ShakeEffect()
    {
        // guardamos la posicion original para no perderla
        Vector3 originalPos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // calculamos una posición aleatoria muy cerca de la original
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            // movemos al enemigo
            transform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null; // Esperamos al siguiente frame
        }

        // devolvemos a la posición original
        transform.position = originalPos;
    }

    private IEnumerator FlashEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }

    public void ReiniciarParaRonda()
    {
        StopAllCoroutines();
         
        currentLife = enemyData.life; 

        transform.position = startPosition;

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        currentAttackRoutine = StartCoroutine(EnemyAttackLoop());
    }

    private void EnemyDead()
    {
        StopAllCoroutines();
        numDeath++;

        if (numDeath == 2) 
        {
            if (deathEffectPrefab != null)
            {
                GameObject explosion = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 2f);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0f, 0f, 0f, 0f);
            }
        }

        if (spriteRenderer != null && numDeath != 2) spriteRenderer.color = Color.gray;

        if (playerCombat != null)
        {
            playerCombat.Win();
        }
    }
}