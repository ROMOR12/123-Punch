using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class ReferenciaObjeto
{
    [FirestoreProperty]
    public double schemaVersion { get; set; }

    [FirestoreProperty]
    public string id_Objeto { get; set; }

    [FirestoreProperty]
    public string name { get; set; }

    [FirestoreProperty]
    public int cantidad { get; set; }

    public override string ToString()
    {
        return name;
    }
}
