/*
    Copyright (c) Matthew Cash 2017, 
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using CashCam.Module;
using CashCam.Modules;
using CashLib.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = CashLib.Console;

namespace CashCam.Files
{
    [ThreadSafe(ThreadSafeFlags.ThreadUnsafe)]
    class DiskController : IThreadTask
    {
        DateTime NextRun;
        public DiskManager Manager { get; private set; }


        public DiskController()
        {

        }

        public void Start()
        {
            Manager = new DiskManager(Console.GetValue(Variables.V_camera_save_path).Value)
            { UsageLimit = new Percentage(50, 100) };
            Program.DiskManager = Manager;
        }

        public void Stop(bool force)
        {

        }


        public void RunTask()
        {
            Manager.PollInvokes();
            if (NextRun <= DateTime.Now)
            {

                Manager.DiskSpaceCheck((object unused, bool isOver) =>
                {
                    if (isOver)
                    {
                        DoDiskCleanup();
                    }
                }, null);
                NextRun = DateTime.Now.AddSeconds(60);
            }
        }

        private void DoDiskCleanup()
        {
            //Count is protected from having non numurical values so if this fails look elsewhere
            int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
            List<FileInfo> list = new List<FileInfo>();

            for (int id = 0; id < count; id++)
            {
                list.AddRange(new DirectoryInfo(string.Format(Console.GetValue(Variables.V_camera_save_path).Value, id)).GetFiles());
            }

            FileInfo oldest = list.OrderBy(f => f.CreationTime).First();

            Debugging.DebugLog(Debugging.DebugLevel.Message, "Removing file due to disk limit reached: {0}", oldest.Name);

            //oldest.Delete();
        }
    }
}
