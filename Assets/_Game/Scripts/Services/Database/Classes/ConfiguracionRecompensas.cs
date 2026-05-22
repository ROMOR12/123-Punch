using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
public class ConfiguracionRecompensas
{
    [FirestoreProperty]
    public List<RecompensaDiaria> dias { get; set; } = new List<RecompensaDiaria>();
}
