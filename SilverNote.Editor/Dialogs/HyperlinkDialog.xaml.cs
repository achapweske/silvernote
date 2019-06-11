/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;

namespace SilverNote.Dialogs
{
    /// <summary>
    /// Interaction logic for HyperlinkDialog.xaml
    /// </summary>
    public partial class HyperlinkDialog : Window
    {
        public HyperlinkDialog()
        {
            InitializeComponent();
        }

        public string HyperlinkText { get; set; }

        public string HyperlinkURL { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextTextBox.Focus();

            if (!String.IsNullOrEmpty(HyperlinkText))
            {
                if (HyperlinkText.Contains("://"))
                {
                    URLText = HyperlinkText;
                }
                else if (HyperlinkText.StartsWith("C:\\"))
                {
                    FileText = HyperlinkText;
                }

                NoteText = HyperlinkText;
            }

            if (!String.IsNullOrEmpty(HyperlinkURL))
            {
                Uri uri;
                if (Uri.TryCreate(HyperlinkURL, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (!uri.IsAbsoluteUri)
                    {
                        NoteText = UriToNoteTitle(uri.ToString());
                        NoteRadioButton.IsChecked = true;
                    }
                    else if (uri.IsAbsoluteUri && uri.IsFile)
                    {
                        FileText = UriToFilePath(HyperlinkURL);
                        FileRadioButton.IsChecked = true;
                    }
                    else
                    {
                        URLText = HyperlinkURL;
                        URLRadioButton.IsChecked = true;
                    }
                }
            }

            if (URLRadioButton.IsChecked == true)
            {
                URLTextBox.Text = URLText;
                URLTextBox.Focus();
            }
            else if (FileRadioButton.IsChecked == true)
            {
                FileTextBox.Text = FileText;
                FileTextBox.Focus();
            }
            else if (NoteRadioButton.IsChecked == true)
            {
                NoteTextBox.Text = NoteText;
                NoteTextBox.Focus();
            }
            else
            {
                URLRadioButton.IsChecked = true;
                URLTextBox.Text = URLText;
                URLTextBox.Focus();
            }
        }

        public static string UriToFilePath(string uri)
        {
            string filePath = HttpUtility.UrlDecode(uri);
            filePath = filePath.Replace("file://", "");
            if (filePath.StartsWith("relative/"))
            {
                filePath = filePath.Remove(0, "relative/".Length);
            }
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);
            return filePath;
        }

        public static string UriToNoteTitle(string uri)
        {
            string[] segments = UriHelper.PathSegments(uri);
            if (segments.FirstOrDefault() != "notes")
            {
                return String.Empty;
            }

            string pattern = @"title:([^&#]*)";
            var match = Regex.Match(uri, pattern);
            if (match != null)
            {
                string title = match.Groups[1].Value;
                title = title.Trim('\"');
                return HttpUtility.UrlDecode(title);
            }
            else
            {
                return String.Empty;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult == true)
            {
                if (URLRadioButton.IsChecked == true)
                {
                    string hyperlinkURL = UrlToUri(URLTextBox.Text.Trim());

                    if (hyperlinkURL == null)
                    {
                        string message = "Invalid URL";
                        MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        return;
                    }

                    HyperlinkURL = hyperlinkURL;
                }
                else if (FileRadioButton.IsChecked == true)
                {
                    string hyperlinkURL = UriFromFilePath(FileTextBox.Text.Trim());

                    if (hyperlinkURL == null)
                    {
                        string message = "Invalid file path";
                        MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        return;
                    }

                    HyperlinkURL = hyperlinkURL;
                }
                else if (NoteRadioButton.IsChecked == true)
                {
                    string hyperlinkURL = UriFromNoteTitle(NoteTextBox.Text.Trim());

                    if (hyperlinkURL == null)
                    {
                        string message = "Invalid note title";
                        MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                        return;
                    }

                    HyperlinkURL = hyperlinkURL;
                }
            }

        }

        public static string UriFromFilePath(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return String.Empty;
            }

            // Absolute path

            if (filePath.Contains(":\\"))
            {
                Uri uri;
                if (Uri.TryCreate(filePath, UriKind.Absolute, out uri))
                {
                    return uri.ToString();
                }
                else
                {
                    return null;
                }
            }

            // Relative path

            else
            {
                string uri = filePath.Replace(Path.DirectorySeparatorChar, '/');
                uri = Uri.EscapeUriString(uri);
                uri = "file://relative/" + uri.TrimStart("/".ToCharArray());
                return uri;
            }
        }

        private static string UrlToUri(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return String.Empty;
            }

            if (!url.Contains("://"))
            {
                url = "http://" + url;
            }

            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                return uri.ToString();
            }
            else
            {
                return null;
            }
        }

        public static string UriFromNoteTitle(string noteTitle)
        {
            if (!String.IsNullOrEmpty(noteTitle))
            {
                return String.Format("/notes?search=title:{0}", HttpUtility.UrlEncode(noteTitle));
            }
            else
            {
                return String.Empty;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browseDialog = new Microsoft.Win32.OpenFileDialog();
            browseDialog.Filter = "All Files|*.*";
            if (browseDialog.ShowDialog() != true)
            {
                return;
            }

            FileTextBox.Text = browseDialog.FileName;
        }

        private string URLText { get; set; }

        private void URLRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            URLTextBox.Text = URLText;
            URLTextBox.SelectAll();
            URLTextBox.Focus();
        }

        private void URLRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            URLText = URLTextBox.Text;
            URLTextBox.Clear();
        }

        private string FileText { get; set; }

        private void FileRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FileTextBox.Text = FileText;
            FileTextBox.SelectAll();
            FileTextBox.Focus();
        }

        private void FileRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            FileText = FileTextBox.Text;
            FileTextBox.Clear();
        }

        private string NoteText { get; set; }

        private void NoteRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            NoteTextBox.Text = NoteText;
            NoteTextBox.SelectAll();
            NoteTextBox.Focus();
        }

        private void NoteRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            NoteText = NoteTextBox.Text;
            NoteTextBox.Clear();
        }
    }
}
