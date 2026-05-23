using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class MejoraStatsManager : MonoBehaviour
{
    private UsuarioService usuarioService = new UsuarioService();

    [Header("Configuración de Costes Base")]
    public int costeBaseVida = 50;
    public int costeBaseEnergia = 50;
    public int costeBaseFuerza = 100;
    public int costeBaseRecuperacion = 75;
    public float multiplicadorPorMejora = 1.5f;

    [Header("Puntos extra por cada mejora")]
    public int aumentoVida = 15;
    public int aumentoEnergia = 10;
    public int aumentoFuerza = 1;
    public int aumentoRecuperacion = 1;

    public int CalcularCoste(StatType stat, Personaje personaje)
    {
        int vecesMejorado = 0;
        int costeBase = 0;

        switch (stat)
        {
            case StatType.Life: vecesMejorado = personaje.mejoras_vida; costeBase = costeBaseVida; break;
            case StatType.Energy: vecesMejorado = personaje.mejoras_energia; costeBase = costeBaseEnergia; break;
            case StatType.Force: vecesMejorado = personaje.mejoras_fuerza; costeBase = costeBaseFuerza; break;
            case StatType.Recovery: vecesMejorado = personaje.mejoras_recuperacion; costeBase = costeBaseRecuperacion; break;
        }

        return Mathf.RoundToInt(costeBase * Mathf.Pow(multiplicadorPorMejora, vecesMejorado));
    }

    public async Task<bool> ComprarMejora(Usuario usuario, Personaje personajeActual, StatType stat)
    {
        int coste = CalcularCoste(stat, personajeActual);

        if (usuario.free_coin < coste)
        {
            Debug.LogWarning($"No tienes monedas. Necesitas {coste}.");
            return false;
        }

        usuario.free_coin -= coste;

        switch (stat)
        {
            case StatType.Life:
                personajeActual.mejoras_vida++;
                personajeActual.life += aumentoVida;
                break;
            case StatType.Energy:
                personajeActual.mejoras_energia++;
                personajeActual.energy += aumentoEnergia;
                break;
            case StatType.Force:
                personajeActual.mejoras_fuerza++;
                personajeActual.force += aumentoFuerza;
                break;
            case StatType.Recovery:
                personajeActual.mejoras_recuperacion++;
                personajeActual.recovery += aumentoRecuperacion;
                break;
        }

        bool userOk = await usuarioService.ActualizarUsuario(usuario);
        bool personajeOk = await usuarioService.ActualizarPersonaje(usuario.id, personajeActual);

        if (userOk && personajeOk)
        {
            Debug.Log($"¡Se ha mejorado la stat {stat}! Nivel actual: {personajeActual.NivelTotal}");
            return true;
        }

        return false;
    }
}
