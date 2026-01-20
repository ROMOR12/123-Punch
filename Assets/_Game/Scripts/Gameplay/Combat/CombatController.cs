using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class CombatController : MonoBehaviour
{
    [Header("Datos del Jugador")]
    public BaseCharacter playerData;
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;

    [Header("Sistema de Esquiva")]
    public int dodgeDirection = 0; // 0=Centro, -1=Izquierda, 1=Derecha
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

    [Header("Barras y Textos")]
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
        HandleStaminaRegen();
        if (playerData != null)
        {
            currentEnergy = (float)playerData.energy;
            currentLife = (float)playerData.life;

            if (playerSpriteRenderer != null && playerData.sprite != null)
                playerSpriteRenderer.sprite = playerData.sprite;

            // --- CONFIGURACIÓN JUGADOR ---
            if (playerHealth != null)
            {
                playerName.text = playerData.name;
                playerHealth.maxValue = (float)playerData.life;
                playerHealth.value = currentLife;
                UpdateHealthText(playerHealthText, currentLife, playerData.life);
            }

            // --- CONFIGURACIÓN ENEMIGO ---
            if (enemyHealthBar != null && currentEnemy != null)
            {
                EnemyName.text = currentEnemy.enemyData.name;
                float enemyMax = (float)currentEnemy.enemyData.life;
                enemyHealthBar.maxValue = enemyMax;
                enemyHealthBar.value = enemyMax;
                UpdateHealthText(enemyHealthText, enemyMax, enemyMax);
            }

            if (staminaBar != null)
            {
                staminaBar.maxValue = (float)playerData.energy;
                staminaBar.value = currentEnergy;
            }

            if (playerAnimator != null)
            {
                // Apagamos el Animator para que no moleste a los sprites y animaciones 2D
                playerAnimator.enabled = false;
            }
            // Guardamos la posición original para la esquiva
            if (playerSpriteRenderer != null)
            {
                originalPosition = playerSpriteRenderer.transform.position;
            }
        }
    }

    void Update()
    {
        DetectarInputSwipe();
        HandleDefense();
        HandleStaminaRegen();
        HandleStaminaColor();
    }

    private void UpdateUI()
    {
        if (staminaBar != null) staminaBar.value = currentEnergy;
    }

    public void PerformAttack()
    {

        if (victory || isDefending || isStunned || isDead || isHardAttack) return;

        if (currentEnergy >= ATTACK_COST)
        {
            GastarEnergia(ATTACK_COST);

            int damageDealt = Mathf.RoundToInt(playerData.force * playerDamageMultiplier);

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

            int damageDealt = Mathf.RoundToInt(playerData.force * playerDamageMultiplier * 2f);
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
                float porcentajeBloqueo = defenseSlider.value; // De 0.0 a 1.0

                // calculamos el daño final
                float factorDeDaño = 1.0f - porcentajeBloqueo;
                danioFinal = Mathf.RoundToInt(damageAmount * factorDeDaño);

                // calculamos la estamina gastada
                float gastoEnergia = maxBlockStaminaCost * porcentajeBloqueo;

                GastarEnergia(gastoEnergia);

                Debug.Log($"Bloqueo: {porcentajeBloqueo * 100}% | Daño: {danioFinal} | Estamina gastada: {gastoEnergia}");
            }
        }

        // Aplicar daño
        currentLife -= danioFinal;

        if (currentLife > 0)
        {
            StartCoroutine(EfectoDañoJugador());
        }

        if (currentLife < 0) currentLife = 0;

        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, playerData.life);
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

        // logica para el parry
        if (defenseSlider.value > 0.95f)
        {
            Debug.Log("¡PERFECT PARRY!");

            // Efecto Visual
            StartCoroutine(EfectoParryExitoso());

            // Recuperar Energía
            currentEnergy += 25f;
            if (currentEnergy > playerData.energy)
                currentEnergy = (float)playerData.energy;

            UpdateUI();

            return;
        }
        Debug.Log("¡El ataque rompió tu defensa!");
        currentLife -= damageAmount;
        if (currentLife > 0) StartCoroutine(EfectoDañoJugador());
        if (currentLife < 0) currentLife = 0;

        // Actualizar UI Vida
        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, playerData.life);
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

        // Aplicamos el daño
        currentLife -= damageAmount;

        // Feedback visual
        if (currentLife > 0) StartCoroutine(EfectoDañoJugador());
        if (currentLife < 0) currentLife = 0;

        // Actualizar UI
        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, playerData.life);
        }

        // Muerte
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
        if (!isStunned && playerData != null && currentEnergy < playerData.energy)
        {
            float actualRecoverySpeed = (float)playerData.recovery * staminaRegenMultiplier;

            if (isDefending)
            {
                // Si el slider está al 100%, el factor es 0.0 no regeneras.
                // Si el slider está al 20%, el factor es 0.8 regeneras rápido.
                float factorPenalizacion = 1.0f - defenseSlider.value;

                // Nos aseguramos de que nunca sea negativo
                factorPenalizacion = Mathf.Clamp(factorPenalizacion, 0f, 1f);

                actualRecoverySpeed *= factorPenalizacion;
            }

            currentEnergy += actualRecoverySpeed * Time.deltaTime;

            if (currentEnergy > playerData.energy) currentEnergy = playerData.energy;

            UpdateUI();
        }
    }

    private void HandleStaminaColor()
    {
        if (staminaFillImage == null || playerData == null) return;

        // Calculamos el porcentaje que va de 0 a 10
        float porcentaje = currentEnergy / (float)playerData.energy;

        if (porcentaje <= 0.25f)
        {
            // Mathf.PingPong hace que el valor suba y baje entre 0 y 1 muy rápido
            float parpadeo = Mathf.PingPong(Time.time * 5f, 1f);

            // Lerp mezcla los dos colores basándose en el parpadeo
            staminaFillImage.color = Color.Lerp(normalStaminaColor, lowStaminaColor, parpadeo);
        }
        else
        {
            // color original
            staminaFillImage.color = normalStaminaColor;
        }
    }

    void ShowDamagePopup(int damage, bool isCritical)
    {
        if (damagePopupPrefab != null && currentEnemy != null)
        {
            // instanciar el texto en la posicion del enemigo
            Vector3 spawnPos = currentEnemy.transform.position + new Vector3(0, 1.5f, 0);

            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

            // configurar el nnmero
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

        // guardamos el estado original
        Transform targetTransform = playerSpriteRenderer.transform;
        Vector3 originalPos = targetTransform.position;
        Color originalColor = playerSpriteRenderer.color;

        // sprte en rojo
        playerSpriteRenderer.color = damageColor;

        // vibracion
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // generamos una posición aleatoria muy cerca de la original
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            // movemos la foto
            targetTransform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // volvemos a la posición original
        targetTransform.position = originalPos;

        // si estamos stuneados, volvemos a gris, si no, al color original
        if (isStunned)
            playerSpriteRenderer.color = Color.gray;
        else
            playerSpriteRenderer.color = Color.white;
    }

    private void DetectarInputSwipe()
    {
        if (isDead || victory || isStunned) return;

        // logica para dispositivos tactiles
        if (Touchscreen.current != null)
        {
            // obtenemos el primer toque
            var touch = Touchscreen.current.primaryTouch;

            // al tocar 
            if (touch.press.wasPressedThisFrame)
            {
                startTouchPosition = touch.position.ReadValue();
            }
            // al soltar
            else if (touch.press.wasReleasedThisFrame)
            {
                endTouchPosition = touch.position.ReadValue();
                ProcesarSwipe();
            }
        }
        // logica para el raton, solo utilizado en el testing
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

        // si el movimiento es muy corto, no hacemos nada
        if (swipeDelta.magnitude < minSwipeDistance) return;

        // comprobar si es horizontal
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            // Si swipeDelta.x es positivo va a la derecha
            // Si swipeDelta.x es negativo va a la izquierda
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

        // calculamos el destino basándonos en el centro, no en donde esté el muñeco ahora
        float distancia = 1.5f;

        // destino final de la esquiva
        Vector3 destino = originalPosition + new Vector3(direccion * distancia, 0, 0);

        // ida rapida
        float timer = 0f;
        float duracionIda = 0.1f;

        // guardamos la posición actual real
        Vector3 inicioReal = playerSpriteRenderer.transform.position;

        while (timer < duracionIda)
        {
            playerSpriteRenderer.transform.position = Vector3.Lerp(inicioReal, destino, timer / duracionIda);
            timer += Time.deltaTime;
            yield return null;
        }
        // forzamos la posición al destino exacto para evitar errores
        playerSpriteRenderer.transform.position = destino;

        // mantenemos la posición un momento para dar mejor efecto
        yield return new WaitForSeconds(0.4f);

        // vuelta al centro
        timer = 0f;
        float duracionVuelta = 0.1f;

        while (timer < duracionVuelta)
        {
            // volvemos desde donde estamos hasta la posicion inicial guardada en start
            playerSpriteRenderer.transform.position = Vector3.Lerp(destino, originalPosition, timer / duracionVuelta);
            timer += Time.deltaTime;
            yield return null;
        }

        // forzamos la posición al centro exacto para evitar errores
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

        currentLife = (float)playerData.life;
        currentEnergy = (float)playerData.energy;

        if (playerHealth != null)
        {
            playerHealth.value = currentLife;
            UpdateHealthText(playerHealthText, currentLife, playerData.life);
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

        // Feedback visual
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.gray;

        float duracionCaida = 1.5f;
        float tiempoPasado = 0f;

        // Buscamos el Transform de la foto
        Transform transformDeLaFoto = playerSpriteRenderer.transform;

        Quaternion rotacionInicial = transformDeLaFoto.rotation;

        // Giramos -90 grados
        Quaternion rotacionFinal = Quaternion.Euler(0, 0, -90);

        while (tiempoPasado < duracionCaida)
        {
            // Giramos la foto poco a poco
            transformDeLaFoto.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, tiempoPasado / duracionCaida);

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        // Aseguramos la posición final
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

        if (currentEnemy != null)
        {
            currentEnemy.CastigarJugador();
        }

        yield return new WaitForSeconds(2f);

        if (stunParticles != null) stunParticles.Stop();

        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.white;

        currentEnergy = (float)playerData.energy * 0.5f;
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
        // Guardamos estado original
        Color colorOriginal = Color.white; 

        Vector3 escalaOriginal = playerSpriteRenderer.transform.localScale;

        // efecto
        playerSpriteRenderer.color = Color.lightGray;

        // Hacemos el efecto pop un poco más grande
        playerSpriteRenderer.transform.localScale = escalaOriginal * 1.1f;

        yield return new WaitForSeconds(0.15f);

        // Volvemos a la normalidad
        playerSpriteRenderer.color = colorOriginal;
        playerSpriteRenderer.transform.localScale = escalaOriginal;
    }

    public void Win()
    {
        if (victory || isDead) return;
        victory = true;

        if (roundManager != null) roundManager.RegistrarFinDeRonda(true);
    }
}