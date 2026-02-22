using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Catálogos cerrados con todos los sonidos. 
public enum SoundType
{
    HIT,
    StrongHit,
    GuardStrike,
    Win,
    Lose,
    Consumable,
    EnemyComplaint,
    Complaint,
    BrokeGuard,
    Dizzy,
    Dodge
}

public enum UiSoundType
{
    CLICK
}

// Gestor central de audio.
[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [Header("Control de Volumen Maestro")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Configuración SFX(juego)")]
    // Listas que se auto-rellenan basándose en los Enums de arriba
    [SerializeField] private SoundList[] soundList;

    [Header("Configuracion Sonidos UI")]
    [SerializeField] private SoundList[] uiSoundList;

    [Header("Música de Fondo")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip BackgroundMusic;

    // Patrón Singleton: Permite llamar al SoundManager desde cualquier script sin buscar referencias
    private static SoundManager instance;
    private AudioSource sfxSource;

    private void Awake()
    {
        // Asegura que solo exista UN SoundManager en todo el juego
        if (instance != null && instance != this)
        {
            if (Application.isPlaying) Destroy(this.gameObject);
            else DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(this.gameObject); // Sobrevive al cambiar de escena
        }
    }

    private void Start()
    {
        // Inicialización de seguridad para las listas
        if (soundList == null) soundList = new SoundList[0];
        if (uiSoundList == null) uiSoundList = new SoundList[0];

        // Autoconfiguración de los canales de audio (Efectos y Música)
        AudioSource[] allSources = GetComponents<AudioSource>();

        if (allSources.Length > 0)
        {
            sfxSource = allSources[0];
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        if (allSources.Length > 1)
        {
            musicSource = allSources[1];
        }
        else if (Application.isPlaying)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;

            if (Application.isPlaying && BackgroundMusic != null)
            {
                musicSource.clip = BackgroundMusic;
                musicSource.Play();
            }
        }
    }

    private void Update()
    {
        // Actualiza el volumen en tiempo real desde el inspector
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        if (sfxSource != null && sfxSource != musicSource)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public static void PlayBackgroundMusic()
    {
        if (instance != null && instance.BackgroundMusic != null)
            PlayMusic(instance.BackgroundMusic);
    }

    // Reproduce un efecto. Soporta delay asíncrono para sincronizar con animaciones
    public static async void PlaySound(SoundType sound, float delay = 0f)
    {
        if (instance == null || instance.soundList == null) return;

        if (delay > 0f)
        {
            await System.Threading.Tasks.Task.Delay((int)(delay * 1000));
        }

        if (instance.soundList.Length > (int)sound)
        {
            SoundList soundItem = instance.soundList[(int)sound];

            // Si hay varios clips para un mismo sonido, elige uno al azar para dar variedad
            if (soundItem.Sounds != null && soundItem.Sounds.Length > 0)
            {
                AudioClip randomClip = soundItem.Sounds[UnityEngine.Random.Range(0, soundItem.Sounds.Length)];
                if (instance.sfxSource != null && randomClip != null)
                    instance.sfxSource.PlayOneShot(randomClip, 1f);
            }
        }
    }

    // Hace exactamente lo mismo que PlaySound, pero busca en la lista de sonidos de la Interfaz.
    public static void PlayUiSound(UiSoundType sound)
    {
        if (instance == null || instance.uiSoundList == null) return;

        // Validación de índice
        if ((int)sound < instance.uiSoundList.Length)
        {
            SoundList soundItem = instance.uiSoundList[(int)sound];

            // Reutiliza la función auxiliar para reproducir el clip
            PlayClipFromItem(soundItem);
        }
    }

    // Método auxiliar para no repetir código.
    // Recibe una lista de sonidos, elige uno al azar para dar variedad, y lo reproduce.
    private static void PlayClipFromItem(SoundList item)
    {
        if (item.Sounds != null && item.Sounds.Length > 0)
        {
            AudioClip randomClip = item.Sounds[UnityEngine.Random.Range(0, item.Sounds.Length)];
            if (instance.sfxSource != null && randomClip != null)
                instance.sfxSource.PlayOneShot(randomClip, 1f);
        }
    }

    public static void PlayMusic(AudioClip musicClip)
    {
        if (instance == null || instance.musicSource == null) return;
        instance.musicSource.clip = musicClip;
        instance.musicSource.Play();
    }

    public static void StopMusic()
    {
        if (instance != null && instance.musicSource != null)
            instance.musicSource.Stop();
    }

    public static void ResumeMusic()
    {
        if (instance != null && instance.musicSource != null)
        {
            instance.musicSource.UnPause();
        }
    }

    public static void PauseMusic()
    {
        if (instance != null && instance.musicSource != null)
        {
            instance.musicSource.Pause();
        }
    }

    // --- HERRAMIENTAS DE EDITOR ---
#if UNITY_EDITOR
    private void OnEnable()
    {
        // Al modificar el script, sincroniza el Inspector con los Enums
        SyncList(ref soundList, typeof(SoundType));
        SyncList(ref uiSoundList, typeof(UiSoundType));
    }

    // Redimensiona y nombra los arrays automáticamente basándose en los Enums
    private void SyncList(ref SoundList[] list, Type enumType)
    {
        string[] names = Enum.GetNames(enumType);
        if (list == null) list = new SoundList[0];

        if (list.Length != names.Length)
            Array.Resize(ref list, names.Length);

        for (int i = 0; i < list.Length; i++)
            list[i].name = names[i];
    }
#endif
}

// Estructura auxiliar para poder agrupar un array de AudioClips bajo un mismo nombre en el Inspector
[Serializable]
public class SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}