using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;
    public bool loop = false;
    public bool playOnAwake = false;

    [HideInInspector]
    public AudioSource source;
}

public class AudioManage : MonoBehaviour
{
    public static AudioManage Instance;

    public Sound[] backgroundMusic;
    public Sound[] soundEffects;

    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private AudioSource currentBGM;
    private string currentBGMName;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Khởi tạo AudioSource cho từng âm thanh
        InitializeAudioSources(backgroundMusic);
        InitializeAudioSources(soundEffects);
    }

    private void InitializeAudioSources(Sound[] sounds)
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume * masterVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = s.playOnAwake;

            if (s.playOnAwake)
            {
                s.source.Play();
            }
        }
    }

    // Phát nhạc nền
    public void PlayMusic(string name)
    {
        Sound s = GetSound(name, backgroundMusic);
        if (s == null)
        {
            Debug.LogWarning($"Sound {name} not found!");
            return;
        }

        // Nếu đang phát nhạc khác, fade out nhạc cũ
        if (currentBGM != null && currentBGMName != name)
        {
            StartCoroutine(FadeOut(currentBGM, 1f));
        }

        currentBGM = s.source;
        currentBGMName = name;
        s.source.volume = s.volume * masterVolume * musicVolume;
        s.source.Play();
    }

    // Phát hiệu ứng âm thanh
    public void PlaySFX(string name)
    {
        Sound s = GetSound(name, soundEffects);
        if (s == null)
        {
            Debug.LogWarning($"Sound {name} not found!");
            return;
        }

        s.source.volume = s.volume * masterVolume * sfxVolume;
        s.source.Play();
    }

    private Sound GetSound(string name, Sound[] sounds)
    {
        return System.Array.Find(sounds, sound => sound.name == name);
    }

    // Điều chỉnh âm lượng
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateSFXVolumes();
    }

    private void UpdateAllVolumes()
    {
        UpdateMusicVolumes();
        UpdateSFXVolumes();
    }

    private void UpdateMusicVolumes()
    {
        foreach (Sound s in backgroundMusic)
        {
            if (s.source != null)
            {
                s.source.volume = s.volume * masterVolume * musicVolume;
            }
        }
    }

    private void UpdateSFXVolumes()
    {
        foreach (Sound s in soundEffects)
        {
            if (s.source != null)
            {
                s.source.volume = s.volume * masterVolume * sfxVolume;
            }
        }
    }

    // Fade out effect
    private IEnumerator FadeOut(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, timer / duration);
            yield return null;
        }

        audioSource.Stop();
    }

    // Tạm dừng/tiếp tục tất cả âm thanh
    public void PauseAll()
    {
        foreach (Sound s in backgroundMusic)
        {
            if (s.source.isPlaying) s.source.Pause();
        }
        foreach (Sound s in soundEffects)
        {
            if (s.source.isPlaying) s.source.Pause();
        }
    }

    public void ResumeAll()
    {
        foreach (Sound s in backgroundMusic)
        {
            if (!s.source.isPlaying) s.source.UnPause();
        }
        foreach (Sound s in soundEffects)
        {
            if (!s.source.isPlaying) s.source.UnPause();
        }
    }
}