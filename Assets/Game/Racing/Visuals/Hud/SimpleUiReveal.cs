using System.Collections;
using UnityEngine;

namespace IdleRacer.Racing.Visuals.Hud
{
    /// <summary>Lightweight presentation-only scale/fade reveal for the pending-item card.</summary>
    public sealed class SimpleUiReveal : MonoBehaviour
    {
        private CanvasGroup _group;
        private RectTransform _rect;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _group = GetComponent<CanvasGroup>();
            if (_group == null)
            {
                _group = gameObject.AddComponent<CanvasGroup>();
            }
        }

        public void Play()
        {
            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            const float duration = 0.22f;
            float t = 0f;
            _group.alpha = 0f;
            _rect.localScale = Vector3.one * 0.88f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / duration);
                float e = 1f - (1f - u) * (1f - u);
                _group.alpha = e;
                _rect.localScale = Vector3.Lerp(Vector3.one * 0.88f, Vector3.one, e);
                yield return null;
            }
            _group.alpha = 1f;
            _rect.localScale = Vector3.one;
        }
    }
}
