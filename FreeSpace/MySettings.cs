// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using Newtonsoft.Json;
using TKUtils;
using System.Windows;
#endregion

namespace FreeSpace
{
    public class MySettings : INotifyPropertyChanged
    {
        public MySettings()
        {

        }
        /////////////////////////////  Properties  //////////////////////////////

        #region Backing fields
        private string logFile;
        private string precision;
        private char timeStamp;
        private bool brackets;
        private double windowTop;
        private double windowLeft;
        private bool dtfixed;
        private bool dtRemovable;
        private bool dtNetwork;
        private bool dtCDRom;
        private bool dtRamDisk;
        private bool dtUnknown;
        #endregion

        [DefaultValue(100)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public double WindowTop
        {
            get { return windowTop; }
            set
            {
                windowTop = value;
                RaisePropertyChanged("WindowTop");
            }
        }

        [DefaultValue(100)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public double WindowLeft
        {
            get { return windowLeft; }
            set
            {
                windowLeft = value;
                RaisePropertyChanged("WindowLeft");
            }
        }

        public string LogFile
        {
            get { return logFile; }
            set
            {
                logFile = value;
                RaisePropertyChanged("LogFile");
            }
        }

        [DefaultValue("1")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                RaisePropertyChanged("Precision");
            }
        }

        [DefaultValue('S')]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public char TimeStamp
        {
            get { return timeStamp; }
            set
            {
                timeStamp = value;
                RaisePropertyChanged("TimeStamp");
            }
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Brackets
        {
            get { return brackets; }
            set
            {
                brackets = value;
                RaisePropertyChanged("Brackets");
            }
        }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DtFixed
        {
            get { return dtfixed; }
            set
            {
                dtfixed = value;
                RaisePropertyChanged("DtFixed");
            }
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DtRemovable
        {
            get { return dtRemovable; }
            set
            {
                dtRemovable = value;
                RaisePropertyChanged("DtRemovable");
            }
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DtNetwork
        {
            get { return dtNetwork; }
            set
            {
                dtNetwork = value;
                RaisePropertyChanged("DtNetwork");
            }
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DtCDRom
        {
            get { return dtCDRom; }
            set
            {
                dtCDRom = value;
                RaisePropertyChanged("DtCDRom");
            }
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DtRamDisk
        {
            get { return dtRamDisk; }
            set
            {
                dtRamDisk = value;
                RaisePropertyChanged("DtRamDisk");
            }
        }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DtUnknown
        {
            get { return dtUnknown; }
            set
            {
                dtUnknown = value;
                RaisePropertyChanged("DtUnknown");
            }
        }

        //////////////////////////  Property Changed  /////////////////////////

        #region Property changed
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
                Debug.WriteLine($"+++ Property Changed: {name}");
            }
        }
        #endregion

        /////////////////////////////  Methods  //////////////////////////////

        #region Read settings
        /// <summary>
        /// Reads settings from the specified JSON file
        /// </summary>
        /// <param name="filename">If filename parameter is empty, the default settings.json will be used</param>
        /// <returns>MySettings object</returns>
        public static MySettings Read(string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = DefaultSettingsFile();
            }
            if (!File.Exists(filename))
            {
                WriteLog.WriteTempFile($"Settings file not found - Creating");
                CreateEmptySettingsFile(filename);
            }
            try
            {
                string rawJSON = File.ReadAllText(filename);
                return JsonConvert.DeserializeObject<MySettings>(rawJSON);
            }
            catch (Exception ex)
            {
                WriteLog.WriteTempFile($"Settings file could not be read - cannot continue");
                WriteLog.WriteTempFile($"Error reading {filename}");
                WriteLog.WriteTempFile($"{ex}");
                Application.Current.Shutdown();
                return null;
            }
        }
        #endregion

        #region Save settings
        /// <summary>
        /// Saves settings to the specified JSON file
        /// </summary>
        /// <param name="s">name of object containing settings</param>
        /// <param name="filename">If filename parameter is empty, the default settings.json will be used</param>
        public static void Save(MySettings s, string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = DefaultSettingsFile();
            }
            if (!File.Exists(filename))
            {
                WriteLog.WriteTempFile($"Settings file not found - cannot save settings");
                WriteLog.WriteTempFile($"{filename} - not found");
                return;
            }
            else
            {
                WriteLog.WriteTempFile($"Saving settings to {filename}");
            }
            try
            {
                string jsonOut = JsonConvert.SerializeObject(s, Formatting.Indented);
                File.WriteAllText(filename, jsonOut);
            }
            catch (Exception ex)
            {
                WriteLog.WriteTempFile($"Settings could not be saved");
                WriteLog.WriteTempFile($"{ex}");
            }
        }
        #endregion

        #region List settings
        /// <summary>
        /// Lists current settings
        /// </summary>
        /// <param name="s">name of object containing settings</param>
        /// <returns>List of strings </returns>
        public static List<string> List(MySettings s)
        {
            List<string> list = new List<string>();

            foreach (PropertyInfo prop in s.GetType().GetProperties())
            {
                Debug.WriteLine($"{prop.Name} : {prop.GetValue(s) }");
                list.Add($"{prop.Name} = {prop.GetValue(s)}");
            }
            return list;
        }
        #endregion

        #region Helper Methods
        private static string DefaultSettingsFile()
        {
            string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            return Path.Combine(dir, "settings.json");
        }

        private static void CreateEmptySettingsFile(string filename)
        {
            try
            {
                File.WriteAllText(filename, "{ }");
                WriteLog.WriteTempFile($"{filename} - Created");
            }
            catch (Exception ex)
            {
                WriteLog.WriteTempFile($"Settings file could not be created - cannot continue");
                WriteLog.WriteTempFile($"{ex}");
                Environment.Exit(1);
            }
        }
        #endregion
    }
}
