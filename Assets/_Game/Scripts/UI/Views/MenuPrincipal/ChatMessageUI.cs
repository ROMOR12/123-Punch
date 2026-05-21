using TMPro;
using UnityEngine;

public class ChatMessageUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text messageText;

    public void Setup(string username, string mensaje)
    {
        usernameText.text = username;
        messageText.text = mensaje;
    }
}
