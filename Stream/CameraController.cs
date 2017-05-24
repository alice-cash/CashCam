using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashLib.Threading;
using Console = CashLib.Console;

using CashLib;
using CashCam.Modules;
using CashLib.Exceptions;

namespace CashCam.Stream
{
    [ThreadSafe(ThreadSafeFlags.ThreadUnsafe)]
    class CameraController : IThreadTask
    {
        public List<IPCamSaveTask> Cameras;
        DateTime NextRun;
        int knownCount = 0;

        public CameraController()
        {

        }

        public void Start()
        {

            Cameras = new List<IPCamSaveTask>();

            // Count is protected from having non numurical values so if this fails look elsewhere
            int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
            knownCount = count;

            for (int id = 0; id < count; id++)
            {
                IPCamSaveTask camera = new IPCamSaveTask(id);
                Cameras.Add(camera);
            }

            NextRun = DateTime.Now;

            Program.ProgramEnding += KillCameras;
        }

        public void Stop()
        {
            KillCameras();
        }

        public void RunTask()
        {
            if (NextRun <= DateTime.Now)
            {
                // Count is protected from having non numurical values so if this fails look elsewhere
                int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
                if (count != knownCount)
                {
                    if (count > knownCount)
                    {
                        Console.WriteLine("{0} new cameras detected!", count - knownCount);
                        for (int id = knownCount; id < count; id++)
                        {
                            IPCamSaveTask camera = new IPCamSaveTask(id);
                            Cameras.Add(camera);
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} cameras removed!", knownCount - count);
                        List<IPCamSaveTask> toRemove = new List<IPCamSaveTask>();
                        foreach (IPCamSaveTask cam in Cameras)
                            if (cam.ID >= count)
                                toRemove.Add(cam);
                        foreach (IPCamSaveTask cam in toRemove)
                            Cameras.Remove(cam);
                    }
                    knownCount = count;
                }

                foreach (IPCamSaveTask cam in Cameras)
                {
                    cam.CheckTask();
                }
                NextRun = DateTime.Now.AddSeconds(5);
            }

        }

        private void KillCameras()
        {
            foreach (IPCamSaveTask cam in Cameras)
                cam.Terminate();
        }
    }
}
