using System;
using Cavrnus.SpatialConnector.API;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public class WidgetUserProfileImage : MonoBehaviour
	{
		[SerializeField] private Image image;
		[SerializeField] private AspectRatioFitter aspectRatioFitter;

		private IDisposable profileDisposable;

		public void Setup(CavrnusUser cu)
		{
			profileDisposable = cu.BindProfilePic(profilePic =>
			{
                image.sprite = profilePic;
                if (profilePic != null)
                    aspectRatioFitter.aspectRatio = (float)profilePic.texture.width / (float)profilePic.texture.height;
            });
		}

        private void OnDestroy()
		{
			profileDisposable?.Dispose();
		}
	}
}