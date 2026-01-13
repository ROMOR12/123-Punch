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

        index = PlayerPrefs.GetInt("ModoDeJuegoIndex");

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
        string escena = "";
        switch(index)
        {
            case 0:
                escena = "CombatPrototype";
            break;
            case 1:
                escena = "CombatPrototype";
            break;
            case 2:
                escena = "CombatPrototype";
            break;  
            default:
                escena = "CombatPrototype";
            break;
        }
        SceneManager.LoadScene(escena);
    }
}
