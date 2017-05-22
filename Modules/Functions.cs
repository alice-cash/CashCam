using System;
using System.Collections.Generic;
using CashLib.Module;
using CashLib;
using Console = CashLib.Console;
using CashLib.Localization;

namespace CashCam.Modules
{
    class Functions : IModuleLoader
    {
        public Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public string Name
        {
            get { return "Variables"; }
        }


        public void Load()
        {
            Console.SetFunc("Disk", new ConsoleFunction()
            {
                Function = Disk,
                HelpInfo = DefaultLanguage.Strings.GetString("disk_Help"),
                TabFunction = Disk_GetTabCompletionValues
            });
        }

        private ConsoleResponse Disk(string[] arguments)
        {
            if (Program.DiskManager == null)
                return ConsoleResponse.NewFailure("Program.DiskManager is not setup yet!");

            if (arguments.Length == 0)
                return ConsoleResponse.NewSucess(DefaultLanguage.Strings.GetString("disk_Help"));

            switch (arguments[0])
            {
                case "Free":
                    return ConsoleResponse.NewSucess("Disk Free: " + Program.DiskManager.SynchronousGetDiskAvaliable().ToString());
                case "Limit":
                    return ConsoleResponse.NewSucess("Disk Limit: " + Program.DiskManager.UsageLimit.ToString());

            }

            return ConsoleResponse.NewSucess(DefaultLanguage.Strings.GetString("disk_Help"));

        }

        private TabData Disk_GetTabCompletionValues(string line)
        {
            string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
           // string subLine = "";

            if (split.Length == 1)
                return new TabData() { Line = line, Result = true, TabStrings = new string[] { "Free", "Limit" } };

            if (split.Length == 2)
                if ("Free".StartsWith(split[1]))
                    return new TabData() { Line = line, Result = true, TabStrings = new string[] { "Free" } };
                if ("Limit".StartsWith(split[1]))
                    return new TabData() { Line = line, Result = true, TabStrings = new string[] { "Limit" } };


            return TabData.Failue();
        }

        private void Save()
        {
        }
    }
}
