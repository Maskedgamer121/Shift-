using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.Properties;
using UnityEngine;
#if CAVRNUS_URP_PRESENT
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace Cavrnus.SpatialConnector.Samples.CollaborationShowcase
{
	public class CavrnusPostProcessingManager : MonoBehaviour
	{
#if CAVRNUS_URP_PRESENT
		[SerializeField] private Volume volume;
		
		[SerializeField] private CavrnusPropertyBinderBool saturationEnabled;
		[SerializeField] private CavrnusPropertyBinderFloat saturation;
		[SerializeField] private CavrnusPropertyBinderBool bloomEnabled;
		[SerializeField] private CavrnusPropertyBinderFloat bloom;
		
		private List<IDisposable> disp = new List<IDisposable>();
		
		private void Start()
		{
			if (volume == null)
			{
				volume = FindFirstObjectByType<Volume>();
				if (volume == null)
				{
					Debug.LogWarning("Missing PostProcessing Volume in Scene!");
					return;
				}
			}
			if (volume.profile.TryGet(out ColorAdjustments ca))
			{
				disp.Add(saturationEnabled.BindProperty(val => ca.saturation.overrideState = val));
				disp.Add(saturation.BindProperty(val => ca.saturation.value = val));
			}
			
			if (volume.profile.TryGet(out Bloom b))
			{
				disp.Add(bloomEnabled.BindProperty(val => b.active = val));
				disp.Add(bloom.BindProperty(val => b.intensity.value = val));
			}
		}
		private void OnDestroy() => disp.ForEach(d => d?.Dispose());
#endif
	}
}