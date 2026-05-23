using TMPro;
using UnityEngine;

public class ChatMessageUI : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text messageText;

    public void Setup(string username, string mensaje)
    {

        Debug.Log(usernameText);
        Debug.Log(messageText);
        Debug.Log("Prueba de mensaje seteado");



        usernameText.text = username;
        messageText.text = mensaje;
    }
}
