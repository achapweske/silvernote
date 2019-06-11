/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using SilverNote.Data.Models;
using System.Collections.ObjectModel;
using SilverNote.Client;

namespace SilverNote.Models
{
    public class NoteModel : ModelBase, ICloneable
    {
        #region Fields

        readonly RepositoryModel _Repository;
        readonly NotebookModel _Notebook;
        readonly Int64 _ID;
        string _Url;
        bool _NeedUrl = true;
        string _Title;
        string _Content;
        string _Text;
        Dictionary<string, FileModel> _AllFiles;
        ObservableCollection<CategoryModel> _Categories;
        bool _CategoriesPending;
        DateTime _CreatedAt;
        DateTime _ModifiedAt;
        DateTime _ViewedAt;
        bool _NeedTitle;
        bool _TitlePending;
        bool _NeedContent;
        bool _NeedCategories;
        bool _NeedCreatedAt;
        bool _NeedModifiedAt;
        bool _NeedViewedAt;
        bool _IsDeleted;
        
        #endregion

        #region Constructors

        /// <summary>
        /// Create a note having the specified ID
        /// </summary>
        public NoteModel(RepositoryModel repository, NotebookModel notebook, Int64 id)
        {
            _Repository = repository;
            _Notebook = notebook;
            _ID = id;
            _AllFiles = new Dictionary<string, FileModel>();
            _Categories = new ObservableCollection<CategoryModel>();
            _NeedTitle = true;
            _NeedContent = true;
            _NeedCategories = true;
            _NeedCreatedAt = true;
            _NeedModifiedAt = true;
            _NeedViewedAt = true;
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return Notebook.Source; }
        }

        /// <summary>
        /// Get the repository to which this note belongs
        /// </summary>
        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        /// <summary>
        /// Get the notebook to which this note belongs
        /// </summary>
        public NotebookModel Notebook
        {
            get { return _Notebook; }
        }

        /// <summary>
        /// Get this note's ID
        /// </summary>
        public Int64 ID
        {
            get { return _ID; }
        }

        public Uri Uri
        {
            get { return Source.Client.NoteUri(Notebook.ID, this.ID); }
        }

        /// <summary>
        /// Get this note's URL
        /// </summary>
        public string Url
        {
            get { return GetUrl(true); }
        }

        /// <summary>
        /// Get/set this note's title
        /// </summary>
        public string Title
        {
            get { return GetTitle(true); }
            set { SetTitle(value, false); } // sync = false
        }

        /// <summary>
        /// Get/set this note's content
        /// </summary>
        public string Content
        {
            get { return GetContent(true); }
            set { SetContent(value, true); }
        }

        /// <summary>
        /// Set this note's text (for searching)
        /// </summary>
        public string Text
        {
            set { SetText(value, true); }
        }

        /// <summary>
        /// Get/set this note's embedded objects
        /// </summary>
        public IList<FileModel> Files
        {
            get { return GetFiles(true); }
            set { SetFiles(value, true); }
        }

        /// <summary>
        /// Get the categories to which this note belongs
        /// </summary>
        public IList<CategoryModel> Categories
        {
            get { return GetCategories(true); }
        }

        /// <summary>
        /// Get this note's created-at timestamp
        /// </summary>
        public DateTime CreatedAt
        {
            get { return GetCreatedAt(true); }
        }

        /// <summary>
        /// Get this note's modified-at timestamp
        /// </summary>
        public DateTime ModifiedAt
        {
            get { return GetModifiedAt(true); }
        }

        /// <summary>
        /// Get this note's viewed-at timestamp
        /// </summary>
        public DateTime ViewedAt
        {
            get { return GetViewedAt(true); }
            set { SetViewedAt(value, true); }
        }

