using TMPro;
using UnityEngine;

// Script que actualiza el numerito para ver las cajas que tienes
public class ContadorCajas : MonoBehaviour
{
    private TextMeshProUGUI textoCajas;

    void Start()
    {
        textoCajas = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (textoCajas == null) return;

        if (SessionManager.shared != null && SessionManager.shared.currentUser != null)
        {
            textoCajas.text = SessionManager.shared.currentUser.lootboxes.ToString();
        }
        else
        {
            textoCajas.text = "0";
        }
    }
}
