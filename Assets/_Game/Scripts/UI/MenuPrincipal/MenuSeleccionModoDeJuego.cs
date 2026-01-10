using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    private int index;

    [SerializeField]
    private Image imagen;
    [SerializeField]
    private TextMeshProUGUI nombre;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        index = PlayerPrefs.GetInt("ModoDeJuegoIndex");
    }

    private void CambiarPantalla()
    {


    }
}
