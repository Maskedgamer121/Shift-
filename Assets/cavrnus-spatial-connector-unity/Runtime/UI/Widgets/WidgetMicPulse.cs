using System;
using System.Collections;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class WidgetMicPulse : MonoBehaviour
	{
		[SerializeField] private float fadeVisibilitySpeed = 0.3f;

		[Space]
		[SerializeField] private float pulseSpeed = 0.9f;
		[SerializeField] private float pulseInterval = 0.6f;
		[SerializeField] private Vector2 pulseRange = new Vector2(0.4f, 1.1f);
		[SerializeField] private Vector2 alphaRange = new Vector2(0.3f, 1f);

		[Space]
		[SerializeField] private GameObject inner;
		[SerializeField] private GameObject outer;

		private WaitForSeconds interval;

		private Coroutine pulseRoutine;
		private Coroutine fadeRoutine;
		private IDisposable disposable;

		private CavrnusUser cavUser;

		private bool hasSetup = false;
		public void Setup(CavrnusUser user)
		{
			cavUser = user;

			mainCg = gameObject.AddComponent<CanvasGroup>();
			inner.AddComponent<CanvasGroup>().alpha = 0f;
			outer.AddComponent<CanvasGroup>().alpha = 0f;

			interval = new WaitForSeconds(pulseInterval);

			//Set the volume component to always match the user's data
			BindUserSpeaking(user);

			hasSetup = true;
		}

		private void BindUserSpeaking(CavrnusUser user)
		{
			if (!gameObject.activeInHierarchy)
				return;
			
			disposable = user.BindUserSpeaking(speaking => {
				if (!gameObject.activeSelf) return;
				
				StartCoroutine(FadeRoutine(speaking));

				if (!speaking && pulseRoutine != null)
				{
					StopCoroutine(pulseRoutine);
					pulseRoutine = null;
				}

				if (speaking)
					pulseRoutine = StartCoroutine(PulseIntervalRoutine());
			});
		}

		private void OnEnable()
		{
			if (hasSetup)
				BindUserSpeaking(cavUser);
		}

		private void OnDisable()
		{
			if (pulseRoutine != null)
			{
				StopCoroutine(pulseRoutine);
				pulseRoutine = null;
			}

			disposable?.Dispose();
		}

		private CanvasGroup mainCg;

		private IEnumerator FadeRoutine(bool speaking)
		{
			if (mainCg == null) 
				yield break;
			
			var target = speaking ? 1 : 0;
			var start = mainCg.alpha;

			var elapsed = 0f;
			while (elapsed < 0.3f)
			{
				if (mainCg == null)
					yield break;
				
				mainCg.alpha = Mathf.Lerp(start, target, elapsed / fadeVisibilitySpeed);
				elapsed += Time.deltaTime;

				yield return null;
			}
		}

		private IEnumerator PulseIntervalRoutine()
		{
			while (true)
			{
				StartCoroutine(StartPulse(inner));
				yield return interval;
				StartCoroutine(StartPulse(outer));
				yield return interval;
			}
		}

		private IEnumerator StartPulse(GameObject target)
		{
			var cg = target.GetComponent<CanvasGroup>();

			var elapsedTime = 0f;
			var startingScale = Vector3.one * pulseRange.x;
			var targetScale = Vector3.one * pulseRange.y;

			var startingAlpha = alphaRange.y;
			var targetAlpha = alphaRange.x;

			while (elapsedTime < pulseSpeed)
			{
				var lerpFactor = Mathf.SmoothStep(0f, 1f, elapsedTime / pulseSpeed);
				elapsedTime += Time.deltaTime;

				target.transform.localScale = Vector3.Lerp(startingScale, targetScale, lerpFactor);
				cg.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerpFactor);

				yield return null;
			}
		}

		private void OnDestroy() => disposable?.Dispose();
	}
}