/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Client;
using SilverNote.Common;
using SilverNote.Editor;
using SilverNote.Models;
using SilverNote.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace SilverNote.Views
{
    public class HyperlinkMediator
    {
        #region Fields

        NoteEditor _Editor;

        #endregion 

        #region Constructors

        public HyperlinkMediator(NoteEditor editor)
        {
            _Editor = editor;

            TextParagraph.AddHyperlinkClickedHandler(editor, Hyperlink_Clicked);
        }

        #endregion

        #region Properties

        public NoteEditor Editor
        {
            get { return _Editor; }
        }

        public NoteViewModel Context
        {
            get;
            set;
        }

        #endregion

        #region Operations

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

        public static string UriFromNoteID(Int64 noteID)
        {
            return String.Format("/notes/{0}", noteID);
        }

        public static Int64 UriToNoteID(string uri)
        {
            string[] segments = UriHelper.PathSegments(uri);
            if (segments.FirstOrDefault() != "notes")
            {
                return -1;
            }

            Int64 noteID;
            if (segments.Length >= 2 && Int64.TryParse(segments[1], out noteID))
            {
                return noteID;
            }
            else
            {
                return -1;
            }
        }

        public static string UriFromFilePath(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
            {
                return String.Empty;
            }

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

        #endregion

        #region Implementation

        protected void Hyperlink_Clicked(object sender, HyperlinkClickedEventArgs e)
        {
            string uriString = e.Uri;
            if (uriString.StartsWith("file://relative"))
            {
                // Handle relative file URIs

                var location = ((App)App.Current).NotebookManager.GetLocation(Context.Notebook.Model);
                if (location != null)
                {
                    string rootPath = location.Repository.Uri;
                    rootPath = rootPath.Replace("sqlite://", "file://");
                    rootPath = rootPath.Remove(rootPath.LastIndexOf('/'));
                    uriString = rootPath + uriString.Remove(0, "file://relative".Length);
                }            
            }

            Uri uri;
            if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri) || uri.IsAbsoluteUri)
            {
                // External request

                if (uri.IsFile)
                {
                    uriString = Path.GetFullPath(uri.LocalPath);
                }

                try
                {
                    System.Diagnostics.Process.Start(uriString);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                e.Handled = true;
                return;
            }

            Int64 noteID = HyperlinkMediator.UriToNoteID(uriString);
            if (noteID != -1)
            {
                // Fetch note by ID
                Context.Notebook.Model.Source.Client.BeginGetNote(
                    Context.Notebook.ID,
                    noteID,
                    Hyperlink_GetNoteCompleted,
                    e
                );

                e.Handled = true;
                return;
            }

            string title = HyperlinkMediator.UriToNoteTitle(uriString);
            if (!String.IsNullOrEmpty(title))
            {
                // Fetch note by title
                Context.Notebook.Model.Source.Client.BeginFindNotes(
                    Context.Notebook.ID,
                    String.Format("title:\"{0}\"", title),
                    default(DateTime), default(DateTime),
                    default(DateTime), default(DateTime),
                    default(DateTime), default(DateTime),
                    null, null,
                    0, 1, false,
                    Hyperlink_FindNoteCompleted,
                    e
                );

                e.Handled = true;
                return;
            }
        }

        protected void Hyperlink_GetNoteCompleted(IAsyncResult result)
        {
            var args = (HyperlinkClickedEventArgs)result.AsyncState;
            SilverNote.Data.Models.NoteDataModel noteData;

            try
            {
                noteData = Context.Notebook.Model.Source.Client.EndGetNote(result);
            }
            catch (NoteClientException e)
            {
                Hyperlink_GetNoteFailed(e);
                Context.Notebook.Model.Source.Client.Resume();
                return;
            }

            // Open the retrieved note
            var note = Context.Notebook.Model.GetNote(noteData.ID);
            OpenNote(note, args.Target);
            Context.Notebook.Model.SelectedNote = note;
        }

        protected void Hyperlink_GetNoteFailed(NoteClientException e)
        {
            string message = e.Message;

            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                message = "The note you requested has been deleted.";
            }

            MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected void Hyperlink_FindNoteCompleted(IAsyncResult result)
        {
            var args = (HyperlinkClickedEventArgs)result.AsyncState;
            SilverNote.Data.Models.SearchResultsDataModel searchResults;

            try
            {
                searchResults = Context.Notebook.Model.Source.Client.EndFindNotes(result);
            }
            catch (NoteClientException)
            {
                Context.Notebook.Model.Source.Client.Resume();
                return;
            }

            // Get the fetched note, or create a new one if not found

            Models.NoteModel note;
            string title = HyperlinkMediator.UriToNoteTitle("notes?" + searchResults.SearchString);
            if (searchResults.Results.Length > 0)
            {
                note = Context.Notebook.Model.GetNote(searchResults.Results[0].Note.ID);
            }
            else
            {
                note = Context.Notebook.Model.CreateNote(title);
            }

            // Update hyperlinks to point to the note by ID rather than title

            string actualURI = HyperlinkMediator.UriFromNoteID(note.ID);
            Editor.ChangeProperty(TextProperties.HyperlinkURLProperty, args.Uri, actualURI);

            // Open the retrieved note
            OpenNote(note, args.Target);
            Context.Notebook.Model.SelectedNote = note;
        }

        private void OpenNote(NoteModel note, string target)
        {
            if (target == "_self")
            {
                int index = Context.Notebook.OpenNotes.IndexOf(Context);
                Context.Notebook.CloseNote(Context);
                Context.Notebook.Model.OpenNote(note, index);
            }
            else
            {
                Context.Notebook.Model.OpenNote(note);
            }
        }

        #endregion
    }
}
