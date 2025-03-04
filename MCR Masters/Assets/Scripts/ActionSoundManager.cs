using UnityEngine;
using System.Collections;

public class ActionSoundManager : MonoBehaviour
{
    public AudioClip chiiClip;
    public AudioClip ponClip;
    public AudioClip kanClip;
    public AudioClip huClip;

    private AudioSource audioSource;

    [Range(0f, 1f)]
    public float volume = 0.5f; // 0~1 범위로 볼륨 설정 가능

    private void Awake()
    {
        // 오디오 소스 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    public void PlayActionSound(string actionType)
    {
        switch (actionType.ToLower())
        {
            case "chii":
                StartCoroutine(PlaySoundCoroutine(chiiClip));
                break;
            case "pon":
                StartCoroutine(PlaySoundCoroutine(ponClip));
                break;
            case "kan":
                StartCoroutine(PlaySoundCoroutine(kanClip));
                break;
            case "hu":
                StartCoroutine(PlaySoundCoroutine(huClip));
                break;
            default:
                Debug.LogWarning($"No sound available for action type: {actionType}");
                break;
        }
    }

    private IEnumerator PlaySoundCoroutine(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null!");
            yield break;
        }

        audioSource.clip = clip;
        audioSource.Play();

        // 오디오 재생이 끝날 때까지 대기
        yield return new WaitWhile(() => audioSource.isPlaying);
    }
}

