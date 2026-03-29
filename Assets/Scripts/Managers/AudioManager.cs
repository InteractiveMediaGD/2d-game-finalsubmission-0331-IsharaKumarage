using UnityEngine;

/// <summary>
/// Retro Procedural Sound Synthesizer!
/// Generates 8-bit sword slashes, hit explosions, and a continuous BGM track mathematically!
/// No audio files required. Just drop this in your scene.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource sfxSource;
    public AudioSource bgmSource; 

    public AudioClip swordSwingClip;
    public AudioClip swordHitClip;
    public AudioClip enemyDeathClip;
    public AudioClip healClip;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (sfxSource == null) { sfxSource = gameObject.AddComponent<AudioSource>(); }
        if (bgmSource == null) 
        { 
            bgmSource = gameObject.AddComponent<AudioSource>(); 
            bgmSource.loop = true; 
            bgmSource.volume = 0.35f;
        }

        // Generate missing sound effects mathematically
        GenerateMissingAudio();
        
        // Generate continuous background music 
        bgmSource.clip = GenerateBackgroundMusic();
        bgmSource.Play();
    }

    void GenerateMissingAudio()
    {
        if (swordSwingClip == null) swordSwingClip = GenerateSwingOrHit(true);
        if (swordHitClip == null) swordHitClip = GenerateSwingOrHit(false);
        if (enemyDeathClip == null) enemyDeathClip = GenerateDeath();
        if (healClip == null)       healClip       = GenerateHeal();
    }

    AudioClip GenerateSwingOrHit(bool isSwing)
    {
        int sampleRate = 44100;
        float duration = isSwing ? 0.3f : 0.15f;
        float[] samples = new float[(int)(sampleRate * duration)];

        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = isSwing ? Mathf.Exp(-t * 15f) : Mathf.Exp(-t * 30f); 
            
            float noise = Random.Range(-1f, 1f);
            
            if (!isSwing)
            {
                // Hit is higher frequency noise mixed with a tiny square wave
                float square = Mathf.Sign(Mathf.Sin(t * Mathf.PI * 2f * 800f * Mathf.Exp(-t*20f)));
                noise = (noise * 0.5f) + (square * 0.5f);
            }
            else 
            {
                // Smooth noise for sword swoosh
                noise = noise * Mathf.Sin(t * Mathf.PI * 2f * 150f); 
            }

            samples[i] = noise * envelope * 0.8f;
        }

        AudioClip clip = AudioClip.Create(isSwing ? "Swish" : "Hit", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateDeath()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        float[] samples = new float[(int)(sampleRate * duration)];
        
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 4f);
            
            // Bass drop square wave falling from 150Hz to 20Hz
            float freq = Mathf.Lerp(150f, 20f, t / duration);
            float square = Mathf.Sign(Mathf.Sin(t * Mathf.PI * 2f * freq));
            float noise = Random.Range(-1f, 1f) * 0.3f; // add rubble noise
            
            samples[i] = (square + noise) * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Death", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip GenerateBackgroundMusic()
    {
        int sampleRate = 44100;
        int bpm = 120;
        float noteDuration = 60f / bpm; // 0.5 seconds per beat
        
        // 4 chords, 2 beats each = 4 seconds total loop
        float totalDuration = noteDuration * 8f; 
        float[] samples = new float[(int)(sampleRate * totalDuration)];

        // Tense RPG Battle Chords (Arpeggiated)
        // C minor, Ab major, F minor, G major
        float[][] chords = new float[][] {
            GetFrequencies(48, 51, 55, 60), // Cm
            GetFrequencies(44, 48, 51, 56), // Ab Major
            GetFrequencies(41, 44, 48, 53), // Fm
            GetFrequencies(43, 47, 50, 55)  // G Major
        };

        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            
            // Current chord (2 beats per chord)
            int chordIdx = (int)(t / (noteDuration * 2f)) % chords.Length;
            float[] chord = chords[chordIdx];

            // Arpeggio index (change every 16th note)
            float sixteenth = noteDuration / 4f;
            int arpIdx = (int)(t / sixteenth) % chord.Length;
            float freq = chord[arpIdx];

            // Note envelope (fading out fast for crisp chiptune)
            float noteTime = t % sixteenth;
            float env = Mathf.Exp(-noteTime * 12f);

            // Synth mixing: Triangle wave (melody) + Deep Square wave (bass)
            float melodyWave = Mathf.Asin(Mathf.Sin(t * Mathf.PI * 2f * freq)); 
            float bassWave   = Mathf.Sign(Mathf.Sin(t * Mathf.PI * 2f * (freq / 2f))); 
            
            samples[i] = (melodyWave * 0.6f + bassWave * 0.15f) * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create("BGM", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Converts MIDI notes into exact Hertz frequencies
    float[] GetFrequencies(params int[] midiNotes)
    {
        float[] freqs = new float[midiNotes.Length];
        for (int i = 0; i < midiNotes.Length; i++)
        {
            freqs[i] = 440f * Mathf.Pow(2f, (midiNotes[i] - 69f) / 12f);
        }
        return freqs;
    }

    AudioClip GenerateHeal()
    {
        int sampleRate = 44100;
        float duration = 0.5f;
        float[] samples = new float[(int)(sampleRate * duration)];
        
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 5f);
            
            // Fast ascending arpeggio (C E G C)
            float freq = 523.25f; // C5
            if (t > 0.1f) freq = 659.25f; // E5
            if (t > 0.2f) freq = 783.99f; // G5
            if (t > 0.3f) freq = 1046.50f; // C6
            
            float wave = Mathf.Sin(t * Mathf.PI * 2f * freq); 
            samples[i] = wave * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("Heal", samples.Length, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlaySwordSwing() { if (sfxSource != null && swordSwingClip != null) { sfxSource.pitch = Random.Range(0.9f, 1.1f); sfxSource.PlayOneShot(swordSwingClip); } }
    public void PlaySwordHit()   { if (sfxSource != null && swordHitClip != null)   { sfxSource.pitch = Random.Range(0.85f, 1.15f); sfxSource.PlayOneShot(swordHitClip); } }
    public void PlayEnemyDeath() { if (sfxSource != null && enemyDeathClip != null) { sfxSource.pitch = Random.Range(0.9f, 1.1f); sfxSource.PlayOneShot(enemyDeathClip); } }
    public void PlayHeal()       { if (sfxSource != null && healClip != null)       { sfxSource.pitch = 1f; sfxSource.PlayOneShot(healClip); } }
}
