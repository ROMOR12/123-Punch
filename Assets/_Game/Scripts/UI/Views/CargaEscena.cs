using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Mathematics;

public class CargaEscena : MonoBehaviour
{
    public Slider barraDeProgreso;
    public string escenaCargando;
    [Range(0.1f, 2f)]
    public float suavizarBarra = 0.5f;

    void Start()
    {
        StartCoroutine(CargarAsincronamente());
    }
    void Update()
    {
        
    }

    IEnumerator CargarAsincronamente()
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync(escenaCargando);

        operacion.allowSceneActivation = false;

        float progresoVisual = 0f;

        float velocidadDeLlenado = 0.5f;

        while (progresoVisual < 1f)
        {
            float progreso = Mathf.Clamp01(operacion.progress / 0.9f);

            progresoVisual += velocidadDeLlenado * Time.deltaTime;

            float valorFinal = Mathf.Min(progresoVisual, progreso);

            barraDeProgreso.value = valorFinal;

            if (valorFinal >= 1f)
            {
                yield return new WaitForSeconds(0.2f);

                operacion.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
