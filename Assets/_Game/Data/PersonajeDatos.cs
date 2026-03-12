using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class PersonajeDatos
{
    [FirestoreProperty]
    public double schemaVersion { get; set; }

    [FirestoreProperty]
    public string id { get; set; }

    [FirestoreProperty]
    public string name { get; set; }

    [FirestoreProperty]
    public int life { get; set; }

    [FirestoreProperty]
    public int energy { get; set; }

    [FirestoreProperty]
    public int force { get; set; }

    [FirestoreProperty]
    public int recovery { get; set; }

    public override string ToString()
    {
        return name;
    }
}
