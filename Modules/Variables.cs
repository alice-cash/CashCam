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
        private string FilenameSettings = "config.ini";
        private string FilenameCameras = "cameras.ini";


        public const string V_encoder_path = "ENCODER_Path";
        public const string V_encoder_stream_args = "ENCODER_Default_Stream_Args";
        public const string V_camera_save_path = "Camera_Save_Path";
        public const string V_camera_count = "Camera_Count";
        public const string V_camera_url = "Camera[{0}]_URL";
        public const string V_camera_save_format = "Camera[{0}]_Save_Format";
        public const string V_camera_stream_args = "Camera[{0}]_Stream_Args";
        public const string V_camera_enabled = "Camera[{0}]_Enabled";
        public const string V_camera_stream_enabled = "Camera[{0}]_Stream_Enabled";

        public Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public string Name
        {
            get { return "Variables"; }
        }

        public void Load()
        {
            SetupVariables();
            Console.ProcessFile(FilenameSettings);
            Console.ProcessFile(FilenameCameras);
            Program.ProgramEnding += Save;
        }

        private void Save()
        {
            // We have 1 camera we are saving.
            List<string> VariablesToSave = new List<string>()
            {
                V_encoder_path,
                V_encoder_stream_args,
                V_camera_save_path,
            };

            Console.SaveToFile(FilenameSettings, VariablesToSave.ToArray());
            Save_cameras();
        }

        private void Save_cameras()
        {
            // This should be safe, the CheckConsoleInput won't let us set non-numbers
            int count = int.Parse(Console.GetVariable(V_camera_count).Value);

            List<string> VariablesToSave = new List<string>() { V_camera_count };

            for (int id = 0; id < count; id++)
            {

                VariablesToSave.AddRange(new string[]
                {
                    string.Format(V_camera_url,id),
                    string.Format(V_camera_stream_args,id),
                    string.Format(V_camera_save_format,id),
                    string.Format(V_camera_enabled,id),
                    string.Format(V_camera_stream_enabled,id),
                });
            }

            Console.SaveToFile(FilenameCameras, VariablesToSave.ToArray());
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
            Console.SetIfNotExsistValue(string.Format(V_camera_enabled, id), ConsoleVarable.OnOffVarable(
                 DefaultLanguage.Strings.GetString("Camera_Enabled_Help")));

            Console.SetIfNotExsistValue(string.Format(V_camera_stream_enabled, id), ConsoleVarable.OnOffVarable(
                DefaultLanguage.Strings.GetString("Camera_Stream_Enabled_Help")));




            Console.SetIfNotExsistValue(string.Format(V_camera_url, id), new ConsoleVarable()
            {
                Value = "rtsp://10.0.0.254/live1.264",
                HelpInfo = DefaultLanguage.Strings.GetString("Camera_URL_Help"),
            });

            if (Program.CurrentOS == OS.Windows)
            {
                Console.SetIfNotExsistValue(string.Format(V_camera_save_format, id), new ConsoleVarable()
                {
                    Value = "yyyy-MM-dd-HH-MM-ss.ogg",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_SAVE_FORMAT_Help"),
                });
            }
            else
            {
                Console.SetIfNotExsistValue(string.Format(V_camera_save_format, id), new ConsoleVarable()
                {
                    Value = "yyyy-MM-dd-HH:MM:ss.\\o\\g\\g",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_SAVE_FORMAT_Help"),
                });
            }


            Console.SetIfNotExsistValue(string.Format(V_camera_stream_args, id), new ConsoleVarable()
            {
                Value = Console.GetValue(V_encoder_stream_args).Value,
                HelpInfo = DefaultLanguage.Strings.GetString("Encoder_Stream_ARGS_Help"),
            });


        }

        private void SetupVariables()
        {

            Console.SetIfNotExsistValue(V_camera_count, new ConsoleVarable()
            {
                Value = "1",
                HelpInfo = DefaultLanguage.Strings.GetString("Camrea_Count_Help"),
                ValidCheck = CheckConsoleInput,
            });

            if (Program.CurrentOS == OS.Windows)
            {
                Console.SetIfNotExsistValue(V_encoder_path, new ConsoleVarable()
                {
                    Value = Environment.CurrentDirectory + "\\bin\\vlc.exe",
                    HelpInfo = DefaultLanguage.Strings.GetString("Encoder_Path_Help"),
                });
                Console.SetIfNotExsistValue(V_camera_save_path, new ConsoleVarable()
                {
                    Value = Environment.CurrentDirectory + "\\sv\\cam{0}\\",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_Save_Path_Help"),
                });
            }
            else
            {
                Console.SetIfNotExsistValue(V_encoder_path, new ConsoleVarable()
                {
                    Value = "/usr/bin/vlc",
                    HelpInfo = DefaultLanguage.Strings.GetString("Encoder_Path_Help"),
                });
                Console.SetIfNotExsistValue(V_camera_save_path, new ConsoleVarable()
                {
                    Value = "/media/sv/cam{0}/",
                    HelpInfo = DefaultLanguage.Strings.GetString("Camera_Save_Path_Help"),
                });
            }

            Console.SetIfNotExsistValue(V_encoder_stream_args, new ConsoleVarable()
            {
                Value = "-I dummy {0} --sout #transcode{{vcodec=theo,acodec=none,hurry-up}}:http{{dst={1}}}",
                HelpInfo = DefaultLanguage.Strings.GetString("Encoder_Stream_ARGS_Help"),
            });

  
            SetupCamera(0);

        }

    }
}