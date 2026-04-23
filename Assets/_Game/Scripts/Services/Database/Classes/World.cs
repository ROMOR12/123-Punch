using Firebase.Firestore;
using UnityEngine;


[FirestoreData]
public class World : MonoBehaviour
{
    [FirestoreProperty]
    public double schemaVersion { get; set; }

    [FirestoreProperty]
    public string id { get; set; }

    [FirestoreProperty]
    public string name { get; set; }

    [FirestoreProperty]
    public string description { get; set; }

    public override string ToString()
    {
        return name;
    }
}
