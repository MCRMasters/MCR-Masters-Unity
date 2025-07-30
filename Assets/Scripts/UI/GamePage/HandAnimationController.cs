using UnityEngine;
using System;
using System.Collections;

namespace MCRGame.UI
{
    public class HandAnimationController : MonoBehaviour
    {
        public Animator handAnimator;
        public string discardTrigger = "TriggerDiscard";
        // 외부에서 구독할 이벤트
        public event Action OnThrowFrame;

        public void PlayDiscardAnimation()
        {
            handAnimator.SetTrigger(discardTrigger);
        }

        // Animation Event로 호출될 메서드
        public void AnimationEvent_Throw()
        {
            OnThrowFrame?.Invoke();
        }
    }
}
