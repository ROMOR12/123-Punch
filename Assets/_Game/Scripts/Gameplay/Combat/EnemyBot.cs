using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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

    public float maxLife;
    private float currentForce;
    private float currentEnergy;
    private float currentRecovery;
    private string enemyName;

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
            // Cargamos los datos por defecto desde el ScriptableObject
            maxLife = enemyData.life;
            currentForce = enemyData.force;
            currentEnergy = enemyData.energy;
            currentRecovery = enemyData.recovery;
            enemyName = enemyData.name;

            currentLife = maxLife;
            name = enemyName;
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
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            int dice = Random.Range(0, 100);

            if (dice < 30)
            {
                yield return StartCoroutine(RealizarFinta());
            }
            else if (dice < 60)
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
            float baseDamage = currentForce * damageMultiplier;
            int finalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier);
            playerCombat.ReceiveDamage(finalDamage);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator RealizarAtaqueInparable(float tiempoAviso, float criticalMultiplier)
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.magenta;
        StartCoroutine(MostrarAlerta(iconoAtaqueFuerte, Color.white, 0.7f));
        yield return new WaitForSeconds(tiempoAviso);

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null && enemyData != null)
        {
            if (countSpecialAttacks >= 2)
            {
                spriteRenderer.color = Color.grey;
                yield return new WaitForSeconds(1f);
                countSpecialAttacks = 0;
                playerCombat.recuperarEnergia(20);
                yield break;
            }

            StartCoroutine(ShowEnemyAttackVisuals(true));

            float baseDamage = currentForce * damageMultiplier;
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

        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;
        StartCoroutine(MostrarAlerta(iconoFinta, Color.white, 0.7f));

        Vector3 originalPos = transform.position;
        transform.position = originalPos + new Vector3(ladoAtaque * 1.5f, 0, 0);

        yield return new WaitForSeconds(1f);

        transform.position = originalPos;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (playerCombat != null)
        {
            bool esquivaExitosa = false;
            if (playerCombat.dodgeDirection != 0 && playerCombat.dodgeDirection != ladoAtaque)
            {
                esquivaExitosa = true;
            }

            if (!esquivaExitosa)
            {
                float baseDamage = currentForce * damageMultiplier;
                int damageFinta = Mathf.RoundToInt(baseDamage * 3f);
                playerCombat.ReceiveTrueDamage(damageFinta);
                SoundManager.PlaySound(SoundType.Complaint);
            }
        }
        yield return new WaitForSeconds(0.5f);
        isFinte = false;
    }

    public bool TakeDamage(int damage)
    {
        if (isFinte) return false;

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

        SoundManager.PlaySound(SoundType.EnemyComplaint, 0.2f);
        return true;
    }

    IEnumerator ShowEnemyAttackVisuals(bool esAtaqueFuerte)
    {
        if (spriteRenderer != null && enemyData != null)
        {
            if (esAtaqueFuerte)
            {
                if (enemyData.HardAttackSprite != null) spriteRenderer.sprite = enemyData.HardAttackSprite;
            }
            else
            {
                if (enemyData.AttackSprite != null) spriteRenderer.sprite = enemyData.AttackSprite;
            }
        }

        yield return new WaitForSeconds(0.2f);

        if (spriteRenderer != null) spriteRenderer.sprite = enemyData.sprite;
    }

    IEnumerator MostrarAlerta(Sprite nuevoSprite, Color colorAviso, float duracion)
    {
        if (AlertIcon != null && alertaSpriteRenderer != null)
        {
            if (nuevoSprite != null) alertaSpriteRenderer.sprite = nuevoSprite;
            AlertIcon.SetActive(true);
            AlertIcon.transform.localScale = Vector3.one * 1.5f;

            float tiempoPasado = 0f;
            float velocidadParpadeo = 5f;

            while (tiempoPasado < duracion)
            {
                float alpha = Mathf.PingPong(Time.time * velocidadParpadeo, 1f);
                alpha = Mathf.Clamp(alpha, 0.4f, 1f);

                Color colorFinal = colorAviso;
                colorFinal.a = alpha;
                alertaSpriteRenderer.color = colorFinal;

                tiempoPasado += Time.deltaTime;
                yield return null;
            }

            AlertIcon.SetActive(false);
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
        yield return StartCoroutine(RealizarAtaque(0.4f, 2f));
        yield return new WaitForSeconds(1.5f);
        currentAttackRoutine = StartCoroutine(EnemyAttackLoop());
    }

    private IEnumerator ShakeEffect()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
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

        currentLife = maxLife;

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

            if (spriteRenderer != null) spriteRenderer.color = new Color(0f, 0f, 0f, 0f);
        }

        if (spriteRenderer != null && numDeath != 2) spriteRenderer.color = Color.gray;

        GameEvents.TriggerKO(); if (playerCombat != null) playerCombat.Win();
    }

    public void SobrescribirStatsDeFirebase(int nuevaVida, int nuevaEnergia, int nuevaFuerza, int nuevaRecuperacion, string nuevoNombre)
    {
        maxLife = nuevaVida;
        currentLife = maxLife;
        currentForce = nuevaFuerza;
        currentEnergy = nuevaEnergia;
        currentRecovery = nuevaRecuperacion;
        enemyName = nuevoNombre;
        name = enemyName;

        Debug.Log($"Stats del enemigo {nuevoNombre} actualizadas desde Firebase!");
    }

    public void CambiarVisualesEnemigo(EnemyBase nuevosVisuales)
    {
        enemyData = nuevosVisuales;
        if (spriteRenderer != null && enemyData.sprite != null)
        {
            spriteRenderer.sprite = enemyData.sprite;
            originalColor = spriteRenderer.color; // Guardar el color base
        }
    }
}