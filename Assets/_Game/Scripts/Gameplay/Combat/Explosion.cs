using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 1f; // Tiempo que dura la animación (aprox)

    void Start()
    {
        Destroy(gameObject, delay);
    }
}
