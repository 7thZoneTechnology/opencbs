﻿// LICENSE PLACEHOLDER

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.Win32;

namespace OpenCBS.Shared.Settings
{
    [Serializable]
    public static class TechnicalSettings
    {
        private static Version _version;

        public const string RegistryPathTemplate = @"Software\Open Octopus Ltd\OpenCBS\{0}";

        private static bool _useOnlineMode;
        private static bool _useDebugMode;
        private static string _remotingServer;
        private static int _remotingServerPort;

        private static readonly Dictionary<string, string> Settings = new Dictionary<string, string>();

        private static Version GetVersion()
        {
            if (_version != null) return _version;

            var assembly = Assembly.GetEntryAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return (_version = new Version(fileVersionInfo.FileVersion));
        }

        public static string DatabaseName
        {
            get { return GetValue("DATABASE_NAME", "Octopus"); }
            set { SetValue("DATABASE_NAME", value); }
        }

        public static string DatabaseServerName
        {
            get { return GetValue("DATABASE_SERVER_NAME", "localhost"); }
            set { SetValue("DATABASE_SERVER_NAME", value); }
        }

        public static string DatabaseLoginName
        {
            get { return GetValue("DATABASE_LOGIN_NAME", "sa"); }
            set { SetValue("DATABASE_LOGIN_NAME", value); }
        }

        public static string DatabasePassword
        {
            get { return GetValue("DATABASE_PASSWORD", "octopus"); }
            set { SetValue("DATABASE_PASSWORD", value); }
        }

        public static List<string> AvailableDatabases
        {
            get
            {
                var retval = new List<string>();
                string val = GetValue("DATABASE_LIST", string.Empty);
                val = val.Replace(" ", "");
                if (string.IsNullOrEmpty(val)) return retval;

                retval.AddRange(val.Split(','));
                return retval;
            }
        }

        public static void AddAvailableDatabase(string database)
        {
            string val = GetValue("DATABASE_LIST", string.Empty);
            if (string.IsNullOrEmpty(val))
            {
                val = database;
            }
            else
            {
                val += "," + database;
            }
            SetValue("DATABASE_LIST", val);
        }

        public static string CurrentVersion
        {
            get { return GlobalSettings.Default.Version; }
        }

        public static string SoftwareVersion
        {
            get { return "v" + CurrentVersion; }
        }

        public static bool UseOnlineMode
        {
            get { return _useOnlineMode; }
            set { _useOnlineMode = value;}
        }

        public static bool UseDebugMode
        {
            get { return _useDebugMode; }
            set { _useDebugMode = value; }
        }

        public static bool SentQuestionnaire
        {
            get { return Convert.ToBoolean(GetValue("SENT_QUESTIONNAIRE", "True")); }
            set { SetValue("SENT_QUESTIONNAIRE", value ? "True" : "False"); }
        }

        public static string RemotingServer
        {
            get
            {
                return _remotingServer;
            }
            set
            {
                _remotingServer = value;
            }
        }

        public static int RemotingServerPort
        {
            get { return _remotingServerPort; }
            set { _remotingServerPort = value; }
        }

        public static int DatabaseTimeout
        {
            get
            {
                string dbTimeoutStr = GetValue("DATABASE_TIMEOUT", "300");
                int dbTimeout;
                return int.TryParse(dbTimeoutStr, out dbTimeout) ? dbTimeout : 300;
            }
        }

        public static int DebugLevel
        {
            get { return 0; }
        }

        public static string SoftwareVersionWithBuild
        {
            get
            {
                string build = SoftwareBuild;
                if (string.IsNullOrEmpty(build)) return SoftwareVersion;
                return string.Format("{0}.{1}", SoftwareVersion, build);
            }
        }

        public static string SoftwareBuild
        {
            get { return GetRevisionNumberFromFile(); }
        }

        private static string GetRevisionNumberFromFile()
        {
            string revisionNumber;

            try
            {
                TextReader textReader = new StreamReader(Application.StartupPath + "\\" + "BuildLabel.txt");
                revisionNumber = textReader.ReadLine();
                if (string.IsNullOrEmpty(revisionNumber))
                    return string.Empty;
            }
            catch (FileNotFoundException)
            {
                return string.Empty;
            }

            try
            {
                Convert.ToInt32(revisionNumber);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return revisionNumber;
        }

        public static bool CheckSettings()
        {
            var values = new[]
            {
                DatabaseLoginName,
                DatabaseName,
                DatabasePassword,
            };
            return values.All(value => !string.IsNullOrEmpty(value));
        }

        private static string GetRegistryPath()
        {
            var version = GetVersion();
            return string.Format(RegistryPathTemplate, version);
        }

        private static void SetValue(string key, string value)
        {
            Settings[key] = value;

            string path = GetRegistryPath();
            using (RegistryKey reg = Registry.LocalMachine.OpenSubKey(path, true))
            {
                if (null == reg) return;
                reg.SetValue(key, value);
            }
        }

        private static string GetValue(string key, string defaultValue)
        {
            if (Settings.ContainsKey(key))
            {
                return Settings[key];
            }

            string path = GetRegistryPath();
            using (RegistryKey reg = Registry.LocalMachine.OpenSubKey(path, true))
            {
                if (null == reg) return defaultValue;
                string value = reg.GetValue(key, defaultValue).ToString();
                Settings[key] = value;
                return value;
            }
        }
    }
}
