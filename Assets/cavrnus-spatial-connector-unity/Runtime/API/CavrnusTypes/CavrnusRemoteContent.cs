using System.Collections.Generic;
using System.Linq;
using Cavrnus.Base.Core;
using Cavrnus.Comm.Comm.NotifyApi;
using Cavrnus.Comm.Comm.RestApi;
using Cavrnus.SpatialConnector.Core;

namespace Cavrnus.SpatialConnector.API
{
    /// <summary>
    /// Represents an asset/file/content stored within the Cavrnus Server.
    ///
    /// These aren't constructed by the user, but retrieved via <see cref="CavrnusFunctionLibrary.FetchRemoteContentInfoById"/> or <see cref="CavrnusFunctionLibrary.FetchAllUploadedContent"/>.
    /// </summary>
	public class CavrnusRemoteContent
    {
        /// <summary>
        /// The unique identifier for the content.
        /// </summary>
        public string Id => indo.Id;

        /// <summary>
        /// The name of the content asset.
        /// </summary>
        public string Name => indo.Name.Value;

        /// <summary>
        /// The original filename of the asset.
        /// </summary>
        public string FileName => indo.Filename;

        /// <summary>
        /// A URL for a thumbnail of the object. Not all objects have thumbnails. If it does not the URL will be null or empty.
        /// </summary>
        public string ThumbnailUrl => indo.Thumbnail.Value?.url;

        /// <summary>
        /// The length in bytes of the original content source.
        /// </summary>
        public long FileSize => indo.Assets?.FirstOrDefault(a => a.Value.assetCategory == ObjectAssetCategoryEnum.Canonical).Value?.length ?? 0;

        /// <summary>
        /// The length in bytes of the original content source, but in human readable format.
        /// </summary>
        public string FileSizeString => FileSize.ToPrettySize();

        /// <summary>
        /// True, if this file has been previously retrieved and exists in the local systems file cache. The content cache is stored encrypted, but you may want to know if it is cached as it will load faster, without needing to be downloaded.
        /// </summary>
        public bool CachedOnDisk => CavrnusStatics.ContentManager.ContentCache.TestIsInCache(CavrnusStatics.ContentManager.Endpoint.Server, indo.ToUoiDeprecateMe(), indo.Assets?.FirstOrDefault().Value, out string cachePath);

        /// <summary>
        /// Custom tags/metadata provided when initially creating/uploading this content.
        /// </summary>
		public Dictionary<string, string> Tags 
        { get 
            {
				var dict = new Dictionary<string, string>();
				foreach (var item in indo.Metadata)
					dict[item.Key] = item.Value;
				return dict;
			}
        }

		internal INotifyDataObject indo;
        internal CavrnusRemoteContent(INotifyDataObject indo)
        {
            this.indo = indo;
        }
    }
}