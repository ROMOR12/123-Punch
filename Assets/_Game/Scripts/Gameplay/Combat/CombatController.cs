using UnityEngine;
using UnityEngine.UI;

public class CombatController : MonoBehaviour
{
    [Header("Datos del Jugador")]
    public BaseCharacter playerData; // <-- Arrastra aquí tu ScriptableObject de Personaje

    [Header("Referencias de Escena")]
    public EnemyBot currentEnemy;
    public Slider defenseSlider; // Slider Vertical Izquierdo
    public Slider staminaBar;    // Barra de energía

    // Variables de lógica interna
    private float currentEnergy;
    private bool isDefending = false;
    private const float ATTACK_COST = 0.5f; // Coste fijo por golpe (puedes moverlo al SO si quieres)

    void Start()
    {
        if (playerData != null)
        {
            // Inicializamos la energía con el valor de tu ScriptableObject
            currentEnergy = (float)playerData.energy;

            // Configuramos el máximo del slider visual
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

    // --- Lógica de Ataque (Conectada a los botones derechos) ---
    public void PerformAttack()
    {
        // Regla 1: No atacar si defiendes
        if (isDefending) return;

        // Regla 2: Tener energía suficiente
        if (currentEnergy >= ATTACK_COST)
        {
            currentEnergy -= ATTACK_COST;
            UpdateUI();

            // Calculamos el daño usando la FUERZA de tu personaje
            int damageDealt = playerData.force;

            if (currentEnemy != null)
            {
                currentEnemy.TakeDamage(damageDealt);
            }
        }
        else
        {
            Debug.Log("¡Sin energía!");
        }
    }

    // --- Lógica de Defensa (Slider Izquierdo) ---
    private void HandleDefense()
    {
        if (defenseSlider == null) return;

        // Si el slider sube más del 10%, consideramos que está defendiendo
        if (defenseSlider.value > 0.1f)
        {
            isDefending = true;
        }
        else
        {
            isDefending = false;
        }
    }

    // --- Regeneración de Energía ---
    private void HandleStaminaRegen()
    {
        // Solo regenera si NO defiende y no está llena
        if (!isDefending && playerData != null && currentEnergy < playerData.energy)
        {
            // Usamos el dato 'recovery' de tu personaje para la velocidad de recarga
            currentEnergy += (float)playerData.recovery * Time.deltaTime;

            // Tope máximo
            if (currentEnergy > playerData.energy) currentEnergy = playerData.energy;

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (staminaBar != null) staminaBar.value = currentEnergy;
    }
}