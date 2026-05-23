#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Firebase;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GeneradorBalanceoStats
{
    [MenuItem("Tools/Firebase/Aplicar Balanceo (Personajes, Enemigos, Objetos)")]
    public static async void SubirBalanceoMaestro()
    {
        Debug.Log("Iniciando conexión con Firebase para aplicar el balanceo maestro...");
        
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Error al iniciar Firebase: " + dependencyStatus);
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        
        await SubirPersonajes(db);
        await SubirEnemigos(db);
        await SubirObjetosGlobales(db);
        
        Debug.Log("¡BALANCEO MAESTRO APLICADO CORRECTAMENTE EN FIREBASE!");
    }

    private static async Task SubirPersonajes(FirebaseFirestore db)
    {
        CollectionReference personajesRef = db.Collection("personajes");

        var p1 = new Personaje { schemaVersion = 1.0, id = "james", name = "James", life = 100, energy = 100, force = 2, recovery = 2 };
        var p2 = new Personaje { schemaVersion = 1.0, id = "ava", name = "Ava", life = 80, energy = 120, force = 4, recovery = 3 };
        var p3 = new Personaje { schemaVersion = 1.0, id = "mike", name = "Mike", life = 200, energy = 90, force = 3, recovery = 2 };
        var p4 = new Personaje { schemaVersion = 1.0, id = "miguel", name = "Miguel", life = 150, energy = 150, force = 4, recovery = 4 };

        await personajesRef.Document("james").SetAsync(p1);
        await personajesRef.Document("ava").SetAsync(p2);
        await personajesRef.Document("mike").SetAsync(p3);
        await personajesRef.Document("miguel").SetAsync(p4);

        Debug.Log("-> 4 Personajes balanceados subidos.");
    }

    private static async Task SubirEnemigos(FirebaseFirestore db)
    {
        CollectionReference enemigosRef = db.Collection("enemigos");

        var bot1 = new Dictionary<string, object> { { "id", "bot_1" }, { "name", "Enemy1" }, { "life", 50 }, { "energy", 50 }, { "force", 1 }, { "recovery", 1 } };
        var bot2 = new Dictionary<string, object> { { "id", "bot_2" }, { "name", "Enemy2" }, { "life", 70 }, { "energy", 60 }, { "force", 2 }, { "recovery", 1 } };
        var bot3 = new Dictionary<string, object> { { "id", "bot_3" }, { "name", "Enemy3" }, { "life", 90 }, { "energy", 70 }, { "force", 2 }, { "recovery", 2 } };
        var bot4 = new Dictionary<string, object> { { "id", "bot_4" }, { "name", "Enemy4" }, { "life", 110 }, { "energy", 80 }, { "force", 3 }, { "recovery", 2 } };
        var jason = new Dictionary<string, object> { { "id", "enemy_jason" }, { "name", "Jason" }, { "life", 130 }, { "energy", 90 }, { "force", 4 }, { "recovery", 2 } };
        var bot5 = new Dictionary<string, object> { { "id", "bot_5" }, { "name", "Enemy5" }, { "life", 150 }, { "energy", 100 }, { "force", 4 }, { "recovery", 3 } };
        var bot6 = new Dictionary<string, object> { { "id", "bot_6" }, { "name", "Enemy6" }, { "life", 170 }, { "energy", 110 }, { "force", 5 }, { "recovery", 3 } };
        var bot7 = new Dictionary<string, object> { { "id", "bot_7" }, { "name", "Enemy7" }, { "life", 200 }, { "energy", 120 }, { "force", 6 }, { "recovery", 3 } };
        var zombie = new Dictionary<string, object> { { "id", "bot_zombie" }, { "name", "Zombie" }, { "life", 250 }, { "energy", 100 }, { "force", 7 }, { "recovery", 2 } };
        var demon = new Dictionary<string, object> { { "id", "bot_demon" }, { "name", "Demon" }, { "life", 300 }, { "energy", 150 }, { "force", 8 }, { "recovery", 4 } };

        await enemigosRef.Document("bot_1").SetAsync(bot1);
        await enemigosRef.Document("bot_2").SetAsync(bot2);
        await enemigosRef.Document("bot_3").SetAsync(bot3);
        await enemigosRef.Document("bot_4").SetAsync(bot4);
        await enemigosRef.Document("enemy_jason").SetAsync(jason);
        await enemigosRef.Document("bot_5").SetAsync(bot5);
        await enemigosRef.Document("bot_6").SetAsync(bot6);
        await enemigosRef.Document("bot_7").SetAsync(bot7);
        await enemigosRef.Document("bot_zombie").SetAsync(zombie);
        await enemigosRef.Document("bot_demon").SetAsync(demon);

        Debug.Log("-> 10 Enemigos balanceados subidos.");
    }

    private static async Task SubirObjetosGlobales(FirebaseFirestore db)
    {
        CollectionReference objetosRef = db.Collection("objetos_globales");

        var obj1 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_activo_isotonica", name = "Bebida Isotónica", description = "Bebida consumida mayormente por crossfiteros. Te ayudará a recuperarte de la paliza del adversario.\nCura 50 de salud.", is_skin = false, is_passive = false, amount = 1, rarity = "Raro", life = 50, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj2 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_chicle_rancio", name = "Chicle Rancio", description = "Este chicle, que tiene aspecto de haberse caído al suelo, parece haber adquirido efectos curativos.\nCura 20 de salud.", is_skin = false, is_passive = false, amount = 1, rarity = "Comun", life = 20, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj3 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_clembuterol", name = "Clembuterol", description = "Estas cansado de ser un flojeras, tomate esto para estar to fuerte y recuperarte de los entrenos.\n+2 de fuerza\n+2 de recuperacion", is_skin = false, is_passive = false, amount = 1, rarity = "Ilegal", life = 0, energy = 0, force = 2, recovery = 2, duration = 0 };
        var obj4 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_kebab_mixto", name = "Kebab Mixto", description = "El alimento de los dioses y tu mejor amigo cuando sales de fiesta. Te dejará como nuevo. Eso sí, saluda de mi parte al váter cuando lo visites.\nCura 150 de vida", is_skin = false, is_passive = false, amount = 1, rarity = "Legendario", life = 150, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj5 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_preentreno", name = "Preentreno", description = "Si lo que quieres es rendir a tope, esto es perfecto. Eso sí, no te pases con la dosis o prepárate para las consecuencias.\n+20 de energia\n+1 de fuerza", is_skin = false, is_passive = false, amount = 1, rarity = "Raro", life = 0, energy = 20, force = 1, recovery = 0, duration = 0 };
        var obj6 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_seta_sospechosa", name = "Seta Sospechosa", description = "Una seta que parece salida de un juego de un fontanero que rescata princesas. Al consumirla, te sentirás invencible.\n+50 de energia\n+2 de fuerza\n-30 de vida", is_skin = false, is_passive = false, amount = 1, rarity = "Epico", life = -30, energy = 50, force = 2, recovery = 0, duration = 0 };
        var obj7 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_aguita_fresca_pasivo", name = "Aguita fresca", description = "Si tienes sed y eres una persona sana, tienes que tomar aguita que es muy buena.\n+1 Energia\n+1 Recuperacion", is_skin = false, is_passive = true, amount = 1, rarity = "Comun", life = 0, energy = 1, force = 0, recovery = 1, duration = 0 };
        var obj8 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_casco_acolchado", name = "Casco Acolchado", description = "Te noquean facil, prueba a ponerte un casco \"Totalmente Legal\" que no te pillen utilizandolo.", is_skin = false, is_passive = true, amount = 1, rarity = "Epico", life = 50, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj9 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_energetica", name = "Energetica", description = "Bebida de marca blanca. Sabe a jarabe para la tos, pero te mantendrá despierto.", is_skin = false, is_passive = true, amount = 1, rarity = "Ilegal", life = 100, energy = 50, force = 0, recovery = 1, duration = 0 };
        var obj10 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_gafas_de_festival", name = "Gafas de festival", description = "Estas gafas tienen más calle que perro viejo; han vivido más cosas de las que tú vivirás en toda tu vida. Serán tus mejores compañeras en la zona de camping de los festivales.", is_skin = false, is_passive = true, amount = 1, rarity = "Epico", life = 30, energy = 30, force = 0, recovery = 0, duration = 0 };
        var obj11 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_puro_montecristo", name = "Puro Montecristo", description = "Los puros que se metía tu abuelo en las bodas. Te suben la testosterona, pero di adiós a tus pulmones.", is_skin = false, is_passive = true, amount = 1, rarity = "Epico", life = -10, energy = -10, force = 3, recovery = 0, duration = 0 };
        var obj12 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_testigo", name = "Testigo", description = "Conocido por el palo de las carreras de relevos, este \"palo\" te dará la resistencia y recuperación de un atleta, pero no se puede tener todo en la vida. Te vas a quedar con menos fuerza, por espabilao.", is_skin = false, is_passive = true, amount = 1, rarity = "Raro", life = 0, energy = 30, force = -1, recovery = 2, duration = 0 };
        var obj13 = new Objeto { schemaVersion = 1.0, id_Objeto = "Item_LootBox", name = "Lootbox", description = "Una misteriosa caja que es capaz de soltar objetos muy interesantes, parece magia y todo.", is_skin = false, is_passive = false, amount = 1, rarity = "Ilegal", life = 0, energy = 0, force = 0, recovery = 0, duration = 0 };

        await objetosRef.Document(obj1.id_Objeto).SetAsync(obj1);
        await objetosRef.Document(obj2.id_Objeto).SetAsync(obj2);
        await objetosRef.Document(obj3.id_Objeto).SetAsync(obj3);
        await objetosRef.Document(obj4.id_Objeto).SetAsync(obj4);
        await objetosRef.Document(obj5.id_Objeto).SetAsync(obj5);
        await objetosRef.Document(obj6.id_Objeto).SetAsync(obj6);
        await objetosRef.Document(obj7.id_Objeto).SetAsync(obj7);
        await objetosRef.Document(obj8.id_Objeto).SetAsync(obj8);
        await objetosRef.Document(obj9.id_Objeto).SetAsync(obj9);
        await objetosRef.Document(obj10.id_Objeto).SetAsync(obj10);
        await objetosRef.Document(obj11.id_Objeto).SetAsync(obj11);
        await objetosRef.Document(obj12.id_Objeto).SetAsync(obj12);
        await objetosRef.Document(obj13.id_Objeto).SetAsync(obj13);
        
        Debug.Log("-> 13 Objetos globales balanceados subidos.");
    }
}
#endif
