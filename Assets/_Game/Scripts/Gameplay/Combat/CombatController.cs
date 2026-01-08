using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CombatController : MonoBehaviour
{
    [Header("Datos del Jugador")]
    public BaseCharacter playerData;
    public SpriteRenderer playerSpriteRenderer;

    [Header("Referencias de Escena")]
    public EnemyBot currentEnemy;
    public Slider defenseSlider;
    public Slider staminaBar;
    public Slider playerHealth;

    // Variables de lógica interna
    private float currentEnergy;
    private float currentLife;      // Variable para la vida
    private bool isDefending = false;
    private bool isStunned = false; // ¿Está mareado?
    private const float ATTACK_COST = 15f;

    void Start()
    {
        if (playerData != null)
        {
            currentEnergy = (float)playerData.energy;
            currentLife = (float)playerData.life;

            if (playerSpriteRenderer != null && playerData.sprite != null)
                playerSpriteRenderer.sprite = playerData.sprite;

            if (playerHealth != null)
            {
                playerHealth.maxValue = (float)playerData.life;
                playerHealth.value = currentLife;
            }

            if (staminaBar != null)
            {
                staminaBar.maxValue = (float)playerData.energy;
                staminaBar.value = currentEnergy;
            }
        }
    }

    void Update()
    {
        HandleDefense();
        HandleStaminaRegen();
    }

    // --- NUEVA FUNCIÓN MAESTRA PARA GASTAR ENERGÍA ---
    // Usaremos esto para atacar Y para defender. Así siempre chequea el Stun.
    private void GastarEnergia(float cantidad)
    {
        currentEnergy -= cantidad;

        // Si bajamos de 0, nos estuneamos
        if (currentEnergy <= 0)
        {
            currentEnergy = 0; // No dejar que sea negativo
            if (!isStunned) // Solo activar si no estaba ya mareado
            {
                StartCoroutine(EnterStunState());
            }
        }
        UpdateUI();
    }

    public void PerformAttack()
    {
        // Si defiendes O estás mareado, no atacas
        if (isDefending || isStunned) return;

        if (currentEnergy >= ATTACK_COST)
        {
            // Usamos la nueva función
            GastarEnergia(ATTACK_COST);

            // Daño y visuales
            int damageDealt = playerData.force;
            StartCoroutine(ShowAttackVisuals());

            if (currentEnemy != null)
                currentEnemy.TakeDamage(damageDealt);
        }
        else
        {
            Debug.Log("¡Sin energía!");
        }
    }

    public void HardAttack()
    {
        if (isDefending || isStunned) return;

        float hardAttackCost = ATTACK_COST * 2f;

        if (currentEnergy >= hardAttackCost)
        {
            GastarEnergia(hardAttackCost);

            int damageDealt = Mathf.RoundToInt(playerData.force * 2f);
            StartCoroutine(ShowAttackVisuals()); // Usamos la misma animacion o una nueva si tienes

            if (currentEnemy != null)
                currentEnemy.TakeDamage(damageDealt);
        }
        else
        {
            Debug.Log("¡Sin energía para ataque fuerte!");
        }
    }

    public void ReceiveDamage(int damageAmount)
    {
        // 1. Si bloqueamos
        if (isDefending)
        {
            if (!isStunned) // Solo bloqueas si no estás mareado
            {
                Debug.Log("¡Bloqueado!");
                // AQUÍ ESTABA EL ERROR: Ahora usamos GastarEnergia para que chequee el Stun
                GastarEnergia(15f);
                return;
            }
            // Si estás estuneado, el bloqueo falla y recibes daño abajo...
        }

        // 2. Si recibimos daño directo
        currentLife -= damageAmount;
        if (playerHealth != null) playerHealth.value = currentLife;

        if (currentLife <= 0)
        {
            Debug.Log("¡K.O.! Has perdido.");
        }
    }

    // --- RUTINA DE FATIGA ---
    IEnumerator EnterStunState()
    {
        isStunned = true;
        isDefending = false; // Te baja la guardia a la fuerza
        Debug.Log("¡FATIGA! No puedes moverte.");

        // Feedback Visual: Te pones ROJO o GRIS
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.gray;

        // Esperamos 2 segundos
        yield return new WaitForSeconds(2f);

        // Recuperación
        if (playerSpriteRenderer != null) playerSpriteRenderer.color = Color.white;

        // Recuperas un poco de aliento
        currentEnergy = (float)playerData.energy * 0.5f;
        UpdateUI();

        isStunned = false;
    }

    IEnumerator ShowAttackVisuals()
    {
        // NOTA: Asegúrate de que en BaseCharacter.cs se llame 'spriteAttack'
        if (playerSpriteRenderer != null && playerData.AttackSprite != null)
        {
            playerSpriteRenderer.sprite = playerData.AttackSprite;
        }

        yield return new WaitForSeconds(0.2f);

        // Solo volvemos a la pose normal si no estamos mareados ni defendiendo
        if (!isDefending && !isStunned && playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sprite = playerData.sprite;
        }
    }

    private void HandleDefense()
    {
        // Si estás estuneado, el código corta aquí y NO te deja defender
        if (isStunned) return;

        if (defenseSlider == null) return;

        bool wasDefending = isDefending;

        if (defenseSlider.value > 0.1f)
            isDefending = true;
        else
            isDefending = false;

        // Cambio visual de escudo
        if (isDefending != wasDefending)
        {
            if (isDefending)
            {
                // NOTA: Asegúrate de que en BaseCharacter.cs se llame 'spriteDefend'
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
        // Solo regenera si NO estás estuneado
        if (!isStunned && playerData != null && currentEnergy < playerData.energy)
        {
            float actualRecoverySpeed = (float)playerData.recovery;

            if (isDefending) actualRecoverySpeed *= 0.3f;

            currentEnergy += actualRecoverySpeed * Time.deltaTime;

            if (currentEnergy > playerData.energy) currentEnergy = playerData.energy;

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (staminaBar != null) staminaBar.value = currentEnergy;
    }
}