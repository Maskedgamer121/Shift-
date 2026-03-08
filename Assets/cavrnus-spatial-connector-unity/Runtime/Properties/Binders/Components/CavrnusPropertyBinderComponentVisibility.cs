using System;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Binders
{
	[AddComponentMenu("Cavrnus/DataBinding/Binders/BindVisibility")]
	public class CavrnusPropertyBinderComponentVisibility : CavrnusPropertyBinderComponent
	{
		[Header("Cavrnus Property")]
	    [SerializeField] private CavrnusPropertyBinderBool vis;
		[SerializeField] private GameObject target;
		
	    private IDisposable binding;
	    
		private void Start()
		{
			binding = vis.BindProperty(v =>
			{
				if (target != null) target.SetActive(v);
			});
		}
		
		private void OnDestroy() => binding?.Dispose();
	}
}