using TMPro;
using UnityEngine;

// esta clase gestiona los campos de texto del mensaje de chat en la interfaz
public class ChatMessageUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text messageText;

    // esta funcion asigna el nombre de usuario y el texto del mensaje a sus componentes visuales
    public void Setup(string username, string mensaje)
    {
        Debug.Log(usernameText);
        Debug.Log(messageText);

        usernameText.text = username;
        messageText.text = mensaje;
    }
}
