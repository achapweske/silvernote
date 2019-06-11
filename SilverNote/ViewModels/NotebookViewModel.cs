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
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using SilverNote.Common;
using SilverNote.Models;
using SilverNote.Properties;
using SilverNote.ViewModels.CategoryTree;
using System.Windows.Data;
using System.Collections;

namespace SilverNote.ViewModels
{
    public class NotebookViewModel : ViewModelBase<NotebookModel, NotebookViewModel>
    {
        #region Fields

        CategoryTreeViewModel _CategoryTree;

        #endregion

        #region Constructors

        protected override void OnInitialize()
        {
            var openNotes = (INotifyCollectionChanged)Model.GetOpenNotes(false);
            openNotes.CollectionChanged += (i, j) => RaisePropertyChanged("TabbedNotes");
            openNotes.CollectionChanged += (i, j) => RaisePropertyChanged("HasTabbedNotes");

            var categories = (INotifyCollectionChanged)Model.GetCategories(false);
            categories.CollectionChanged += (i, j) => RaisePropertyChanged("HasCategories");
        }

        #endregion

        #region Properties

        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        /// <summary>
        /// Get this notebook's URL
        /// </summary>
        public string Url
        {
            get { return Model.Url; }
        }

        public Int64 ID
        {
            get { return Model.ID; }
        }

        public virtual string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public string Description
        {
            get { return Model.Description; }
        }

        public IList<NoteViewModel> OpenNotes
        {
            get { return NoteViewModel.FromCollection(Model.OpenNotes); }
        }

        public bool HasTabbedNotes
        {
            get { return TabbedNotes.Count > 0; }
        }

        public IList<NoteViewModel> TabbedNotes
        {
            get 
            {
                return (from note in OpenNotes where !note.IsFloating select note).ToList(); 
            }
        }

        public NoteViewModel SelectedNote
        {
            get { return NoteViewModel.FromModel(Model.SelectedNote); }
            set { Model.SelectedNote = value != null ? value.Model : null; }
        }

        public bool HasCategories
        {
            get { return Model.Categories.Count > 0; }
        }

        public virtual IList<CategoryViewModel> Categories
        {
            get { return CategoryViewModel.FromCollection(Model.Categories); }
        }

        public IList<CategoryViewModel> SpecialCategories
        {
            get { return CategoryViewModel.FromCollection(Model.SpecialCategories); }
        }

        public CategoryViewModel AllNotesCategory
        {
            get { return CategoryViewModel.FromModel(Model.AllNotesCategory); }
        }

        public CategoryViewModel UncategorizedCategory
        {
            get { return CategoryViewModel.FromModel(Model.UncategorizedCategory); }
        }

        public CategoryTreeViewModel CategoryTree
        {
            get { return _CategoryTree ?? (_CategoryTree = new CategoryTreeViewModel(this)); }
        }

        public IList SelfAndChildren
        {
            get { return new NotebookChildrenViewModel(this); }
        }

        public bool IsSelected
        {
            get { return Model.IsSelected; }
            set { Model.IsSelected = value; }
        }

        public SearchViewModel Search
        {
            get { return SearchViewModel.FromModel(Model.Search); }
        }

        private bool _IsSearching;

        public bool IsSearching
        {
            get 
            { 
                return _IsSearching; 
            }
            set
            {
                if (value != _IsSearching)
                {
                    _IsSearching = value;
                    RaisePropertyChanged("IsSearching");
                }
            }
        }

        public bool HasPassword
        {
            get { return Model.HasPassword; }
        }

        #endregion

        #region Operations

        public object Locate(Uri uri)
        {
            object result = Locator.Locate(Model, uri);

            if (result is NotebookModel)
            {
                return NotebookViewModel.FromModel((NotebookModel)result);
            }

            if (result is NoteModel)
            {
                return NoteViewModel.FromModel((NoteModel)result);
            }

            if (result is CategoryModel)
            {
                return CategoryViewModel.FromModel((CategoryModel)result);
            }

            return result;
        }

        public void RefreshOpenNotes()
        {
            RaisePropertyChanged("OpenNotes");
            RaisePropertyChanged("TabbedNotes");
        }

        public void Rename(string newName = null)
        {
            if (newName == null)
            {
                var dialog = new Dialogs.NewNotebookDialog();
                dialog.Title = "Rename Notebook";
                dialog.NotebookName = this.Name;
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                newName = dialog.NotebookName;
            }

            this.Name = newName;
        }

        public NoteViewModel CreateNote(bool floating = false)
        {
            NoteModel noteModel = Model.CreateNote();

            var noteViewModel = NoteViewModel.FromModel(noteModel);

            noteViewModel.IsFloating = floating;

            if (Settings.Default.SearchPaneAutoHide)
            {
                IsSearching = false;
            }

            return noteViewModel;
        }

