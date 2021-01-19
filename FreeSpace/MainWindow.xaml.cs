// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

#region Using directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Win32;
using TKUtils;
#endregion Using directives

namespace FreeSpace
{
    // FreeSpace logs free (available) space on specified drives. Run without command line argument
    // to show settings window. Run with /hide or /write to write to log without showing window
    public partial class MainWindow : Window
    {
        #region private variables
        private static readonly StringBuilder strDriveInfo = new StringBuilder();
        #endregion private variables

        public MainWindow()
        {
            UserSettings.Init(UserSettings.AppFolder, UserSettings.DefaultFilename, true);

            InitializeComponent();

            ReadSettings();

            ProcessCommandLine();

            InitializeComboboxes();
        }

        #region Read Settings
        private void ReadSettings()
        {
            // Handle what didn't get handled
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Put version in the window title
            Title = AppInfo.AppName + " - " + AppInfo.TitleVersion;
            WriteLog.WriteTempFile($"{AppInfo.AppName} {AppInfo.TitleVersion} is starting up");

            // Window position
            Top = UserSettings.Setting.WindowTop;
            Left = UserSettings.Setting.WindowLeft;

            // Set window zoom level
            double curZoom = UserSettings.Setting.GridZoom;
            grid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);

            // Use either GB or GiB for space measurements
            if (UserSettings.Setting.Use1024)
            {
                chkGiB.IsChecked = true;
            }
            else
            {
                chkGB.IsChecked = true;
            }

            // Put the log file on the desktop if it isn't specified
            if (string.IsNullOrEmpty(UserSettings.Setting.LogFile))
            {
                UserSettings.Setting.LogFile = Path.Combine(SpecialFolders.GetDesktopFolder(), "FreeSpace.log");
            }
            WriteLog.WriteTempFile($"Log file is {UserSettings.Setting.LogFile}");

            // Settings change event
            UserSettings.Setting.PropertyChanged += UserSettingChanged;
        }
        #endregion Read Settings

