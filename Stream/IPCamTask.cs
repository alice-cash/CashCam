using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CashLib.Threading;

using CashLib;
using Console = CashLib.TConsole;
using System.IO;

namespace CashCam.Stream
{
    class IPCamTask
    {
        Process ffmpegProcess;
        int ConfigID;

        public IPCamTask(int id)
        {
            ConfigID = id;
        }

        public void CheckTask()
        {
            if (ffmpegProcess == null || ffmpegProcess.HasExited)
                StartStream(Console.GetValue("Camera[" + ConfigID + "]_URL").Value,
                    string.Format(Console.GetValue("Camera_SAVE_PATH").Value, ConfigID) +
                    Console.GetValue("Camera[" + ConfigID + "]_SAVE_FORMAT").Value);
        }

        internal void Terminate()
        {
            if (ffmpegProcess != null && ffmpegProcess.HasExited)
                ffmpegProcess.Kill();
        }

        void StartStream(string URL, string filename)
        {
            Console.WriteLine("Starting Camera " + ConfigID);
            /*ffmpeg -i 'rtsp://10.0.0.49/live1.264'
             -an -c copy -map 0 -f segment -segment_time 1800 -segment_atclocktime 1 
             -segment_format mp4 -strftime 1  $PWD/"%Y-%m-%d-%H:%M:%S.mp4"*/

            if (!new FileInfo(filename).Directory.Exists)
                new FileInfo(filename).Directory.Create();
            Console.WriteLine(Console.GetValue("FFMPEG_PATH").Value + " " + String.Format(Console.GetValue("FFMPEG_STREAM_ARGS").Value, URL, filename));

            ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = Console.GetValue("FFMPEG_PATH").Value;
            ffmpegProcess.StartInfo.Arguments = String.Format(Console.GetValue("FFMPEG_STREAM_ARGS").Value, URL, filename);
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.Start();
        }
    }
}
