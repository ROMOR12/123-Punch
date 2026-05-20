using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class Personaje
{
    [FirestoreProperty] public double schemaVersion { get; set; }
    [FirestoreProperty] public string id { get; set; }
    [FirestoreProperty] public string name { get; set; }

    // --- STATS REALES (Los que usa el combate) ---
    [FirestoreProperty] public int life { get; set; }
    [FirestoreProperty] public int energy { get; set; }
    [FirestoreProperty] public int force { get; set; }
    [FirestoreProperty] public int recovery { get; set; }

    // --- CONTADORES DE MEJORA (Para calcular el coste) ---
    [FirestoreProperty] public int mejoras_vida { get; set; } = 0;
    [FirestoreProperty] public int mejoras_energia { get; set; } = 0;
    [FirestoreProperty] public int mejoras_fuerza { get; set; } = 0;
    [FirestoreProperty] public int mejoras_recuperacion { get; set; } = 0;

    // Nivel visual del personaje
    public int NivelTotal => 1 + mejoras_vida + mejoras_energia + mejoras_fuerza + mejoras_recuperacion;

    public override string ToString()
    {
        return name;
    }
}