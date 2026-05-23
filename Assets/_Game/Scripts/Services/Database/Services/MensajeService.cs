using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class MensajeService : MonoBehaviour
{

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
