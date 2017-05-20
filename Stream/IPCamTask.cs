using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CashLib.Threading;

using CashLib;
using Console = CashLib.Console;
using System.IO;
using Mono.Unix.Native;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CashCam.Module;

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
            if ((ffmpegProcess == null || ffmpegProcess.HasExited) && Program.ThreadsRunning)
                StartStream(Console.GetValue("Camera[" + ConfigID + "]_URL").Value,
                    string.Format(Console.GetValue("Camera_SAVE_PATH").Value, ConfigID) +
                    Console.GetValue("Camera[" + ConfigID + "]_SAVE_FORMAT").Value);
  
        }

        internal void ReadError()
        {
            StringBuilder input = new StringBuilder();
            while (ffmpegProcess.StandardError.Peek() != -1)
                input.Append(Char.ConvertFromUtf32(ffmpegProcess.StandardError.Read()));

            if (input.Length == 0) return;

            string line = input.ToString();
            Debugging.DebugLog(Debugging.DebugLevel.Debug, line);
        }

        internal void ReadOutput()
        {
            StringBuilder input = new StringBuilder();
            while (ffmpegProcess.StandardOutput.Peek() != -1)
                input.Append(Char.ConvertFromUtf32(ffmpegProcess.StandardOutput.Read()));

            if (input.Length == 0) return;

            string line = input.ToString();
            Debugging.DebugLog(Debugging.DebugLevel.Debug, line);
        }

        internal void Terminate()
        {
            if (ffmpegProcess != null && !ffmpegProcess.HasExited)
            {
                ffmpegProcess.StandardInput.WriteLine("q");
                if(!ffmpegProcess.WaitForExit(5000))
                    ffmpegProcess.Kill();
            }
        }

        private void StartStream(string URL, string filename)
        {
            Debugging.DebugLog(Debugging.DebugLevel.Info, "Starting Camera " + ConfigID);

            if (!new DirectoryInfo(GetDirectory(filename)).Exists)
                new DirectoryInfo(GetDirectory(filename)).Create();

            Debugging.DebugLog(Debugging.DebugLevel.Debug, "Executing: " + Console.GetValue("FFMPEG_PATH").Value + " " + String.Format(Console.GetValue("FFMPEG_STREAM_ARGS").Value, URL, filename));

            ffmpegProcess = new Process();
            ffmpegProcess.StartInfo.FileName = Console.GetValue("FFMPEG_PATH").Value;
            ffmpegProcess.StartInfo.Arguments = String.Format(Console.GetValue("FFMPEG_STREAM_ARGS").Value, URL, filename);
            ffmpegProcess.StartInfo.CreateNoWindow = true;
            ffmpegProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            ffmpegProcess.StartInfo.UseShellExecute = false;
            ffmpegProcess.StartInfo.RedirectStandardInput = true;
            ffmpegProcess.StartInfo.RedirectStandardError = true;
            ffmpegProcess.StartInfo.RedirectStandardOutput = true;
            ffmpegProcess.Start();
        }

        /// <summary>
        /// Extracts the URL from the provided file path. 
        /// Windows version of FileInfo may not like the ffmpeg filenames so this
        /// prevents exceptions
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private string GetDirectory(string file)
        {
            int position = file.Length - 1;
            for (; position > 0; position--)
            {
                if (file[position] == '/' || file[position] == '\\')
                    return file.Substring(0, position);
            }
            throw new ArgumentException(string.Format("Provided file '{0}' is not a full path or does not contain [\\/]", file));
        }
    }
}
