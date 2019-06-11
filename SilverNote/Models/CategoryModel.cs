/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SilverNote.Data.Models;
using System.Collections.ObjectModel;

namespace SilverNote.Models
{
    public class CategoryModel : ModelBase
    {
        #region Fields

        readonly RepositoryModel _Repository;
        readonly NotebookModel _Notebook;
        readonly Int64 _ID;
        readonly bool _IsPseudoCategory;
        HashSet<NoteModel> _Notes;
        SearchModel _Search;
        string _Name;
        CategoryModel _Parent;
        IList<CategoryModel> _Children;
        bool _NeedName;
        bool _NeedParent;
        bool _NeedChildren;
        bool _IsDeleted;

        #endregion

        #region Constructors

        public CategoryModel(RepositoryModel repository, NotebookModel notebook, Int64 id, bool isPseudoCategory = false)
        {
            _Repository = repository;
            _Notebook = notebook;
            _ID = id;
            _IsPseudoCategory = isPseudoCategory;
            _Notes = new HashSet<NoteModel>();
            _Search = new SearchModel(repository, notebook, this);
            _Search.SortBy = SearchSort.Title;
            _Search.Order = SearchOrder.Ascending;
            _Search.ReturnText = false;
            _Children = new ObservableCollection<CategoryModel>();
            _NeedName = true;
            _NeedParent = true;
            _NeedChildren = true;
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return Notebook.Source; }
        }

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public NotebookModel Notebook
        {
            get { return _Notebook; }
        }

        public Int64 ID
        {
            get { return _ID; }
        }

        public bool IsPseudoCategory
        {
            get { return _IsPseudoCategory; }
        }

        public SearchModel Search
        {
            get { return _Search; }
        }

        public Uri Uri
        {
            get { return Source.Client.CategoryUri(Notebook.ID, this.ID); }
        }

        public virtual string Name
        {
            get { return GetName(true); }
            set { SetName(value, true); }
        }

        public virtual CategoryModel Parent
        {
            get { return GetParent(true); }
            set { SetParent(value, false); }
        }

        public virtual IList<CategoryModel> Children
        {
            get { return GetChildren(true); }
        }

        public IList<NoteModel> Notes
        {
            get { return _Notes.ToArray(); }
        }

        public bool IsDeleted
        {
            get { return _IsDeleted; }
        }

        #endregion

        #region Methods

        public void Create(string name, CategoryModel parent)
        {
            Create(name, parent, true);
        }

        public virtual void Delete()
        {
            Delete(true);
        }

        public CategoryModel CreateCategory(string name)
        {
            return Notebook.CreateCategory(name, this);
        }

        public void AddChild(CategoryModel category)
        {
            AddChild(category, true);
        }

        public void RemoveChild(CategoryModel category)
        {
            RemoveChild(category, true);
        }

        public void AddNote(NoteModel note)
        {
            AddNote(note, true);
        }

        public void RemoveNote(NoteModel note)
        {
            RemoveNote(note, true);
        }

        #endregion

        #region Implementation

        public void Create(string name, CategoryModel parent, bool sync)
        {
            if (IsPseudoCategory)
            {
                throw new InvalidOperationException();
            }

            SetName(name, false);
            SetParent(parent, false);

            if (sync)
            {
                Int64 parentID = (parent != null) ? parent.ID : -1;
                Source.Client.CreateCategory(Notebook.ID, this.ID, name, parentID);
            }
        }

        public virtual void Delete(bool sync)
        {
            if (IsPseudoCategory)
            {
                throw new InvalidOperationException();
            }

            if (!_IsDeleted)
            {
                _IsDeleted = true;

                if (sync)
                {
                    Source.Client.DeleteCategory(Notebook.ID, this.ID, true);
                }

                CategoryModel parent = GetParent(false);

                if (parent != null)
                {
                    parent.RemoveChild(this, false);
                }

                foreach (var note in Notes)
                {
                    note.RemoveFromCategory(this, false);
                }

                RaisePropertyChanged("IsDeleted");
            }
        }


        #region Name

        protected string GetName(bool sync)
        {
            if (sync && _NeedName)
            {
                Source.Client.GetCategoryName(Notebook.ID, ID);
            }
            return _Name;
        }

        public void SetName(string value, bool sync)
        {
            if (sync && IsPseudoCategory)
            {
                throw new InvalidOperationException();
            }

            _NeedName = false;

            if (value != _Name)
            {
                _Name = value;

                if (sync)
                {
                    Source.Client.SetCategoryName(Notebook.ID, ID, value);
                }

                RaisePropertyChanged("Name");
            }
        }
        #endregion

