/*
 * Copyright (c) Adam Chapweske
 * 
 * Use of this software is governed by an MIT license that can be found in the LICENSE file at the root of this project.
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Diagnostics;
using SilverNote.Common;
using SilverNote.Editor;
using SilverNote.Models;
using SilverNote.Properties;
using SilverNote.Data.Models;
using SilverNote.ViewModels;
using SilverNote.Views;
using SilverNote.Dialogs;
using System.Threading;
using SilverNote.Commands;
using System.Windows.Media;
using SilverNote.Client;
using SilverNote.Server;

namespace SilverNote
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        #region Fields

        RepositoryManager _RepositoryManager;
        NotebookManager _NotebookManager;
        double _DrawingSmoothness;

        #endregion

        #region Constructors

        public App()
        {
            _RepositoryManager = new RepositoryManager();
            _RepositoryManager.Opened += RepositoryManager_Opened;
            _RepositoryManager.Closing += RepositoryManager_Closing;
            _NotebookManager = new NotebookManager(_RepositoryManager);
            _DrawingSmoothness = 256;
        }

        #endregion

        #region Properties

        public RepositoryManager RepositoryManager
        {
            get { return _RepositoryManager; }
        }

        public NotebookManager NotebookManager
        {
            get { return _NotebookManager; }
        }

        /// <summary>
        /// The smoothness of lines drawn with the SmartPencil tool (larger numbers = smoother lines)
        /// </summary>
        public double DrawingSmoothness
        {
            get
            {
                return _DrawingSmoothness;
            }
            set
            {
                if (value != _DrawingSmoothness)
                {
                    _DrawingSmoothness = value;
                    RaisePropertyChanged("DrawingSmoothness");
                }
            }
        }

        /// <summary>
        /// Determine if we're running in debug mode (used by XAML to conditionally show the Debug menu)
        /// </summary>
        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Operations

        /// <summary>
        /// Display the Settings dialog
        /// </summary>
        /// <param name="tabName">Name of tab to default to</param>
        public static void Configure(string tabName = null)
        {
            var settings = Settings.Default;

            // Create dialog and initialize to current settings
            var dialog = new SettingsDialog();
            dialog.InitialTab = tabName;
            dialog.NewNoteHotKey = settings.NewNoteHotKey;
            dialog.CaptureSelectionHotKey = settings.CaptureSelectionHotKey;
            dialog.CaptureRegionHotKey = settings.CaptureRegionHotKey;
            dialog.CaptureWindowHotKey = settings.CaptureWindowHotKey;
            dialog.CaptureScreenHotKey = settings.CaptureScreenHotKey;
            dialog.Shortcuts = NCommands.ToShortcuts();
            dialog.DropFileAsImage = settings.DropFileAsImage;
            dialog.LookupServices = settings.LookupCommands;

            dialog.Owner = App.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                // Apply new settings

                var repository = App.Current.MainWindow as RepositoryView;
                if (repository != null)
                {
                    repository.NewNoteHotKey.RegisterHotKey(dialog.NewNoteHotKey);
                    repository.CaptureSelectionHotKey.RegisterHotKey(dialog.CaptureSelectionHotKey);
                    repository.CaptureRegionHotKey.RegisterHotKey(dialog.CaptureRegionHotKey);
                    repository.CaptureWindowHotKey.RegisterHotKey(dialog.CaptureWindowHotKey);
                    repository.CaptureScreenHotKey.RegisterHotKey(dialog.CaptureScreenHotKey);
                }
                NCommands.LoadShortcuts(dialog.Shortcuts);
                NoteEditor.LookupServices = settings.LookupCommands;

                // Save new settings

                settings.NewNoteHotKey = dialog.NewNoteHotKey;
                settings.CaptureSelectionHotKey = dialog.CaptureSelectionHotKey;
                settings.CaptureRegionHotKey = dialog.CaptureRegionHotKey;
                settings.CaptureWindowHotKey = dialog.CaptureWindowHotKey;
                settings.CaptureScreenHotKey = dialog.CaptureScreenHotKey;
                settings.Shortcuts = dialog.Shortcuts;
                settings.DropFileAsImage = dialog.DropFileAsImage;

                try
                {
                    settings.Save();
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.Message);
                }
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// Load default settings and upgrade settings from previous versions of SilverNote if needed
        /// </summary>
        public void UpdateSettings()
        {
            bool settingsChanged = false;

            if (Settings.Default.UpgradeRequired)
            {
                try
                {
                    Settings.Default.Upgrade();
                }
                catch (Exception e)
                {
                    string message = "An error occurred while importing settings from a previous version of SilverNote:\n\n";
                    message = message + e.Message + "\n\n";
                    message += "Some settings may have been lost";
                    MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }

                Settings.Default.UpgradeRequired = false;
                settingsChanged = true;
            }

            if (Settings.Default.RecentRepositories == null)
            {
                Settings.Default.RecentRepositories = new RepositoryLocationCollection();
                settingsChanged = true;
            }

            if (Settings.Default.ClientID == 0)
            {
                Settings.Default.ClientID = IDGenerator.RandomID();
                settingsChanged = true;
            }

            // Load default lookup services

            if (Settings.Default.LookupCommands == null)
            {
                Settings.Default.LookupCommands = new LookupCollection(new[] {
                    new LookupService
                    {
                        Name = "Google",
                        Command = "http://www.google.com/search?q=%phrase%"
                    },
                    new LookupService
                    {
                        Name = "Wikipedia",
                        Command = "http://www.wikipedia.org/wiki/%phrase%"
                    },
                    new LookupService
                    {
                        Name = "Dictionary",
                        Command = "http://www.thefreedictionary.com/%phrase%"
                    }
                });
                settingsChanged = true;
            }

            // Add installed time if this is the first run

            if (!Settings.Default.HasInstalledTime)
            {
                Settings.Default.OriginalVersion = Assembly.GetExecutingAssembly().GetName().Version;
                Settings.Default.InstalledTime = DateTime.Now;
                Settings.Default.HasInstalledTime = true;
                settingsChanged = true;
            }

            if (settingsChanged)
            {
                try
                {
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
            }
        }

        /// <summary>
        /// Load user-defined shortcuts
        /// </summary>
        public void LoadShortcuts()
        {
            var shortcuts = Settings.Default.Shortcuts;
            if (shortcuts != null)
            {
                NCommands.LoadShortcuts(shortcuts);
            }
        }

        /// <summary>
        /// Load user-defined lookup services
        /// </summary>
        public void LoadLookups()
        {
            NoteEditor.LookupServices = Settings.Default.LookupCommands;
        }

        /// <summary>
        /// Main application entry point
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Shutdown only when Shutdown() is explicitly called
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Set a global exception hanndler for uncaught exceptions
            DispatcherUnhandledException += OnUnhandledException;

            // Add a fallback assembly resolver to handle resolution errors in a user-friendly way
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            // Load default settings, and upgrade settings from previous version of SilverNote if needed
            UpdateSettings();

            // Create the main window
            MainWindow = new RepositoryView();
            MainWindow.Closed += MainWindow_Closed;

            // Load user-defined shortcuts and lookup services
            LoadShortcuts();
            LoadLookups();

            // If a notebook name was passed, save it for use after the repository is initialized
            if (e.Args != null && e.Args.Count() > 0)
            {
                this.Properties["FileName"] = e.Args.Last();
            }

            // The default (and only) repository currently supported is the user's local application data folder.
            // This may be extended in the future to allow remote locations such as a web server URL.
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var location = new RepositoryLocationModel(filePath);
            if (!RepositoryManager.Open(location))
            {
                Shutdown();
            }
        }

        /// <summary>
        /// Called when MainWindow is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            RepositoryManager.Close();
            Shutdown();
        }

        /// <summary>
        /// Called when the repository is successfully opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RepositoryManager_Opened(object sender, RepositoryEventArgs e)
        {
            var repository = e.Repository;

            // Generate a view model and set it as the context for the main window
            MainWindow.DataContext = RepositoryViewModel.FromModel(repository);

            // Enable client request/response logging in debug mode
#if DEBUG
            repository.Source.Client.IsLogging = true;
#endif

            // Fetch metadadta for the newly-opened repository
            repository.Source.Client.GetRepositorySucceeded += Client_GetRepositorySucceeded;
            repository.Source.Client.Priority = ThreadPriority.AboveNormal;
            repository.Source.Client.GetRepository();
        }

        /// <summary>
        /// Called when the repository has been successfully fetched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Client_GetRepositorySucceeded(object sender, RepositoryDataEventArgs e)
        {
            var repository = RepositoryManager.CurrentRepository;
            repository.Source.Client.GetRepositorySucceeded -= Client_GetRepositorySucceeded;
            repository.Source.Client.Priority = System.Threading.ThreadPriority.Normal;

            if (!MainWindow.IsLoaded)
            {
                MainWindow.Show();

                // Force the main window to the foreground
                // (required when InstallShield launches the app on Vista)
                MainWindow.Topmost = true;
                MainWindow.Topmost = false;
                MainWindow.Activate();
            }

            if (Properties.Contains("FileName"))
            {
                // A notebook was specified on the command line - open it
                NotebookManager.Open(Properties["FileName"].ToString());
            }
            else if (NotebookManager.DefaultLocation != null)
            {
                // Otherwise, open the most recently-opened notebook (if any)
                NotebookManager.Open(NotebookManager.DefaultLocation);
            }
        }

        /// <summary>
        /// Called when RepositoryManager is about to close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RepositoryManager_Closing(object sender, RepositoryEventArgs e)
        {
            // Write any unsaved changes
            Messenger.Instance.Notify("Flush", e.Repository);
        }

        /// <summary>
        /// The set of missing assemblies - we keep track of these in order to suppress repeated errors for the same assembly
        /// </summary>
        private HashSet<string> _MissingAssemblies = new HashSet<string>();

        /// <summary>
        /// Fallback assembly resolution handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            var references = Assembly.GetExecutingAssembly().GetReferencedAssemblies();

            if (!references.Contains(new AssemblyName(args.Name)))
            {
                return null;
            }

            if (!_MissingAssemblies.Contains(args.Name))
            {
                _MissingAssemblies.Add(args.Name);

                string message = "Unable to locate the following file:\n\n";
                message += "\"" + args.Name + "\"\n\n";
                message += "To fix this error, please re-install SilverNote.";

                MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        /// <summary>
        /// Global callback for unhandled exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var dialog = new ExceptionDialog();

            dialog.Report = CrashReporter.CreateReport(e.Exception);
            dialog.SendReport = true;

            if (dialog.ShowDialog() == true)
            {
                // Restart the application
                Process.Start(Application.ResourceAssembly.Location);
            }

            if (dialog.SendReport)
            {
                CrashReporter.ReportCrash(dialog.Report);
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            VerifyProperty(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        public void VerifyProperty(string propertyName)
        {
            Type type = this.GetType();
            if (type.GetProperty(propertyName) == null)
            {
                string msg = String.Format("{0} is not a public property of {1}", propertyName, type.FullName);
                throw new ArgumentOutOfRangeException(propertyName, msg);
            }
        }

        #endregion

    }
}
