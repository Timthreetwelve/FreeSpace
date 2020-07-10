// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using Newtonsoft.Json;
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

        public double WindowTop
        {
            get { return windowTop; }
            set
            {
                windowTop = value;
                RaisePropertyChanged("WindowTop");
            }
        }

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

        public string Precision
        {
            get { return precision; }
            set
            {
                precision = value;
                RaisePropertyChanged("Precision");
            }
        }

        public char TimeStamp
        {
            get { return timeStamp; }
            set
            {
                timeStamp = value;
                RaisePropertyChanged("TimeStamp");
            }
        }

        public bool Brackets
        {
            get { return brackets; }
            set
            {
                brackets = value;
                RaisePropertyChanged("Brackets");
            }
        }

        public bool DtFixed
        {
            get { return dtfixed; }
            set
            {
                dtfixed = value;
                RaisePropertyChanged("DtFixed");
            }
        }

        public bool DtRemovable
        {
            get { return dtRemovable; }
            set
            {
                dtRemovable = value;
                RaisePropertyChanged("DtRemovable");
            }
        }

        public bool DtNetwork
        {
            get { return dtNetwork; }
            set
            {
                dtNetwork = value;
                RaisePropertyChanged("DtNetwork");
            }
        }

        public bool DtCDRom
        {
            get { return dtCDRom; }
            set
            {
                dtCDRom = value;
                RaisePropertyChanged("DtCDRom");
            }
        }

        public bool DtRamDisk
        {
            get { return dtRamDisk; }
            set
            {
                dtRamDisk = value;
                RaisePropertyChanged("DtRamDisk");
            }
        }

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
            string rawJSON = File.ReadAllText(filename);
            return JsonConvert.DeserializeObject<MySettings>(rawJSON);
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
                Debug.WriteLine($"{filename} not found");
                return;
            }
            else
            {
                Debug.WriteLine($"Saving settings to {filename}");
            }
            try
            {
                string jsonOut = JsonConvert.SerializeObject(s, Formatting.Indented);
                File.WriteAllText(filename, jsonOut);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Saving {filename} failed\n{ex}");
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
        #endregion
    }
}
