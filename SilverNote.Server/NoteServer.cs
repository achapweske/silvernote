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
using System.Security;
using System.Diagnostics;
using SilverNote.Data.Store;
using SilverNote.Data.Models;

namespace SilverNote.Server
{
    public class NoteServer : IDisposable
    {
        #region Static Members

        static NoteServer()
        {
            MaxNotes = int.MaxValue;
        }

        public static int MaxNotes { get; set; }

        /// <summary>
        /// Create a NoteServer capable of servicing the given repository
        /// 
        /// If the repository does not exist and autoCreate = false, this will return null
        /// </summary>
        /// <param name="uri">URL of the target repository</param>
        /// <param name="username">Username used to login to the repository</param>
        /// <param name="password">Password used to login to the repository</param>
        /// <param name="autoCreate">true to automatically create the repository if it does not already exist</param>
        /// <returns></returns>
        public static NoteServer Create(Uri uri, string username, SecureString password, bool autoCreate)
        {
            // Attempt to create the appropriate NoteStore

            NoteStore store = NoteStore.Create(uri, username, password, autoCreate);

            // Attach the store to a new NoteServer

            if (store != null)
            {
                return new NoteServer { BaseUri = uri, Store = store };
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Constructors

        public NoteServer()
        {
            BaseUri = new Uri("/", UriKind.Relative);
        }

        #endregion

        #region Properties

        /// <summary>
        /// URI of the repository represented by this NoteServer
        /// </summary>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// NoteStore used to perform the actual repository I/O
        /// </summary>
        public NoteStore Store { get; set; }

        #endregion

        #region Requests

        /// <summary>
        /// Routes incoming requests to the appropriate handler
        /// </summary>
        private RequestRouter Router { get; set; }

        /// <summary>
        /// Request entry point
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Response Request(Request request)
        {
            // Route request to the appropriate handler

            try
            {
                if (Router == null)
                {
                    Router = new RequestRouter(BaseUri);
                    // Build a routing table based on our methods' RequestTarget attribute
                    Router.AddRoutes(this);
                }

                return Router.Route(request);
            }

            // We don't allow I/O to cause crashes - all exceptions are converted to responses

            catch (NotImplementedException e)
            {
                return Response.NotImplemented(e.Message);
            }
            catch (NotFoundException e)
            {
                return Response.NotFound(e.Message);
            }
            catch (NoteStoreException e)
            {
                return Response.InternalServerError(e.Message);
            }
            catch (Exception e)
            {
                return Response.InternalServerError(e.Message + "\n\n" + e.StackTrace);
            }
        }

        #endregion

        #region Repository

        [RequestTarget(RequestMethod.Get, "")]
        protected Response GetRepository(Request request)
        {
            var repository = Store.GetRepository();

            if (repository == null)
            {
                return Response.NotFound("Unable to retrieve repository");
            }

            if (repository.Notebooks != null)
            {
                var notebooks = repository.Notebooks;

                for (int i = 0; i < notebooks.Length; i++)
                {
                    if (notebooks[i].ID == repository.SelectedNotebookID)
                    {
                        var selectedNotebook = Store.GetNotebook(notebooks[i].ID);
                        if (selectedNotebook != null)
                        {
                            notebooks[i] = selectedNotebook;
                        }
                    }
                }
            }

            return Response.Ok(repository, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "")]
        protected Response UpdateRepository(Request request)
        {
            bool purge = request.Params.GetBool("purge");

            var repository = request.Content<RepositoryDataModel>();
            if (repository == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.UpdateRepository(repository, purge))
            {
                return Response.Forbidden("Unable to update repository");
            }

            var result = Store.GetRepository();
            if (result == null)
            {
                return Response.NotFound("Unable to retrieve repository");
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "selected-notebook")]
        protected Response GetSelectedNotebook(Request request)
        {
            var repository = new RepositoryDataModel
            {
                SelectedNotebookID = Store.GetSelectedNotebook()
            };

            if (repository.SelectedNotebookID == -1)
            {
                return Response.NotFound("Unable to retrieve selected notebook");
            }

            return Response.Ok(repository, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "selected-notebook")]
        protected Response SetSelectedNotebook(Request request)
        {
            var repository = request.Content<RepositoryDataModel>();
            if (repository == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.SetSelectedNotebook(repository.SelectedNotebookID))
            {
                return Response.Forbidden("Unable to set selected notebook");
            }

            var result = new RepositoryDataModel
            {
                SelectedNotebookID = Store.GetSelectedNotebook()
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "password")]
        protected Response GetPasswordStatus(Request request)
        {
            var repository = new RepositoryDataModel
            {
                HasPassword = Store.HasPassword
            };

            return Response.Ok(repository, "text/xml");
        }

        [RequestTarget(RequestMethod.Post, "password")]
        protected Response ChangePassword(Request request)
        {
            var data = request.Content<ChangedPasswordDataModel>();
            if (data == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.ChangePassword(data.OldPassword, data.NewPassword))
            {
                return Response.Forbidden("The old password you supplied is not correct");
            }

            return GetPasswordStatus(request);
        }

        #endregion

        #region Notebooks

        public Uri NotebookUri(Int64 notebookID)
        {
            return new Uri(Router.Prefix, String.Format("notebooks/{0}", notebookID));
        }

        [RequestTarget(RequestMethod.Get, "notebooks")]
        protected Response GetNotebooks(Request request)
        {
            var notebooks = Store.GetNotebooks();
            if (notebooks != null)
            {
                var result = new RepositoryDataModel { Notebooks = notebooks };

                return Response.Ok(result, "text/xml");
            }
            else
            {
                return Response.NotFound("Unable to retrieve notebooks");
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks")]
        protected Response SetNotebooks(Request request)
        {
            var repository = request.Content<RepositoryDataModel>();

            if (!Store.SetNotebooks(repository.Notebooks))
            {
                return Response.Forbidden("Unable to set notebooks");
            }

            var result = new RepositoryDataModel { Notebooks = Store.GetNotebooks() };
            if (result.Notebooks == null)
            {
                return Response.NotFound("Unable to retrieve notebooks");
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Patch, "notebooks")]
        protected Response UpdateNotebooks(Request request)
        {
            bool purge = request.Params.GetBool("purge");
            bool deleteMissingItems = request.Params.GetBool("deleteMissingItems");

            var repository = request.Content<RepositoryDataModel>();

            if (repository != null && repository.Notebooks != null)
            {
                if (!Store.UpdateNotebooks(repository.Notebooks, deleteMissingItems, purge))
                {
                    return Response.Forbidden("Unable to update notebooks");
                }
            }

            var result = new RepositoryDataModel { Notebooks = Store.GetNotebooks() };
            if (result.Notebooks == null)
            {
                return Response.NotFound("Unable to retrieve notebooks");
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Post, "notebooks")]
        protected Response CreateNotebook(Request request)
        {
            var notebook = request.Content<NotebookDataModel>();
            if (notebook == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            Int64 notebookID = Store.CreateNotebook(notebook);
            if (notebookID == 0)
            {
                return Response.Forbidden("Unable to create notebook");
            }

            Uri uri = NotebookUri(notebookID);

            var result = Store.GetNotebook(notebookID);
            if (result == null)
            {
                return Response.NotFound("Unable to retrieve notebook");
            }

            return Response.Created(uri, result);
        }

        [RequestTarget(RequestMethod.Delete, "notebooks/{id}")]
        protected Response DeleteNotebook(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            bool purge = request.Params.GetBool("purge");

            if (Store.DeleteNotebook(notebookID, purge))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to delete notebook");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{id}")]
        protected Response GetNotebook(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var notebook = Store.GetNotebook(notebookID);
            if (notebook != null)
            {
                return Response.Ok(notebook, "text/xml");
            }
            else
            {
                return Response.NotFound("Unable to retrieve notebook");
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{id}")]
        protected Response UpdateNotebook(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var notebook = request.Content<NotebookDataModel>();
            if (notebook == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.UpdateNotebook(notebookID, notebook))
            {
                return Response.NotFound("Unable to update notebook");
            }

            var result = Store.GetNotebook(notebookID);
            if (result == null)
            {
                return Response.NotFound("Unable to retrieve notebook");
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{id}/name")]
        protected Response GetNotebookName(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            string name = Store.GetNotebookName(notebookID);
            if (name == null)
            {
                return Response.NotFound("Unable to retrieve notebook name");
            }

            NotebookDataModel result = new NotebookDataModel
            {
                ID = notebookID,
                Name = name
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{id}/name")]
        protected Response SetNotebookName(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var notebook = request.Content<NotebookDataModel>();
            if (notebook == null)
            {
                return Response.Forbidden("Missing or invalid notebook data");
            }

            if (Store.SetNotebookName(notebookID, notebook.Name, notebook.NameModifiedAt))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to set notebook name");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{id}/open-notes")]
        protected Response GetOpenNotes(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            Int64[] openNotes = Store.GetOpenNotes(notebookID);
            if (openNotes != null)
            {
                NotebookDataModel result = new NotebookDataModel
                {
                    ID = notebookID,
                    OpenNotes = openNotes
                };

                return Response.Ok(result, "text/xml");
            }
            else
            {
                return Response.NotFound("Unable to retrieve open notes");
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{id}/open-notes")]
        protected Response SetOpenNotes(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var notebook = request.Content<NotebookDataModel>();
            if (notebook == null)
            {
                return Response.Forbidden("Missing or invalid notebook data");
            }

            if (Store.SetOpenNotes(notebookID, notebook.OpenNotes))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to set open notes");
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/open-notes/{note-id}")]
        protected Response OpenNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            if (Store.OpenNote(notebookID, noteID))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to open note");
            }
        }

        [RequestTarget(RequestMethod.Delete, "notebooks/{notebook-id}/open-notes/{note-id}")]
        protected Response CloseNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            if (Store.CloseNote(notebookID, noteID))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to close note");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/selected-note")]
        protected Response GetSelectedNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            var result = new NotebookDataModel
            {
                ID = notebookID,
                SelectedNoteID = Store.GetSelectedNote(notebookID),
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/selected-note")]
        protected Response SetSelectedNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            var notebook = request.Content<NotebookDataModel>();
            if (notebook != null)
            {
                if (!Store.SetSelectedNote(notebookID, notebook.SelectedNoteID))
                {
                    return Response.NotFound();
                }

                return Response.Ok(notebook, "text/xml");
            }
            else
            {
                return Response.Forbidden("Unable to set selected note");
            }
        }

        #endregion

        #region Notes

        public Uri NoteUri(Int64 notebookID, Int64 noteID)
        {
            return new Uri(Router.Prefix, String.Format("/notebooks/{0}/notes/{1}", notebookID, noteID));
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{id}/notes")]
        protected Response FindNotes(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            string phrase = request.Params.GetString("search");

            DateTime createdAfter = DateTime.MinValue;
            DateTime createdBefore = DateTime.MaxValue;
            string created = request.Params.GetString("created");
            if (created != null)
            {
                FromRange(created, out createdAfter, out createdBefore);
            }

            DateTime modifiedAfter = DateTime.MinValue;
            DateTime modifiedBefore = DateTime.MaxValue;
            string modified = request.Params.GetString("modified");
            if (modified != null)
            {
                FromRange(modified, out modifiedAfter, out modifiedBefore);
            }

            DateTime viewedAfter = DateTime.MinValue;
            DateTime viewedBefore = DateTime.MaxValue;
            string viewed = request.Params.GetString("viewed");
            if (viewed != null)
            {
                FromRange(viewed, out viewedAfter, out viewedBefore);
            }

            NoteStore.NoteSort sort = NoteStore.NoteSort.ViewedAt;
            string sortString = request.Params.GetString("sort");
            if (sortString != null)
            {
                switch (sortString.ToLower())
                {
                    case "title":
                        sort = NoteStore.NoteSort.Title;
                        break;
                    case "createdat":
                        sort = NoteStore.NoteSort.CreatedAt;
                        break;
                    case "modifiedat":
                        sort = NoteStore.NoteSort.ModifiedAt;
                        break;
                    case "viewedat":
                        sort = NoteStore.NoteSort.ViewedAt;
                        break;
                }
            }

            NoteStore.NoteOrder order = NoteStore.NoteOrder.Descending;
            string orderString = request.Params.GetString("order");
            if (orderString != null)
            {
                switch (orderString.ToLower())
                {
                    case "asc":
                        order = NoteStore.NoteOrder.Ascending;
                        break;
                    case "desc":
                        order = NoteStore.NoteOrder.Descending;
                        break;
                }
            }

            int offset = request.Params.GetInt("offset", 0);
            int limit = request.Params.GetInt("limit", -1);
            bool returnText = request.Params.GetString("return") == "text";

            var result = Store.FindNotes(notebookID, phrase, createdAfter, createdBefore, modifiedAfter, modifiedBefore, viewedAfter, viewedBefore, sort, order, offset, limit, returnText);
            if (result != null)
            {
                return Response.Ok(result, "text/xml");
            }
            else
            {
                return Response.NotFound("Unable to search notes");
            }
        }

        static void FromRange(string str, out DateTime first, out DateTime second)
        {
            var items = str.Split('-');

            if (items.Length >= 1)
            {
                if (!DateTime.TryParse(items[0], out first))
                {
                    first = DateTime.MinValue;
                }
            }
            else
            {
                first = DateTime.MinValue;
            }

            if (items.Length >= 2)
            {
                if (!DateTime.TryParse(items[1], out second))
                {
                    second = DateTime.MaxValue;
                }
            }
            else
            {
                second = DateTime.MaxValue;
            }
        }

        [RequestTarget(RequestMethod.Patch, "notebooks/{id}/notes")]
        protected Response UpdateNotes(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            bool deleteMissingItems = request.Params.GetBool("deleteMissingItems");
            bool purge = request.Params.GetBool("purge");

            var notebook = request.Content<NotebookDataModel>();
            if (notebook == null)
            {
                return Response.Forbidden("Missing or invalid notebook data");
            }

            var notes = notebook.Notes;
            if (notes == null)
            {
                notes = new NoteDataModel[] { };
            }

            HashFilter(notes);

            var hashes = ExtractRemoteHashes(notes);

            if (!Store.UpdateNotes(notebookID, notes, deleteMissingItems, purge))
            {
                return Response.NotFound("Unable to update notes");
            }

            notes = Store.GetNotes(notebookID);
            if (notes == null)
            {
                return Response.NotFound("Unable to retrieve notes");
            }

            SetRemoteHashes(notes, hashes);

            var result = new NotebookDataModel 
            { 
                ID = notebookID, 
                Notes = notes 
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{id}/notes/metadata")]
        protected Response GetNotesMetadata(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var notes = Store.GetNotesMetadata(notebookID);
            if (notes == null)
            {
                return Response.NotFound("Unable to retrieve notes");
            }

            var result = new NotebookDataModel 
            {
                ID = notebookID,
                Notes = notes 
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Patch, "notebooks/{id}/notes/metadata")]
        protected Response UpdateNotesMetadata(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            bool deleteMissingItems = request.Params.GetBool("deleteMissingItems");
            bool purge = request.Params.GetBool("purge");

            var notebook = request.Content<NotebookDataModel>();
            if (notebook == null)
            {
                return Response.Forbidden("Missing or invalid notebook data");
            }

            var notes = notebook.Notes;
            if (notes != null)
            {
                HashFilter(notes);

                if (!Store.UpdateNotes(notebookID, notes, deleteMissingItems, purge))
                {
                    return Response.NotFound("Unable to update notes");
                }
            }

            notes = Store.GetNotesMetadata(notebookID);
            if (notes == null)
            {
                return Response.NotFound("Unable to retrieve notes");
            }

            var result = new NotebookDataModel 
            { 
                ID = notebookID, 
                Notes = notes 
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Post, "notebooks/{id}/notes")]
        protected Response CreateNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var note = request.Content<NoteDataModel>();
            if (note == null)
            {
                return Response.Forbidden("Missing or invalid note data");
            }
            note.NotebookID = notebookID;

            try
            {
                int count = Store.GetNoteCount(notebookID);
                if (count >= MaxNotes)
                {
                    string message = "You are currently restricted to " + MaxNotes + " notes per notebook. For unlimited notes please upgrade to SilverNote Premium Edition.";
                    return Response.Forbidden(message);
                }
            }
            catch (NotImplementedException)
            {
                // Store doesn't implement GetNoteCount()
            }

            Int64 noteID = Store.CreateNote(notebookID, note);
            if (noteID == 0)
            {
                return Response.Forbidden("Unable to create note");
            }

            Uri uri = NoteUri(notebookID, noteID);

            note.ID = noteID;
            note.NotebookID = notebookID;

            return Response.Created(uri, note);
        }

        [RequestTarget(RequestMethod.Delete, "notebooks/{notebook-id}/notes/{note-id}")]
        protected Response DeleteNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            bool purge = request.Params.GetBool("purge");

            if (Store.DeleteNote(notebookID, noteID, purge))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to delete note");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}")]
        protected Response GetNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var note = Store.GetNote(notebookID, noteID);
            if (note != null)
            {
                note.Files = Store.GetFiles(notebookID, noteID);

                return Response.Ok(note, "text/xml");
            }
            else
            {
                string message = String.Format("Unable to read note {0}", noteID);
                return Response.NotFound(message);
            }
        }

        [RequestTarget(RequestMethod.Patch, "notebooks/{notebook-id}/notes/{note-id}")]
        protected Response UpdateNote(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var note = request.Content<NoteDataModel>();
            if (note == null)
            {
                return Response.Forbidden("Content missing or invalid");
            }
            note.NotebookID = notebookID;
            note.ID = noteID;

            HashFilter(note);

            // If content is updated, plaintext needs to be updated as well
            if (!String.IsNullOrWhiteSpace(note.Content) &&
                String.IsNullOrWhiteSpace(note.Text))
            {
                try
                {
                    var html = DOM.DOMFactory.CreateHTMLDocument();
                    html.Open();
                    html.Write(note.Content);
                    html.Close();
                    note.Text = html.Body.InnerText;
                }
                catch
                {
                    Debug.WriteLine("Error: Unable to extract document text");
                }
            }

            if (!Store.UpdateNote(notebookID, noteID, note, autoCreate: true))
            {
                return Response.NotFound("Unable to update note");
            }

            if (note.Files != null)
            {
                if (!Store.SetFiles(notebookID, noteID, note.Files, true))
                {
                    return Response.NotFound("Unable to update note resources");
                }
            }

            var result = Store.GetNote(notebookID, noteID);
            if (result == null)
            {
                return Response.NotFound("Unable to retrieve note");
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/metadata")]
        protected Response GetNoteMetadata(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var note = Store.GetNoteMetadata(notebookID, noteID);
            if (note != null)
            {
                return Response.Ok(note, "text/xml");
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/title")]
        protected Response GetNoteTitle(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string title = Store.GetNoteTitle(notebookID, noteID);
            if (title == null)
            {
                return Response.NotFound("Unable to retrieve note title");
            }

            var result = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                Title = title
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/title")]
        protected Response SetNoteTitle(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            if (request.ContentType != "text/plain")
            {
                return Response.UnsupportedMediaType();
            }

            string newTitle = request.ContentObject as string;
            if (newTitle == null)
            {
                newTitle = "";
            }

            if (Store.SetNoteTitle(notebookID, noteID, newTitle))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to set note title");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/content")]
        protected Response GetNoteContent(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string content = Store.GetNoteContent(notebookID, noteID);
            if (content != null)
            {
                return Response.Ok(content, "text/html");
            }
            else
            {
                return Response.NotFound("Unable to retrieve note content");
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/content")]
        protected Response SetNoteContent(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string newContent = request.ContentString;
            if (newContent == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            DateTime modifiedAt = ExtractRevisedTime(newContent);
            if (modifiedAt == default(DateTime))
            {
                modifiedAt = Store.Now;
            }

            if (!Store.SetNoteContent(notebookID, noteID, newContent, modifiedAt))
            {
                return Response.NotFound("Unable to save note");
            }

            return Response.NoContent();
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/text")]
        protected Response SetNoteText(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string newText = request.ContentString;
            if (newText == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.SetNoteText(notebookID, noteID, newText))
            {
                return Response.NotFound("Unable to index note");
            }

            return Response.NoContent();
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/created_at")]
        protected Response GetNoteCreatedAt(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            DateTime createdAt = Store.GetNoteCreatedAt(notebookID, noteID);
            if (createdAt == null)
            {
                return Response.NotFound("Unable to retrieve note created time");
            }

            var result = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                CreatedAt = createdAt
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/modified_at")]
        protected Response GetNoteModifiedAt(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            DateTime modifiedAt = Store.GetNoteModifiedAt(notebookID, noteID);
            if (modifiedAt == null)
            {
                return Response.NotFound("Unable to retrieve note modified time");
            }

            var result = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                ModifiedAt = modifiedAt
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/viewed_at")]
        protected Response GetNoteViewedAt(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            DateTime viewedAt = Store.GetNoteViewedAt(notebookID, noteID);
            if (viewedAt == null)
            {
                return Response.NotFound("Unable to retrieve note viewed time");
            }

            var result = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                ViewedAt = viewedAt
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/viewed_at")]
        protected Response SetNoteViewedAt(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var note = request.Content<NoteDataModel>();
            if (note == null || note.ViewedAt == default(DateTime))
            {
                return Response.Forbidden("Missing or invalid note data");
            }

            if (Store.SetNoteViewedAt(notebookID, noteID, note.ViewedAt))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to save note timestamp");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/categories")]
        protected Response GetNoteCategories(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var categories = Store.GetNoteCategories(notebookID, noteID);
            if (categories == null)
            {
                return Response.NotFound("Unable to retrieve note categories");
            }

            var result = new NoteDataModel
            {
                ID = noteID,
                NotebookID = notebookID,
                Categories = categories
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/categories/{category-id}")]
        protected Response AddNoteCategory(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            if (Store.AddNoteCategory(notebookID, noteID, categoryID))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to add note category");
            }
        }

        [RequestTarget(RequestMethod.Delete, "notebooks/{notebook-id}/notes/{note-id}/categories/{category-id}")]
        protected Response RemoveNoteCategory(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            if (Store.RemoveNoteCategory(notebookID, noteID, categoryID))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to remove note category");
            }
        }

        private static void HashFilter(IEnumerable<NoteDataModel> notes)
        {
            foreach (var note in notes)
            {
                HashFilter(note);
            }
        }

        private static void HashFilter(NoteDataModel note)
        {
            note.RemoteHash = note.Hash;

            if (note.Content != null)
            {
                note.LastSentHash = "*";
                note.LastRecvHash = note.Hash;
            }
            else
            {
                var tempHash = note.LastSentHash;
                note.LastSentHash = note.LastRecvHash;
                note.LastRecvHash = tempHash;
            }

            note.Hash = null;
        }

        private static Dictionary<Int64, string> ExtractRemoteHashes(IEnumerable<NoteDataModel> notes)
        {
            var result = new Dictionary<Int64, string>();

            foreach (var note in notes)
            {
                result[note.ID] = note.RemoteHash;
                note.RemoteHash = null;
            }

            return result;
        }

        private static void SetRemoteHashes(IEnumerable<NoteDataModel> notes, Dictionary<Int64, string> remoteHashes)
        {
            foreach (var note in notes)
            {
                string remoteHash;
                if (remoteHashes.TryGetValue(note.ID, out remoteHash))
                {
                    note.RemoteHash = remoteHash;
                }
            }
        }

        #endregion

        #region Files

        public Uri FileUri(Int64 notebookID, Int64 noteID, string fileName)
        {
            return new Uri(Router.Prefix, String.Format("/notebooks/{0}/notes/{1}/files/{2}", notebookID, noteID, HttpUtility.UrlEncode(fileName)));
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/files")]
        protected Response SetFiles(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var note = request.Content<NoteDataModel>();
            if (note == null)
            {
                return Response.Forbidden("Missing or invalid file data");
            }

            if (!Store.SetFiles(notebookID, noteID, note.Files, true))
            {
                return Response.NotFound("Unable to write files");
            }

            return Response.NoContent();
        }

        [RequestTarget(RequestMethod.Post, "notebooks/{notebook-id}/notes/{note-id}/files")]
        protected Response CreateFile(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            var file = request.Content<FileDataModel>();
            if (file == null)
            {
                return Response.Forbidden();
            }
            file.NotebookID = notebookID;
            file.NoteID = noteID;

            string fileName = Store.CreateFile(notebookID, noteID, file);
            if (fileName != null)
            {
                Uri uri = FileUri(notebookID, noteID, fileName);

                var created = new FileDataModel()
                {
                    NotebookID = notebookID,
                    NoteID = noteID,
                    Name = fileName
                };

                return Response.Created(uri, created);
            }
            else
            {
                return Response.Forbidden("Unable to create file \"" + file.Name + "\"");
            }
        }

        [RequestTarget(RequestMethod.Delete, "notebooks/{notebook-id}/notes/{note-id}/files/{file-name}")]
        protected Response DeleteFile(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string fileName = request.Params.GetString("file-name");
            if (fileName == null)
            {
                return InvalidIDResponse("file", request.Params["file-name"]);
            }

            if (Store.DeleteFile(notebookID, noteID, fileName))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound("Unable to delete file \"" + fileName + "\"");
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/notes/{note-id}/files/{file-name}")]
        protected Response GetFile(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string fileName = request.Params.GetString("file-name");
            if (fileName == null)
            {
                return InvalidIDResponse("file", request.Params["file-name"]);
            }

            var data = Store.GetFileData(notebookID, noteID, fileName);
            if (data != null)
            {
                return Response.Ok(data, null);
            }
            else
            {
                return Response.NotFound("Unable to read file \"" + fileName + "\"");
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/notes/{note-id}/files/{file-name}")]
        protected Response SetFile(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string fileName = request.Params.GetString("file-name");
            if (fileName == null)
            {
                return InvalidIDResponse("file", request.Params["file-name"]);
            }

            var file = new FileDataModel 
            { 
                NotebookID = notebookID, 
                NoteID = noteID, 
                Name = fileName,
                Data = request.ContentObject as byte[]
            };

            if (file.Data == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.SetFile(notebookID, noteID, fileName, file))
            {
                return Response.NotFound("File not found: \"" + fileName + "\"");
            }

            return Response.NoContent();
        }

        [RequestTarget(RequestMethod.Patch, "notebooks/{notebook-id}/notes/{note-id}/files/{file-name}")]
        protected Response UpdateFile(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 noteID = request.Params.GetInt64("note-id");
            if (noteID == 0)
            {
                return InvalidIDResponse("note", request.Params["note-id"]);
            }

            string fileName = request.Params.GetString("file-name");
            if (fileName == null)
            {
                return InvalidIDResponse("file", request.Params["file-name"]);
            }

            var file = new FileDataModel
            {
                NotebookID = notebookID,
                NoteID = noteID,
                Name = fileName,
                Data = request.ContentObject as byte[]
            };

            if (file.Data == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            if (!Store.UpdateFile(notebookID, noteID, fileName, file))
            {
                return Response.NotFound("Unable to update file: \"" + fileName + "\"");
            }

            return Response.NoContent();
        }

        #endregion

        #region Categories

        public Uri CategoryUri(Int64 notebookID, Int64 categoryID)
        {
            return new Uri(Router.Prefix, String.Format("/notebooks/{0}/categories/{1}", notebookID, categoryID));
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{id}/categories")]
        protected Response GetCategories(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var categories = Store.GetCategories(notebookID);
            if (categories != null)
            {
                var notebook = new NotebookDataModel
                {
                    ID = notebookID,
                    Categories = categories
                };

                return Response.Ok(notebook, "text/xml");
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{id}/categories")]
        protected Response UpdateCategories(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            bool purge = request.Params.GetBool("purge");

            var notebook = request.Content<NotebookDataModel>();

            if (notebook != null && notebook.Categories != null)
            {
                Store.UpdateCategories(notebookID, notebook.Categories, purge);
            }

            var result = new NotebookDataModel 
            {
                ID = notebookID,
                Categories = Store.GetCategories(notebookID) 
            };

            if (result.Categories == null)
            {
                return Response.NotFound();
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Post, "notebooks/{id}/categories")]
        protected Response CreateCategory(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["id"]);
            }

            var category = request.Content<CategoryDataModel>();
            if (category == null)
            {
                return Response.Forbidden();
            }
            category.NotebookID = notebookID;

            Int64 categoryID = Store.CreateCategory(notebookID, category);
            if (categoryID != 0)
            {
                Uri uri = CategoryUri(notebookID, categoryID);

                category.ID = categoryID;
                category.NotebookID = notebookID;

                return Response.Created(uri, category);
            }
            else
            {
                return Response.Forbidden();
            }
        }

        [RequestTarget(RequestMethod.Delete, "notebooks/{notebook-id}/categories/{category-id}")]
        protected Response DeleteCategory(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            bool purge = request.Params.GetBool("purge");

            if (Store.DeleteCategory(notebookID, categoryID, purge))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/categories/{category-id}")]
        protected Response GetCategory(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("note", request.Params["category-id"]);
            }

            var category = Store.GetCategory(notebookID, categoryID);
            if (category != null)
            {
                return Response.Ok(category, "text/xml");
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/categories/{category-id}/name")]
        protected Response GetCategoryName(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            string name = Store.GetCategoryName(notebookID, categoryID);
            if (name == null)
            {
                return Response.NotFound("Unable to retrieve category name");
            }

            var category = new CategoryDataModel
            {
                ID = categoryID,
                NotebookID = notebookID,
                Name = name
            };

            return Response.Ok(category, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/categories/{category-id}/name")]
        protected Response SetCategoryName(Request request)
        {
            // Get category ID

            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            // Extract new category name

            var category = request.Content<CategoryDataModel>();
            if (category == null)
            {
                return Response.Forbidden();
            }

            // Set new category name

            if (Store.SetCategoryName(notebookID, categoryID, category.Name, category.NameModifiedAt))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/categories/{category-id}/parent")]
        protected Response GetCategoryParent(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            Int64 parentID = Store.GetCategoryParent(notebookID, categoryID);
            if (parentID == 0)
            {
                return Response.NotFound("Unable to retrieve category parent");
            }

            var category = new CategoryDataModel
            {
                ID = categoryID,
                NotebookID = notebookID,
                ParentID = parentID
            };

            return Response.Ok(category, "text/xml");
        }

        [RequestTarget(RequestMethod.Put, "notebooks/{notebook-id}/categories/{category-id}/parent")]
        protected Response SetCategoryParent(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");
            if (categoryID == 0)
            {
                return InvalidIDResponse("category", request.Params["category-id"]);
            }

            var category = request.Content<CategoryDataModel>();
            if (category == null)
            {
                return Response.Forbidden();
            }

            if (Store.SetCategoryParent(notebookID, categoryID, category.ParentID, category.ParentIDModifiedAt))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "notebooks/{notebook-id}/categories/{category-id}/children")]
        protected Response GetCategoryChildren(Request request)
        {
            Int64 notebookID = request.Params.GetInt64("notebook-id");
            if (notebookID == 0)
            {
                return InvalidIDResponse("notebook", request.Params["notebook-id"]);
            }

            Int64 categoryID = request.Params.GetInt64("category-id");

            var children = Store.GetCategoryChildren(notebookID, categoryID);
            if (children == null)
            {
                return Response.NotFound("Unable to retrieve category children");
            }

            var category = new CategoryDataModel
            {
                ID = categoryID,
                NotebookID = notebookID,
                Children = children
            };

            return Response.Ok(category, "text/xml");
        }

        #endregion

        #region Clipart

        public Uri ClipartGroupUri(Int64 groupID)
        {
            return new Uri(Router.Prefix, String.Format("clipart/{0}", groupID));
        }

        public Uri ClipartUri(Int64 groupID, Int64 clipartID)
        {
            return new Uri(Router.Prefix, String.Format("clipart/{0}/items/{1}", groupID, clipartID));
        }

        [RequestTarget(RequestMethod.Get, "clipart")]
        protected Response GetClipartGroups(Request request)
        {
            var groups = Store.GetClipartGroups();
            if (groups != null)
            {
                var result = new RepositoryDataModel { ClipartGroups = groups };

                return Response.Ok(result, "text/xml");
            }
            else
            {
                return Response.NotFound("Unable to retrieve clipart groups");
            }
        }

        [RequestTarget(RequestMethod.Patch, "clipart")]
        protected Response UpdateClipartGroups(Request request)
        {
            bool deleteMissingItems = request.Params.GetBool("deleteMissingItems");
            bool purge = request.Params.GetBool("purge");

            var repository = request.Content<RepositoryDataModel>();

            if (repository != null && repository.ClipartGroups != null)
            {
                Store.UpdateClipartGroups(repository.ClipartGroups, deleteMissingItems, purge);
            }

            var result = new RepositoryDataModel { ClipartGroups = Store.GetClipartGroups() };
            if (result.ClipartGroups == null)
            {
                return Response.NotFound();
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Post, "clipart")]
        protected Response CreateClipartGroup(Request request)
        {
            var group = request.Content<ClipartGroupDataModel>();
            if (group == null)
            {
                return Response.Forbidden("Missing or invalid content");
            }

            Int64 groupID = Store.CreateClipartGroup(group);
            if (groupID == 0)
            {
                return Response.NotFound();
            }

            Uri uri = ClipartGroupUri(groupID);

            var result = Store.GetClipartGroup(groupID);
            if (result == null)
            {
                return Response.NotFound();
            }

            return Response.Created(uri, result);
        }

        [RequestTarget(RequestMethod.Delete, "clipart/{group-id}")]
        protected Response DeleteClipartGroup(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            if (Store.DeleteClipartGroup(groupID))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "clipart/{group-id}/name")]
        protected Response GetClipartGroupName(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            string name = Store.GetClipartGroupName(groupID);
            if (name == null)
            {
                return Response.NotFound("Unable to retrieve clipart group name");
            }

            var result = new ClipartGroupDataModel
            {
                ID = groupID,
                Name = name
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "clipart/{group-id}/items")]
        protected Response GetClipartItems(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            var items = Store.GetClipartItems(groupID);
            if (items == null)
            {
                return Response.NotFound("Unable to retrieve clipart for group " + groupID);
            }

            var result = new ClipartGroupDataModel
            {
                ID = groupID,
                Items = items
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Patch, "clipart/{group-id}/items")]
        protected Response UpdateClipartItems(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("group", request.Params["group-id"]);
            }

            bool deleteMissingItems = request.Params.GetBool("deleteMissingItems");
            bool purge = request.Params.GetBool("purge");

            var group = request.Content<ClipartGroupDataModel>();
            if (group == null)
            {
                return Response.Forbidden("Missing or invalid clipart data");
            }

            var items = group.Items;
            if (items == null)
            {
                items = new ClipartDataModel[] { };
            }

            HashFilter(items);

            var hashes = ExtractRemoteHashes(items);

            if (!Store.UpdateClipartItems(groupID, items, deleteMissingItems, purge))
            {
                return Response.NotFound("Unable to update notes");
            }

            items = Store.GetClipartItems(groupID);
            if (items == null)
            {
                return Response.NotFound("Unable to retrieve notes");
            }

            SetRemoteHashes(items, hashes);

            var result = new ClipartGroupDataModel
            {
                ID = groupID,
                Items = items
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "clipart/{group-id}/items/metadata")]
        protected Response GetClipartItemsMetadata(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("group", request.Params["group-id"]);
            }

            var items = Store.GetClipartItemsMetadata(groupID);
            if (items == null)
            {
                return Response.NotFound("Unable to retrieve clipart metadata");
            }

            var result = new ClipartGroupDataModel
            {
                ID = groupID,
                Items = items
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Patch, "clipart/{group-id}/items/metadata")]
        protected Response UpdateClipartItemsMetadata(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("group", request.Params["group-id"]);
            }

            bool deleteMissingItems = request.Params.GetBool("deleteMissingItems");
            bool purge = request.Params.GetBool("purge");

            var group = request.Content<ClipartGroupDataModel>();
            if (group == null)
            {
                return Response.Forbidden("Missing or invalid notebook data");
            }

            var items = group.Items;
            if (items != null)
            {
                HashFilter(items);

                if (!Store.UpdateClipartItems(groupID, items, deleteMissingItems, purge))
                {
                    return Response.NotFound("Unable to update notes");
                }
            }

            items = Store.GetClipartItemsMetadata(groupID);
            if (items == null)
            {
                return Response.NotFound("Unable to retrieve notes");
            }

            var result = new ClipartGroupDataModel
            {
                ID = groupID,
                Items = items
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Post, "clipart/{group-id}/items")]
        protected Response CreateClipart(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            var clipart = request.Content<ClipartDataModel>();
            if (clipart == null)
            {
                return Response.Forbidden();
            }

            Int64 clipartID = Store.CreateClipart(groupID, clipart);
            if (clipartID == 0)
            {
                return Response.Forbidden();
            }

            Uri uri = ClipartUri(groupID, clipartID);

            var result = Store.GetClipart(groupID, clipartID);
            if (result == null)
            {
                return Response.NotFound();
            }

            return Response.Created(uri, result);
        }

        [RequestTarget(RequestMethod.Delete, "clipart/{group-id}/items/{clipart-id}")]
        protected Response DeleteClipart(Request request)
        {
            bool purge = request.Params.GetBool("purge", false);

            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            Int64 clipartID = request.Params.GetInt64("clipart-id");
            if (clipartID == 0)
            {
                return InvalidIDResponse("clipart", request.Params["clipart-id"]);
            }

            if (Store.DeleteClipart(groupID, clipartID, purge))
            {
                return Response.NoContent();
            }
            else
            {
                return Response.NotFound();
            }
        }

        [RequestTarget(RequestMethod.Get, "clipart/{group-id}/items/{clipart-id}")]
        protected Response GetClipart(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("group", request.Params["group-id"]);
            }

            Int64 clipartID = request.Params.GetInt64("clipart-id");
            if (clipartID == 0)
            {
                return InvalidIDResponse("clipart", request.Params["clipart-id"]);
            }

            var clipart = Store.GetClipart(groupID, clipartID);
            if (clipart != null)
            {
                return Response.Ok(clipart, "text/xml");
            }
            else
            {
                return Response.NotFound("Unable to retrieve clipart");
            }
        }

        [RequestTarget(RequestMethod.Patch, "clipart/{group-id}/items/{clipart-id}")]
        protected Response UpdateClipart(Request request)
        {
            bool purge = request.Params.GetBool("purge");

            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("group", request.Params["group-id"]);
            }

            Int64 clipartID = request.Params.GetInt64("clipart-id");
            if (clipartID == 0)
            {
                return InvalidIDResponse("clipart", request.Params["clipart-id"]);
            }

            var clipart = request.Content<ClipartDataModel>();
            if (clipart == null)
            {
                return Response.Forbidden("Content missing or invalid");
            }

            HashFilter(clipart);

            if (!Store.UpdateClipart(groupID, clipartID, clipart, purge))
            {
                return Response.NotFound();
            }

            var result = Store.GetClipart(groupID, clipartID);
            if (result == null)
            {
                return Response.NotFound();
            }

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "clipart/{group-id}/items/{clipart-id}/name")]
        protected Response GetClipartName(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            Int64 clipartID = request.Params.GetInt64("clipart-id");
            if (clipartID == 0)
            {
                return InvalidIDResponse("clipart", request.Params["clipart-id"]);
            }

            string name = Store.GetClipartName(groupID, clipartID);
            if (name == null)
            {
                return Response.NotFound("Unable to retrieve clipart name");
            }

            var result = new ClipartDataModel
            {
                ID = clipartID,
                GroupID = groupID,
                Name = name
            };

            return Response.Ok(result, "text/xml");
        }

        [RequestTarget(RequestMethod.Get, "clipart/{group-id}/items/{clipart-id}/data")]
        protected Response GetClipartData(Request request)
        {
            Int64 groupID = request.Params.GetInt64("group-id");
            if (groupID == 0)
            {
                return InvalidIDResponse("clipart group", request.Params["group-id"]);
            }

            Int64 clipartID = request.Params.GetInt64("clipart-id");
            if (clipartID == 0)
            {
                return InvalidIDResponse("clipart", request.Params["clipart-id"]);
            }

            string data = Store.GetClipartData(groupID, clipartID);
            if (data == null)
            {
                return Response.NotFound("Unable to retrieve clipart data");
            }

            var result = new ClipartDataModel
            {
                ID = clipartID,
                GroupID = groupID,
                Data = data
            };

            return Response.Ok(result, "text/xml");
        }

        private static void HashFilter(IEnumerable<ClipartDataModel> items)
        {
            foreach (var item in items)
            {
                HashFilter(item);
            }
        }

        private static void HashFilter(ClipartDataModel item)
        {
            item.RemoteHash = item.Hash;

            if (item.Data != null)
            {
                item.LastSentHash = "*";
                item.LastRecvHash = item.Hash;
            }
            else
            {
                var tempHash = item.LastSentHash;
                item.LastSentHash = item.LastRecvHash;
                item.LastRecvHash = tempHash;
            }

            item.Hash = null;
        }

        private static Dictionary<Int64, string> ExtractRemoteHashes(IEnumerable<ClipartDataModel> items)
        {
            var result = new Dictionary<Int64, string>();

            foreach (var item in items)
            {
                result[item.ID] = item.RemoteHash;
                item.RemoteHash = null;
            }

            return result;
        }

        private static void SetRemoteHashes(IEnumerable<ClipartDataModel> items, Dictionary<Int64, string> remoteHashes)
        {
            foreach (var item in items)
            {
                string remoteHash;
                if (remoteHashes.TryGetValue(item.ID, out remoteHash))
                {
                    item.RemoteHash = remoteHash;
                }
            }
        }

        #endregion

        #region Helpers

        protected static Response InvalidIDResponse(string name, string value)
        {
            string message = String.Format("Unable to locate the requested {0}: ", name);
            if (value == null)
            {
                message += "ID not specified";
            }
            else
            {
                message += String.Format("\"{0}\" is not a valid ID", value);
            }
            return Response.NotFound(message);
        }

        protected static DateTime ExtractRevisedTime(string html)
        {
            string revisedString = ExtractHtmlMeta(html, "revised");

            DateTime revisedTime;
            if (DateTime.TryParse(revisedString, out revisedTime))
            {
                return revisedTime;
            }
            else
            {
                return default(DateTime);
            }
        }

        protected static string ExtractHtmlMeta(string html, string name)
        {
            int startIndex, endIndex = 0;
            while ((startIndex = html.IndexOf("<meta", endIndex)) != -1)
            {
                endIndex = html.IndexOf(">", startIndex);
                if (endIndex == -1)
                {
                    return null;
                }

                string meta = html.Substring(startIndex, endIndex - startIndex + 1);
                if (ExtractHtmlMetaAttr(meta, "name") == name)
                {
                    return ExtractHtmlMetaAttr(meta, "content");
                }
            }

            return null;
        }

        public static string ExtractHtmlMetaAttr(string meta, string attr)
        {
            attr += "=\"";

            int startIndex = meta.IndexOf(attr);
            if (startIndex == -1)
            {
                return null;
            }
            startIndex += attr.Length;

            int endIndex = meta.IndexOf('\"', startIndex);
            if (endIndex == -1)
            {
                return null;
            }

            return meta.Substring(startIndex, endIndex - startIndex);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (Store != null)
            {
                Store.Dispose();
                Store = null;
            }
        }

        #endregion

    }
}
