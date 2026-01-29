using UnityEngine;

public class SaltoIcono : MonoBehaviour
{
    [Header("Configuración del Salto")]
    public float fuerzaSalto = 50f;
    public float velocidadSalto = 5f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    void Update()
    {
        float nuevoY = Mathf.Abs(Mathf.Sin(Time.time * velocidadSalto)) * fuerzaSalto;

        transform.localPosition = posicionInicial + new Vector3(0, nuevoY, 0);
    }
}
