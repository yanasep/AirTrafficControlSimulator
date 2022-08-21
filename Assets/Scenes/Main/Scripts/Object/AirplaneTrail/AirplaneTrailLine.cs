using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AirTraffic.Main
{
    public class AirplaneTrailLine : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private static readonly int AnimStateShow = Animator.StringToHash("Show");
        private static readonly int AnimStateHide = Animator.StringToHash("Hide");

        private void Awake()
        {
            animator.keepAnimatorControllerStateOnDisable = true;
        }

        public async UniTask ShowAsync(float duration)
        {
            animator.Play(AnimStateShow, 0, 0);

            float t = 0;
            while (t < duration)
            {
                var scale = transform.localScale;
                scale.y = Mathf.Lerp(1, 0, t / duration);
                transform.localScale = scale;
                await UniTask.Yield();
                if (this == null) return;
                t += Time.deltaTime;
            }

            animator.Play(AnimStateHide);
        }
    }
}