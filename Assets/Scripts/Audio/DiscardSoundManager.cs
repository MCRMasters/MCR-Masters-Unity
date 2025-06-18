using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCRGame.Audio
{
    /// <summary>
    /// 버림(Discard) 효과음을 즉시 재생하는 매니저
    /// </summary>
    public class DiscardSoundManager : MonoBehaviour
    {
        public static DiscardSoundManager Instance { get; private set; }

        [Header("Audio Source (즉시 재생용)")]
        [SerializeField] private AudioSource audioSource;

        [Header("Discard Clip")]
        [SerializeField] private AudioClip discardClip;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // AudioSource가 없으면 자동 생성
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
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
                Debug.Log("[DiscardSoundManager] ResetState 호출");
            }
        }

        /// <summary>
        /// 씬 재진입 시 재생 중인 효과음을 멈춥니다.
        /// </summary>
        private void ResetState()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        /// <summary>
        /// 즉시 재생용 AudioSource 볼륨 (0~1)
        /// </summary>
        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 즉시 버림 효과음 재생
        /// </summary>
        public void PlayDiscardSound()
        {
            if (discardClip == null || audioSource == null) return;
            audioSource.PlayOneShot(discardClip);
        }
    }
}
