/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Common;
using SilverNote.Data.Store;
using SilverNote.Dialogs;
using SilverNote.Models;
using SilverNote.Properties;
using SilverNote.ViewModels;
using SilverNote.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace SilverNote
{
    /// <summary>
    /// This class manages the RepositoryModel lifecycle.
    /// </summary>
    public class RepositoryManager : ObservableObject
    {
        #region Fields

        RepositoryLocationModel _CurrentLocation;
        RepositoryModel _CurrentRepository;

        #endregion

        #region Constructors

        public RepositoryManager()
        {
            
        }

        #endregion

        #region Properties

        /// <summary>
        /// Currently-opened location
        /// </summary>
        public RepositoryLocationModel CurrentLocation
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

        /// <summary>
        /// Currently-opened repository
        /// </summary>
        public RepositoryModel CurrentRepository
        {
            get
            {
                return _CurrentRepository;
            }
            private set
            {
                if (value != _CurrentRepository)
                {
                    _CurrentRepository = value;
                    RaisePropertyChanged("CurrentRepository");
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked immediately after a repository has been opened
        /// </summary>
        public event RepositoryEventDelegate Opened;

        /// <summary>
        /// Invoked immediately before a repository is closed
        /// </summary>
        public event RepositoryEventDelegate Closing;

        #endregion

        #region Methods

        /// <summary>
        /// Open the respository at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public bool Open(RepositoryLocationModel location)
        {
            RepositoryModel repository = null;
            bool autoCreate = false;

            while (repository == null)
            {
                try
                {
                    repository = new RepositoryModel();

                    repository.Source.Open(location, autoCreate);

                    if (location.Password != null)
                    {
                        location.Password.Dispose();
                        location.Password = null;
                    }
                }
                catch (Exception e)
                {
                    repository = null;

                    if (location.Password != null)
                    {
                        location.Password.Dispose();
                        location.Password = null;
                    }

                    if (e is NotFoundException)
                    {
                        if (OnNotFound(location, e.Message))
                        {
                            autoCreate = true;
                            continue;
                        }
                    }
                    else if (e is UnauthorizedException)
                    {
                        if (OnUnauthorized(location, e.Message))
                        {
                            autoCreate = false;
                            continue;
                        }
                    }
                    else
                    {
                        OnOpenFailed(location, e.Message);
                    }

                    location = null;
                    autoCreate = false;
                }

            }

            Close();    // close the previously-opened repository

            CurrentLocation = location;
            CurrentRepository = repository;

            RaiseOpened(repository);

            return true;
        }

        /// <summary>
        /// Close the currently-opened repository if any
        /// </summary>
        public void Close()
        {
            var repository = CurrentRepository;
            if (repository != null)
            {
                RaiseClosing(repository);
                repository.Source.Close();
                CurrentRepository = null;
            }
        }

        #endregion

        #region Implementation

        protected void RaiseOpened(RepositoryModel repository)
        {
            if (Opened != null)
            {
                Opened(this, new RepositoryEventArgs(repository));
            }
        }

        protected void RaiseClosing(RepositoryModel repository)
        {
            if (Closing != null)
            {
                Closing(this, new RepositoryEventArgs(repository));
            }
        }

        private bool OnNotFound(RepositoryLocationModel location, string reason)
        {
            if (MessageBox.Show(reason, "SilverNote", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return false;
            }

            return true;
        }

        private bool OnUnauthorized(RepositoryLocationModel location, string reason)
        {
            var dialog = new PasswordDialog();
            dialog.Message = reason;
            if (dialog.ShowDialog() == true)
            {
                location.Password = dialog.Password;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnOpenFailed(RepositoryLocationModel location, string reason)
        {
            var locationViewModel = RepositoryLocationViewModel.FromModel(location);

            string message = String.Format("Unable to open {0}:\n\n{1}", locationViewModel.DisplayName, reason);

            MessageBox.Show(message, "SilverNote", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }

    public delegate void RepositoryEventDelegate(object sender, RepositoryEventArgs e);

    public class RepositoryEventArgs
    {
        readonly RepositoryModel _Repository;

        public RepositoryEventArgs(RepositoryModel repository)
        {
            _Repository = repository;
        }

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }
    }
}
