using UnityEngine;

// Este script destruye el objeto de la explosion despues de un tiempo para que no se quede en bucle
public class AutoDestroy : MonoBehaviour
{
    public float delay = 1f;

    void Start()
    {
        Destroy(gameObject, delay);
    }
}
