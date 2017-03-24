using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CashLib.Threading;

using CashLib;
using Console = CashLib.TConsole;

namespace CashCam.Stream
{
    class IPCamTask
    {
        string FullURL;
        StreamConfig config;
        Process ffmpegProcess;

        public void CheckTask()
        {
            if (ffmpegProcess == null || ffmpegProcess.HasExited)
                StartStream("/media/sv/cam2/%Y-%m-%d-%H:%M:%S.mp4");
        }

        void StartStream(string filename)
        {
            /*ffmpeg -i 'rtsp://10.0.0.49/live1.264'
             -an -c copy -map 0 -f segment -segment_time 1800 -segment_atclocktime 1 
             -segment_format mp4 -strftime 1  $PWD/"%Y-%m-%d-%H:%M:%S.mp4"*/


            ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = Console.GetValue("FFMPEG_PATH").Value;
            ffmpegProcess.StartInfo.Arguments = String.Format(Console.GetValue("FFMPEG_SAVE_ARGS").Value, FullURL, filename);
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.Start();
        }
    }
}
