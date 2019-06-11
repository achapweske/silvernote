/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SilverNote.Common;
using SilverNote.Models;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security;

namespace SilverNote.ViewModels
{
    public class RepositoryViewModel : ViewModelBase<RepositoryModel, RepositoryViewModel>
    {
        #region Constructors

        protected override void OnInitialize()
        {
            Model.WhenPropertyChanged("SelectedNotebook", (i, j) => RaisePropertyChanged("IsNotebookSelected"));
            Model.WhenPropertyChanged("SelectedNotebook", (i, j) => RaisePropertyChanged("HasPassword"));
            Model.WhenPropertyChanged("SelectedNotebook", (i, j) => RaisePropertyChanged("CanChangePassword"));

            var notebooks = Model.Notebooks as INotifyCollectionChanged;
            if (notebooks != null)
            {
                notebooks.CollectionChanged += (i, j) => RaisePropertyChanged("HasNotebooks");
            }

            var clipartGroups = Model.ClipartGroups as INotifyCollectionChanged;
            if (clipartGroups != null)
            {
                clipartGroups.CollectionChanged += (i, j) => RaisePropertyChanged("ClipartGroups");
            }
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return Model.IsEmpty; }
            set { Model.IsEmpty = value; }
        }

        public ClientViewModel Client
        {
            get 
            {
                return ClientViewModel.FromModel(Model.Source.Client);
            }
        }

        public bool HasNotebooks
        {
            get { return Model.Notebooks.Count > 0; }
        }

        public IList<NotebookViewModel> Notebooks
        {
            get { return NotebookViewModel.FromCollection(Model.Notebooks); }
        }

        public bool IsNotebookSelected
        {
            get { return Model.SelectedNotebook != null; }
        }

        public NotebookViewModel SelectedNotebook
        {
            get { return NotebookViewModel.FromModel(Model.SelectedNotebook); }
            set { Model.SelectedNotebook = (value != null) ? value.Model : null; }
        }

        public IList<ClipartGroupViewModel> ClipartGroups
        {
            get 
            { 
                return ClipartGroupViewModel
                    .FromCollection(Model.ClipartGroups)
                    .Except(InternalGroups)
                    .OrderBy(group => group.Name)
                    .ToArray(); 
            }
        }

        public ClipartGroupViewModel[] InternalGroups
        {
            get { return new ClipartGroupViewModel[] { Lines, Markers }; }
        }

        public ClipartGroupViewModel Lines
        {
            get { return ClipartGroupViewModel.FromModel(Model.Lines); }
        }

        public ClipartGroupViewModel Markers
        {
            get { return ClipartGroupViewModel.FromModel(Model.Markers); }
        }

        public bool HasPassword
        {
            get
            {
                if (SelectedNotebook != null)
                {
                    return SelectedNotebook.Model.HasPassword;
                }
                else
                {
                    return Model.HasPassword;
                }
            }
        }

        public bool CanChangePassword
        {
            get
            {
                if (SelectedNotebook != null)
                {
                    return SelectedNotebook.Model.Source.CanChangePassword;
                }
                else
                {
                    return Model.Source.CanChangePassword;
                }
            }
        }

        #endregion

        #region Operations

        public NotebookViewModel CreateNotebook(string name = null)
        {
            if (name == null)
            {
                var dialog = new Dialogs.NewNotebookDialog();
                dialog.Owner = App.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return null;
                }
                name = dialog.NotebookName;
            }

            var model = Model.CreateNotebook(name);
            Model.SelectedNotebook = model;

            return NotebookViewModel.FromModel(model);
        }

        public void DeleteNotebook(NotebookViewModel notebook)
        {
            string message = String.Format("Deleting notebook \"{0}\".\n\n", notebook.Name);
            message += "ALL NOTES IN THIS NOTEBOOK WILL BE DELETED.\n\n";
            message += "Are you sure you want to proceed?";
                
            if (MessageBox.Show(message, "SilverNote", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                notebook.Model.Delete();
            }
        }

        public ClipartGroupViewModel CreateClipartGroup(string name)
        {
            var clipartGroup = Model.CreateClipartGroup(name);

            return ClipartGroupViewModel.FromModel(clipartGroup);
        }

        public void DeleteClipartGroup(ClipartGroupViewModel clipartGroup)
        {
            string message = String.Format("Deleting group \"{0}\".\n\n", clipartGroup.Name);
            message += "ALL DRAWINGS IN THIS GROUP WILL BE DELETED.\n\n";
            message += "Are you sure you want to proceed?";

            if (MessageBox.Show(message, "SilverNote", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
            {
                clipartGroup.Model.Delete();
            }
        }

        public void ChangePassword(SecureString oldPassword, SecureString newPassword)
        {
            if (SelectedNotebook != null)
            {
                SelectedNotebook.Model.Source.Client.ChangePassword(oldPassword, newPassword);
            }
            else
            {
                Model.Source.Client.ChangePassword(oldPassword, newPassword);
            }
        }

        public void Crash()
        {
#if DEBUG
            Window window = null;
            window.ShowDialog();
#endif
        }

        #endregion

        #region Commands

        private ICommand _SelectNotebookCommand;

        public ICommand SelectNotebookCommand
        {
            get
            {
                if (_SelectNotebookCommand == null)
                {
                    _SelectNotebookCommand = new DelegateCommand(o => SelectedNotebook = o as NotebookViewModel);
                }
                return _SelectNotebookCommand;
            }
        }

        private DelegateCommand _CreateClipartGroupCommand;

        public ICommand CreateClipartGroupCommand
        {
            get
            {
                if (_CreateClipartGroupCommand == null)
                {
                    _CreateClipartGroupCommand = new DelegateCommand(o => CreateClipartGroup(o as string));
                }
                return _CreateClipartGroupCommand;
            }
        }

        private DelegateCommand _DeleteClipartGroupCommand;

        public ICommand DeleteClipartGroupCommand
        {
            get
            {
                if (_DeleteClipartGroupCommand == null)
                {
                    _DeleteClipartGroupCommand = new DelegateCommand(o => DeleteClipartGroup(o as ClipartGroupViewModel));
                }
                return _DeleteClipartGroupCommand;
            }
        }

        private DelegateCommand _CrashCommand;

        public ICommand CrashCommand
        {
            get
            {
                if (_CrashCommand == null)
                {
                    _CrashCommand = new DelegateCommand(o => Crash());
                }
                return _CrashCommand;
            }
        }

        #endregion

    }
}
