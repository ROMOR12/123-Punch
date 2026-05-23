using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// esta clase controla la logica del chat en tiempo real escuchando la base de datos de firestore
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
        // esta funcion inicializa el contexto de la base de datos y comienza a escuchar los mensajes nuevos
        _context = DatabaseManager.shared.db;
        EmpezarListener();
    }

    private void EmpezarListener()
    {
        // esta funcion suscribe un listener a la coleccion de mensajes para recibir actualizaciones en tiempo real
        Debug.Log("Listener iniciado");

        listener = _context.Collection("chat_global")
                     .OrderBy("fecha")
                     .LimitToLast(30)
                     .Listen(snapshot =>
                     {
                         // esta iteracion recorre las modificaciones en los documentos de la coleccion
                         foreach (DocumentChange change in snapshot.GetChanges())
                         {
                             // esta condicion comprueba si el cambio consiste en un nuevo mensaje añadido
                             if (change.ChangeType == DocumentChange.Type.Added)
                             {
                                 // esta linea convierte los datos de firestore a la clase del mensaje
                                 ChatMessage mensaje =
                                     change.Document.ConvertTo<ChatMessage>();

                                 // esta funcion instancia el mensaje en la interfaz
                                 CrearMensajeUI(mensaje);
                              }
                          }
                      });
    }

    public void CrearMensajeUI(ChatMessage mensaje)
    {
        // esta funcion instancia y configura el prefab de mensaje en el scroll de la interfaz
        GameObject obj = Instantiate(messagePrefab, content);
        obj.SetActive(true);

        ChatMessageUI ui = obj.GetComponent<ChatMessageUI>();
        ui.Setup(mensaje.username, mensaje.mensaje);
        
        // esta funcion fuerza la actualizacion del canvas de unity para posicionar correctamente el nuevo elemento
        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;
    }

    public async void MandarMensaje()
    {
        // esta funcion envia el mensaje escrito por el usuario actual a la base de datos de firestore
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
        // esta funcion detiene la escucha de mensajes al destruir el objeto
        listener?.Stop();
    }
}
