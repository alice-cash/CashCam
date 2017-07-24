using CashCam.Modules;
using Console = CashLib.Console;

namespace CashCam.OGGStream
{
    class Camera
    {
        public IPCamSaveTask SaveTask;
        public IPCamStreamTask StreamTask;

        public Camera(int id)
        {
            this.ID = id;
            SaveTask = new IPCamSaveTask(id);
            StreamTask = new IPCamStreamTask(this, id);
        }

        public bool SaveEnabled()
        {
            return Console.GetOnOff(string.Format(Variables.V_camera_enabled, ID)).Value;
        }

        public bool StreamEnabled()
        {
            return Console.GetOnOff(string.Format(Variables.V_camera_stream_enabled, ID)).Value;
        }

        public int ID { get; internal set; }

        public void CheckTask(bool LongRun)
        {
            SaveTask.CheckTask(LongRun);
            StreamTask.CheckTask(LongRun);
        }

        internal void Terminate()
        {
            SaveTask.Terminate();
            StreamTask.Terminate();
        }
    }
}
