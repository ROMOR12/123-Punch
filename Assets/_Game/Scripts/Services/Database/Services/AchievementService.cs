using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class AchievementService
{
    public async Task<List<AchievementData>> ObtenerLogrosUsuario(string userId)
    {
        List<AchievementData> logros = new List<AchievementData>();

        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;
            QuerySnapshot snapshot = await _context.Collection("usuarios").Document(userId).Collection("achievements").GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    AchievementData data = doc.ConvertTo<AchievementData>();
                    logros.Add(data);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error al obtener logros de Firebase: {e.Message}");
        }

        return logros;
    }

    public async Task<bool> GuardarLogroUsuario(string userId, AchievementData data)
    {
        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;
            DocumentReference docRef = _context.Collection("usuarios").Document(userId).Collection("achievements").Document(data.id);

            await docRef.SetAsync(data, SetOptions.MergeAll);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error guardando logro {data.id}: {e.Message}");
            return false;
        }
    }
}
