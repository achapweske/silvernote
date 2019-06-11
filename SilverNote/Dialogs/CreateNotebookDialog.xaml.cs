/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Microsoft.Win32;
using System;
using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for CreateNotebookDialog.xaml
    /// </summary>
    public partial class CreateNotebookDialog : Window
    {
        #region Constructors

        public CreateNotebookDialog()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        #endregion

        #region Properties

        public string FilePath { get; set; }

        #endregion

        #region Implementation

        string CurrentDirectory { get; set; }
        string CurrentExtension { get; set; }

        void UpdateFilePath(string filePath)
        {
            // Display just the file name in the textbox, and keep track of the directory + extension

            if (!String.IsNullOrEmpty(filePath))
            {
                FilePathTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(filePath);
                CurrentDirectory = System.IO.Path.GetDirectoryName(filePath);
                CurrentExtension = System.IO.Path.GetExtension(filePath);
            }

            // Use default values for missing components

            if (String.IsNullOrEmpty(FilePathTextBox.Text))
            {
                FilePathTextBox.Text = "My Notebook";
            }

            if (String.IsNullOrEmpty(CurrentDirectory))
            {
                CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            if (String.IsNullOrEmpty(CurrentExtension))
            {
                CurrentExtension = ".nbk";
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateFilePath(FilePath);
            FilePathTextBox.SelectAll();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (this.DialogResult == true)
            {
                if (String.IsNullOrWhiteSpace(FilePathTextBox.Text))
                {
                    MessageBox.Show("Please provide a name for your new notebook", "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (this.DialogResult != true)
            {
                return;
            }

            // Remove any illegal characters
            string filePath = FilePathTextBox.Text;
            foreach (var c in System.IO.Path.GetInvalidPathChars())
            {
                filePath = filePath.Replace(c, '_');
            }

            // Add directory/extension if needed
            string dirName = System.IO.Path.GetDirectoryName(filePath);
            if (String.IsNullOrEmpty(dirName))
            {
                filePath = System.IO.Path.Combine(CurrentDirectory, filePath) + CurrentExtension;
            }

            FilePath = filePath;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath;
            string directory = System.IO.Path.GetDirectoryName(FilePathTextBox.Text);
            if (!String.IsNullOrEmpty(directory))
            {
                // Treat typed text as file path
                filePath = FilePathTextBox.Text;
            }
            else
            {
                // Treat typed text as file name
                filePath = System.IO.Path.Combine(CurrentDirectory, FilePathTextBox.Text) + CurrentExtension;
                directory = CurrentDirectory;
            }

            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.InitialDirectory = directory;
            dialog.FileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            dialog.Filter = "Notebooks (*.nbk)|*.nbk";
            dialog.DereferenceLinks = true;

            if (dialog.ShowDialog() == true)
            {
                UpdateFilePath(dialog.FileName);
                FilePathTextBox.Focus();
                FilePathTextBox.SelectAll();
            }
        }

        #endregion
    }
}
