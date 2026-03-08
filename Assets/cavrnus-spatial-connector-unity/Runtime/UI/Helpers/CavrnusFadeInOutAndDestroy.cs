using System.Collections;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Core;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class CavrnusFadeInOutAndDestroy : MonoBehaviour
    {
        private CanvasGroup cg;
        private float fadeInDuration;

        private void Awake()
        {
            cg = gameObject.AddComponent<CanvasGroup>();
        }

        public void Begin(float fadeInDuration, float ttl)
        {
            gameObject.DoFade(new List<CanvasGroup> {cg}, fadeInDuration, true, () => {
                CavrnusStatics.Scheduler.ExecCoRoutine(TimeToLiveRoutine(ttl));
            });
        }

        private IEnumerator TimeToLiveRoutine(float ttl)
        {
            yield return new WaitForSeconds(ttl);
            gameObject.DoFade(new List<CanvasGroup> {cg}, 0.7f, false,
              () => {
                  Destroy(gameObject);
              });
        }
    }
}