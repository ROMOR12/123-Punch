using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordToggle : MonoBehaviour
{
    [Header("Referencias")]
    public TMP_InputField inputPass; 
    public Image imagenIcono;

    [Header("Iconos")]
    public Sprite iconoOcultar;
    public Sprite iconoVer;

    private bool esVisible = false;

    // metodo para alternar la visibilidad de la contraseña
    public void AlternarVisibilidad()
    {
        // cambiar el estado de visibilidad
        esVisible = !esVisible;

        if (esVisible)
        {
            inputPass.contentType = TMP_InputField.ContentType.Standard;
            imagenIcono.sprite = iconoOcultar;
        }
        else
        {
            inputPass.contentType = TMP_InputField.ContentType.Password;
            imagenIcono.sprite = iconoVer;
        }

        // actualizar la visualización del texto
        inputPass.ForceLabelUpdate();
    }
}
