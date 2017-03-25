using System;
using System.Collections.Generic;
using CashLib.Module;
using CashLib;
using Console = CashLib.TConsole;
using CashLib.Localization;

namespace CashCam.Modules
{
    class Variables : IModuleLoader
    {
        public Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public string Name
        {
            get { return "Variables"; }
        }

        private string Filename = "variables.ini";

        public void Load()
        {
            SetupVariables();
            Console.ProcessFile(Filename);
            Program.ProgramEnding += Save;
        }

        private void Save()
        {
            //We have 1 camera we are saving.
            List<string> VariablesToSave = new List<string>();
            VariablesToSave.Add("FFMPEG_PATH");
            VariablesToSave.Add("FFMPEG_STREAM_ARGS");
            VariablesToSave.Add("Camera_SAVE_PATH");
            VariablesToSave.Add("Camera[0]_URL");
            VariablesToSave.Add("Camera[0]_SAVE_FORMAT");
            Console.SaveToFile(Filename, VariablesToSave.ToArray());
        }

        private void SetupVariables()
        {
            Console.SetValue("FFMPEG_PATH", new ConsoleVarable()
            {
                Value = "/usr/bin/ffmpeg",
                HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_PATH_Help"),
            });
            Console.SetValue("FFMPEG_STREAM_ARGS", new ConsoleVarable()
            {
                Value = "-i {0} -an -c copy -map 0 -f segment -segment_time 1800 " +
                "-segment_atclocktime 1 -segment_format mp4 -strftime 1 {1}",
                HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_SAVE_ARGS_Help"),
            });

            Console.SetValue("Camera_SAVE_PATH", new ConsoleVarable()
            {
                Value = "/media/sv/cam{0}/"
            });

            Console.SetValue("Camera[0]_URL", new ConsoleVarable()
            {
                Value = "rtsp://10.0.0.49/live1.264",
            });

            Console.SetValue("Camera[0]_SAVE_FORMAT", new ConsoleVarable()
            {
                Value = "%Y-%m-%d-%H:%M:%S.mp4",
            });
        }
    }
}