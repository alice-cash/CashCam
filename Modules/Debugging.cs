/*
 * Copyright 2014 Matthew Cash. All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are
 * permitted provided that the following conditions are met:
 * 
 *    1. Redistributions of source code must retain the above copyright notice, this list of
 *       conditions and the following disclaimer.
 * 
 *    2. Redistributions in binary form must reproduce the above copyright notice, this list
 *       of conditions and the following disclaimer in the documentation and/or other materials
 *       provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY Matthew Cash ``AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 * FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Matthew Cash OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
 * ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * The views and conclusions contained in the software and documentation are those of the
 * authors and should not be interpreted as representing official policies, either expressed
 * or implied, of Matthew Cash.
 */

/*
 * File: Tortoise.Client.Module.Debugging.cs
 * Description : Provides Logging and Debugging information which can be relayed to the remote server. 
 * Dependencies: None
 * Console Variables: 
 *     Debugging_Level
 * Console Functions: None
 */

using System;
using System.Collections.Generic;
using CashLib.Module;
using CashLib;
using Console = CashLib.Console;
using CashLib.Localization;

namespace CashCam.Module
{
    class Debugging : IModuleLoader
    {

       public Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public string Name
        {
            get { return "Debugging"; }
        }

        private string Filename = "Debug.ini";

        public enum DebugLevel
        {
            Info = 0,
            Message = 1,
            Debug1 = 2,
            Debug2 = 3,
            Debug3 = 4,
        }

        public void Load()
        {
            Console.SetValue("Debugging_Level",
                new ConsoleVarable() {
                    ValidCheck = CheckConsoleInput,
                    Value = "0",
                    TabFunction = GetTabCompletionValues,
                    HelpInfo = DefaultLanguage.Strings.GetString("Debugging_Level_Help"),
                });
            Console.ProcessFile(Filename);
            Program.ProgramEnding += Save;

        }

        private void Save()
        {
            //We have 1 camera we are saving.
            List<string> VariablesToSave = new List<string>()
                {
                    "Debugging_Level",
                };

            Console.SaveToFile(Filename, VariablesToSave.ToArray());
        }


        public static void DebugLog(DebugLevel level, string message, params string[] format)
        {
            if (CurrentLevel(Console.GetVariable("Debugging_Level").Value) >= level)
                Console.WriteLine(message, format);
        }

        private static DebugLevel CurrentLevel(string value)
        {
            switch (value)
            {
                case "0":
                    return DebugLevel.Info;
                case "1":
                    return DebugLevel.Message;
                case "2":
                    return DebugLevel.Debug1;
                case "3":
                    return DebugLevel.Debug2;
                case "4":
                    return DebugLevel.Debug3;
            }
            return DebugLevel.Info;
        }

        /// <summary>
        /// Verify the input for Debugging_Level is a valid level.
        /// </summary>
        /// <param name="input">Entered level</param>
        /// <returns>Returned status regarding success or failure of input</returns>
        ExecutionState CheckConsoleInput(string input)
        {
            input = input.Trim();
            switch (input)
            {
                case "0":
                case "1":
                case "2":
                case "3":
                case "4":
                    return ExecutionState.Succeeded();
                default:
                    return ExecutionState.Failed("Input must be 0, 1, or 2");
            }
        }

        public TabData GetTabCompletionValues(string line)
        {
            //If theres any second or third level data we return no suggestion
            if (line.Trim().Contains(" "))
                return new TabData() { Result = false };
            return new TabData() { Result = true, Line = line, TabStrings = new string[] { "0", "1", "2", "3", "4" } };
        }
    }
}
