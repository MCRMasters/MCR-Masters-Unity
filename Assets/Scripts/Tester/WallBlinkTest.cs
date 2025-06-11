using UnityEngine;
using System.Collections;

namespace MCRGame.Tester
{
    [RequireComponent(typeof(MeshRenderer))]
    public class WallBlink : MonoBehaviour
    {
        [Tooltip("clip 값이 낮을수록 보임, 높을수록 안 보임")]
        public float visibleClip = 0.2f;
        public float invisibleClip = 1.0f;
        public float interval = 0.1f;

        private MaterialPropertyBlock _mpb;
        private MeshRenderer _renderer;
        private bool _isVisible = true;

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _mpb = new MaterialPropertyBlock();
            _mpb.SetFloat("_clip", visibleClip);
            _renderer.SetPropertyBlock(_mpb);
        }

        void OnEnable()
        {
            StartCoroutine(Blink());
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        IEnumerator Blink()
        {
            while (true)
            {
                _isVisible = !_isVisible;
                float clip = _isVisible ? visibleClip : invisibleClip;

                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetFloat("_clip", clip);
                _renderer.SetPropertyBlock(_mpb);

                yield return new WaitForSeconds(interval);
            }
        }
    }
}