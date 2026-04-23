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
    public bool is_admin { get; set; }

    [FirestoreProperty]
    public int xp { get; set; }

    [FirestoreProperty]
    public int id_world { get; set; }

    [FirestoreProperty]
    public int id_level { get; set; }

    

    public override string ToString()
    {
        return username;
    }
}
