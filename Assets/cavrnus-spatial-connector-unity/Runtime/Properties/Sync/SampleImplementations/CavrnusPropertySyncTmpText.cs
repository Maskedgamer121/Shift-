using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Properties.Sync
{
	[AddComponentMenu("Cavrnus/DataBinding/Sync/SyncTmpText")]
	public class CavrnusPropertySyncTmpText : CavrnusValueSyncString
	{
		[Header("The text component you want to update")]
		public TMP_Text TextComponent;

		public override string GetValue() { return TextComponent.text; }

		public override void SetValue(string value) { TextComponent.text = value; }
	}
}