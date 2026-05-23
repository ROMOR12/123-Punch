using UnityEngine;

using Firebase.Firestore;

[FirestoreData]
public class ChatMessage
{
    [FirestoreProperty] public string userId { get; set; }

    [FirestoreProperty] public string username { get; set; }

    [FirestoreProperty] public string mensaje { get; set; }

    [FirestoreProperty] public Timestamp fecha { get; set; }
}

