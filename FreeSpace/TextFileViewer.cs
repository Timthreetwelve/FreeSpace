﻿// Copyright(c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

// Comment out the following if MessageBox is not to be used
#define messagebox

#region using directives
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using NLog;
#endregion

namespace TKUtils
{
    /// <summary>
    ///  Class for viewing text files. If the file extension is not associated
    ///  with an application, notepad.exe will be attempted.
    /// </summary>
    public static class TextFileViewer
    {
        private static readonly Logger logTemp = LogManager.GetLogger("logTemp");

        #region Text file viewer
        /// <summary>
        /// Opens specified text file
        /// </summary>
        /// <param name="txtfile">Full path for text file</param>
        public static void ViewTextFile(string txtfile)
        {
            if (File.Exists(txtfile))
            {
                try
                {
                    using (Process p = new Process())
                    {
                        p.StartInfo.FileName = txtfile;
                        p.StartInfo.UseShellExecute = true;
                        p.StartInfo.ErrorDialog = false;
                        _ = p.Start();
                        logTemp.Debug($"Opening {txtfile} in default application");
                    }
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1155)
                    {
                        using (Process p = new Process())
                        {
                            p.StartInfo.FileName = "notepad.exe";
                            p.StartInfo.Arguments = txtfile;
                            p.StartInfo.UseShellExecute = true;
                            p.StartInfo.ErrorDialog = false;
                            _ = p.Start();
                            logTemp.Debug($"Opening {txtfile} in Notepad.exe");
                        }
                    }
                    else
                    {
#if messagebox
                        _ = TKMessageBox.Show($"Error reading file {txtfile}\n{ex.Message}",
                                              "Watcher Error",
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Error);
#endif
                        logTemp.Error(ex, $"* Unable to open {txtfile}");
                    }
                }
                catch (Exception ex)
                {
#if messagebox
                    _ = TKMessageBox.Show("Unable to start default application used to open\n" +
                                          $" {txtfile}",
                                          "Error",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
#endif
                    logTemp.Error(ex, $"* Unable to open {txtfile}");
                }
            }
            else
            {
                logTemp.Error($"File not found: {txtfile}");
            }
        }
        #endregion
    }
}
