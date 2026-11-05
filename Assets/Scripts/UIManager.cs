using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// Builds the whole UI (menu, HUD, pause, game over, settings) from code, so there is nothing to wire up in the Inspector.
public class UIManager : MonoBehaviour
{
    static readonly Color Icy = new Color(0.6f, 0.85f, 1f);
    static readonly Color Gold = new Color(1f, 0.85f, 0.25f);
    static readonly Color BtnGreen = new Color(0.12f, 0.6f, 0.28f);
    static readonly Color BtnRed = new Color(0.8f, 0.14f, 0.18f);
    static readonly Color BtnBlue = new Color(0.2f, 0.5f, 0.85f);

    Font font;
    Canvas canvas;
    GameObject menuPanel, settingsPanel, hudPanel, pausePanel, gameOverPanel;
    Text menuBest, hudScore, hudCoins, hudDistance, hudExtras, hudPower;
    Text goScore, goBest, goNewBest, goCoins;
    Text musicLabel, sfxLabel;
    bool settingsOpen;
    int lastScore = -1, lastCoins = -1, lastDist = -1, lastFlakes = -1, lastPresents = -1;
    string lastPower;

    public void Build()
    {
        font = LoadFont();
        EnsureEventSystem();

        GameObject c = new GameObject("UI Canvas");
        c.transform.SetParent(transform, false);
        canvas = c.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = c.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        c.AddComponent<GraphicRaycaster>();

        BuildMenu();
        BuildSettings();
        BuildHud();
        BuildPause();
        BuildGameOver();
    }

    static Font LoadFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    void EnsureEventSystem()
    {
#if UNITY_2023_1_OR_NEWER
        bool exists = FindAnyObjectByType<EventSystem>() != null;
#else
    bool exists = FindObjectOfType<EventSystem>() != null;
#endif
        if (exists) return;
        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ------------------------------------------------------------ helpers
    RectTransform NewRect(string name, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return rt;
    }

    GameObject Panel(string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = color.a > 0.01f;
        return go;
    }

    Text MakeText(Transform parent, string text, int size, Vector2 anchor, Vector2 pos, Vector2 box, TextAnchor align, Color color)
    {
        RectTransform rt = NewRect("Text", parent, anchor, pos, box);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.text = text; t.fontSize = size; t.alignment = align; t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        Outline o = rt.gameObject.AddComponent<Outline>();
        o.effectColor = new Color(0.05f, 0.15f, 0.35f, 0.9f);
        o.effectDistance = new Vector2(3f, -3f);
        return t;
    }

    Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color color, UnityAction onClick, int fontSize)
    {
        RectTransform rt = NewRect(label + " Button", parent, anchor, pos, size);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        Outline border = rt.gameObject.AddComponent<Outline>();
        border.effectColor = new Color(1f, 1f, 1f, 0.85f);
        border.effectDistance = new Vector2(3f, -3f);
        Button b = rt.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        ColorBlock cb = b.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        b.colors = cb;
        MakeText(rt, label, fontSize, new Vector2(0.5f, 0.5f), Vector2.zero, size, TextAnchor.MiddleCenter, Color.white);
        b.onClick.AddListener(() => { Click(); onClick(); });
        return b;
    }

