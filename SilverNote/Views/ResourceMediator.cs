/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Editor;
using SilverNote.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using DOM;
using System.Diagnostics;
using SilverNote.ViewModels;
using SilverNote.Client;

namespace SilverNote.Views
{
    public class ResourceMediator
    {
        #region Fields

        NoteEditor _Editor;

        #endregion 

        #region Constructors

        public ResourceMediator(NoteEditor editor)
        {
            _Editor = editor;

            editor.ResourceChanged += OnResourceChanged;
            editor.ResourceRequested += OnResourceRequested;
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

        #region Implementation

        private void OnResourceRequested(object sender, ResourceEventArgs e)
        {        
            string url = Editor.OwnerDocument.ResolveURL(e.Url);

            // Automatically translate URLs for the form:
            // 
            //    /notes/<note-id>/<file-id>
            //
            // to the form:
            //
            //    /notes/<note-id>/files/<file-id>
            //

            string baseURL = Context.Notebook.Url.TrimEnd('/') + "/notes";
            if (url.StartsWith(baseURL) && !url.Contains("/files/"))
            {
                string filePath = url.Substring(baseURL.Length);
                url = Context.Url.TrimEnd('/') + "/files/" + filePath.TrimStart('/');
            }

            // Get a client capable of handling requests to the given URL

            NoteClient client;
            try
            {
                client = NoteClient.Instance(url);
            }
            catch (UriFormatException)
            {
                // This can happen if the URI is too long, as may occur
                // if the user pastes an HTML image with embedded data.
                // We should support this, which will require replacing
                // Microsoft's Uri class with something more forgiving.
                // For now, we'll handle the error silently.
                client = null;
            }

            if (client == null)
            {
                return;
            }

            // Now request the file and asynchronously read the response

            client.BeginGet(url, (asyncResult) =>
            {
                try
                {
                    var response = client.EndGet(asyncResult);
                    var data = response.ContentObject as byte[];
                    OnGetResourceSucceeded(e.Url, data);
                }
                catch (Exception ex)
                {
                    OnGetResourceFailed(ex.Message);
                    client.Resume();
                }
            }, null);
        }

        protected void OnGetResourceSucceeded(string url, byte[] data)
        {
            Editor.SetResourceData(url, data);
        }

        protected void OnGetResourceFailed(string message)
        {
            Debug.Assert(false, message);
        }

        private void OnResourceChanged(object sender, RoutedEventArgs e)
        {
            if (Context == null) return;    // sanity check

            NoteModel note = Context.Model;

            // Get all internal resource names (i.e., not those starting with http:, etc.)
            var resourceNames = 
                Editor.ResourceNames
                .Where(name => !name.Contains(':'))
                .ToArray();
            
            // Update each file's data
            var files = new List<FileModel>();
            foreach (var resourceName in resourceNames)
            {
                var data = Editor.GetResourceData(resourceName);
                if (data == null) continue;     // sanity check
                string fileName = Uri.UnescapeDataString(resourceName);
                FileModel file = note.GetFile(fileName);
                // If data empty, it means the file has not yet been loaded
                if (data.Length > 0) file.Data = data;
                files.Add(file);
            }

            // Update the list of files
            note.Files = files;
        }

        #endregion
    }
}
