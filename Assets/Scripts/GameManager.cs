using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed (units per second)")]
    public float startSpeed = 11f;
    public float maxSpeed = 26f;
    public float accelerationPerSecond = 0.12f;
    public float boostMultiplier = 1.5f;

    [Header("Power-up durations (seconds)")]
    public float magnetDuration = 8f;
    public float boostDuration = 5f;

    [Header("Seconds until maximum difficulty")]
    public float secondsToMaxDifficulty = 150f;

    // Set by GameBootstrap
    [HideInInspector] public PlayerController player;
    [HideInInspector] public TrackGenerator track;
    [HideInInspector] public CameraFollow cameraFollow;

    public GameState State { get; private set; }
    public float CurrentSpeed { get; private set; }
    public float Distance { get; private set; }
    public int Coins { get; private set; }
    public int Presents { get; private set; }
    public int Snowflakes { get; private set; }
    public int BestScore { get; private set; }
    public bool NewBest { get; private set; }
    public float MagnetTimer { get; private set; }
    public float BoostTimer { get; private set; }
    public bool ShieldActive { get; private set; }

    public bool MagnetActive { get { return MagnetTimer > 0f; } }
    public bool BoostActive { get { return BoostTimer > 0f; } }
    public int Score { get { return Mathf.FloorToInt(Distance) + Coins * 5 + Presents * 50 + Snowflakes * 25; } }
    public float RunTime { get { return runTime; } }
    public float Difficulty { get { return Mathf.Clamp01(runTime / secondsToMaxDifficulty); } }

    const string BestKey = "WinterRush_BestScore";
    float runTime;

    void Awake()
    {
        Instance = this;
        State = GameState.Menu;
        BestScore = PlayerPrefs.GetInt(BestKey, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }
        if (State != GameState.Playing) return;

        float dt = Time.deltaTime;
        runTime += dt;
        float baseSpeed = Mathf.Min(maxSpeed, startSpeed + runTime * accelerationPerSecond);
        float target = BoostActive ? baseSpeed * boostMultiplier : baseSpeed;
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, target, 4f * dt);
        Distance += CurrentSpeed * dt;

        if (MagnetTimer > 0f) MagnetTimer = Mathf.Max(0f, MagnetTimer - dt);
        if (BoostTimer > 0f) BoostTimer = Mathf.Max(0f, BoostTimer - dt);
    }

    void ResetRun()
    {
        runTime = 0f; Distance = 0f; Coins = 0; Presents = 0; Snowflakes = 0;
        MagnetTimer = 0f; BoostTimer = 0f; ShieldActive = false; NewBest = false;
        CurrentSpeed = 0f;
    }

    void ResetWorld(bool rebuildTrack)
    {
        ResetRun();
        if (player != null) player.ResetPlayer();
        if (track != null && rebuildTrack) track.ResetTrack();
        if (cameraFollow != null) cameraFollow.Snap();
    }

    // ---------- State changes (called by UI buttons) ----------
    public void ShowMenu()
    {
        ResetWorld(true);
        State = GameState.Menu;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        bool rebuild = State != GameState.Menu;   // track is already fresh when coming from the menu
        ResetWorld(rebuild);
        State = GameState.Playing;
        CurrentSpeed = startSpeed;
        Time.timeScale = 1f;
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (State != GameState.Playing) return;
        State = GameState.GameOver;
        CurrentSpeed = 0f;
        int sc = Score;
        NewBest = sc > BestScore;
        if (NewBest)
        {
            BestScore = sc;
            PlayerPrefs.SetInt(BestKey, sc);
            PlayerPrefs.Save();
        }
        Sound(Sfx.Hit);
        if (cameraFollow != null) cameraFollow.Shake(0.5f, 0.3f);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ---------- Pickups ----------
    public void OnCollect(CollectibleType t)
    {
        switch (t)
        {
            case CollectibleType.Coin: Coins++; Sound(Sfx.Coin); break;
            case CollectibleType.Present: Presents++; Sound(Sfx.Present); break;
            case CollectibleType.Snowflake: Snowflakes++; Sound(Sfx.Present); break;
            case CollectibleType.Magnet: MagnetTimer = magnetDuration; Sound(Sfx.PowerUp); break;
            case CollectibleType.Shield: ShieldActive = true; Sound(Sfx.PowerUp); break;
            case CollectibleType.Boost: BoostTimer = boostDuration; Sound(Sfx.PowerUp); break;
        }
    }

    public void ConsumeShield() { ShieldActive = false; Sound(Sfx.Hit); }

    static void Sound(Sfx s)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(s);
    }
}
