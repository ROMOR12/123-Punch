using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MostrarItems : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField]
    private Button Objeto1;
    [SerializeField]
    private Button Objeto2;
    [SerializeField]
    private Button Objeto3;
    [SerializeField]
    private Button Objeto4;
    [SerializeField]
    private Button Skin1;
    [SerializeField]
    private Button Skin2;
    [SerializeField]
    private Button Skin3;
    [SerializeField]
    private Button Skin4;



    private List<Button> botones = new List<Button>();

    private List<ItemBase> items = new List<ItemBase>();
    private List<ItemBase> itemsActuales;
    private void Start()
    {
        gameManager = GameManager.Instance;

        botones.Add(Objeto1);
        botones.Add(Objeto2);
        botones.Add(Objeto3);
        botones.Add(Objeto4);
        botones.Add(Skin1);
        botones.Add(Skin2);
        botones.Add(Skin3);
        botones.Add(Skin4);

        items = gameManager.items;

        

        InicializarSprite();
    }

    private void InicializarSprite()
    {
        for (int i = 0; i <= 7; i++)
        {
            botones[i].GetComponent<Image>().sprite = (items[i].icon == null? gameManager.imageDefault: items[i].icon); 
        }
    }
}
