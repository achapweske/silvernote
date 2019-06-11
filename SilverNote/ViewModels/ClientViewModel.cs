/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SilverNote.ViewModels
{
    public class ClientViewModel : ViewModelBase<NoteClient, ClientViewModel>
    {
        #region Fields

        RequestViewModel _SelectedItem;

        #endregion

        #region Constructors

        protected override void OnInitialize()
        {
            base.OnInitialize();

            SelectedItem = Log.FirstOrDefault();

            ((INotifyCollectionChanged)Log).CollectionChanged += Log_CollectionChanged;
        }

        #endregion

        #region Properties

        public IList<RequestViewModel> Log
        {
            get { return RequestViewModel.FromCollection(Model.Log); }
        }

        public RequestViewModel SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        #endregion

        #region Implementation

        void Log_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedItem == null)
            {
                SelectedItem = Log.FirstOrDefault();
            }
        }

        #endregion


    }
}
