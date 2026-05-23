const functions = require("firebase-functions");
const admin = require("firebase-admin");

// Inicializa la app de Firebase Admin SDK
admin.initializeApp();

/**
 * Función programada que se ejecuta cada hora.
 * Busca usuarios cuya última recompensa fue hace más de 24 horas y les envía un Push Notification.
 */
exports.checkDailyRewards = functions.pubsub.schedule("every 1 hours").onRun(async (context) => {
    const db = admin.firestore();
    
    // Calcular el timestamp exacto de hace 24 horas
    const ahora = new Date();
    const hace24Horas = new Date(ahora.getTime() - (24 * 60 * 60 * 1000));
    
    console.log(`[Recompensas] Buscando usuarios cuya última recompensa sea ANTERIOR a: ${hace24Horas}`);

    try {
        // Consultar a todos los usuarios que cobraron por última vez hace más de 24h
        const snapshot = await db.collection("usuarios")
            .where("last_daily_reward", "<=", hace24Horas)
            .get();

        if (snapshot.empty) {
            console.log("[Recompensas] Nadie tiene recompensas pendientes.");
            return null;
        }

        const tokens = [];

        // Recopilar todos los tokens FCM válidos de los usuarios
        snapshot.forEach((doc) => {
            const userData = doc.data();
            // Asegurarse de que tienen un token y no se les haya notificado ya hoy (opcional: añadir bandera)
            if (userData.fcm_token && userData.fcm_token !== "") {
                tokens.push(userData.fcm_token);
            }
        });

        if (tokens.length === 0) {
            console.log("[Recompensas] Se encontraron usuarios elegibles, pero ninguno tiene fcm_token.");
            return null;
        }

        // Crear el mensaje de la notificación
        const message = {
            notification: {
                title: "¡Recompensa Diaria Lista!",
                body: "¡Entra ahora para reclamar tu premio diario antes de perder la racha!"
            },
            tokens: tokens // Multicast: envía a todos los tokens de golpe
        };

        // Enviar las notificaciones a través de Firebase Cloud Messaging
        const response = await admin.messaging().sendEachForMulticast(message);
        
        console.log(`[Recompensas] Se han enviado ${response.successCount} notificaciones correctamente. Fallaron ${response.failureCount}.`);
        
        return null;

    } catch (error) {
        console.error("[Recompensas] Error al enviar notificaciones:", error);
        return null;
    }
});

// =======================================================
// FUNCIONES DE RECOMPENSA DE MONEDAS
// =======================================================

async function darMonedas(uid, cantidad) {
    if (!uid) {
        throw new functions.https.HttpsError(
            'unauthenticated', 
            'El usuario debe estar autenticado para recibir recompensas.'
        );
    }

    const usuarioRef = admin.firestore().collection('usuarios').doc(uid);
    
    try {
        await usuarioRef.update({
            free_coin: admin.firestore.FieldValue.increment(cantidad)
        });
        
        return { 
            success: true, 
            mensaje: `Se han añadido ${cantidad} monedas exitosamente.`,
            monedas_otorgadas: cantidad
        };
    } catch (error) {
        console.error("Error al dar monedas:", error);
        throw new functions.https.HttpsError('internal', 'Error al actualizar las monedas en la base de datos.');
    }
}

exports.recompensa25 = functions.https.onCall(async (data, context) => {
    return darMonedas(context.auth?.uid, 25);
});

exports.recompensa50 = functions.https.onCall(async (data, context) => {
    return darMonedas(context.auth?.uid, 50);
});

exports.recompensa200 = functions.https.onCall(async (data, context) => {
    return darMonedas(context.auth?.uid, 200);
});

exports.recompensa300 = functions.https.onCall(async (data, context) => {
    return darMonedas(context.auth?.uid, 300);
});
