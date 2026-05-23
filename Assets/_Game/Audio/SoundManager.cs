using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// esta enumeracion define los diferentes tipos de efectos de sonido del juego
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

// esta enumeracion define los efectos de sonido de la interfaz de usuario
public enum UiSoundType
{
    CLICK
}

// esta clase gestiona los diferentes canales de audio y la reproduccion de musica y efectos en el juego
[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [Header("Control de Volumen Maestro")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Configuración SFX(juego)")]
    // esta variable almacena la lista de efectos de sonido del juego
    [SerializeField] private SoundList[] soundList;

    [Header("Configuracion Sonidos UI")]
    // esta variable almacena la lista de sonidos exclusivos de la interfaz
    [SerializeField] private SoundList[] uiSoundList;

    [Header("Música de Fondo")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip BackgroundMusic;

    // esta variable guarda la instancia unica del gestor para acceder desde otros scripts
    private static SoundManager instance;
    private AudioSource sfxSource;

    private void Awake()
    {
        // esta funcion inicializa el patron singleton y carga la configuracion guardada del volumen
        if (instance != null && instance != this)
        {
            if (Application.isPlaying) Destroy(this.gameObject);
            else DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(this.gameObject); // esta funcion hace que el gestor persista al cambiar de escena
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }
    }

    private void Start()
    {
        // esta funcion inicializa las referencias de los componentes de audio
        if (soundList == null) soundList = new SoundList[0];
        if (uiSoundList == null) uiSoundList = new SoundList[0];

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
        // esta funcion actualiza los volumenes en tiempo real desde el inspector
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

    // esta funcion reproduce un efecto de sonido especifico con un retraso opcional
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

            // esta funcion selecciona un clip aleatorio si hay varios configurados para evitar la repeticion
            if (soundItem.Sounds != null && soundItem.Sounds.Length > 0)
            {
                AudioClip randomClip = soundItem.Sounds[UnityEngine.Random.Range(0, soundItem.Sounds.Length)];
                if (instance.sfxSource != null && randomClip != null)
                    instance.sfxSource.PlayOneShot(randomClip, 1f);
            }
        }
    }

    // esta funcion reproduce un sonido especifico de la interfaz de usuario
    public static void PlayUiSound(UiSoundType sound)
    {
        if (instance == null || instance.uiSoundList == null) return;

        if ((int)sound < instance.uiSoundList.Length)
        {
            SoundList soundItem = instance.uiSoundList[(int)sound];

            PlayClipFromItem(soundItem);
        }
    }

    // esta funcion reproduce un clip de sonido aleatorio de una lista de sonidos dada
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

#if UNITY_EDITOR
    private void OnEnable()
    {
        // esta funcion sincroniza las listas del inspector al modificar el script
        SyncList(ref soundList, typeof(SoundType));
        SyncList(ref uiSoundList, typeof(UiSoundType));
    }

    // esta funcion ajusta el tamano y nombre de las listas segun los tipos de enumerados
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

// esta clase define una estructura de datos para agrupar clips de audio en el editor
[Serializable]
public class SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}