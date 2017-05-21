using CashLib.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CashCam.HTTP
{
    class WebServer : IThreadTask
    {
        private HttpListener _listener = new HttpListener();

        public WebServer()
        {

        }

        public void RunTask()
        {
            if (!_listener.IsListening) return;

            var asynccontext = _listener.BeginGetContext((IAsyncResult result) =>
            {
                HttpListenerContext context;
                try
                {
                    context = _listener.EndGetContext(result);
                }
                catch { return; }

                byte[] buffer = Encoding.UTF8.GetBytes("<html><head><title> CashCam Camera</ title></ head><body></ body></ html>");
                try
                {
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                finally
                {
                    context.Response.OutputStream.Close();
                }
            }, _listener);

            while (asynccontext.AsyncWaitHandle.WaitOne(100) && !asynccontext.IsCompleted)
            {
                if (!Program.ThreadsRunning)
                    return;
                System.Threading.Thread.Yield();
            }

        }



        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:8080/");
            _listener.Start();
        }

        public void Stop()
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }
    }
}
