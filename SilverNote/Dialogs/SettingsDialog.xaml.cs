/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InternalLookupServices = new ObservableCollection<LookupService>();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (LookupServices != null)
            {
                foreach (var lookupService in LookupServices)
                {
                    InternalLookupServices.Add(lookupService);
                }
            }

            InternalLookupServices.Add(new LookupService());

            // Select initial tab

            if (!String.IsNullOrEmpty(InitialTab))
            {
                foreach (TabItem item in Tabs.Items)
                {
                    string header = item.Header as string;

                    if (InitialTab.Equals(header, StringComparison.CurrentCultureIgnoreCase))
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DialogResult == true)
            {
                LookupServices.Clear();

                foreach (var lookupService in InternalLookupServices)
                {
                    if (!String.IsNullOrWhiteSpace(lookupService.Name) &&
                        !String.IsNullOrWhiteSpace(lookupService.Command))
                    {
                        LookupServices.Add(lookupService);
                    }
                }
            }
        }

        public string InitialTab { get; set; }
        public string NewNoteHotKey { get; set; }
        public string CaptureSelectionHotKey { get; set; }
        public string CaptureRegionHotKey { get; set; }
        public string CaptureWindowHotKey { get; set; }
        public string CaptureScreenHotKey { get; set; }
        public NShortcutCollection Shortcuts { get; set; }
        public bool DropFileAsImage { get; set; }
        public LookupCollection LookupServices { get; set; }

        #region Shortcuts

        private void DefaultShortcut_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var shortcut = (NShortcut)button.DataContext;
            var command = NCommands.GetCommand(shortcut.Command);
            shortcut.Gesture = command.DefaultGestureText;
        }

        #endregion

        #region Lookup

        public ObservableCollection<LookupService> InternalLookupServices { get; set; }

        private void DeleteLookupService_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var item = (LookupService)element.DataContext;

            InternalLookupServices.Remove(item);

            if (InternalLookupServices.Count == 0)
            {
                InternalLookupServices.Add(new LookupService());
            }
        }

        private void LookupService_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var item = (LookupService)element.DataContext;

            if (item == InternalLookupServices.Last())
            {
                if (!String.IsNullOrWhiteSpace(item.Name) ||
                    !String.IsNullOrWhiteSpace(item.Command))
                {
                    InternalLookupServices.Add(new LookupService());
                }
            }
        }

        #endregion
    }
}