        public bool DeleteNote(NoteViewModel note = null)
        {
            if (note == null)
            {
                note = SelectedNote;
            }

            if (note != null)
            {
                string message = String.Format("The note \"{0}\" will be PERMANENTLY DELETED.", note.Title);

                if (MessageBox.Show(message, "SilverNote", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    Messenger.Instance.Notify("Flush", Model);
                    Model.DeleteNote(note.Model);
                    return true;
                }
            }

            return false;
        }

        public bool DeleteNotes(IEnumerable<NoteViewModel> notes)
        {
            if (notes.Count() == 1)
            {
                return DeleteNote(notes.First());
            }
            else if (notes.Count() > 1)
            {
                string message = String.Format("The {0} selected notes will be PERMANENTLY DELETED.", notes.Count());

                if (MessageBox.Show(message, "SilverNote", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    Messenger.Instance.Notify("Flush", Model);
                    notes.ForEach(note => Model.DeleteNote(note.Model));
                    return true;
                }
            }

            return false;
        }

        public void OpenNote(NoteViewModel note = null)
        {
            if (note == null)
            {
                note = SelectedNote;
            }

            if (note != null)
            {
                note.IsFloating = false;
                Model.OpenNote(note.Model);
                ActivateNote(note);
            }
        }

        public void FloatNote(NoteViewModel note = null)
        {
            if (note == null)
            {
                note = SelectedNote;
            }

            if (note != null)
            {
                note.IsFloating = true;
                Model.OpenNote(note.Model);
                SelectedNote = GetNextTabbedNote(note);
            }
        }

        private NoteViewModel GetNextTabbedNote(NoteViewModel note)
        {
            int index = OpenNotes.IndexOf(note);

            for (int i = index; i < OpenNotes.Count; i++)
            {
                if (!OpenNotes[i].IsFloating)
                {
                    return OpenNotes[i];
                }
            }

            for (int i = index - 1; i >= 0; i--)
            {
                if (!OpenNotes[i].IsFloating)
                {
                    return OpenNotes[i];
                }
            }

            return null;
        }

        public void CloseNote(NoteViewModel note = null)
        {
            if (note == null)
            {
                note = SelectedNote;
            }

            if (note != null)
            {
                Messenger.Instance.Notify("Flush", note.Model);
                Model.CloseNote(note.Model);
            }
        }

        public void CloseOtherNotes(NoteViewModel note = null)
        {
            if (note == null)
            {
                note = SelectedNote;
            }

            foreach (var openNote in OpenNotes.ToArray())
            {
                if (openNote != note)
                {
                    CloseNote(openNote);
                }
            }
        }

        public void SelectNote(NoteViewModel note, object context = null)
        {
            SelectedNote = note;

            if (note != null)
            {
                ActivateNote(note, context);
            }
        }

        public void ActivateNote(NoteViewModel note = null, object context = null)
        {
            if (note == null)
            {
                note = SelectedNote;
            }

            if (note != null)
            {
                note.ActivationContext = context;
                note.IsActive = true;

                if (Settings.Default.SearchPaneAutoHide)
                {
                    IsSearching = false;
                }
            }
        }

        public void RearrangeNotes(NoteViewModel source, NoteViewModel target)
        {
            Model.RearrangeNotes(source.Model, target.Model);
        }

        public CategoryViewModel CreateCategory(string name)
        {
            var newModel = Model.CreateCategory(name);

            var newViewModel = CategoryViewModel.FromModel(newModel);

            newViewModel.IsEditable = true;
            newViewModel.IsNew = true;

            return newViewModel;
        }

        public void DeleteCategory(CategoryViewModel category)
        {
            if (category != null)
            {
                category.Model.Delete();
            }
        }

        public void BrowseCategory(CategoryViewModel category)
        {
            if (category != null)
            {
                Search.Category = category;
                IsSearching = true;
            }
        }

        public void AddTopLevelCategory(CategoryViewModel category)
        {
            Model.AddTopLevelCategory(category.Model);
        }

        #endregion

        #region Commands

        private ICommand _CreateNoteCommand = null;

        public ICommand CreateNoteCommand
        {
            get
            {
                if (_CreateNoteCommand == null)
                {
                    _CreateNoteCommand = new DelegateCommand(o =>
                    {
                        bool floating = (o is bool) ? (bool)o : false;
                        CreateNote(floating);
                    });
                }
                return _CreateNoteCommand;
            }
        }

        private ICommand _QuickNoteCommand = null;

        public ICommand QuickNoteCommand
        {
            get
            {
                if (_QuickNoteCommand == null)
                {
                    _QuickNoteCommand = new DelegateCommand(o => CreateNote(true));
                }
                return _QuickNoteCommand;
            }
        }

        private ICommand _DeleteNoteCommand = null;

        public ICommand DeleteNoteCommand
        {
            get
            {
                if (_DeleteNoteCommand == null)
                {
                    _DeleteNoteCommand = new DelegateCommand(o => DeleteNote(o as NoteViewModel));
                }
                return _DeleteNoteCommand;
            }
        }

        private ICommand _OpenNoteCommand = null;

        public ICommand OpenNoteCommand
        {
            get
            {
                if (_OpenNoteCommand == null)
                {
                    _OpenNoteCommand = new DelegateCommand(o => OpenNote(o as NoteViewModel));
                }
                return _OpenNoteCommand;
            }
        }

        private ICommand _FloatNoteCommand = null;

        public ICommand FloatNoteCommand
        {
            get
            {
                if (_FloatNoteCommand == null)
                {
                    _FloatNoteCommand = new DelegateCommand(o => FloatNote(o as NoteViewModel));
                }
                return _FloatNoteCommand;
            }
        }

        private ICommand _CloseNoteCommand = null;

        public ICommand CloseNoteCommand
        {
            get
            {
                if (_CloseNoteCommand == null)
                {
                    _CloseNoteCommand = new DelegateCommand(o => CloseNote(o as NoteViewModel));
                }
                return _CloseNoteCommand;
            }
        }

        private ICommand _CloseOtherNotesCommand = null;

        public ICommand CloseOtherNotesCommand
        {
            get
            {
                if (_CloseOtherNotesCommand == null)
                {
                    _CloseOtherNotesCommand = new DelegateCommand(o => CloseOtherNotes(o as NoteViewModel));
                }
                return _CloseOtherNotesCommand;
            }
        }

        private ICommand _SelectNoteCommand = null;

        public ICommand SelectNoteCommand
        {
            get
            {
                if (_SelectNoteCommand == null)
                {
                    _SelectNoteCommand = new DelegateCommand(o => SelectNote(o as NoteViewModel));
                }
                return _SelectNoteCommand;
            }
        }

        private ICommand _ActivateNoteCommand = null;

        public ICommand ActivateNoteCommand
        {
            get
            {
                if (_ActivateNoteCommand == null)
                {
                    _ActivateNoteCommand = new DelegateCommand(o => ActivateNote(o as NoteViewModel));
                }
                return _ActivateNoteCommand;
            }
        }

        private ICommand _CreateCategoryCommand = null;

        public ICommand CreateCategoryCommand
        {
            get
            {
                if (_CreateCategoryCommand == null)
                {
                    _CreateCategoryCommand = new DelegateCommand(o => CreateCategory(o as string));
                }
                return _CreateCategoryCommand;
            }
        }

        private ICommand _DeleteCategoryCommand = null;

        public ICommand DeleteCategoryCommand
        {
            get
            {
                if (_DeleteCategoryCommand == null)
                {
                    _DeleteCategoryCommand = new DelegateCommand(DeleteCategoryCommand_Executed);
                }
                return _DeleteCategoryCommand;
            }
        }

        private void DeleteCategoryCommand_Executed(object parameter)
        {
            var category = parameter as CategoryViewModel;
            if (!SpecialCategories.Contains(category))
            {
                DeleteCategory(category);
            }
        }

        private ICommand _BrowseCategoryCommand = null;

        public ICommand BrowseCategoryCommand
        {
            get
            {
                if (_BrowseCategoryCommand == null)
                {
                    _BrowseCategoryCommand = new DelegateCommand(o => BrowseCategory(o as CategoryViewModel));
                }
                return _BrowseCategoryCommand;
            }
        }

        #region DropCommand

        private ICommand _DropCommand = null;

        public ICommand DropCommand
        {
            get { return _DropCommand ?? (_DropCommand = new DelegateCommand(DropCommand_Executed)); }
        }

        private void DropCommand_Executed(object parameter)
        {
            var data = parameter as IDataObject;
            if (data != null)
            {
                Paste(data);
            }
        }

        private void Paste(IDataObject data)
        {
            var uri = (Uri)data.GetData(typeof(Uri));
            if (uri != null)
            {
                Paste(uri);
                return;
            }

            var uriList = (List<Uri>)data.GetData(typeof(List<Uri>));
            if (uriList != null)
            {
                Paste(uriList);
            }
        }

        private void Paste(IEnumerable<Uri> uriList)
        {
            foreach (var uri in uriList)
            {
                Paste(uri);
            }
        }

        private void Paste(Uri uri)
        {
            var item = Locate(uri);
            if (item is CategoryViewModel)
            {
                Paste((CategoryViewModel)item);
            }
        }

        private void Paste(CategoryViewModel category)
        {
            if (!category.IsPseudoCategory)
            {
                AddTopLevelCategory(category);
            }
        }

        #endregion

        #endregion

        #region Object

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
