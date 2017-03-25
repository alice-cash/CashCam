using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashLib.Threading;

namespace CashCam.Stream
{
    class CameraController : IThreadTask
    {
        public IPCamTask Camera1;
        DateTime NextRun;

        public CameraController()
        {
            Camera1 = new IPCamTask(0);
            NextRun = DateTime.Now;

            Program.ProgramEnding += KillCameras;

        }

        public void RunTask()
        {
            if(NextRun <= DateTime.Now)
            {
                Camera1.CheckTask();
                NextRun = DateTime.Now.AddSeconds(5);
            }
        }

        private void KillCameras()
        {
            Camera1.Terminate();
        }
    }
}
