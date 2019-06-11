/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SilverNote.Common
{
    public class EmbeddedFile
    {
        #region Fields

        Dispatcher _Dispatcher;
        string _FilePath;
        Process _Process;

        #endregion

        #region Constructors

        public EmbeddedFile()
        {
            _Dispatcher = Dispatcher.CurrentDispatcher;
        }

        public EmbeddedFile(string type, byte[] data)
            : this()
        {
            Type = type;
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// File extension associated with the embedded file (e.g. ".png")
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Embedded file data
        /// </summary>
        public byte[] Data { get; set; }

        public bool IsOpen
        {
            get {  return _Process != null; }
        }

        #endregion

        #region Events

        public event EventHandler Changed;

        #endregion

        #region Operations

        /// <summary>
        /// Open the embedded file using the default program associated
        /// with the file extension specified in the Type property
        /// </summary>
        /// <returns>true on success or false if an error occurs</returns>
        public bool Open()
        {
            return Open(null);
        }

        /// <summary>
        /// Open the embedded file using the given command
        /// </summary>
        /// <param name="command">The command used to open the file, or null to prompt the user to select an application</param>
        /// <returns>true on success or false if an error occurs</returns>
        public bool OpenWith(string command)
        {
            if (String.IsNullOrEmpty(command))
            {
                command = "rundll32.exe shell32.dll, OpenAs_RunDLL %1";
            }

            return Open(command);
        }

        #endregion

        #region Implementation

        [DllImport("user32.dll")]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        bool Open(string command)
        {
            try
            {
                // If already open, switch to the opened application

                if (_Process != null)
                {
                    SwitchToThisWindow(_Process.MainWindowHandle, true);
                    return true;
                }

                // Write embedded file data to a temporary file

                _FilePath = CreateTempFilePath(Type);
                File.WriteAllBytes(_FilePath, Data);

                if (String.IsNullOrEmpty(command))
                {
                    // Open the temporary file using the default application
                    // associated with the file's extension

                    _Process = Process.Start(_FilePath);
                }
                else
                {
                    // Open the temporary file using the given command

                    command = command.Replace("%1", _FilePath);
                    string fileName, arguments;
                    ParseCommand(command, out fileName, out arguments);

                    _Process = Process.Start(fileName, arguments);
                }

                if (_Process == null)
                {
                    return false;
                }

                // Notify when the file changes
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = Path.GetDirectoryName(_FilePath);
                watcher.Filter = Path.GetFileName(_FilePath);
                watcher.Changed += TempFile_Changed;
                watcher.EnableRaisingEvents = true;

                // Notify when the process exits
                _Process.Exited += Process_Exited;
                _Process.EnableRaisingEvents = true;

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + "\n\n" + e.StackTrace);
                return false;
            }
        }

        static string CreateTempFilePath(string extension)
        {
            string dirPath = Path.GetTempPath();
            dirPath = Path.Combine(dirPath, Path.GetRandomFileName());
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            string result = Path.GetRandomFileName();
            result = Path.ChangeExtension(result, extension);
            return Path.Combine(dirPath, result);
        }

        static void ParseCommand(string command, out string fileName, out string arguments)
        {
            Match match = Regex.Match(command, @"^(?<AppName>.*\.(exe|dll)\""?)\s+(?<Arguments>.*)$");
            fileName = match.Groups["AppName"].Value;
            arguments = match.Groups["Arguments"].Value;
        }

        void TempFile_Changed(object sender, FileSystemEventArgs e)
        {
            // This method is called from a non-UI thread

            _Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Data = File.ReadAllBytes(_FilePath);
                    
                }
                catch
                {
                    Data = null;
                }

                RaiseChanged();

            }), null);
        }

        void Process_Exited(object sender, EventArgs e)
        {
            // This method is called from a non-UI thread

            _Dispatcher.BeginInvoke(new Action(() =>
            {
                // The process has exited - dispose of the object
                // 
                // Note that the other application may not have actually exited
                // since this process may have spawned other processes.

                Process process = (Process)sender;
                if (process == _Process)
                {
                    _Process.Exited -= Process_Exited;
                    _Process.Dispose();
                    _Process = null;
                }

            }), null);
        }

        void RaiseChanged()
        {
            if (Changed != null)
            {
                Changed(this, EventArgs.Empty);
            }
        }

        #endregion

    }
}
