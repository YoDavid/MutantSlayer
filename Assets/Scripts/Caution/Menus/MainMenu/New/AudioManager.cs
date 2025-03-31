using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Libraries")]
    [SerializeField] private List<Sound> sfxLibrary = new List<Sound>();
    [SerializeField] private List<Sound> musicLibrary = new List<Sound>();

    private Dictionary<string, AudioClip> sfxLookup = new Dictionary<string, AudioClip>();
    private Dictionary<string, Sound> musicLookup = new Dictionary<string, Sound>();
    private string currentMusic;
    private float fadeDuration = 1.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionaries();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionaries()
    {
        // SFX initialization
        sfxLookup.Clear();
        foreach (var sound in sfxLibrary)
        {
            if (!sfxLookup.ContainsKey(sound.name))
                sfxLookup.Add(sound.name, sound.clip);
        }

        // Music initialization
        musicLookup.Clear();
        foreach (var track in musicLibrary)
        {
            if (!musicLookup.ContainsKey(track.name))
                musicLookup.Add(track.name, track);
        }
    }

    public void PlayMusic(string trackName, bool forceRestart = false)
    {
        if (musicLookup.TryGetValue(trackName, out Sound track))
        {
            if (!forceRestart && currentMusic == trackName) return;

            currentMusic = trackName;
            StartCoroutine(FadeMusic(track));
        }
    }

    private IEnumerator FadeMusic(Sound newTrack)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // Fade out current music
        while (elapsed < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Switch track
        musicSource.clip = newTrack.clip;
        musicSource.loop = true;
        musicSource.Play();

        // Fade in new music
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(0f, newTrack.volume, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void UpdateMusicByLocation(float xPosition)
    {
        string trackName = xPosition switch
        {
            < 50f => "lab_theme",
            < 100f => "buildings_indoor_theme",
            < 150f => "buildings_outdoor_theme",
            < 200f => "sewer_theme",
            < 250f => "train_station_theme",
            _ => "boss_theme"
        };

        if (trackName != currentMusic)
        {
            PlayMusic(trackName);
        }
    }

    public void StopMusic()
    {
        StartCoroutine(FadeOutMusic());
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 1f; // Reset volume
    }

    // SFX methods remain unchanged
    public void PlaySFX(string soundName, float volume = 1f)
    {
        if (sfxLookup.TryGetValue(soundName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayButtonClick() => PlaySFX("button_click");
    public void PlayButtonHover() => PlaySFX("button_hover");
    public void PlaySlideTransition() => PlaySFX("slide_transition");
}