/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Client;
using SilverNote.Clipart;
using SilverNote.Data.Models;
using SilverNote.Data.Store;
using SilverNote.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows;

namespace SilverNote.Models
{
    /// <summary>
    /// NoteClient lifecycle manager
    /// </summary>
    public class ClientManager : IDisposable
    {
        #region Fields

        readonly RepositoryModel _Repository;
        NoteClient _Client;

        #endregion

        #region Constructors

        public ClientManager(RepositoryModel repository)
        {
            _Repository = repository;
        }

        #endregion

        #region Properties

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public NoteClient Client
        {
            get
            {
                return _Client;
            }
            set
            {
                if (value != _Client)
                {
                    var oldValue = _Client;
                    if (_Client != null) OnClientReset(_Client);
                    _Client = value;
                    if (_Client != null) OnClientSet(_Client);
                    RaiseClientChanged(oldValue);
                }
            }
        }

        public bool IsOpen
        {
            get { return Client != null && Client.IsStarted; }
        }

        public bool CanChangePassword
        {
            get { return Client.Server != null && Client.Server.Store is NoteDbStore; }
        }

        #endregion

        #region Methods

        public void Open(RepositoryLocationModel login, bool autoCreate = true)
        {
            Open(login.Uri, login.Username, login.Password, autoCreate);
        }

        public void Open(string uri, string username = null, SecureString password = null, bool autoCreate = true)
        {
            Open(new Uri(uri), username, password, autoCreate);
        }

        public void Open(Uri uri, string username = null, SecureString password = null, bool autoCreate = true)
        {
            Client = new NoteClient(uri, username, password, autoCreate);
        }

        public void Close()
        {
            if (Client != null)
            {
                Client.Close();
            }
        }

        #endregion

        #region Events

        public event EventHandler<NoteClientChangedEventArgs> ClientChanged;

        protected void RaiseClientChanged(NoteClient oldClient)
        {
            if (ClientChanged != null)
            {
                ClientChanged(this, new NoteClientChangedEventArgs(oldClient));
            }
        }

        #endregion

        #region Implementation

        void OnClientSet(NoteClient client)
        {
            client.GetRepositorySucceeded += Repository_Updated;
            client.GetSelectedNotebookSucceeded += Repository_Updated;
            client.GetPasswordStatusSucceeded += Repository_Updated;
            client.GetNotebooksSucceeded += Repository_Updated;
            client.GetNotebookSucceeded += Notebook_Updated;
            client.GetNotebookNameSucceeded += Notebook_Updated;
            client.GetOpenNotesSucceeded += Notebook_Updated;
            client.GetSelectedNoteSucceeded += Notebook_Updated;
            client.GetNoteSucceeded += Note_Updated;
            client.GetNoteTitleSucceeded += Note_Updated;
            client.GetNoteContentSucceeded += Note_Updated;
            client.GetNoteCreatedAtSucceeded += Note_Updated;
            client.GetNoteModifiedAtSucceeded += Note_Updated;
            client.GetNoteViewedAtSucceeded += Note_Updated;
            client.GetNoteCategoriesSucceeded += Note_Updated;
            client.GetFileSucceeded += File_Updated;
            client.UpdateCategoriesSucceeded += Notebook_Updated;
            client.GetCategoriesSucceeded += Notebook_Updated;
            client.GetCategorySucceeded += Category_Updated;
            client.GetCategoryParentSucceeded += Category_Updated;
            client.GetCategoryChildrenSucceeded += Category_Updated;
            client.GetCategoryNameSucceeded += Category_Updated;
            client.GetClipartGroupsSucceeded += Repository_Updated;
            client.GetClipartGroupNameSucceeded += ClipartGroup_Updated;
            client.GetClipartItemsSucceeded += ClipartGroup_Updated;
            client.GetClipartNameSucceeded += Clipart_Updated;
            client.GetClipartDataSucceeded += Clipart_Updated;

            client.DefaultErrorHandler += Client_ErrorHandler;
            client.GetClipartItemsFailed += Client_SilentErrorHandler;
            client.GetSelectedNotebookFailed += Client_SilentErrorHandler;
            client.SetSelectedNotebookFailed += Client_SilentErrorHandler;
            client.GetOpenNotesFailed += Client_SilentErrorHandler;
            client.OpenNoteFailed += Client_SilentErrorHandler;
            client.CloseNoteFailed += Client_SilentErrorHandler;
            client.GetSelectedNoteFailed += Client_SilentErrorHandler;
            client.SetSelectedNoteFailed += Client_SilentErrorHandler;

            client.CreateNoteSucceeded += Client_CreateNoteSucceeded;
            client.CreateNoteFailed += Client_CreateNoteFailed;
        }

