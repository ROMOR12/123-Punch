using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public Transform content;
    public GameObject messagePrefab;
    public ScrollRect scrollRect;

    public TMP_InputField messageText;



    private FirebaseFirestore _context;

    private ListenerRegistration listener;

    private void Start()
    {
        _context = DatabaseManager.shared.db;

        EmpezarListener();
    }

    private void EmpezarListener()
    {
        Debug.Log("Listener iniciado");

        listener = _context.Collection("chat_global")
                     .OrderBy("fecha")
                     .LimitToLast(30)
                     .Listen(snapshot =>
                     {
                         //Voy a recorrer todos los documentos, para luego recorrer que cambios tienen cada uno de ellos
                         foreach (DocumentChange change in snapshot.GetChanges())
                         {
                             // Solo nuevos mensajes
                             if (change.ChangeType == DocumentChange.Type.Added)
                             {
                                 // Convertimos el documento a objeto
                                 ChatMessage mensaje =
                                     change.Document.ConvertTo<ChatMessage>();

                                 // Creamos mensaje visual
                                 CrearMensajeUI(mensaje);
                             }
                         }
                     });
    }



    public void CrearMensajeUI(ChatMessage mensaje)
    {
        GameObject obj = Instantiate(messagePrefab, content);
        obj.SetActive(true);

        ChatMessageUI ui = obj.GetComponent<ChatMessageUI>();

        ui.Setup(mensaje.username, mensaje.mensaje);
        

        Canvas.ForceUpdateCanvases(); //Redenderiza el canvas para que se pongan los mensajes nuevos acomodados

        scrollRect.verticalNormalizedPosition = 0f;
    }

    public async void MandarMensaje()
    {
        if (!string.IsNullOrEmpty(messageText.text))
        {
            try
            {

                ChatMessage mensaje = new ChatMessage()
                {
                    userId = SessionManager.shared.currentUser.id,
                    username = SessionManager.shared.currentUser.username,
                    mensaje = messageText.text,
                    fecha = Timestamp.GetCurrentTimestamp()
                };

                await _context.Collection("chat_global")
                .AddAsync(mensaje);

                messageText.text = "";
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error al subir un mensaje: " + e);
            }
        }
        else 
        {
            Debug.Log("No se puede subir un mensaje sin texto");
        }
    }

    private void OnDestroy()
    {
        listener?.Stop();
    }
}
