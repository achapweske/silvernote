/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Input;
using System.Diagnostics;
using SilverNote.Common;
using SilverNote.Models;
using SilverNote.Views;

namespace SilverNote.ViewModels
{
    public class NoteViewModel : ViewModelBase<NoteModel, NoteViewModel>
    {
        #region Constructors

        protected override void OnInitialize()
        {
            var categories = Model.Categories as INotifyCollectionChanged;
            if (categories != null)
            {
                categories.CollectionChanged += (i, j) => RaisePropertyChanged("HasCategories");
            }
            Model.WhenPropertyChanged("Categories", (i, j) => RaisePropertyChanged("HasCategories"));
        }

        #endregion

        #region Properties

        public Int64 ID
        {
            get { return Model.ID; }
        }

        /// <summary>
        /// Get the repository to which this note belongs
        /// </summary>
        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        /// <summary>
        /// Get the notebook to which this note belongs
        /// </summary>
        public NotebookViewModel Notebook
        {
            get { return NotebookViewModel.FromModel(Model.Notebook); }
        }

        /// <summary>
        /// Get this note's URL
        /// </summary>
        public string Url
        {
            get { return Model.Url; }
        }

        /// <summary>
        /// Get/set this note's title
        /// </summary>
        public string Title
        {
            get { return Model.Title; }
            set { Model.Title = value; }
        }

        /// <summary>
        /// Get/set this note's content
        /// </summary>
        public string Content
        {
            get { return Model.Content; }
            set { Model.Content = value; }
        }

        /// <summary>
        /// Set this note's text (for searching)
        /// </summary>
        public string Text
        {
            get { return null; }
            set { if (value != null) Model.Text = value; }
        }

        /// <summary>
        /// Return true iff this note belongs to at least one category
        /// </summary>
        public bool HasCategories
        {
            get { return Model.Categories.Count > 0; }
        }

        /// <summary>
        /// Get the categories to which this note belongs
        /// </summary>
        public IList<CategoryViewModel> Categories
        {
            get { return CategoryViewModel.FromCollection(Model.Categories); }
        }

        /// <summary>
        /// Get this note's created-at timestamp
        /// </summary>
        public DateTime CreatedAt
        {
            get { return Model.CreatedAt; }
        }

        /// <summary>
        /// Get this note's modified-at timestamp
        /// </summary>
        public DateTime ModifiedAt
        {
            get { return Model.ModifiedAt; }
        }

        /// <summary>
        /// Get this note's viewed-at timestamp
        /// </summary>
        public DateTime ViewedAt
        {
            get { return Model.ViewedAt; }
            set { Model.ViewedAt = value; }
        }

        /// <summary>
        /// Get some useful details about this note
        /// </summary>
        public string Details
        {
            get
            {
                var details = new StringBuilder();
                if (CreatedAt != default(DateTime))
                {
                    details.AppendFormat("Created: {0}\n", CreatedAt.ToLocalTime());
                }
                if (ModifiedAt != default(DateTime))
                {
                    details.AppendFormat("Last Edited: {0}\n", ModifiedAt.ToLocalTime());
                }
                if (ViewedAt != default(DateTime))
                {
                    details.AppendFormat("Last Viewed: {0}", ViewedAt.ToLocalTime());
                }
                return details.ToString();
            }
        }

        private bool _IsActive;

        /// <summary>
        /// Get/set whether this note is active.
        /// 
        /// A note is active if its UI has input focus.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                if (value != _IsActive)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        private object _ActivationContext;

        /// <summary>
        /// Object that activated this note
        /// </summary>
        public object ActivationContext
        {
            get
            {
                return _ActivationContext;
            }
            set
            {
                if (value != _ActivationContext)
                {
                    _ActivationContext = value;
                    RaisePropertyChanged("ActivationContext");
                }
            }
        }

        private bool _IsFloating;

        /// <summary>
        /// Get/set whether this note is in a floating window vs. tabbed
        /// </summary>
        public bool IsFloating
        {
            get 
            { 
                return _IsFloating; 
            }
            set
            {
                if (value != _IsFloating)
                {
                    Messenger.Instance.Notify("Flush", Model);
                    _IsFloating = value;
                    if (value) Float(); else Unfloat();
                    RaisePropertyChanged("IsFloating");
                    Notebook.RefreshOpenNotes();
                }
            }
        }

        /// <summary>
        /// Overriden from base class
        /// </summary>
        protected override void OnModelPropertyChanged(string propertyName)
        {
            base.OnModelPropertyChanged(propertyName);

            RaisePropertyChanged("Details");
        }

        #endregion

        #region Operations

        /// <summary>
        /// Delete this note
        /// </summary>
        public bool Delete()
        {
            return Notebook.DeleteNote(this);
        }

        /// <summary>
        /// Add this note to the given category
        /// </summary>
        /// <param name="category"></param>
        public void AddToCategory(CategoryViewModel category)
        {
            Model.AddToCategory(category.Model);
        }

        /// <summary>
        /// Remove this note from the given category
        /// </summary>
        public void RemoveFromCategory(CategoryViewModel category)
        {
            Model.RemoveFromCategory(category.Model);
        }

        #endregion

        #region Commands

        ICommand _DeleteCommand = null;
        ICommand _AddToCategoryCommand = null;
        ICommand _RemoveFromCategoryCommand = null;

        public ICommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                {
                    _DeleteCommand = new DelegateCommand(o => Delete());
                }
                return _DeleteCommand;
            }
        }

        public ICommand AddToCategoryCommand
        {
            get
            {
                if (_AddToCategoryCommand == null)
                {
                    _AddToCategoryCommand = new DelegateCommand(o => AddToCategory(o as CategoryViewModel));
                }
                return _AddToCategoryCommand;
            }
        }


        public ICommand RemoveFromCategoryCommand
        {
            get
            {
                if (_RemoveFromCategoryCommand == null)
                {
                    _RemoveFromCategoryCommand = new DelegateCommand(o => RemoveFromCategory(o as CategoryViewModel));
                }
                return _RemoveFromCategoryCommand;
            }
        }

        #endregion

        #region Implementation

        /// <summary>
        /// The floating window associated with this note (if any)
        /// </summary>
        public FloatingNoteView FloatingView { get; set; }

        /// <summary>
        /// Display this note in its own window
        /// </summary>
        private void Float()
        {
            if (FloatingView == null)
            {
                FloatingView = new FloatingNoteView { DataContext = this };
                FloatingView.Closed += FloatingView_Closed;
            }

            if (!App.Current.MainWindow.IsActive)
            {
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
            }

            FloatingView.Show();
        }

        /// <summary>
        /// Display this note in a tab
        /// </summary>
        private void Unfloat()
        {
            if (FloatingView != null)
            {
                FloatingView.Close();
            }
        }

        /// <summary>
        /// Called when this note's window closes
        /// </summary>
        private void FloatingView_Closed(object sender, EventArgs e)
        {
            FloatingView = null;

            Notebook.OpenNote(this);

            Notebook.SelectedNote = this;
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return (Title != null) ? Title : "Untitled";
        }

        #endregion
    }
}
