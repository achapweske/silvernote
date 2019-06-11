/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for DownloadProgressDialog.xaml
    /// </summary>
    public partial class DownloadProgressDialog : Window
    {
        public DownloadProgressDialog()
        {
            InitializeComponent();
        }

        public WebClient Client { get; set; }

        public string Message { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Client.DownloadProgressChanged += Client_DownloadProgressChanged;
            Client.DownloadDataCompleted += Client_DownloadDataCompleted;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Client.DownloadProgressChanged -= Client_DownloadProgressChanged;
            Client.DownloadDataCompleted -= Client_DownloadDataCompleted;
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageTextBlock.Text = e.Error.Message;
                OkButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.DialogResult = true;
            }
        }

    }
}
