using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    [System.Serializable]
    public struct Sound
    {
        public string Name;
        public AudioClip audioClip;

        [Range(0, 1)]
        public float Volume;
        [Range(0, 256)]
        public int priority;
        public Vector2 Pitch;
        public float fadeDuration;
        private float currentPitch; // not currently used
        public bool loop;

        [Tooltip("Spatail blend, 2D to 3D")]
        [Range(0, 1)]
        public float SpatialBlend;
    };

    [Header("Sounds")]
    public List<Sound> Sounds = new List<Sound>();

    public static SoundManager Instance = null;
    private AudioSource[] audioSources;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitAudioSource();
            InitSounds();
        }
        else
        {
            Destroy(this);
        }
    }

    void InitAudioSource() // clean and refill when a scene is changed
    {
        audioSources = FindObjectsOfType<AudioSource>();
    }

    void InitSounds()
    {
        for (int i = 0; i < Sounds.Count; i++)
        {
            Sound s = Sounds[i];
            s.audioClip = (AudioClip)Resources.Load("Sounds/" + s.Name);

            if (s.audioClip == null)
            {
                continue;
            }
            Sounds[i] = s;
        }
    }

    public void PlaySound(string name, AudioSource source, bool FadeIn = false)
    {
        Sound s = findSound(name);
        if (s.audioClip != null)
        {
            source.clip = s.audioClip;
            source.volume = s.Volume;
            source.pitch = Random.Range(s.Pitch.x, s.Pitch.y);
            source.spatialBlend = s.SpatialBlend;
            source.priority = s.priority;
            source.loop = s.loop;

            source.Play();
            if (FadeIn)
                StartCoroutine(FadeVolume(source, s.Volume, s.fadeDuration, true));
        }
    }

    public void StopSound(AudioSource source, bool FadeOut = false, float fadeDuration = 1.0f)
    {
        if (source.isPlaying)
        {
            if (FadeOut)
                StartCoroutine(FadeVolume(source, 0.0f, fadeDuration, false));
            source.Stop();
        }
    }

    public void PlaySoundOneShot(string name, AudioSource source)
    {
        Sound s = findSound(name);
        if (s.audioClip != null)
        {
            source.volume = s.Volume;
            source.pitch = Random.Range(s.Pitch.x, s.Pitch.y);
            source.spatialBlend = s.SpatialBlend;
            source.priority = s.priority;
            source.clip = s.audioClip;
            source.loop = false;

            source.Play();
        }
    }

    public void SpawnPlaySound(string name, Vector3 pos)
    {
        Sound s = findSound(name);
        if (s.audioClip != null)
        {
            AudioSource.PlayClipAtPoint(s.audioClip, pos, s.Volume);
        }
    }

    public Sound findSound(string name)
    {
        foreach (Sound sound in Sounds)
        {
            if (sound.Name == name)
                return sound;
        }
        return new Sound();
    }

    IEnumerator FadeVolume(AudioSource audioSource, float volume, float duration, bool IsFadeIn)
    {
        if (audioSource != null)
            yield return null;

        float vol = volume;

        if (!IsFadeIn)
            vol = audioSource.volume;

        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            if (IsFadeIn)
                audioSource.volume = Mathf.Lerp(0, volume, time / duration);
            else
                audioSource.volume = Mathf.Lerp(volume, 0, time / duration);
            yield return null;
        }
    }

    [ContextMenu("RegenerateList")]
    public void RegenerateList()
    {
        AudioClip[] audioClip = Resources.LoadAll<AudioClip>("Sounds");
        if (Sounds == null)
            Sounds = new List<Sound>();

        foreach (Sound s in Sounds) // Remove
        {
            bool check = true;
            for (int i = 0; i < audioClip.Length; i++)
            {
                if (audioClip[i].name == s.Name)
                {
                    check = false;
                    break;
                }
            }

            if (check)
                Sounds.Remove(s);
        }

        for (int i = 0; i < audioClip.Length; i++)
        {
            bool check = true;
            foreach (Sound s in Sounds)
            {
                if (audioClip[i].name == s.Name)
                {
                    check = false;
                    break;
                }
            }
            if (check)
            {
                Sound snd = new Sound { Name = audioClip[i].name, Volume = 1.0f, Pitch = new Vector2(1, 1), SpatialBlend = 1.0f, priority = 128, fadeDuration = 0, loop = false };
                snd.audioClip = audioClip[i];
                Sounds.Add(snd);
            }
        }
    }

    public IEnumerator PauseAudioSource()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].Pause();
            yield return null;
        }
    }

    public IEnumerator UnPauseAudioSource()
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].UnPause();
            yield return null;
        }
    }

    public void PlaySoundOneShot(string[] s, AudioSource au) {
        PlaySoundOneShot( s[ Random.Range(0, s.Length) ] , au);

    }
    public void SpawnPlaySound(string[] s, Vector3 v)
    {
        SpawnPlaySound(s[Random.Range(0, s.Length)], v);
    }
}