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
using CashCam.Modules;
using CashLib.Exceptions;

namespace CashCam.OGGStream
{
    class IPCamSaveTask
    {
        Process StreamSaveThread;
        public int ID { get; private set; }

        public IPCamSaveTask(int id)
        {
            ID = id;
        }

        public void CheckTask(bool LongRun)
        {
            if (!LongRun) return;
            ConsoleResponseBoolean variable = Console.GetOnOff(string.Format(Variables.V_camera_enabled, ID));
            if (variable.State == ConsoleCommandState.Failure)
                throw new LogicException(string.Format("{0} is not a boolean variable!", Variables.V_camera_enabled));
            if (variable.Value)
            {
                if ((StreamSaveThread == null || StreamSaveThread.HasExited) && Program.ThreadsRunning)
                    StartStream(Console.GetValue(string.Format(Variables.V_camera_url, ID)).Value,
                        string.Format(Console.GetValue(Variables.V_camera_save_path).Value, ID) +
                        Console.GetValue(string.Format(Variables.V_camera_save_format, ID)).Value);
            } else
            {
                if ((StreamSaveThread != null && !StreamSaveThread.HasExited) && Program.ThreadsRunning)
                {
                    Console.WriteLine("Killing camera {0}", ID);
                    Terminate();
                }
            }
        }

        internal void Terminate()
        {
            if (StreamSaveThread != null && !StreamSaveThread.HasExited)
            {
                StreamSaveThread.StandardInput.Close();
                if (!StreamSaveThread.WaitForExit(5000))
                {
                    Console.WriteLine("Camera {0} did not close after 5 seconds, forcefully terminating!", ID);
                    StreamSaveThread.Kill();
                }
            }
        }

        private void StartStream(string URL, string filename)
        {
            Debugging.DebugLog(Debugging.DebugLevel.Info, "Starting Camera " + ID);

            if (!new DirectoryInfo(GetDirectory(filename)).Exists)
                new DirectoryInfo(GetDirectory(filename)).Create();

            Debugging.DebugLog(Debugging.DebugLevel.Debug1, "Executing: " + Console.GetValue(Variables.V_encoder_path).Value + " " + String.Format(Console.GetValue(Variables.V_encoder_stream_args).Value, URL, filename));

            StreamSaveThread = new Process();
            StreamSaveThread.StartInfo.FileName = Console.GetValue(Variables.V_encoder_path).Value;
            StreamSaveThread.StartInfo.Arguments = String.Format(Console.GetValue(Variables.V_encoder_save_args).Value, URL, filename);
            StreamSaveThread.StartInfo.CreateNoWindow = true;
            StreamSaveThread.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            StreamSaveThread.StartInfo.UseShellExecute = false;
            StreamSaveThread.StartInfo.RedirectStandardInput = true;
            StreamSaveThread.StartInfo.RedirectStandardError = true;
            StreamSaveThread.StartInfo.RedirectStandardOutput = true;

            StreamSaveThread.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                Debugging.DebugLog(Debugging.DebugLevel.Debug3, e.Data);
            };
            StreamSaveThread.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                Debugging.DebugLog(Debugging.DebugLevel.Debug3, e.Data);
            };

            StreamSaveThread.Start();
            StreamSaveThread.BeginErrorReadLine();
            StreamSaveThread.BeginOutputReadLine();
        }


        ///  <summary>
        ///  Extracts the URL from the provided file path. 
        ///  Windows version of FileInfo may not like the generated filenames so this
        ///  prevents exceptions
        ///  </summary>
        ///  <param name="file"></param>
        ///  <returns></returns>
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
