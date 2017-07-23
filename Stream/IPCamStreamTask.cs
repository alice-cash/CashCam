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

namespace CashCam.Stream
{
    class IPCamStreamTask
    {
        Process encoderProcess;
        public int ID { get; private set; }

        public IPCamStreamRepeater Repeater { get; private set; }

        public bool IsRunning { get { return encoderProcess != null && !encoderProcess.HasExited; } }

        private string hostname;
        private int port;
        private static Random r = new Random();

        public Camera Parent;

        public IPCamStreamTask(Camera parent, int id)
        {
            ID = id;
            Parent = parent;
            hostname = "127.0.0.1";
            port = r.Next(20000, 29999);
            Repeater = new IPCamStreamRepeater(this, "http://" + hostname + ":" + port + "/camera.ogg"); //, port);
        }

        public void CheckTask(bool LongRun)
        {
            Repeater.RunTask();
            if (LongRun)
            {
                ConsoleResponseBoolean variable = Console.GetOnOff(string.Format(Variables.V_camera_stream_enabled, ID));
                if (variable.State == ConsoleCommandState.Failure)
                    throw new LogicException(string.Format("{0} is not a boolean variable!", Variables.V_camera_stream_enabled));
                if (variable.Value)
                {
                    if ((encoderProcess == null || encoderProcess.HasExited) && Program.ThreadsRunning)
                        StartStream(Console.GetValue(string.Format(Variables.V_camera_url, ID)).Value,
                            hostname);
                }
                else
                {
                    if ((encoderProcess != null && !encoderProcess.HasExited) && Program.ThreadsRunning)
                    {
                        Console.WriteLine("Killing camera {0}", ID);
                        Terminate();
                    }
                }
            }
        }

        internal void Terminate()
        {
            Repeater?.Stop(true);
            if (encoderProcess != null && !encoderProcess.HasExited)
            {
                encoderProcess.StandardInput.WriteLine("\x3");
                encoderProcess.CloseMainWindow();

                if (Program.CurrentOS == OS.Linux)
                {
                    Syscall.kill(encoderProcess.Id, Signum.SIGTERM);
                    if (!encoderProcess.WaitForExit(5000))
                    {
                        Console.WriteLine("Camera {0} did not close after 5 seconds, forcefully terminating!", ID);
                        Syscall.kill(encoderProcess.Id, Signum.SIGKILL);
                    }
                } else if (!encoderProcess.WaitForExit(5000))
                {
                    Console.WriteLine("Camera {0} did not close after 5 seconds, forcefully terminating!", ID);
                    encoderProcess.Kill();
                }

            }
        }

        private void StartStream(string URL, string hostname)
        {
            Repeater?.Start();
            Debugging.DebugLog(Debugging.DebugLevel.Info, "Starting Camera Sttram" + ID);

            Debugging.DebugLog(Debugging.DebugLevel.Debug1, "Executing: " +
                Console.GetValue(Variables.V_encoder_path).Value + " " + 
                String.Format(Console.GetValue(string.Format(Variables.V_camera_stream_args, ID)).Value, URL, string.Format("{0}:{1}/camera.ogg", hostname, port)));

            encoderProcess = new Process();
            encoderProcess.StartInfo.FileName = Console.GetValue(Variables.V_encoder_path).Value;
            encoderProcess.StartInfo.Arguments = String.Format(Console.GetValue(string.Format(Variables.V_camera_stream_args, ID)).Value, URL, string.Format("{0}:{1}/camera.ogg", hostname, port));
            encoderProcess.StartInfo.CreateNoWindow = true;
            encoderProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            encoderProcess.StartInfo.UseShellExecute = false;
            encoderProcess.StartInfo.RedirectStandardInput = true;
            encoderProcess.StartInfo.RedirectStandardError = true;
            encoderProcess.StartInfo.RedirectStandardOutput = true;

            encoderProcess.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                Debugging.DebugLog(Debugging.DebugLevel.Debug3, e.Data);
            };
            encoderProcess.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                Debugging.DebugLog(Debugging.DebugLevel.Debug3, e.Data);
            };

            encoderProcess.Start();
            encoderProcess.BeginErrorReadLine();
            encoderProcess.BeginOutputReadLine();

        }


        /// <summary>
        /// Extracts the URL from the provided file path. 
        /// Windows version of FileInfo may not like the encoder filenames so this
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
