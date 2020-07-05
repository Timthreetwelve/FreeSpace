// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private readonly Properties.Settings settings = Properties.Settings.Default;
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
            if (settings.SettingsUpgradeRequired)
            {
                settings.Upgrade();
                settings.SettingsUpgradeRequired = false;
                settings.Save();
                CleanUp.CleanupPrevSettings();
                Debug.WriteLine("*** SettingsUpgradeRequired");
            }

            Properties.Settings.Default.SettingChanging += SettingChanging;

            Title = AppInfo.AppName + " - " + AppInfo.TitleVersion;
            WriteLog.WriteTempFile($"{AppInfo.AppName} {AppInfo.TitleVersion} is starting up");

            Top = settings.WindowTop;
            Left = settings.WindowLeft;

            if (string.IsNullOrEmpty(settings.LogFile))
            {
                settings.LogFile = Path.Combine(SpecialFolders.GetDesktopFolder(), "FreeSpace.log");
            }
            WriteLog.WriteTempFile($"Log file is {settings.LogFile}");
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
                        if (settings.dtUnknown && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Removable:
                        if (settings.dtRemoveable && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Fixed:
                        if (settings.dtFixed && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Network:
                        if (settings.dtNetwork && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.CDRom:
                        if (settings.dtCDRom && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Ram:
                        if (settings.dtRamDisk && drive.IsReady)
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

            switch (settings.Precision)
            {
                // No decimals, no thousands separator
                case "NoSep":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,4:###0} GB   ");
                    break;

                // No decimals
                case "Zero":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,5:N0} GB   ");
                    break;

                // 1 decimal place
                case "One":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,7:N1} GB   ");
                    break;

                // 2 decimal places
                case "Two":
                    strDriveInfo.AppendFormat($"{driveName} {freeSpace,8:N2} GB   ");
                    break;
            }
        }

        #endregion Format the free space amount

        #region Write to log file
        private void WriteToLog()
        {
            bool result;

            if (settings.Brackets)
            {
                result = WriteLog.WriteLogFile(settings.LogFile, strDriveInfo.ToString(), settings.TimeStamp, 'Y');
            }
            else
            {
                result = WriteLog.WriteLogFile(settings.LogFile, strDriveInfo.ToString(), settings.TimeStamp, 'N');
            }

            WriteLog.WriteTempFile(strDriveInfo.ToString());

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

        #region Settings change events
        private void SettingChanging(object sender, SettingChangingEventArgs e)
        {
            // Using this method to ensure that text boxes aren't left blank
            switch (e.SettingName)
            {
                case "LogFile":
                    {
                        if (string.IsNullOrWhiteSpace(e.NewValue.ToString()))
                        {
                            _ = MessageBox.Show(this, "Log file name cannot be blank",
                                "FreeSpace Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            e.Cancel = true;
                        }
                        break;
                    }

                default:
                    break;
            }

            Debug.WriteLine($"*** Changing {e.SettingName}, new value: {e.NewValue}");

            // Write settings changes (except window changes) to temp file
            if (!e.SettingName.StartsWith("Window"))
            {
                WriteLog.WriteTempFile($"Changing setting {e.SettingName} to {e.NewValue}");
            }
        }
        #endregion Settings change events

        #region Window events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.WindowLeft = Left;
            Properties.Settings.Default.WindowTop = Top;
            Properties.Settings.Default.Save();

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
            if (!string.IsNullOrEmpty(settings.LogFile))
            {
                dlgOpen.FileName = settings.LogFile;
            }
            bool? result = dlgOpen.ShowDialog();
            if (result == true)
            {
                settings.LogFile = dlgOpen.FileName;
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
            cbxTimeStamp.SelectedValue = settings.TimeStamp;
        }

        private void InitializePrecisionCombobox()
        {
            List<PrecisionClass> plist = new List<PrecisionClass>
            {
                new PrecisionClass { Description = "0 - 9,999 GB", Value = "Zero" },
                new PrecisionClass { Description = "1 - 9,999.9 GB", Value = "One" },
                new PrecisionClass { Description = "2 - 9,999.99 GB", Value = "Two" },
                new PrecisionClass { Description = "No Separator", Value = "NoSep" }
            };

            cbxPrecision.ItemsSource = plist;
            cbxPrecision.SelectedValue = settings.Precision;
        }
        #endregion Initialize combo boxes

        #region Combo box events
        private void CbxPrecision_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxPrecision.SelectedItem != null)
            {
                PrecisionClass x = (PrecisionClass)cbxPrecision.SelectedItem;
                settings.Precision = x.Value;
            }
        }

        private void CbxTimeStamp_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxTimeStamp.SelectedItem != null)
            {
                TimeStamp x = (TimeStamp)cbxTimeStamp.SelectedItem;
                settings.TimeStamp = x.Value;
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
            settings.Save();
            GetDriveInfo();
            WriteToLog();
            TextFileViewer.ViewTextFile(settings.LogFile);
        }

        private void MnuViewLog_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(settings.LogFile);
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