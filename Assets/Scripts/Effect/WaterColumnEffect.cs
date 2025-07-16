using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace MCRGame.Effect
{
    /// <summary>
    /// DOTween 으로 _clip 페이드인·유지·페이드아웃을 처리하고
    /// 마무리되면 오브젝트를 파괴합니다.
    /// </summary>
    public class WaterColumnEffect : MonoBehaviour, IWinningEffect
    {
        [Header("Durations")]
        [Tooltip("페이드인 시간 (초)")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [Tooltip("전체 지속 시간 (초, 페이드인·페이드아웃 포함)")]
        [SerializeField] private float effectLifetime = 5f;
        [Tooltip("페이드아웃 시간 (초)")]
        [SerializeField] private float fadeOutDuration = 1.5f;

        // effectLifetime 에서 in/out 시간을 빼고 남은 만큼 유지
        private float holdDuration => Mathf.Max(0f, effectLifetime - fadeInDuration - fadeOutDuration);

        private readonly List<Material> _clipMaterials = new();

        private void Awake()
        {
            // 모든 자식 렌더러의 모든 Material을 검사
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_clip"))
                    {
                        // 시작 시 완전 클립 상태
                        mat.SetFloat("_clip", 1f);
                        _clipMaterials.Add(mat);
                    }
                }
            }
        }

        public Sequence PlayEffect()
        {
            var seq = DOTween.Sequence();

            // --- 페이드인 (1 → 0) ---
            foreach (var mat in _clipMaterials)
            {
                seq.Join(
                    mat
                        .DOFloat(0f, "_clip", fadeInDuration)
                        .SetEase(Ease.InOutQuad)
                );
            }

            // --- 유지 ---
            if (holdDuration > 0f)
                seq.AppendInterval(holdDuration);

            // --- 페이드아웃 (0 → 1) ---
            foreach (var mat in _clipMaterials)
            {
                seq.Join(
                    mat
                        .DOFloat(1f, "_clip", fadeOutDuration)
                        .SetEase(Ease.InOutQuad)
                );
            }

            // --- 완료 후 상태 보장 및 파괴 ---
            seq.OnComplete(() =>
            {
                foreach (var mat in _clipMaterials)
                    mat.SetFloat("_clip", 1f);
                Destroy(gameObject);
            });

            return seq;
        }
    }
}
