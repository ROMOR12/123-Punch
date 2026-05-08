using Firebase.Firestore;
using System.Threading.Tasks;

public class EnemyService
{
    public async Task<Enemy> ObtenerEnemigo(string enemyId)
    {
        try
        {
            var db = DatabaseManager.shared.db;
            DocumentSnapshot snap = await db.Collection("enemigos").Document(enemyId).GetSnapshotAsync();

            if (snap.Exists)
            {
                return snap.ConvertTo<Enemy>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
