using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CombatController : MonoBehaviour
{
    [Header("Datos del Jugador")]
    public BaseCharacter playerData;

    [HideInInspector] public float maxLife;
    [HideInInspector] public float maxEnergy;
    [HideInInspector] public float currentForce;
    [HideInInspector] public float currentRecovery;

    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;

    [Header("Inventario")]
    public List<Pasivo> pasivosEquipados;

    [Header("Sistema de Esquiva")]
    public int dodgeDirection = 0;
    private bool isDodging = false;
    private Vector3 originalPosition;

    [Header("Control Táctil (Swipe)")]
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    public float minSwipeDistance = 50f;

    [Header("Referencias de Escena")]
    public EnemyBot currentEnemy;
    public Slider defenseSlider;
    public Slider staminaBar;
    public ParticleSystem hitParticles;
    public ParticleSystem stunParticles;
    public GameObject damagePopupPrefab;
    public RoundManager roundManager;

    [Header("Balance del Juego")]
    public float playerDamageMultiplier = 5f;
    public float staminaRegenMultiplier = 3f;
    public float maxBlockStaminaCost = 25f;

    [Header("Feedback de Daño")]
    public float shakeAmount = 0.15f;
    public float shakeDuration = 0.2f;
    public Color damageColor = Color.red;

    [Header("Efectos de victoria")]
    public ParticleSystem victoriaConfeti;
    public GameObject victoriaTextoUI;

    [Header("Barras y Textos de Vida")]
    public Slider playerHealth;
    public TMP_Text playerHealthText;
    public TMP_Text playerName;
    public TMP_Text EnemyName;

    [Header("Configuración Defensa Visual")]
    public Image defenseFillImage;
    public Color colorDefensaNormal = Color.blue;
    public Color colorParry = Color.yellow;
    public float zonaParry = 0.95f;

    [Header("Efectos UI")]
    public Image staminaFillImage;
    public Color normalStaminaColor = Color.yellow;
    public Color lowStaminaColor = Color.red;

    [Header("Inventario en combate")]
    public List<Consumible> mochilaConsumibles;

    public Slider enemyHealthBar;
    public TMP_Text enemyHealthText;

    private float currentEnergy;
    private float currentLife;
    private bool isDefending = false;
    private bool isStunned = false;
    private bool isDead = false;
    private const float ATTACK_COST = 15f;
    private bool isHardAttack = false;
    private bool victory = false;

    void Start()
    {
        if (playerData != null)
        {
            maxLife = (float)playerData.life;
            maxEnergy = (float)playerData.energy;
            currentForce = (float)playerData.force;
            currentRecovery = (float)playerData.recovery;

            if (playerData.objetosPasivos != null)
            {
                foreach (var item in playerData.objetosPasivos)
                {
                    if (item != null) item.Equipar(this);
                }
            }

            if (playerData.objetosConsumibles != null)
            {
                mochilaConsumibles = new List<Consumible>(playerData.objetosConsumibles);
            }
            else
            {
                mochilaConsumibles = new List<Consumible>();
            }

            foreach (var pasivo in pasivosEquipados)
            {
                // Le pasamos 'this' porque ahora CombatController es el que se encarga de los items
                if (pasivo != null) pasivo.Equipar(this);
            }

            // Asignamos vida y energía inicial usando los NUEVOS máximos (que pueden haber crecido por items)
            currentLife = maxLife;
            currentEnergy = maxEnergy;

            // Configuración visual
            if (playerSpriteRenderer != null && playerData.sprite != null)
                playerSpriteRenderer.sprite = playerData.sprite;

            // --- CONFIGURACIÓN JUGADOR UI ---
            if (playerHealth != null)
            {
                playerName.text = playerData.name;
                playerHealth.maxValue = maxLife;
                playerHealth.value = currentLife;
                UpdateHealthText(playerHealthText, currentLife, maxLife);
            }

            if (staminaBar != null)
            {
                staminaBar.maxValue = maxEnergy;
                staminaBar.value = currentEnergy;
            }
        }

        if (enemyHealthBar != null && currentEnemy != null)
        {
            EnemyName.text = currentEnemy.enemyData.name;
            float enemyMax = (float)currentEnemy.enemyData.life;
            enemyHealthBar.maxValue = enemyMax;
            enemyHealthBar.value = enemyMax;
            UpdateHealthText(enemyHealthText, enemyMax, enemyMax);
        }

        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
        }

        if (playerSpriteRenderer != null)
        {
            originalPosition = playerSpriteRenderer.transform.position;
        }

        HandleStaminaRegen();
    }

    void Update()
    {
        DetectarInputSwipe();
        HandleDefense();
        HandleStaminaRegen();
        HandleStaminaColor();
    }

    //Recibe modificaciones de CharacterStats
    public void AplicarCambioEstadistica(StatType tipo, int cantidad, bool esRecuperacion)
    {
        switch (tipo)
        {
            case StatType.Life:
                if (esRecuperacion)
                {
                    currentLife += cantidad;

                    if (currentLife > maxLife)
                    {
                        currentLife = maxLife;
                    }
                }
                else
                {
                    maxLife += cantidad;
                    currentLife += cantidad;
                    if (playerHealth != null) playerHealth.maxValue = maxLife;
                }

                if (playerHealth != null) playerHealth.value = currentLife;
                UpdateHealthText(playerHealthText, currentLife, maxLife);

                Debug.Log($"Vida REAL interna: {currentLife} / {maxLife}");
                break;
        }
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (staminaBar != null) staminaBar.value = currentEnergy;
        // Agregado update de vida por si acaso se llama desde fuera
        if (playerHealth != null) playerHealth.value = currentLife;
    }

    public void PerformAttack()
    {
        if (victory || isDefending || isStunned || isDead || isHardAttack) return;

        if (currentEnergy >= ATTACK_COST)
        {
            GastarEnergia(ATTACK_COST);
            SoundManager.PlaySound(SoundType.HIT);
            int damageDealt = Mathf.RoundToInt(currentForce * playerDamageMultiplier);

            StartCoroutine(ShowAttackVisuals());

            if (currentEnemy != null)
            {
                bool golpeo = currentEnemy.TakeDamage(damageDealt);

                if (golpeo)
                {
                    ShowDamagePopup(damageDealt, false);
                    if (enemyHealthBar != null)
                    {
                        enemyHealthBar.value -= damageDealt;
                        UpdateHealthText(enemyHealthText, enemyHealthBar.value, enemyHealthBar.maxValue);
                        PlayHitParticles();
                    }
                }
            }
        }
        else
        {
            Debug.Log("¡Sin energía!");
        }
    }

    public void HardAttack()
    {
        if (victory || isDefending || isStunned || isDead) return;

        float hardAttackCost = ATTACK_COST * 2f;

        if (currentEnergy >= hardAttackCost)
        {
            GastarEnergia(hardAttackCost);
            SoundManager.PlaySound(SoundType.StrongHit);
            int damageDealt = Mathf.RoundToInt(currentForce * playerDamageMultiplier * 2f);

            ShowDamagePopup(damageDealt, false);

            isHardAttack = true;
            StartCoroutine(ShowAttackVisuals());

            if (currentEnemy != null)
            {
                bool golpeo = currentEnemy.TakeDamage(damageDealt);

                if (golpeo)
                {
                    enemyHealthBar.value -= damageDealt;
                    UpdateHealthText(enemyHealthText, enemyHealthBar.value, enemyHealthBar.maxValue);
                    PlayHitParticles();
                }
            }
        }
        else
        {
            Debug.Log("¡Sin energía para ataque fuerte!");
        }
    }

    private void GastarEnergia(float cantidad)
    {
        currentEnergy -= cantidad;

        if (currentEnergy <= 0)
        {
            currentEnergy = 0;
            if (!isStunned)
            {
                StartCoroutine(EnterStunState());
            }
        }
        UpdateUI();
    }

    public void recuperarEnergia(int cantidad)
    {
        currentEnergy += cantidad;

        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        UpdateUI();
    }

    private void HandleDefense()
    {
        if (isStunned) return;
        if (defenseSlider == null) return;

        bool wasDefending = isDefending;

        if (defenseSlider.value > 0.1f)
            isDefending = true;
        else
            isDefending = false;

        if (isDefending != wasDefending)
        {
            if (isDefending)
            {
                if (playerData.DefetSprite != null)
                    playerSpriteRenderer.sprite = playerData.DefetSprite;
            }
            else
            {
                playerSpriteRenderer.sprite = playerData.sprite;
            }
        }
    }

    public void ReceiveDamage(int damageAmount)
    {
        if (isDead) return;

        int danioFinal = damageAmount;

        if (isDefending)
        {
            if (!isStunned)
            {
                float porcentajeBloqueo = defenseSlider.value;

                float factorDeDaño = 1.0f - porcentajeBloqueo;
                danioFinal = Mathf.RoundToInt(damageAmount * factorDeDaño);

                float gastoEnergia = maxBlockStaminaCost * porcentajeBloqueo;

                GastarEnergia(gastoEnergia);

                Debug.Log($"Bloqueo: {porcentajeBloqueo * 100}% | Daño: {danioFinal} | Estamina gastada: {gastoEnergia}");
                SoundManager.PlaySound(SoundType.GuardStrike, 0.1f);
            }
        }

        if (!isDefending)
        {
            SoundManager.PlaySound(SoundType.Complaint);
        }

        currentLife -= danioFinal;

        if (currentLife > 0)
        {
            StartCoroutine(EfectoDañoJugador());
        }

        if (currentLife < 0) currentLife = 0;

        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, maxLife);
        }

        if (currentLife <= 0)
        {
            Debug.Log("¡K.O.!");
            StartCoroutine(AnimacionMuerte());
        }
    }

    public void ReceiveUnstoppableDamage(int damageAmount)
    {
        if (isDead || victory) return;

        if (defenseSlider.value > 0.95f)
        {
            Debug.Log("¡PERFECT PARRY!");

            StartCoroutine(EfectoParryExitoso());
            SoundManager.PlaySound(SoundType.GuardStrike, 0.1f);

            currentEnergy += 25f;

            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

            UpdateUI();
            return;
        }

        Debug.Log("¡El ataque rompió tu defensa!");
        currentLife -= damageAmount;
        if (currentLife > 0) StartCoroutine(EfectoDañoJugador());
        if (currentLife < 0) currentLife = 0;

        if (defenseSlider.value > 0 && defenseSlider.value <= 0.95f)
        {
            SoundManager.PlaySound(SoundType.BrokeGuard, 0.1f);
        }

        if (defenseSlider.value == 0)
        {
            SoundManager.PlaySound(SoundType.Complaint);
        }

        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, maxLife);
        }

        if (currentLife <= 0)
        {
            Debug.Log("¡K.O.!");
            StartCoroutine(AnimacionMuerte());
        }
    }

    public void ReceiveTrueDamage(int damageAmount)
    {
        if (isDead || victory) return;

        Debug.Log("¡GOLPE DE FINTA! No se puede bloquear.");

        currentLife -= damageAmount;

        if (currentLife > 0) StartCoroutine(EfectoDañoJugador());
        if (currentLife < 0) currentLife = 0;

        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, maxLife);
        }

        if (currentLife <= 0)
        {
            StartCoroutine(AnimacionMuerte());
        }
    }

    void UpdateHealthText(TMP_Text label, float current, float max)
    {
        if (label != null)
        {
            label.text = $"{current} / {max}";
        }
    }

    private void HandleStaminaRegen()
    {
        if (!isStunned && playerData != null && currentEnergy < maxEnergy)
        {
            float actualRecoverySpeed = currentRecovery * staminaRegenMultiplier;

            if (isDefending)
            {
                float factorPenalizacion = 1.0f - defenseSlider.value;
                factorPenalizacion = Mathf.Clamp(factorPenalizacion, 0f, 1f);
                actualRecoverySpeed *= factorPenalizacion;
            }

            currentEnergy += actualRecoverySpeed * Time.deltaTime;

            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

            UpdateUI();
        }
    }

    private void HandleStaminaColor()
    {
        if (staminaFillImage == null || playerData == null) return;
        float porcentaje = currentEnergy / maxEnergy;

        if (porcentaje <= 0.25f)
        {
            float parpadeo = Mathf.PingPong(Time.time * 5f, 1f);
            staminaFillImage.color = Color.Lerp(normalStaminaColor, lowStaminaColor, parpadeo);
        }
        else
        {
            staminaFillImage.color = normalStaminaColor;
        }
    }

    void ShowDamagePopup(int damage, bool isCritical)
    {
        if (damagePopupPrefab != null && currentEnemy != null)
        {
            Vector3 spawnPos = currentEnemy.transform.position + new Vector3(0, 1.5f, 0);

            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damage, isCritical);
            }
        }
    }

    void PlayHitParticles()
    {
        if (hitParticles != null && currentEnemy != null)
        {
            hitParticles.transform.position = currentEnemy.transform.position;
            hitParticles.Play();
        }
    }

    IEnumerator EfectoDañoJugador()
    {
        if (playerSpriteRenderer == null) yield break;

        Transform targetTransform = playerSpriteRenderer.transform;
        Vector3 originalPos = targetTransform.position;
        Color originalColor = playerSpriteRenderer.color;

        playerSpriteRenderer.color = damageColor;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            targetTransform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        targetTransform.position = originalPos;

        if (isStunned)
            playerSpriteRenderer.color = Color.gray;
        else
            playerSpriteRenderer.color = Color.white;
    }

    private void DetectarInputSwipe()
    {
        if (isDead || victory || isStunned) return;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                startTouchPosition = touch.position.ReadValue();
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                endTouchPosition = touch.position.ReadValue();
                ProcesarSwipe();
            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                startTouchPosition = Mouse.current.position.ReadValue();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                endTouchPosition = Mouse.current.position.ReadValue();
                ProcesarSwipe();
            }
        }
    }

    private void ProcesarSwipe()
    {
        Vector2 swipeDelta = endTouchPosition - startTouchPosition;

        if (swipeDelta.magnitude < minSwipeDistance) return;

        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            if (swipeDelta.x > 0)
            {
                if (!isDodging && !isStunned && !isDead && !victory)
                    StartCoroutine(RutinaEsquiva(1));
            }
            else
            {
                if (!isDodging && !isStunned && !isDead && !victory)
                    StartCoroutine(RutinaEsquiva(-1));
            }
        }
    }

    IEnumerator RutinaEsquiva(int direccion)
    {
        isDodging = true;
        dodgeDirection = direccion;

        float distancia = 1.5f;
        Vector3 destino = originalPosition + new Vector3(direccion * distancia, 0, 0);

        float timer = 0f;
        float duracionIda = 0.1f;

        Vector3 inicioReal = playerSpriteRenderer.transform.position;

        while (timer < duracionIda)
        {
            playerSpriteRenderer.transform.position = Vector3.Lerp(inicioReal, destino, timer / duracionIda);
            timer += Time.deltaTime;
            yield return null;
        }
        playerSpriteRenderer.transform.position = destino;

        yield return new WaitForSeconds(0.4f);

        timer = 0f;
        float duracionVuelta = 0.1f;

        while (timer < duracionVuelta)
        {
            playerSpriteRenderer.transform.position = Vector3.Lerp(destino, originalPosition, timer / duracionVuelta);
            timer += Time.deltaTime;
            yield return null;
        }

        playerSpriteRenderer.transform.position = originalPosition;

        dodgeDirection = 0;
        isDodging = false;
    }

    public void ReiniciarParaRonda()
    {
        isDead = false;
        victory = false;
        isStunned = false;
        isDefending = false;
        currentLife = maxLife;
        currentEnergy = maxEnergy;

        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, maxLife);
        }
        UpdateUI();

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.transform.position = originalPosition;
            playerSpriteRenderer.color = Color.white;
            playerSpriteRenderer.transform.rotation = Quaternion.identity;
        }

        if (playerAnimator != null) playerAnimator.enabled = false;

        if (currentEnemy != null && enemyHealthBar != null)
        {
            float maxVidaEnemigo = (float)currentEnemy.enemyData.life;
            enemyHealthBar.value = maxVidaEnemigo;
            UpdateHealthText(enemyHealthText, maxVidaEnemigo, maxVidaEnemigo);
        }
    }

    public void CelebrarVictoria()
    {
        if (victoriaConfeti != null)
        {
            victoriaConfeti.gameObject.SetActive(true);
            victoriaTextoUI.gameObject.SetActive(true);
            victoriaConfeti.Play();
        }
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            playerAnimator.SetBool("IsWinner", true);
        }
    }

    IEnumerator AnimacionMuerte()
    {
        isDead = true;
        isDefending = false;
        isStunned = false;

        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.gray;

        float duracionCaida = 1.5f;
        float tiempoPasado = 0f;

        Transform transformDeLaFoto = playerSpriteRenderer.transform;
        Quaternion rotacionInicial = transformDeLaFoto.rotation;
        Quaternion rotacionFinal = Quaternion.Euler(0, 0, -90);

        while (tiempoPasado < duracionCaida)
        {
            transformDeLaFoto.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, tiempoPasado / duracionCaida);
            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        transformDeLaFoto.rotation = rotacionFinal;
        Debug.Log("Animación de muerte terminada.");

        if (roundManager != null)
        {
            roundManager.RegistrarFinDeRonda(false);
        }
    }

    IEnumerator EnterStunState()
    {
        isStunned = true;
        isDefending = false;
        Debug.Log("¡FATIGA! No puedes moverte.");

        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.gray;

        if (stunParticles != null) stunParticles.Play();

        SoundManager.PlaySound(SoundType.Dizzy);

        if (currentEnemy != null)
        {
            currentEnemy.CastigarJugador();
        }

        yield return new WaitForSeconds(2f);

        if (stunParticles != null) stunParticles.Stop();

        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.white;
        currentEnergy = maxEnergy * 0.5f;
        UpdateUI();

        isStunned = false;
    }

    IEnumerator ShowAttackVisuals()
    {
        if (playerSpriteRenderer != null && playerData.AttackSprite != null && isHardAttack == false)
        {
            playerSpriteRenderer.sprite = playerData.AttackSprite;
        }

        if (playerSpriteRenderer != null && playerData.AttackSprite != null && isHardAttack == true)
        {
            isHardAttack = false;
            playerSpriteRenderer.sprite = playerData.HardAttackSprite;
        }

        yield return new WaitForSeconds(0.2f);

        if (!isDefending && !isStunned && playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sprite = playerData.sprite;
        }
    }

    IEnumerator EfectoParryExitoso()
    {
        Color colorOriginal = Color.white;
        Vector3 escalaOriginal = playerSpriteRenderer.transform.localScale;

        playerSpriteRenderer.color = Color.lightGray;
        playerSpriteRenderer.transform.localScale = escalaOriginal * 1.1f;

        yield return new WaitForSeconds(0.15f);

        playerSpriteRenderer.color = colorOriginal;
        playerSpriteRenderer.transform.localScale = escalaOriginal;
    }

    public void Win()
    {
        if (victory || isDead) return;
        victory = true;

        if (roundManager != null) roundManager.RegistrarFinDeRonda(true);
    }

    // función es la que buscarás en el botón
    public void UsarConsumible(Consumible item)
    {
        if (isDead || victory || isStunned || item == null) return;

        if (mochilaConsumibles.Contains(item))
        {
            item.Usar(this);
            SoundManager.PlaySound(SoundType.Consumable);

            mochilaConsumibles.Remove(item);

            Debug.Log($"Objeto usado. Quedan: {mochilaConsumibles.Count}");
        }
        else
        {
            Debug.Log("¡No te quedan existencias de este objeto!");
        }
    }
}