    static void Click()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Click);
    }

    static void Show(GameObject go, bool visible)
    {
        if (go != null && go.activeSelf != visible) go.SetActive(visible);
    }

    // ------------------------------------------------------------ panels
    void BuildMenu()
    {
        menuPanel = Panel("MenuPanel", new Color(0.04f, 0.1f, 0.22f, 0.5f));
        Transform p = menuPanel.transform;
        MakeText(p, "WINTER RUSH", 170, new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(1500, 230), TextAnchor.MiddleCenter, Color.white);
        MakeText(p, "H O L I D A Y   R U N N E R", 52, new Vector2(0.5f, 1f), new Vector2(0, -300), new Vector2(1200, 80), TextAnchor.MiddleCenter, new Color(0.5f, 1f, 0.6f));
        Vector2 mid = new Vector2(0.5f, 0.5f);
        Vector2 size = new Vector2(540, 120);
        MakeButton(p, "PLAY", mid, new Vector2(0, 60), size, BtnGreen, () => GameManager.Instance.StartGame(), 60);
        MakeButton(p, "SETTINGS", mid, new Vector2(0, -80), size, BtnBlue, () => settingsOpen = true, 56);
        MakeButton(p, "QUIT", mid, new Vector2(0, -220), size, BtnRed, () => GameManager.Instance.QuitGame(), 56);
        menuBest = MakeText(p, "Best Score: 0", 52, new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(900, 70), TextAnchor.MiddleCenter, Gold);
        MakeText(p, "Arrows / A D: change lane     Space / W: jump     Down / S: slide     Esc: pause", 34,
            new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(1600, 50), TextAnchor.MiddleCenter, Icy);
    }

    void BuildSettings()
    {
        settingsPanel = Panel("SettingsPanel", new Color(0.04f, 0.1f, 0.22f, 0.8f));
        Transform p = settingsPanel.transform;
        Vector2 mid = new Vector2(0.5f, 0.5f);
        Vector2 size = new Vector2(620, 110);
        MakeText(p, "SETTINGS", 110, new Vector2(0.5f, 1f), new Vector2(0, -120), new Vector2(1000, 150), TextAnchor.MiddleCenter, Color.white);
        Button m = MakeButton(p, "Music", mid, new Vector2(0, 70), size, BtnBlue, () => { AudioManager.Instance.ToggleMusic(); RefreshSettingsLabels(); }, 50);
        Button s = MakeButton(p, "Sound FX", mid, new Vector2(0, -70), size, BtnBlue, () => { AudioManager.Instance.ToggleSfx(); RefreshSettingsLabels(); }, 50);
        musicLabel = m.GetComponentInChildren<Text>();
        sfxLabel = s.GetComponentInChildren<Text>();
        MakeButton(p, "BACK", mid, new Vector2(0, -230), size, BtnRed, () => settingsOpen = false, 54);
        RefreshSettingsLabels();
    }

    void RefreshSettingsLabels()
    {
        AudioManager a = AudioManager.Instance;
        if (a == null) return;
        musicLabel.text = "Music: " + (a.MusicOn ? "ON" : "OFF");
        sfxLabel.text = "Sound FX: " + (a.SfxOn ? "ON" : "OFF");
    }

    void BuildHud()
    {
        hudPanel = Panel("HudPanel", Color.clear);
        Transform p = hudPanel.transform;
        Vector2 tl = new Vector2(0f, 1f);
        hudScore = MakeText(p, "SCORE  0", 70, tl, new Vector2(40, -30), new Vector2(700, 90), TextAnchor.MiddleLeft, Color.white);
        hudCoins = MakeText(p, "COINS  0", 48, tl, new Vector2(40, -120), new Vector2(700, 60), TextAnchor.MiddleLeft, Gold);
        hudDistance = MakeText(p, "0 m", 42, tl, new Vector2(40, -185), new Vector2(700, 55), TextAnchor.MiddleLeft, Icy);
        hudExtras = MakeText(p, "Snowflakes 0   Presents 0", 36, tl, new Vector2(40, -240), new Vector2(900, 50), TextAnchor.MiddleLeft, new Color(0.8f, 0.95f, 1f));
        hudPower = MakeText(p, "", 46, new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(800, 200), TextAnchor.UpperCenter, new Color(1f, 0.95f, 0.5f));
        MakeButton(p, "II", new Vector2(1f, 1f), new Vector2(-40, -40), new Vector2(110, 110), BtnBlue, () => GameManager.Instance.Pause(), 60);
    }

    void BuildPause()
    {
        pausePanel = Panel("PausePanel", new Color(0.02f, 0.06f, 0.15f, 0.75f));
        Transform p = pausePanel.transform;
        Vector2 mid = new Vector2(0.5f, 0.5f);
        Vector2 size = new Vector2(560, 110);
        MakeText(p, "PAUSED", 130, new Vector2(0.5f, 1f), new Vector2(0, -130), new Vector2(1000, 170), TextAnchor.MiddleCenter, Color.white);
        MakeButton(p, "RESUME", mid, new Vector2(0, 90), size, BtnGreen, () => GameManager.Instance.Resume(), 54);
        MakeButton(p, "RESTART", mid, new Vector2(0, -50), size, BtnBlue, () => GameManager.Instance.StartGame(), 54);
        MakeButton(p, "MAIN MENU", mid, new Vector2(0, -190), size, BtnRed, () => GameManager.Instance.ShowMenu(), 54);
    }

    void BuildGameOver()
    {
        gameOverPanel = Panel("GameOverPanel", new Color(0.1f, 0.03f, 0.08f, 0.72f));
        Transform p = gameOverPanel.transform;
        Vector2 mid = new Vector2(0.5f, 0.5f);
        Vector2 size = new Vector2(560, 110);
        MakeText(p, "GAME OVER", 140, new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(1400, 180), TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.35f));
        goScore = MakeText(p, "Score: 0", 80, mid, new Vector2(0, 230), new Vector2(900, 100), TextAnchor.MiddleCenter, Color.white);
        goBest = MakeText(p, "Best: 0", 56, mid, new Vector2(0, 150), new Vector2(900, 70), TextAnchor.MiddleCenter, Gold);
        goNewBest = MakeText(p, "NEW BEST SCORE!", 54, mid, new Vector2(0, 85), new Vector2(900, 70), TextAnchor.MiddleCenter, new Color(0.5f, 1f, 0.6f));
        goCoins = MakeText(p, "Coins: 0", 46, mid, new Vector2(0, 25), new Vector2(900, 60), TextAnchor.MiddleCenter, Icy);
        MakeButton(p, "RESTART", mid, new Vector2(0, -90), size, BtnGreen, () => GameManager.Instance.StartGame(), 54);
        MakeButton(p, "MAIN MENU", mid, new Vector2(0, -230), size, BtnRed, () => GameManager.Instance.ShowMenu(), 54);
    }

    // ------------------------------------------------------------ per-frame
    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || canvas == null) return;
        GameState s = gm.State;
        if (s != GameState.Menu) settingsOpen = false;

        Show(menuPanel, s == GameState.Menu && !settingsOpen);
        Show(settingsPanel, s == GameState.Menu && settingsOpen);
        Show(hudPanel, s == GameState.Playing || s == GameState.Paused);
        Show(pausePanel, s == GameState.Paused);
        Show(gameOverPanel, s == GameState.GameOver);

        if (s == GameState.Menu) menuBest.text = "Best Score: " + gm.BestScore;
        if (hudPanel.activeSelf) UpdateHud(gm);
        if (s == GameState.GameOver)
        {
            goScore.text = "Score: " + gm.Score;
            goBest.text = "Best: " + gm.BestScore;
            goCoins.text = "Coins: " + gm.Coins + "     Snowflakes: " + gm.Snowflakes + "     Presents: " + gm.Presents;
            goNewBest.gameObject.SetActive(gm.NewBest);
        }
    }

    void UpdateHud(GameManager gm)
    {
        int score = gm.Score;
        if (score != lastScore) { hudScore.text = "SCORE  " + score; lastScore = score; }
        if (gm.Coins != lastCoins) { hudCoins.text = "COINS  " + gm.Coins; lastCoins = gm.Coins; }
        int dist = Mathf.FloorToInt(gm.Distance);
        if (dist != lastDist) { hudDistance.text = dist + " m"; lastDist = dist; }
        if (gm.Snowflakes != lastFlakes || gm.Presents != lastPresents)
        {
            hudExtras.text = "Snowflakes " + gm.Snowflakes + "   Presents " + gm.Presents;
            lastFlakes = gm.Snowflakes; lastPresents = gm.Presents;
        }
        string power = "";
        if (gm.MagnetActive) power += "MAGNET  " + Mathf.CeilToInt(gm.MagnetTimer) + "s\n";
        if (gm.ShieldActive) power += "SHIELD READY\n";
        if (gm.BoostActive) power += "SPEED BOOST  " + Mathf.CeilToInt(gm.BoostTimer) + "s\n";
        if (power != lastPower) { hudPower.text = power; lastPower = power; }
    }
}
