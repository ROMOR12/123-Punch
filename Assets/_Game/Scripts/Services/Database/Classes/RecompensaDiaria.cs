using Firebase.Firestore;

[FirestoreData]
public class RecompensaDiaria
{
    [FirestoreProperty]
    public int dia { get; set; }

    [FirestoreProperty]
    public string tipo { get; set; }

    [FirestoreProperty]
    public int cantidad { get; set; }

    [FirestoreProperty]
    public string id_item { get; set; }
}
