using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using TMPro;
using UnityEngine;

namespace Cavrnus.SpatialConnector.UI
{
	public class SpacePicker : MonoBehaviour
	{
		private class SpacePickerOption : IListElement
		{
			private readonly CavrnusSpaceInfo content;
			private readonly Action<CavrnusSpaceInfo> selected;
			
			public SpacePickerOption(CavrnusSpaceInfo content, Action<CavrnusSpaceInfo> selected)
			{
				this.content = content;
				this.selected = selected;
			}
			
			public void EntryBuilt(GameObject element)
			{
				element.GetComponent<SpacePickerEntry>().Setup(content, selected);
			}
		}
		
		[SerializeField] private TMP_InputField search;
		[SerializeField] private GameObject spacePickerPrefab;
		[SerializeField] private Pagination pagination;
		
		private List<CavrnusSpaceInfo> allSpaces;
		private List<CavrnusSpaceInfo> currentDisplayedSpaces;

		private void Start()
		{
			search.interactable = false;

			CavrnusFunctionLibrary.FetchJoinableSpaces(spaces =>
			{
				allSpaces = spaces;
				currentDisplayedSpaces = allSpaces;

				search.interactable = true;
				search.onValueChanged.AddListener(Search);
				
				UpdatePagination(allSpaces);
			});		
		}

		public void Search(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) {
				pagination.ResetPagination();
				currentDisplayedSpaces.Clear();

				// Default to showing all available spaces
				UpdatePagination(allSpaces);

				return;
			}
			
			// None of this is performant...
			currentDisplayedSpaces = new List<CavrnusSpaceInfo>();
			foreach (var space in allSpaces) {
				if (space.Name.ToLowerInvariant().Contains(value.ToLowerInvariant()))
					currentDisplayedSpaces.Add(space);
			}

			UpdatePagination(currentDisplayedSpaces);
		}

		private void UpdatePagination(List<CavrnusSpaceInfo> spaces)
		{
			var options = new List<IListElement>();
			spaces.Sort((x, y) => DateTime.Compare(y.LastAccessedTime, x.LastAccessedTime));
			spaces.ForEach(s => options.Add(new SpacePickerOption(s, JoinSelectedSpace)));

			pagination.NewPagination(spacePickerPrefab, options);
		}

		private void JoinSelectedSpace(CavrnusSpaceInfo csi)
		{
			CavrnusFunctionLibrary.JoinSpace(csi.Id, (spaceConn) => {
				/*The Post-Load cleanup is done by the Cavrnus Spatial Connector.
				 If you did you own version though, you would need to implement this*/
			}, onFailure =>
			{
				
			});
		}

		private void OnDestroy()
		{
			search.onValueChanged.RemoveListener(Search);
		}
	}
}