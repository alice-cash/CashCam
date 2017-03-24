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

        public void Load()
        {
            Console.SetValue("FFMPEG_PATH", new ConsoleVarable()
            {
                Value = "/usr/bin/ffmpeg",
                HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_PATH_Help"),
            });
            Console.SetValue("FFMPEG_SAVE_ARGS", new ConsoleVarable()
            {
                Value = "-i {0} -an -c copy -map 0 -f segment -segment_time 1800 -segment_atclocktime 1 -segment_format mp4 -strftime 1 {1}",
                HelpInfo = DefaultLanguage.Strings.GetString("FFMPEG_SAVE_ARGS_Help"),
            });
        }
    }
}
