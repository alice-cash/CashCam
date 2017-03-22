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
using System.Collections.Generic;
using System.Text;
using System.IO;
using CashLib.Threading;

namespace CashCam.Files
{
    /// <summary>
    /// Track and perform required tasks regarding Disk space usage.
    /// </summary>
    class DiskManager: Invoker
    {
        //private Percentage DiskUsage { get; set; }
        public Percentage UsageLimit { get; set; }

        private DriveInfo _workingDrive;
        private DirectoryInfo _workingDirectory;

        public DiskManager(string saveDirectory) : this(new DirectoryInfo(saveDirectory)) { }

        public DiskManager(DirectoryInfo saveDirectory): base("DiskManager: " + saveDirectory) { 
            this._workingDirectory = saveDirectory;
            this._workingDrive = GetDrive(_workingDirectory);
        }

        /// <summary>
        /// Get the best drive/mounted drvice for a given path. This detects
        /// Unux based folder mounts but will fail to detect a Windows volume mounted as a folder.
        /// </summary>
        /// <param name="path">A string containing a valid path.</param>
        /// <returns>A System.IO.DriveInfo object.</returns>
        private DriveInfo GetDrive(DirectoryInfo path)
        {
            /*
             * Some clarification on this function. For a Windows only tool this would be simple.
             * On Unux based systems however folders are all mounted off of /
             * Due to this we need to check every drive's root path. 
             * For example our system has 3 drives /, /foo/ and /hello/ we want to match /foo/bar then 
             * we want to match on /foo and not / or /hello/world.
             * */

            DriveInfo[] drives = DriveInfo.GetDrives();
            int maxDepth = int.MaxValue, depthCounter = 0;
            DriveInfo matchedDrive = null ;
            DirectoryInfo tempDirectory;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    // We do some basic checks, such as if we were given a root directory.
                    if (path.FullName == drive.RootDirectory.FullName) return drive;
                    tempDirectory = path;
                    depthCounter = int.MaxValue;
                    
                    //We loop back until we either match the drive root directory or the folder root directory.
                    //If we detect a folder is a root directory we skip to the next folder.
                    do
                    {
                        tempDirectory = tempDirectory.Parent;
                        if (tempDirectory.FullName == drive.RootDirectory.FullName)
                        {
                            //If max dept is smaller then we don't want to use it
                            //e.g. / == 2 depth, /foo/ == 1 depth
                            if (maxDepth > depthCounter)
                            {
                                maxDepth = depthCounter;
                                matchedDrive = drive;
                            }
                            break;
                        }
                        depthCounter++;
                    } while (tempDirectory.FullName != tempDirectory.Root.FullName) ;

                }
            }
            if (matchedDrive != null) Console.WriteLine(matchedDrive.RootDirectory);
            return matchedDrive;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>This is threadsafe and may be asynchronous.</remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void DiskSpaceCheck(Action<object,bool> callback, object CallbackData)
        {
            InvokeMethod(() =>
            {
                Percentage DiskUsage = new Percentage(divisor: _workingDrive.TotalSize, dividend: _workingDrive.TotalFreeSpace);

                callback(CallbackData, DiskUsage.PercentageDouble > UsageLimit.PercentageDouble);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="CallbackData"></param>
        /// <remarks>This is threadsafe and may be asynchronous.</remarks>
        [ThreadSafe(ThreadSafeFlags.ThreadSafeAsynchronous)]
        public void GetDiskSpace(Action<object, Percentage> callback, object CallbackData)
        {
            InvokeMethod(() =>
            {
                Percentage DiskUsage = new Percentage(divisor: _workingDrive.TotalSize, dividend: _workingDrive.TotalFreeSpace);
                callback(CallbackData, DiskUsage);
            });
        }
    }
}
