using Raylib_cs;

namespace CoreShift.Game;

public enum Sfx
{
    Shoot,
    Hit,
    Kill,
    Pickup,
    LevelUp,
    Hurt,
    UiMove,
    UiConfirm,
}

/// <summary>
/// Procedural audio: all sound effects are synthesized in code as WAV buffers and loaded with
/// raylib. No audio asset files are required, and the service degrades to a no-op when no audio
/// device is available (e.g. headless screenshot runs).
/// </summary>
public static class Audio
{
    private const int SampleRate = 22050;

    private static readonly Dictionary<Sfx, Sound> Sounds = new();
    private static readonly Dictionary<Sfx, float> LastPlayed = new();
    private static readonly Dictionary<Sfx, float> Cooldowns = new()
    {
        [Sfx.Shoot] = 0.035f,
        [Sfx.Hit] = 0.035f,
        [Sfx.Pickup] = 0.02f,
    };

    private static bool _ready;
    private static Music _music;
    private static bool _musicReady;
    private static float _musicVolume = 0.5f;

    public static bool Ready => _ready;

    public static void Initialize()
    {
        if (_ready) return;

        try
        {
            Raylib.InitAudioDevice();
        }
        catch
        {
            return;
        }

        if (!Raylib.IsAudioDeviceReady()) return;
        _ready = true;

        Register(Sfx.Shoot, SynthSweep(880f, 320f, 0.07f, 0.45f));
        Register(Sfx.Hit, SynthNoise(0.05f, 0.35f));
        Register(Sfx.Kill, SynthSweep(420f, 90f, 0.20f, 0.5f));
        Register(Sfx.Pickup, SynthSweep(660f, 1180f, 0.09f, 0.4f));
        Register(Sfx.LevelUp, SynthArpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.09f, 0.45f));
        Register(Sfx.Hurt, SynthSweep(200f, 90f, 0.16f, 0.5f));
        Register(Sfx.UiMove, SynthSweep(620f, 620f, 0.03f, 0.3f));
        Register(Sfx.UiConfirm, SynthSweep(520f, 940f, 0.12f, 0.4f));

        try
        {
            _music = Raylib.LoadMusicStreamFromMemory(".wav", ToWav(SynthMusic()));
            _musicReady = true;
            Raylib.SetMusicVolume(_music, _musicVolume);
        }
        catch
        {
            _musicReady = false;
        }
    }

    public static void StartMusic()
    {
        if (!_ready || !_musicReady) return;
        Raylib.PlayMusicStream(_music);
    }

    public static void UpdateMusic()
    {
        if (!_ready || !_musicReady) return;
        Raylib.UpdateMusicStream(_music);
        if (!Raylib.IsMusicStreamPlaying(_music)) Raylib.PlayMusicStream(_music);
    }

    public static void SetMusicVolume(float volume)
    {
        _musicVolume = Math.Clamp(volume, 0f, 1f);
        if (_ready && _musicReady) Raylib.SetMusicVolume(_music, _musicVolume);
    }

    public static void Play(Sfx id)
    {
        if (!_ready) return;
        if (!Sounds.TryGetValue(id, out var sound)) return;

        float now = (float)Raylib.GetTime();
        if (Cooldowns.TryGetValue(id, out var cooldown)
            && LastPlayed.TryGetValue(id, out var last)
            && now - last < cooldown)
        {
            return;
        }
        LastPlayed[id] = now;

        Raylib.PlaySound(sound);
    }

    public static void SetVolumes(float master, float sfx)
    {
        if (!_ready) return;
        Raylib.SetMasterVolume(Math.Clamp(master, 0f, 1f));
        foreach (var sound in Sounds.Values)
        {
            Raylib.SetSoundVolume(sound, Math.Clamp(sfx, 0f, 1f));
        }
    }

    public static void Shutdown()
    {
        if (!_ready) return;
        if (_musicReady)
        {
            Raylib.UnloadMusicStream(_music);
            _musicReady = false;
        }
        foreach (var sound in Sounds.Values) Raylib.UnloadSound(sound);
        Sounds.Clear();
        Raylib.CloseAudioDevice();
        _ready = false;
    }

    private static void Register(Sfx id, short[] samples)
    {
        var wave = Raylib.LoadWaveFromMemory(".wav", ToWav(samples));
        Sounds[id] = Raylib.LoadSoundFromWave(wave);
        Raylib.UnloadWave(wave);
    }

    private static short[] SynthSweep(float startHz, float endHz, float seconds, float volume)
    {
        int count = Math.Max(1, (int)(SampleRate * seconds));
        var samples = new short[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)count;
            float frequency = startHz + (endHz - startHz) * t;
            phase += frequency / SampleRate;
            float envelope = 1f - t;
            float value = MathF.Sin(phase * MathF.Tau) * envelope * volume;
            samples[i] = (short)(value * short.MaxValue);
        }

        return samples;
    }

    private static short[] SynthNoise(float seconds, float volume)
    {
        int count = Math.Max(1, (int)(SampleRate * seconds));
        var samples = new short[count];
        uint state = 0x1234567u;

        for (int i = 0; i < count; i++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            float noise = (state / (float)uint.MaxValue) * 2f - 1f;
            float envelope = 1f - i / (float)count;
            samples[i] = (short)(noise * envelope * volume * short.MaxValue);
        }

        return samples;
    }

    private static short[] SynthArpeggio(float[] notes, float noteSeconds, float volume)
    {
        int perNote = Math.Max(1, (int)(SampleRate * noteSeconds));
        var samples = new short[perNote * notes.Length];

        for (int n = 0; n < notes.Length; n++)
        {
            float phase = 0f;
            for (int i = 0; i < perNote; i++)
            {
                float t = i / (float)perNote;
                phase += notes[n] / SampleRate;
                float envelope = 1f - t;
                float value = MathF.Sin(phase * MathF.Tau) * envelope * volume;
                samples[n * perNote + i] = (short)(value * short.MaxValue);
            }
        }

        return samples;
    }

    private static short[] SynthMusic()
    {
        const float duration = 8f;
        int count = (int)(SampleRate * duration);
        var samples = new short[count];

        float[] melody = { 440f, 523.25f, 659.25f, 523.25f, 587.33f, 698.46f, 880f, 698.46f };
        float[] bass = { 110f, 110f, 146.83f, 146.83f, 98f, 98f, 130.81f, 130.81f };
        float noteLength = duration / melody.Length;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)SampleRate;
            int index = (int)(t / noteLength) % melody.Length;
            float local = t - index * noteLength;
            float envelope = MathF.Min(1f, local * 24f) * MathF.Exp(-local * 2.2f);

            float lead = MathF.Sin(melody[index] * MathF.Tau * t) * 0.20f * envelope;
            float low = MathF.Sin(bass[index] * MathF.Tau * t) * 0.15f * (0.8f + 0.2f * MathF.Sin(t * 1.5f));
            float value = lead + low;

            samples[i] = (short)(System.Math.Clamp(value, -1f, 1f) * short.MaxValue);
        }

        return samples;
    }

    private static byte[] ToWav(short[] samples)
    {
        int dataSize = samples.Length * 2;
        using var stream = new MemoryStream(44 + dataSize);
        using var writer = new BinaryWriter(stream);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8.ToArray());
        writer.Write(dataSize);
        foreach (var sample in samples) writer.Write(sample);
        writer.Flush();

        return stream.ToArray();
    }
}
