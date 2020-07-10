// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using TKUtils;
#endregion Using directives

namespace FreeSpace
{
    // FreeSpace logs free (available) space on specified drives. Run without command line argument
    // to show settings window. Run with /hide or /write to write to log without showing window
    public partial class MainWindow : Window
    {
        #region private variables
        private MySettings mySettings;
        private static readonly StringBuilder strDriveInfo = new StringBuilder();
        #endregion private variables

        public MainWindow()
        {
            InitializeComponent();

            ReadSettings();

            ProcessCommandLine();

            InitializeComboboxes();
        }

        #region Read Settings
        private void ReadSettings()
        {
            Title = AppInfo.AppName + " - " + AppInfo.TitleVersion;
            WriteLog.WriteTempFile($"{AppInfo.AppName} {AppInfo.TitleVersion} is starting up");

            mySettings = MySettings.Read();
            DataContext = mySettings;

            Top = mySettings.WindowTop;
            Left = mySettings.WindowLeft;

            if (string.IsNullOrEmpty(mySettings.LogFile))
            {
                mySettings.LogFile = Path.Combine(SpecialFolders.GetDesktopFolder(), "FreeSpace.log");
            }
            WriteLog.WriteTempFile($"Log file is {mySettings.LogFile}");
        }
        #endregion Read Settings

        #region Get info from each drive that is "ready"
        private void GetDriveInfo()
        {
            // DriveInfo gets the particulars for each drive
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                // Determine if drive type is ready and if the type is wanted
                switch (drive.DriveType)
                {
                    case DriveType.Unknown:
                        if (mySettings.DtUnknown && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Removable:
                        if (mySettings.DtRemovable && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Fixed:
                        if (mySettings.DtFixed && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Network:
                        if (mySettings.DtNetwork && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.CDRom:
                        if (mySettings.DtCDRom && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Ram:
                        if (mySettings.DtRamDisk && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    default:
                        Debug.WriteLine("Switch statement fell through to default");
                        break;
                }
            }
        }
        #endregion Get info from each drive that is "ready"

        #region Format the free space amount

        private void FormatLogLine(DriveInfo drive)
        {
            // Trim the trailing backslash
            string driveName = drive.Name.TrimEnd('\\');

            // Convert to GB
            double freeSpace = (drive.TotalFreeSpace / Math.Pow(1024, 3));

            switch (mySettings.Precision.ToLower())
            {
                // No decimals, no thousands separator
                case "n":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,4:###0} GB   ");
                    break;

                // No decimals
                case "0":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,5:N0} GB   ");
                    break;

                // 1 decimal place
                case "1":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,7:N1} GB   ");
                    break;

                // 2 decimal places
                case "2":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,8:N2} GB   ");
                    break;
            }
        }

        #endregion Format the free space amount

        #region Write to log file
        private void WriteToLog()
        {
            bool result;

            if (mySettings.Brackets)
            {
                result = WriteLog.WriteLogFile(mySettings.LogFile, strDriveInfo.ToString(), mySettings.TimeStamp, 'Y');
            }
            else
            {
                result = WriteLog.WriteLogFile(mySettings.LogFile, strDriveInfo.ToString(), mySettings.TimeStamp, 'N');
            }

            WriteLog.WriteTempFile(strDriveInfo.ToString());

            strDriveInfo.Clear();

            if (!result)
            {
                _ = MessageBox.Show($"Error writing to log file\n{WriteLog.WLMessage}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
            }
        }
        #endregion Write to log file

        #region Command line arguments
        private void ProcessCommandLine()
        {
            // If count is less that two, bail out
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 2)
            {
                return;
            }

            foreach (string item in args)
            {
                // If command line argument "write" or "hide" is found, execute without showing window.
                string arg = item.Replace("-", "").Replace("/", "").ToLower();

                if (arg == "write" || arg == "hide")
                {
                    WriteLog.WriteTempFile($"Command line argument \"{item}\" found.");

                    // hide the window
                    Visibility = Visibility.Hidden;

                    // Only write so log file when the window is hidden
                    GetDriveInfo();
                    WriteToLog();

                    // Shutdown after the log file was written to
                    Application.Current.Shutdown();
                }
                else
                {
                    if (item != args[0])
                    {
                        WriteLog.WriteTempFile($"Unknown command line argument  \"{item}\" found.");
                    }
                }
            }
        }
        #endregion Command line arguments

        #region Window events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mySettings.WindowLeft = Left;
            mySettings.WindowTop = Top;
            MySettings.Save(mySettings);

            WriteLog.WriteTempFile($"{AppInfo.AppName} is shutting down");
        }

        // File picker
        private void BtnOpenDlg_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpen = new OpenFileDialog
            {
                Title = "Choose Log File ",
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "All files|*.*"
            };
            if (!string.IsNullOrEmpty(mySettings.LogFile))
            {
                dlgOpen.FileName = mySettings.LogFile;
            }
            bool? result = dlgOpen.ShowDialog();
            if (result == true)
            {
                mySettings.LogFile = dlgOpen.FileName;
            }
        }
        #endregion Window events

