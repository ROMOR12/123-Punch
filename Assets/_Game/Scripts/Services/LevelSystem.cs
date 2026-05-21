using UnityEngine;

public static class LevelSystem
{
    // Curva de experiencia: Cada nivel cuesta más que el anterior.
    // Nivel 1 a 2: 100 XP
    // Nivel 2 a 3: 200 XP
    // Nivel 3 a 4: 300 XP
    // La fórmula para la XP total requerida para llegar a un nivel N es: 50 * N * (N - 1)
    
    public static int CalcularNivel(int xpTotal)
    {
        // Resolvemos la ecuación cuadrática inversa para saber el nivel
        // xp = 50 * n^2 - 50 * n => n^2 - n - (xp/50) = 0
        int nivel = 1;
        while (xpTotal >= XPParaNivel(nivel + 1))
        {
            nivel++;
        }
        return nivel;
    }

    // Devuelve la XP total requerida para llegar a un nivel específico desde 0
    public static int XPParaNivel(int nivel)
    {
        return 50 * nivel * (nivel - 1);
    }

    // Calcula cuánta XP tienes dentro de tu nivel actual (ej. si tienes 150 y el nivel 2 pide 100 y el 3 pide 300, devolverá 50)
    public static int XPEnNivelActual(int xpTotal)
    {
        int nivelActual = CalcularNivel(xpTotal);
        int xpBaseDelNivel = XPParaNivel(nivelActual);
        return xpTotal - xpBaseDelNivel;
    }

    // Calcula cuánta XP en total requiere tu nivel actual para subir al siguiente (ej. del 2 al 3 requiere 200 XP)
    public static int XPNecesariaParaSiguienteNivel(int xpTotal)
    {
        int nivelActual = CalcularNivel(xpTotal);
        int xpBaseNivel = XPParaNivel(nivelActual);
        int xpBaseSiguiente = XPParaNivel(nivelActual + 1);
        return xpBaseSiguiente - xpBaseNivel; // Es decir, 100 * nivelActual
    }
    
    // Devuelve el porcentaje de progreso en el nivel actual de 0.0 a 1.0 (ideal para el Slider de UI)
    public static float ProgresoNivel(int xpTotal)
    {
        float xpActual = XPEnNivelActual(xpTotal);
        float xpRequerida = XPNecesariaParaSiguienteNivel(xpTotal);
        if (xpRequerida == 0) return 1f; // Prevención division por cero (max level)
        return xpActual / xpRequerida;
    }
}