        #region Parent

        CategoryModel GetParent(bool sync)
        {
            if (sync && _NeedParent)
            {
                Source.Client.GetCategoryParent(Notebook.ID, this.ID);
            }
            return _Parent;
        }

        public void SetParent(CategoryModel newParent, bool sync)
        {
            if (sync && IsPseudoCategory)
            {
                throw new InvalidOperationException();
            }

            if (newParent == this)
            {
                return;
            }

            _NeedParent = false;

            var oldParent = _Parent;

            if (newParent != oldParent)
            {
                _Parent = newParent;

                OnParentChanged(oldParent, newParent, sync);

                RaisePropertyChanged("Parent");
            }
        }

        void OnParentChanged(CategoryModel oldParent, CategoryModel newParent, bool sync)
        {
            if (sync)
            {
                if (newParent != null)
                {
                    Source.Client.SetCategoryParent(Notebook.ID, this.ID, newParent.ID);
                }
                else
                {
                    Source.Client.SetCategoryParent(Notebook.ID, this.ID, -1);
                }
            }

            if (oldParent == null)
            {
                Notebook.RemoveTopLevelCategory(this, sync);
            }
            else
            {
                oldParent.RemoveChild(this, sync);
            }

            if (newParent == null)
            {
                Notebook.AddTopLevelCategory(this, sync);
            }
            else
            {
                newParent.AddChild(this, sync);
            }
        }

        #endregion

        #region Children

        IList<CategoryModel> GetChildren(bool sync)
        {
            if (_NeedChildren && sync)
            {
                Source.Client.GetCategoryChildren(Notebook.ID, this.ID);
            }

            return _Children;
        }

        protected void AddChildren(IList<CategoryModel> children, bool sync)
        {
            _NeedChildren = false;

            foreach (var child in children)
            {
                AddChild(child, sync);
            }
        }

        protected void AddChild(CategoryModel child, bool sync)
        {
            if (sync && IsPseudoCategory)
            {
                throw new InvalidOperationException();
            }

            if (IsSelfOrAncestor(child))
            {
                return;
            }

            if (!_Children.Contains(child) && !child.IsDeleted)
            {
                _Children.Add(child);

                child.SetParent(this, sync);
            }
        }

        protected void RemoveChild(CategoryModel child, bool sync)
        {
            if (_Children.Contains(child))
            {
                _Children.Remove(child);
            }
        }

        public bool IsSelfOrAncestor(CategoryModel category)
        {
            if (category == this)
            {
                return true;
            }

            var parent = GetParent(false);
            if (parent == null)
            {
                return false;
            }

            return parent.IsSelfOrAncestor(category);
        }

        #endregion

        #region Notes

        public void AddNote(NoteModel note, bool sync)
        {
            if (IsPseudoCategory)
            {
                return;
            }

            if (!_Notes.Contains(note))
            {
                _Notes.Add(note);
                note.AddToCategory(this, sync);
                if (sync)
                {
                    Search.Results.Invalidate();
                    Notebook.UncategorizedCategory.Search.Results.Invalidate();
                }
            }
        }

        public void RemoveNote(NoteModel note, bool sync)
        {
            if (IsPseudoCategory)
            {
                return;
            }

            if (_Notes.Contains(note))
            {
                _Notes.Remove(note);
                note.RemoveFromCategory(this, sync);
                if (sync)
                {
                    Search.Results.Remove(note);
                    Notebook.UncategorizedCategory.Search.Results.Invalidate();
                }
            }
        }

        #endregion

        #endregion

        #region Data Model

        public void Update(CategoryDataModel update)
        {
            if (update.IsDeleted)
            {
                Delete(false);
            }

            if (update.Name != null)
            {
                SetName(update.Name, false);
            }

            if (update.ParentID != 0)
            {
                UpdateParent(update.ParentID);
            }

            if (update.Children != null)
            {
                UpdateChildren(update.Children);
            }
        }

        public void UpdateParent(Int64 parentID)
        {
            CategoryModel parent = null;

            if (parentID > 0)
            {
                parent = Notebook.GetCategory(parentID);
            }

            SetParent(parent, false);
        }

        public void UpdateChildren(CategoryDataModel[] updates)
        {
            var children = new List<CategoryModel>();

            foreach (var update in updates)
            {
                var child = Notebook.GetCategory(update.ID, true);

                child.Update(update);

                children.Add(child);
            }

            AddChildren(children, false);
        }

        #endregion

        #region Object

        public override string ToString()
        {
            if (_NeedName)
            {
                return ID.ToString();
            }
            return _Name;
        }

        #endregion
    }
}
