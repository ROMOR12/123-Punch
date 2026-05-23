using UnityEngine;

public static class LevelSystem
{
    public static int CalcularNivel(int xpTotal)
    {
        int nivel = 1;
        while (xpTotal >= XPParaNivel(nivel + 1))
        {
            nivel++;
        }
        return nivel;
    }

    public static int XPParaNivel(int nivel)
    {
        return 50 * nivel * (nivel - 1);
    }

    public static int XPEnNivelActual(int xpTotal)
    {
        int nivelActual = CalcularNivel(xpTotal);
        int xpBaseDelNivel = XPParaNivel(nivelActual);
        return xpTotal - xpBaseDelNivel;
    }

    public static int XPNecesariaParaSiguienteNivel(int xpTotal)
    {
        int nivelActual = CalcularNivel(xpTotal);
        int xpBaseNivel = XPParaNivel(nivelActual);
        int xpBaseSiguiente = XPParaNivel(nivelActual + 1);
        return xpBaseSiguiente - xpBaseNivel;
    }
    
    public static float ProgresoNivel(int xpTotal)
    {
        float xpActual = XPEnNivelActual(xpTotal);
        float xpRequerida = XPNecesariaParaSiguienteNivel(xpTotal);
        if (xpRequerida == 0) return 1f;
        return xpActual / xpRequerida;
    }
}
