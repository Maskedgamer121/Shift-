using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class LoadingMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup cg;
        [SerializeField] private float fadeDuration = 1.0f;

        [Space]
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private float timer = 0.0f;
        private bool isFadingIn = true;

        private void Update()
        {
            timer += Time.deltaTime;

            HandleFade(isFadingIn);

            if (timer >= fadeDuration) {
                isFadingIn = !isFadingIn;
                timer = 0.0f;
            }
        }

        private void HandleFade(bool isFadeIn)
        {
            var t = timer / fadeDuration;
            if (!isFadeIn) t = 1 - t;

            var alpha = fadeCurve.Evaluate(t);
            cg.alpha = alpha;
        }
    }
}