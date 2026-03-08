using Cavrnus.Base.Graphics;
using FM.LiveSwitch;
using UnityEngine;
using AudioSource = FM.LiveSwitch.AudioSource;
using LSUnity = FM.LiveSwitch.Unity;
using Matroska = FM.LiveSwitch.Matroska;
using Opus = FM.LiveSwitch.Opus;
using Vp8 = FM.LiveSwitch.Vp8;
using Vp9 = FM.LiveSwitch.Vp9;
using Yuv = FM.LiveSwitch.Yuv;

namespace Cavrnus.RTC
{
    public abstract class LocalMedia : RtcLocalMedia<Texture>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalMedia"/> class.
        /// </summary>
        /// <param name="disableAudio">Whether to disable audio.</param>
        /// <param name="disableVideo">Whether to disable video.</param>
        /// <param name="aecContext">The AEC context, if using software echo cancellation.</param>
        public LocalMedia(bool disableAudio, bool disableVideo, AecContext aecContext)
            : base(disableAudio, disableVideo, aecContext)
        { }

        /// <summary>
        /// Creates an audio recorder.
        /// </summary>
        /// <param name="inputFormat">The input format.</param>
        protected override AudioSink CreateAudioRecorder(AudioFormat inputFormat)
        {
	        if (AudioDisabled)
		        return null;
	        return new Matroska.AudioSink(Id + "-local-audio-" + inputFormat.Name.ToLower() + ".mkv");
        }

        /// <summary>
        /// Creates an audio source.
        /// </summary>
        /// <param name="config">The configuration.</param>
        protected override AudioSource CreateAudioSource(AudioConfig config)
        {
	        if (AudioDisabled)
		        return null;
            return new LSUnity.CavrnusLSAudioClipSource(config);
        }

        /// <summary>
        /// Creates an image converter.
        /// </summary>
        /// <param name="outputFormat">The output format.</param>
        protected override VideoPipe CreateImageConverter(VideoFormat outputFormat)
        {
	        if (VideoDisabled)
		        return null;
            return new Yuv.ImageConverter(outputFormat);
        }

        /// <summary>
        /// Creates an Opus encoder.
        /// </summary>
        /// <param name="config">The configuration.</param>
        protected override AudioEncoder CreateOpusEncoder(AudioConfig config)
        {
	        if (AudioDisabled)
		        return null;
            return new Opus.Encoder(config);
        }

        /// <summary>
        /// Creates a video recorder.
        /// </summary>
        /// <param name="inputFormat">The input format.</param>
        protected override VideoSink CreateVideoRecorder(VideoFormat inputFormat)
        {
	        if (VideoDisabled)
		        return null;
            return new Matroska.VideoSink(Id + "-local-video-" + inputFormat.Name.ToLower() + ".mkv");
        }

        /// <summary>
        /// Creates a VP8 encoder.
        /// </summary>
        protected override VideoEncoder CreateVp8Encoder()
        {
	        if (VideoDisabled)
		        return null;
    #if PLATFORM_LUMIN
	        return null;
    #else
            return new Vp8.Encoder();
    #endif
        }

        /// <summary>
        /// Creates an H.264 encoder.
        /// </summary>
        protected override VideoEncoder CreateH264Encoder()
        {
	        if (VideoDisabled)
		        return null;
    #if PLATFORM_LUMIN
	        return null;
    #else
            // OpenH264 requires a runtime download from Cisco
            // for licensing reasons, which is not currently
            // supported on Unity.
            return null;
    #endif
        }

        /// <summary>
        /// Creates a VP9 encoder.
        /// </summary>
        protected override VideoEncoder CreateVp9Encoder()
        {
	        if (VideoDisabled)
		        return null;
    #if PLATFORM_LUMIN
	        return null;//return new FM.LiveSwitch.MagicLeap.Vp9.Encoder();
    #else
            return new Vp9.Encoder();
    #endif
        }
    }

    public class LocalCompositeMedia : LocalMedia
    {
	    /// <summary>
	    /// Initializes a new instance of the <see cref="LocalTexture2DMedia"/> class.
	    /// </summary>
	    /// <param name="disableAudio">Whether to disable audio.</param>
	    /// <param name="disableVideo">Whether to disable video.</param>
	    /// <param name="aecContext">The AEC context, if using software echo cancellation.</param>
	    public LocalCompositeMedia(bool disableAudio, bool disableVideo, AecContext aecContext)
		    : base(disableAudio, disableVideo, aecContext)
	    {
		    VideoSimulcastDegradationPreference = VideoDegradationPreference.Automatic;
		    VideoSimulcastEncodingCount = 3;
		    VideoSimulcastPreferredBitrate = RtcRegistrySettings.GetPreferredBitrate() ?? 1024;
		    VideoSimulcastDisabled = false;

    #if PLATFORM_LUMIN
		    VideoSimulcastDisabled = true;
    #endif
        
		    Initialize();
	    }

	    /// <summary>
	    /// Creates a video source.
	    /// </summary>
	    protected override VideoSource CreateVideoSource()
	    {
		    if (VideoDisabled)
			    return null;
    #if PLATFORM_LUMIN
            return new NullVideoSource(VideoFormat.Rgba);
    #else
            return new CompositeVideoSourceUnity(VideoFormat.Rgba);
    #endif
	    }

	    /// <summary>
	    /// Creates a view sink.
	    /// </summary>
	    protected override ViewSink<Texture> CreateViewSink()
	    {
		    // Texture capture doesn't generally need a preview.
		    // If you want one, return a new RectTransformSink here.
		    return null;
	    }

	    public override Texture View
	    {
		    get { return (Texture)((VideoSource as CompositeVideoSourceUnity)?.CurrentTexture); }
	    }


        /// <summary>
        /// Gets or sets the underlying Texture2D.
        /// </summary>
        public void UpdateStreamingTextureSource(Texture2D texSrc)
	    {
		    (VideoSource as CompositeVideoSourceUnity)?.UpdateTextureSource(texSrc);
	    }


        public void ProvideImageToImageVideoSource(IImage2D image)
        {
	        (VideoSource as CompositeVideoSourceUnity)?.ReceiveImagesVideoSourceImage(image);
        }
    }
}