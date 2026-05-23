using UnityEngine;
using Firebase.Firestore;

// esta clase define la estructura de datos de un mensaje de chat almacenado en firebase firestore
[FirestoreData]
public class ChatMessage
{
    // este atributo almacena el identificador unico del usuario
    [FirestoreProperty] public string userId { get; set; }

    // este atributo almacena el nombre publico del usuario
    [FirestoreProperty] public string username { get; set; }

    // este atributo almacena el contenido de texto del mensaje
    [FirestoreProperty] public string mensaje { get; set; }

    // este atributo almacena la marca de tiempo de cuando se envio el mensaje
    [FirestoreProperty] public Timestamp fecha { get; set; }
}
