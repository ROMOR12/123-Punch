using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Firestore;

public class TiendaService
{
    private FirebaseFirestore db;
    private UsuarioService usuarioService;

    public TiendaService()
    {
        db = FirebaseFirestore.DefaultInstance;
        usuarioService = new UsuarioService();
    }

    public async Task<List<TiendaItem>> ObtenerCatalogoActivo()
    {
        List<TiendaItem> catalogo = new List<TiendaItem>();
        
        try
        {
            QuerySnapshot snapshot = await db.Collection("tienda").WhereEqualTo("en_venta", true).GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    TiendaItem item = document.ConvertTo<TiendaItem>();
                    catalogo.Add(item);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error descargando tienda: {ex}");
        }

        return catalogo;
    }

    public async Task<bool> Comprar(Usuario user, TiendaItem oferta)
    {
        if (user.free_coin < oferta.precio_monedas)
        {
            Debug.LogWarning("Monedas insuficientes.");
            return false;
        }

        // Restar dinero localmente
        user.free_coin -= oferta.precio_monedas;
        
        // Añadir el item correspondiente
        if (oferta.id_Objeto == "Item_LootBox")
        {
            user.lootboxes++;
        }
        else
        {
            if (user.inventario == null) user.inventario = new List<string>();
            user.inventario.Add(oferta.id_Objeto);
        }

        // Actualizar en Firebase
        return await usuarioService.ActualizarUsuario(user);
    }
}
