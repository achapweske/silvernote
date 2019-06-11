/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using DOM;
using DOM.HTML;
using FileFormats.OpenXML;
using FileFormats.PlainText;
using FileFormats.RTF;
using SilverNote.Common;
using SilverNote.Dialogs;
using SilverNote.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote
{
    /// <summary>
    /// A collection of utilities for saving a note to a file of the given format
    /// </summary>
    public static class ExportManager
    {
        /// <summary>
        /// Save the given note as a DOCX file
        /// </summary>
        /// <param name="note">the note to be saved</param>
        /// <param name="filePath">target filepath (if null, the user will be prompted)</param>
        /// <returns>true if the note was successfully saved, otherwise false</returns>
        public static bool SaveAsDOCX(NoteEditor note, string filePath = null)
        {
            // If no filepath specified, prompt the user for one
            if (String.IsNullOrEmpty(filePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = DefaultFilenameForNote(note, ".docx");
                dialog.DefaultExt = ".docx";
                dialog.Filter = "Word documents (.docx)|*.docx";
                if (dialog.ShowDialog() != true)
                {
                    return false;
                }
                filePath = dialog.FileName;
            }

            // Get the note as an HTMLDocument object
            var document = note.ExportDocument();

            // Transform the HTMLDocument to something appropriate for exporting
            HTMLFilters.ExportFilter(document);

            // Get the embedded resources
            var files = note.ExportResources();

            try
            {
                // Now actually save the HTMLDocument as an OpenXML file
                OpenXMLDocument.Save(filePath, document, files);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save the given note as an RTF file
        /// </summary>
        /// <param name="note">the note to be saved</param>
        /// <param name="filePath">target filepath (if null, the user will be prompted)</param>
        /// <returns>true if the note was successfully saved, otherwise false</returns>
        public static bool SaveAsRTF(NoteEditor note, string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = DefaultFilenameForNote(note, ".rtf");
                dialog.DefaultExt = ".rtf";
                dialog.Filter = "Rich text format (.rtf)|*.rtf";
                if (dialog.ShowDialog() != true)
                {
                    return false;
                }
                filePath = dialog.FileName;
            }

            var document = note.ExportDocument();
            HTMLFilters.ExportFilter(document);

            try
            {
                string text = RTFDocument.FromHTML(document);
                File.WriteAllText(filePath, text);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save the given note as a TXT file
        /// </summary>
        /// <param name="note">the note to be saved</param>
        /// <param name="filePath">target filepath (if null, the user will be prompted)</param>
        /// <returns>true if the note was successfully saved, otherwise false</returns>
        public static bool SaveAsTXT(NoteEditor note, string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = DefaultFilenameForNote(note, ".txt");
                dialog.DefaultExt = ".txt";
                dialog.Filter = "Plain text (.txt)|*.txt";
                if (dialog.ShowDialog() != true)
                {
                    return false;
                }
                filePath = dialog.FileName;
            }

            var document = note.ExportDocument();
            HTMLFilters.ExportFilter(document);

            try
            {
                string text = TextDocument.FromHTML(document);
                File.WriteAllText(filePath, text);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Save the given note as an HTML file
        /// </summary>
        /// <param name="note">the note to be saved</param>
        /// <param name="filePath">target filepath (if null, the user will be prompted)</param>
        /// <returns>true if the note was successfully saved, otherwise false</returns>
        public static bool SaveAsHTML(NoteEditor note, string filePath = null)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                var dialog = new Microsoft.Win32.SaveFileDialog();
                dialog.FileName = DefaultFilenameForNote(note, ".html");
                dialog.DefaultExt = ".html";
                dialog.Filter = "HTML documents (.html)|*.html";
                if (dialog.ShowDialog() != true)
                {
                    return false;
                }
                filePath = dialog.FileName;
            }

            // Convert to HTML

            string html = note.ToHTML(HTMLFilters.ExportFilter);

            // Create a directory to save embedded content

            // The directory will have the same name as the given file 
            // (without the file extension)

            string relativeDirPath = Path.GetFileNameWithoutExtension(filePath);
            string absoluteDirPath = Path.GetDirectoryName(filePath);
            absoluteDirPath = Path.Combine(absoluteDirPath, relativeDirPath);

            if (!Directory.Exists(absoluteDirPath))
            {
                Directory.CreateDirectory(absoluteDirPath);
            }

            // Save all embedded content to the directory

            var resources = note.ExportResources();
            foreach (var resource in resources)
            {
                string resourcePath = Path.Combine(absoluteDirPath, resource.Key);
                using (var stream = new FileStream(resourcePath, FileMode.Create))
                {
                    stream.Write(resource.Value, 0, resource.Value.Length);
                }
            }

            // Update URLs within the HTML

            html = SetRootPath(html, relativeDirPath);

            try
            {
                File.WriteAllText(filePath, html, Encoding.UTF8);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        #region Implementation

        /// <summary>
        /// Update all URLs referenced in the given document to be
        /// relative to the given directory path
        /// </summary>
        /// <param name="html">HTML to be converted</param>
        /// <param name="dirPath">root directory</param>
        /// <returns>converted HTML</returns>
        private static string SetRootPath(string html, string dirPath)
        {
            Uri uri;
            if (Uri.TryCreate(dirPath, UriKind.Absolute, out uri))
            {
                return SetRootUri(html, uri);
            }

            dirPath = dirPath.Replace("\\", "/");

            if (Uri.TryCreate(dirPath, UriKind.Relative, out uri))
            {
                return SetRootUri(html, uri);
            }

            return html;
        }

        public static string SetRootUri(string html, Uri rootURI)
        {
            var document = DOMFactory.CreateHTMLDocument();

            document.Open();
            document.Write(html);
            document.Close();

            SetRootUri(document, rootURI);

            return document.ToString();
        }

        public static void SetRootUri(HTMLDocument document, Uri rootURI)
        {
            // Update hyperlinks

            var hyperlinks = document.QuerySelectorAll(@"a[href]");
            if (hyperlinks != null)
            {
                foreach (HTMLElement hyperlink in hyperlinks)
                {
                    string uri = hyperlink.GetAttribute("href");
                    uri = UpdateUri(uri, rootURI);
                    hyperlink.SetAttribute("href", uri);
                }
            }

            // Update images

            var images = document.QuerySelectorAll(@"img[src]");
            if (images != null)
            {
                foreach (HTMLElement image in images)
                {
                    string uri = image.GetAttribute("src");
                    uri = UpdateUri(uri, rootURI);
                    image.SetAttribute("src", uri);
                }
            }
        }

        /// <summary>
        /// Prepend a URI with the given root URI.
        /// 
        /// Absolute URIs are returned unmodified
        /// </summary>
        /// <param name="uri">URI to be updated</param>
        /// <param name="rootURI">new root URI</param>
        /// <returns>updated URI</returns>
        private static string UpdateUri(string uri, Uri rootURI)
        {
            if (uri.Contains("://"))
            {
                return uri;
            }

            try
            {
                var result = new Uri(rootURI, uri);

                return result.ToString();
            }
            catch
            {
                string rootString = rootURI.ToString();

                rootString = Uri.EscapeUriString(rootString);

                rootString = rootString.TrimEnd('/');

                return rootString + "/" + uri;
            }
        }

        private static string DefaultFilenameForNote(NoteEditor note, string extension)
        {
            string filename = note.Title;
            if (String.IsNullOrEmpty(filename))
            {
                filename = "Untitled";
            }
            return filename + extension;
        }

        #endregion
    }
}
