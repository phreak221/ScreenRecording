using SharpAvi;
using SharpAvi.Codecs;
using SharpAvi.Output;
using System.Windows.Forms;

namespace ScreenRecording
{
    public class Recorder
    {
        private string _filename;
        private int _framerate;
        private FourCC _codec;
        private int _quality;

        public int Height { get; private set; }
        public int Width { get; private set; }

        public Recorder(string filename, int frameRate, FourCC encoder, int quality)
        {
            _filename = filename;
            _framerate = frameRate;
            _codec = encoder;
            _quality = quality;

            Height = Screen.PrimaryScreen.Bounds.Height;
            Width = Screen.PrimaryScreen.Bounds.Width;
        }

        public AviWriter CreateAviWriter()
        {
            return new AviWriter(_filename)
            {
                FramesPerSecond = _framerate,
                EmitIndex1 = true,
            };
        }

        public IAviVideoStream CreateVideoStream(AviWriter writer)
        {
            if (_codec == KnownFourCCs.Codecs.Uncompressed)
                return writer.AddUncompressedVideoStream(Width, Height);
            else if (_codec == KnownFourCCs.Codecs.MotionJpeg)
                return writer.AddMotionJpegVideoStream(Width, Height, _quality);
            else
            {
                return writer.AddMpeg4VideoStream(Width, Height, (double)writer.FramesPerSecond, quality: _quality, codec: _codec, forceSingleThreadedAccess: true);
            }
        }
    }
}
