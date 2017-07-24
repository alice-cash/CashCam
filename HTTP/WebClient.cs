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

        public HttpListenerContext Context;
        bool isDone;
        bool chunkEnabled;

        public WebClient(HttpListenerContext context)
        {
            this.Context = context;
            isDone = false;
            chunkEnabled = false;
        }


        public bool Poll()
        {
            if (isDone) return true;

            if (Context.Request.Url.LocalPath == "/")
            {
                ServeRoot();
            }
            else if (Context.Request.Url.LocalPath == "/stream")
            {
                ServeStream();
            }
            else if (Context.Request.Url.LocalPath == "/control")
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
            byte[] buffer = Encoding.UTF8.GetBytes(TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\index.template", new Dictionary<string, Func<string>>()
                {
                    {"URI" , ()=>{return Context.Request.Url.LocalPath; }}
                }));

    
            Context.Response.ContentLength64 = buffer.Length;
            WriteData(buffer, buffer.Length);
            Context.Response.OutputStream.Close();
            
        }

        private void ServeControl()
        {
            byte[] buffer = Encoding.UTF8.GetBytes(StreamControl.GetPage(Context));
            Context.Response.ContentLength64 = buffer.Length;
            WriteData(buffer, buffer.Length);
            Context.Response.OutputStream.Close();
          
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
                        Context.Response.ContentType = "application/ogg";
                        Context.Response.SendChunked = true;
                        Context.Response.KeepAlive = true;
                        Program.CameraManager.GetGamera(0)
                            .StreamTask.Repeater.AddStream(
                            new OGGStream.StreamClient(Context.Response.OutputStream)
                           );
                    }
                    else
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\noStrean.template", new Dictionary<string, Func<string>>()
                        {
                            {"URI" , ()=>{return Context.Request.Url.LocalPath; }},
                            {"CameraID" , ()=>{return "0"; }}
                        }));

                        Context.Response.StatusCode = 404;
                        Context.Response.ContentLength64 = buffer.Length;
                        WriteData(buffer, buffer.Length);
                        Context.Response.OutputStream.Close();

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
                    {"URI" , ()=>{return Context.Request.Url.LocalPath; }}
                }));
   
            Context.Response.StatusCode = 404;
            Context.Response.ContentLength64 = buffer.Length;
            WriteData(buffer, buffer.Length);
            Context.Response.OutputStream.Close();
            
        }

        public bool WriteData(byte[] data, int length)
        {
            try
            {
                Context.Response.OutputStream.Write(data, 0, length);
            }
            catch { return false; }
            return true;
        }
    }
}
