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
                playerHealth.maxValue = (float)playerData.life;
                playerHealth.value = currentLife;
                UpdateHealthText(playerHealthText, currentLife, playerData.life);
            }

            // --- CONFIGURACIÓN ENEMIGO ---
            if (enemyHealthBar != null && currentEnemy != null)
            {
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
                // Apagamos el Animator para que NO moleste a tus ataques manuales
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

    void UpdateHealthText(TMP_Text label, float current, float max)
    {
        if (label != null)
        {
            label.text = $"{current} / {max}";
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

    public void PerformAttack()
    {
        if (victory || isDefending || isStunned || isDead || isHardAttack) return;

        if (currentEnergy >= ATTACK_COST)
        {
            GastarEnergia(ATTACK_COST);

            // <--- CORREGIDO: APLICAMOS EL MULTIPLICADOR (Fuerza * 5)
            int damageDealt = Mathf.RoundToInt(playerData.force * playerDamageMultiplier);

            StartCoroutine(ShowAttackVisuals());

            if (currentEnemy != null)
            {
                currentEnemy.TakeDamage(damageDealt);
                ShowDamagePopup(damageDealt, false);

                if (enemyHealthBar != null)
                {
                    enemyHealthBar.value -= damageDealt;
                    UpdateHealthText(enemyHealthText, enemyHealthBar.value, enemyHealthBar.maxValue);
                    PlayHitParticles();
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

            // <--- CORREGIDO: APLICAMOS MULTIPLICADOR * 2 (Fuerza * 5 * 2)
            int damageDealt = Mathf.RoundToInt(playerData.force * playerDamageMultiplier * 2f);
            ShowDamagePopup(damageDealt, false);

            isHardAttack = true;
            StartCoroutine(ShowAttackVisuals());

            if (currentEnemy != null)
            {
                currentEnemy.TakeDamage(damageDealt);

                if (enemyHealthBar != null)
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

    void PlayHitParticles()
    {
        if (hitParticles != null && currentEnemy != null)
        {
            // 1. Mover el efecto a la posición del enemigo
            hitParticles.transform.position = currentEnemy.transform.position;
            // 2. ¡Acción!
            hitParticles.Play();
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

                // 1. CALCULAR DAÑO
                float factorDeDaño = 1.0f - porcentajeBloqueo;
                danioFinal = Mathf.RoundToInt(damageAmount * factorDeDaño);

                // 2. CALCULAR GASTO DE ESTAMINA (Proporcional al bloqueo)
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
        if (isDead) return;

        // Margen de error (0.95)
        if (defenseSlider.value > 0.95f)
        {
            Debug.Log("¡PERFECT PARRY! Energía recuperada.");

            // 1. Sumamos energía
            currentEnergy += 25f;

            // 2. CLAMP (Tope): Importante para que no supere el máximo y haga cosas raras
            if (currentEnergy > playerData.energy)
                currentEnergy = (float)playerData.energy;

            // 3. Actualizamos la barra visualmente YA
            UpdateUI();

            return; // No recibes daño.
        }

        Debug.Log("¡El ataque rompió tu defensa!");

        // Daño directo
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

    public void recuperarEnergia(int cantidad) 
    {
        currentEnergy += cantidad;
        UpdateUI();
    }

    IEnumerator EfectoDañoJugador()
    {
        // 1. CHEQUEOS DE SEGURIDAD
        if (playerSpriteRenderer == null) yield break;

        // Guardamos el estado original
        Transform targetTransform = playerSpriteRenderer.transform;
        Vector3 originalPos = targetTransform.position;
        Color originalColor = playerSpriteRenderer.color;

        // 2. sprte en rojo
        playerSpriteRenderer.color = damageColor;

        // 3. VIBRACIÓN
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            // Generamos una posición aleatoria muy cerca de la original
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            // Movemos la FOTO, no el script
            targetTransform.position = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. RESTAURAR TODO
        targetTransform.position = originalPos;

        // Si estamos stuneados, volvemos a gris, si no, al color que tuviera antes
        if (isStunned)
            playerSpriteRenderer.color = Color.gray;
        else
            playerSpriteRenderer.color = Color.white; // O 'originalColor' si prefieres
    }

    IEnumerator AnimacionMuerte()
    {
        isDead = true;
        isDefending = false;
        isStunned = false;

        // Feedback visual: Gris
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.gray;

        float duracionCaida = 1.5f;
        float tiempoPasado = 0f;

        // --- EL CAMBIO ESTÁ AQUÍ ---
        // 1. Buscamos el Transform DE LA FOTO, no del script
        Transform transformDeLaFoto = playerSpriteRenderer.transform;
        // ---------------------------

        Quaternion rotacionInicial = transformDeLaFoto.rotation;

        // Giramos -90 grados (tumbado hacia atrás). 
        // Si cae hacia el otro lado, cambia -90 por 90.
        Quaternion rotacionFinal = Quaternion.Euler(0, 0, -90);

        while (tiempoPasado < duracionCaida)
        {
            // Giramos la FOTO
            transformDeLaFoto.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, tiempoPasado / duracionCaida);

            tiempoPasado += Time.deltaTime;
            yield return null;
        }

        // Aseguramos la posición final
        transformDeLaFoto.rotation = rotacionFinal;
        Debug.Log("Animación de muerte terminada.");
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

        // Ahora sí esperamos sufriendo
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

    private void HandleStaminaRegen()
    {
        if (!isStunned && playerData != null && currentEnergy < playerData.energy)
        {
            float actualRecoverySpeed = (float)playerData.recovery * staminaRegenMultiplier;

            // LÓGICA DINÁMICA:
            if (isDefending)
            {
                // Si el slider está al 1.0 (100%), el factor es 0.0 -> No regeneras.
                // Si el slider está al 0.2 (20%), el factor es 0.8 -> Regeneras rápido.
                float factorPenalizacion = 1.0f - defenseSlider.value;

                // Nos aseguramos de que nunca sea negativo (por si acaso)
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

        // 1. Calculamos el porcentaje (0.0 a 1.0)
        float porcentaje = currentEnergy / (float)playerData.energy;

        // 2. Si es menor al 25% (0.25)
        if (porcentaje <= 0.25f)
        {
            // --- PARPADEO ---
            // Mathf.PingPong hace que el valor suba y baje entre 0 y 1 muy rápido
            float parpadeo = Mathf.PingPong(Time.time * 5f, 1f);

            // Lerp mezcla los dos colores basándose en el parpadeo
            staminaFillImage.color = Color.Lerp(normalStaminaColor, lowStaminaColor, parpadeo);
        }
        else
        {
            // 3. ESTADO NORMAL
            staminaFillImage.color = normalStaminaColor;
        }
    }

    void ShowDamagePopup(int damage, bool isCritical)
    {
        if (damagePopupPrefab != null && currentEnemy != null)
        {
            // Instanciar el texto en la posición del enemigo (un poco más arriba)
            Vector3 spawnPos = currentEnemy.transform.position + new Vector3(0, 1.5f, 0); // Ajusta la altura (1.5f)

            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);

            // Configurar el número
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(damage, isCritical);
            }
        }
    }

    public void Win()
    {
        if (isDead || victory) return;

        victory = true;
        isDefending = false; // Bajamos guardia

        Debug.Log("¡VICTORIA!");

        // 1. Activar Animación
        if (playerAnimator != null)
        {
            // --- AÑADE ESTA LÍNEA ---
            playerAnimator.enabled = true;

            playerAnimator.SetBool("IsWinner", true);
        }

        // 1. Activar Animación
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsWinner", true);
        }

        // 2. Activar Confeti
        if (victoriaConfeti != null)
        {
            victoriaConfeti.gameObject.SetActive(true);
            victoriaConfeti.Play();
        }

        // 3. Activar Texto
        if (victoriaTextoUI != null)
        {
            victoriaTextoUI.SetActive(true);
        }

        // Opcional: Recuperar energía visualmente
        currentEnergy = (float)playerData.energy;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (staminaBar != null) staminaBar.value = currentEnergy;
    }

    private void DetectarInputSwipe()
    {
        if (isDead || victory || isStunned) return;

        // --- LÓGICA PARA MÓVIL (TOUCH) - VERSIÓN BLINDADA ---
        if (Touchscreen.current != null)
        {
            // Usamos 'primaryTouch' en vez de 'touches[0]' porque es más seguro
            var touch = Touchscreen.current.primaryTouch;

            // 1. Al tocar (Press)
            if (touch.press.wasPressedThisFrame)
            {
                startTouchPosition = touch.position.ReadValue();
            }
            // 2. Al soltar (Release) - Esto solo ocurre 1 vez, imposible que haga bucle
            else if (touch.press.wasReleasedThisFrame)
            {
                endTouchPosition = touch.position.ReadValue();
                ProcesarSwipe();
            }
        }
        // --- LÓGICA PARA PC (RATÓN) ---
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

        // Si el movimiento es muy corto, no hacemos nada
        if (swipeDelta.magnitude < minSwipeDistance) return;

        // Comprobar si es horizontal
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            // Si swipeDelta.x es positivo -> Derecha (1)
            // Si swipeDelta.x es negativo -> Izquierda (-1)
            if (swipeDelta.x > 0)
            {
                // DERECHA
                if (!isDodging && !isStunned && !isDead && !victory)
                    StartCoroutine(RutinaEsquiva(1));
            }
            else
            {
                // IZQUIERDA
                if (!isDodging && !isStunned && !isDead && !victory)
                    StartCoroutine(RutinaEsquiva(-1));
            }
        }
    }
    IEnumerator RutinaEsquiva(int direccion)
    {
        isDodging = true;
        dodgeDirection = direccion;

        // 1. Calculamos el destino basándonos en el CENTRO FIJO, no en donde esté el muñeco ahora
        float distancia = 1.5f;

        // Destino = El centro guardado + el desplazamiento
        Vector3 destino = originalPosition + new Vector3(direccion * distancia, 0, 0);

        // 2. Ida rápida
        float timer = 0f;
        float duracionIda = 0.1f;

        // Guardamos desde dónde sale (por si acaso no estaba en el centro perfecto al empezar la animación)
        Vector3 inicioReal = playerSpriteRenderer.transform.position;

        while (timer < duracionIda)
        {
            playerSpriteRenderer.transform.position = Vector3.Lerp(inicioReal, destino, timer / duracionIda);
            timer += Time.deltaTime;
            yield return null;
        }
        playerSpriteRenderer.transform.position = destino; // Aseguramos que llega al sitio exacto

        // 3. Esperar (Mantener la esquiva)
        yield return new WaitForSeconds(0.4f);

        // 4. Vuelta al CENTRO FIJO
        timer = 0f;
        float duracionVuelta = 0.1f;

        while (timer < duracionVuelta)
        {
            // Volvemos desde donde estamos hasta la POSICIÓN INICIAL guardada en Start
            playerSpriteRenderer.transform.position = Vector3.Lerp(destino, originalPosition, timer / duracionVuelta);
            timer += Time.deltaTime;
            yield return null;
        }

        // 5. ¡Paso clave! Forzamos la posición al centro exacto para evitar errores decimales
        playerSpriteRenderer.transform.position = originalPosition;

        dodgeDirection = 0;
        isDodging = false;
    }
}