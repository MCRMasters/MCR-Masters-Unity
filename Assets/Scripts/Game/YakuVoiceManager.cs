// YakuVoiceManager.cs
using System;
using System.Collections.Generic;
using MCRGame.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

public class YakuVoiceManager : MonoBehaviour
{
    public static YakuVoiceManager Instance { get; private set; }

    [Tooltip("역 음성을 재생할 AudioSource")]
    [SerializeField] private AudioSource audioSource;

    // Resources/Voices/YakuVoice/YakuVoice_<Yaku 이름>.wav
    private Dictionary<Yaku, AudioClip> voiceClips = new Dictionary<Yaku, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadVoiceClips();
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
        if (scene.name == "GameScene")
        {
            ResetState();
            Debug.Log("[YakuVoiceManager] ResetState 호출");
        }
    }

    /// <summary>
    /// 씬 재진입 시 재생 중인 음성을 멈춥니다.
    /// </summary>
    private void ResetState()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    private void LoadVoiceClips()
    {
        foreach (Yaku y in Enum.GetValues(typeof(Yaku)))
        {
            if ((int)y < 0) continue;
            string path = $"Voices/YakuVoice/YakuVoice_{y}";
            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
                voiceClips[y] = clip;
            else
                Debug.LogWarning($"[YakuVoiceManager] 음성 파일 못 찾음: {path}");
        }
    }

    /// <summary>
    /// 해당 역의 음성을 재생하고, 재생 길이를 초 단위로 반환합니다.
    /// </summary>
    public float PlayYakuVoice(Yaku yaku)
    {
        if (voiceClips.TryGetValue(yaku, out var clip) && clip != null)
        {
            audioSource.PlayOneShot(clip);
            return clip.length;
        }
        return 0f;
    }
}
