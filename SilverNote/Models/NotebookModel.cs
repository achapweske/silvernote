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
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using SilverNote.Data.Models;
using System.Collections.ObjectModel;
using SilverNote.Client;

namespace SilverNote.Models
{
    public class NotebookModel : ModelBase, ICloneable
    {
        #region Fields

        readonly RepositoryModel _Repository;
        readonly Int64 _ID;
        ClientManager _Source;
        string _Url;
        bool _NeedUrl = true;
        string _Description;
        Dictionary<Int64, NoteModel> _NotesMap;
        string _Name;
        ObservableCollection<NoteModel> _OpenNotes;
        NoteModel _SelectedNote;
        Dictionary<Int64, CategoryModel> _Categories;
        ObservableCollection<CategoryModel> _VisibleTopLevelCategories;
        bool _NeedName;
        bool _NeedOpenNotes;
        bool _NeedSelectedNote;
        bool _NeedCategories;
        bool _IsDeleted;

        #endregion

        #region Constructors

        public NotebookModel(RepositoryModel repository, Int64 id)
        {
            _Repository = repository;
            _Repository.Source.ClientChanged += Source_ClientChanged;
            _ID = id;
            _Search = new SearchModel(repository, this, AllNotesCategory);
            _NotesMap = new Dictionary<Int64, NoteModel>();
            _OpenNotes = new ObservableCollection<NoteModel>();
            _Categories = new Dictionary<long, CategoryModel>();
            _VisibleTopLevelCategories = new ObservableCollection<CategoryModel>();
            _NeedName = true;
            _NeedOpenNotes = true;
            _NeedSelectedNote = true;
            _NeedCategories = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the RepositoryModel that created this NotebookModel
        /// </summary>
        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        /// <summary>
        /// Get the ID assigned to this notebook by its RepositoryModel
        /// </summary>
        public Int64 ID
        {
            get { return _ID; }
        }

        /// <summary>
        /// Get the client associated with this notebook.
        /// 
        /// By default a NotebookModel uses the same client as its RepositoryModel,
        /// though this can be overriden on a per-notebook basis.
        /// </summary>
        public ClientManager Source
        {
            get { return GetSource(); }
            set { SetSource(value); }
        }

        /// <summary>
        /// Get this note's URL
        /// </summary>
        public string Url
        {
            get { return GetUrl(true); }
        }

        public string Description
        {
            get
            {
                return _Description;
            }
            set
            {
                if (value != _Description)
                {
                    _Description = value;
                    RaisePropertyChanged("Description");
                }
            }
        }

        public string Name
        {
            get { return GetName(true); }
            set { SetName(value, true); }
        }

        public IList<NoteModel> OpenNotes
        {
            get { return GetOpenNotes(true); }
        }

        public NoteModel SelectedNote
        {
            get { return GetSelectedNote(true); }
            set { SetSelectedNote(value, true); }
        }

        public IList<CategoryModel> Categories
        {
            get { return GetCategories(true); }
        }

        private IList<CategoryModel> _SpecialCategories;

        public IList<CategoryModel> SpecialCategories
        {
            get
            {
                if (_SpecialCategories == null)
                {
                    _SpecialCategories = new CategoryModel[]
					{
						AllNotesCategory,
						UncategorizedCategory
					};
                }
                return _SpecialCategories;
            }
        }

        private CategoryModel _AllNotesCategory;

        public CategoryModel AllNotesCategory
        {
            get
            {
                if (_AllNotesCategory == null)
                {
                    _AllNotesCategory = new AllNotesCategoryModel(Repository, this);
                }

                return _AllNotesCategory;
            }
        }

        private CategoryModel _UncategorizedCategory;

        public CategoryModel UncategorizedCategory
        {
            get
            {
                if (_UncategorizedCategory == null)
                {
                    _UncategorizedCategory = new UncategorizedCategoryModel(Repository, this);
                }

                return _UncategorizedCategory;
            }
        }

        private SearchModel _Search;

        public SearchModel Search
        {
            get { return _Search; }
        }

        private bool _IsSelected;

        public bool IsSelected
        {
            get 
            { 
                return _IsSelected; 
            }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;

                    if (value)
                    {
                        Repository.SelectedNotebook = this;
                    }

                    RaisePropertyChanged("IsSelected");

                    if (value && _SelectedNote != null)
                    {
                        _SelectedNote.ViewedAt = NoteClient.Now;
                    }
                }
            }
        }

