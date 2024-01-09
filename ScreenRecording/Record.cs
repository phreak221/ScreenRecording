using SharpAvi.Output;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenRecording
{
    public class Record : IDisposable
    {
        private AviWriter _writer;
        private Recorder _recordParams;
        private IAviVideoStream _videoStream;
        private Thread _screenThread;
        private ManualResetEvent _stopThread = new ManualResetEvent(false);

        public Record(Recorder recordParams)
        {
            _recordParams = recordParams;
            _writer = _recordParams.CreateAviWriter();

            _videoStream = _recordParams.CreateVideoStream(_writer);
            _videoStream.Name = "Capture";

            _screenThread = new Thread(RecordScreen)
            {
                Name = typeof(Record).Name + ".RecordScreen",
                IsBackground = true
            };
            _screenThread.Start();
        }

        public void Dispose()
        {
            _stopThread.Set();
            _screenThread.Join();
            _writer.Close();
            _stopThread.Dispose();
        }

        public void RecordScreen()
        {
            var frameInterval = TimeSpan.FromSeconds(1 / (double)_writer.FramesPerSecond);
            var buffer = new byte[_recordParams.Width * _recordParams.Height * 4];
            Task videoWriteTask = null;
            var timeTillNextFrame = TimeSpan.Zero;

            while (!_stopThread.WaitOne(timeTillNextFrame))
            {
                var timestamp = DateTime.Now;
                Screenshot(buffer);
                videoWriteTask?.Wait();
                videoWriteTask = _videoStream.WriteFrameAsync(true, buffer, 0, buffer.Length);
                timeTillNextFrame = timestamp + frameInterval - DateTime.Now;
                if (timeTillNextFrame < TimeSpan.Zero)
                    timeTillNextFrame = TimeSpan.Zero;
            }
        }

        public void Screenshot(byte[] Buffer)
        {
            using (var BMP = new Bitmap(_recordParams.Width, _recordParams.Height))
            {
                using (var g = Graphics.FromImage(BMP))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, new Size(_recordParams.Width, _recordParams.Height), CopyPixelOperation.SourceCopy);
                    g.Flush();
                    var bits = BMP.LockBits(new Rectangle(0, 0, _recordParams.Width, _recordParams.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                    Marshal.Copy(bits.Scan0, Buffer, 0, Buffer.Length);
                    BMP.UnlockBits(bits);
                }
            }
        }
    }
}
