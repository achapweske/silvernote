/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for ExceptionDialog.xaml
    /// </summary>
    public partial class ExceptionDialog : Window
    {
        public ExceptionDialog()
        {
            InitializeComponent();
        }

        public string Report { get; set; }
        public bool SendReport { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DebugTextBox.Text = Report;
            SendReportCheckBox.IsChecked = SendReport;
        }

        protected override void OnClosed(EventArgs e)
        {
            SendReport = SendReportCheckBox.IsChecked == true;
            base.OnClosed(e);
        }

        private void ViewReportButton_Click(object sender, RoutedEventArgs e)
        {
            DebugTextBox.Visibility = Visibility.Visible;
        }
    }
}
