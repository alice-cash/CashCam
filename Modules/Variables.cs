using System;
using System.Collections.Generic;
using CashLib.Module;
using CashLib;
using Console = CashLib.Console;
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
            List<string> VariablesToSave = new List<string>()
            {
                "FFMPEG_PATH",
                "FFMPEG_STREAM_ARGS",
                "Camera_SAVE_PATH",
                "Camrea_Count",

            };

            Console.SaveToFile(Filename, VariablesToSave.ToArray());
        }

        private void Save_cameras()
        {
            //This should be safe, the CheckConsoleInput won't let us set non-numbers
            int count = int.Parse(Console.GetVariable("Debugging_Level").Value);

            List<string> VariablesToSave = new List<string>();

            for (int id = 0; id < count; id++)
            {

                VariablesToSave.AddRange(new string[]
                {
                    "Camera[" + id + "]_URL",
                    "Camera[" + id + "]_SAVE_FORMAT"
                });
            }

            Console.SaveToFile(Filename, VariablesToSave.ToArray());
        }

        private void CheckCameraVariables()
        {

        }


        /// <summary>
        /// Verify the input for Camrea_Count is valid and configure any values that don't exsist.
        /// </summary>
        /// <param name="input">Entered level</param>
        /// <returns>Returned status regarding success or failure of input</returns>
        ExecutionState CheckConsoleInput(string input)
        {
            input = input.Trim();
            // int count;

            if (int.TryParse(input, out int count))
            {
                if (count > 0)
                {
                    for (int id = 0; id < count; id++)
                    {
                        SetupCamera(id);
                    }

                    return ExecutionState.Succeeded();
                }
            }

            return ExecutionState.Failed("Input must be a number greater than 0.");
        }

        private void SetupCamera(int id)
        {
            Console.SetIfNotExsistValue("Camera[" + id + "]_URL", new ConsoleVarable()
            {
                Value = "rtsp://10.0.0.49/live1.264",
                HelpInfo = DefaultLanguage.Strings.GetString("Camera_URL_Help"),
            });

            if (Program.CurrentOS == OS.Windows)
            {
                Console.SetIfNotExsistValue("Camera[" + id + "]_SAVE_FORMAT", new ConsoleVarable()
                {
                    Value = "%Y-%m-%d-%H_%M_%S.mp4",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_SAVE_FORMAT_Help"),
                });
            }
            else
            {
                Console.SetIfNotExsistValue("Camera[" + id + "]_SAVE_FORMAT", new ConsoleVarable()
                {
                    Value = "%Y-%m-%d-%H:%M:%S.mp4",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_SAVE_FORMAT_Help"),
                });
            }
        }

        private void SetupVariables()
        {

            Console.SetValue("Camrea_Count", new ConsoleVarable()
            {
                Value = "1",
                HelpInfo = DefaultLanguage.Strings.GetString("Camrea_Count_Help"),
                ValidCheck = CheckConsoleInput,
            });

            SetupCamera(0);

            if (Program.CurrentOS == OS.Windows)
            {
                Console.SetValue("FFMPEG_PATH", new ConsoleVarable()
                {
                    Value = Environment.CurrentDirectory + "\\bin\\ffmpeg.exe",
                    HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_PATH_Help"),
                });
                Console.SetValue("Camera_SAVE_PATH", new ConsoleVarable()
                {
                    Value = Environment.CurrentDirectory + "\\sv\\cam{0}\\",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_SAVE_PATH_Help"),
                });
            }
            else
            {
                Console.SetValue("FFMPEG_PATH", new ConsoleVarable()
                {
                    Value = "/usr/bin/ffmpeg",
                    HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_PATH_Help"),
                });
                Console.SetValue("Camera_SAVE_PATH", new ConsoleVarable()
                {
                    Value = "/media/sv/cam{0}/",
                    HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_SAVEPATH_Help"),
                });
            }

            Console.SetValue("FFMPEG_STREAM_ARGS", new ConsoleVarable()
            {
                Value = "-i {0} -an -c copy -map 0 -f segment -segment_time 1800 " +
                "-segment_atclocktime 1 -segment_format mp4 -strftime 1 {1}",
                HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_SAVE_ARGS_Help"),
            });
        }

    }
}