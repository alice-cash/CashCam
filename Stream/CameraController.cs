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
        public List<IPCamTask> Cameras;
        DateTime NextRun;
        int knownCount = 0;

        public CameraController()
        {

        }

        public void Start()
        {

            Cameras = new List<IPCamTask>();

            //Count is protected from having non numurical values so if this fails look elsewhere
            int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
            knownCount = count;

            for (int id = 0; id < count; id++)
            {
                IPCamTask camera = new IPCamTask(id);
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
                //Count is protected from having non numurical values so if this fails look elsewhere
                int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
                if (count != knownCount)
                {
                    if (count > knownCount)
                    {
                        Console.WriteLine("{0} new cameras detected!", count - knownCount);
                        for (int id = knownCount; id < count; id++)
                        {
                            IPCamTask camera = new IPCamTask(id);
                            Cameras.Add(camera);
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} cameras removed!", knownCount - count);
                        List<IPCamTask> toRemove = new List<IPCamTask>();
                        foreach (IPCamTask cam in Cameras)
                            if (cam.ID >= count)
                                toRemove.Add(cam);
                        foreach (IPCamTask cam in toRemove)
                            Cameras.Remove(cam);
                    }
                    knownCount = count;
                }

                foreach (IPCamTask cam in Cameras)
                {
                    cam.CheckTask();
                }
                NextRun = DateTime.Now.AddSeconds(5);
            }

        }

        private void KillCameras()
        {
            foreach (IPCamTask cam in Cameras)
                cam.Terminate();
        }
    }
}
