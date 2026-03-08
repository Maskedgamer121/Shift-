using Cavrnus.Base.Graphics;
using FM.LiveSwitch;
using FM.LiveSwitch.Yuv;

namespace Cavrnus.RTC
{
	public class ImagesVideoSource : VideoSource
	{
		private bool isRunning = false;
		private ImageConverter imgConverter;

		private Image2D resolvedIm;

		public ImagesVideoSource() : base(VideoFormat.Rgba)
		{
			imgConverter = new ImageConverter(VideoFormat.Rgba);
		}

		public void ImageProvided(IImage2D iim)
		{
			if (!isRunning)
				return;

			iim.ResolveToImage2D(ref resolvedIm); // TODO: Figure out how to use a native buffer here without a copy.

			var dataBufferRgba = FM.LiveSwitch.DataBuffer.Wrap(resolvedIm.ImageData);
			var vidBufferRgba = new FM.LiveSwitch.VideoBuffer(resolvedIm.Resolution.x, resolvedIm.Resolution.y, dataBufferRgba, VideoFormat.Rgba);

			imgConverter.ProcessBuffer(vidBufferRgba).Then((vidBufferBgr) =>
			{
				var vidFrame = new FM.LiveSwitch.VideoFrame(vidBufferBgr);
				this.RaiseFrame(vidFrame);
				//vidBufferRgba.Free();
			});
		}

		protected override Future<object> DoStart()
		{
			isRunning = true;
			Promise<object> f = new Promise<object>();
			f.Resolve(null);
			return f;
		}

		protected override Future<object> DoStop()
		{
			isRunning = true;
			Promise<object> f = new Promise<object>();
			f.Resolve(null);
			resolvedIm = null; // release
			return f;
		}

		public override string Label => "Images";
	}
}