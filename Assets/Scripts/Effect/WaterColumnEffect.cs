using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MCRGame.Effect
{
    public class WaterColumnEffect : MonoBehaviour, IWinningEffect
    {
        private float speedMultiplier = 2.25f;

        [Header("Durations")]
        [Tooltip("페이드인 시간 (초)")]
        [SerializeField] private float fadeInDuration      = 0.3f;
        [Tooltip("전체 지속 시간 (초, 페이드인·페이드아웃 포함)")]
        [SerializeField] private float effectLifetime      = 2.0f;
        [Tooltip("페이드아웃 시간 (초)")]
        [SerializeField] private float fadeOutDuration     = 0.7f;

        [Header("Emission Taper Settings")]
        [Tooltip("방출 중지 전 앞당길 시간 (초)")]
        [SerializeField] private float emissionAdvance     = 0f;
        [Tooltip("파티클 taper 단계 시간 (초)")]
        [SerializeField] private float taperDuration       = 1.5f;
        [Tooltip("taper 후 버퍼 (초)")]
        [SerializeField] private float bufferAfterTaper    = 0.3f;

        [Header("Geometry Final Fade")]
        [Tooltip("남은 클립을 완전 투명화할 시간 (초)")]
        [SerializeField] private float geometryFadeDuration = 0.3f;

        // effectLifetime 에서 in/out 시간을 뺀 hold 시간
        private float holdDuration => Mathf.Max(
            0f,
            effectLifetime - fadeInDuration - fadeOutDuration - emissionAdvance
        );

        private readonly List<Material> _clipMaterials      = new();
        private readonly List<float>    _originalClipValues = new();
        private readonly List<Material> _psMaterials          = new();
        private readonly List<Color>    _originalAlbedoColors = new();

        private void Awake()
        {
            // 1) Geometry clip 머티리얼 수집
            foreach (var rend in GetComponentsInChildren<Renderer>())
                foreach (var mat in rend.materials)
                    if (mat.HasProperty("_clip"))
                    {
                        _originalClipValues.Add(mat.GetFloat("_clip"));
                        mat.SetFloat("_clip", 1f);
                        _clipMaterials.Add(mat);
                    }

            // 2) ParticleSystemRenderer 전용 머티리얼 수집
            foreach (var psr in GetComponentsInChildren<ParticleSystemRenderer>())
                foreach (var mat in psr.materials)
                    if (mat.HasProperty("_Color"))
                    {
                        _psMaterials.Add(mat);
                        _originalAlbedoColors.Add(mat.GetColor("_Color"));
                    }
        }

        /// <summary>
        /// 재생되는 전체 Sequence:
        /// 1) clip 페이드인,
        /// 2) hold,
        /// 3) clip & PS Albedo 페이드아웃,
        /// 4) geometry final fade,
        /// 완료 시 코루틴으로 taper 후 Destroy.
        /// </summary>
        public Sequence PlayEffect()
        {
            // 파티클 시스템 수집 및 백업
            var systems   = GetComponentsInChildren<ParticleSystem>();
            var origRates = new float[systems.Length];
            var origSizes = new float[systems.Length];
            for (int i = 0; i < systems.Length; i++)
            {
                var ps = systems[i];
                origRates[i] = ps.emission.rateOverTime.constant;
                origSizes[i] = ps.main.startSizeMultiplier;
                ps.Play();
            }

            // **스케일된** 구간별 시간 계산
            float fi  = fadeInDuration      / speedMultiplier;
            float fo  = fadeOutDuration     / speedMultiplier;
            float geo = geometryFadeDuration/ speedMultiplier;
            float hold= holdDuration        / speedMultiplier;
            float tap = taperDuration       / speedMultiplier;
            float buf = bufferAfterTaper    / speedMultiplier;

            var seq = DOTween.Sequence();

            // 1) 페이드인 (1 → originalClip)
            foreach (var (mat, orig) in _clipMaterials.Zip(_originalClipValues, (m, v) => (m, v)))
            {
                seq.Join(mat
                    .DOFloat(orig, "_clip", fi)
                    .SetEase(Ease.InOutQuad));
            }

            // 2) 유지
            if (hold > 0f)
                seq.AppendInterval(hold);

            // 3) emission 중지 콜백
            seq.AppendCallback(() =>
            {
                foreach (var ps in systems)
                {
                    var em = ps.emission;
                    em.enabled = false;
                }
            });

            // 4) 페이드아웃 트윈 (clip & PS Albedo)
            var fadeOutTween = DOTween.Sequence();
            foreach (var mat in _clipMaterials)
            {
                fadeOutTween.Join(mat
                    .DOFloat(1.02f, "_clip", fo)
                    .SetEase(Ease.InOutQuad));
            }
            for (int i = 0; i < _psMaterials.Count; i++)
            {
                var mat   = _psMaterials[i];
                var origC = _originalAlbedoColors[i];
                Color targetColor = new Color(1f, 1f, 1f, origC.a);
                fadeOutTween.Join(mat
                    .DOColor(targetColor, "_Color", fo)
                    .From(origC)
                    .SetEase(Ease.InQuad));
            }
            seq.Append(fadeOutTween);

            // 5) geometry final fade
            var geoTween = DOTween.Sequence();
            foreach (var mat in _clipMaterials)
            {
                geoTween.Join(mat
                    .DOFloat(1.02f, "_clip", geo)
                    .SetEase(Ease.InOutQuad));
            }
            seq.Append(geoTween);

            // 완료 시 taper 코루틴 실행
            seq.OnComplete(() =>
            {
                foreach (var rend in GetComponentsInChildren<Renderer>())
                    if (!(rend is ParticleSystemRenderer))
                        rend.enabled = false;

                StartCoroutine(TaperAndDestroy(systems, origRates, origSizes, tap, buf));
            });

            return seq;
        }

        // taperDuration과 bufferAfterTaper도 스케일된 값을 파라미터로 받음
        private IEnumerator TaperAndDestroy(
            ParticleSystem[] systems,
            float[] origRates,
            float[] origSizes,
            float scaledTaper,
            float scaledBuffer)
        {
            var emissions = new ParticleSystem.EmissionModule[systems.Length];
            var mains     = new ParticleSystem.MainModule[systems.Length];
            for (int i = 0; i < systems.Length; i++)
            {
                emissions[i] = systems[i].emission;
                mains[i]     = systems[i].main;
            }

            float elapsed = 0f;
            while (elapsed < scaledTaper)
            {
                float t = elapsed / scaledTaper;
                for (int i = 0; i < systems.Length; i++)
                {
                    emissions[i].rateOverTime    =
                        new ParticleSystem.MinMaxCurve(Mathf.Lerp(origRates[i], 0f, t));
                    mains[i].startSizeMultiplier =
                        Mathf.Lerp(origSizes[i], 0f, t);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // 최종 보정
            for (int i = 0; i < systems.Length; i++)
            {
                emissions[i].rateOverTime    = new ParticleSystem.MinMaxCurve(0f);
                mains[i].startSizeMultiplier = 0f;
            }

            // 스케일된 버퍼 시간 만큼 대기
            yield return new WaitForSeconds(scaledBuffer);
            Destroy(gameObject);
        }
    }
}
