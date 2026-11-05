using UnityEngine;

public enum Sfx { Coin, Jump, Slide, Hit, PowerUp, Present, Click }

/// Generates simple placeholder sounds + a looping holiday-style melody in code, so no audio files are needed.
/// To use your own audio: put clips in Assets/Resources/Audio/ named Music, Coin, Jump, Slide, Hit, PowerUp, Present, Click.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public bool MusicOn { get; private set; }
    public bool SfxOn { get; private set; }

    const int Rate = 44100;
    AudioSource musicSource, sfxSource;
    AudioClip music, coin, jump, slide, hit, power, present, click;

    void Awake()
    {
        Instance = this;
        MusicOn = PlayerPrefs.GetInt("WR_Music", 1) == 1;
        SfxOn = PlayerPrefs.GetInt("WR_Sfx", 1) == 1;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true; musicSource.playOnAwake = false; musicSource.volume = 0.3f;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false; sfxSource.volume = 0.8f;

        coin = Load("Coin", Notes("coin", new float[] { 988f, 1319f }, 0.07f, 0.5f));
        jump = Load("Jump", Sweep("jump", 300f, 650f, 0.18f, 0.5f, false));
        slide = Load("Slide", Sweep("slide", 500f, 150f, 0.2f, 0.4f, true));
        hit = Load("Hit", Sweep("hit", 160f, 50f, 0.45f, 0.8f, true));
        power = Load("PowerUp", Notes("power", new float[] { 523f, 659f, 784f, 1047f }, 0.08f, 0.5f));
        present = Load("Present", Notes("present", new float[] { 784f, 1047f, 1319f }, 0.06f, 0.45f));
        click = Load("Click", Notes("click", new float[] { 700f }, 0.05f, 0.4f));
        music = Load("Music", BuildMusic());

        musicSource.clip = music;
        musicSource.mute = !MusicOn;
        musicSource.Play();
    }

    static AudioClip Load(string name, AudioClip fallback)
    {
        AudioClip c = Resources.Load<AudioClip>("Audio/" + name);
        return c != null ? c : fallback;
    }

    public void Play(Sfx s)
    {
        if (!SfxOn) return;
        AudioClip c = null;
        switch (s)
        {
            case Sfx.Coin: c = coin; break;
            case Sfx.Jump: c = jump; break;
            case Sfx.Slide: c = slide; break;
            case Sfx.Hit: c = hit; break;
            case Sfx.PowerUp: c = power; break;
            case Sfx.Present: c = present; break;
            case Sfx.Click: c = click; break;
        }
        if (c != null) sfxSource.PlayOneShot(c);
    }

    public void ToggleMusic()
    {
        MusicOn = !MusicOn;
        musicSource.mute = !MusicOn;
        PlayerPrefs.SetInt("WR_Music", MusicOn ? 1 : 0);
    }

    public void ToggleSfx()
    {
        SfxOn = !SfxOn;
        PlayerPrefs.SetInt("WR_Sfx", SfxOn ? 1 : 0);
    }

    // ------------------------------------------------ procedural audio
    static AudioClip MakeClip(string name, float[] data)
    {
        AudioClip c = AudioClip.Create(name, data.Length, 1, Rate, false);
        c.SetData(data, 0);
        return c;
    }

    static AudioClip Sweep(string name, float f0, float f1, float dur, float vol, bool noise)
    {
        int n = (int)(Rate * dur);
        float[] d = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            phase += 2f * Mathf.PI * Mathf.Lerp(f0, f1, t) / Rate;
            float s = noise ? (Random.value * 2f - 1f) * 0.6f + Mathf.Sin(phase) * 0.4f : Mathf.Sin(phase);
            float env = (1f - t) * (1f - t) * Mathf.Min(1f, i / 150f);
            d[i] = s * env * vol;
        }
        return MakeClip(name, d);
    }

    static AudioClip Notes(string name, float[] freqs, float noteLen, float vol)
    {
        int per = (int)(Rate * noteLen);
        float[] d = new float[per * freqs.Length];
        for (int k = 0; k < freqs.Length; k++)
        {
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / per;
                float env = (1f - t) * Mathf.Min(1f, i / 100f);
                d[k * per + i] = Mathf.Sin(2f * Mathf.PI * freqs[k] * i / Rate) * env * vol;
            }
        }
        return MakeClip(name, d);
    }

    static float Semi(int semitonesFromC5) { return 440f * Mathf.Pow(2f, (72 + semitonesFromC5 - 69) / 12f); }

    static void AddNote(float[] buf, int start, float freq, float len, float vol)
    {
        int n = Mathf.Min((int)(len * Rate), buf.Length - start);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / Rate;
            float env = Mathf.Exp(-t * 3.5f) * Mathf.Min(1f, i / 100f);
            float w = 2f * Mathf.PI * freq * t;
            buf[start + i] += (Mathf.Sin(w) + 0.35f * Mathf.Sin(2f * w) + 0.15f * Mathf.Sin(3f * w)) * env * vol;
        }
    }

    // Original 8-second bell-like loop (C major pentatonic).
    static AudioClip BuildMusic()
    {
        int[] melody =
        {
            0,-99,4,-99, 7,-99,4,-99,   9,-99,7,-99, 4,-99,2,-99,
            0,-99,4,-99, 7,-99,9,-99,   12,-99,9,-99, 7,-99,4,-99
        };
        int[] bass = { -24, -27, -24, -29 };
        float step = 0.25f;
        int stepSamples = (int)(step * Rate);
        float[] buf = new float[stepSamples * melody.Length];
        for (int i = 0; i < melody.Length; i++)
            if (melody[i] > -90) AddNote(buf, i * stepSamples, Semi(melody[i]), 0.7f, 0.22f);
        for (int b = 0; b < bass.Length; b++)
            AddNote(buf, b * 8 * stepSamples, Semi(bass[b]), 1.8f, 0.3f);
        for (int i = 0; i < buf.Length; i++) buf[i] = Mathf.Clamp(buf[i], -0.9f, 0.9f);
        return MakeClip("music", buf);
    }
}
