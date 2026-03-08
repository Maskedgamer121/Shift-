using System;
using System.Collections;
using Cavrnus.SpatialConnector.Core;
using Cavrnus.SpatialConnector.API;
using Cavrnus.EngineConnector;
using UnityEngine;
using UnityEngine.UI;

namespace Cavrnus.SpatialConnector.UI
{
	public static class CavrnusUserPicStreamHelpers
    {
        public static IDisposable BindUserPicToImage(CavrnusUser user, Image image, AspectRatioFitter ratioFitter)
        {
            var picDisp = user.BindProfilePic(profilePic =>
            {
                if (image != null) {
                    image.sprite = profilePic;
                }
                if (profilePic != null) {
                    ratioFitter.aspectRatio = (float)profilePic.texture.width / (float)profilePic.texture.height;
                }
            });

            return picDisp;
        }
        
        
        public static IDisposable BindUserStreamToRawImage(CavrnusUser user, RawImage image, AspectRatioFitter ratioFitter)
        {
            return user.BindUserVideoFrames(tex => {
                CavrnusStatics.Scheduler.ExecCoRoutine(AssignVidTexture(tex, image, ratioFitter));
            });
        }
        
        private static IEnumerator AssignVidTexture(TextureWithUVs tex, RawImage image, AspectRatioFitter ratioFitter)
        {
            if (tex.Texture.width > 0 && tex.Texture.height > 0)
                ratioFitter.aspectRatio = (float) tex.Texture.width / (float) tex.Texture.height;
            else
                ratioFitter.aspectRatio = 1.67f;
			
            yield return new WaitForSeconds(1f); // Need delay to handle if user is already streaming when loading space

            if (image != null)
            {
               image.texture = tex.Texture; 
               image.uvRect = tex.UVRect;
            }
        }
    }
}