using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CashCam.HTTP
{
    class WebClient
    {
        HttpListenerContext context;
        bool isDone;
        bool chunkEnabled;

        public WebClient(HttpListenerContext context)
        {
            this.context = context;
            isDone = false;
            chunkEnabled = false;
        }


        public bool Poll()
        {
            if (isDone) return true;

            if (context.Request.Url.LocalPath == "/")
            {
                ServeRoot();
            }
            else if (context.Request.Url.LocalPath == "/stream")
            {
                ServeStream();
            }
            else if (context.Request.Url.LocalPath == "/control")
            {
                ServeControl();
            }
            else
            {
                Serve404();
            }

            return isDone = true;
        }

        private void ServeRoot()
        {


            //byte[] buffer = Encoding.UTF8.GetBytes("<html><head><title>CashCam Camera</title></head><body><a href='/stream'>/stream</a></body></html>");
            byte[] buffer = Encoding.UTF8.GetBytes(TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\index.template", new Dictionary<string, Func<string>>()
                {
                    {"URI" , ()=>{return context.Request.Url.LocalPath; }}
                }));

            try
            {
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private void ServeControl()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(StreamControl.GetPage(context));
            try
            {
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private void ServeStream()
        {
            try
            {
                if (!chunkEnabled)
                {
                    chunkEnabled = true;

                    //Camera 0

                    if (Program.CameraManager.GetGamera(0) != null && Program.CameraManager.GetGamera(0).StreamEnabled())
                    {
                        context.Response.ContentType = "application/ogg";
                        context.Response.SendChunked = true;
                        context.Response.KeepAlive = true;
                        Program.CameraManager.GetGamera(0).StreamTask.Repeater.AddStream(context);
                    }
                    else
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\noStrean.template", new Dictionary<string, Func<string>>()
                        {
                            {"URI" , ()=>{return context.Request.Url.LocalPath; }},
                            {"CameraID" , ()=>{return "0"; }}
                        }));

                        context.Response.StatusCode = 404;
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();

                    }

                }
            }
            finally
            {
            }
        }

        private void Serve404()
        {
            //byte[] buffer = Encoding.UTF8.GetBytes("<html><head><title>CashCam Camera</title></head><body>404 file not found</body></html>");
            byte[] buffer = Encoding.UTF8.GetBytes(TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\404.template", new Dictionary<string, Func<string>>()
                {
                    {"URI" , ()=>{return context.Request.Url.LocalPath; }}
                }));
            try
            {
                context.Response.StatusCode = 404;
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }
    }
}
