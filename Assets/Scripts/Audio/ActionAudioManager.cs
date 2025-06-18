using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MCRGame.Common;
using UnityEngine.SceneManagement;   // GameActionType, CallBlockType

namespace MCRGame.Audio
{
    /// <summary>특정 액션(Chi/Pung/Kong/Flower/Hu) 전용 효과음 큐</summary>
    public class ActionAudioManager : MonoBehaviour
    {
        public static ActionAudioManager Instance { get; private set; }

        [Header("Audio Source (1개면 충분)")]
        [SerializeField] private AudioSource audioSource;

        [Header("Action → Clip 매핑")]
        [SerializeField] private AudioClip chiClip;
        [SerializeField] private AudioClip pungClip;
        [SerializeField] private AudioClip kongClip;
        [SerializeField] private AudioClip flowerClip;
        [SerializeField] private AudioClip huClip;

        private readonly Queue<AudioClip> clipQueue = new();
        private bool isPlaying = false;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
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
                Debug.Log("[ActionAudioManager] ResetState 호출");
            }
        }

        /// <summary>
        /// 씬 재진입 시 큐를 비우고 사운드 재생을 멈춥니다.
        /// </summary>
        private void ResetState()
        {
            clipQueue.Clear();
            isPlaying = false;
            StopAllCoroutines();
            audioSource.Stop();
        }

        /* ───────────────────── 퍼블릭 API ───────────────────── */

        public void EnqueueCallSound(CallBlockType type)
        {
            switch (type)
            {
                case CallBlockType.CHII: EnqueueClip(chiClip); break;
                case CallBlockType.PUNG: EnqueueClip(pungClip); break;
                case CallBlockType.AN_KONG:
                case CallBlockType.DAIMIN_KONG:
                case CallBlockType.SHOMIN_KONG:
                    EnqueueClip(kongClip); break;
            }
        }

        public void EnqueueFlowerSound() => EnqueueClip(flowerClip);
        public void EnqueueHuSound() => EnqueueClip(huClip);

        /// <summary>
        /// 현재 AudioSource 볼륨 (0~1)
        /// </summary>
        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = Mathf.Clamp01(value);
        }

        /* ───────────────────── 내부 구현 ───────────────────── */

        private void EnqueueClip(AudioClip clip)
        {
            if (clip == null) return;
            clipQueue.Enqueue(clip);
            if (!isPlaying) StartCoroutine(PlayQueueRoutine());
        }

        private IEnumerator PlayQueueRoutine()
        {
            isPlaying = true;
            while (clipQueue.Count > 0)
            {
                AudioClip clip = clipQueue.Dequeue();
                audioSource.PlayOneShot(clip);
                // clip.length 가 0이면 (Streaming 등의 이유) 최소 0.1초 확보
                yield return new WaitForSeconds(Mathf.Max(clip.length, 0.1f));
            }
            isPlaying = false;
        }
    }
}
