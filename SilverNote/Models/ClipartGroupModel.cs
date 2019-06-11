/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using SilverNote.Data.Models;
using System.Collections.ObjectModel;

namespace SilverNote.Models
{
    public class ClipartGroupModel : ModelBase, ICloneable
    {
        #region Fields

        readonly RepositoryModel _Repository;
        readonly Int64 _ID;
        string _Name;
        Dictionary<Int64, ClipartModel> _Clipart;
        ObservableCollection<ClipartModel> _VisibleClipart;
        bool _NeedName;
        bool _NeedClipart;
        bool _ClipartPending;
        bool _IsDeleted;

        #endregion

        #region Constructors

        public ClipartGroupModel(Int64 id, RepositoryModel repository)
        {
            _Repository = repository;
            _ID = id;
            _Clipart = new Dictionary<Int64, ClipartModel>();
            _VisibleClipart = new ObservableCollection<ClipartModel>();
            _NeedName = true;
            _NeedClipart = true;
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return Repository.Source; }
        }

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public Int64 ID
        {
            get { return _ID; }
        }

        public string Name
        {
            get { return GetName(true); }
            set { SetName(value, true); }
        }

        public IList<ClipartModel> Items
        {
            get { return GetItems(true); }
        }

        public bool IsDeleted
        {
            get { return _IsDeleted; }
        }

        #endregion

        #region Methods

        public void Create(string name)
        {
            Create(name, true);
        }

        public void Delete()
        {
            Delete(true);
        }

        public ClipartModel CreateClipart(string name, string data)
        {
            return CreateClipart(name, data, true);
        }

        public ClipartModel GetClipart(Int64 id, bool autoCreate = true)
        {
            ClipartModel result;
            if (TryGetClipart(id, out result))
            {
                return result;
            }
            else if (autoCreate)
            {
                return AllocClipart(id);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Implementation

        public void Create(string name, bool sync)
        {
            _NeedName = false;
            _NeedClipart = false;

            SetName(name, false);

            if (sync)
            {
                Source.Client.CreateClipartGroup(this.ID, name);
            }
        }

        void Delete(bool sync)
        {
            _IsDeleted = true;

            if (sync)
            {
                Source.Client.DeleteClipartGroup(this.ID);
            }

            RaisePropertyChanged("IsDeleted");
        }

        #region Name

        protected string GetName(bool sync)
        {
            if (_NeedName && sync)
            {
                Source.Client.GetClipartGroupName(this.ID);
            }
            return _Name;
        }

        protected void SetName(string name, bool sync)
        {
            _NeedName = false;

            if (name != _Name)
            {
                _Name = name;

                if (sync)
                {
                    Source.Client.SetClipartGroupName(this.ID, name);
                }

                RaisePropertyChanged("Name");
            }
        }

        #endregion

        #region Items

        ClipartModel AllocClipart(Int64 id)
        {
            var result = new ClipartModel(Repository, this, id);
            _Clipart.Add(id, result);
            result.WhenPropertyChanged("IsDeleted", Clipart_IsDeletedChanged);
            return result;
        }

        ClipartModel CreateClipart(string name, string data, bool sync)
        {
            Int64 id = IDGenerator.NextID();
            while (_Clipart.ContainsKey(id))
            {
                id = IDGenerator.NextID();
            }

            var clipart = AllocClipart(id);
            clipart.Create(name, data, sync);
            _VisibleClipart.Add(clipart);

            return clipart;
        }

        void Clipart_IsDeletedChanged(object sender, PropertyChangedEventArgs e)
        {
            var clipart = (ClipartModel)sender;

            if (clipart.IsDeleted)
            {
                _VisibleClipart.Remove(clipart);
            }
        }

        bool TryGetClipart(Int64 id, out ClipartModel result)
        {
            return _Clipart.TryGetValue(id, out result);
        }

        IList<ClipartModel> GetItems(bool sync)
        {
            if (sync && _NeedClipart && !_ClipartPending)
            {
                _ClipartPending = true;
                Source.Client.GetClipartItems(ID);
            }

            return _VisibleClipart;
        }

        void SetItems(ClipartModel[] items, bool sync)
        {
            _NeedClipart = false;
            _ClipartPending = false;

            foreach (var item in items)
            {
                if (!item.IsDeleted && !_VisibleClipart.Contains(item))
                {
                    _VisibleClipart.Add(item);
                }
            }
        }

        #endregion

        #endregion

        #region Data Model

        public void Update(ClipartGroupDataModel update)
        {
            if (update.IsDeleted)
            {
                Delete(false);
            }

            if (update.Name != null)
            {
                SetName(update.Name, false);
            }

            if (update.Items != null)
            {
                UpdateItems(update.Items);
            }
        }

        public void UpdateItems(IList<ClipartDataModel> updates)
        {
            var items = updates.Select(update =>
            {
                UpdateClipart(update);
                return GetClipart(update.ID);
            });

            SetItems(items.ToArray(), false);
        }

        public void UpdateClipart(ClipartDataModel data)
        {
            var clipart = GetClipart(data.ID);
            clipart.Update(data);
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
