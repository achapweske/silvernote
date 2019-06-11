/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Commands;
using System.Windows;
using System.Windows.Input;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for ScreenCaptureDialog.xaml
    /// </summary>
    public partial class ScreenCaptureDialog : Window
    {
        public ScreenCaptureDialog()
        {
            InitializeComponent();
        }

        public void CaptureRegion(bool hideThisWindow)
        {
            WindowState windowState = WindowState;
            if (hideThisWindow)
            {
                WindowState = WindowState.Minimized;
            }
            NInsertionCommands.CaptureRegion.Execute(null, Application.Current.MainWindow);
            WindowState = windowState;
        }

        public void CaptureWindow(bool hideThisWindow)
        {
            WindowState windowState = WindowState;
            if (hideThisWindow)
            {
                WindowState = WindowState.Minimized;
            }
            NInsertionCommands.CaptureWindow.Execute(null, Application.Current.MainWindow);
            WindowState = windowState;
        }

        public void CaptureScreen(bool hideThisWindow)
        {
            WindowState windowState = WindowState;
            if (hideThisWindow)
            {
                WindowState = WindowState.Minimized;
            }
            NInsertionCommands.CaptureScreen.Execute(null, Application.Current.MainWindow);
            WindowState = windowState;
        }

        private void CaptureRegionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CaptureRegion(true);

            if (KeepOpenCheckBox.IsChecked != true)
            {
                Close();
            }
        }

        private void CaptureWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CaptureWindow(true);

            if (KeepOpenCheckBox.IsChecked != true)
            {
                Close();
            }
        }

        private void CaptureScreenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CaptureScreen(true);

            if (KeepOpenCheckBox.IsChecked != true)
            {
                Close();
            }
        }
    }
}
