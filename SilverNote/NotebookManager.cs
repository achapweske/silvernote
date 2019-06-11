/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using SilverNote.Data.Models;
using SilverNote.Data.Store;
using SilverNote.Dialogs;
using SilverNote.Models;
using SilverNote.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace SilverNote
{
    /// <summary>
    /// This class manages the NotebookModel lifecycle
    /// </summary>
    public class NotebookManager : ObservableObject
    {
        #region Fields

        RepositoryManager _RepositoryManager;
        NotebookLocationModel _CurrentLocation;

        #endregion

        #region Constructors

        public NotebookManager(RepositoryManager repositoryManager)
        {
            _RepositoryManager = repositoryManager;
            _RepositoryManager.Opened += RepositoryManager_Opened;
            _RepositoryManager.Closing += RepositoryManager_Closing;
            MaxRecentLocations = 8;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Maximum number of recent locations we'll keep track of
        /// </summary>
        public int MaxRecentLocations { get; set; }

        /// <summary>
        /// Recently-opened locations
        /// </summary>
        public IList<NotebookLocationModel> RecentLocations
        {
            get { return Settings.Default.RecentNotebooks; }
        }

        /// <summary>
        /// Default location to open on application startup
        /// </summary>
        public NotebookLocationModel DefaultLocation
        {
            get
            {
                var locations = RecentLocations;
                if (locations != null)
                {
                    return locations.FirstOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Currently-selected location
        /// </summary>
        public NotebookLocationModel CurrentLocation
        {
            get 
            { 
                return _CurrentLocation; 
            }
            private set
            {
                if (value != _CurrentLocation)
                {
                    _CurrentLocation = value;
                    RaisePropertyChanged("CurrentLocation");
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Open a notebook by prompting the user for a filepath
        /// </summary>
        /// <returns>The opened notebook, or null on error</returns>
        public NotebookModel Open()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Notebooks|*.nbk|All Files|*.*";
            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                return Open(dialog.FileName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Open a notebook at the specified filepath.
        /// </summary>
        /// <param name="path">An absolute or relative filepath</param>
        /// <returns>The opened notebook, or null on error</returns>
        public NotebookModel Open(string path)
        {
            path = Path.GetFullPath(path);
            var location = new NotebookLocationModel(path);
            return Open(location);
        }

        /// <summary>
        /// Open a notebook at the specified location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public NotebookModel Open(NotebookLocationModel location)
        {
            // Open the given location

            var source = OpenLocation(location, false);
            if (source == null)
            {
                return null;
            }

            // Fetch the internal notebook ID

            Int64 internalID = GetNotebookID(source);
            if (internalID == 0)
            {
                source.Close();
                return null;
            }

            // Now get/create a NotebookModel for this notebook.

            NotebookModel notebook;
            if (location.NotebookID != 0)
            {
                notebook = _RepositoryManager.CurrentRepository.GetNotebook(location.NotebookID);
                notebook.SetName(location.Name, false);
            }
            else
            {
                notebook = _RepositoryManager.CurrentRepository.CreateNotebook(location.Name, false);
                location.NotebookID = notebook.ID;
            }
            
            // Close any previous source associated with this notebook

            if (notebook.Source.IsOpen && notebook.Source != notebook.Repository.Source)
            {
                CloseNotebook(notebook);
            }

            // Set the new source associated with this notebook

            source.Client.UriMap.Add("/notebooks/" + notebook.ID, "/notebooks/" + internalID);
            notebook.Source = source;
            SetLocation(notebook, location);

            // Add the notebook to the repository's list
            if (!_RepositoryManager.CurrentRepository.Notebooks.Contains(notebook))
            {
                _RepositoryManager.CurrentRepository.Notebooks.Add(notebook);
            }

            // Automatically select the newly-opened notebook
            _RepositoryManager.CurrentRepository.SelectedNotebook = notebook;

            return notebook;
        }

        /// <summary>
        /// Create a notebook by prompting the user for a filepath
        /// </summary>
        /// <returns>The created notebook, or null on error</returns>
        public NotebookModel Create()
        {
            while (true)
            {
                var dialog = new CreateNotebookDialog();
                dialog.Owner = App.Current.MainWindow;
                if (dialog.ShowDialog() != true)
                {
                    return null;
                }

                NotebookModel notebook;
                try
                {
                    notebook = Create(dialog.FilePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                    notebook = null;
                }

                if (notebook != null)
                {
                    return notebook;
                }
            }
        }

        /// <summary>
        /// Create a notebook at the specified filepath
        /// </summary>
        /// <param name="path">An absolute or relative filepath</param>
        /// <returns></returns>
        public NotebookModel Create(string path)
        {
            path = Path.GetFullPath(path);
            var location = new NotebookLocationModel(path);
            return Create(location);
        }

        /// <summary>
        /// Create a notebook at the specified location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public NotebookModel Create(NotebookLocationModel location)
        {
            // Check that the location doesn't already exist
            if (File.Exists(location.Repository.FilePath))
            {
                string message = String.Format("The file {0} already exists", location.Repository.FilePath);
                message = String.Format("Unable to create \"{0}\":\n\n{1}", location.Name, message);
                MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            // Open the given location
            var source = OpenLocation(location, true);
            if (source == null)
            {
                return null;
            }

            var repository = _RepositoryManager.CurrentRepository;

            // Now actually create the notebook
            var notebook = repository.CreateNotebook(location.Name, true, source);
            location.NotebookID = notebook.ID;
            SetLocation(notebook, location);

            // Automatically select the newly-created notebook
            repository.SelectedNotebook = notebook;

            return notebook;
        }

        /// <summary>
        /// Delete the notebook at the given location
        /// </summary>
        /// <param name="location"></param>
        public void Delete(NotebookLocationModel location)
        {
            try
            {
                string filePath = location.Repository.FilePath;
                System.IO.File.Delete(filePath);
            }
            catch (Exception e)
            {
                string message = String.Format("Unable to delete {0}:\n\n{1}", location.Name, e.Message);
                MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RemoveRecentLocation(location);
        }

        /// <summary>
        /// Move a notebook by prompting the user for a new filepath
        /// </summary>
        /// <param name="notebook"></param>
        /// <returns></returns>
        public bool Move(NotebookModel notebook)
        {
            NotebookLocationModel oldLocation = GetLocation(notebook);
            if (oldLocation == null)
            {
                return false;
            }

            // If notebook is initially open, we will re-open it on failure
            bool wasOpen = notebook.Source != notebook.Repository.Source && notebook.Source.IsOpen;

            NotebookLocationModel newLocation = null;
            while (true)
            {
                // Select a new location
                var dialog = new CreateNotebookDialog
                {
                    Title = "Rename Notebook...",
                    Owner = App.Current.MainWindow,
                    FilePath = oldLocation.Repository.FilePath
                };
                if (dialog.ShowDialog() != true)
                {
                    break;  // User canceled
                }
                newLocation = new NotebookLocationModel(dialog.FilePath);
                newLocation.NotebookID = notebook.ID;

                // Attempt the move
                if (!Move(notebook, newLocation))
                {
                    break;  // Move failed
                }

                return true;
            }

            if (wasOpen)
            {
                OpenNotebook(notebook);
            }

            return false;
        }

        /// <summary>
        /// Move a notebook to the specified location.
        /// 
        /// Note: this automatically closes the notebook's current source (if open)
        /// </summary>
        /// <param name="notebook"></param>
        /// <param name="newLocation"></param>
        /// <returns></returns>
        private bool Move(NotebookModel notebook, NotebookLocationModel newLocation)
        {
            var oldLocation = GetLocation(notebook);

            // Close the old location
            if (notebook.Source.IsOpen && notebook.Source != notebook.Repository.Source)
            {
                CloseNotebook(notebook);
            }

            // Move the file
            try
            {
                File.Move(oldLocation.Repository.FilePath, newLocation.Repository.FilePath);
            }
            catch (Exception e)
            {
                string message = String.Format("Unable to move {0}:\n\n{1}", newLocation.Name, e.Message);
                MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Open the new location
            notebook.Source = OpenLocation(newLocation, false);
            if (notebook.Source == null)
            {
                return false;
            }

            SetLocation(notebook, newLocation);

            // Update RecentLocations
            RemoveRecentLocation(oldLocation);
            AddRecentLocation(newLocation);

            // Update CurrentLocation
            if (CurrentLocation == oldLocation)
            {
                CurrentLocation = newLocation;
            }

            // Make notebook name match new location name
            notebook.Name = newLocation.Name;

            return true;
        }

        /// <summary>
        /// Get the location of the given notebook
        /// </summary>
        /// <param name="notebook"></param>
        /// <returns></returns>
        public NotebookLocationModel GetLocation(NotebookModel notebook)
        {
            NotebookLocationModel location;
            if (_Notebooks.TryGetValue(notebook, out location))
            {
                return location;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add an entry to our list of recent locations
        /// </summary>
        /// <param name="newLocation"></param>
        public void AddRecentLocation(NotebookLocationModel newLocation)
        {
            var recentLocations = Settings.Default.RecentNotebooks;
            if (recentLocations == null)
            {
                recentLocations = new NotebookLocationCollection();
            }

            var duplicateLocation = (from location in recentLocations
                                     where location.Repository.FilePath == newLocation.Repository.FilePath
                                     select location).FirstOrDefault();

            recentLocations.Remove(duplicateLocation);
            recentLocations.Insert(0, newLocation);
            while (recentLocations.Count > MaxRecentLocations)
            {
                recentLocations.RemoveAt(recentLocations.Count - 1);
            }

            try
            {
                Settings.Default.RecentNotebooks = recentLocations;
                Settings.Default.Save();
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }

            RaisePropertyChanged("RecentLocations");
        }

        /// <summary>
        /// Remove an entry from our list of recent locations
        /// </summary>
        /// <param name="oldLocation"></param>
        public void RemoveRecentLocation(NotebookLocationModel oldLocation)
        {
            var recentLocations = Settings.Default.RecentNotebooks;
            if (recentLocations == null)
            {
                return;
            }

            int result = recentLocations.RemoveAll(location => location.Repository.FilePath == oldLocation.Repository.FilePath);
            if (result == 0)
            {
                return;
            }

            RaisePropertyChanged("RecentLocations");
        }

        #endregion

        #region Implementation

        Dictionary<NotebookModel, NotebookLocationModel> _Notebooks = new Dictionary<NotebookModel, NotebookLocationModel>();

        void SetLocation(NotebookModel notebook, NotebookLocationModel location)
        {
            if (_Notebooks.ContainsKey(notebook))
            {
                _Notebooks.Remove(notebook);
                notebook.WhenPropertyChanged("IsDeleted", Notebook_IsDeletedChanged, false);
            }

            if (location != null)
            {
                _Notebooks.Add(notebook, location);
                notebook.WhenPropertyChanged("IsDeleted", Notebook_IsDeletedChanged, true);
                notebook.Description = location.Repository.FilePath;
            }
        }

        /// <summary>
        /// Used by Open() to determine which notebook in the selected repository should be opened
        /// </summary>
        /// <param name="source"></param>
        /// <returns>A notebook ID, or 0 on error</returns>
        private Int64 GetNotebookID(ClientManager source)
        {
            var notebooks = GetNotebookData(source);
            if (notebooks == null)
            {
                return 0;
            }

            if (notebooks.Count == 0)
            {
                // No notebooks
                string message = String.Format("Unable to read {0}:\n\n{1}", source.Client.Uri.LocalPath, "The selected file does not contain any notebooks.");
                MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                source.Close();
                return 0;
            }

            return notebooks.FirstOrDefault().ID;
        }

        /// <summary>
        /// Used by GetNotebookID() to determine the ID of the notebook in the given repository
        /// </summary>
        /// <param name="source"></param>
        /// <returns>Notebook data, or null on error</returns>
        private IList<NotebookDataModel> GetNotebookData(ClientManager source)
        {
            try
            {
                var asyncResult = source.Client.BeginGetNotebooks();
                var response = source.Client.EndGetNotebooks(asyncResult);
                return response.Notebooks;
            }
            catch (Exception e)
            {
                string message = String.Format("Unable to read {0}:\n\n{1}", source.Client.Uri.LocalPath, e.Message);
                MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
                source.Client.Resume();
                return null;
            }
        }

        /// <summary>
        /// Called when the main repository is opened.
        /// 
        /// Populate its list of notebooks and hook into its SelectingNotebook event.
        /// We'll automatically open a seperate ClientManager for each notebook
        /// when it's selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RepositoryManager_Opened(object sender, RepositoryEventArgs e)
        {
            // Do this first in case something in the populate-notebooks logic ends up selecting a notebook
            e.Repository.SelectingNotebook += Repository_SelectingNotebook;

            if (RecentLocations != null)
            {
                AddNotebooksToRepository(e.Repository, RecentLocations);
            }
        }

        /// <summary>
        /// Called when the main repository is closing.
        /// 
        /// Close any ClientManagers we've opened for its notebooks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RepositoryManager_Closing(object sender, RepositoryEventArgs e)
        {
            CloseNotebooks(e.Repository.Notebooks);

            e.Repository.SelectingNotebook -= Repository_SelectingNotebook;
        }

        /// <summary>
        /// Called when a notebook is being selected.
        /// 
        /// Open a ClientManager for the newly-selected notebook (and close the ClientManager
        /// for the previously selected one if it's one we opened).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Repository_SelectingNotebook(object sender, SelectingNotebookEventArgs e)
        {
            if (e.NewValue != null)
            {
                if (!OpenNotebook(e.NewValue))
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (e.OldValue != null)
            {
                if (e.OldValue.Source != e.OldValue.Repository.Source)
                {
                    CloseNotebook(e.OldValue);
                }
            }
        }

        /// <summary>
        /// Called when a notebook is deleted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Notebook_IsDeletedChanged(object sender, PropertyChangedEventArgs e)
        {
            var notebook = (NotebookModel)sender;
            if (!notebook.IsDeleted)
            {
                return;
            }

            if (notebook.Source != notebook.Repository.Source)
            {
                CloseNotebook(notebook);
            }

            var location = GetLocation(notebook);
            Delete(location);
        }

        /// <summary>
        /// Populate a respository's list of notebooks
        /// </summary>
        void AddNotebooksToRepository(RepositoryModel repository, IList<NotebookLocationModel> locations)
        {
            foreach (var location in locations)
            {
                AddNotebookToRepository(repository, location);
            }
        }

        void AddNotebookToRepository(RepositoryModel repository, NotebookLocationModel location)
        {
            var notebook = repository.GetNotebook(location.NotebookID);
            notebook.Source = new ClientManager(notebook.Repository);
            notebook.SetName(location.Name, false);
            SetLocation(notebook, location);

            if (!repository.Notebooks.Contains(notebook))
            {
                repository.Notebooks.Add(notebook);
            }
        }

        /// <summary>
        /// Open the given notebook
        /// </summary>
        /// <param name="notebook"></param>
        /// <returns></returns>
        bool OpenNotebook(NotebookModel notebook)
        {
            if (notebook.Source == notebook.Repository.Source)
            {
                notebook.Source = new ClientManager(notebook.Repository);
            }

            var location = GetLocation(notebook);

            if (!notebook.Source.IsOpen)
            {
                if (!OpenLocation(notebook.Source, location, false))
                {
                    return false;
                }

                // Fetch the internal notebook ID

                Int64 internalID = GetNotebookID(notebook.Source);
                if (internalID == 0)
                {
                    notebook.Source.Close();
                    return false;
                }

                notebook.Source.Client.UriMap.Add("/notebooks/" + notebook.ID, "/notebooks/" + internalID);
            }

            AddRecentLocation(location);
            CurrentLocation = location;

            return true;
        }

        void CloseNotebooks(IEnumerable<NotebookModel> notebooks)
        {
            foreach (var notebook in notebooks)
            {
                CloseNotebook(notebook);
            }
        }

        /// <summary>
        /// Close a notebook
        /// </summary>
        /// <param name="notebook"></param>
        /// <returns></returns>
        bool CloseNotebook(NotebookModel notebook)
        {
            if (notebook.Source == notebook.Repository.Source)
            {
                return false;
            }

            Messenger.Instance.Notify("Flush", notebook);
            notebook.Source.Close();
            return true;
        }

        /// <summary>
        /// Open the repository at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="create">create the repository if it does not exist</param>
        /// <returns></returns>
        private ClientManager OpenLocation(NotebookLocationModel location, bool create)
        {
            var source = new ClientManager(_RepositoryManager.CurrentRepository);

            if (OpenLocation(source, location, create))
            {
                return source;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Open the repository at the given location
        /// </summary>
        /// <param name="source"></param>
        /// <param name="location"></param>
        /// <param name="create">create the repository if it does not exist</param>
        /// <returns></returns>
        private bool OpenLocation(ClientManager source, NotebookLocationModel location, bool create)
        {
            while (true)
            {
                try
                {
                    source.Open(location.Repository, create);

                    if (location.Repository.Password != null)
                    {
                        location.Repository.Password.Dispose();
                        location.Repository.Password = null;
                    }
                    return true;
                }
                catch (Exception e)
                {
                    if (location.Repository.Password != null)
                    {
                        location.Repository.Password.Dispose();
                        location.Repository.Password = null;
                    }

                    if (e is UnauthorizedException)
                    {
                        if (!OnUnauthorized(location, e.Message))
                        {
                            return false;    // User clicked cancel
                        }
                    }
                    else if (e is NotFoundException)
                    {
                        OnNotFound(location, e.Message);
                        return false;
                    }
                    else
                    {
                        OnOpenFailed(location, e.Message);
                        return false;
                    }
                }
            }
        }

        bool OnUnauthorized(NotebookLocationModel location, string reason)
        {
            var dialog = new PasswordDialog();
            dialog.Message = reason;
            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            if (location.Repository.Password != null)
            {
                location.Repository.Password.Dispose();
            }
            location.Repository.Password = dialog.Password;
            
            return true;
        }

        void OnNotFound(NotebookLocationModel location, string reason)
        {
            OnOpenFailed(location, reason);

            _Notebooks
                .Where(item => item.Value.Repository.Uri == location.Repository.Uri)
                .Select(item => item.Key)
                .ForEach(item => item.Repository.Notebooks.Remove(item));
            
            RemoveRecentLocation(location);
        }

        void OnOpenFailed(NotebookLocationModel location, string reason)
        {
            string message = String.Format("Unable to read {0}:\n\n{1}", location.Name, reason);
            MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
