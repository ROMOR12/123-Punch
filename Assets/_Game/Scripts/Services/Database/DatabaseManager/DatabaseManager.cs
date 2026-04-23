using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager shared;

    public FirebaseFirestore db { get; private set; }

    void Awake()
    {
        if (shared == null)
        {
            shared = this;
            DontDestroyOnLoad(gameObject);

            db = FirebaseFirestore.DefaultInstance;
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