        #region Get info from each drive that is "ready"
        private void GetDriveInfo()
        {
            // DriveInfo gets the particulars for each drive
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                // Determine if drive type is ready and if the type is wanted
                switch (drive.DriveType)
                {
                    case DriveType.Unknown:
                        if (UserSettings.Setting.DtUnknown && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Removable:
                        if (UserSettings.Setting.DtRemovable && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Fixed:
                        if (UserSettings.Setting.DtFixed && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Network:
                        if (UserSettings.Setting.DtNetwork && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.CDRom:
                        if (UserSettings.Setting.DtCDRom && drive.IsReady)
                        {
                            FormatLogLine(drive);
                        }
                        break;

                    case DriveType.Ram:
                        if (UserSettings.Setting.DtRamDisk && drive.IsReady)
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
            int GBPref = UserSettings.Setting.Use1024 ? 1024 : 1000;
            double freeSpace = drive.TotalFreeSpace / Math.Pow(GBPref, 3);

            switch (UserSettings.Setting.Precision.ToLower())
            {
                // No decimals, no thousands separator
                case "n":
                    _ = strDriveInfo.AppendFormat($"{driveName} {freeSpace,4:###0} GB   ");
                    break;

                // No decimals
                case "0":
                    _ = strDriveInfo.AppendFormat($"{driveName} {freeSpace,5:N0} GB   ");
                    break;

                // 1 decimal place
                case "1":
                    _ = strDriveInfo.AppendFormat($"{driveName} {freeSpace,7:N1} GB   ");
                    break;

                // 3 decimal places
                case "2":
                    _ = strDriveInfo.AppendFormat($"{driveName} {freeSpace,8:N2} GB   ");
                    break;

                // 2 decimal places
                case "3":
                    _ = strDriveInfo.AppendFormat($"{driveName} {freeSpace,9:N3} GB   ");
                    break;
            }
        }
        #endregion Format the free space amount

        #region Write to log file
        private void WriteToLog()
        {
            bool result;

            if (UserSettings.Setting.Brackets)
            {
                result = WriteLog.WriteLogFile(UserSettings.Setting.LogFile, strDriveInfo.ToString(),
                                               UserSettings.Setting.TimeStamp, 'Y');
            }
            else
            {
                result = WriteLog.WriteLogFile(UserSettings.Setting.LogFile, strDriveInfo.ToString(),
                                               UserSettings.Setting.TimeStamp, 'N');
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

                    // Only write to log file when the window is hidden
                    GetDriveInfo();
                    WriteToLog();

                    // Shutdown after the log file was written to
                    Application.Current.Shutdown();
                }
                else if (item != args[0])
                {
                    WriteLog.WriteTempFile($"Unknown command line argument  \"{item}\" found.");
                }
            }
        }
        #endregion Command line arguments

        #region Window events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UserSettings.Setting.WindowLeft = Left;
            UserSettings.Setting.WindowTop = Top;
            UserSettings.SaveSettings();

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
            if (!string.IsNullOrEmpty(UserSettings.Setting.LogFile))
            {
                dlgOpen.FileName = UserSettings.Setting.LogFile;
            }
            bool? result = dlgOpen.ShowDialog();
            if (result == true)
            {
                UserSettings.Setting.LogFile = dlgOpen.FileName;
            }
        }
        #endregion Window events

        #region Mouse Events
        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            if (e.Delta > 0)
            {
                GridLarger();
            }
            else if (e.Delta < 0)
            {
                GridSmaller();
            }
        }
        #endregion Mouse Events

        #region Keyboard Events
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.NumPad0 && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                GridSizeReset();
            }

            if (e.Key == Key.Add && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                GridLarger();
            }

            if (e.Key == Key.Subtract && (Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                GridSmaller();
            }
            if (e.Key == Key.F1)
            {
                About about = new About
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                _ = about.ShowDialog();
            }
        }
        #endregion Keyboard Events

        #region Initialize combo boxes
        private void InitializeComboboxes()
        {
            InitializePrecisionCombobox();

            InitializeTimestampCombobox();
        }

        private void InitializeTimestampCombobox()
        {
            cbxTimeStamp.ItemsSource = new List<TimeStamp>
            {
                new TimeStamp { Description = "MM/dd/yy HH:mm", Value = 'S' },
                new TimeStamp { Description = "MM/dd/yyyy HH:mm", Value = 'T' },
                new TimeStamp { Description = "MM/dd/yyyy HH:mm:ss", Value = 'U' },
                new TimeStamp { Description = "yyyy/MM/dd HH:mm", Value = 'F' },
                new TimeStamp { Description = "yyyy/MM/dd HH:mm:ss", Value = 'E' },
                new TimeStamp { Description = "dd MMM yyyy HH:mm", Value = 'N' },
                new TimeStamp { Description = "dd MMM yyyy HH:mm:ss", Value = 'M' }
            };
            cbxTimeStamp.SelectedValue = UserSettings.Setting.TimeStamp;
        }

        private void InitializePrecisionCombobox()
        {
            cbxPrecision.ItemsSource = new List<PrecisionClass>
            {
                new PrecisionClass { Description = "0", Value = "0" },
                new PrecisionClass { Description = "1", Value = "1" },
                new PrecisionClass { Description = "2", Value = "2" },
                new PrecisionClass { Description = "3", Value = "3" },
                new PrecisionClass { Description = "No Separator", Value = "n" }
            };
            cbxPrecision.SelectedValue = UserSettings.Setting.Precision;
        }
        #endregion Initialize combo boxes

        #region Combo box events
        private void CbxPrecision_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxPrecision.SelectedItem != null)
            {
                PrecisionClass x = (PrecisionClass)cbxPrecision.SelectedItem;
                UserSettings.Setting.Precision = x.Value;

                switch (UserSettings.Setting.Precision)
                {
                    case "0":
                        tbDPlaces.Text = "1,234 GB";
                        break;
                    case "1":
                        tbDPlaces.Text = "1,234.5 GB";
                        break;
                    case "2":
                        tbDPlaces.Text = "1,234.56 GB";
                        break;
                    case "3":
                        tbDPlaces.Text = "1,234.567 GB";
                        break;
                    case "n":
                        tbDPlaces.Text = "1234 GB";
                        break;
                }
            }
        }

        private void CbxTimeStamp_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cbxTimeStamp.SelectedItem != null)
            {
                TimeStamp x = (TimeStamp)cbxTimeStamp.SelectedItem;
                UserSettings.Setting.TimeStamp = x.Value;
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
            UserSettings.SaveSettings();
            GetDriveInfo();
            WriteToLog();
            TextFileViewer.ViewTextFile(UserSettings.Setting.LogFile);
        }

        private void MnuViewLog_Click(object sender, RoutedEventArgs e)
        {
            TextFileViewer.ViewTextFile(UserSettings.Setting.LogFile);
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

        #region Unhandled Exception Handler
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            WriteLog.WriteTempFile("Unhandled Exception");
            Exception e = (Exception)args.ExceptionObject;
            WriteLog.WriteTempFile(e.Message);
            WriteLog.WriteTempFile(e.InnerException.ToString());
            WriteLog.WriteTempFile(e.StackTrace);
        }
        #endregion

        #region Setting Change
        private void UserSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyInfo prop = sender.GetType().GetProperty(e.PropertyName);
            var newValue = prop?.GetValue(sender, null);

            Debug.WriteLine($"***Setting change: {e.PropertyName} New Value: {newValue}");
        }
        #endregion Setting Change

        #region Grid Size
        private void GridSmaller()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom > 0.9)
            {
                curZoom -= .05;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
            grid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        private void GridLarger()
        {
            double curZoom = UserSettings.Setting.GridZoom;
            if (curZoom < 1.3)
            {
                curZoom += .05;
                UserSettings.Setting.GridZoom = Math.Round(curZoom, 2);
            }
            grid1.LayoutTransform = new ScaleTransform(curZoom, curZoom);
        }
        private void GridSizeReset()
        {
            UserSettings.Setting.GridZoom = 1.0;
            grid1.LayoutTransform = new ScaleTransform(1, 1);
        }
        #endregion Grid Size
    }
}