using CashCam.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Console = CashLib.Console;

using System.Collections.Specialized;
using CashLib;

namespace CashCam.HTTP
{
    class StreamControl
    {
        public static string GetPage(HttpListenerContext context)
        {
            string ACTION = "";
            int id;
            if (context.Request.QueryString.Count != 0)
            {
                foreach(string key in context.Request.QueryString.AllKeys)
                {
                    switch (key)
                    {
                        case "stops":
                            if(int.TryParse(context.Request.QueryString[key], out id))
                            {
                                if (Console.GetOnOff(string.Format(Variables.V_camera_stream_enabled, id)).Value)
                                {
                                    ConsoleResponse cr = Console.SetValue(string.Format(Variables.V_camera_stream_enabled, id), "false");
                                    if(cr.State == ConsoleCommandState.Sucess)
                                        ACTION += "Sucess! Camera " + id + " disabled.<br/>";
                                    else
                                        ACTION += "Failure while disabling camera " + id + ": " + cr.Value + "<br/>";

                                }
                            }

                            break;
                        case "starts":
                            if (int.TryParse(context.Request.QueryString[key], out id))
                            {
                                if (!Console.GetOnOff(string.Format(Variables.V_camera_stream_enabled, id)).Value)
                                {
                                    ConsoleResponse cr = Console.SetValue(string.Format(Variables.V_camera_stream_enabled, id), "true");
                                    if (cr.State == ConsoleCommandState.Sucess)
                                        ACTION += "Sucess! Camera " + id + " enabled.<br/>";
                                    else
                                        ACTION += "Failure while enabling camera " + id + ": " + cr.Value + "<br/>";

                                }
                            }
                            break;
                    }
                }
            }
                
            string camerafor = "";

            // Count is protected from having non numurical values so if this fails look elsewhere
            int count = int.Parse(Console.GetValue(Variables.V_camera_count).Value);
            for(int i=0; i < count; i++)
            {
                camerafor += TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\control.camerafor.template", new Dictionary<string, Func<string>>()
                {
                    {"URI" , ()=>{return context.Request.Url.LocalPath; }},
                    {"STATUS" , ()=>{return Console.GetValue(string.Format(Variables.V_camera_stream_enabled, i)).Value; }},
                    {"ID" , ()=>{return i.ToString(); }},
                });
            }


            return TemplateProcessor.Process(Environment.CurrentDirectory + "\\HTTP\\templates\\control.template", new Dictionary<string, Func<string>>()
                {
                    {"URI" , ()=>{return context.Request.Url.LocalPath; }},
                    {"ACTION" , ()=>{return ACTION; }},
                    {"control.camerafor.template" , ()=>{return camerafor; }},
                });
        }
    }
}
