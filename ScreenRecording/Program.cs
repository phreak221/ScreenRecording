using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenRecording
{
    class Program
    {
        static void Main(string[] args)
        {
            var rec = new Record(new Recorder("out.avi", 10, SharpAvi.KnownFourCCs.Codecs.MotionJpeg, 70));
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            rec.Dispose();
        }
    }
}
