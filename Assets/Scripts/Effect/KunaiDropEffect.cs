using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MCRGame.Effect
{
    /// <summary>
    /// Wrapper Prefab 안의 Kunai와 ImpactFX를 이용해
    /// 단일 Prefab으로 Kunai 낙하 + 임팩트 이펙트를 재생합니다.
    /// 반환된 Sequence는 낙하 완료 후 1초 대기 시점에 완료되며,
    /// 그 OnComplete에서 파티클이 모두 사라진 뒤에 Destroy를 예약합니다.
    /// </summary>
    public class KunaiDropEffect : MonoBehaviour, IWinningEffect
    {
        [Header("Child References")]
        [SerializeField] private Transform kunai;
        [SerializeField] private GameObject impactFX;

        [Header("Offsets")]
        [SerializeField] private Vector3 arrivalOffset = Vector3.zero;
        [SerializeField] private Vector3 startOffset   = new Vector3(20f, 20f, 10f);

        [Header("Animation Settings")]
        [SerializeField] private float     dropDuration      = 1.0f;
        [SerializeField] private Vector3   rotationPerSecond = new Vector3(30f, 30f, 30f);

        // 런타임에 Instantiate된 임팩트 FX들 추적
        private readonly List<GameObject> _spawnedFX = new();

        public Sequence PlayEffect()
        {
            // 1) 초기 배치
            Vector3 targetPos   = transform.position + arrivalOffset;
            kunai.localPosition = startOffset;
            kunai.localRotation = Quaternion.identity;
            impactFX.SetActive(false);

            // 2) 시퀀스 생성
            var seq = DOTween.Sequence();

            // 2a) 낙하 + 회전
            seq.Append(kunai
                        .DOMove(targetPos, dropDuration)
                        .SetEase(Ease.InExpo))
               .Join(kunai
                        .DORotate(rotationPerSecond * dropDuration, dropDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.Linear));

            // 3) 낙하 완료 콜백: 임팩트 스폰
            seq.AppendCallback(() =>
            {
                if (impactFX != null)
                {
                    var fx = Instantiate(impactFX, kunai.position, Quaternion.identity, transform);
                    fx.SetActive(true);
                    foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
                        ps.Play();
                    _spawnedFX.Add(fx);
                }
            });

            // 4) 1초 대기
            seq.AppendInterval(1f);

            // 5) Sequence 완료 시(OnComplete) 정리 예약
            seq.OnComplete(() =>
            {
                // 5a) Kunai 제거
                Destroy(kunai.gameObject);

                // 5b) 각 FX 파티클 재생 길이에 맞춰 Destroy 예약
                foreach (var fx in _spawnedFX)
                {
                    float maxDur = 0f;
                    foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
                    {
                        var m = ps.main;
                        maxDur = Mathf.Max(maxDur, m.duration + m.startLifetime.constantMax);
                    }
                    Destroy(fx, maxDur + 0.5f);
                }

                // 5c) Wrapper Prefab 루트도 최대 파티클 길이 후에 제거
                float overallMax = 0f;
                foreach (var fx in _spawnedFX)
                {
                    foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
                    {
                        var m = ps.main;
                        overallMax = Mathf.Max(overallMax, m.duration + m.startLifetime.constantMax);
                    }
                }
                Destroy(gameObject, overallMax + 0.5f);
            });

            return seq;
        }
    }
}
