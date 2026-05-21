using Firebase.Firestore;

[FirestoreData]
public class TiendaItem
{
    [FirestoreProperty]
    public string id_oferta { get; set; }

    [FirestoreProperty]
    public string id_Objeto { get; set; }

    [FirestoreProperty]
    public int precio_monedas { get; set; }

    [FirestoreProperty]
    public bool en_venta { get; set; }
}
