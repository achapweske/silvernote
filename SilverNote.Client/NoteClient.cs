/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Reflection;
using System.Security;
using System.Diagnostics;
using SilverNote.Common;
using SilverNote.Data.Models;
using SilverNote.Server;
using System.ComponentModel;

namespace SilverNote.Client
{
    /// <summary>
    /// Client interface to the SilverNote web service or local NoteServer
    /// </summary>
    public class NoteClient : NoteClientThread, INotifyPropertyChanged
    {
        #region Fields

        static List<NoteClient> _Clients = new List<NoteClient>();
        bool _HasPassword;
        Exception _OpenException;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a client without connecting to a SilverNote repository
        /// 
        /// This is used for performing general purpose web requests
        /// </summary>
        public NoteClient()
        {
            Open();

            // Note: _Clients is NOT updated if Open() throws an exception
            // Therefore, _Clients only contains successfully initialized clients

            _Clients.Add(this);
        }

        /// <summary>
        /// Create a client and connect to a SilverNote repository
        /// </summary>
        /// <param name="uri">URL of a SilverNote repository</param>
        /// <param name="username">Username to apply to requests</param>
        /// <param name="password">Password to apply to requests</param>
        /// <param name="autoCreate">true to automatically create a repository if one does not already exist at the given URL</param>
        public NoteClient(Uri uri, string username, SecureString password, bool autoCreate)
        {
            Open(uri, username, password, autoCreate);

            // Note: _Clients is NOT updated if Open() throws an exception
            // Therefore, _Clients only contains successfully initialized clients

            _Clients.Add(this);
        }

        /// <summary>
        /// Get a NoteClient for servicing the specified URL.
        /// 
        /// Note: this is not thread-safe and must be called from the application's main thread
        /// </summary>
        public static NoteClient Instance(Uri uri)
        {
            // First, search for a client associated with the given URL

            foreach (var client in _Clients)
            {
                if (client.Uri != null)
                {
                    var match = UriTemplate.MatchTemplate(client.Uri, uri);
                    if (match != null)
                    {
                        return client;
                    }
                }
            }

            // If none found, use the first client not associated with any URL

            foreach (var client in _Clients)
            {
                if (client.Uri == null)
                {
                    return client;
                }
            }

            // If no clients have been created that are suitable for servicing 
            // this URL, return null

            return null;
        }

        /// <summary>
        /// Overloaded
        /// </summary>
        public static NoteClient Instance(string uri)
        {
            return Instance(new Uri(uri));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the repository URI passed to the constructor
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Returns the repository username passed to the constructor
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Returns the repository password passed to the constructor
        /// 
        /// For security reasons this is only valid during initialization and is disposed thereafter
        /// </summary>
        public SecureString Password { get; private set; }

        /// <summary>
        /// Determine if the repository is password-protected
        /// </summary>
        public bool HasPassword
        {
            get
            {
                return _HasPassword;
            }
            protected set
            {
                if (value != _HasPassword)
                {
                    _HasPassword = value;
                    RaisePropertyChanged("HasPassword");
                }
            }
        }

        /// <summary>
        /// Returns the autoCreate flag passed to the constructor
        /// </summary>
        public bool AutoCreate { get; private set; }

        /// <summary>
        /// The NoteServer object to handle requests made by this client
        /// 
        /// All requests for locally-stored data are routed to an internal
        /// NoteServer object, which then parses the request and delegates
        /// I/O to the proper NoteStore object.
        /// 
        /// This is not used when remote data (on an external webserver)
        /// is requested.
        /// </summary>
        public NoteServer Server { get; set; }

        /// <summary>
        /// Synchronization context of the thread that created this client
        /// 
        /// This is automatically set during NoteClient initialization
        /// </summary>
        public SynchronizationContext Context { get; set; }

        #endregion

        #region Operations

        /// <summary>
        /// Initialize the underlying thread
        /// 
        /// This must be called prior to performing any requests
        /// </summary>
        public void Open()
        {
            Context = SynchronizationContext.Current;

            _OpenException = null;

            base.Start(null);

            if (_OpenException != null)
            {
                throw _OpenException;
            }
        }

        /// <summary>
        /// Initialize the underlying thread and connect to the given repository
        /// 
        /// This must be called prior to performing any requests
        /// </summary>
        /// <param name="uri">URI of the repository to connect to</param>
        /// <param name="username">Username used to logon to the repository</param>
        /// <param name="password">Password used to logon to the repository</param>
        /// <param name="autoCreate">true to automatically create a new repository if the one specified by uri does not exist</param>
        public void Open(Uri uri, string username, SecureString password, bool autoCreate)
        {
            Uri = uri;
            Username = username;
            Password = password;
            HasPassword = password != null && password.Length > 0;
            AutoCreate = autoCreate;

            Open();
        }

        /// <summary>
        /// Abort all pending requests and shutdown the underlying thread
        /// 
        /// This must be called prior to application exit
        /// </summary>
        public void Close()
        {
            base.Stop();
        }

        #endregion

        #region GET/PUT/POST

        #region GET

        /// <summary>
        /// Issue a GET request asynchronously
        /// </summary>
        public IAsyncResult BeginGet(string uri, AsyncCallback callback, object state)
        {
            return BeginGet(new Uri(uri), callback, state);
        }

        /// <summary>
        /// Issue a GET request asynchronously
        /// </summary>
        public IAsyncResult BeginGet(Uri uri, AsyncCallback callback, object state)
        {
            var request = new Request
            {
                Method = RequestMethod.Get,
                Uri = uri
            };

            return BeginRequest(request, callback, state);
        }

        /// <summary>
        /// Complete an asynchronous GET request
        /// 
        /// If an error occurred, this method will throw an exception
        /// </summary>
        public Response EndGet(IAsyncResult asyncResult)
        {
            return EndRequest(asyncResult);
        }

        #endregion

        #region PUT

        /// <summary>
        /// Issue a Put request asynchronously
        /// </summary>
        public IAsyncResult BeginPut(string uri, string contentType, string content, AsyncCallback callback, object state)
        {
            return BeginPut(new Uri(uri), contentType, content, callback, state);
        }

        /// <summary>
        /// Issue a Put request asynchronously
        /// </summary>
        public IAsyncResult BeginPut(Uri uri, string contentType, string content, AsyncCallback callback, object state)
        {
            var request = new Request
            {
                Method = RequestMethod.Put,
                Uri = uri,
                ContentType = contentType,
                ContentString = content
            };

            return BeginRequest(request, callback, state);
        }

        /// <summary>
        /// Complete an asynchronous Put request
        /// 
        /// If an error occurred, this method will throw an exception
        /// </summary>
        public Response EndPut(IAsyncResult asyncResult)
        {
            return EndRequest(asyncResult);
        }

        #endregion

        #region POST

        /// <summary>
        /// Issue a Post request asynchronously
        /// </summary>
        public IAsyncResult BeginPost(string uri, string contentType, string content, AsyncCallback callback, object state)
        {
            return BeginPost(new Uri(uri), contentType, content, callback, state);
        }

        /// <summary>
        /// Issue a Post request asynchronously
        /// </summary>
        public IAsyncResult BeginPost(Uri uri, string contentType, string content, AsyncCallback callback, object state)
        {
            var request = new Request
            {
                Method = RequestMethod.Post,
                Uri = uri,
                ContentType = contentType,
                ContentString = content
            };

            return BeginRequest(request, callback, state);
        }

        /// <summary>
        /// Complete an asynchronous Post request
        /// 
        /// If an error occurred, this method will throw an exception
        /// </summary>
        public Response EndPost(IAsyncResult asyncResult)
        {
            return EndRequest(asyncResult);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Issue a Delete request asynchronously
        /// </summary>
        public IAsyncResult BeginDelete(string uri, AsyncCallback callback, object state)
        {
            return BeginDelete(new Uri(uri), callback, state);
        }

        /// <summary>
        /// Issue a Delete request asynchronously
        /// </summary>
        public IAsyncResult BeginDelete(Uri uri, AsyncCallback callback, object state)
        {
            var request = new Request
            {
                Method = RequestMethod.Delete,
                Uri = uri
            };

            return BeginRequest(request, callback, state);
        }

        /// <summary>
        /// Complete an asynchronous Delete request
        /// 
        /// If an error occurred, this method will throw an exception
        /// </summary>
        public Response EndDelete(IAsyncResult asyncResult)
        {
            return EndRequest(asyncResult);
        }

        #endregion

        #endregion

        #region Repository

        #region URIs

        public Uri RepositoryUri()
        {
            return Uri;
        }

        public Uri SelectedNotebookUri()
        {
            return UriHelper.AppendPath(RepositoryUri(), "selected-notebook");
        }

        public Uri PasswordUri()
        {
            return UriHelper.AppendPath(RepositoryUri(), "password");
        }

        public static Uri SetPurge(Uri uri, bool purge)
        {
            if (purge)
            {
                return UriHelper.SetQueryParameter(uri, "purge", "true");
            }
            else
            {
                return uri;
            }
        }

        public static Uri SetDeleteMissingItems(Uri uri, bool deleteMissingItems)
        {
            if (deleteMissingItems)
            {
                return UriHelper.SetQueryParameter(uri, "deleteMissingItems", "true");
            }
            else
            {
                return uri;
            }
        }

        public static Uri SetUpdateParameters(Uri uri, bool deleteMissingItems, bool purge)
        {
            uri = SetDeleteMissingItems(uri, deleteMissingItems);
            uri = SetPurge(uri, purge);
            return uri;
        }

        #endregion

        // GET /
        #region GetRepository

        public event EventHandler<RepositoryDataEventArgs> GetRepositorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetRepositoryFailed;

        public Request GetRepository_Request()
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = RepositoryUri()
            };
        }

        public bool IsGetRepositoryPending { get; private set; }

        public void GetRepository()
        {
            if (!IsGetRepositoryPending)
            {
                IsGetRepositoryPending = true;

                Request request = GetRepository_Request();

                request.Completed += OnGetRepositoryResponse;

                Request(request);
            }
        }

        public void OnGetRepositoryResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetRepositorySucceeded, GetRepositoryFailed);

