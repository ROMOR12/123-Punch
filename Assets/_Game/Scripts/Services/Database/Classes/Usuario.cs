using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class Usuario
{
    [FirestoreProperty]
    public double schemaVersion { get; set; }
    [FirestoreProperty]
    public string id { get; set; }
    [FirestoreProperty]
    public string username { get; set; }

    [FirestoreProperty] 
    public string email { get; set; }

    [FirestoreProperty]
    public Timestamp first_log { get; set; }

    [FirestoreProperty]
    public Timestamp last_log { get; set; }

    [FirestoreProperty] 
    public int free_coin { get; set; }

    [FirestoreProperty]
    public int premium_coin { get; set; }

    [FirestoreProperty]
    public int lootboxes { get; set; }

    [FirestoreProperty]
    public List<string> inventario { get; set; } = new List<string>();

    [FirestoreProperty]
    public List<string> objetos_equipados { get; set; } = new List<string> { "", "" };

    [FirestoreProperty]
    public string pasivo_equipado { get; set; } = "";

    [FirestoreProperty]
    public bool is_admin { get; set; }

    [FirestoreProperty]
    public int xp { get; set; }

    [FirestoreProperty]
    public string id_world { get; set; }

    [FirestoreProperty]
    public string id_level { get; set; }

    [FirestoreProperty]
    public Timestamp last_daily_reward { get; set; } // Marca de tiempo del último cobro

    [FirestoreProperty]
    public int daily_reward_streak { get; set; } = 0; // Racha de días consecutivos

    [FirestoreProperty]
    public string fcm_token { get; set; } = ""; // Token de Cloud Messaging para notificaciones

    public override string ToString()
    {
        return username;
    }
}