        #region Initialize combo boxes
        private void InitializeComboboxes()
        {
            InitializePrecisionCombobox();

            InitializeTimestampCombobox();
        }

        private void InitializeTimestampCombobox()
        {
            List<TimeStamp> tslist = new List<TimeStamp>
            {
                new TimeStamp { Description = "MM/dd/yy HH:mm", Value = 'S' },
                new TimeStamp { Description = "MM/dd/yyyy HH:mm:ss", Value = 'U' },
                new TimeStamp { Description = "MM/dd/yyyy HH:mm:ss.ffff", Value = 'L' },
                new TimeStamp { Description = "yyyy/MM/dd HH:mm:ss", Value = 'E' },
                new TimeStamp { Description = "dd MMM yyyy HH:mm:ss", Value = 'M' }
            };
            cbxTimeStamp.ItemsSource = tslist;
            cbxTimeStamp.SelectedValue = mySettings.TimeStamp;
        }

        private void InitializePrecisionCombobox()
        {
            List<PrecisionClass> plist = new List<PrecisionClass>
            {
                new PrecisionClass { Description = "0 - 9,999 GB", Value = "0" },
                new PrecisionClass { Description = "1 - 9,999.9 GB", Value = "1" },
                new PrecisionClass { Description = "2 - 9,999.99 GB", Value = "2" },
                new PrecisionClass { Description = "No Separator", Value = "n" }
            };

            cbxPrecision.ItemsSource = plist;
            cbxPrecision.SelectedValue = mySettings.Precision;
        }
        #endregion Initialize combo boxes

        #region Combo box events
        private void CbxPrecision_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxPrecision.SelectedItem != null)
            {
                PrecisionClass x = (PrecisionClass)cbxPrecision.SelectedItem;
                mySettings.Precision = x.Value;
            }
        }

        private void CbxTimeStamp_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxTimeStamp.SelectedItem != null)
            {
                TimeStamp x = (TimeStamp)cbxTimeStamp.SelectedItem;
                mySettings.TimeStamp = x.Value;
            }
        }
        #endregion Combo box events

        #region Menu events
        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MnuTest_Click(object sender, RoutedEventArgs e)
        {
            MySettings.Save(mySettings);
            GetDriveInfo();
            WriteToLog();
            TextFileViewer.ViewTextFile(mySettings.LogFile);
        }

        private void MnuViewLog_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(mySettings.LogFile);
        }

        private void MnuAbout_Click(object sender, RoutedEventArgs e)
        {
            About about = new About
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            about.ShowDialog();
        }

        private void MnuViewReadMe_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(@".\ReadMe.txt");
        }

        private void MnuTaskSched_Click(object sender, RoutedEventArgs e)
        {
            using (Process taskSched = new Process())
            {
                taskSched.StartInfo.FileName = "mmc.exe";
                taskSched.StartInfo.Arguments = @"c:\windows\system32\taskschd.msc";
                taskSched.Start();
            }
        }
        #endregion Menu events
    }
}