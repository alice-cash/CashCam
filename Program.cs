/*
    Copyright (c) Matthew Cash 2017, 
    All rights reserved.

    Redistribution and use in source and binary forms, with or without
    modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this
      list of conditions and the following disclaimer.

    * Redistributions in binary form must reproduce the above copyright notice,
      this list of conditions and the following disclaimer in the documentation
      and/or other materials provided with the distribution.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
    AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
    FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
    DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
    CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
    OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Diagnostics;

using CashCam.Files;
using CashLib.Module;
using CashLib.Localization;
using System.Reflection;

namespace CashCam
{
    class Program
    {

        private static CashLib.Threading.Thread SchedulerThread;
        private static CashLib.Tasks.Scheduler Scheduler;

        /// <summary>
        /// Determin if threads are running. Any threads should run when this is true.
        /// When set false all threads should cease execution. You should only set when you
        /// wish to kill the appliication.
        /// </summary>
        public static bool ThreadsRunning { get; set; }
        public static Action ThreadsStopped;

        /// <summary>
        /// Determin if the console is visisble.
        /// </summary>
        private static bool _consoleVisible;

        static void Main(string[] args)
        {
            _consoleVisible = true;

            foreach (string arg in args)
            {
                if (string.Compare(arg, "-d", true) == 0 ||
                    string.Compare(arg, "--daemon", true) == 0)
                    _consoleVisible = false;
                if(string.Compare(arg, "-h", true) == 0 ||
                    string.Compare(arg, "--help", true) == 0)
                {
                    PrintHelp();
                    return;
                }
            }

            ThreadsRunning = true;

            _initialize();
            
            while (ThreadsRunning)
            {
                if (_consoleVisible)
                {
                    Console.Write("#>");
                    string line = Console.ReadLine();
                    var responce = CashLib.TConsole.ProcessLine(line);
                    Console.WriteLine(responce.Value);
                } else
                {
                    System.Threading.Thread.Sleep(1000);
                }
                System.Threading.Thread.Yield();
            }
            ThreadsStopped();
        }   

        public static void Stop()
        {
            ThreadsRunning = false;
        }

        static void PrintHelp()
        {
            Console.WriteLine(string.Format("CashCam Version {0}.{1}.{2}.{3}", Program.Version.Major, Program.Version.Minor, Program.Version.Build, Program.Version.Revision));
            Console.WriteLine();
            Console.WriteLine("Usage: {0} [OPTION]", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine(" -h, --help\t\tPrint this help menu");
            Console.WriteLine(" -d, --daemon\t\tRun in daemon mode, no console input or output.");
            Console.WriteLine();
        }


        /// <summary>
        /// Function to initialize the application.
        /// </summary>
        private static void _initialize()
        {

            Debug.Listeners.Clear();
            Trace.Listeners.Clear();

            CashLib.TConsole.Init();

            Debug.Listeners.Add(new ConsoleTraceListener());
            Debug.Listeners.Add(new CashLib.Diagnostics.ConsoleTraceListiner());
            //Trace.Listeners.Add(new ConsoleTraceListener());
            //Trace.Listeners.Add(new TortoiseConsoleTraceListiner());

            Trace.WriteLine(string.Format("CashCam Version {0}.{1}.{2}.{3}", Program.Version.Major, Program.Version.Minor, Program.Version.Build, Program.Version.Revision));

            DefaultLanguage.InitDefault();
            ModuleInfo.LoadModules(Assembly.GetExecutingAssembly(), true);

            Scheduler = new CashLib.Tasks.Scheduler("CashCam Scheduler");
            SchedulerThread = new CashLib.Threading.Thread("SchedulerThread");
            SchedulerThread.AddTask(Scheduler);

            SchedulerThread.Start();
            ThreadsStopped += SchedulerThread.Stop;
        }

        /// <summary>
        /// Return the applicaion's version.
        /// </summary>
        public static Version Version
        {
            get
            {
                return typeof(Program).Assembly.GetName().Version;
            }
        }
    }
}