        public bool IsDeleted
        {
            get { return _IsDeleted; }
        }

        public bool HasPassword
        {
            get { return Source != null && Source.Client != null && Source.Client.HasPassword; }
        }

        #endregion

        #region Methods

        public void Create(string name)
        {
            Create(name, true);
        }

        public void Delete()
        {
            Delete(true);
        }

        public NoteModel CreateNote(string title = "Untitled", bool open = true)
        {
            return CreateNote(title, open, true);
        }

        public void DeleteNote(NoteModel note)
        {
            DeleteNote(note, true);
        }

        public NoteModel GetNote(Int64 id, bool autoCreate = true)
        {
            NoteModel result;
            if (TryGetNote(id, out result))
            {
                return result;
            }
            else if (autoCreate)
            {
                return AllocNote(id);
            }
            else
            {
                return null;
            }
        }

        public void OpenNote(NoteModel note)
        {
            OpenNote(note, true);
        }

        public void OpenNote(NoteModel note, int index)
        {
            OpenNote(note, index, true);
        }

        public void CloseNote(NoteModel note)
        {
            CloseNote(note, true);
        }
        
        public void RearrangeNotes(NoteModel sourceNote, NoteModel targetNote)
        {
            RearrangeNotes(sourceNote, targetNote, true);
        }

        public CategoryModel CreateCategory(string name, CategoryModel parent = null)
        {
            return CreateCategory(name, parent, true);
        }

        public CategoryModel GetCategory(Int64 id, bool autoCreate = true)
        {
            var result = this.SpecialCategories.FirstOrDefault(category => category.ID == id);
            if (result != null)
            {
                return result;
            }
            else if (TryGetCategory(id, out result))
            {
                return result;
            }
            else if (autoCreate)
            {
                return AllocCategory(id);
            }
            else
            {
                return null;
            }
        }

        public void AddTopLevelCategory(CategoryModel category)
        {
            AddTopLevelCategory(category, true);
        }

        #endregion

        #region Implementation

        #region Source

        private ClientManager GetSource()
        {
            return _Source ?? Repository.Source; 
        }

        private void SetSource(ClientManager newSource)
        {
            // this.HasPassword must reflect Source.Client.HasPassword

            if (newSource != _Source)
            {
                NoteClient oldClient = null;
                if (Source != null)
                {
                    oldClient = Source.Client;
                    Source.ClientChanged -= Source_ClientChanged;
                }

                _Source = newSource;

                NoteClient newClient = null;
                if (Source != null)
                {
                    newClient = Source.Client;
                    Source.ClientChanged += Source_ClientChanged;
                }

                if (oldClient != newClient)
                {
                    Source_ClientChanged(Source, new NoteClientChangedEventArgs(oldClient));
                }
            }
        }

        private void Source_ClientChanged(object sender, NoteClientChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.PropertyChanged -= Client_PropertyChanged;
            }

            if (Source.Client != null)
            {
                Source.Client.PropertyChanged += Client_PropertyChanged;
            }

