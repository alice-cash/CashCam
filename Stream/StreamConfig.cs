using System;
using System.Collections.Generic;
using CashLib.Module;
using CashLib;
using Console = CashLib.TConsole;

namespace CashCam.Stream
{
    class  StreamConfig
    {
        public StreamConfig()
        {
          }
        public  string FFMPEG_PATH = "/usr/bin/ffmpeg";
        public  string FFMPEG_SAVE_ARGS = "";
        public  string FFMPEG_STREAM_ARGS = "";
    }
}
