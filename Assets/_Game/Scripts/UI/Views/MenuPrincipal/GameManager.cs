using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int numCajas = 0;

    public static GameManager Instance;

    public List<ModosDeJuego> modosDeJuego;

    public List<ItemBase> items;

    public Sprite imageDefault;

    public List<BaseCharacter> listaPersonajes;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            GameManager.Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }
}
