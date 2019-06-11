/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Security;
using SilverNote.Data.Models;
using SilverNote.Dialogs;
using System.ComponentModel;
using SilverNote.Clipart;
using SilverNote.Client;

namespace SilverNote.Models
{
    public class RepositoryModel : ModelBase, IDisposable
    {
        #region Fields

        ClientManager _Source;
        Dictionary<Int64, NotebookModel> _Notebooks;
        Dictionary<Int64, ClipartGroupModel> _ClipartGroups;
        ObservableCollection<NotebookModel> _VisibleNotebooks;
        ObservableCollection<ClipartGroupModel> _VisibleClipartGroups;
        NotebookModel _SelectedNotebook;
        bool _NeedNotebooks;
        bool _NeedSelectedNotebook;
        bool _NeedClipartGroups;
        bool _IsEmpty;

        #endregion

        #region Constructors

        public RepositoryModel()
        {
            _Source = new ClientManager(this);
            _Source.ClientChanged += Source_ClientChanged;
            _Notebooks = new Dictionary<Int64, NotebookModel>();
            _ClipartGroups = new Dictionary<Int64, ClipartGroupModel>();
            _VisibleNotebooks = new ObservableCollection<NotebookModel>();
            _VisibleClipartGroups = new ObservableCollection<ClipartGroupModel>();
            _NeedNotebooks = true;
            _NeedSelectedNotebook = true;
            _NeedClipartGroups = true;
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return _Source; }
        }

        public bool HasPassword
        {
            get { return Source.Client != null && Source.Client.HasPassword; }
        }

        public IList<NotebookModel> Notebooks
        {
            get { return GetNotebooks(true); }
        }

        public NotebookModel SelectedNotebook
        {
            get { return GetSelectedNotebook(true); }
            set { SetSelectedNotebook(value, true); }
        }

        public IList<ClipartGroupModel> ClipartGroups
        {
            get { return GetClipartGroups(true); }
        }

        public ClipartGroupModel Lines
        {
            get { return GetClipartGroup(LINES_GROUP_ID); }
        }

        public ClipartGroupModel Markers
        {
            get { return GetClipartGroup(MARKERS_GROUP_ID); }
        }

        public bool IsEmpty
        {
            get
            {
                return _IsEmpty;
            }
            set
            {
                if (value != _IsEmpty)
                {
                    _IsEmpty = value;
                    RaisePropertyChanged("IsEmpty");
                }
            }
        }

        #endregion

        #region Methods

        public NotebookModel CreateNotebook(string name)
        {
            return CreateNotebook(name, true);
        }

        public NotebookModel GetNotebook(Int64 id, bool autoCreate = true)
        {
            NotebookModel result;
            if (TryGetNotebook(id, out result))
            {
                return result;
            }
            else if (autoCreate)
            {
                return AllocNotebook(id);
            }
            else
            {
                return null;
            }
        }

        public ClipartGroupModel CreateClipartGroup(string name)
        {
            return CreateClipartGroup(name, true);
        }

