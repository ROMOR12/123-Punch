using TMPro;
using UnityEngine;

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
