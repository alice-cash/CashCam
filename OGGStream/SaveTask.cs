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
    class SaveTask
    {
       // Process StreamSaveThread;
        FileStream stream;
        public int ID { get; private set; }
        private DateTime NextSegment;
        private DateTime NextFlush;

        public SaveTask(int id)
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
                if ((stream == null || !stream.CanWrite) && Program.ThreadsRunning)
                    StartStream();
                else
                {
                    if (DateTime.Now >= NextSegment)
                        StartStream();
                }

                if (DateTime.Now >= NextFlush && LongRun)
                {
                    stream.Flush(true);
                    NextFlush = DateTime.Now.AddMinutes(1);
                }

            }
            else
            {
               /* if ((StreamSaveThread != null && !StreamSaveThread.HasExited) && Program.ThreadsRunning)*/
               if(stream != null)
                {
                    Console.WriteLine("Killing camera {0}", ID);
                    Terminate();
                }
            }
        }


        internal void Terminate()
        {
            try { stream?.Close(); } catch { };
            stream = null;

            /*    if (StreamSaveThread != null && !StreamSaveThread.HasExited)
                {
                    StreamSaveThread.StandardInput.Close();
                    if (!StreamSaveThread.WaitForExit(5000))
                    {
                        Console.WriteLine("Camera {0} did not close after 5 seconds, forcefully terminating!", ID);
                        StreamSaveThread.Kill();
                    }
                }*/
        }

        private void StartStream()
        {
            string filename = string.Format(Console.GetValue(Variables.V_camera_save_path).Value, ID) +
                DateTime.Now.ToString(Console.GetValue(string.Format(Variables.V_camera_save_format, ID)).Value);

            Debugging.DebugLog(Debugging.DebugLevel.Info, "Starting Camera " + ID);
            //We need to make sure the stream task is started also so we can pull the data from it
            ConsoleResponseBoolean variable = Console.GetOnOff(string.Format(Variables.V_camera_stream_enabled, ID));
            if (variable.State == ConsoleCommandState.Failure)
                throw new LogicException(string.Format("{0} is not a boolean variable!", Variables.V_camera_stream_enabled));
            if (!variable.Value)
            {
                Console.SetValue(string.Format(Variables.V_camera_stream_enabled, ID), "True");
            }


            try { stream?.Close(); } catch { };

            File.Delete(filename);
            stream = File.OpenWrite(filename);

            Program.CameraManager.GetGamera(ID)
                .StreamTask.Repeater.AddStream(
                new StreamClient(stream, (data, offset, length) => {
                    try { stream.Write(data, offset, length);

                  /*      string s = ASCIIEncoding.ASCII.GetString(data, 0, data.Length);
                        var sb = new StringBuilder();
                        int i = 0;

                        sb.Append(" Capture Pattern: ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append("\n");

                        sb.Append("         Version: ")
                            .Append(data[i++].ToString("X2")).Append("\n");

                        sb.Append("     Header Type: ")
                            .Append(data[i++].ToString("X2")).Append("\n");

                        sb.Append("Granule Position: ")
                             .Append(BitConverter.ToUInt64(new byte[] { data[6],  data[7],  data[8], data[9], data[10], data[11], data[12], data[13] }, 0)).Append("\n");

                        sb.Append("Granule Position: ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append("\n");

                        sb.Append("Bitstream Number: ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append("\n");

                        sb.Append(" Page Seq Number: ")
                            .Append(data[i++].ToString("X2")).Append(" ") // 18
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append("\n");

                        sb.Append("        Checksum: ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append(" ")
                            .Append(data[i++].ToString("X2")).Append("\n");


                        sb.Append("\n");
                        Console.Write(sb.ToString());*/

                        return true; } catch { }
                    return false;
                })
            );

            NextSegment = DateTime.Now.AddMinutes(30);
            NextFlush = DateTime.Now.AddMinutes(1);
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
