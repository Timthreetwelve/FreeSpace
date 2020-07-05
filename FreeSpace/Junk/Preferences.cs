// Copyright (c) TIm Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

using Newtonsoft.Json.Linq;
using TKUtils;
#endregion

namespace FreeSpace
{


    public class Rootobject
    {
        public Preferences Preferences { get; set; }
    }

    public class Preferences : INotifyPropertyChanged
    {
        private string precision;
        private string timestamp;
        private string d_fixed;

        public string D_Fixed
        {
            get { return d_fixed; }
            set
            {
                if (d_fixed != value)
                {
                    d_fixed = value;
                    Something("D_Fixed", value);
                }
            }
        }

        public string D_Removable { get; set; }
        public string D_Network { get; set; }
        public string D_CDRom { get; set; }
        public string D_Ram { get; set; }
        public string D_Unknown { get; set; }


        public string TimeStamp
        {
            get => timestamp;
            set
            {
                if (timestamp != value)
                {
                    timestamp = value;
                    Something("TimeStamp", value);
                }
            }
        }

        public string Precision
        {
            get => precision;
            set
            {
                if (value != precision)
                {
                    precision = value;
                    Something("Precision" , value);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void Something(string name, string newvalue)
        {
            Debug.WriteLine($"+++ {name} Changed to {newvalue}");

            if (newvalue != null)
            {
                JObject obj = JObject.Parse(GetJsonFile());

                JObject pref = (JObject)obj["Preferences"];

                pref[name] = newvalue;

                string j = obj.ToString();

                WriteJsonFile(j);

                NotifyPropertyChanged();
            }

        }

        private string GetJsonFile()
        {
            Debug.WriteLine("+++ Reading file");
            return File.ReadAllText(Path.Combine(AppInfo.AppDirectory, Properties.Settings.Default.Prefs));
        }

        private void WriteJsonFile(string json)
        {
            File.WriteAllText(Path.Combine(AppInfo.AppDirectory, Properties.Settings.Default.Prefs), json);
            Debug.WriteLine("+++ Writing file");
        }
    }
}