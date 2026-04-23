using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class Objeto
{
    [FirestoreProperty]
    public double schemaVersion { get; set; }

    [FirestoreProperty]
    public string id_Objeto { get; set; }

    [FirestoreProperty]
    public string name { get; set; }

    [FirestoreProperty]
    public string description { get; set; }

    [FirestoreProperty]
    public bool is_skin { get; set; }

    [FirestoreProperty]
    public bool is_passive { get; set; }

    [FirestoreProperty]
    public int amount { get; set; }

    [FirestoreProperty]
    public string rarity { get; set; }

    [FirestoreProperty]
    public int life { get; set; }

    [FirestoreProperty]
    public int energy { get; set; }

    [FirestoreProperty]
    public int force { get; set; }

    [FirestoreProperty]
    public int recovery { get; set; }

    [FirestoreProperty]
    public double duration { get; set; }

    public override string ToString()
    {
        return name;
    }
}