            RaisePropertyChanged("HasPassword");
        }

        private void Client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HasPassword")
            {
                RaisePropertyChanged("HasPassword");
            }
        }

        #endregion

        #region Create/Delete

        internal void Create(string name, bool sync)
        {
            SetName(name, false);
            SetOpenNotes(new NoteModel[0], false);
            SetSelectedNote(null, false);
            SetCategories(new CategoryModel[0], false);

            if (sync)
            {
                Source.Client.CreateNotebook(ID, name);
            }
        }

        void Delete(bool sync)
        {
            _IsDeleted = true;

            if (sync)
            {
                Source.Client.DeleteNotebook(ID, purge: true);
            }

            RaisePropertyChanged("IsDeleted");
        }

        #endregion

        #region Url

        /// <summary>
        /// Get this notebook's URL
        /// </summary>
        /// <param name="sync">true to get the current value from the server as needed</param>
        /// <returns></returns>
        protected string GetUrl(bool sync)
        {
            if (sync && _NeedUrl)
            {
                _Url = Source.Client.NotebookUri(ID).ToString();
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

        #region Name

        public string GetName(bool sync)
        {
            if (_NeedName && sync)
            {
                Source.Client.GetNotebookName(ID);
            }
            return _Name;
        }

        public void SetName(string name, bool sync)
        {
            _NeedName = false;

            if (name != _Name)
            {
                _Name = name;
                
                if (sync)
                {
                    Source.Client.SetNotebookName(ID, name);
                }

                RaisePropertyChanged("Name");
            }
        }

        #endregion

        #region Notes

        Int64 NextNoteID
        {
            get
            {
                Int64 id = IDGenerator.NextID();
                while (_NotesMap.ContainsKey(id))
                {
                    id = IDGenerator.NextID();
                }
                return id;
            }
        }

        NoteModel AllocNote(Int64 id)
        {
            var note = new NoteModel(Repository, this, id);
            _NotesMap.Add(note.ID, note);
            return note;
        }

        bool TryGetNote(Int64 id, out NoteModel result)
        {
            return _NotesMap.TryGetValue(id, out result);
        }

        public NoteModel CreateNote(string title, bool open, bool sync)
        {
            var note = AllocNote(NextNoteID);

            note.Create(title, sync);

            if (open)
            {
                OpenNote(note, sync);
                SetSelectedNote(note, sync);
            }

            AllNotesCategory.Search.Results.Invalidate();
            UncategorizedCategory.Search.Results.Invalidate();
            Search.Results.Invalidate();

            return note;
        }

        public void DeleteNote(NoteModel note, bool sync)
        {
            CloseNote(note, sync);
            note.Delete(sync);
        }
		
        #endregion

        #region OpenNotes

        public IList<NoteModel> GetOpenNotes(bool sync)
        {
            if (_NeedOpenNotes && sync)
            {
                // Get selected note prior to getting the open notes; 
                // otherwise, when open notes is returned we will automatically 
                // select one and when the previously selected note is retrieved,
                // it will be ignored
                if (_NeedSelectedNote)
                {
                    Source.Client.GetSelectedNote(ID);
                }

                Source.Client.GetOpenNotes(ID);
            }
            return _OpenNotes;
        }

        public void SetOpenNotes(IList<NoteModel> notes, bool sync)
        {
            _NeedOpenNotes = false;

            foreach (var note in notes)
            {
                OpenNote(note, sync, false);
            }

            RaisePropertyChanged("OpenNotes");
        }

        void OpenNote(NoteModel note, bool sync, bool notify = true)
        {
            OpenNote(note, _OpenNotes.Count, sync, notify);
        }

        void OpenNote(NoteModel note, int index, bool sync, bool notify = true)
        {
            if (note.Notebook != this)
            {
                string message = String.Format("Note {0} does not belong to notebook {1}", note.ID, this.ID);
                throw new ArgumentException(message);
            }

            int currentIndex = _OpenNotes.IndexOf(note);
            if (currentIndex == -1)
            {
                _OpenNotes.Insert(index, note);
                if (sync) Source.Client.OpenNote(this.ID, note.ID);
                if (notify) RaisePropertyChanged("OpenNotes");
            }
            else if (currentIndex != index)
            {
                bool wasSelected = note == _SelectedNote;
                _OpenNotes.Remove(note);
                if (currentIndex < index) --index;
                _OpenNotes.Insert(index, note);
                if (wasSelected) SelectedNote = note;
                if (notify) RaisePropertyChanged("OpenNotes");
            }
        }

        protected void CloseNote(NoteModel note, bool sync, bool notify = true)
        {
            // Auto-select next note

            if (GetSelectedNote(false) == note)
            {
                SelectNextNote(note);
            }

            if (_OpenNotes.Contains(note))
            {
                _OpenNotes.Remove(note);
                if (sync) Source.Client.CloseNote(this.ID, note.ID);
                if (notify) RaisePropertyChanged("OpenNotes");
            }
        }

        protected void RearrangeNotes(NoteModel sourceNote, NoteModel targetNote, bool sync, bool notify = true)
        {
            if (sourceNote.Notebook != this)
            {
                string message = String.Format("Note {0} does not belong to notebook {1}", sourceNote.ID, this.ID);
                throw new ArgumentException(message);
            }

            if (targetNote.Notebook != this)
            {
                string message = String.Format("Note {0} does not belong to notebook {1}", targetNote.ID, this.ID);
                throw new ArgumentException(message);
            }

            int targetIndex = _OpenNotes.IndexOf(targetNote);
            if (targetIndex == -1)
            {
                throw new InvalidOperationException("Only open notes can be rearranged");
            }

            _OpenNotes.Remove(sourceNote);
            _OpenNotes.Insert(targetIndex, sourceNote);
            
            if (sync)
            {
                Int64[] openNoteIDs = _OpenNotes.Select((note) => note.ID).ToArray();
                Source.Client.SetOpenNotes(this.ID, openNoteIDs);
            }

            if (notify) RaisePropertyChanged("OpenNotes");
        }

        #endregion

        #region SelectedNote

        protected NoteModel GetSelectedNote(bool sync)
        {
            if (sync && _NeedSelectedNote)
            {
                Source.Client.GetSelectedNote(ID);
            }
            return _SelectedNote;
        }

        protected void SetSelectedNote(NoteModel note, bool sync)
        {
            if (note != null && !OpenNotes.Contains(note))
            {
                OpenNote(note, sync);
            }

            _NeedSelectedNote = false;

            if (note != _SelectedNote)
            {
                _SelectedNote = note;

                if (sync)
                {
                    Int64 noteID = note != null ? note.ID : 0;
                    Source.Client.SetSelectedNote(this.ID, noteID);
                }

                RaisePropertyChanged("SelectedNote");

                if (note != null && IsSelected)
                {
                    note.ViewedAt = NoteClient.Now;
                }
            }
        }

        public void SelectNextNote(NoteModel note)
        {
            var openNotes = GetOpenNotes(false);
            if (openNotes != null)
            {
                SelectNextNote(openNotes, note);
            }
            else
            {
                SelectedNote = null;
            }
        }

        public void SelectNextNote(IList<NoteModel> notes, NoteModel note = null)
        {
            SelectNextNote(notes, notes.IndexOf(note));
        }

        public void SelectNextNote(IList<NoteModel> notes, int index = -1)
        {
            if (index + 1 < notes.Count)
            {
                SelectedNote = notes[index + 1];
            }
            else if (index - 1 >= 0)
            {
                SelectedNote = notes[index - 1];
            }
            else
            {
                SelectedNote = null;
            }
        }

        #endregion

        #region Categories

        CategoryModel AllocCategory(Int64 id)
        {
            var category = new CategoryModel(Repository, this, id);
            category.WhenPropertyChanged("IsDeleted", Category_IsDeletedChanged);
            _Categories.Add(id, category);
            return category;
        }

        bool TryGetCategory(Int64 id, out CategoryModel result)
        {
            return _Categories.TryGetValue(id, out result);
        }

        public IList<CategoryModel> GetCategories(bool sync)
        {
            if (_NeedCategories && sync)
            {
                Source.Client.GetCategoryChildren(this.ID, -1);
            }

            return _VisibleTopLevelCategories;
        }

        CategoryModel CreateCategory(string name, CategoryModel parent, bool sync)
        {
            if (parent != null && parent.IsPseudoCategory)
            {
                throw new InvalidOperationException();
            }

            Int64 id = IDGenerator.NextID();
            while (_Categories.ContainsKey(id))
            {
                id = IDGenerator.NextID();
            }

            var category = AllocCategory(id);

            category.Create(name, parent, sync);

            if (parent == null)
            {
                _VisibleTopLevelCategories.Add(category);
            }

            return category;
        }

        void Category_IsDeletedChanged(object sender, PropertyChangedEventArgs e)
        {
            var category = (CategoryModel)sender;

            if (category.IsDeleted && category.Parent == null)
            {
                _VisibleTopLevelCategories.Remove(category);
            }
        }

        protected void SetCategories(IList<CategoryModel> categories, bool sync)
        {
            _NeedCategories = false;

            foreach (var category in categories)
            {
                if (category.Parent == null && !category.IsDeleted && !_VisibleTopLevelCategories.Contains(category))
                {
                    _VisibleTopLevelCategories.Add(category);
                }
            }
        }

        public void AddTopLevelCategory(CategoryModel category, bool sync)
        {
            category.SetParent(null, sync);

            if (!category.IsDeleted && !_VisibleTopLevelCategories.Contains(category))
            {
                _VisibleTopLevelCategories.Add(category);
            }
        }

        public void RemoveTopLevelCategory(CategoryModel category, bool sync)
        {
            _VisibleTopLevelCategories.Remove(category);
        }

        #endregion

        #endregion

        #region Data Model

        public void Update(NotebookDataModel data)
        {
            if (data.Name != null)
            {
                SetName(data.Name, false);
            }

            if (data.OpenNotes != null && _NeedOpenNotes)
            {
                foreach (Int64 noteID in data.OpenNotes)
                {
                    NoteModel note = GetNote(noteID);
                    if (!note.IsDeleted)
                    {
                        OpenNote(note, sync: false);
                    }
                }
                _NeedOpenNotes = false;
            }

            if (data.SelectedNoteID != 0 && _NeedSelectedNote)
            {
                var note = GetNote(data.SelectedNoteID);

                SetSelectedNote(note, sync: false);
            }

            if (data.Categories != null)
            {
                UpdateCategories(data.Categories);
            }

            if (data.Notes != null)
            {
                UpdateNotes(data.Notes);
            }
        }

        public void UpdateNotes(NoteDataModel[] updates)
        {
            foreach (var update in updates)
            {
                UpdateNote(update);
            }
        }

        public void UpdateNote(NoteDataModel data)
        {
            var note = GetNote(data.ID);

            note.Update(data);
        }

        public void UpdateFile(FileDataModel data)
        {
            var note = GetNote(data.NoteID);

            note.UpdateFile(data);
        }

        public void UpdateCategories(CategoryDataModel[] updates)
        {
            updates = RemoveOrphans(updates);

            var categories = new List<CategoryModel>();

            foreach (var update in updates)
            {
                var category = GetCategory(update.ID);

                category.Update(update);

                categories.Add(category);
            }

            SetCategories(categories, false);
        }

        public CategoryDataModel[] RemoveOrphans(CategoryDataModel[] updates)
        {
            while (true)
            {
                var orphans = new List<CategoryDataModel>();

                foreach (var update in updates)
                {
                    if (update.ParentID > 0)
                    {
                        var parent = (from _parent in updates
                                      where _parent.ID == update.ParentID
                                      select _parent).FirstOrDefault();

                        if (parent == null)
                        {
                            orphans.Add(update);
                        }
                    }
                }

                if (orphans.Count > 0)
                {
                    updates = updates.Except(orphans).ToArray();
                }
                else
                {
                    return updates;
                }
            }
        }

        public void UpdateCategory(CategoryDataModel update)
        {
            if (update.ID > 0)
            {
                var category = GetCategory(update.ID);

                category.Update(update);
            }
            else
            {
                if (update.Children != null)
                {
                    UpdateCategories(update.Children);
                }
            }
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
