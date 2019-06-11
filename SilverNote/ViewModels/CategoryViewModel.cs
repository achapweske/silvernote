/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SilverNote.Common;
using SilverNote.Models;
using System.Windows.Data;
using System.Windows;

namespace SilverNote.ViewModels
{
    public class CategoryViewModel : ViewModelBase<CategoryModel, CategoryViewModel>
    {
        #region Fields

        bool _IsNew = false;
        bool _IsEditable = false;
        bool _IsExpanded = false;
        CompositeCollection _Content;

        #endregion

        #region Constructors

        protected override void OnInitialize()
        {

        }

        #endregion

        #region Properties

        public Uri Uri
        {
            get { return Model.Uri; }
        }

        public Int64 ID
        {
            get { return Model.ID; }
        }

        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        public NotebookViewModel Notebook
        {
            get { return NotebookViewModel.FromModel(Model.Notebook); }
        }

        public CategoryViewModel Parent
        {
            get { return CategoryViewModel.FromModel(Model.Parent); }
        }

        public virtual IList<CategoryViewModel> Children
        {
            get { return CategoryViewModel.FromCollection(Model.Children); }
        }

        public IList SelfAndChildren
        {
            get { return new CategoryChildrenViewModel(this); }
        }

        public bool IsPseudoCategory
        {
            get { return Model.IsPseudoCategory; }
        }

        public SearchViewModel Search
        {
            get { return SearchViewModel.FromModel(Model.Search); }
        }

        public IList Content
        {
            get
            {
                if (_Content == null)
                {
                    _Content = new CompositeCollection();
                    _Content.Add(new CollectionContainer { Collection = Children });
                    _Content.Add(new CollectionContainer { Collection = Search.Results });
                }
                return _Content;
            }
        }

        public virtual string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public bool IsNew
        {
            get
            {
                return _IsNew;
            }
            set
            {
                _IsNew = value;
                RaisePropertyChanged("IsNew");
            }
        }

        public bool IsEditable
        {
            get 
            { 
                return _IsEditable; 
            }
            set
            {
                _IsEditable = value;
                RaisePropertyChanged("IsEditable");
                if (Parent != null && value)
                {
                    Parent.IsExpanded = true;
                }
            }
        }

        public bool IsExpanded
        {
            get 
            { 
                return _IsExpanded; 
            }
            set
            {
                _IsExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        #endregion

        #region Operations

        public void Delete()
        {
            if (!Model.IsPseudoCategory)
            {
                Model.Delete();
            }
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

        public void AddChild(CategoryViewModel category)
        {
            Model.AddChild(category.Model);
        }

        public void AddNote(NoteViewModel note)
        {
            Model.AddNote(note.Model);
        }

        public void Rename()
        {
            if (!Model.IsPseudoCategory)
            {
                IsEditable = !IsEditable;

                if (!IsEditable)
                {
                    IsNew = false;
                }
            }
        }

        #endregion

        #region Commands

        #region CreateCategoryCommand

        private ICommand _CreateCategoryCommand = null;

        public ICommand CreateCategoryCommand
        {
            get
            {
                if (_CreateCategoryCommand == null)
                {
                    _CreateCategoryCommand = new DelegateCommand(CreateCategoryCommand_Executed);
                }
                return _CreateCategoryCommand;
            }
        }

        private void CreateCategoryCommand_Executed(object parameter)
        {
            if (Notebook.SpecialCategories.Contains(this))
            {
                Notebook.CreateCategory(parameter as string);
            }
            else
            {
                CreateCategory(parameter as string);
            }
        }

        #endregion

        #region DeleteCategoryCommand

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
            if (parameter != null)
            {
                DeleteCategory(parameter as CategoryViewModel);
            }
            else if (Parent != null)
            {
                Parent.DeleteCategory(this);
            }
            else
            {
                Notebook.DeleteCategory(this);
            }
        }

        #endregion

        #region RenameCommand

        private ICommand _RenameCommand = null;

        public ICommand RenameCommand
        {
            get
            {
                if (_RenameCommand == null)
                {
                    _RenameCommand = new DelegateCommand(RenameCommand_Executed);
                }
                return _RenameCommand;
            }
        }

        private void RenameCommand_Executed(object parameter)
        {
            // Categories should not have empty names:
            if (IsEditable && String.IsNullOrWhiteSpace(Name))
            {
                if (IsNew)
                {
                    Delete();
                    return;
                }
                else
                {
                    Name = "Untitled";
                }
            }

            Rename();
        }

        #endregion

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
            var item = Notebook.Locate(uri);
            if (item is CategoryViewModel)
            {
                Paste((CategoryViewModel)item);
            }
            else if (item is NoteViewModel)
            {
                Paste((NoteViewModel)item);
            }
        }

        private void Paste(CategoryViewModel category)
        {
            if (!IsPseudoCategory && !category.IsPseudoCategory)
            {
                AddChild(category);
            }
        }

        private void Paste(NoteViewModel note)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AddNote(note);
            }));
        }

        #endregion

        #endregion
    }
}
