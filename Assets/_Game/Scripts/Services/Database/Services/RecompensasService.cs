using UnityEngine;
using Firebase.Firestore;
using System.Threading.Tasks;
using System;

public class RecompensasService
{
    private FirebaseFirestore db;
    private const string COLLECTION_GLOBAL = "ConfiguracionGlobal";
    private const string DOC_RECOMPENSAS = "RecompensasDiarias";

    public RecompensasService()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task<ConfiguracionRecompensas> ObtenerConfiguracion()
    {
        try
        {
            DocumentSnapshot snapshot = await db.Collection(COLLECTION_GLOBAL).Document(DOC_RECOMPENSAS).GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<ConfiguracionRecompensas>();
            }
            else
            {
                Debug.LogWarning("No se encontró el documento de RecompensasDiarias en Firestore.");
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error obteniendo configuración de recompensas: {e.Message}");
            return null;
        }
    }

    public bool PuedeReclamar(Usuario user)
    {
        if (user == null) return false;
        
        // Si nunca ha reclamado, el Timestamp será 1970 (default)
        if (user.last_daily_reward.ToDateTime().Year < 2000) return true;

        DateTime lastClaimDate = user.last_daily_reward.ToDateTime().ToLocalTime();
        DateTime now = Timestamp.GetCurrentTimestamp().ToDateTime().ToLocalTime();

        // Comprobamos la diferencia de días calendario
        TimeSpan diff = now.Date - lastClaimDate.Date;
        return diff.TotalDays >= 1;
    }

    public int ObtenerRachaActualizada(Usuario user)
    {
        if (user == null) return 0;
        if (user.last_daily_reward.ToDateTime().Year < 2000) return 1;
        
        DateTime lastClaimDate = user.last_daily_reward.ToDateTime().ToLocalTime();
        DateTime now = Timestamp.GetCurrentTimestamp().ToDateTime().ToLocalTime();

        TimeSpan diff = now.Date - lastClaimDate.Date;

        if (diff.TotalDays == 1)
        {
            // Mantiene racha: al siguiente día
            int nuevaRacha = user.daily_reward_streak + 1;
            if (nuevaRacha > 7) nuevaRacha = 1; // Vuelve a empezar el ciclo si pasa de 7
            return nuevaRacha;
        }
        else if (diff.TotalDays > 1)
        {
            // Perdió la racha por faltar un día o más
            return 1;
        }
        else
        {
            // Mismo día (0 días de diferencia)
            return user.daily_reward_streak;
        }
    }

    public async Task<bool> Reclamar(Usuario user, RecompensaDiaria recompensaGanada)
    {
        if (!PuedeReclamar(user)) return false;

        // 1. Actualizamos racha y recompensa
        user.daily_reward_streak = ObtenerRachaActualizada(user);
        
        // 2. Sumamos el premio al inventario/saldo del usuario
        if (recompensaGanada.tipo == "free_coin")
        {
            user.free_coin += recompensaGanada.cantidad;
        }
        else if (recompensaGanada.tipo == "premium_coin")
        {
            user.premium_coin += recompensaGanada.cantidad;
        }
        else if (recompensaGanada.tipo == "lootbox")
        {
            user.lootboxes += recompensaGanada.cantidad;
        }
        else if (recompensaGanada.tipo == "item")
        {
            // Ejemplo para añadir un item específico por su ID al inventario
            if (!string.IsNullOrEmpty(recompensaGanada.id_item))
            {
                user.inventario.Add(recompensaGanada.id_item);
            }
        }
        
        // 3. Marcamos la hora de cobro como la hora actual sincronizada con Firebase
        user.last_daily_reward = Timestamp.GetCurrentTimestamp();
        
        // 4. Guardamos en Base de Datos
        try
        {
            UsuarioService usuarioService = new UsuarioService();
            bool ok = await usuarioService.ActualizarUsuario(user);
            return ok;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al guardar tras reclamar recompensa: {e}");
            return false;
        }
    }
}
