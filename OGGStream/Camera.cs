using CashCam.Modules;
using Console = CashLib.Console;

namespace CashCam.OGGStream
{
    class Camera
    {
        public SaveTask SaveTask;
        public StreamTask StreamTask;

        public Camera(int id)
        {
            this.ID = id;
            SaveTask = new SaveTask(id);
            StreamTask = new StreamTask(this, id);
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
