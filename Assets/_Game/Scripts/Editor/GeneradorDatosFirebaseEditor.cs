#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Firebase;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GeneradorDatosFirebaseEditor
{
    [MenuItem("Tools/Firebase/Subir Datos Tienda Prueba")]
    public static async void SubirDatos()
    {
        Debug.Log("Iniciando conexión con Firebase...");
        
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Error al iniciar Firebase: " + dependencyStatus);
            return;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        
        await SubirObjetosGlobales(db);
        await SubirTienda(db);
        
        Debug.Log("¡✅ TODOS LOS DATOS (12 ITEMS) SUBIDOS A FIREBASE CORRECTAMENTE!");
    }

    private static async Task SubirObjetosGlobales(FirebaseFirestore db)
    {
        CollectionReference objetosRef = db.Collection("objetos_globales");

        var obj1 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_activo_isotonica", name = "Bebida Isotónica", description = "Bebida consumida mayormente por crossfiteros. Te ayudará a recuperarte de la paliza del adversario.\nCura 40 de salud.", is_skin = false, is_passive = false, amount = 1, rarity = "Raro", life = 40, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj2 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_chicle_rancio", name = "Chicle Rancio", description = "Este chicle, que tiene aspecto de haberse caído al suelo, parece haber adquirido efectos curativos.\nCura 20 de salud.", is_skin = false, is_passive = false, amount = 1, rarity = "Comun", life = 20, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj3 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_clembuterol", name = "Clembuterol", description = "Estas cansado de ser un flojeras, tomate esto para estar to fuerte y recuperarte de los entrenos.\n+1 de fuerza\n+1 de recuperacion", is_skin = false, is_passive = false, amount = 1, rarity = "Ilegal", life = 0, energy = 0, force = 1, recovery = 1, duration = 0 };
        var obj4 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_kebab_mixto", name = "Kebab Mixto", description = "El alimento de los dioses y tu mejor amigo cuando sales de fiesta. Te dejará como nuevo. Eso sí, saluda de mi parte al váter cuando lo visites.\nCura 100 de vida", is_skin = false, is_passive = false, amount = 1, rarity = "Legendario", life = 100, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj5 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_preentreno", name = "Preentreno", description = "Si lo que quieres es rendir a tope, esto es perfecto. Eso sí, no te pases con la dosis o prepárate para las consecuencias.\n+10 de enegia\n+1 de fuerza", is_skin = false, is_passive = false, amount = 1, rarity = "Raro", life = 0, energy = 10, force = 1, recovery = 0, duration = 0 };
        var obj6 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_seta_sospechosa", name = "Seta Sospechosa", description = "Una seta que parece salida de un juego de un fontanero que rescata princesas. Al consumirla, te sentirás invencible.\n+20 de enegia\n+1 de fuerza\n-100 de vida", is_skin = false, is_passive = false, amount = 1, rarity = "Epico", life = -100, energy = 20, force = 1, recovery = 0, duration = 0 };
        var obj7 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_aguita_fresca_pasivo", name = "Aguita fresca", description = "Si tienes sed y eres una persona sana, tienes que tomar aguita que es muy buena.\n+1 Energia\n+1 Recuperacion", is_skin = false, is_passive = true, amount = 1, rarity = "Comun", life = 0, energy = 1, force = 0, recovery = 1, duration = 0 };
        var obj8 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_casco_acolchado", name = "Casco Acolchado", description = "Te noquean facil, prueba a ponerte un casco \"Totalmente Legal\" que no te pillen utilizandolo.", is_skin = false, is_passive = true, amount = 1, rarity = "Epico", life = 30, energy = 0, force = 0, recovery = 0, duration = 0 };
        var obj9 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_energetica", name = "Energetica", description = "Bebida de marca blanca. Sabe a jarabe para la tos, pero te mantendrá despierto.", is_skin = false, is_passive = true, amount = 1, rarity = "Ilegal", life = 200, energy = 10, force = 0, recovery = 0, duration = 0 };
        var obj10 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_gafas_de_festival", name = "Gafas de festival", description = "Estas gafas tienen más calle que perro viejo; han vivido más cosas de las que tú vivirás en toda tu vida. Serán tus mejores compañeras en la zona de camping de los festivales.", is_skin = false, is_passive = true, amount = 1, rarity = "Epico", life = 30, energy = 20, force = 0, recovery = 0, duration = 0 };
        var obj11 = new Objeto { schemaVersion = 1.0, id_Objeto = "item_pasivo_puro_montecristo", name = "Puro Montecristo", description = "Los puros que se metía tu abuelo en las bodas. Te suben la testosterona, pero di adiós a tus pulmones.", is_skin = false, is_passive = true, amount = 1, rarity = "Epico", life = 0, energy = -40, force = 2, recovery = 0, duration = 0 };
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
        
        Debug.Log("-> 13 Objetos globales subidos.");
    }

    private static async Task SubirTienda(FirebaseFirestore db)
    {
        CollectionReference tiendaRef = db.Collection("tienda");

        var oferta1 = new Dictionary<string, object> { { "id_oferta", "oferta_isotonica" }, { "id_Objeto", "item_activo_isotonica" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta2 = new Dictionary<string, object> { { "id_oferta", "oferta_chicle" }, { "id_Objeto", "item_chicle_rancio" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta3 = new Dictionary<string, object> { { "id_oferta", "oferta_clembuterol" }, { "id_Objeto", "item_clembuterol" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta4 = new Dictionary<string, object> { { "id_oferta", "oferta_kebab" }, { "id_Objeto", "item_kebab_mixto" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta5 = new Dictionary<string, object> { { "id_oferta", "oferta_preentreno" }, { "id_Objeto", "item_preentreno" }, { "precio_monedas", 500 }, { "en_venta", false } };
        var oferta6 = new Dictionary<string, object> { { "id_oferta", "oferta_seta" }, { "id_Objeto", "item_seta_sospechosa" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta7 = new Dictionary<string, object> { { "id_oferta", "oferta_aguita" }, { "id_Objeto", "item_pasivo_aguita_fresca_pasivo" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta8 = new Dictionary<string, object> { { "id_oferta", "oferta_casco" }, { "id_Objeto", "item_pasivo_casco_acolchado" }, { "precio_monedas", 0 }, { "en_venta", false } };
        var oferta9 = new Dictionary<string, object> { { "id_oferta", "oferta_energetica" }, { "id_Objeto", "item_pasivo_energetica" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta10 = new Dictionary<string, object> { { "id_oferta", "oferta_gafas" }, { "id_Objeto", "item_pasivo_gafas_de_festival" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta11 = new Dictionary<string, object> { { "id_oferta", "oferta_puro" }, { "id_Objeto", "item_pasivo_puro_montecristo" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta12 = new Dictionary<string, object> { { "id_oferta", "oferta_testigo" }, { "id_Objeto", "item_pasivo_testigo" }, { "precio_monedas", 100 }, { "en_venta", false } };
        var oferta13 = new Dictionary<string, object> { { "id_oferta", "oferta_caja_misteriosa" }, { "id_Objeto", "Item_LootBox" }, { "precio_monedas", 1000 }, { "en_venta", false } };

        await tiendaRef.Document("oferta_isotonica").SetAsync(oferta1);
        await tiendaRef.Document("oferta_chicle").SetAsync(oferta2);
        await tiendaRef.Document("oferta_clembuterol").SetAsync(oferta3);
        await tiendaRef.Document("oferta_kebab").SetAsync(oferta4);
        await tiendaRef.Document("oferta_preentreno").SetAsync(oferta5);
        await tiendaRef.Document("oferta_seta").SetAsync(oferta6);
        await tiendaRef.Document("oferta_aguita").SetAsync(oferta7);
        await tiendaRef.Document("oferta_casco").SetAsync(oferta8);
        await tiendaRef.Document("oferta_energetica").SetAsync(oferta9);
        await tiendaRef.Document("oferta_gafas").SetAsync(oferta10);
        await tiendaRef.Document("oferta_puro").SetAsync(oferta11);
        await tiendaRef.Document("oferta_testigo").SetAsync(oferta12);
        await tiendaRef.Document("oferta_caja_misteriosa").SetAsync(oferta13);

        Debug.Log("-> Catálogo de tienda (13 items) subido.");
    }
}
#endif
