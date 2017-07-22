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
    class CameraManager : IThreadTask
    {
        public List<Camera> Cameras;
        DateTime NextRun;
        int knownCount = 0;

        public CameraManager()
        {

        }

        public Camera GetGamera(int id) 
        {
            foreach (Camera cam in Cameras)
            {
                if (cam.ID == id) return cam;
            }
            return null;
        }

        public void Start()
        {

            Cameras = new List<Camera>();

            // Count is protected from having non numurical values so if this fails look elsewhere
            int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
            knownCount = count;

            for (int id = 0; id < count; id++)
            {

                Camera camera = new Camera(id);
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
            bool LongRun = NextRun <= DateTime.Now;
            if (LongRun)
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
                            Camera camera = new Camera(id);
                            Cameras.Add(camera);
                        }
                    }
                    else
                    {
                        Console.WriteLine("{0} cameras removed!", knownCount - count);
                        List<Camera> toRemove = new List<Camera>();
                        foreach (Camera cam in Cameras)
                            if (cam.ID >= count)
                                toRemove.Add(cam);
                        foreach (Camera cam in toRemove)
                            Cameras.Remove(cam);
                    }
                    knownCount = count;
                }

                NextRun = DateTime.Now.AddSeconds(5);
            }

            foreach (Camera cam in Cameras)
            {
                cam.CheckTask(LongRun);
            }
        }

        private void KillCameras()
        {
            foreach (Camera cam in Cameras)
                cam.Terminate();
        }
    }
}
