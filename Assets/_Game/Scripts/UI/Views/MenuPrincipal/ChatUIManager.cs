using UnityEngine;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public Transform content;
    public GameObject messagePrefab;
    public ScrollRect scrollRect;

    public void CrearMensajeUI(ChatMessage mensaje)
    {
        GameObject obj = Instantiate(messagePrefab, content);

        ChatMessageUI ui = obj.GetComponent<ChatMessageUI>();

        ui.Setup(mensaje.username, mensaje.mensaje);

        Canvas.ForceUpdateCanvases(); //Redenderiza el canvas para que se pongan los mensajes nuevos acomodados

        scrollRect.verticalNormalizedPosition = 0f;
    }
}