        public ClipartGroupModel GetClipartGroup(Int64 id, bool autoCreate = true)
        {
            ClipartGroupModel result;
            if (TryGetClipartGroup(id, out result))
            {
                return result;
            }
            else if (autoCreate)
            {
                return AllocClipartGroup(id);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Events

        public event SelectingNotebookDelegate SelectingNotebook;

        protected bool RaiseSelectingNotebook(NotebookModel oldValue, NotebookModel newValue)
        {
            if (SelectingNotebook != null)
            {
                var e = new SelectingNotebookEventArgs(oldValue, newValue);
                SelectingNotebook(this, e);
                return !e.Cancel;
            }
            else
            {
                return true;
            }
        }

        #endregion

        #region Implementation

        #region Source

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

        #region Notebooks

        NotebookModel AllocNotebook(Int64 id)
        {
            var result = new NotebookModel(this, id);
            result.WhenPropertyChanged("IsDeleted", Notebook_IsDeletedChanged);
            _Notebooks.Add(id, result);
            return result;
        }

        bool TryGetNotebook(Int64 id, out NotebookModel result)
        {
            return _Notebooks.TryGetValue(id, out result);
        }

        void Notebook_IsDeletedChanged(object sender, PropertyChangedEventArgs e)
        {
            var notebook = (NotebookModel)sender;

            if (notebook.IsDeleted)
            {
                OnNotebookDeleted(notebook);
            }
        }

        protected void OnNotebookDeleted(NotebookModel notebook)
        {
            if (GetSelectedNotebook(false) == notebook)
            {
                SelectNextNotebook(notebook);
            }

            _VisibleNotebooks.Remove(notebook);
        }

        public NotebookModel CreateNotebook(string name, bool sync, ClientManager source = null)
        {
            Int64 id = IDGenerator.NextID();
            while (_Notebooks.ContainsKey(id))
            {
                id = IDGenerator.NextID();
            }

            var notebook = AllocNotebook(id);
            notebook.Source = source;
            notebook.Create(name, sync);
            _VisibleNotebooks.Add(notebook);

            return notebook;
        }

        public IList<NotebookModel> GetNotebooks(bool sync)
        {
            if (_NeedNotebooks && sync)
            {
                Source.Client.GetNotebooks();
            }

            return _VisibleNotebooks;
        }

        public void SetNotebooks(IList<NotebookModel> notebooks, bool sync)
        {
            _NeedNotebooks = false;

            foreach (var notebook in notebooks)
            {
                if (!notebook.IsDeleted && !_VisibleNotebooks.Contains(notebook))
                {
                    _VisibleNotebooks.Add(notebook);
                }
            }
        }

        #endregion

        #region SelectedNotebook

        public NotebookModel GetSelectedNotebook(bool sync)
        {
            if (sync && _NeedSelectedNotebook)
            {
                Source.Client.GetSelectedNotebook();
            }
            return _SelectedNotebook;
        }

        public void SetSelectedNotebook(NotebookModel notebook, bool sync)
        {
            if (notebook == _SelectedNotebook)
            {
                return;
            }

            if (!RaiseSelectingNotebook(_SelectedNotebook, notebook))
            {
                return;
            }

            _NeedSelectedNotebook = false;

            if (_SelectedNotebook != null)
            {
                _SelectedNotebook.IsSelected = false;
            }

            _SelectedNotebook = notebook;

            if (_SelectedNotebook != null)
            {
                _SelectedNotebook.IsSelected = true;
            }

            if (sync)
            {
                Int64 notebookID = notebook != null ? notebook.ID : 0;
                Source.Client.SetSelectedNotebook(notebookID);
            }

            RaisePropertyChanged("SelectedNotebook");
        }

        void SelectNextNotebook(NotebookModel notebook)
        {
            var notebooks = GetNotebooks(false);
            if (notebooks != null)
            {
                SelectNextNotebook(notebooks, notebook);
            }
            else
            {
                SelectedNotebook = null;
            }
        }

        void SelectNextNotebook(IList<NotebookModel> notebooks, NotebookModel notebook = null)
        {
            SelectNextNotebook(notebooks, notebooks.IndexOf(notebook));
        }

        void SelectNextNotebook(IList<NotebookModel> notebooks, int index = -1)
        {
            if (index + 1 < notebooks.Count)
            {
                SelectedNotebook = notebooks[index + 1];
            }
            else if (index - 1 >= 0)
            {
                SelectedNotebook = notebooks[index - 1];
            }
            else
            {
                SelectedNotebook = null;
            }
        }

        #endregion

        #region ClipartGroups

        const Int64 LINES_GROUP_ID = 1;
        const string DEFAULT_LINES_GROUP_NAME = "Default Lines";
        const string LINES_GROUP_NAME = "Lines (favorites)";

        const Int64 MARKERS_GROUP_ID = 2;
        const string DEFAULT_MARKERS_GROUP_NAME = "Default Markers";
        const string MARKERS_GROUP_NAME = "Markers (favorites)";

        ClipartGroupModel AllocClipartGroup(Int64 id)
        {
            var result = new ClipartGroupModel(id, this);
            _ClipartGroups.Add(id, result);
            result.WhenPropertyChanged("IsDeleted", ClipartGroup_IsDeleted);
            return result;
        }

        ClipartGroupModel CreateClipartGroup(string name, bool sync)
        {
            Int64 id;
            if (name == DEFAULT_LINES_GROUP_NAME)
            {
                id = LINES_GROUP_ID;
                name = LINES_GROUP_NAME;
            }
            else if (name == DEFAULT_MARKERS_GROUP_NAME)
            {
                id = MARKERS_GROUP_ID;
                name = MARKERS_GROUP_NAME;
            }
            else
            {
                do
                {
                    id = IDGenerator.NextID();

                } while (_ClipartGroups.ContainsKey(id));
            }

            var group = GetClipartGroup(id);
            group.Create(name, sync);
            _VisibleClipartGroups.Add(group);

            return group;
        }

        void ClipartGroup_IsDeleted(object sender, PropertyChangedEventArgs e)
        {
            var cliparatGroup = (ClipartGroupModel)sender;
            _VisibleClipartGroups.Remove(cliparatGroup);
        }

        bool TryGetClipartGroup(Int64 id, out ClipartGroupModel result)
        {
            return _ClipartGroups.TryGetValue(id, out result);
        }

        IList<ClipartGroupModel> GetClipartGroups(bool sync)
        {
            if (_NeedClipartGroups && sync)
            {
                Source.Client.GetClipartGroups();
            }

            return _VisibleClipartGroups;
        }

        void SetClipartGroups(IList<ClipartGroupModel> clipartGroups, bool sync)
        {
            _NeedClipartGroups = false;

            foreach (var clipartGroup in clipartGroups)
            {
                if (!clipartGroup.IsDeleted && !_VisibleClipartGroups.Contains(clipartGroup))
                {
                    _VisibleClipartGroups.Add(clipartGroup);
                }
            }
        }

        #endregion

        #endregion

        #region Data Model

        public void Update(RepositoryDataModel update)
        {
            if (update.Notebooks != null)
            {
                UpdateNotebooks(update.Notebooks);
            }

            if (update.SelectedNotebookID != 0 && _NeedSelectedNotebook)
            {
                var notebook = GetNotebook(update.SelectedNotebookID);

                SetSelectedNotebook(notebook, false);
            }

            if (update.ClipartGroups != null)
            {
                UpdateClipartGroups(update.ClipartGroups);
            }
        }

        public void UpdateNotebooks(IList<NotebookDataModel> updates)
        {
            var notebooks = updates.Select(update =>
            {
                UpdateNotebook(update);
                return GetNotebook(update.ID);
            });

            SetNotebooks(notebooks.ToArray(), false);

            if (!notebooks.Any())
            {
                IsEmpty = true;
            }
        }

        public void UpdateNotebook(NotebookDataModel update)
        {
            var notebook = GetNotebook(update.ID);
            notebook.Update(update);
        }

        public void UpdateClipartGroups(IList<ClipartGroupDataModel> updates)
        {
            var groups = updates.Select(update => 
            {
                UpdateClipartGroup(update);
                return GetClipartGroup(update.ID);
            });

            SetClipartGroups(groups.ToArray(), false);

            if (!groups.Any())
            {
                ClipartLoader.LoadClipart(this);
            }
        }

        public void UpdateClipartGroup(ClipartGroupDataModel update)
        {
            var group = GetClipartGroup(update.ID);
            group.Update(update);
        }

        public void UpdateFile(FileDataModel update)
        {
            var notebook = GetNotebook(update.NotebookID);
            notebook.UpdateFile(update);
        }

        public void UpdateCategory(CategoryDataModel data)
        {
            var notebook = GetNotebook(data.NotebookID);
            notebook.UpdateCategory(data);
        }

        #endregion

        #region IDisposable

        private bool _IsDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_Source != null)
                {
                    _Source.Dispose();
                }
            }

            _IsDisposed = true;
        }

        #endregion

    }

    public delegate void SelectingNotebookDelegate(object sender, SelectingNotebookEventArgs e);

    public class SelectingNotebookEventArgs
    {
        readonly NotebookModel _OldValue;
        readonly NotebookModel _NewValue;

        public SelectingNotebookEventArgs(NotebookModel oldValue, NotebookModel newValue)
        {
            _OldValue = oldValue;
            _NewValue = newValue;
        }

        public NotebookModel OldValue
        {
            get { return _OldValue; }
        }

        public NotebookModel NewValue
        {
            get { return _NewValue; }
        }

        public bool Cancel { get; set; }
    }
}
