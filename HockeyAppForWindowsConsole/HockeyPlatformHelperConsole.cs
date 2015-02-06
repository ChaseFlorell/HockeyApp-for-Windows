﻿﻿using HockeyApp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyAppForWindows.Hoch
{

#pragma warning disable 1998
    public class HockeyPlatformHelperConsole : IHockeyPlatformHelper
    {

        private const string FILE_PREFIX = "HA__SETTING_";
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

        private string PostfixWithAppIdHash(string folderName, bool noDirectorySeparator = false)
        {
            return ((folderName ?? "") + (noDirectorySeparator ? "" : "" + Path.DirectorySeparatorChar) + HockeyClientConsoleExtensions.AppIdHash);
        }

        public void SetSettingValue(string key, string value)
        {
            using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(FILE_PREFIX + key, true), FileMode.Create, FileAccess.Write))
            {
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(value);
                }
            }
        }

        public string GetSettingValue(string key)
        {
            if (isoStore.FileExists(FILE_PREFIX + key))
            {
                using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(FILE_PREFIX + key, true), FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(fileStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return null;
        }

        public void RemoveSettingValue(string key)
        {
            if (isoStore.FileExists(PostfixWithAppIdHash(FILE_PREFIX + key, true)))
            {
                isoStore.DeleteFile(PostfixWithAppIdHash(FILE_PREFIX + key, true));
            }
        }


        #region File access

        public async Task<bool> DeleteFileAsync(string fileName, string folderName = null)
        {
            if (isoStore.FileExists(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName))
            {
                isoStore.DeleteFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName);
                return true;
            }
            return false;
        }

        public async Task<bool> FileExistsAsync(string fileName, string folderName = null)
        {
            return isoStore.FileExists(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName);
        }

        public async Task<Stream> GetStreamAsync(string fileName, string folderName = null)
        {
            return isoStore.OpenFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Open, FileAccess.Read);
        }

        public async Task WriteStreamToFileAsync(Stream dataStream, string fileName, string folderName = null)
        {
            // Ensure crashes folder exists
            if (!isoStore.DirectoryExists(PostfixWithAppIdHash(folderName)))
            {
                isoStore.CreateDirectory(PostfixWithAppIdHash(folderName));
            }

            using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write))
            {
                await dataStream.CopyToAsync(fileStream);
            }
        }

        public async Task<IEnumerable<string>> GetFileNamesAsync(string folderName = null, string fileNamePattern = null)
        {
            try
            {
                return isoStore.GetFileNames(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileNamePattern ?? "*");
            }
            catch (DirectoryNotFoundException)
            {
                return new string[0];
            }
        }

        public bool PlatformSupportsSyncWrite
        {
            get { return true; }
        }

        public void WriteStreamToFileSync(Stream dataStream, string fileName, string folderName = null)
        {
            // Ensure crashes folder exists
            if (!isoStore.DirectoryExists(PostfixWithAppIdHash(folderName)))
            {
                isoStore.CreateDirectory(PostfixWithAppIdHash(folderName));
            }

            using (var fileStream = isoStore.OpenFile(PostfixWithAppIdHash(folderName) + Path.DirectorySeparatorChar + fileName, FileMode.Create, FileAccess.Write))
            {
                dataStream.CopyTo(fileStream);
            }
        }

        #endregion


        string _appPackageName = null;
        public string AppPackageName
        {
            get
            {
                if (_appPackageName == null)
                {
                    _appPackageName = "HockeyApp";
                }
                return _appPackageName;
            }
            set
            {
                _appPackageName = value;
            }
        }

        string _appVersion = null;
        public string AppVersion
        {
            get
            {
                if (_appVersion == null)
                {
                    //ClickOnce
                    try
                    {
                        var type = Type.GetType("System.Deployment.Application.ApplicationDeployment");
                        object deployment = type.GetMethod("CurrentDeployment").Invoke(null, null);
                        Version version = type.GetMethod("CurrentVersion").Invoke(deployment, null) as Version;
                        _appVersion = version.ToString();
                    }
                    catch (Exception)
                    {
                        //Entry Assembly
                        _appVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                    }
                }
                return _appVersion ?? "0.0.0.1";
            }
            set
            {
                _appVersion = value;
            }
        }

        public string OSVersion
        {
            get
            {
                //as windows 8.1 lies to us to be 8 we try via registry
                try
                {
                    using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion"))
                    {
                        return (string)registryKey.GetValue("CurrentVersion") + "." + (string)registryKey.GetValue("CurrentBuild") + ".0";
                    }
                }
                catch (Exception e)
                {
                    HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                }
                return Environment.OSVersion.Version.ToString() + " " + Environment.OSVersion.ServicePack;
            }
        }

        public string OSPlatform
        {
            get { return "Windows"; }
        }

        public string SDKVersion
        {
            get { return Constants.SDKVERSION; }
        }

        public string SDKName
        {
            get
            { return Constants.SDKNAME; }
        }

        public string UserAgentString
        {
            get { return Constants.USER_AGENT_STRING; }
        }

        private string _productID = null;
        public string ProductID
        {
            get { return _productID; }
            set { _productID = value; }
        }

        public string Manufacturer
        {
            get
            {
                return null;
            }
        }

        public string Model
        {
            get
            {
                return null;
            }
        }

    }
#pragma warning restore 1998
}