            IsGetRepositoryPending = false;
        }

        public IAsyncResult BeginGetRepository(AsyncCallback callback = null, object state = null)
        {
            Request request = GetRepository_Request();

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndGetRepository(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // PUT /
        #region UpdateRepository

        public event EventHandler<RepositoryDataEventArgs> UpdateRepositorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateRepositoryFailed;

        public Request UpdateRepository_Request(RepositoryDataModel repository = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = RepositoryUri(),
                ContentType = "text/xml",
                ContentObject = repository
            };
        }

        public void UpdateRepository(RepositoryDataModel repository)
        {
            UpdateRepository();

            Request request = UpdateRepository_Request(repository);

            request.Completed += OnUpdateRepositoryResponse;

            Request(request);
        }

        public void UpdateRepository()
        {
            Request request = UpdateRepository_Request();

            Cancel(request);
        }

        protected void OnUpdateRepositoryResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateRepositorySucceeded, UpdateRepositoryFailed);
        }

        #endregion

        // GET /selected-notebook
        #region GetSelectedNotebook

        public event EventHandler<RepositoryDataEventArgs> GetSelectedNotebookSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetSelectedNotebookFailed;

        public Request GetSelectedNotebook_Request()
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = SelectedNotebookUri(),
            };
        }

        public bool IsGetSelectedNotebookPending { get; private set; }

        public void GetSelectedNotebook()
        {
            if (!IsGetSelectedNotebookPending && !IsGetRepositoryPending)
            {
                Request request = GetSelectedNotebook_Request();

                IsGetSelectedNotebookPending = true;

                request.Completed += OnGetSelectedNotebookResponse;

                Request(request);
            }
        }

        public void OnGetSelectedNotebookResponse(Request request, Response response)
        {
            IsGetSelectedNotebookPending = false;

            DefaultResponseHandler(request, response, GetSelectedNotebookSucceeded, GetSelectedNotebookFailed);
        }

        #endregion

        // PUT /selected-notebook
        #region SetSelectedNotebook

        public event EventHandler<RepositoryDataEventArgs> SetSelectedNotebookSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetSelectedNotebookFailed;

        public Request SetSelectedNotebook_Request(Int64 notebookID = 0)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = SelectedNotebookUri(),
                ContentType = "text/xml",
                ContentObject = new RepositoryDataModel { SelectedNotebookID = notebookID }
            };
        }

        public void SetSelectedNotebook(Int64 notebookID)
        {
            CancelSetSelectedNotebook();

            Request request = SetSelectedNotebook_Request(notebookID);

            request.Completed += OnSetSelectedNotebookResponse;

            Request(request);
        }

        public bool IsSetSelectedNotebookPending()
        {
            Request request = SetSelectedNotebook_Request();

            return IsPending(request);
        }

        public void CancelSetSelectedNotebook()
        {
            Request request = SetSelectedNotebook_Request();

            Cancel(request);
        }

        public void OnSetSelectedNotebookResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetSelectedNotebookSucceeded, SetSelectedNotebookFailed);
        }

        #endregion

        // GET /password
        #region GetPasswordStatus

        public event EventHandler<RepositoryDataEventArgs> GetPasswordStatusSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetPasswordStatusFailed;

        public Request GetPasswordStatus_Request()
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = PasswordUri()
            };
        }

        public void GetPasswordStatus()
        {
            Request request = GetPasswordStatus_Request();

            request.Completed += OnGetPasswordStatusResponse;

            Request(request);
        }

        public void OnGetPasswordStatusResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetPasswordStatusSucceeded, GetPasswordStatusFailed);
        }

        public IAsyncResult BeginGetPasswordStatus(AsyncCallback callback = null, object state = null)
        {
            Request request = GetPasswordStatus_Request();

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndGetPasswordStatus(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // POST /password
        #region ChangePassword

        public event EventHandler<RepositoryDataEventArgs> ChangePasswordSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> ChangePasswordFailed;

        public Request ChangePassword_Request(SecureString oldPassword, SecureString newPassword)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = PasswordUri(),
                ContentType = "text/xml",
                ContentObject = new ChangedPasswordDataModel 
                { 
                    OldPassword = oldPassword, 
                    NewPassword = newPassword 
                }
            };
        }

        public void ChangePassword(SecureString oldPassword, SecureString newPassword)
        {
            Request request = ChangePassword_Request(oldPassword, newPassword);

            request.Completed += OnChangePasswordResponse;

            Request(request);
        }

        public void OnChangePasswordResponse(Request request, Response response)
        {
            if (response.IsSuccess)
            {
                HasPassword = response.ContentOrDefault<RepositoryDataModel>().HasPassword == true;
            }

            DefaultResponseHandler(request, response, ChangePasswordSucceeded, ChangePasswordFailed);
        }

        public IAsyncResult BeginChangePassword(SecureString oldPassword, SecureString newPassword, AsyncCallback callback = null, object state = null)
        {
            Request request = ChangePassword_Request(oldPassword, newPassword);

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndChangePassword(IAsyncResult asyncResult)
        {
            var content = EndRequest(asyncResult).ContentOrDefault<RepositoryDataModel>();

            HasPassword = content.HasPassword == true;

            return content;
        }

        #endregion

        #endregion

        #region Notebooks

        #region URIs

        public Uri NotebooksUri()
        {
            return UriHelper.AppendPath(RepositoryUri(), "notebooks");
        }

        public Uri NotebookUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotebooksUri(), notebookID.ToString());
        }

        public Uri NotebookNameUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotebookUri(notebookID), "name");
        }

        public Uri OpenNotesUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotebookUri(notebookID), "open-notes");
        }

        public Uri OpenNoteUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(OpenNotesUri(notebookID), noteID.ToString());
        }

        public Uri SelectedNoteUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotebookUri(notebookID), "selected-note");
        }

        #endregion

        // GET /notebooks
        #region GetNotebooks

        public event EventHandler<RepositoryDataEventArgs> GetNotebooksSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNotebooksFailed;

        public Request GetNotebooks_Request()
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NotebooksUri()
            };
        }

        public bool IsGetNotebooksPending { get; private set; }

        public void GetNotebooks()
        {
            if (!IsGetNotebooksPending && !IsGetRepositoryPending)
            {
                Request request = GetNotebooks_Request();

                IsGetNotebooksPending = true;

                request.Completed += OnGetNotebooksResponse;

                Request(request);
            }
        }

        protected void OnGetNotebooksResponse(Request request, Response response)
        {
            IsGetNotebooksPending = false;

            DefaultResponseHandler(request, response, GetNotebooksSucceeded, GetNotebooksFailed);
        }

        public IAsyncResult BeginGetNotebooks(AsyncCallback callback = null, object state = null)
        {
            Request request = GetNotebooks_Request();

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndGetNotebooks(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // PUT /notebooks
        #region SetNotebooks

        public event EventHandler<RepositoryDataEventArgs> SetNotebooksSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetNotebooksFailed;

        public Request SetNotebooks_Request(NotebookDataModel[] notebooks = null, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = SetPurge(NotebooksUri(), purge),
                ContentType = "text/xml",
                ContentObject = new RepositoryDataModel { Notebooks = notebooks }
            };
        }

        public void SetNotebooks(NotebookDataModel[] notebooks, bool purge = false)
        {
            Request request = SetNotebooks_Request(notebooks, purge);

            request.Completed += OnSetNotebooksResponse;

            Request(request);
        }

        protected void OnSetNotebooksResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetNotebooksSucceeded, SetNotebooksFailed);
        }

        public IAsyncResult BeginSetNotebooks(NotebookDataModel[] notebooks, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = SetNotebooks_Request(notebooks, purge);

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndSetNotebooks(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // PATCH /notebooks
        #region UpdateNotebooks

        public event EventHandler<RepositoryDataEventArgs> UpdateNotebooksSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateNotebooksFailed;

        public Request UpdateNotebooks_Request(NotebookDataModel[] notebooks = null, bool deleteMissingItems = false, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetUpdateParameters(NotebooksUri(), deleteMissingItems, purge),
                ContentType = "text/xml",
                ContentObject = new RepositoryDataModel { Notebooks = notebooks }
            };
        }

        public void UpdateNotebooks(NotebookDataModel[] notebooks, bool deleteMissingItems = false, bool purge = false)
        {
            Request request = UpdateNotebooks_Request(notebooks, deleteMissingItems, purge);

            request.Completed += OnUpdateNotebooksResponse;

            Request(request);
        }

        protected void OnUpdateNotebooksResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateNotebooksSucceeded, UpdateNotebooksFailed);
        }

        public IAsyncResult BeginUpdateNotebooks(NotebookDataModel[] notebooks, bool deleteMissingItems = false, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateNotebooks_Request(notebooks, deleteMissingItems, purge);

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndUpdateNotebooks(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // POST /notebooks
        #region CreateNotebook

        public event EventHandler<NotebookDataEventArgs> CreateNotebookSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CreateNotebookFailed;

        Request CreateNotebook_Request(Int64 notebookID, string name = null)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = NotebooksUri(),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel 
                {
                    ID = notebookID, 
                    Name = name 
                }
            };
        }

        public void CreateNotebook(Int64 notebookID, string name)
        {
            if (!IsCreateNotebookPending(notebookID))
            {
                Request request = CreateNotebook_Request(notebookID, name);

                request.Completed += OnCreateNotebookResponse;

                Request(request);
            }
        }

        public bool IsCreateNotebookPending(Int64 notebookID)
        {
            Request request = CreateNotebook_Request(notebookID);

            return IsPending(request);
        }

        public void CancelCreateNotebook(Int64 notebookID)
        {
            Request request = CreateNotebook_Request(notebookID);

            Cancel(request);
        }

        protected void OnCreateNotebookResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CreateNotebookSucceeded, CreateNotebookFailed);
        }

        #endregion

        // DELETE /notebooks/{id}
        #region DeleteNotebook

        public event EventHandler<RepositoryDataEventArgs> DeleteNotebookSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> DeleteNotebookFailed;

        Request DeleteNotebook_Request(Int64 notebookID, bool purge = false)
        {
            var request = new Request
            {
                Method = RequestMethod.Delete,
                Uri = NotebookUri(notebookID)
            };

            if (purge)
            {
                request.Params.Set("purge", "true");
            }

            return request;
        }

        public void DeleteNotebook(Int64 notebookID, bool purge)
        {
            Request request = DeleteNotebook_Request(notebookID, purge);

            request.Completed += OnDeleteNotebookResponse;

            Request(request);
        }

        public bool IsDeleteNotebookPending(Int64 notebookID)
        {
            Request request = DeleteNotebook_Request(notebookID);

            return IsPending(request);
        }

        public void CancelDeleteNotebook(Int64 notebookID)
        {
            Request request = DeleteNotebook_Request(notebookID);

            Cancel(request);
        }

        protected void OnDeleteNotebookResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, DeleteNotebookSucceeded, DeleteNotebookFailed);
        }

        #endregion

        // GET /notebooks/{id}
        #region GetNotebook

        public event EventHandler<NotebookDataEventArgs> GetNotebookSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNotebookFailed;

        Request GetNotebook_Request(Int64 notebookID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NotebookUri(notebookID)
            };
        }

        public void GetNotebook(Int64 notebookID)
        {
            if (!IsGetNotebookPending(notebookID))
            {
                Request request = GetNotebook_Request(notebookID);

                request.Completed += OnGetNotebookResponse;

                Request(request);
            }
        }

        public bool IsGetNotebookPending(Int64 notebookID)
        {
            Request request = GetNotebook_Request(notebookID);

            return IsPending(request);
        }

        public void CancelGetNotebook(Int64 notebookID)
        {
            Request request = GetNotebook_Request(notebookID);

            Cancel(request);
        }

        protected void OnGetNotebookResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNotebookSucceeded, GetNotebookFailed);
        }

        #endregion

        // PUT /notebooks/{id}
        #region UpdateNotebook

        public event EventHandler<NotebookDataEventArgs> UpdateNotebookSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateNotebookFailed;

        Request UpdateNotebook_Request(Int64 notebookID, NotebookDataModel notebook = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = NotebookUri(notebookID),
                ContentType = "text/xml",
                ContentObject = notebook
            };
        }

        public void UpdateNotebook(Int64 notebookID, NotebookDataModel notebook)
        {
            CancelUpdateNotebook(notebookID);

            Request request = UpdateNotebook_Request(notebookID, notebook);

            request.Completed += OnUpdateNotebookResponse;

            Request(request);
        }

        public void CancelUpdateNotebook(Int64 notebookID)
        {
            Request request = UpdateNotebook_Request(notebookID);

            Cancel(request);
        }

        protected void OnUpdateNotebookResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateNotebookSucceeded, UpdateNotebookFailed);
        }

        #endregion

        // GET /notebooks/{id}/name
        #region GetNotebookName

        public event EventHandler<NotebookDataEventArgs> GetNotebookNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNotebookNameFailed;

        Request GetNotebookName_Request(Int64 notebookID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NotebookNameUri(notebookID)
            };
        }

        public void GetNotebookName(Int64 notebookID)
        {
            if (!IsGetRepositoryPending &&
                !IsGetNotebookPending(notebookID) &&
                !IsGetNotebookNamePending(notebookID))
            {
                Request request = GetNotebookName_Request(notebookID);

                request.Completed += OnGetNotebookNameResponse;

                Request(request);
            }
        }

        public bool IsGetNotebookNamePending(Int64 notebookID)
        {
            Request request = GetNotebookName_Request(notebookID);

            return IsPending(request);
        }

        public void CancelGetNotebookName(Int64 notebookID)
        {
            Request request = GetNotebookName_Request(notebookID);

            Cancel(request);
        }

        public void OnGetNotebookNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNotebookNameSucceeded, GetNotebookNameFailed);
        }

        #endregion

        // PUT /notebooks/{id}/name
        #region SetNotebookName

        public event EventHandler<NotebookDataEventArgs> SetNotebookNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetNotebookNameFailed;

        Request SetNotebookName_Request(Int64 notebookID, string newName = "")
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = NotebookNameUri(notebookID),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel { Name = newName, NameModifiedAt = Now }
            };
        }

        public void SetNotebookName(Int64 notebookID, string newName)
        {
            CancelSetNotebookName(notebookID);

            Request request = SetNotebookName_Request(notebookID, newName);

            request.Completed += OnSetNotebookNameResponse;

            Request(request);
        }

        public bool IsSetNotebookNamePending(Int64 notebookID)
        {
            Request request = SetNotebookName_Request(notebookID);

            return IsPending(request);
        }

        public void CancelSetNotebookName(Int64 notebookID)
        {
            Request request = SetNotebookName_Request(notebookID);

            Cancel(request);
        }

        public void OnSetNotebookNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetNotebookNameSucceeded, SetNotebookNameFailed);
        }

        #endregion

        // GET /notebooks/{id}/selected-note
        #region GetSelectedNote

        public event EventHandler<NotebookDataEventArgs> GetSelectedNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetSelectedNoteFailed;

        Request GetSelectedNote_Request(Int64 notebookID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = SelectedNoteUri(notebookID)
            };
        }

        public void GetSelectedNote(Int64 notebookID)
        {
            if (!IsGetRepositoryPending &&
                !IsGetNotebookPending(notebookID) &&
                !IsGetSelectedNotePending(notebookID))
            {
                Request request = GetSelectedNote_Request(notebookID);

                request.Completed += OnGetSelectedNoteResponse;

                Request(request);
            }
        }

        public bool IsGetSelectedNotePending(Int64 notebookID)
        {
            Request request = GetSelectedNote_Request(notebookID);

            return IsPending(request);
        }

        public void OnGetSelectedNoteResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetSelectedNoteSucceeded, GetSelectedNoteFailed);
        }

        #endregion

        // PUT /notebooks/{id}/selected-note
        #region SetSelectedNote

        public event EventHandler<NotebookDataEventArgs> SetSelectedNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetSelectedNoteFailed;

        Request SetSelectedNote_Request(Int64 notebookID, Int64 noteID = 0)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = SelectedNoteUri(notebookID),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel { ID = notebookID, SelectedNoteID = noteID }
            };
        }

        public void SetSelectedNote(Int64 notebookID, Int64 noteID)
        {
            CancelSetSelectedNote(notebookID);

            Request request = SetSelectedNote_Request(notebookID, noteID);

            request.Completed += OnSetSelectedNoteResponse;

            Request(request);
        }

        public void CancelSetSelectedNote(Int64 notebookID)
        {
            Request request = SetSelectedNote_Request(notebookID);

            Cancel(request);
        }

        public void OnSetSelectedNoteResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetSelectedNoteSucceeded, SetSelectedNoteFailed);
        }

        #endregion

        // GET /notebooks/{id}/open-notes
        #region GetOpenNotes

        public event EventHandler<NotebookDataEventArgs> GetOpenNotesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetOpenNotesFailed;

        Request GetOpenNotes_Request(Int64 notebookID)
        {
            return new Request()
            {
                Method = RequestMethod.Get,
                Uri = OpenNotesUri(notebookID)
            };
        }

        public void GetOpenNotes(Int64 notebookID)
        {
            if (!IsGetRepositoryPending &&
                !IsGetNotebookPending(notebookID) && 
                !IsGetOpenNotesPending(notebookID))
            {
                Request request = GetOpenNotes_Request(notebookID);

                request.Completed += OnGetOpenNotesCompleted;

                Request(request);
            }
        }

        public bool IsGetOpenNotesPending(Int64 notebookID)
        {
            Request request = GetOpenNotes_Request(notebookID);

            return IsPending(request);
        }

        public void CancelOpenNotes(Int64 notebookID)
        {
            Request request = GetOpenNotes_Request(notebookID);

            Cancel(request);
        }

        public void OnGetOpenNotesCompleted(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetOpenNotesSucceeded, GetOpenNotesFailed);
        }

        #endregion

        // PUT /notebooks/{id}/open-notes
        #region SetOpenNotes

        public event EventHandler<NotebookDataEventArgs> SetOpenNotesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetOpenNotesFailed;

        Request SetOpenNotes_Request(Int64 notebookID, Int64[] openNotes = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = OpenNotesUri(notebookID),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel { OpenNotes = openNotes }
            };
        }

        public void SetOpenNotes(Int64 notebookID, Int64[] openNotes)
        {
            CancelSetOpenNotes(notebookID);

            Request request = SetOpenNotes_Request(notebookID, openNotes);

            request.Completed += OnSetOpenNotesResponse;

            Request(request);
        }

        public void CancelSetOpenNotes(Int64 notebookID)
        {
            Request request = SetOpenNotes_Request(notebookID);

            Cancel(request);
        }

        public void OnSetOpenNotesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetOpenNotesSucceeded, SetOpenNotesFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/open-notes/{note-id}
        #region OpenNote

        public event EventHandler<NotebookDataEventArgs> OpenNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> OpenNoteFailed;

        Request OpenNote_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request()
            {
                Method = RequestMethod.Put,
                Uri = OpenNoteUri(notebookID, noteID)
            };
        }

        public void OpenNote(Int64 notebookID, Int64 noteID)
        {
            CancelOpenNote(notebookID, noteID);
            CancelCloseNote(notebookID, noteID);

            Request request = OpenNote_Request(notebookID, noteID);

            request.Completed += OnOpenNoteCompleted;

            Request(request);
        }

        public bool IsOpenNotePending(Int64 notebookID, Int64 noteID)
        {
            Request request = OpenNote_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void CancelOpenNote(Int64 notebookID, Int64 noteID)
        {
            Request request = OpenNote_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnOpenNoteCompleted(Request request, Response response)
        {
            DefaultResponseHandler(request, response, OpenNoteSucceeded, OpenNoteFailed);
        }

        #endregion

        // DELETE /notebooks/{notebook-id}/open-notes/{note-id}
        #region CloseNote

        public event EventHandler<NotebookDataEventArgs> CloseNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CloseNoteFailed;

        Request CloseNote_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Delete,
                Uri = OpenNoteUri(notebookID, noteID)
            };
        }

        public void CloseNote(Int64 notebookID, Int64 noteID)
        {
            CancelOpenNote(notebookID, noteID);
            CancelCloseNote(notebookID, noteID);

            Request request = CloseNote_Request(notebookID, noteID);

            request.Completed += OnCloseNoteCompleted;

            Request(request);
        }

        public bool IsCloseNotePending(Int64 notebookID, Int64 noteID)
        {
            Request request = CloseNote_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void CancelCloseNote(Int64 notebookID, Int64 noteID)
        {
            Request request = CloseNote_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnCloseNoteCompleted(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CloseNoteSucceeded, CloseNoteFailed);
        }

        #endregion

        #endregion

        #region Notes

        public enum NoteSort
        {
            ViewedAt,
            CreatedAt,
            ModifiedAt,
            Title
        }
        public enum NoteOrder
        {
            Descending,
            Ascending
        }

        #region URIs

        public Uri NotesUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotebookUri(notebookID), "notes");
        }

        public Uri NotesMetadataUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotesUri(notebookID), "metadata");
        }

        public Uri FindNotesUri(Int64 notebookID, string search, DateTime createdAfter = default(DateTime), DateTime createdBefore = default(DateTime), DateTime modifiedAfter = default(DateTime), DateTime modifiedBefore = default(DateTime), DateTime viewedAfter = default(DateTime), DateTime viewedBefore = default(DateTime), NoteSort? sortBy = null, NoteOrder? order = null, int offset = 0, int limit = -1, bool returnText = false)
        {
            // Don't use UriBuilder for this. It doesn't properly format URIs with non-standard schemes in release build.

            StringBuilder query = new StringBuilder();
            query.Append("search=" + HttpUtility.UrlEncode(search));

            string created = ToRange(createdAfter, createdBefore);
            if (created != null)
            {
                query.Append("&created=" + HttpUtility.UrlEncode(created));
            }

            string modified = ToRange(modifiedAfter, modifiedBefore);
            if (modified != null)
            {
                query.Append("&modified=" + HttpUtility.UrlEncode(modified));
            }

            string viewed = ToRange(viewedAfter, viewedBefore);
            if (viewed != null)
            {
                query.Append("&viewed=" + HttpUtility.UrlEncode(viewed));
            }

            if (sortBy.HasValue)
            {
                switch (sortBy.Value)
                {
                    case NoteSort.ViewedAt:
                        query.Append("&sort=viewedAt");
                        break;
                    case NoteSort.CreatedAt:
                        query.Append("&sort=createdAt");
                        break;
                    case NoteSort.ModifiedAt:
                        query.Append("&sort=modifiedAt");
                        break;
                    case NoteSort.Title:
                        query.Append("&sort=title");
                        break;
                }
            }
            if (order.HasValue)
            {
                switch (order.Value)
                {
                    case NoteOrder.Descending:
                        query.Append("&order=desc");
                        break;
                    case NoteOrder.Ascending:
                        query.Append("&order=asc");
                        break;
                }
            }

            if (offset != 0)
            {
                query.Append("&offset=" + offset);
            }

            if (limit != -1)
            {
                query.Append("&limit=" + limit);
            }

            if (returnText)
            {
                query.Append("&return=text");
            }

            string uri = NotesUri(notebookID) + "?" + query;
            return new Uri(uri);
        }

        static string ToRange(DateTime first, DateTime second)
        {
            if ((first == default(DateTime) || first == DateTime.MinValue) &&
                (second == default(DateTime) || second == DateTime.MaxValue))
            {
                return null;
            }

            var result = new StringBuilder();

            if (first != DateTime.MinValue && first != DateTime.MinValue)
            {
                result.Append(first.ToString());
            }

            result.Append('-');

            if (second != default(DateTime) && second != DateTime.MaxValue)
            {
                result.Append(second.ToString());
            }

            return result.ToString();
        }

        public Uri NoteUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NotesUri(notebookID), noteID.ToString());
        }

        public Uri NoteMetadataUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "metadata");
        }

        public Uri NoteTitleUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "title");
        }

        public Uri NoteContentUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "content");
        }

        public Uri NoteTextUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "text");
        }        

        public Uri NoteCreatedAtUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "created_at");
        }

        public Uri NoteModifiedAtUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "modified_at");
        }

        public Uri NoteViewedAtUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "viewed_at");
        }

        public Uri NoteCategoriesUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "categories");
        }

        public Uri NoteCategoryUri(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            return UriHelper.AppendPath(NoteCategoriesUri(notebookID, noteID), categoryID.ToString());
        }

        #endregion

        // GET /notebooks/{id}/notes?search={search}&offset={offset}&limit={limit}
        #region FindNotes

        public event EventHandler<SearchResultsDataEventArgs> FindNotesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> FindNotesFailed;

        Request FindNotes_Request(Int64 notebookID, string phrase = "", DateTime createdAfter = default(DateTime), DateTime createdBefore = default(DateTime), DateTime modifiedAfter = default(DateTime), DateTime modifiedBefore = default(DateTime), DateTime viewedAfter = default(DateTime), DateTime viewedBefore = default(DateTime), NoteSort? sortBy = null, NoteOrder? order = null, int offset = 0, int limit = -1, bool returnText = true)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = FindNotesUri(notebookID, phrase, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sortBy, order, offset, limit, returnText)
            };
        }

        public void FindNotes(Int64 notebookID, string searchString, DateTime createdAfter = default(DateTime), DateTime createdBefore = default(DateTime), DateTime modifiedAfter = default(DateTime), DateTime modifiedBefore = default(DateTime), DateTime viewedAfter = default(DateTime), DateTime viewedBefore = default(DateTime), NoteSort? sortBy = null, NoteOrder? order = null, int offset = 0, int limit = -1, bool returnText = true)
        {
            Request request = FindNotes_Request(notebookID, searchString, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sortBy, order, offset, limit, returnText);

            request.Completed += OnFindNotesResponse;

            Request(request);
        }

        public bool IsFindNotesPending(Int64 notebookID, string searchString, DateTime createdAfter, DateTime createdBefore, DateTime modifiedAfter, DateTime modifiedBefore, DateTime viewedAfter, DateTime viewedBefore, NoteSort? sortBy, NoteOrder? order, int offset, int limit, bool returnText)
        {
            Request request = FindNotes_Request(notebookID, searchString, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sortBy, order, offset, limit, returnText);

            return IsPending(request);
        }

        protected void OnFindNotesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, FindNotesSucceeded, FindNotesFailed);
        }

        public IAsyncResult BeginFindNotes(Int64 notebookID, string searchString, DateTime createdAfter = default(DateTime), DateTime createdBefore = default(DateTime), DateTime modifiedAfter = default(DateTime), DateTime modifiedBefore = default(DateTime), DateTime viewedAfter = default(DateTime), DateTime viewedBefore = default(DateTime), NoteSort? sortBy = null, NoteOrder? order = null, int offset = 0, int limit = -1, bool returnText = false, AsyncCallback callback = null, object state = null)
        {
            Request request = FindNotes_Request(notebookID, searchString, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sortBy, order, offset, limit, returnText);

            return BeginRequest(request, callback, state);
        }

        public SearchResultsDataModel EndFindNotes(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<SearchResultsDataModel>();
        }


        #endregion

        // PATCH /notebooks/{id}/notes
        #region UpdateNotes

        public event EventHandler<NotebookDataEventArgs> UpdateNotesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateNotesFailed;

        Request UpdateNotes_Request(Int64 notebookID, NoteDataModel[] notes, bool deleteMissingItems, bool purge)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetUpdateParameters(NotesUri(notebookID), deleteMissingItems, purge),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel { Notes = notes }
            };
        }

        public void UpdateNotes(Int64 notebookID, NoteDataModel[] notes, bool deleteMissingItems = false, bool purge = false)
        {
            Request request = UpdateNotes_Request(notebookID, notes, deleteMissingItems, purge);

            request.Completed += OnUpdateNotesResponse;

            Request(request);
        }

        protected void OnUpdateNotesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateNotesSucceeded, UpdateNotesFailed);
        }

        public IAsyncResult BeginUpdateNotes(Int64 notebookID, NoteDataModel[] notes, bool deleteMissingItems = false, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateNotes_Request(notebookID, notes, deleteMissingItems, purge);

            return BeginRequest(request, callback, state);
        }

        public NotebookDataModel EndUpdateNotes(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NotebookDataModel>();
        }

        #endregion

        // GET /notebooks/{id}/notes/metadata
        #region GetNotesMetadata

        public event EventHandler<NotebookDataEventArgs> GetNotesMetadataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNotesMetadataFailed;

        Request GetNotesMetadata_Request(Int64 notebookID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NotesMetadataUri(notebookID)
            };
        }

        public void GetNotesMetadata(Int64 notebookID)
        {
            if (!IsGetNotesMetadataPending(notebookID))
            {
                Request request = GetNotesMetadata_Request(notebookID);

                request.Completed += OnGetNotesMetadataResponse;

                Request(request);
            }
        }

        public bool IsGetNotesMetadataPending(Int64 notebookID)
        {
            Request request = GetNotesMetadata_Request(notebookID);

            return IsPending(request);
        }

        protected void OnGetNotesMetadataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNotesMetadataSucceeded, GetNotesMetadataFailed);
        }

        public IAsyncResult BeginGetNotesMetadata(Int64 notebookID, AsyncCallback callback = null, object state = null)
        {
            Request request = GetNotesMetadata_Request(notebookID);

            return BeginRequest(request, callback, state);
        }

        public NotebookDataModel EndGetNotesMetadata(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NotebookDataModel>();
        }

        #endregion

        // PATCH /notebooks/{id}/notes/metadata
        #region UpdateNotesMetadata

        public event EventHandler<NotebookDataEventArgs> UpdateNotesMetadataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateNotesMetadataFailed;

        Request UpdateNotesMetadata_Request(Int64 notebookID, NoteDataModel[] notes, bool purge)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetPurge(NotesMetadataUri(notebookID), purge),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel { Notes = notes }
            };
        }

        public void UpdateNotesMetadata(Int64 notebookID, NoteDataModel[] notes, bool purge = false)
        {
            Request request = UpdateNotesMetadata_Request(notebookID, notes, purge);

            request.Completed += OnUpdateNotesMetadataResponse;

            Request(request);
        }

        protected void OnUpdateNotesMetadataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateNotesMetadataSucceeded, UpdateNotesMetadataFailed);
        }

        public IAsyncResult BeginUpdateNotesMetadata(Int64 notebookID, NoteDataModel[] notes, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateNotesMetadata_Request(notebookID, notes, purge);

            return BeginRequest(request, callback, state);
        }

        public NotebookDataModel EndUpdateNotesMetadata(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NotebookDataModel>();
        }

        #endregion

        // POST /notebooks/{id}/notes
        #region CreateNote

        public event EventHandler<NoteDataEventArgs> CreateNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CreateNoteFailed;

        Request CreateNote_Request(Int64 notebookID, Int64 noteID, DateTime createdAt)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = NotesUri(notebookID),
                ContentType = "text/xml",
                ContentObject = new NoteDataModel 
                { 
                    ID = noteID,
                    CreatedAt = createdAt
                }
            };
        }

        public void CreateNote(Int64 notebookID, Int64 noteID)
        {
            Request request = CreateNote_Request(notebookID, noteID, Now);

            request.Completed += OnCreateNoteResponse;

            Request(request);
        }

        protected void OnCreateNoteResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CreateNoteSucceeded, CreateNoteFailed);
        }

        #endregion

        // DELETE /notebooks/{notebook-id}/notes/{note-id}
        #region DeleteNote

        public event EventHandler<NoteDataEventArgs> DeleteNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> DeleteNoteFailed;

        Request DeleteNote_Request(Int64 notebookID, Int64 noteID, bool purge)
        {
            var request = new Request
            {
                Method = RequestMethod.Delete,
                Uri = NoteUri(notebookID, noteID)
            };

            if (purge)
            {
                request.Params.Set("purge", "true");
            }

            return request;
        }

        public void DeleteNote(Int64 notebookID, Int64 noteID, bool purge)
        {
            Request request = DeleteNote_Request(notebookID, noteID, purge);

            request.Completed += OnDeleteNoteResponse;

            Request(request);
        }

        public void OnDeleteNoteResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, DeleteNoteSucceeded, DeleteNoteFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}
        #region GetNote

        public event EventHandler<NoteDataEventArgs> GetNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteFailed;

        Request GetNote_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteUri(notebookID, noteID)
            };
        }

        public void GetNote(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNotePending(notebookID, noteID))
            {
                Request request = GetNote_Request(notebookID, noteID);

                request.Completed += OnGetNoteResponse;

                Request(request);
            }
        }

        public bool IsGetNotePending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNote_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteSucceeded, GetNoteFailed);
        }

        public IAsyncResult BeginGetNote(Int64 notebookID, Int64 noteID, AsyncCallback callback = null, object state = null)
        {
            Request request = GetNote_Request(notebookID, noteID);

            return BeginRequest(request, callback, state);
        }

        public NoteDataModel EndGetNote(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NoteDataModel>();
        }

        #endregion

        // PATCH /notebooks/{notebook-id}/notes/{note-id}
        #region UpdateNote

        public event EventHandler<NoteDataEventArgs> UpdateNoteSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateNoteFailed;

        Request UpdateNote_Request(Int64 notebookID, Int64 noteID, NoteDataModel note = null)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = NoteUri(notebookID, noteID),
                ContentType = "text/xml",
                ContentObject = note
            };
        }

        public void UpdateNote(Int64 notebookID, Int64 noteID, NoteDataModel note)
        {
            CancelUpdateNote(notebookID, noteID);

            Request request = UpdateNote_Request(notebookID, noteID, (NoteDataModel)note.Clone());

            request.Completed += OnUpdateNoteResponse;

            Request(request);
        }

        public void CancelUpdateNote(Int64 notebookID, Int64 noteID)
        {
            Request request = UpdateNote_Request(notebookID, noteID);

            Cancel(request);
        }

        protected void OnUpdateNoteResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateNoteSucceeded, UpdateNoteFailed);
        }

        public IAsyncResult BeginUpdateNote(Int64 notebookID, Int64 noteID, NoteDataModel note, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateNote_Request(notebookID, noteID, (NoteDataModel)note.Clone());

            return BeginRequest(request, callback, state);
        }

        public NoteDataModel EndUpdateNote(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NoteDataModel>();
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/metadata
        #region GetNoteMetadata

        public event EventHandler<NoteDataEventArgs> GetNoteMetadataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteMetadataFailed;

        Request GetNoteMetadata_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteMetadataUri(notebookID, noteID)
            };
        }

        public void GetNoteMetadata(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNoteMetadataPending(notebookID, noteID))
            {
                Request request = GetNoteMetadata_Request(notebookID, noteID);

                request.Completed += OnGetNoteMetadataResponse;

                Request(request);
            }
        }

        public bool IsGetNoteMetadataPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteMetadata_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteMetadataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteMetadataSucceeded, GetNoteMetadataFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/title
        #region GetNoteTitle

        public event EventHandler<NoteDataEventArgs> GetNoteTitleSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteTitleFailed;

        Request GetNoteTitle_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteTitleUri(notebookID, noteID)
            };
        }

        public void GetNoteTitle(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNoteTitlePending(notebookID, noteID))
            {
                Request request = GetNoteTitle_Request(notebookID, noteID);

                request.Completed += OnGetNoteTitleResponse;

                Request(request);
            }
        }

        public bool IsGetNoteTitlePending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteTitle_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteTitleResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteTitleSucceeded, GetNoteTitleFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/notes/{note-id}/title
        #region SetNoteTitle

        public event EventHandler<NoteDataEventArgs> SetNoteTitleSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetNoteTitleFailed;

        Request SetNoteTitle_Request(Int64 notebookID, Int64 noteID, string newTitle = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = NoteTitleUri(notebookID, noteID),
                ContentType = "text/plain",
                ContentObject = newTitle
            };
        }

        public void SetNoteTitle(Int64 notebookID, Int64 noteID, string newTitle)
        {
            CancelSetNoteTitle(notebookID, noteID);

            Request request = SetNoteTitle_Request(notebookID, noteID, newTitle);

            request.Completed += OnSetNoteTitleResponse;

            Request(request);
        }

        public void CancelSetNoteTitle(Int64 notebookID, Int64 noteID)
        {
            Request request = SetNoteTitle_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnSetNoteTitleResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetNoteTitleSucceeded, SetNoteTitleFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/content
        #region GetNoteContent

        public event EventHandler<NoteDataEventArgs> GetNoteContentSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteContentFailed;

        Request GetNoteContent_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Args = new NoteDataModel { NotebookID = notebookID, ID = noteID },
                Method = RequestMethod.Get,
                Uri = NoteContentUri(notebookID, noteID)
            };
        }

        public void GetNoteContent(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNoteContentPending(notebookID, noteID))
            {
                Request request = GetNoteContent_Request(notebookID, noteID);

                request.Completed += OnGetNoteContentResponse;

                Request(request);
            }
        }

        public bool IsGetNoteContentPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteContent_Request(notebookID, noteID);

            return IsPending(request);
        }

        protected void OnGetNoteContentResponse(Request request, Response response)
        {
            if (!response.IsSuccess)
            {
                OnDefaultErrorHandler(request, response, GetNoteContentFailed);
                return;
            }

            NoteDataModel args = (NoteDataModel)request.Args;

            NoteDataModel note = new NoteDataModel
            {
                ID = args.ID,
                NotebookID = args.NotebookID,
                Content = response.ContentString
            };

            OnGetNoteContentSucceeded(note);
        }

        protected void OnGetNoteContentSucceeded(NoteDataModel note)
        {
            var handler = GetNoteContentSucceeded;
            if (handler != null)
            {
                handler(this, new NoteDataEventArgs { Note = note });
            }
        }

        #endregion

        // PUT /notebooks/{notebook-id}/notes/{note-id}/content
        #region SetNoteContent

        public event EventHandler<NoteDataEventArgs> SetNoteContentSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetNoteContentFailed;

        Request SetNoteContent_Request(Int64 notebookID, Int64 noteID, string newContent = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = NoteContentUri(notebookID, noteID),
                ContentType = "text/html",
                ContentString = newContent
            };
        }

        public void SetNoteContent(Int64 notebookID, Int64 noteID, string newContent)
        {
            CancelSetNoteContent(notebookID, noteID);

            Request request = SetNoteContent_Request(notebookID, noteID, newContent);

            request.Completed += OnSetNoteContentResponse;

            Request(request);
        }

        public void CancelSetNoteContent(Int64 notebookID, Int64 noteID)
        {
            Request request = SetNoteContent_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnSetNoteContentResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetNoteContentSucceeded, SetNoteContentFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/notes/{note-id}/text
        #region SetNoteText

        public event EventHandler<NoteDataEventArgs> SetNoteTextSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetNoteTextFailed;

        Request SetNoteText_Request(Int64 notebookID, Int64 noteID, string newText = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = NoteTextUri(notebookID, noteID),
                ContentType = "text/plain",
                ContentString = newText
            };
        }

        public void SetNoteText(Int64 notebookID, Int64 noteID, string newText)
        {
            CancelSetNoteText(notebookID, noteID);

            Request request = SetNoteText_Request(notebookID, noteID, newText);

            request.Completed += OnSetNoteTextResponse;

            Request(request);
        }

        public void CancelSetNoteText(Int64 notebookID, Int64 noteID)
        {
            Request request = SetNoteText_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnSetNoteTextResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetNoteTextSucceeded, SetNoteTextFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/created_at
        #region GetNoteCreatedAt

        public event EventHandler<NoteDataEventArgs> GetNoteCreatedAtSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteCreatedAtFailed;

        Request GetNoteCreatedAt_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteCreatedAtUri(notebookID, noteID)
            };
        }

        public void GetNoteCreatedAt(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNoteCreatedAtPending(notebookID, noteID))
            {
                Request request = GetNoteCreatedAt_Request(notebookID, noteID);

                request.Completed += OnGetNoteCreatedAtResponse;

                Request(request);
            }
        }

        public bool IsGetNoteCreatedAtPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteCreatedAt_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteCreatedAtResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteCreatedAtSucceeded, GetNoteCreatedAtFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/modified_at
        #region GetNoteModifiedAt

        public event EventHandler<NoteDataEventArgs> GetNoteModifiedAtSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteModifiedAtFailed;

        Request GetNoteModifiedAt_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteModifiedAtUri(notebookID, noteID)
            };
        }

        public void GetNoteModifiedAt(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNoteModifiedAtPending(notebookID, noteID))
            {
                Request request = GetNoteModifiedAt_Request(notebookID, noteID);

                request.Completed += OnGetNoteModifiedAtResponse;

                Request(request);
            }
        }

        public bool IsGetNoteModifiedAtPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteModifiedAt_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteModifiedAtResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteModifiedAtSucceeded, GetNoteModifiedAtFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/viewed_at
        #region GetNoteViewedAt

        public event EventHandler<NoteDataEventArgs> GetNoteViewedAtSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteViewedAtFailed;

        Request GetNoteViewedAt_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteViewedAtUri(notebookID, noteID)
            };
        }

        public void GetNoteViewedAt(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetNoteViewedAtPending(notebookID, noteID))
            {
                Request request = GetNoteViewedAt_Request(notebookID, noteID);

                request.Completed += OnGetNoteViewedAtResponse;

                Request(request);
            }
        }

        public bool IsGetNoteViewedAtPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteViewedAt_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteViewedAtResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteViewedAtSucceeded, GetNoteViewedAtFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/notes/{note-id}/viewedAt
        #region SetNoteViewedAt

        public event EventHandler<NoteDataEventArgs> SetNoteViewedAtSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetNoteViewedAtFailed;

        Request SetNoteViewedAt_Request(Int64 notebookID, Int64 noteID, DateTime viewedAt = default(DateTime))
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = NoteViewedAtUri(notebookID, noteID),
                ContentType = "text/xml",
                ContentObject = new NoteDataModel { ViewedAt = viewedAt }
            };
        }

        public void SetNoteViewedAt(Int64 notebookID, Int64 noteID, DateTime viewedAt)
        {
            CancelSetNoteViewedAt(notebookID, noteID);

            Request request = SetNoteViewedAt_Request(notebookID, noteID, viewedAt);

            request.Completed += OnSetNoteViewedAtResponse;

            Request(request);
        }

        public void CancelSetNoteViewedAt(Int64 notebookID, Int64 noteID)
        {
            Request request = SetNoteViewedAt_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnSetNoteViewedAtResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetNoteViewedAtSucceeded, SetNoteViewedAtFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/categories
        #region GetNoteCategories

        public event EventHandler<NoteDataEventArgs> GetNoteCategoriesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetNoteCategoriesFailed;

        Request GetNoteCategories_Request(Int64 notebookID, Int64 noteID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = NoteCategoriesUri(notebookID, noteID)
            };
        }

        public void GetNoteCategories(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteCategories_Request(notebookID, noteID);

            request.Completed += OnGetNoteCategoriesResponse;

            Request(request);
        }

        public bool IsGetNoteCategoriesPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetNoteCategories_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetNoteCategoriesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetNoteCategoriesSucceeded, GetNoteCategoriesFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/open-notes/{note-id}/categories/{category-id}
        #region AddNoteCategory

        public event EventHandler<NoteDataEventArgs> AddNoteCategorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> AddNoteCategoryFailed;

        Request AddNoteCategory_Request(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            return new Request()
            {
                Method = RequestMethod.Put,
                Uri = NoteCategoryUri(notebookID, noteID, categoryID)
            };
        }

        public void AddNoteCategory(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            Request request = AddNoteCategory_Request(notebookID, noteID, categoryID);

            request.Completed += OnAddNoteCategoryCompleted;

            Request(request);
        }

        public void OnAddNoteCategoryCompleted(Request request, Response response)
        {
            DefaultResponseHandler(request, response, AddNoteCategorySucceeded, AddNoteCategoryFailed);
        }

        #endregion

        // DELETE /notebooks/{notebook-id}/open-notes/{note-id}/categories/{category-id}
        #region RemoveNoteCategory

        public event EventHandler<NoteDataEventArgs> RemoveNoteCategorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> RemoveNoteCategoryFailed;

        Request RemoveNoteCategory_Request(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            return new Request()
            {
                Method = RequestMethod.Delete,
                Uri = NoteCategoryUri(notebookID, noteID, categoryID)
            };
        }

        public void RemoveNoteCategory(Int64 notebookID, Int64 noteID, Int64 categoryID)
        {
            Request request = RemoveNoteCategory_Request(notebookID, noteID, categoryID);

            request.Completed += OnRemoveNoteCategoryCompleted;

            Request(request);
        }

        public void OnRemoveNoteCategoryCompleted(Request request, Response response)
        {
            DefaultResponseHandler(request, response, RemoveNoteCategorySucceeded, RemoveNoteCategoryFailed);
        }

        #endregion

        #endregion

        #region Files

        #region URI

        public Uri FilesUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(NoteUri(notebookID, noteID), "files");
        }

        public Uri FilesMetadataUri(Int64 notebookID, Int64 noteID)
        {
            return UriHelper.AppendPath(FilesUri(notebookID, noteID), "metadata");
        }

        public Uri FileUri(Int64 notebookID, Int64 noteID, string fileName)
        {
            return UriHelper.AppendPath(FilesUri(notebookID, noteID), HttpUtility.UrlEncode(fileName));
        }

        public Uri FileMetadataUri(Int64 notebookID, Int64 noteID, string fileName)
        {
            return UriHelper.AppendPath(FileUri(notebookID, noteID, fileName), "metadata");
        }

        #endregion

        // PUT /notesbooks/{notebook-id}/notes/{note-id}/files
        #region SetFiles

        public event EventHandler<NoteDataEventArgs> SetFilesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetFilesFailed;

        Request SetFiles_Request(Int64 notebookID, Int64 noteID, string[] fileNames)
        {
            var files = (from name in fileNames select new FileDataModel { Name = name }).ToArray();

            return new Request
            {
                Method = RequestMethod.Put,
                Uri = FilesUri(notebookID, noteID),
                ContentObject = new NoteDataModel { Files = files }
            };
        }

        public void SetFiles(Int64 notebookID, Int64 noteID, string[] fileNames)
        {
            Request request = SetFiles_Request(notebookID, noteID, fileNames);

            request.Completed += OnSetFilesResponse;

            Request(request);
        }

        public void OnSetFilesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetFilesSucceeded, SetFilesFailed);
        }

        #endregion

        // POST /notebooks/{notebook-id}/notes/{note-id}/files
        #region CreateFile

        public event EventHandler<FileDataEventArgs> CreateFileSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CreateFileFailed;

        Request CreateFile_Request(Int64 notebookID, Int64 noteID, string fileName)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = FilesUri(notebookID, noteID),
                ContentType = "text/xml",
                ContentObject = new FileDataModel { Name = fileName }
            };
        }

        public void CreateFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            Request request = CreateFile_Request(notebookID, noteID, fileName);

            request.Completed += OnCreateFileResponse;

            Request(request);
        }

        protected void OnCreateFileResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CreateFileSucceeded, CreateFileFailed);
        }

        #endregion

        // DELETE /notebooks/{notebook-id}/notes/{note-id}/files/{file-name}
        #region DeleteFile

        public event EventHandler<FileDataEventArgs> DeleteFileSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> DeleteFileFailed;

        Request DeleteFile_Request(Int64 notebookID, Int64 noteID, string fileName)
        {
            return new Request
            {
                Method = RequestMethod.Delete,
                Uri = FileUri(notebookID, noteID, fileName)
            };
        }

        public void DeleteFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            Request request = DeleteFile_Request(notebookID, noteID, fileName);

            request.Completed += OnDeleteFileResponse;

            Request(request);
        }

        public void OnDeleteFileResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, DeleteFileSucceeded, DeleteFileFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/notes/{note-id}/files/{file-name}
        #region GetFile

        public event EventHandler<FileDataEventArgs> GetFileSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetFileFailed;

        Request GetFile_Request(Int64 notebookID, Int64 noteID, string fileName)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = FileUri(notebookID, noteID, fileName),
                Args = new FileDataModel { NotebookID = notebookID, NoteID = noteID, Name = fileName }
            };
        }

        public void GetFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            if (!IsGetFilePending(notebookID, noteID, fileName))
            {
                Request request = GetFile_Request(notebookID, noteID, fileName);

                request.Completed += OnGetFileResponse;

                Request(request);
            }
        }

        public bool IsGetFilePending(Int64 notebookID, Int64 noteID, string fileName)
        {
            Request request = GetFile_Request(notebookID, noteID, fileName);

            return IsPending(request);
        }

        protected void OnGetFileResponse(Request request, Response response)
        {
            if (!response.IsSuccess)
            {
                OnDefaultErrorHandler(request, response, GetFileFailed);
                return;
            }

            FileDataModel file = (FileDataModel)request.Args;
            file.Data = response.ContentObject as byte[];
            OnGetFileSucceeded(file);
        }

        protected void OnGetFileSucceeded(FileDataModel file)
        {
            var handler = GetFileSucceeded;
            if (handler != null)
            {
                handler(this, new FileDataEventArgs { File = file });
            }
        }

        #endregion

        // PUT /notebooks/{notebook-id}/notes/{note-id}/files/{file-name}
        #region SetFile

        public event EventHandler<FileDataEventArgs> SetFileSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetFileFailed;

        Request SetFile_Request(Int64 notebookID, Int64 noteID, string fileName, byte[] data = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = FileUri(notebookID, noteID, fileName),
                ContentObject = data
            };
        }

        public void SetFile(Int64 notebookID, Int64 noteID, string fileName, byte[] data)
        {
            CancelSetFile(notebookID, noteID, fileName);

            Request request = SetFile_Request(notebookID, noteID, fileName, data);

            request.Completed += OnSetFileResponse;

            Request(request);
        }

        public void CancelSetFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            Request request = SetFile_Request(notebookID, noteID, fileName);

            Cancel(request);
        }

        public void OnSetFileResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetFileSucceeded, SetFileFailed);
        }

        #endregion

        // PATCH /notebooks/{notebook-id}/notes/{note-id}/files/{file-name}
        #region UpdateFile

        public event EventHandler<FileDataEventArgs> UpdateFileSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateFileFailed;

        Request UpdateFile_Request(Int64 notebookID, Int64 noteID, string fileName, byte[] data = null)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = FileUri(notebookID, noteID, fileName),
                ContentObject = data
            };
        }

        public void UpdateFile(Int64 notebookID, Int64 noteID, string fileName, byte[] data)
        {
            CancelUpdateFile(notebookID, noteID, fileName);

            Request request = UpdateFile_Request(notebookID, noteID, fileName, data);

            request.Completed += OnUpdateFileResponse;

            Request(request);
        }

        public void CancelUpdateFile(Int64 notebookID, Int64 noteID, string fileName)
        {
            Request request = UpdateFile_Request(notebookID, noteID, fileName);

            Cancel(request);
        }

        public void OnUpdateFileResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateFileSucceeded, UpdateFileFailed);
        }

        #endregion

        #endregion

        #region Categories

        #region URIs

        public Uri CategoriesUri(Int64 notebookID)
        {
            return UriHelper.AppendPath(NotebookUri(notebookID), "categories");
        }

        public Uri CategoriesUri(Int64 notebookID, Int64 parentID)
        {
            var baseUri = CategoriesUri(notebookID);
            var result = new UriBuilder(baseUri);
            result.Query = String.Format("parent={0}", parentID);
            return result.Uri;
        }

        public Uri CategoryUri(Int64 notebookID, Int64 categoryID)
        {
            return UriHelper.AppendPath(CategoriesUri(notebookID), categoryID.ToString());
        }

        public Uri CategoryNameUri(Int64 notebookID, Int64 categoryID)
        {
            return UriHelper.AppendPath(CategoryUri(notebookID, categoryID), "name");
        }

        public Uri CategoryParentUri(Int64 notebookID, Int64 categoryID)
        {
            return UriHelper.AppendPath(CategoryUri(notebookID, categoryID), "parent");
        }

        public Uri CategoryChildrenUri(Int64 notebookID, Int64 categoryID)
        {
            return UriHelper.AppendPath(CategoryUri(notebookID, categoryID), "children");
        }

        #endregion

        // GET /notebooks/{notebook-id}/categories
        #region GetCategories

        public event EventHandler<NotebookDataEventArgs> GetCategoriesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetCategoriesFailed;

        Request GetCategories_Request(Int64 notebookID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = CategoriesUri(notebookID)
            };
        }

        public void GetCategories(Int64 notebookID)
        {
            Request request = GetCategories_Request(notebookID);

            request.Completed += OnGetCategoriesResponse;

            Request(request);
        }

        public bool IsGetCategoriesPending(Int64 notebookID)
        {
            Request request = GetCategories_Request(notebookID);

            return IsPending(request);
        }

        protected void OnGetCategoriesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetCategoriesSucceeded, GetCategoriesFailed);
        }

        public IAsyncResult BeginGetCategories(Int64 notebookID, AsyncCallback callback = null, object state = null)
        {
            Request request = GetCategories_Request(notebookID);

            return BeginRequest(request, callback, state);
        }

        public NotebookDataModel EndGetCategories(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NotebookDataModel>();
        }

        #endregion

        // PUT /notebooks/{notebook-id}/categories
        #region UpdateCategories

        public event EventHandler<NotebookDataEventArgs> UpdateCategoriesSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateCategoriesFailed;

        Request UpdateCategories_Request(Int64 notebookID, CategoryDataModel[] categories = null, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = SetPurge(CategoriesUri(notebookID), purge),
                ContentType = "text/xml",
                ContentObject = new NotebookDataModel { Categories = categories }
            };
        }

        public void UpdateCategories(Int64 notebookID, CategoryDataModel[] categories, bool purge = false)
        {
            Request request = UpdateCategories_Request(notebookID, categories, purge);

            request.Completed += OnUpdateCategoriesResponse;

            Request(request);
        }

        protected void OnUpdateCategoriesResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateCategoriesSucceeded, UpdateCategoriesFailed);
        }

        public IAsyncResult BeginUpdateCategories(Int64 notebookID, CategoryDataModel[] categories, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateCategories_Request(notebookID, categories, purge);

            return BeginRequest(request, callback, state);
        }

        public NotebookDataModel EndUpdateCategories(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<NotebookDataModel>();
        }

        #endregion

        // POST /notebooks/{notebook-id}/categories
        #region CreateCategory

        public event EventHandler<CategoryDataEventArgs> CreateCategorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CreateCategoryFailed;

        Request CreateCategory_Request(Int64 notebookID, Int64 categoryID, string name = null, Int64 parentID = 0)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = CategoriesUri(notebookID),
                ContentType = "text/xml",
                ContentObject = new CategoryDataModel { ID = categoryID, Name = name, ParentID = parentID }
            };
        }

        public void CreateCategory(Int64 notebookID, Int64 categoryID, string name, Int64 parentID = 0)
        {
            Request request = CreateCategory_Request(notebookID, categoryID, name, parentID);

            request.Completed += OnCreateCategoryResponse;

            Request(request);
        }

        protected void OnCreateCategoryResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CreateCategorySucceeded, CreateCategoryFailed);
        }

        #endregion

        // DELETE /notebooks/{notebook-id}/categories/{category-id}
        #region DeleteCategory

        public event EventHandler<CategoryDataEventArgs> DeleteCategorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> DeleteCategoryFailed;

        Request DeleteCategory_Request(Int64 notebookID, Int64 categoryID, bool purge)
        {
            var request = new Request
            {
                Method = RequestMethod.Delete,
                Uri = CategoryUri(notebookID, categoryID)
            };

            if (purge)
            {
                request.Params.Set("purge", "true");
            }

            return request;
        }

        public void DeleteCategory(Int64 notebookID, Int64 categoryID, bool purge)
        {
            Request request = DeleteCategory_Request(notebookID, categoryID, purge);

            request.Completed += OnDeleteCategoryResponse;

            Request(request);
        }

        public void OnDeleteCategoryResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, DeleteCategorySucceeded, DeleteCategoryFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/categories/{category-id}
        #region GetCategory

        public event EventHandler<CategoryDataEventArgs> GetCategorySucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetCategoryFailed;

        Request GetCategory_Request(Int64 notebookID, Int64 categoryID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = CategoryUri(notebookID, categoryID)
            };
        }

        public void GetCategory(Int64 notebookID, Int64 categoryID)
        {
            if (!IsGetCategoryPending(notebookID, categoryID))
            {
                Request request = GetCategory_Request(notebookID, categoryID);

                request.Completed += OnGetCategoryResponse;

                Request(request);
            }
        }

        public bool IsGetCategoryPending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetCategory_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetCategoryResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetCategorySucceeded, GetCategoryFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/categories/{category-id}/name
        #region GetCategoryName

        public event EventHandler<CategoryDataEventArgs> GetCategoryNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetCategoryNameFailed;

        Request GetCategoryName_Request(Int64 notebookID, Int64 categoryID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = CategoryNameUri(notebookID, categoryID)
            };
        }

        public void GetCategoryName(Int64 notebookID, Int64 noteID)
        {
            if (!IsGetCategoryNamePending(notebookID, noteID))
            {
                Request request = GetCategoryName_Request(notebookID, noteID);

                request.Completed += OnGetCategoryNameResponse;

                Request(request);
            }
        }

        public bool IsGetCategoryNamePending(Int64 notebookID, Int64 noteID)
        {
            Request request = GetCategoryName_Request(notebookID, noteID);

            return IsPending(request);
        }

        public void OnGetCategoryNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetCategoryNameSucceeded, GetCategoryNameFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/categories/{category-id}/name
        #region SetCategoryName

        public event EventHandler<CategoryDataEventArgs> SetCategoryNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetCategoryNameFailed;

        Request SetCategoryName_Request(Int64 notebookID, Int64 noteID, string newName = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = CategoryNameUri(notebookID, noteID),
                ContentType = "text/xml",
                ContentObject = new CategoryDataModel { Name = newName, NameModifiedAt = Now }
            };
        }

        public void SetCategoryName(Int64 notebookID, Int64 noteID, string newName)
        {
            CancelSetCategoryName(notebookID, noteID);

            Request request = SetCategoryName_Request(notebookID, noteID, newName);

            request.Completed += OnSetCategoryNameResponse;

            Request(request);
        }

        public void CancelSetCategoryName(Int64 notebookID, Int64 noteID)
        {
            Request request = SetCategoryName_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnSetCategoryNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetCategoryNameSucceeded, SetCategoryNameFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/categories/{category-id}/parent
        #region GetCategoryParent

        public event EventHandler<CategoryDataEventArgs> GetCategoryParentSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetCategoryParentFailed;

        Request GetCategoryParent_Request(Int64 notebookID, Int64 categoryID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = CategoryParentUri(notebookID, categoryID)
            };
        }

        public void GetCategoryParent(Int64 notebookID, Int64 categoryID)
        {
            if (!IsGetCategoryParentPending(notebookID, categoryID))
            {
                Request request = GetCategoryParent_Request(notebookID, categoryID);

                request.Completed += OnGetCategoryParentResponse;

                Request(request);
            }
        }

        public bool IsGetCategoryParentPending(Int64 notebookID, Int64 categoryID)
        {
            Request request = GetCategoryParent_Request(notebookID, categoryID);

            return IsPending(request);
        }

        public void OnGetCategoryParentResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetCategoryParentSucceeded, GetCategoryParentFailed);
        }

        #endregion

        // PUT /notebooks/{notebook-id}/categories/{category-id}/parent
        #region SetCategoryParent

        public event EventHandler<CategoryDataEventArgs> SetCategoryParentSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetCategoryParentFailed;

        Request SetCategoryParent_Request(Int64 notebookID, Int64 noteID, Int64 newParent = 0)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = CategoryParentUri(notebookID, noteID),
                ContentType = "text/xml",
                ContentObject = new CategoryDataModel { ParentID = newParent, ParentIDModifiedAt = Now }
            };
        }

        public void SetCategoryParent(Int64 notebookID, Int64 noteID, Int64 newParent)
        {
            CancelSetCategoryParent(notebookID, noteID);

            Request request = SetCategoryParent_Request(notebookID, noteID, newParent);

            request.Completed += OnSetCategoryParentResponse;

            Request(request);
        }

        public void CancelSetCategoryParent(Int64 notebookID, Int64 noteID)
        {
            Request request = SetCategoryParent_Request(notebookID, noteID);

            Cancel(request);
        }

        public void OnSetCategoryParentResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetCategoryParentSucceeded, SetCategoryParentFailed);
        }

        #endregion

        // GET /notebooks/{notebook-id}/categories/{category-id}/children
        #region GetCategoryChildren

        public event EventHandler<CategoryDataEventArgs> GetCategoryChildrenSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetCategoryChildrenFailed;

        public Request GetCategoryChildren_Request(Int64 notebookID, Int64 categoryID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = CategoryChildrenUri(notebookID, categoryID)
            };
        }

        public void GetCategoryChildren(Int64 notebookID, Int64 categoryID)
        {
            if (categoryID == -1 && 
                (IsGetRepositoryPending || IsGetNotebookPending(notebookID)))
            {
                return;
            }

            if (!IsGetCategoryChildrenPending(notebookID, categoryID))
            {
                Request request = GetCategoryChildren_Request(notebookID, categoryID);

                request.Completed += OnGetCategoryChildrenResponse;

                Request(request);
            }
        }

        public bool IsGetCategoryChildrenPending(Int64 notebookID, Int64 categoryID)
        {
            Request request = GetCategoryChildren_Request(notebookID, categoryID);

            return IsPending(request);
        }

        public void OnGetCategoryChildrenResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetCategoryChildrenSucceeded, GetCategoryChildrenFailed);
        }

        #endregion

        #endregion

        #region Clipart

        #region URIs

        public Uri ClipartGroupsUri()
        {
            return UriHelper.AppendPath(RepositoryUri(), "clipart");
        }

        public Uri ClipartGroupUri(Int64 groupID)
        {
            return UriHelper.AppendPath(ClipartGroupsUri(), groupID.ToString());
        }

        public Uri ClipartGroupNameUri(Int64 groupID)
        {
            return UriHelper.AppendPath(ClipartGroupUri(groupID), "name");
        }

        public Uri ClipartGroupItemsUri(Int64 groupID)
        {
            return UriHelper.AppendPath(ClipartGroupUri(groupID), "items");
        }

        public Uri ClipartItemsMetadataUri(Int64 groupID)
        {
            return UriHelper.AppendPath(ClipartGroupItemsUri(groupID), "metadata");
        }

        public Uri ClipartUri(Int64 groupID, Int64 clipartID)
        {
            return UriHelper.AppendPath(ClipartGroupItemsUri(groupID), clipartID.ToString());
        }

        public Uri ClipartNameUri(Int64 groupID, Int64 clipartID)
        {
            return UriHelper.AppendPath(ClipartUri(groupID, clipartID), "name");
        }

        public Uri ClipartDataUri(Int64 groupID, Int64 clipartID)
        {
            return UriHelper.AppendPath(ClipartUri(groupID, clipartID), "data");
        }

        #endregion

        // GET /clipart
        #region GetClipartGroups

        public event EventHandler<RepositoryDataEventArgs> GetClipartGroupsSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartGroupsFailed;

        public Request GetClipartGroups_Request()
        {
            return new Request()
            {
                Method = RequestMethod.Get,
                Uri = ClipartGroupsUri()
            };
        }

        public bool IsGetClipartGroupsPending { get; private set; }

        public void GetClipartGroups()
        {
            if (!IsGetClipartGroupsPending)
            {
                IsGetClipartGroupsPending = true;

                Request request = GetClipartGroups_Request();

                request.Completed += OnGetClipartGroupsCompleted;

                Request(request);
            }
        }

        public void OnGetClipartGroupsCompleted(Request request, Response response)
        {
            IsGetClipartGroupsPending = false;

            DefaultResponseHandler(request, response, GetClipartGroupsSucceeded, GetClipartGroupsFailed);
        }

        public IAsyncResult BeginGetClipartGroups(AsyncCallback callback = null, object state = null)
        {
            Request request = GetClipartGroups_Request();

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndGetClipartGroups(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // PATCH /clipart
        #region UpdateClipartGroups

        public event EventHandler<RepositoryDataEventArgs> UpdateClipartGroupsSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateClipartGroupsFailed;

        public Request UpdateClipartGroups_Request(ClipartGroupDataModel[] groups = null, bool deleteMissingItems = false, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetUpdateParameters(ClipartGroupsUri(), deleteMissingItems, purge),
                ContentType = "text/xml",
                ContentObject = new RepositoryDataModel { ClipartGroups = groups }
            };
        }

        public void UpdateClipartGroups(ClipartGroupDataModel[] groups, bool deleteMissingItems = false, bool purge = false)
        {
            Request request = UpdateClipartGroups_Request(groups, deleteMissingItems, purge);

            request.Completed += OnUpdateClipartGroupsResponse;

            Request(request);
        }

        protected void OnUpdateClipartGroupsResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateClipartGroupsSucceeded, UpdateClipartGroupsFailed);
        }

        public IAsyncResult BeginUpdateClipartGroups(ClipartGroupDataModel[] groups, bool deleteMissingItems = false, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateClipartGroups_Request(groups, deleteMissingItems, purge);

            return BeginRequest(request, callback, state);
        }

        public RepositoryDataModel EndUpdateClipartGroups(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<RepositoryDataModel>();
        }

        #endregion

        // POST /clipart
        #region CreateClipartGroup

        public event EventHandler<ClipartGroupDataEventArgs> CreateClipartGroupSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CreateClipartGroupFailed;

        public Request CreateClipartGroup_Request(Int64 groupID, string name)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = ClipartGroupsUri(),
                ContentType = "text/xml",
                ContentObject = new ClipartGroupDataModel { ID = groupID, Name = name }
            };
        }

        public void CreateClipartGroup(Int64 groupID, string name = null)
        {
            Request request = CreateClipartGroup_Request(groupID, name);

            request.Completed += OnCreateClipartGroupResponse;

            Request(request);
        }

        protected void OnCreateClipartGroupResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CreateClipartGroupSucceeded, CreateClipartGroupFailed);
        }

        #endregion

        // DELETE /clipart/{group-id}
        #region DeleteClipartGroup

        public event EventHandler<ClipartGroupDataEventArgs> DeleteClipartGroupSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> DeleteClipartGroupFailed;

        public Request DeleteClipartGroup_Request(Int64 groupID)
        {
            return new Request
            {
                Method = RequestMethod.Delete,
                Uri = ClipartGroupUri(groupID)
            };
        }

        public void DeleteClipartGroup(Int64 groupID)
        {
            Request request = DeleteClipartGroup_Request(groupID);

            request.Completed += OnDeleteClipartGroupResponse;

            Request(request);
        }

        public void OnDeleteClipartGroupResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, DeleteClipartGroupSucceeded, DeleteClipartGroupFailed);
        }

        #endregion

        // GET /clipart/{group-id}/name
        #region GetClipartGroupName

        public event EventHandler<ClipartGroupDataEventArgs> GetClipartGroupNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartGroupNameFailed;

        public Request GetClipartGroupName_Request(Int64 groupID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = ClipartGroupNameUri(groupID)
            };
        }

        public void GetClipartGroupName(Int64 groupID)
        {
            if (!IsGetClipartGroupNamePending(groupID))
            {
                Request request = GetClipartGroupName_Request(groupID);

                request.Completed += OnGetClipartGroupNameResponse;

                Request(request);
            }
        }

        public bool IsGetClipartGroupNamePending(Int64 groupID)
        {
            Request request = GetClipartGroupName_Request(groupID);

            return IsPending(request);
        }

        public void CancelGetClipartGroupName(Int64 groupID)
        {
            Request request = GetClipartGroupName_Request(groupID);

            Cancel(request);
        }

        public void OnGetClipartGroupNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetClipartGroupNameSucceeded, GetClipartGroupNameFailed);
        }

        #endregion

        // PUT /clipart/{group-id}/name
        #region SetClipartGroupName

        public event EventHandler<ClipartGroupDataEventArgs> SetClipartGroupNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetClipartGroupNameFailed;

        public Request SetClipartGroupName_Request(Int64 groupID, string newName = "")
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = ClipartGroupNameUri(groupID),
                ContentType = "text/xml",
                ContentObject = new ClipartGroupDataModel { ID = groupID, Name = newName }
            };
        }

        public void SetClipartGroupName(Int64 groupID, string newName)
        {
            CancelSetClipartGroupName(groupID);

            Request request = SetClipartGroupName_Request(groupID, newName);

            request.Completed += OnSetClipartGroupNameResponse;

            Request(request);
        }

        public bool IsSetClipartGroupNamePending(Int64 groupID)
        {
            Request request = SetClipartGroupName_Request(groupID);

            return IsPending(request);
        }

        public void CancelSetClipartGroupName(Int64 groupID)
        {
            Request request = SetClipartGroupName_Request(groupID);

            Cancel(request);
        }

        public void OnSetClipartGroupNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetClipartGroupNameSucceeded, SetClipartGroupNameFailed);
        }

        #endregion

        // GET /clipart/{group-id}/items
        #region GetClipartItems

        public event EventHandler<ClipartGroupDataEventArgs> GetClipartItemsSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartItemsFailed;

        public Request GetClipartItems_Request(Int64 groupID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = ClipartGroupItemsUri(groupID)
            };
        }

        public void GetClipartItems(Int64 groupID)
        {
            CancelGetClipartItems(groupID);

            Request request = GetClipartItems_Request(groupID);

            request.Completed += OnGetClipartItemsResponse;

            Request(request);
        }

        public bool IsGetClipartItemsPending(Int64 groupID)
        {
            Request request = GetClipartItems_Request(groupID);

            return IsPending(request);
        }

        public void CancelGetClipartItems(Int64 groupID)
        {
            Request request = GetClipartItems_Request(groupID);

            Cancel(request);
        }

        protected void OnGetClipartItemsResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetClipartItemsSucceeded, GetClipartItemsFailed);
        }

        #endregion

        // PATCH /clipart/{group-id}/items
        #region UpdateClipartItems

        public event EventHandler<ClipartGroupDataEventArgs> UpdateClipartItemsSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateClipartItemsFailed;

        public Request UpdateClipartItems_Request(Int64 groupID = 0, ClipartDataModel[] items = null, bool deleteMissingItems = false, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetUpdateParameters(ClipartGroupItemsUri(groupID), deleteMissingItems, purge),
                ContentType = "text/xml",
                ContentObject = new ClipartGroupDataModel { Items = items }
            };
        }

        public void UpdateClipartItems(Int64 groupID, ClipartDataModel[] items, bool deleteMissingItems = false, bool purge = false)
        {
            Request request = UpdateClipartItems_Request(groupID, items, deleteMissingItems, purge);

            request.Completed += OnUpdateClipartItemsResponse;

            Request(request);
        }

        protected void OnUpdateClipartItemsResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateClipartItemsSucceeded, UpdateClipartItemsFailed);
        }

        public IAsyncResult BeginUpdateClipartItems(Int64 groupID, ClipartDataModel[] items, bool deleteMissingItems = false, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateClipartItems_Request(groupID, items, deleteMissingItems, purge);

            return BeginRequest(request, callback, state);
        }

        public ClipartGroupDataModel EndUpdateClipartItems(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<ClipartGroupDataModel>();
        }

        #endregion

        // GET /clipart/{group-id}/items/metadata
        #region GetClipartItemsMetadata

        public event EventHandler<ClipartGroupDataEventArgs> GetClipartItemsMetadataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartItemsMetadataFailed;

        public Request GetClipartItemsMetadata_Request(Int64 groupID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = ClipartItemsMetadataUri(groupID)
            };
        }

        public void GetClipartItemsMetadata(Int64 groupID)
        {
            if (!IsGetClipartItemsMetadataPending(groupID))
            {
                Request request = GetClipartItemsMetadata_Request(groupID);

                request.Completed += OnGetClipartItemsMetadataResponse;

                Request(request);
            }
        }

        public bool IsGetClipartItemsMetadataPending(Int64 groupID)
        {
            Request request = GetClipartItemsMetadata_Request(groupID);

            return IsPending(request);
        }

        protected void OnGetClipartItemsMetadataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetClipartItemsMetadataSucceeded, GetClipartItemsMetadataFailed);
        }

        public IAsyncResult BeginGetClipartItemsMetadata(Int64 groupID, AsyncCallback callback = null, object state = null)
        {
            Request request = GetClipartItemsMetadata_Request(groupID);

            return BeginRequest(request, callback, state);
        }

        public ClipartGroupDataModel EndGetClipartItemsMetadata(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<ClipartGroupDataModel>();
        }

        #endregion

        // PATCH /clipart/{group-id}/items/metadata
        #region UpdateClipartItemsMetadata

        public event EventHandler<ClipartGroupDataEventArgs> UpdateClipartItemsMetadataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateClipartItemsMetadataFailed;

        public Request UpdateClipartItemsMetadata_Request(Int64 groupID = 0, ClipartDataModel[] items = null, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetPurge(ClipartItemsMetadataUri(groupID), purge),
                ContentType = "text/xml",
                ContentObject = new ClipartGroupDataModel { Items = items }
            };
        }

        public void UpdateClipartItemsMetadata(Int64 groupID, ClipartDataModel[] items, bool purge = false)
        {
            Request request = UpdateClipartItemsMetadata_Request(groupID, items, purge);

            request.Completed += OnUpdateClipartItemsMetadataResponse;

            Request(request);
        }

        protected void OnUpdateClipartItemsMetadataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateClipartItemsMetadataSucceeded, UpdateClipartItemsMetadataFailed);
        }

        public IAsyncResult BeginUpdateClipartItemsMetadata(Int64 groupID, ClipartDataModel[] items, bool purge = false, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateClipartItemsMetadata_Request(groupID, items, purge);

            return BeginRequest(request, callback, state);
        }

        public ClipartGroupDataModel EndUpdateClipartItemsMetadata(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<ClipartGroupDataModel>();
        }

        #endregion

        // POST /clipart/{group-id}/items
        #region CreateClipart

        public event EventHandler<NoteDataEventArgs> CreateClipartSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> CreateClipartFailed;

        public Request CreateClipart_Request(Int64 groupID, Int64 clipartID, string name = null, string data = null)
        {
            return new Request
            {
                Method = RequestMethod.Post,
                Uri = ClipartGroupItemsUri(groupID),
                ContentType = "text/xml",
                ContentObject = new ClipartDataModel { GroupID = groupID, ID = clipartID, Name = name, Data = data }
            };
        }

        public void CreateClipart(Int64 groupID, Int64 clipartID, string name = null, string data = null)
        {
            Request request = CreateClipart_Request(groupID, clipartID, name, data);

            request.Completed += OnCreateClipartResponse;

            Request(request);
        }

        protected void OnCreateClipartResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, CreateClipartSucceeded, CreateClipartFailed);
        }

        #endregion

        // DELETE /clipart/{group-id}/items/{clipart-id}
        #region DeleteClipart

        public event EventHandler<ClipartDataEventArgs> DeleteClipartSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> DeleteClipartFailed;

        public Request DeleteClipart_Request(Int64 groupID, Int64 clipartID)
        {
            return new Request
            {
                Method = RequestMethod.Delete,
                Uri = ClipartUri(groupID, clipartID)
            };
        }

        public void DeleteClipart(Int64 groupID, Int64 clipartID)
        {
            Request request = DeleteClipart_Request(groupID, clipartID);

            request.Completed += OnDeleteClipartResponse;

            Request(request);
        }

        public void OnDeleteClipartResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, DeleteClipartSucceeded, DeleteClipartFailed);
        }

        #endregion

        // GET /clipart/{group-id}/items/{clipart-id}
        #region GetClipart

        public event EventHandler<ClipartDataEventArgs> GetClipartSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartFailed;

        public Request GetClipart_Request(Int64 groupID, Int64 clipartID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = ClipartUri(groupID, clipartID)
            };
        }

        public void GetClipart(Int64 groupID, Int64 clipartID)
        {
            if (!IsGetClipartPending(groupID, clipartID))
            {
                Request request = GetClipart_Request(groupID, clipartID);

                request.Completed += OnGetClipartResponse;

                Request(request);
            }
        }

        public bool IsGetClipartPending(Int64 groupID, Int64 clipartID)
        {
            Request request = GetClipart_Request(groupID, clipartID);

            return IsPending(request);
        }

        public void OnGetClipartResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetClipartSucceeded, GetClipartFailed);
        }

        public IAsyncResult BeginGetClipart(Int64 groupID, Int64 clipartID, AsyncCallback callback = null, object state = null)
        {
            Request request = GetClipart_Request(groupID, clipartID);

            return BeginRequest(request, callback, state);
        }

        public ClipartDataModel EndGetClipart(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<ClipartDataModel>();
        }

        #endregion

        // PATCH /clipart/{group-id}/items/{clipart-id}
        #region UpdateClipart

        public event EventHandler<ClipartDataEventArgs> UpdateClipartSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> UpdateClipartFailed;

        public Request UpdateClipart_Request(Int64 groupID, Int64 clipartID, ClipartDataModel clipart = null, bool purge = false)
        {
            return new Request
            {
                Method = RequestMethod.Patch,
                Uri = SetPurge(ClipartUri(groupID, clipartID), purge),
                ContentType = "text/xml",
                ContentObject = clipart
            };
        }

        public void UpdateClipart(Int64 groupID, Int64 clipartID, ClipartDataModel clipart, bool purge = false)
        {
            CancelUpdateClipart(groupID, clipartID);

            Request request = UpdateClipart_Request(groupID, clipartID, (ClipartDataModel)clipart.Clone(), purge);

            request.Completed += OnUpdateClipartResponse;

            Request(request);
        }

        public void CancelUpdateClipart(Int64 groupID, Int64 clipartID)
        {
            Request request = UpdateClipart_Request(groupID, clipartID);

            Cancel(request);
        }

        protected void OnUpdateClipartResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, UpdateClipartSucceeded, UpdateClipartFailed);
        }

        public IAsyncResult BeginUpdateClipart(Int64 groupID, Int64 clipartID, ClipartDataModel clipart, bool purge, AsyncCallback callback = null, object state = null)
        {
            Request request = UpdateClipart_Request(groupID, clipartID, clipart, purge);

            return BeginRequest(request, callback, state);
        }

        public ClipartDataModel EndUpdateClipart(IAsyncResult asyncResult)
        {
            var response = EndRequest(asyncResult);

            return response.ContentOrDefault<ClipartDataModel>();
        }

        #endregion

        // GET /clipart/{group-id}/items/{item-id}/name
        #region GetClipartName

        public event EventHandler<ClipartDataEventArgs> GetClipartNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartNameFailed;

        public Request GetClipartName_Request(Int64 groupID, Int64 clipartID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = ClipartNameUri(groupID, clipartID)
            };
        }

        public void GetClipartName(Int64 groupID, Int64 clipartID)
        {
            if (!IsGetClipartNamePending(groupID, clipartID))
            {
                Request request = GetClipartName_Request(groupID, clipartID);

                request.Completed += OnGetClipartNameResponse;

                Request(request);
            }
        }

        public bool IsGetClipartNamePending(Int64 groupID, Int64 clipartID)
        {
            Request request = GetClipartName_Request(groupID, clipartID);

            return IsPending(request);
        }

        public void OnGetClipartNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetClipartNameSucceeded, GetClipartNameFailed);
        }

        #endregion

        // PUT /clipart/{group-id}/items/{item-id}/name
        #region SetClipartName

        public event EventHandler<ClipartDataEventArgs> SetClipartNameSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetClipartNameFailed;

        public Request SetClipartName_Request(Int64 groupID, Int64 clipartID, string newName = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = ClipartNameUri(groupID, clipartID),
                ContentType = "text/plain",
                ContentObject = newName
            };
        }

        public void SetClipartName(Int64 groupID, Int64 clipartID, string newTitle)
        {
            CancelSetClipartName(groupID, clipartID);

            Request request = SetClipartName_Request(groupID, clipartID, newTitle);

            request.Completed += OnSetClipartNameResponse;

            Request(request);
        }

        public void CancelSetClipartName(Int64 groupID, Int64 clipartID)
        {
            Request request = SetClipartName_Request(groupID, clipartID);

            Cancel(request);
        }

        public void OnSetClipartNameResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetClipartNameSucceeded, SetClipartNameFailed);
        }

        #endregion

        // GET /clipart/{group-id}/items/{item-id}/data
        #region GetClipartData

        public event EventHandler<ClipartDataEventArgs> GetClipartDataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> GetClipartDataFailed;

        public Request GetClipartData_Request(Int64 groupID, Int64 clipartID)
        {
            return new Request
            {
                Method = RequestMethod.Get,
                Uri = ClipartDataUri(groupID, clipartID)
            };
        }

        public void GetClipartData(Int64 groupID, Int64 clipartID)
        {
            if (!IsGetClipartDataPending(groupID, clipartID))
            {
                Request request = GetClipartData_Request(groupID, clipartID);

                request.Completed += OnGetClipartDataResponse;

                Request(request);
            }
        }

        public bool IsGetClipartDataPending(Int64 groupID, Int64 clipartID)
        {
            Request request = GetClipartData_Request(groupID, clipartID);

            return IsPending(request);
        }

        public void OnGetClipartDataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, GetClipartDataSucceeded, GetClipartDataFailed);
        }
        #endregion

        // PUT /clipart/{group-id}/items/{item-id}/data
        #region SetClipartData

        public event EventHandler<ClipartDataEventArgs> SetClipartDataSucceeded;
        public event EventHandler<NoteClientErrorEventArgs> SetClipartDataFailed;

        public Request SetClipartData_Request(Int64 groupID, Int64 clipartID, string newData = null)
        {
            return new Request
            {
                Method = RequestMethod.Put,
                Uri = ClipartDataUri(groupID, clipartID),
                ContentType = "text/plain",
                ContentObject = newData
            };
        }

        public void SetClipartData(Int64 groupID, Int64 clipartID, string newData)
        {
            CancelSetClipartData(groupID, clipartID);

            Request request = SetClipartData_Request(groupID, clipartID, newData);

            request.Completed += OnSetClipartDataResponse;

            Request(request);
        }

        public void CancelSetClipartData(Int64 groupID, Int64 clipartID)
        {
            Request request = SetClipartData_Request(groupID, clipartID);

            Cancel(request);
        }

        public void OnSetClipartDataResponse(Request request, Response response)
        {
            DefaultResponseHandler(request, response, SetClipartDataSucceeded, SetClipartDataFailed);
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            VerifyProperty(propertyName);

            var handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        [Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            if (propertyName == "Item[]")
            {
                propertyName = "Item";
            }
            Type type = this.GetType();
            if (type.GetProperty(propertyName) == null)
            {
                string msg = String.Format("{0} is not a public property of {1}", propertyName, type.FullName);
                Debug.Fail(msg);
            }
        }

        #endregion

        #region Implementation

        #region NoteClientThread

        /// <summary>
        /// Thread initialization callback
        /// </summary>
        protected override bool OnStart(object arg)
        {
            try
            {
                // Create a credential cache and add the given credentials (if any)

                Credentials = new CredentialCache();

                if (Username != null || Password != null)
                {
                    Credentials.Add(Uri, "Basic", new NetworkCredential(Username, Password));
                }

                // Create a server object if connecting to a local repository

                if (Uri != null && Server == null)
                {
                    Server = NoteServer.Create(Uri, Username, Password, AutoCreate);
                }

                return true;
            }
            catch (Exception e)
            {
                // Pass any exceptions on to the originating call to Open()
                _OpenException = e;
                return false;
            }
        }

        /// <summary>
        /// Request handler
        /// </summary>
        protected override Response OnRequest(Request request)
        {
            // Execute the request

            Response response;

            if (request.Uri.Scheme == "data")
            {
                // Handle requests for embedded data
                response = OnDataRequest(request);
            }
            else if (Server != null)
            {
                // Handle requests for local data
                response = Server.Request(request);
            }
            else
            {
                // Handle requests for remote data
                response = OnWebRequest(request);
            }

            return response;
        }

        /// <summary>
        /// Thread deinitialization callback
        /// </summary>
        protected override void OnStop()
        {
            // Complete all local requests prior to exit

            if (Server != null)
            {
                base.OnStop();
            }

            if (Server != null)
            {
                Server.Dispose();
            }
        }

        #endregion

        #region Embedded Data

        /// <summary>
        /// Process a request containing data embedded in the URL itself
        /// 
        /// Such URLs are commonly found in HTML documents where they are used
        /// to embed images
        /// </summary>
        protected virtual Response OnDataRequest(Request request)
        {
            DataUri uri;

            if (DataUri.TryParse(request.Uri.ToString(), out uri))
            {
                return Response.Ok(uri.DataBytes, uri.MediaType);
            }
            else
            {
                return Response.NotFound();
            }
        }

        #endregion

        #region Web Requests

        protected CredentialCache Credentials { get; set; }

        /// <summary>
        /// Web request handler
        /// </summary>
        protected virtual Response OnWebRequest(Request request)
        {
            System.Net.ServicePointManager.Expect100Continue = false;

            // Initialize HTTP request

            WebRequest webRequest;

            try
            {
                webRequest = WebRequest.Create(request.Uri);
            }
            catch (Exception e)
            {
                return Response.InternalServerError(e.Message);
            }

            webRequest.Method = request.Method.ToString().ToUpper();
            webRequest.Credentials = Credentials;
            webRequest.Proxy = null;

            byte[] requestContent = null;
            if (request.ContentString != null)
            {
                requestContent = Encoding.UTF8.GetBytes(request.ContentString);
                webRequest.ContentLength = requestContent.Length;
                webRequest.ContentType = request.ContentType;
            }

            // Bypass pre-authentication logic

            webRequest.PreAuthenticate = true;

            var modules = AuthenticationManager.RegisteredModules;
            while (modules.MoveNext())
            {
                var module = (IAuthenticationModule)modules.Current;
                if (module.AuthenticationType.Equals("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    var type = typeof(AuthenticationManager);
                    var name = "BindModule";
                    var attrs = BindingFlags.NonPublic | BindingFlags.Static;
                    var parms = new object[] { webRequest.RequestUri, new Authorization(null), module };
                    var mi = type.GetMethod(name, attrs);
                    mi.Invoke(null, parms);
                }
            }

            IAsyncResult asyncResult = null;
            WaitHandle[] waitHandles = new WaitHandle[] { _StopEvent, null };

            // Send request content if any

            if (requestContent != null)
            {
                // Connect to server

                // Trace.WriteLine("Connecting to {0}..." + ((HttpWebRequest)webRequest).ServicePoint.Address);

                // Note: BeginGetRequestStream() blocks until a TCP connection is established with the server

                try
                {
                    asyncResult = webRequest.BeginGetRequestStream(null, null);

                    waitHandles[1] = asyncResult.AsyncWaitHandle;
                    if (WaitHandle.WaitAny(waitHandles) == 0)
                    {
                        throw new Exception("The request was cancelled");
                    }
                }
                catch (Exception e)
                {
                    webRequest.Abort();
                    return Response.InternalServerError(e.Message);
                }

                // Write request content

                try
                {
                    using (var requestStream = webRequest.EndGetRequestStream(asyncResult))
                    {
                        asyncResult = requestStream.BeginWrite(requestContent, 0, requestContent.Length, null, null);
                        
                        waitHandles[1] = asyncResult.AsyncWaitHandle;
                        if (WaitHandle.WaitAny(waitHandles) == 0)
                        {
                            throw new Exception("The request was cancelled");
                        }

                        requestStream.EndWrite(asyncResult);
                    }
                }
                catch (Exception e)
                {
                    webRequest.Abort();
                    return Response.InternalServerError(e.Message);
                }
            }

            // Wait for response

            WebResponse webResponse = null;
            try
            {
                asyncResult = webRequest.BeginGetResponse(null, null);

                waitHandles[1] = asyncResult.AsyncWaitHandle;
                if (WaitHandle.WaitAny(waitHandles) == 0)
                {
                    throw new Exception("The request was cancelled");
                }

                webResponse = webRequest.EndGetResponse(asyncResult);
            }
            catch (Exception e)
            {
                webRequest.Abort();
                return Response.InternalServerError(e.Message);
            }

            // Get status code

            var statusCode = HttpStatusCode.OK;
            var statusDescription = "OK";
            
            var httpResponse = webResponse as HttpWebResponse;
            if (httpResponse != null)
            {
                statusCode = httpResponse.StatusCode;
                statusDescription = httpResponse.StatusDescription;
            }

            // Determine encoding

            Encoding encoding = Encoding.UTF8;

            if (httpResponse != null && !String.IsNullOrEmpty(httpResponse.CharacterSet))
            {
                encoding = Encoding.GetEncoding(httpResponse.CharacterSet);
            }

            // Read content

            var responseData = new System.IO.MemoryStream();
            try
            {
                using (var responseStream = webResponse.GetResponseStream())
                {   
                    var responseBuffer = new byte[8196];
                    int readResult;
                    do
                    {
                        asyncResult = responseStream.BeginRead(
                            responseBuffer,
                            0,
                            responseBuffer.Length,
                            null,
                            null
                        );

                        waitHandles[1] = asyncResult.AsyncWaitHandle;
                        if (WaitHandle.WaitAny(waitHandles) == 0)
                        {
                            throw new Exception("The request was cancelled");
                        }

                        readResult = responseStream.EndRead(asyncResult);
                        if (readResult > 0)
                        {
                            responseData.Write(responseBuffer, 0, readResult);
                        }

                    } while (readResult > 0);
                }
            }
            catch (Exception e)
            {
                webResponse.Close();
                return Response.InternalServerError(e.Message);
            }

            byte[] content = responseData.ToArray();
            string contentString = new string(encoding.GetChars(content));

            // Re-package the response in an instance of our own "Response" class

            Response response = new Response
            {
                StatusCode = statusCode,
                StatusDescription = statusDescription,
                ContentObject = content,
                ContentString = contentString,
                ContentType = webResponse.ContentType
            };
            response.Headers.Add(webResponse.Headers);

            return response;
        }

        #endregion

        #region Response Handlers

        /// <summary>
        /// Default error handler
        /// 
        /// This is raised when an error occurs that is not handled by any other event handler
        /// </summary>
        public event EventHandler<NoteClientErrorEventArgs> DefaultErrorHandler;

        /// <summary>
        /// Raise the default error event
        /// </summary>
        private void RaiseDefaultErrorHandler(Request request, Response response)
        {
            var callback = DefaultErrorHandler;
            if (callback != null)
            {
                callback(this, new NoteClientErrorEventArgs(request, response));
            }
        }

        /// <summary>
        /// Process a response
        /// </summary>
        /// <param name="request">The associated request</param>
        /// <param name="response">The received response</param>
        /// <param name="succeeded">The delegate to invoke on success</param>
        /// <param name="failed">The delegate to invoke on failure</param>
        void DefaultResponseHandler(Request request, Response response, Delegate succeeded, EventHandler<NoteClientErrorEventArgs> failed)
        {
            if (response.IsSuccess)
            {
                OnDefaultSuccessHandler(request, response, succeeded);
            }
            else
            {
                OnDefaultErrorHandler(request, response, failed);
            }
        }

        /// <summary>
        /// Process a successful response.
        /// 
        /// This method creates an appropriate EventArgs object based on the given
        /// callback type and the response contents, then passes that to the callback
        /// </summary>
        /// <param name="request">The associated request</param>
        /// <param name="response">The received response</param>
        /// <param name="callback">The response handler to be invoked</param>
        void OnDefaultSuccessHandler(Request request, Response response, Delegate callback)
        {
            if (callback != null)
            {
                if (callback is EventHandler<RepositoryDataEventArgs>)
                {
                    var e = new RepositoryDataEventArgs 
                    { 
                        Repository = response.ContentOrDefault<RepositoryDataModel>()
                    };

                    ((EventHandler<RepositoryDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<NotebookDataEventArgs>)
                {
                    var e = new NotebookDataEventArgs 
                    {
                        Notebook = response.ContentOrDefault<NotebookDataModel>()
                    };

                    e.Notebook.ID = ReverseTranslateNotebookID(e.Notebook.ID);

                    ((EventHandler<NotebookDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<SearchResultsDataEventArgs>)
                {
                    var e = new SearchResultsDataEventArgs
                    {
                        SearchResults = response.ContentOrDefault<SearchResultsDataModel>()
                    };

                    e.SearchResults.NotebookID = ReverseTranslateNotebookID(e.SearchResults.NotebookID);

                    ((EventHandler<SearchResultsDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<NoteDataEventArgs>)
                {
                    var e = new NoteDataEventArgs
                    {
                        Note = response.ContentOrDefault<NoteDataModel>()
                    };

                    e.Note.NotebookID = ReverseTranslateNotebookID(e.Note.NotebookID);

                    ((EventHandler<NoteDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<FileDataEventArgs>)
                {
                    var e = new FileDataEventArgs
                    {
                        File = response.ContentOrDefault<FileDataModel>()
                    };

                    e.File.NotebookID = ReverseTranslateNotebookID(e.File.NotebookID);

                    ((EventHandler<FileDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<CategoryDataEventArgs>)
                {
                    var e = new CategoryDataEventArgs
                    {
                        Category = response.ContentOrDefault<CategoryDataModel>()
                    };

                    e.Category.NotebookID = ReverseTranslateNotebookID(e.Category.NotebookID);

                    ((EventHandler<CategoryDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<ClipartGroupDataEventArgs>)
                {
                    var e = new ClipartGroupDataEventArgs
                    {
                        ClipartGroup = response.ContentOrDefault<ClipartGroupDataModel>()
                    };

                    ((EventHandler<ClipartGroupDataEventArgs>)callback)(this, e);
                }
                else if (callback is EventHandler<ClipartDataEventArgs>)
                {
                    var e = new ClipartDataEventArgs
                    {
                        Clipart = response.ContentOrDefault<ClipartDataModel>()
                    };

                    ((EventHandler<ClipartDataEventArgs>)callback)(this, e);
                }
                else
                {
                    Debug.WriteLine("Unhandled response for request: {0} {1}", request.Method, request.Uri);
                }
            }

        }

        /// <summary>
        /// Process an error response
        /// </summary>
        void OnDefaultErrorHandler(Request request, Response response, EventHandler<NoteClientErrorEventArgs> callback)
        {
            if (callback != null)
            {
                callback(this, new NoteClientErrorEventArgs(request, response));
            }
            else
            {
                RaiseDefaultErrorHandler(request, response);
            }
        }

        Int64 ReverseTranslateNotebookID(Int64 notebookID)
        {
            string notebookURI = "/notebooks/" + notebookID;
            var result = UriMap.FirstOrDefault((entry) => entry.Value == notebookURI);
            if (result.Key != null)
            {
                string[] segments = result.Key.Split('/');
                notebookID = Int64.Parse(segments[2]);
            }
            return notebookID;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the current date/time
        /// </summary>
        public static DateTime Now
        {
            get { return DateTime.UtcNow; }
        }

        #endregion

        #endregion Implementation
    }

    public class StatusChangedEventArgs : EventArgs
    {
        public string NewStatus { get; set; }
    }
}
