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
        new AudioCategory { name = "PlayerOthers" },
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

    public void PlaySFXWithRandomPitch(string categoryName, string soundName, float volumeMultiplier = 1f)
    {
        float randomPitch = Random.Range(0.8f, 1.2f); // Randomize pitch between 0.8 and 1.2
        PlaySFX(categoryName, soundName, volumeMultiplier, randomPitch);
    }

    //                UI 
    public void PlayButtonClick() => PlaySFX("UI", "button_click");
    public void PlayButtonHover() => PlaySFX("UI", "button_hover");
    public void PlaySlideTransition() => PlaySFX("UI", "slide_transition");
    public void PlayMenuOpen() => PlaySFX("UI", "menu_open");
    public void PlayMenuClose() => PlaySFX("UI", "menu_close");
    public void PlayDeathScreen() => PlaySFX("UI", "death_Screen");

    //               Environment
    public void PlayBloodParticlesDeathSound() => PlaySFX("Environment", "blood_particles_death_sound");
    public void PlayHealingSound() => PlaySFX("Environment", "healing_sound");
    public void PlayCreatingHeal() => PlaySFX("Environment", "creating_heal");

    //               Player taking hit
    public void PlayPlayer_TakeHit01() => PlaySFX("Player", "take_hit_01");   // make random takehit sounds 
    public void PlayPlayer_TakeHit02() => PlaySFX("Player", "take_hit_02");   // make random takehit sounds 
    public void PlayPlayer_TakeHit03() => PlaySFX("Player", "take_hit_03");   // make random takehit sounds 

    //               Player hit enemy
    public void PlaySlash_Hit01() => PlaySFX("Player", "slash_hit_01");   
    public void PlaySlash_Hit02() => PlaySFX("Player", "slash_hit_02");   
    public void PlaySlash_Hit03() => PlaySFX("Player", "slash_hit_03");   
    public void PlaySlash_Hit04() => PlaySFX("Player", "slash_hit_04");

    //               Player swing sword
    public void PlaySwing_00() => PlaySFX("PlayerOthers", "player_swing_00");
    public void PlaySwing_01() => PlaySFX("PlayerOthers", "player_swing_01");
    public void PlaySwing_02() => PlaySFX("PlayerOthers", "player_swing_02");

    public void PlayPlayerAttackSwing(int attackIndex)
    {
        // Clamp to valid range just in case
        attackIndex = Mathf.Clamp(attackIndex, 0, 2);

        string soundName = $"player_swing_0{attackIndex}";
        PlaySFX("PlayerOthers", soundName);
        Debug.Log("Play");
    }

    //               Player steps
    public void PlaySteps() => PlaySFX("PlayerOthers", "player_steps");

    public void PlayHealingGrunt() => PlaySFX("Player", "player_grunt");

    public void PlayDash() => PlaySFX("Player", "player_dash");



    //               Enemies
    public void PlayMediumEnemyScream() => PlaySFX("Enemies", "scream_medium_enemy");
    public void PlayMediumEnemyLastScream() => PlaySFX("Enemies", "last_scream_medium_enemy");
    public void PlayMediumEnemyTakeHit() => PlaySFX("Enemies", "takehit_medium_enemy");

    public void PlaySmallEnemyScream() => PlaySFX("Enemies", "scream_small_enemy");
    public void PlaySmallEnemyTakeHit() => PlaySFX("Enemies", "takehit_small_enemy");
    public void PlaySmallEnemyLastScream() => PlaySFX("Enemies", "last_scream_small_enemy");


    //               Boss
    public void PlayBossWalking() => PlaySFX("Boss", "walking_boss");
    


    public void PlayBossScream_00() => PlaySFX("Boss", "scream00_boss");
    public void PlayBossScream_01() => PlaySFX("Boss", "scream01_boss");
    public void PlayBossScream_02() => PlaySFX("Boss", "scream02_boss");
    public void PlayBossScream_03() => PlaySFX("Boss", "scream03_boss");


    public void PlayComboAttackBoss() => PlaySFX("Boss", "comboattack_boss");
    public void PlayAOEAttackBoss() => PlaySFX("Boss", "aoeattack_boss");
    public void PlayJumpAttackBoss() => PlaySFX("Boss", "jumpattack_boss");
    public void PlaySpitAttackBoss() => PlaySFX("Boss", "spitattack_boss");
}