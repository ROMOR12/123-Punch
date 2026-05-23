using UnityEngine;
using TMPro; // Necesario si usas TextMeshPro

public class MostrarBilleteraUI : MonoBehaviour
{
    [Header("Textos de la Interfaz")]
    public TextMeshProUGUI textoMonedas;
    public TextMeshProUGUI textoBilletes;

    void Update()
    {
        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            if (textoMonedas != null)
                textoMonedas.text = SessionManager.shared.currentUser.free_coin.ToString();

            if (textoBilletes != null)
                textoBilletes.text = SessionManager.shared.currentUser.premium_coin.ToString();
        }
    }
}
