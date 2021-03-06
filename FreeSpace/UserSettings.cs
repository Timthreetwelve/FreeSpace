﻿// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using TKUtils;

namespace FreeSpace
{
    public class UserSettings : SettingsManager<UserSettings>, INotifyPropertyChanged
    {
        #region Constructor
        public UserSettings()
        {
            // Set defaults
            Brackets = false;
            DtCDRom = false;
            DtFixed = true;
            DtNetwork = false;
            DtRamDisk = false;
            DtRemovable = true;
            DtUnknown = false;
            GridZoom = 1;
            Precision = "1";
            TimeStamp = "M1";
            Use1024 = true;
            WindowLeft = 100;
            WindowTop = 100;
        }
        #endregion Constructor

        #region Properties
        public bool Brackets
        {
            get => brackets;
            set
            {
                brackets = value;
                OnPropertyChanged();
            }
        }

        public bool DtCDRom
        {
            get => dtCDRom;
            set
            {
                dtCDRom = value;
                OnPropertyChanged();
            }
        }

        public bool DtFixed
        {
            get => dtFixed;
            set
            {
                dtFixed = value;
                OnPropertyChanged();
            }
        }

        public bool DtNetwork
        {
            get => dtNetwork;
            set
            {
                dtNetwork = value;
                OnPropertyChanged();
            }
        }

        public bool DtRamDisk
        {
            get => dtRamDisk;
            set
            {
                dtRamDisk = value;
                OnPropertyChanged();
            }
        }

        public bool DtRemovable
        {
            get => dtRemovable;
            set
            {
                dtRemovable = value;
                OnPropertyChanged();
            }
        }

        public bool DtUnknown
        {
            get => dtUnknown; set
            {
                dtUnknown = value;
                OnPropertyChanged();
            }
        }

        public double GridZoom
        {
            get
            {
                if (gridZoom <= 0)
                {
                    gridZoom = 1;
                }
                return gridZoom;
            }
            set
            {
                gridZoom = value;
                OnPropertyChanged();
            }
        }

        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                OnPropertyChanged();
            }
        }

        public string Precision
        {
            get => precision;
            set
            {
                precision = value;
                OnPropertyChanged();
            }
        }

        public string TimeStamp
        {
            get => timeStamp; set
            {
                timeStamp = value;
                OnPropertyChanged();
            }
        }

        public bool Use1024
        {
            get => use1024;
            set
            {
                use1024 = value;
                OnPropertyChanged();
            }
        }

        public double WindowLeft
        {
            get
            {
                if (windowLeft < 0)
                {
                    windowLeft = 0;
                }
                return windowLeft;
            }
            set => windowLeft = value;
        }

        public double WindowTop
        {
            get
            {
                if (windowTop < 0)
                {
                    windowTop = 0;
                }
                return windowTop;
            }
            set => windowTop = value;
        }
        #endregion Properties

        #region Private backing fields
        private bool brackets;
        private bool dtCDRom;
        private bool dtFixed;
        private bool dtNetwork;
        private bool dtRamDisk;
        private bool dtRemovable;
        private bool dtUnknown;
        private double gridZoom;
        private string logFile;
        private string precision;
        private string timeStamp;
        private bool use1024;
        private double windowTop;
        private double windowLeft;
        #endregion Private backing fields

        #region Handle property change event
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Handle property change event
    }
}
