/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Behaviors;
using SilverNote.Commands;
using SilverNote.Common;
using SilverNote.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SilverNote.ViewModels.CategoryTree
{
    public class CategoryTreeViewModel : CompositeCollection, INotifyPropertyChanged
    {
        #region Fields

        NotebookViewModel _Notebook;
        IList<ITreeNode> _SelectedItems;

        #endregion

        #region Constructors

        public CategoryTreeViewModel(NotebookViewModel notebook)
        {
            _Notebook = notebook;
            _SelectedItems = new List<ITreeNode>();

            Add(new CollectionContainer 
            { 
                Collection = new CategoryNodeCollection(this, null, notebook.SpecialCategories, 0) 
            });

            Add(new CollectionContainer 
            { 
                Collection = new CategoryNodeCollection(this, null, notebook.Categories, 0) 
            });
        }

        #endregion

        #region Properties

        public NotebookViewModel Notebook
        {
            get { return _Notebook; }
        }

        public IList<ITreeNode> SelectedItems
        {
            get { return _SelectedItems; }
        }

        #endregion

        #region Operations

        public void OpenItem(ITreeNode item)
        {
            item.Open();
        }

        public void OpenItems(IEnumerable<ITreeNode> items)
        {
            items.ForEach(OpenItem);
        }

        public void OpenSelectedItems()
        {
            OpenItems(SelectedItems);
        }

        public void Copy()
        {
            var uris = ClipboardConverter.Convert(SelectedItems);

            Clipboard.SetDataObject(uris);
        }

        public void Paste()
        {
            var data = NClipboard.GetDataObject();
            if (data != null)
            {
                Paste(data);
            }
        }

        public bool CanPaste()
        {
            var data = NClipboard.GetDataObject();
            if (data != null)
            {
                return CanPaste(data);
            }
            else
            {
                return false;
            }
        }

        private void Paste(IDataObject data)
        {
            var uriList = (List<Uri>)data.GetData(typeof(List<Uri>));
            if (uriList != null)
            {
                Paste(uriList);
            }
        }

        private bool CanPaste(IDataObject data)
        {
            return data.GetDataPresent(typeof(List<Uri>));
        }

        private void Paste(IEnumerable<Uri> items)
        {
            foreach (var item in items)
            {
                PasteItem(item);
            }
        }

        private void PasteItem(Uri uri)
        {
            var category = Notebook.Locate(uri) as CategoryViewModel;
            if (category != null)
            {
                PasteItem(category);
            }
        }

        private void PasteItem(CategoryViewModel category)
        {
            if (!category.IsPseudoCategory)
            {
                Notebook.AddTopLevelCategory(category);
            }
        }

        public bool DeleteItem(ITreeNode item)
        {
            return item.Delete();
        }

        public bool DeleteItems(IEnumerable<ITreeNode> items)
        {
            var searchResults = items.OfType<SearchResultNode>();

            if (searchResults.Any())
            {
                if (!Notebook.DeleteNotes(searchResults.Select(node => node.SearchResult.Note)))
                {
                    return false;
                }

                items = items.Except(searchResults);
            }

            return items.All(DeleteItem);
        }

        public bool DeleteSelectedItems()
        {
            return DeleteItems(SelectedItems.ToArray());
        }

        public void RemoveItem(ITreeNode item)
        {
            item.Remove();
        }

        public void RemoveItems(IEnumerable<ITreeNode> items)
        {
            items.ForEach(RemoveItem);
        }

        public void RemoveSelectedItems()
        {
            RemoveItems(SelectedItems.ToArray());
        }

        #endregion

        #region Commands

        #region CreateNoteCommand

        ICommand _CreateNoteCommand;

        public ICommand CreateNoteCommand
        {
            get { return _CreateNoteCommand ?? (_CreateNoteCommand = new DelegateCommand(CreateNoteCommand_Executed, CreateNoteCommand_CanExecute)); }
        }

        void CreateNoteCommand_Executed(object parameter)
        {
            var node = SelectedItems.FirstOrDefault() as CategoryNode;
            if (node != null)
            {
                var note = node.Category.Notebook.CreateNote();
                note.AddToCategory(node.Category);

                if (!node.Category.IsPseudoCategory)
                {
                    node.IsExpanded = true;
                }
            }
        }

        bool CreateNoteCommand_CanExecute(object parameter)
        {
            return SelectedItems.Count == 1 && SelectedItems[0] is CategoryNode;
        }

        #endregion

        #region CreateCategoryCommand

        ICommand _CreateCategoryCommand;

        public ICommand CreateCategoryCommand
        {
            get { return _CreateCategoryCommand ?? (_CreateCategoryCommand = new DelegateCommand(CreateCategoryCommand_Executed, CreateCategoryCommand_CanExecute)); }
        }

        void CreateCategoryCommand_Executed(object parameter)
        {
            if (SelectedItems.Count == 0)
            {
                Notebook.CreateCategory("Untitled");
                return;
            }

            var node = SelectedItems.FirstOrDefault() as CategoryNode;
            if (node != null)
            {
                node.Category.CreateCategory("Untitled");

                if (!node.Category.IsPseudoCategory)
                {
                    node.IsExpanded = true;
                }
            }
        }

        bool CreateCategoryCommand_CanExecute(object parameter)
        {
            return SelectedItems.Count == 0 || SelectedItems.Count == 1 && SelectedItems[0] is CategoryNode;
        }

        #endregion

        #region AddToCategoryCommand

        ICommand _AddToCategoryCommand;

        public ICommand AddToCategoryCommand
        {
            get { return _AddToCategoryCommand ?? (_AddToCategoryCommand = new DelegateCommand(AddToCategoryCommand_Executed, AddToCategoryCommand_CanExecute)); }
        }

        void AddToCategoryCommand_Executed(object parameter)
        {
            var category = parameter as CategoryViewModel;
            if (category == null)
            {
                return;
            }

            foreach (var item in SelectedItems.OfType<SearchResultNode>().ToArray())
            {
                category.AddNote(item.SearchResult.Note);
            }
        }

        bool AddToCategoryCommand_CanExecute(object parameter)
        {
            return parameter is CategoryViewModel && SelectedItems.OfType<SearchResultNode>().Any();
        }

        #endregion

        #region MoveToCommand

        ICommand _MoveToCommand;

        public ICommand MoveToCommand
        {
            get { return _MoveToCommand ?? (_MoveToCommand = new DelegateCommand(MoveToCommand_Executed, MoveToCommand_CanExecute)); }
        }

        void MoveToCommand_Executed(object parameter)
        {
            var items = ClipboardConverter.Normalize(SelectedItems);

            // Move to category
            var category = parameter as CategoryViewModel;
            if (category != null)
            {
                foreach (var item in items.ToArray())
                {
                    if (item is CategoryNode)
                    {
                        category.AddChild(((CategoryNode)item).Category);
                    }
                    else if (item is SearchResultNode)
                    {
                        category.AddNote(((SearchResultNode)item).SearchResult.Note);
                        item.Remove();
                    }
                }
            }

            // Move to notebook
            var notebook = parameter as NotebookViewModel;
            if (notebook != null)
            {
                foreach (var item in items.OfType<CategoryNode>().ToArray())
                {
                    notebook.AddTopLevelCategory(item.Category);
                }
            }
        }

        bool MoveToCommand_CanExecute(object parameter)
        {
            return (parameter is CategoryViewModel || parameter is NotebookViewModel) && SelectedItems.Any();
        }

        #endregion

        #region OpenCommand

        ICommand _OpenCommand;

        public ICommand OpenCommand
        {
            get { return _OpenCommand ?? (_OpenCommand = new DelegateCommand(OpenCommand_Executed)); }
        }

        void OpenCommand_Executed(object parameter)
        {
            if (parameter == null)
            {
                OpenSelectedItems();
            }
            else if (parameter is IEnumerable<ITreeNode>)
            {
                OpenItems((IEnumerable<ITreeNode>)parameter);
            }
            else if (parameter is ITreeNode)
            {
                OpenItems((ITreeNode)parameter);
            }
        }

        #endregion

        #region CutCommand

        ICommand _CutCommand;

        public ICommand CutCommand
        {
            get { return _CutCommand ?? (_CutCommand = new DelegateCommand(CutCommand_Executed)); }
        }

        void CutCommand_Executed(object parameter)
        {
            Copy();
            RemoveSelectedItems();
        }

        #endregion

        #region CopyCommand

        ICommand _CopyCommand;

        public ICommand CopyCommand
        {
            get { return _CopyCommand ?? (_CopyCommand = new DelegateCommand(CopyCommand_Executed)); }
        }

        void CopyCommand_Executed(object parameter)
        {
            Copy();
        }

        #endregion

        #region PasteCommand

        ICommand _PasteCommand;

        public ICommand PasteCommand
        {
            get { return _PasteCommand ?? (_PasteCommand = new DelegateCommand(PasteCommand_Executed, PasteCommand_CanExecute)); }
        }

        void PasteCommand_Executed(object parameter)
        {
            if (SelectedItems.Count == 0)
            {
                Paste();
            }
            else if (SelectedItems.Count == 1 && SelectedItems[0] is CategoryNode)
            {
                ((CategoryNode)SelectedItems[0]).Paste();
            }
        }

        bool PasteCommand_CanExecute(object parameter)
        {
            if (SelectedItems.Count == 0)
            {
                return CanPaste();
            }
            else if (SelectedItems.Count == 1 && SelectedItems[0] is CategoryNode)
            {
                return ((CategoryNode)SelectedItems[0]).CanPaste();
            }

            return false;
        }

        #endregion

        #region DeleteCommand

        ICommand _DeleteCommand;

        public ICommand DeleteCommand
        {
            get { return _DeleteCommand ?? (_DeleteCommand = new DelegateCommand(DeleteCommand_Executed)); }
        }

        void DeleteCommand_Executed(object parameter)
        {
            if (parameter == null)
            {
                DeleteSelectedItems();
            }
            else if (parameter is IEnumerable<ITreeNode>)
            {
                DeleteItems((IEnumerable<ITreeNode>)parameter);
            }
            else if (parameter is ITreeNode)
            {
                DeleteItems((ITreeNode)parameter);
            }
        }

        #endregion

        #region RemoveCommand

        ICommand _RemoveCommand;

        public ICommand RemoveCommand
        {
            get { return _RemoveCommand ?? (_RemoveCommand = new DelegateCommand(RemoveCommand_Executed)); }
        }

        void RemoveCommand_Executed(object parameter)
        {
            if (parameter == null)
            {
                RemoveSelectedItems();
            }
            else if (parameter is IEnumerable<ITreeNode>)
            {
                RemoveItems((IEnumerable<ITreeNode>)parameter);
            }
            else if (parameter is ITreeNode)
            {
                RemoveItems((ITreeNode)parameter);
            }
        }

        #endregion

        #region RenameCommand

        ICommand _RenameCommand;

        public ICommand RenameCommand
        {
            get { return _RenameCommand ?? (_RenameCommand = new DelegateCommand(RenameCommand_Executed)); }
        }

        void RenameCommand_Executed(object parameter)
        {
            if (parameter == null)
            {
                parameter = SelectedItems.OfType<CategoryNode>().FirstOrDefault();
            }
            
            if (parameter is CategoryNode)
            {
                ((CategoryNode)parameter).Category.Rename();
            }
        }

        #endregion

        #region SearchCommand

        ICommand _SearchCommand;

        public ICommand SearchCommand
        {
            get { return _SearchCommand ?? (_SearchCommand = new DelegateCommand(SearchCommand_Executed)); }
        }

        void SearchCommand_Executed(object parameter)
        {
            if (parameter == null)
            {
                parameter = SelectedItems.OfType<CategoryNode>().FirstOrDefault();
            }

            var node = parameter as CategoryNode;
            if (node != null)
            {
                NViewCommands.SearchNotebook.Execute(node.Category, App.Current.MainWindow);
            }
        }

        #endregion

        #region DragCommand

        ICommand _DragCommand;

        public ICommand DragCommand
        {
            get { return _DragCommand ?? (_DragCommand = new DelegateCommand(DragCommand_Executed)); }
        }

        void DragCommand_Executed(object parameter)
        {
            var context = DragDropBehavior.DragContext;
            if (context == null)
            {
                throw new Exception("This command must be invoked by DragDropBehavior");
            }

            var dragData = ((IEnumerable)context.DragData).OfType<SearchResultNode>();

            // Only non-null when drop target is in same process and it utilizes DragDropBehavior
            var targetElement = context.DropTarget as FrameworkElement;
            if (targetElement != null)
            {
                CategoryNode targetCategory = null;
                if (targetElement.DataContext is CategoryNode)
                {
                    targetCategory = (CategoryNode)targetElement.DataContext;
                }
                else if (targetElement.DataContext is SearchResultNode)
                {
                    targetCategory = (CategoryNode)((SearchResultNode)targetElement.DataContext).Parent;
                }
                else if (targetElement.DataContext is NotebookViewModel || targetElement.DataContext is CategoryTreeViewModel)
                {
                    return;
                }

                if (targetCategory != null)
                {
                    dragData = dragData.Where(node => node.SearchResult.Search.Category != targetCategory.Category);
                }
            }

            foreach (var item in dragData.ToArray())
            {
                item.Remove();
            }
        }

        #endregion

        #region DropCommand

        ICommand _DropCommand = null;

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

        #endregion

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

    }
}
