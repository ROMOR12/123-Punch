using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TarjetaRecompensaUI : MonoBehaviour
{
    public TextMeshProUGUI txtDia;
    public TextMeshProUGUI txtNombreItem;
    public TextMeshProUGUI txtCantidad;
    public Image icono;
    
    [Header("Estados Visuales")]
    public GameObject overlayReclamado;
    public GameObject outlineActual;

    public void Configurar(RecompensaDiaria recompensa, Sprite spriteIcono, string nombreItem, bool yaReclamado, bool esDiaActual)
    {
        if (txtDia != null) txtDia.text = $"Día {recompensa.dia}";
        if (txtNombreItem != null) txtNombreItem.text = nombreItem;
        if (txtCantidad != null) txtCantidad.text = $"x{recompensa.cantidad}";
        if (icono != null && spriteIcono != null) icono.sprite = spriteIcono;

        if (overlayReclamado != null) 
        {
            overlayReclamado.SetActive(yaReclamado);
        }

        if (outlineActual != null) 
        {
            outlineActual.SetActive(esDiaActual);
        }
    }
}
