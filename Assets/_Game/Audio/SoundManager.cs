using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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
    Dizzy
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [Header("Control de Volumen Maestro")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Configuración")]
    [SerializeField] private SoundList[] soundList;

    [Header("Música de Fondo")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip BackgroundMusic;

    private static SoundManager instance;
    private AudioSource sfxSource; 

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            if (Application.isPlaying) Destroy(this.gameObject);
            else DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        if (soundList == null) soundList = new SoundList[0];

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
            if (soundItem.Sounds != null && soundItem.Sounds.Length > 0)
            {
                AudioClip randomClip = soundItem.Sounds[UnityEngine.Random.Range(0, soundItem.Sounds.Length)];
                if (instance.sfxSource != null && randomClip != null)
                    instance.sfxSource.PlayOneShot(randomClip, 1f);
            }
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

#if UNITY_EDITOR
    private void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        if (soundList == null) soundList = new SoundList[0];
        if (soundList.Length != names.Length) Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++) soundList[i].name = names[i];
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}