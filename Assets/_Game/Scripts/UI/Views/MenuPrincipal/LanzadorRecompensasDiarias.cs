using UnityEngine;

public class LanzadorRecompensasDiarias : MonoBehaviour
{
    public GameObject panelRecompensas;
    private RecompensasService recompensasService;

    private void Start()
    {
        recompensasService = new RecompensasService();
        
        // Retrasamos un poquito la comprobación para asegurarnos de que SessionManager ha cargado
        Invoke(nameof(ComprobarYAbrir), 0.5f);
    }

    private void ComprobarYAbrir()
    {
        Usuario user = SessionManager.shared.currentUser;
        
        // Si hay un usuario logueado y tiene recompensa disponible hoy...
        if (user != null && recompensasService.PuedeReclamar(user))
        {
            if (panelRecompensas != null)
            {
                panelRecompensas.SetActive(true);
            }
        }
    }
}
