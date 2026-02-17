using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private int index;

    [SerializeField]
    private Image ImagenModoDeJuego;
    [SerializeField]
    private TextMeshProUGUI TextoModoDeJuego;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        index = PlayerPrefs.GetInt("ModoDeJuegoIndex", 0);

        if (gameManager != null && index >= gameManager.modosDeJuego.Count)
        {
            index = 0;
        }

        CambiarPantalla();
    }

    private void CambiarPantalla()
    {
        PlayerPrefs.SetInt("ModoDeJuegoIndex", index);
        ImagenModoDeJuego.sprite = gameManager.modosDeJuego[index].Imagen;
        TextoModoDeJuego.text = gameManager.modosDeJuego[index].Nombre;
    }
       
    public void SiguienteModoDeJuego()
    {
        if (index == gameManager.modosDeJuego.Count - 1)
        {
            index = 0;
        }
        else
        {
            index++;
        }
        CambiarPantalla();
    }
    public void AnteriorModoDeJuego()
    {
        if (index == 0)
        {
            index = gameManager.modosDeJuego.Count-1;
        }
        else
        {
            index--;
        }
        CambiarPantalla();
    }

    public void IniciarModoDeJuego()
    {
        var modoSeleccionado = gameManager.modosDeJuego[index];

        string nombreDelModo = modoSeleccionado.Nombre;

        string escena = "";
        switch (nombreDelModo)
        {
            case "Campaña":
                escena = "CombateDePrueba";
                break;

            case "Buffet LIbre": 
                escena = "EatingMinigame";
                break;

            case "VSBot":
                escena = "CombateDePrueba";
                break;
        }
        CargaEscena.Cargar(escena);
    }
}