        /// <summary>
        /// Determine if this note has been deleted
        /// </summary>
        public bool IsDeleted
        {
            get { return _IsDeleted; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create the note represented by this object
        /// </summary>
        public void Create(string title = "Untitled")
        {
            Create(title, true);
        }

        public void Delete()
        {
            Delete(true);
        }

        /// <summary>
        /// Add this note to the specified category
        /// </summary>
        public void AddToCategory(CategoryModel category)
        {
            AddToCategory(category, true);
        }

        /// <summary>
        /// Remove this note from the specified category
        /// </summary>
        /// <param name="category"></param>
        public void RemoveFromCategory(CategoryModel category)
        {
            RemoveFromCategory(category, true);
        }

        #endregion

        #region Implementation

        #region Create/Delete

        public void Create(string title, bool sync)
        {
            SetTitle(title, false);
            SetCreatedAt(NoteClient.Now, false);
            SetModifiedAt(default(DateTime), false);
            SetViewedAt(default(DateTime), false);

            if (sync)
            {
                Source.Client.CreateNote(Notebook.ID, this.ID);
            }

            Content = String.Format("<html><head><title>{0}</title></head><body><h1><span>{0}</span></h1><p></p></body></html>", title);
        }

        public void Delete(bool sync)
        {
            _IsDeleted = true;

            if (sync)
            {
                Source.Client.DeleteNote(Notebook.ID, this.ID, purge: true);

                // TODO: move this to class SearchModel
                foreach (var search in Searches)
                {
                    search.Results.Remove(this);
                }
            }

            RaisePropertyChanged("IsDeleted");
        }

        #endregion

        #region Url

        /// <summary>
        /// Get this note's URL
        /// </summary>
        /// <param name="sync">true to get the current value from the server as needed</param>
        /// <returns></returns>
        protected string GetUrl(bool sync)
        {
            if (sync && _NeedUrl)
            {
                _Url = Source.Client.NoteUri(Notebook.ID, ID).ToString();
            }

            return _Url;
        }

        /// <summary>
        /// Set this note's URL
        /// </summary>
        /// <param name="value">New url</param>
        /// <param name="sync">true to update the server with this new value</param>
        protected void SetUrl(string value, bool sync)
        {
            _NeedUrl = false;

            if (value != _Url)
            {
                _Url = value;

                RaisePropertyChanged("Url");
            }
        }
        #endregion

        #region Title

        /// <summary>
        /// Get this note's title
        /// </summary>
        /// <param name="sync">true to get the current value from the server as needed</param>
        /// <returns></returns>
        protected string GetTitle(bool sync)
        {
            if (sync && _NeedTitle && !_TitlePending)
            {
                _TitlePending = true;
                Source.Client.GetNoteTitle(Notebook.ID, ID);
            }

            return _Title;
        }

        /// <summary>
        /// Set this note's title
        /// </summary>
        /// <param name="value">New title</param>
        /// <param name="sync">true to update the server with this new value</param>
        protected void SetTitle(string value, bool sync)
        {
            _NeedTitle = false;
            _TitlePending = false;

            if (value != _Title)
            {
                _Title = value;
                
                if (sync)
                {
                    Source.Client.SetNoteTitle(Notebook.ID, ID, value);
                }

                RaisePropertyChanged("Title");
            }
        }

        #endregion

        #region Content

        /// <summary>
        /// Get this note's content
        /// </summary>
        /// <param name="sync">true to get the current value from the server as needed</param>
        /// <returns></returns>
        protected string GetContent(bool sync)
        {
            if (sync && _NeedContent)
            {
                Source.Client.GetNoteContent(Notebook.ID, ID);
            }
            return _Content;
        }

        /// <summary>
        /// Set this note's content
        /// </summary>
        /// <param name="value">New content</param>
        /// <param name="sync">true to update the server with this new value</param>
        protected void SetContent(string value, bool sync)
        {
            _NeedContent = false;

            if (value != _Content)
            {
                _Content = value;
                if (sync)
                {
                    Source.Client.SetNoteContent(Notebook.ID, ID, value);
                    SetModifiedAt(NoteClient.Now, false);
                }
                RaisePropertyChanged("Content");
            }
        }

        #endregion

        #region Text

        /// <summary>
        /// Set this note's text
        /// </summary>
        /// <param name="value">New text</param>
        /// <param name="sync">true to update the server with this new value</param>
        protected void SetText(string value, bool sync)
        {
            if (value != _Text)
            {
                _Text = value;

                if (sync)
                {
                    Source.Client.SetNoteText(Notebook.ID, ID, value);
                }
            }
        }

        #endregion

        #region Files

        /// <summary>
        /// Allocate a FileModel for an object embedded in this note
        /// </summary>
        private FileModel AllocFile(string filename)
        {
            var file = new FileModel(Repository, Notebook, this, filename);
            _AllFiles.Add(filename, file);
            return file;
        }

        /// <summary>
        /// Create an object embedded in this note
        /// </summary>
        public FileModel CreateFile(string filename)
        {
            var file = AllocFile(filename);
            file.Create();
            return file;
        }

        /// <summary>
        /// Get an object embedded in this note
        /// </summary>
        public FileModel GetFile(string filename, bool autoCreate = true)
        {
            FileModel result;
            if (_AllFiles.TryGetValue(filename, out result))
            {
                return result;
            }
            else if (autoCreate)
            {
                return AllocFile(filename);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Cached set of objects embedded in this note
        /// </summary>
        private List<FileModel> _Files = new List<FileModel>();

        /// <summary>
        /// true if _Files is NOT valid
        /// </summary>
        private bool _NeedFiles = true;

        /// <summary>
        /// Get the set of objects embedded in this note
        /// </summary>
        /// <param name="sync">true to retrieve this list from the server as needed</param>
        /// <returns></returns>
        protected FileModel[] GetFiles(bool sync)
        {
            if (sync && _NeedFiles)
            {
                // Repository.Client.GetFiles(Notebook.ID, this.ID);
            }

            return _Files.ToArray();
        }

        /// <summary>
        /// Set the list of objects embedded in this note
        /// </summary>
        /// <param name="files">New set of embedded objects</param>
        /// <param name="sync">true to update the server with this new value</param>
        protected void SetFiles(IList<FileModel> files, bool sync)
        {
            if (_NeedFiles || !files.SequenceEqual(_Files))
            {
                _NeedFiles = false;

                _Files = new List<FileModel>(files);
                
                if (sync)
                {
                    var fileNames = files.Select(file => file.Name).ToArray();
                    Source.Client.SetFiles(Notebook.ID, this.ID, fileNames);
                }

                RaisePropertyChanged("Files");
            }
        }

        #endregion

        #region Categories

        protected IList<CategoryModel> GetCategories(bool sync)
        {
            if (sync && _NeedCategories && !_CategoriesPending)
            {
                _CategoriesPending = true;
                Source.Client.GetNoteCategories(Notebook.ID, this.ID);
            }

            return _Categories;
        }

        protected void SetCategories(IList<CategoryModel> categories, bool sync)
        {
            _NeedCategories = false;
            _CategoriesPending = false;

            foreach (var category in _Categories.ToArray())
            {
                if (!categories.Contains(category))
                {
                    _Categories.Remove(category);
                    category.RemoveNote(this, sync);
                }
            }

            foreach (var category in categories)
            {
                if (!category.IsDeleted && !_Categories.Contains(category))
                {
                    _Categories.Add(category);
                    category.AddNote(this, sync);
                }
            }
        }

        /// <summary>
        /// Add this note to the given category
        /// </summary>
        /// <param name="category">Category to which this note is to be added</param>
        /// <param name="sync">true to update the server with the new category list</param>
        public void AddToCategory(CategoryModel category, bool sync)
        {
            if (!_Categories.Contains(category))
            {
                _Categories.Add(category);

                if (sync)
                {
                    Source.Client.AddNoteCategory(Notebook.ID, this.ID, category.ID);

                    if (_NeedCategories)
                    {
                        Source.Client.GetNoteCategories(Notebook.ID, ID);
                    }
                }

                category.AddNote(this, sync);
            }
        }

        /// <summary>
        /// Remove this note from the given category
        /// </summary>
        /// <param name="category">Category from which this note is to be removed</param>
        /// <param name="sync">true to update the server with the new category list</param>
        public void RemoveFromCategory(CategoryModel category, bool sync)
        {
            if (_Categories.Contains(category))
            {
                _Categories.Remove(category);

                if (sync)
                {
                    Source.Client.RemoveNoteCategory(Notebook.ID, ID, category.ID);

                    if (_NeedCategories)
                    {
                        Source.Client.GetNoteCategories(this.Notebook.ID, this.ID);
                    }
                }
                category.RemoveNote(this, sync);
            }
        }

        #endregion

        #region CreatedAt

        protected DateTime GetCreatedAt(bool sync)
        {
            if (sync && _NeedCreatedAt)
            {
                Source.Client.GetNoteCreatedAt(Notebook.ID, this.ID);
            }
            return _CreatedAt;
        }

        protected void SetCreatedAt(DateTime value, bool sync)
        {
            _NeedCreatedAt = false;

            if (value != _CreatedAt)
            {
                _CreatedAt = value;
                //if (sync) Repository.Client.SetNoteCreatedAt(Notebook.ID, this.ID, value);
                RaisePropertyChanged("CreatedAt");
            }
        }
        #endregion

        #region ModifiedAt

        protected DateTime GetModifiedAt(bool sync)
        {
            if (sync && _NeedModifiedAt)
            {
                Source.Client.GetNoteModifiedAt(Notebook.ID, this.ID);
            }
            return _ModifiedAt;
        }

        protected void SetModifiedAt(DateTime value, bool sync)
        {
            _NeedModifiedAt = false;

            if (value != _ModifiedAt)
            {
                _ModifiedAt = value;
                //if (sync) Repository.Client.SetNoteModifiedAt(Notebook.ID, this.ID, value);
                RaisePropertyChanged("ModifiedAt");
            }
        }
        #endregion

        #region ViewedAt

        public DateTime GetViewedAt(bool sync)
        {
            if (sync && _NeedViewedAt)
            {
                Source.Client.GetNoteViewedAt(Notebook.ID, this.ID);
            }
            return _ViewedAt;
        }

        public void SetViewedAt(DateTime value, bool sync)
        {
            _NeedViewedAt = false;

            if (value != _ViewedAt)
            {
                _ViewedAt = value;
                
                if (sync)
                {
                    Source.Client.SetNoteViewedAt(Notebook.ID, this.ID, value);
                }

                RaisePropertyChanged("ViewedAt");
            }
        }
        #endregion

        /// <summary>
        /// Get the set of all searches that might include this note in their results
        /// </summary>
        protected IEnumerable<SearchModel> Searches
        {
            get
            {
                yield return Notebook.Search;

                if (Categories.Count > 0)
                {
                    foreach (var category in Categories)
                    {
                        yield return category.Search;
                    }
                }
                else
                {
                    yield return Notebook.UncategorizedCategory.Search;
                }

                yield return Notebook.AllNotesCategory.Search;
            }
        }

        #endregion

        #region Data Model

        public void Update(NoteDataModel update)
        {
            if (update.IsDeleted)
            {
                Delete(false);
            }

            if (update.Url != null && _NeedUrl)
            {
                SetUrl(update.Url, false);
            }

            if (update.Title != null && String.IsNullOrEmpty(GetTitle(false)))
            {
                SetTitle(update.Title, false);
            }

            if (update.Content != null)
            {
                SetContent(update.Content, false);
            }

            if (update.Files != null)
            {
                UpdateFiles(update.Files);
            }

            if (update.Categories != null)
            {
                UpdateCategories(update.Categories);
            }

            if (update.CreatedAt != default(DateTime))
            {
                SetCreatedAt(update.CreatedAt, false);
            }

            if (update.ModifiedAt != default(DateTime))
            {
                SetModifiedAt(update.ModifiedAt, false);
            }

            if (update.ViewedAt != default(DateTime))
            {
                SetViewedAt(update.ViewedAt, false);
            }
        }

        public void UpdateCategories(IList<CategoryDataModel> updates)
        {
            var categories = updates.Select(update =>
                {
                    UpdateCategory(update);
                    return Notebook.GetCategory(update.ID);
                });

            SetCategories(categories.ToArray(), false);
        }

        public void UpdateCategory(CategoryDataModel update)
        {
            var category = Notebook.GetCategory(update.ID);
            category.Update(update);
        }

        public void UpdateFiles(IList<FileDataModel> updates)
        {
            var files = updates.Select(update =>
                {
                    UpdateFile(update);
                    return GetFile(update.Name);
                });

            SetFiles(files.ToArray(), false);
        }

        public void UpdateFile(FileDataModel update)
        {
            var file = GetFile(update.Name);
            file.Update(update);
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
