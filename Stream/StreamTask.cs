using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashLib.Threading;

namespace CashCam.Stream
{
    class StreamTask : IThreadTask
    {
        public IPCamTask Camera1;

        public StreamTask()
        {
            Camera1 = new IPCamTask();
        }

        public void RunTask()
        {
            
        }
    }
}
