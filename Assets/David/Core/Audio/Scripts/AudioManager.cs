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
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    [System.Serializable]
    public class AudioCategory
    {
        public string name;
        public AudioSource source;
        public List<Sound> sounds = new List<Sound>();
        [HideInInspector] public Dictionary<string, Sound> soundDict;
    }

    [SerializeField] private float musicFadeDuration = 0.5f;


    [Header("Audio Sources")]
    [SerializeField]
    private AudioCategory[] categories = {
        new AudioCategory { name = "UI" },
        new AudioCategory { name = "Environment" },
        new AudioCategory { name = "Player" },
        new AudioCategory { name = "Enemies" },
        new AudioCategory { name = "Boss" }
    };

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<Sound> musicTracks = new List<Sound>();
    private Dictionary<string, Sound> musicDict = new Dictionary<string, Sound>();
    private string currentMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSystem()
    {
        // Initialize music
        foreach (var track in musicTracks)
        {
            if (!musicDict.ContainsKey(track.name))
            {
                musicDict.Add(track.name, track);
            }
        }

        // Initialize SFX categories
        foreach (var category in categories)
        {
            category.soundDict = new Dictionary<string, Sound>();
            foreach (var sound in category.sounds)
            {
                if (!category.soundDict.ContainsKey(sound.name))
                {
                    category.soundDict.Add(sound.name, sound);
                }
            }
        }
    }

    public void PlaySFX(string categoryName, string soundName, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
    {
        foreach (var category in categories)
        {
            if (category.name == categoryName)
            {
                if (category.soundDict.TryGetValue(soundName, out Sound sound))
                {

                    // Log the volume being played for this sound
                    float effectiveVolume = sound.volume * volumeMultiplier;

                    // Immediately play the sound
                    category.source.PlayOneShot(sound.clip, effectiveVolume);
                    category.source.pitch = sound.pitch * pitchMultiplier;
                    return;
                }
                else
                {
                    Debug.LogWarning($"Sound '{soundName}' not found in category '{categoryName}'");
                }
                return;
            }
        }
    }

    public void StopCategory(string categoryName)
    {
        foreach (var category in categories)
        {
            if (category.name == categoryName)
            {
                category.source.Stop();
                return;
            }
        }
    }

    public void PlayMusic(string trackName, bool forceRestart = false)
    {
        if (musicDict.TryGetValue(trackName, out Sound track))
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

        while (elapsed < musicFadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = 0f;

        musicSource.clip = newTrack.clip;
        musicSource.loop = true;
        musicSource.Play();


        elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            musicSource.volume = Mathf.Lerp(0f, newTrack.volume, elapsed / musicFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = newTrack.volume;
    }



    public void UpdateMusicByPosition(float xPosition)
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
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }

    public void SetCategoryVolume(string categoryName, float volume)
    {
        foreach (var category in categories)
        {
            if (category.name == categoryName)
            {
                category.source.volume = Mathf.Clamp(volume, 0f, 1f);
                return;
            }
        }
    }

    //                UI 
    public void PlayButtonClick() => PlaySFX("UI", "button_click");
    public void PlayButtonHover() => PlaySFX("UI", "button_hover");
    public void PlaySlideTransition() => PlaySFX("UI", "slide_transition");
    public void PlayMenuOpen() => PlaySFX("UI", "menu_open");
    public void PlayMenuClose() => PlaySFX("UI", "menu_close");

    //               Environment
    public void PlayBloodParticlesDeathSound() => PlaySFX("Environment", "blood_particles_death_sound");

    //               Player
    public void PlayPlayerTakeHit() => PlaySFX("Player", "take_hit");

}