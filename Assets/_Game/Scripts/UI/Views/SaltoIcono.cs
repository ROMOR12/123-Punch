using UnityEngine;

public class SaltoIcono : MonoBehaviour
{
    [Header("Configuración del Salto")]
    public float fuerzaSalto = 50f;
    public float velocidadSalto = 5f;

    private Vector3 posicionInicial;

    void Start()
    {
        // Guarda la posición original
        posicionInicial = transform.localPosition;
    }

    void Update()
    {
        // Calcula el salto
        float nuevoY = Mathf.Abs(Mathf.Sin(Time.time * velocidadSalto)) * fuerzaSalto;

        // Simula el salto cambiando la posición actual por la orginial más la nueva
        transform.localPosition = posicionInicial + new Vector3(0, nuevoY, 0);
    }
}
