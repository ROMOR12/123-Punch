using UnityEngine;

public class LanzadorRecompensasDiarias : MonoBehaviour
{
    public GameObject panelRecompensas;
    private RecompensasService recompensasService;

    private void Start()
    {
        recompensasService = new RecompensasService();
        
        Invoke(nameof(ComprobarYAbrir), 0.5f);
    }

    private void ComprobarYAbrir()
    {
        Usuario user = SessionManager.shared.currentUser;
        
        if (user != null && recompensasService.PuedeReclamar(user))
        {
            if (panelRecompensas != null)
            {
                panelRecompensas.SetActive(true);
            }
        }
    }
}
