using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorEscenas : MonoBehaviour
{

    public GameObject popUp;
    public TextMeshProUGUI textoInfo;
    public Image imagenObjeto;
    private ItemBase itemActual;

    public void IrAEscenaCajas()
    {
        if(GameManager.Instance.numCajas >= 1)
        {
            GameManager.Instance.numCajas--;
            CargaEscena.Cargar("LootBoxScene");
        }
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }



    public void MostrarPopUpObjeto(GameObject botonPulsado)
    {
        itemActual = GameManager.Instance.items.Find(item => item.icon == botonPulsado.GetComponent<Image>().sprite);
        textoInfo.text = itemActual.name;
        imagenObjeto.sprite = botonPulsado.GetComponent<Image>().sprite;
        popUp.SetActive(true);
    }
    public void CerrarPopUp()
    {
        popUp.SetActive(false);
    }
}
