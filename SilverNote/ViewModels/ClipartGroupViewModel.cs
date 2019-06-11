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
using System.Windows.Input;
using System.Diagnostics;
using SilverNote.Common;
using SilverNote.Models;

namespace SilverNote.ViewModels
{
    public class ClipartGroupViewModel : ViewModelBase<ClipartGroupModel, ClipartGroupViewModel>
    {
        protected override void OnInitialize()
        {
            var items = Model.Items as INotifyCollectionChanged;
            if (items != null)
            {
                items.CollectionChanged += Items_CollectionChanged;
            }
        }

        #region Properties

        public RepositoryViewModel Repository
        {
            get { return RepositoryViewModel.FromModel(Model.Repository); }
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public IList<ClipartViewModel> Items
        {
            get 
            { 
                return ClipartViewModel
                    .FromCollection(Model.Items)
                    .OrderBy(item => item.Model.ID)
                    .ToArray(); 
            }
        }

        void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("Items");
        }

        #endregion

        #region Operations

        public ClipartViewModel CreateClipart(string name, string data)
        {
            ClipartModel result = Model.CreateClipart(name, data);

            return ClipartViewModel.FromModel(result);
        }

        public void DeleteClipart(ClipartViewModel clipart)
        {
            if (clipart != null)
            {
                clipart.Model.Delete();
            }
        }

        #endregion

        #region Commands

        private ICommand _CreateClipartCommand = null;

        public ICommand CreateClipartCommand
        {
            get
            {
                if (_CreateClipartCommand == null)
                {
                    _CreateClipartCommand = new DelegateCommand(o =>
                    {
                        var canvas = o as Editor.NCanvas;
                        if (canvas != null)
                        {
                            CreateClipart("", canvas.ToSVG());
                        }
                        else
                        {
                            CreateClipart("", "");
                        }
                    });
                }
                return _CreateClipartCommand;
            }
        }

        private ICommand _DeleteClipartCommand = null;

        public ICommand DeleteClipartCommand
        {
            get
            {
                if (_DeleteClipartCommand == null)
                {
                    _DeleteClipartCommand = new DelegateCommand(o => DeleteClipart(o as ClipartViewModel));
                }
                return _DeleteClipartCommand;
            }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
