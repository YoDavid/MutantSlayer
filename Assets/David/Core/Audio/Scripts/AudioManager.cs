using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

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
        [HideInInspector] public float originalVolume; // Store original volume for pause/unpause
    }

    [SerializeField] private float musicFadeDuration = 0.5f;
    [SerializeField] private bool isPaused = false;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioCategory[] categories = {
        new AudioCategory { name = "UI" },
        new AudioCategory { name = "Environment" },
        new AudioCategory { name = "Player" },
        new AudioCategory { name = "PlayerOthers" },
        new AudioCategory { name = "Enemies" },
        new AudioCategory { name = "Boss" },
        new AudioCategory { name = "Status" },
        new AudioCategory { name = "Spikes" }
    };

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private List<Sound> musicTracks = new List<Sound>();
    private Dictionary<string, Sound> musicDict = new Dictionary<string, Sound>();
    private string currentMusic;
    private float musicOriginalVolume; // Store original music volume

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
        musicOriginalVolume = musicSource.volume;

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
            category.originalVolume = category.source.volume;
        }
    }

    public void PlaySFX(string categoryName, string soundName, float volumeMultiplier = 1f, float pitchMultiplier = 1f)
    {
        if (isPaused && categoryName != "UI") return;

        foreach (var category in categories)
        {
            if (category.name == categoryName)
            {
                if (category.soundDict.TryGetValue(soundName, out Sound sound))
                {
                    float effectiveVolume = sound.volume * volumeMultiplier;
                    category.source.PlayOneShot(sound.clip, effectiveVolume);
                    category.source.pitch = sound.pitch * pitchMultiplier;
                    return;
                }
                else
                {
                    Debug.LogWarning($"Sound '{soundName}' not found    in category '{categoryName}'");
                }
                return;
            }
        }
    }

    public void SetPauseState(bool paused)
    {
        if (isPaused == paused) return;

        isPaused = paused;

        if (paused)
        {
            foreach (var category in categories)
            {
                if (category.name != "UI") // Skip UI sounds
                {
                    category.source.Stop();
                }
            }
            // Reduce music volume when paused
            musicSource.volume = 0.115f;
        }
        else
        {
            // Restore music volume when unpaused
            musicSource.volume = 0.175f;

            // If music is not playing, unpause it
            if (!musicSource.isPlaying)
            {
                musicSource.UnPause(); // Unpause music if it's paused
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

        // Fade out current track
        while (elapsed < musicFadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = 0f;

        // Switch to new track and apply pitch
        musicSource.clip = newTrack.clip;
        musicSource.pitch = newTrack.pitch; // Apply pitch from Inspector
        musicSource.loop = true;
        musicSource.Play();

        // Fade in new track
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

    public void StopSound(string categoryName, string soundName = null)
    {
        foreach (var category in categories)
        {
            if (category.name == categoryName)
            {
                if (string.IsNullOrEmpty(soundName))
                {
                    // Stop all sounds in the category
                    category.source.Stop();
                }
                else
                {
                    // Optionally stop a specific sound if necessary
                    if (category.soundDict.ContainsKey(soundName))
                    {
                        // Stop the specific sound (this could be adjusted based on whether you're using one-shot sounds or loops)
                        category.source.Stop();
                    }
                }
                return;
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop all SFX immediately
        StopAllSFX();

        switch (scene.name)
        {
            case "Scene_MainMenu":
                PlayMusic("music_main_menu");
                break;
            case "Scene_SlideShow":
                PlayMusic("music_opening_slideshow");
                break;
            case "Scene_Game":
                StartCoroutine(PlayGameMusicSequence());
                break;
            case "Scene_SlideShowEnding":
                PlayMusic("music_ending_slideshow");
                break;
        }
    }

    private IEnumerator PlayGameMusicSequence()
    {
        while (true) // Infinite loop to alternate between bg and bgv2
        {
            // Play bg (non-looping)
            if (musicDict.TryGetValue("music_game_bg", out Sound track1))
            {
                currentMusic = "music_game_bg";
                musicSource.loop = false; // Disable looping (we control it manually)
                yield return StartCoroutine(FadeMusic(track1));

                while (musicSource.isPlaying)
                    yield return null;
            }

            if (musicDict.TryGetValue("music_game_bgv2", out Sound track2))
            {
                currentMusic = "music_game_bgv2";
                musicSource.loop = false; // Disable looping (we control it manually)
                yield return StartCoroutine(FadeMusic(track2));

                // Wait for bgv2 to finish
                while (musicSource.isPlaying)
                    yield return null;
            }
        }
    }

    private void StopAllSFX()
    {
        // Stop all SFX sources
        foreach (var category in categories)
        {
            category.source.Stop();
        }
    }


    // ====================== UI Sounds ======================
    #region UI - Menus
    public void PlayMenuOpen() => PlaySFX("UI", "sfx_ui_menu_open");
    public void PlayMenuClose() => PlaySFX("UI", "sfx_ui_menu_close");
    public void PlaySlideTransition() => PlaySFX("UI", "sfx_ui_slide_transition");
    public void PlayDeathScreen() => PlaySFX("UI", "sfx_ui_death_screen");
    #endregion

    #region UI - Buttons
    public void PlayButtonClick() => PlaySFX("UI", "sfx_ui_button_click");
    public void PlayButtonHover() => PlaySFX("UI", "sfx_ui_button_hover");
    #endregion

    #region UI - SlideShow
    public void PlayKeyboardSound() => PlaySFX("UI", "sfx_ui_keyboard_sound");
    public void PlayKeyboardSound(float volume) => PlaySFX("UI", "sfx_ui_keyboard_sound", volume);
    #endregion


    // ====================== Environment Sounds ======================
    #region Environment - Effects
    public void PlayBloodParticlesDeathSound() => PlaySFX("Environment", "sfx_env_blood_particles_death");
    public void PlayProjectileHit() => PlaySFX("Environment", "sfx_env_projectile_hit");
    #endregion

    #region Environment - Healing
    public void PlayHealingSound() => PlaySFX("Environment", "sfx_env_healing");
    public void PlayCreatingHeal() => PlaySFX("Environment", "sfx_env_heal_creation");
    #endregion


    // ====================== Player Sounds ======================
    #region Player - Combat: Swing Sounds
    public void PlaySwing_00() => PlaySFX("PlayerOthers", "sfx_player_attack_swing_00");
    public void PlaySwing_01() => PlaySFX("PlayerOthers", "sfx_player_attack_swing_01");
    public void PlaySwing_02() => PlaySFX("PlayerOthers", "sfx_player_attack_swing_02");

    public void PlayPlayerAttackSwing(int attackIndex)
    {
        attackIndex = Mathf.Clamp(attackIndex, 0, 2);
        string soundName = $"sfx_player_attack_swing_0{attackIndex}";
        PlaySFX("PlayerOthers", soundName);
    }
    #endregion

    #region Player - Combat: Combo Slashes
    public void PlayComboSlashLoop() => PlaySFX("PlayerOthers", "sfx_player_combo_slash_loop");
    public void PlayComboFinalHit() => PlaySFX("Player", "sfx_player_combo_final_hit");
    public void PlayEarlyComboExit() => PlaySFX("Player", "sfx_player_attack_early_exit");

    #endregion

    #region Player - Combat: Attack Hits
    public void PlaySlash_Hit00() => PlaySFX("Player", "sfx_player_attack_hit_00");
    public void PlaySlash_Hit01() => PlaySFX("Player", "sfx_player_attack_hit_01");
    public void PlaySlash_Hit02() => PlaySFX("Player", "sfx_player_attack_hit_02");
    public void PlaySlash_Hit03() => PlaySFX("Player", "sfx_player_attack_hit_03");
    #endregion

    #region Player - Combat: Taking Damage
    public void PlayPlayer_TakeHit00() => PlaySFX("Player", "sfx_player_take_hit_00");
    public void PlayPlayer_TakeHit01() => PlaySFX("Player", "sfx_player_take_hit_01");
    public void PlayPlayer_TakeHit02() => PlaySFX("Player", "sfx_player_take_hit_02");
    #endregion

    #region Player - Combat: Charging
    public void PlayerChargingRangeAttack() => PlaySFX("Player", "sfx_player_charge_range_attack");
    public void PlayerChargingSwordDraw() => PlaySFX("PlayerOthers", "sfx_player_charge_sword_draw");
    public void PlayerChargingClimax() => PlaySFX("PlayerOthers", "sfx_player_charge_climax");
    public void PlayPlayerChargeElectricityLoop() => PlaySFX("Player", "sfx_player_charge_electricity_loop");
    #endregion

    #region Player - Movement
    public void PlaySteps() => PlaySFX("PlayerOthers", "sfx_player_footsteps");
    public void PlayDash() => PlaySFX("Player", "sfx_player_dash");
    #endregion

    #region Player - Reactions
    public void PlayHealingGrunt() => PlaySFX("Player", "sfx_player_healing_grunt");
    #endregion

    #region Player - Projectiles
    public void PlayPlayerProjectileSound() => PlaySFX("PlayerOthers", "sfx_player_projectile");
    public void PlayPlayerReleaseRangeGrunt() => PlaySFX("Player", "sfx_player_release_range_grunt");
    #endregion

    // ====================== Player - State Effects ======================
    #region Player - State Effects
    public void PlayHeartbeatLowHP() => PlaySFX("Status", "sfx_player_heartbeat_lowhp");
    #endregion



    // ====================== Enemy Sounds ======================
    #region Enemies - Small
    public void PlaySmallEnemyScream() => PlaySFX("Enemies", "sfx_enemy_small_scream");
    public void PlaySmallEnemyAttack() => PlaySFX("Enemies", "sfx_enemy_small_attack");
    public void PlaySmallEnemyTakeHit() => PlaySFX("Enemies", "sfx_enemy_small_take_hit");
    public void PlaySmallEnemyLastScream() => PlaySFX("Enemies", "sfx_enemy_small_death_scream");
    #endregion

    #region Enemies - Medium
    public void PlayMediumEnemyScream() => PlaySFX("Enemies", "sfx_enemy_medium_scream");
    public void PlayMediumEnemyTakeHit() => PlaySFX("Enemies", "sfx_enemy_medium_take_hit");
    public void PlayMediumEnemyLastScream() => PlaySFX("Enemies", "sfx_enemy_medium_death_scream");
    #endregion


    // ====================== Boss Sounds ======================
    #region Boss - Movement
    public void PlayBossWalking() => PlaySFX("Boss", "sfx_boss_walk");
    #endregion

    #region Boss - Voice / Roar
    public void PlayBossScream_00() => PlaySFX("Boss", "sfx_boss_scream_00");
    public void PlayBossScream_01() => PlaySFX("Boss", "sfx_boss_scream_01");
    public void PlayBossScream_02() => PlaySFX("Boss", "sfx_boss_scream_02");
    public void PlayBossScream_03() => PlaySFX("Boss", "sfx_boss_scream_03");
    #endregion

    #region Boss - Attacks
    public void PlayComboAttackBoss() => PlaySFX("Boss", "sfx_boss_attack_combo");
    public void PlayAOEAttackBoss() => PlaySFX("Boss", "sfx_boss_attack_aoe");
    public void PlayJumpAttackBoss() => PlaySFX("Boss", "sfx_boss_attack_jump");
    public void PlaySpitAttackBoss() => PlaySFX("Boss", "sfx_boss_attack_spit");
    #endregion

    // ====================== Spikes Sounds ======================
    #region Spikes - Traps
    public void PlaySpikeHit() => PlaySFXWithRandomPitch("Spikes", "sfx_fallin_spike_hit");
    #endregion

    // ====================== Music ======================
    #region Music - Themes
    public void PlayMusicMainMenu() => PlayMusic("music_main_menu");
    public void PlayMusicOpeningSlideshow() => PlayMusic("music_opening_slideshow");
    public void PlayMusicGameBackground() => PlayMusic("music_game_bg");
    public void PlayMusicGameBackgroundV2() => PlayMusic("music_game_bgv2");
    public void PlayMusicBossBattle() => PlayMusic("music_boss_battle");
    public void PlayMusicEndingSlideshow() => PlayMusic("music_ending_slideshow");
    #endregion

}