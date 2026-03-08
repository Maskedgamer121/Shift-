using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[AddComponentMenu("Cavrnus/DataBinding/Sync/SyncVisibility")]
	public class CavrnusPropertySyncVisibility : CavrnusValueSyncBool
	{
		public override bool GetValue()
		{
			return gameObject.activeSelf;
		}

		public override void SetValue(bool value)
		{
			gameObject.SetActive(value);
		}

		private void Reset()
		{
			PropertyName = "Visible";
		}
	}
}