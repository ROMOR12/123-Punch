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
    public GameObject overlayReclamado; // Capa oscura con el tick verde
    public GameObject outlineActual;    // Borde brillante para el día de hoy

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
            // Solo brilla si es el día que toca cobrar hoy
            outlineActual.SetActive(esDiaActual);
        }
    }
}
