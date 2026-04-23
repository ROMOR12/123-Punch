using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class WorldService : MonoBehaviour
{
    public async Task<World?> ObtenerMundo(string nombreMundo)
    {
        try
        {
            FirebaseFirestore _context = DatabaseManager.shared.db;

            DocumentSnapshot docSnap = await _context.Collection("mundos")
                .Document(nombreMundo)
                .GetSnapshotAsync();

            if(docSnap.Exists)
            {
                return docSnap.ConvertTo<World>();
            }
            else
            {
                return null;
            }    
        }
        catch
        {
            return null;
        }
    }
}
