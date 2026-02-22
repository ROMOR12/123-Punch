using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    public List<Consumible> mochilaConsumibles = new List<Consumible>();
    public Image imagenBotonConsumible;
    public GameObject FlechaConsumible;

    [Header("Estadísticas de Combate")]
    public ResultScreenUI pantallaResultados;
    public int contadorGolpes = 0;
    public int contadorDañoTotal = 0;
    public float tiempoInicioCombate;

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
        mochilaConsumibles = new List<Consumible>();

        if (playerData != null)
        {
            // Cargar consumibles solo si existen
            if (playerData.objetosConsumibles != null)
            {
                // Filtramos para no añadir objetos vacios
                foreach (var item in playerData.objetosConsumibles)
                {
                    if (item != null) mochilaConsumibles.Add(item);
                }
            }

            // Cargamos los datos basicos
            maxLife = (float)playerData.life;
            maxEnergy = (float)playerData.energy;
            currentForce = (float)playerData.force;
            currentRecovery = (float)playerData.recovery;

            // Cargar Pasivos
            if (playerData.objetosPasivos != null)
            {
                foreach (var item in playerData.objetosPasivos)
                    if (item != null) item.Equipar(this);
            }

            foreach (var pasivo in pasivosEquipados)
                if (pasivo != null) pasivo.Equipar(this);

            currentLife = maxLife;
            currentEnergy = maxEnergy;

            if (playerSpriteRenderer != null && playerData.sprite != null)
                playerSpriteRenderer.sprite = playerData.sprite;

            if (playerSpriteRenderer != null)
                originalPosition = playerSpriteRenderer.transform.position;

            if (playerAnimator != null) playerAnimator.enabled = false;

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

        // UI Enemigo
        if (enemyHealthBar != null && currentEnemy != null)
        {
            if (EnemyName != null) EnemyName.text = currentEnemy.enemyData.name;
            float enemyMax = (float)currentEnemy.enemyData.life;
            enemyHealthBar.maxValue = enemyMax;
            enemyHealthBar.value = enemyMax;
            UpdateHealthText(enemyHealthText, enemyMax, enemyMax);
        }

        tiempoInicioCombate = Time.time;
        contadorGolpes = 0;
        contadorDañoTotal = 0;

        if(pantallaResultados != null)
        {
            pantallaResultados.ventanaResultados.SetActive(false);
        }

        HandleStaminaRegen();

        ActualizarInterfazObjeto();
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

    // Atacar
    public void PerformAttack()
    {
        // si se cumplen estas condiciones no hacemos nada
        if (victory || isDefending || isStunned || isDead || isHardAttack) return;

        // Si tenemos energia suficiente
        if (currentEnergy >= ATTACK_COST)
        {
            // gastamos la energia
            GastarEnergia(ATTACK_COST);
            SoundManager.PlaySound(SoundType.HIT);
            // Calculamos daño
            int damageDealt = Mathf.RoundToInt(currentForce * playerDamageMultiplier);

            // animacion
            StartCoroutine(ShowAttackVisuals());

            if (currentEnemy != null)
            {
                bool golpeo = currentEnemy.TakeDamage(damageDealt);

                if (golpeo)
                {
                    ShowDamagePopup(damageDealt, false);
                    if (enemyHealthBar != null)
                    {
                        //va cogiendo el daño que le haces al enemigo
                        RegistrarEstadistica(damageDealt);

                        // restamos vida al enemigo
                        enemyHealthBar.value -= damageDealt;
                        UpdateHealthText(enemyHealthText, enemyHealthBar.value, enemyHealthBar.maxValue);
                        PlayHitParticles();
                    }
                }
            }
        }
    }

    // Golpe fuerte
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
                    //va cogiendo el daño que le haces al enemigo
                    RegistrarEstadistica(damageDealt);

                    enemyHealthBar.value -= damageDealt;
                    UpdateHealthText(enemyHealthText, enemyHealthBar.value, enemyHealthBar.maxValue);
                    PlayHitParticles();
                }
            }
        }
    }

    // gastar enegia
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

    // Defensa
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

    // Recivir daño
    public void ReceiveDamage(int damageAmount)
    {
        if (isDead) return;

        int danioFinal = damageAmount;

        if (isDefending)
        {
            if (!isStunned)
            {
                // Recogemos el porcentaje de defensa
                float porcentajeBloqueo = defenseSlider.value;

                float factorDeDaño = 1.0f - porcentajeBloqueo;
                danioFinal = Mathf.RoundToInt(damageAmount * factorDeDaño);

                float gastoEnergia = maxBlockStaminaCost * porcentajeBloqueo;

                GastarEnergia(gastoEnergia);

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
            StartCoroutine(AnimacionMuerte());
        }
    }

    // Recibir daño de ataque fuerte
    public void ReceiveUnstoppableDamage(int damageAmount)
    {
        if (isDead || victory) return;

        if (defenseSlider.value > 0.95f)
        {

            StartCoroutine(EfectoParryExitoso());
            SoundManager.PlaySound(SoundType.GuardStrike, 0.1f);

            currentEnergy += 25f;

            if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

            UpdateUI();
            return;
        }

        currentLife -= damageAmount;

        float porcentajeBloqueo = defenseSlider.value;
        float gastoEnergia = maxBlockStaminaCost * porcentajeBloqueo;

        GastarEnergia(gastoEnergia);

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
            StartCoroutine(AnimacionMuerte());
        }
    }

    // Recibir daño verdadero
    public void ReceiveTrueDamage(int damageAmount)
    {
        if (isDead || victory) return;


        currentLife -= damageAmount;
        if (defenseSlider != null)
        {
            float porcentajeBloqueo = defenseSlider.value;
            float gastoEnergia = maxBlockStaminaCost * porcentajeBloqueo;
            GastarEnergia(gastoEnergia);
        }

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

    // actualizar teto de vida
    void UpdateHealthText(TMP_Text label, float current, float max)
    {
        if (label != null)
        {
            label.text = $"{current} / {max}";
        }
    }

    // regeneracion de energia
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

    // Color de energia baja
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

    // Animacion de puntos de daño
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

    // Particulas de golpes
    void PlayHitParticles()
    {
        if (hitParticles != null && currentEnemy != null)
        {
            hitParticles.transform.position = currentEnemy.transform.position;
            hitParticles.Play();
        }
    }

    // Efecto de cuando el jugador recibe daño
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

    // Deteccion del deslice para esquivar
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

    // Procesamos el deslice
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
                    SoundManager.PlaySound(SoundType.Dodge);
            }
            else
            {
                if (!isDodging && !isStunned && !isDead && !victory)
                    StartCoroutine(RutinaEsquiva(-1));
                    SoundManager.PlaySound(SoundType.Dodge);
            }
        }
    }

    // Corutina para esquivar
    IEnumerator RutinaEsquiva(int direccion)
    {
        isDodging = true;
        dodgeDirection = direccion;

        if (this == null || playerSpriteRenderer == null) yield break;

        float distancia = 1.5f;
        Vector3 destino = originalPosition + new Vector3(direccion * distancia, 0, 0);

        float timer = 0f;
        float duracionIda = 0.1f;

        if (playerSpriteRenderer == null) yield break;
        Vector3 inicioReal = playerSpriteRenderer.transform.position;

        while (timer < duracionIda)
        {
            if (this == null || playerSpriteRenderer == null) yield break;

            playerSpriteRenderer.transform.position = Vector3.Lerp(inicioReal, destino, timer / duracionIda);
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerSpriteRenderer != null)
            playerSpriteRenderer.transform.position = destino;

        yield return new WaitForSeconds(0.4f);

        if (this == null || playerSpriteRenderer == null) yield break;

        timer = 0f;
        float duracionVuelta = 0.1f;

        while (timer < duracionVuelta)
        {
            if (this == null || playerSpriteRenderer == null) yield break;

            playerSpriteRenderer.transform.position = Vector3.Lerp(destino, originalPosition, timer / duracionVuelta);
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerSpriteRenderer != null)
            playerSpriteRenderer.transform.position = originalPosition;

        dodgeDirection = 0;
        isDodging = false;
    }

    // Reinicia las datos por cada ronda
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

    // Animacion de muerte del jugador
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

        if (roundManager != null)
        {
            roundManager.RegistrarFinDeRonda(false);
        }
    }

    // Rutina para entrar en el estado de aturdido
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

    // Rutina para cambiar el srpite de atacar
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

    // Efecto de cuando bloqueamos un ataque fuerte
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

    // metodo cuando ganas un combate
    public void Win()
    {
        if (victory || isDead) return;
        victory = true;
        SoundManager.StopMusic();

        if (roundManager != null) roundManager.RegistrarFinDeRonda(true);
    }

    // Metodo para usar items
    public void UsarConsumible(Consumible item)
    {
        // si estas en algun mal estado no hace nada
        if (isDead || victory || isStunned || item == null) return;

        // Si la lsita tiene el item
        if (mochilaConsumibles.Contains(item))
        {
            // usamos el item
            item.Usar(this);
            SoundManager.PlaySound(SoundType.Consumable);

            // quitamos el item
            mochilaConsumibles.Remove(item);
        }
    }

    //Boton dinamico
    void ActualizarInterfazObjeto()
    {
        // Comprobar que el consumible tenga icono
        if (imagenBotonConsumible == null) return;

        // Comprobamos si hay objetos
        if (mochilaConsumibles != null && mochilaConsumibles.Count > 0)
        {
            // Cogemos el primero
            Consumible primerItem = mochilaConsumibles[0];

            //  Comprobamos que el item no sea null antes de pedir su icono
            if (primerItem != null && primerItem.icon != null)
            {
                imagenBotonConsumible.sprite = primerItem.icon;
                imagenBotonConsumible.enabled = true;
            }
            else
            {
                // Si el item está vacío, ocultamos la imagen
                imagenBotonConsumible.enabled = false;
            }
        }
        else
        {
            // Si la mochila está vacía, ocultamos la imagen
            imagenBotonConsumible.enabled = false;
        }

        // Flecha
        if (FlechaConsumible != null)
        {
            // Solo activar si hay más de 1 item y la lista no es nula
            bool activarFlecha = (mochilaConsumibles != null && mochilaConsumibles.Count > 1);
            FlechaConsumible.SetActive(activarFlecha);
        }
    }

    public void UsarPrimerConsumible()
    {
        if (isDead || victory || isStunned) return;

        if (mochilaConsumibles.Count > 0)
        {
            // Cogemos el primer item de la lista
            Consumible item = mochilaConsumibles[0];

            // Usamos el item
            item.Usar(this);
            SoundManager.PlaySound(SoundType.Consumable);

            // Borramos el item de la lista
            mochilaConsumibles.RemoveAt(0);

            //Actualizamos la interfaz para mosntrar el siguiente item
            ActualizarInterfazObjeto();
        }
    }

    // Pasar el siguiente item
    public void SiguienteConsumible()
    {
        // Si la lista tiene 1 item o menos no dejamos cambiar
        if (mochilaConsumibles.Count <= 1) return;

        // Guardamos el primer objeto, osea el actual en una variable temporal
        Consumible objetoActual = mochilaConsumibles[0];

        // Borramos el item seleccionado
        mochilaConsumibles.RemoveAt(0);

        // Añadimos el item anterioremnte guardado en la lista
        mochilaConsumibles.Add(objetoActual);

        // Actualizamos la interfaz para que muestre visualmente el siguiente item
        ActualizarInterfazObjeto();
    }
    
    // cada vez que haces daño esto se va incrementando
    void RegistrarEstadistica(int damage)
    {
        contadorGolpes++;
        contadorDañoTotal += damage;
    }

    // Metodos de seguridad para que cuando se haya termiando el combate no quere nada residual
    private void OnDisable()
    {
        DetenerCombate();
    }

    private void OnDestroy()
    {
        DetenerCombate();
    }
    public void DetenerCombate()
    {
        StopAllCoroutines();

        this.enabled = false;
    }
}