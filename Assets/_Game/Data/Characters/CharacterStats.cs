using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("Referencia a tus Datos")]
    public EntityBase datosBase;

    [Header("Estado Actual (Se llena solo al play)")]
    public int currentLife;
    public int maxLife;

    public int currentEnergy;
    public int maxEnergy;

    public int currentForce;
    public int currentRecovery;

    private void Awake()
    {
        // Al empezar el juego, COPIAMOS los datos de tu ficha
        // Así, si le pegan, baja 'currentLife' pero NO se modifica el archivo
        maxLife = datosBase.life;
        maxEnergy = datosBase.energy;
        currentForce = datosBase.force;
        currentRecovery = datosBase.recovery;

        // Llenamos la vida y energía a tope
        currentLife = maxLife;
        currentEnergy = maxEnergy;
    }

    //Funcion para el combate
    public void RecibirDaño(int cantidad)
    {
        currentLife -= cantidad;
        if (currentLife <= 0)
        {
            currentLife = 0;
            Debug.Log($"{gameObject.name} ha sido NOQUEADO!");
        }
    }

    public void AplicarModificador(StatType tipo, int cantidad, bool esRecuperacion)
    {
        switch (tipo)
        {
            case StatType.Life:
                if (esRecuperacion)
                {
                    currentLife += cantidad;
                    // Que no supere el máximo
                    if (currentLife > maxLife) currentLife = maxLife;
                }
                else // Es un pasivo que aumenta el TOPE de vida
                {
                    maxLife += cantidad;
                    currentLife += cantidad; // Subimos también la actual para que se note
                }
                break;

            case StatType.Energy:
                if (esRecuperacion)
                {
                    currentEnergy += cantidad;
                    if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
                }
                else
                {
                    maxEnergy += cantidad;
                    currentEnergy += cantidad;
                }
                break;

            case StatType.Force:
                currentForce += cantidad;
                break;

            case StatType.Recovery:
                currentRecovery += cantidad;
                break;
        }
    }
}
