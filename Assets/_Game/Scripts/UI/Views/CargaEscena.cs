using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Mathematics;

public class CargaEscena : MonoBehaviour
{
    public Slider barraDeProgreso;
    [Range(0.1f, 2f)]
    public float suavizarBarra = 0.5f; // Controla lo rápido que se llena la barra de carga

    public static string siguienteEscena;

    public static void Cargar(string nombreEscena)
    {
        siguienteEscena = nombreEscena;
        // Cargamos la escena de carga
        SceneManager.LoadScene("LoadingScreen");
    }

    void Start()
    {
        // Iniciamos la carga en segundo plano
        StartCoroutine(CargarAsincronamente(siguienteEscena));
    }
    void Update()
    {
        
    }

    IEnumerator CargarAsincronamente(string escenaCargando)
    {
        // Carga la siguiente escena en segundo plano
        AsyncOperation operacion = SceneManager.LoadSceneAsync(escenaCargando);

        // Evitamos cambie a la siguiente escena cuando carga
        operacion.allowSceneActivation = false;

        float progresoVisual = 0f;

        float velocidadDeLlenado = 0.5f;

        while (!operacion.isDone)
        {
            float progreso = Mathf.Clamp01(operacion.progress / 0.9f);

            // La barra se llena gradualmente sin dar saltos bruscos
            progresoVisual += velocidadDeLlenado * Time.deltaTime;

            // Hace que la barra no supere al progreso real
            float valorFinal = Mathf.Min(progresoVisual, progreso);
            barraDeProgreso.value = valorFinal;

            // Una vez la barra y la carga real terminan
            if (valorFinal >= 1f)
            {
                // Delay estético y permite que se active la proxima escena
                yield return new WaitForSeconds(0.2f);
                operacion.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
