using System;
using UnityEngine;
using Firebase.Messaging;
using System.Threading.Tasks;

public class NotificacionesService
{
    private bool isInitialized = false;

    public void Inicializar()
    {
        if (isInitialized) return;

        // Suscribirse a eventos de recepción de token y mensajes
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        
        // Pide permisos en iOS y Android 13+
        FirebaseMessaging.RequestPermissionAsync().ContinueWith(task => {
            isInitialized = true;
        });

        Debug.Log("[FCM] Servicio de Notificaciones Push inicializado.");
    }

    private async void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log("[FCM] Token recibido: " + token.Token);
        
        // Guardar el token en la base de datos si el usuario está logueado
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            Usuario user = SessionManager.shared.currentUser;
            
            // Solo actualizamos la BD si el token ha cambiado para ahorrar escrituras
            if (user.fcm_token != token.Token)
            {
                user.fcm_token = token.Token;
                
                UsuarioService usuarioService = new UsuarioService();
                await usuarioService.ActualizarUsuario(user);
                Debug.Log("[FCM] Token actualizado en Firestore.");
            }
        }
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        // Este evento salta cuando recibes una notificación y TIENES LA APP ABIERTA.
        // Si la app está cerrada, la maneja el sistema operativo directamente.
        Debug.Log("[FCM] Notificación recibida en primer plano: " + e.Message.Notification.Title);
    }
}
