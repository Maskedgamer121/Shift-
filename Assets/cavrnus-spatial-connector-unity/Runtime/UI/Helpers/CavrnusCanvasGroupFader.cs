using System;
using System.Collections;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Core;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public static class CavrnusCanvasGroupFader
    {
        public static void DoFade(this GameObject go, List<CanvasGroup> cgs, float duration, bool fadeIn, Action onComplete = null)
        {
            CavrnusStatics.Scheduler.StartCoroutine(DoFadeRoutine(go, cgs, duration, fadeIn, onComplete));
        }
        
        public static IEnumerator DoFadeRoutine(GameObject go, List<CanvasGroup> cgs, float duration, bool fadeIn, Action onComplete = null)
        {
            var animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            var start = fadeIn ? 0f : 1f;
            var end = fadeIn ? 1f : 0f;
            cgs.ForEach(cg => {
                if (cg != null)
                    cg.alpha = fadeIn ? 0f : 1f;
            });
            
            var elapsedTime = 0f;
            while (elapsedTime < duration) {
                if (go == null || go.Equals(null))
                    yield break;
                
                var normalizedTime = elapsedTime / duration;
                cgs.ForEach(cg => {
                    if (cg != null) {
                        var curveVal = animationCurve.Evaluate(normalizedTime);
                        cg.alpha = Mathf.Lerp(start, end, curveVal);
                    }
                });

                elapsedTime += Time.deltaTime;

                yield return null;
            }
            
            cgs.ForEach(cg => {
                if (cg != null)
                    cg.alpha = fadeIn ? 1f : 0f;
            });
            
            onComplete?.Invoke();
        }
    }
}