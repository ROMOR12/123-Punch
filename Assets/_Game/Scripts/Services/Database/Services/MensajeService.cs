using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

// esta clase gestiona los servicios de envio de mensajes al chat en la base de datos
public class MensajeService : MonoBehaviour
{
    // esta funcion envia un nuevo mensaje de chat a la coleccion global de firebase firestore
    public async Task EnviarMensaje(string userId, string username, string texto)
    {
        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;

            ChatMessage mensaje = new ChatMessage()
            {
                userId = userId,
                username = username,
                mensaje = texto,
                fecha = Timestamp.GetCurrentTimestamp()
            };

            await _context.Collection("chat_global")
                    .AddAsync(mensaje);
        }
        catch
        {
            Console.WriteLine("Error al enviar mensaje a la BD");
        }
    }
}
