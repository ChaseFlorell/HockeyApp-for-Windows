﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HockeyAppForWindows.Hoch
{
    internal class Constants
    {

        internal static string GetPathToHockeyCrashes()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!path.EndsWith("\\")) { path += "\\"; }
            path += "HockeyApp\\" + HockeyClientConsoleExtensions.AppIdHash + "\\";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path); }
            return path;
        }

        internal const string CrashFilePrefix = "crashinfo_";

        internal const string USER_AGENT_STRING = "Hockey/WinConsoleHoch";
        internal const string SDKNAME = "HockeySDKHochExe";
        internal const string SDKVERSION = "2.2.1";

        internal const string NAME_OF_SYSTEM_SEMAPHORE = "HOCKEYAPPSDK_SEMAPHORE";
    }
}