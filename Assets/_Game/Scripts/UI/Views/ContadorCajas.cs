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
        textoCajas.text = GameManager.Instance.numCajas.ToString();
    }
}
