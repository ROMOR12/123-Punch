using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BotonNivel : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [Tooltip("El número de este nivel (1 para Nivel 1, 2 para Nivel 2, etc.)")]
    public int numeroDeEsteNivel;

    private Button miBoton;


    void OnEnable()
    {
        miBoton = GetComponent<Button>();
        ComprobarBloqueo();
    }

    public void ComprobarBloqueo()
    {
        if (GameManager.Instance == null) return;

        int nivelPermitido = GameManager.Instance.nivelMaximoDesbloqueado;

        if (numeroDeEsteNivel <= nivelPermitido)
        {
            miBoton.interactable = true;
        }
        else
        {
            miBoton.interactable = false;
        }
    }
}