        void OnClientReset(NoteClient client)
        {
            client.GetRepositorySucceeded -= Repository_Updated;
            client.GetSelectedNotebookSucceeded -= Repository_Updated;
            client.GetPasswordStatusSucceeded -= Repository_Updated;
            client.GetNotebooksSucceeded -= Repository_Updated;
            client.GetNotebookSucceeded -= Notebook_Updated;
            client.GetNotebookNameSucceeded -= Notebook_Updated;
            client.GetOpenNotesSucceeded -= Notebook_Updated;
            client.GetSelectedNoteSucceeded -= Notebook_Updated;
            client.GetNoteSucceeded -= Note_Updated;
            client.GetNoteTitleSucceeded -= Note_Updated;
            client.GetNoteContentSucceeded -= Note_Updated;
            client.GetNoteCreatedAtSucceeded -= Note_Updated;
            client.GetNoteModifiedAtSucceeded -= Note_Updated;
            client.GetNoteViewedAtSucceeded -= Note_Updated;
            client.GetNoteCategoriesSucceeded -= Note_Updated;
            client.GetFileSucceeded -= File_Updated;
            client.UpdateCategoriesSucceeded -= Notebook_Updated;
            client.GetCategoriesSucceeded -= Notebook_Updated;
            client.GetCategorySucceeded -= Category_Updated;
            client.GetCategoryParentSucceeded -= Category_Updated;
            client.GetCategoryChildrenSucceeded -= Category_Updated;
            client.GetCategoryNameSucceeded -= Category_Updated;
            client.GetClipartGroupsSucceeded -= Repository_Updated;
            client.GetClipartGroupNameSucceeded -= ClipartGroup_Updated;
            client.GetClipartItemsSucceeded -= ClipartGroup_Updated;
            client.GetClipartNameSucceeded -= Clipart_Updated;
            client.GetClipartDataSucceeded -= Clipart_Updated;

            client.DefaultErrorHandler -= Client_ErrorHandler;
            client.GetClipartItemsFailed -= Client_SilentErrorHandler;
            client.GetSelectedNotebookFailed -= Client_SilentErrorHandler;
            client.SetSelectedNotebookFailed -= Client_SilentErrorHandler;
            client.GetOpenNotesFailed -= Client_SilentErrorHandler;
            client.OpenNoteFailed -= Client_SilentErrorHandler;
            client.CloseNoteFailed -= Client_SilentErrorHandler;
            client.GetSelectedNoteFailed -= Client_SilentErrorHandler;
            client.SetSelectedNoteFailed -= Client_SilentErrorHandler;

            client.CreateNoteSucceeded -= Client_CreateNoteSucceeded;
            client.CreateNoteFailed -= Client_CreateNoteFailed;
        }

        void Repository_Updated(object sender, RepositoryDataEventArgs e)
        {
            Repository.Update(e.Repository);
        }

        void Notebook_Updated(object sender, NotebookDataEventArgs e)
        {
            Repository.UpdateNotebook(e.Notebook);
        }

        void Note_Updated(object sender, NoteDataEventArgs e)
        {
            var notebook = Repository.GetNotebook(e.Note.NotebookID);
            notebook.UpdateNote(e.Note);
        }

        void File_Updated(object sender, FileDataEventArgs e)
        {
            Repository.UpdateFile(e.File);
        }

        void Category_Updated(object sender, CategoryDataEventArgs e)
        {
            Repository.UpdateCategory(e.Category);
        }

        void ClipartGroup_Updated(object sender, ClipartGroupDataEventArgs e)
        {
            Repository.UpdateClipartGroup(e.ClipartGroup);
        }

        void Clipart_Updated(object sender, ClipartDataEventArgs e)
        {
            var group = Repository.GetClipartGroup(e.Clipart.GroupID);
            group.UpdateClipart(e.Clipart);
        }

        void Client_ErrorHandler(object sender, NoteClientErrorEventArgs e)
        {
            NoteClient client = (NoteClient)sender;

            MessageBox.Show(e.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);

            client.Resume();
        }

        void Client_SilentErrorHandler(object sender, NoteClientErrorEventArgs e)
        {
            var client = (NoteClient)sender;
            client.Resume();
        }

        void Client_CreateNoteSucceeded(object sender, NoteDataEventArgs e)
        {
        }

        void Client_CreateNoteFailed(object sender, NoteClientErrorEventArgs e)
        {
            var noteData = (NoteDataModel)e.Request.ContentObject;

            var notebook = Repository.GetNotebook(noteData.NotebookID);
            var note = notebook.GetNote(noteData.ID);
            notebook.DeleteNote(note, false);

            Client_ErrorHandler(sender, e);
        }

        #endregion

        #region IDisposable

        private bool _IsDisposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (Client != null)
                {
                    Client.Dispose();
                }
            }

            _IsDisposed = true;
        }

        #endregion
    }
}
