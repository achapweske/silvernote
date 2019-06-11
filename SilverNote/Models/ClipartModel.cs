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
using System.Xml.Serialization;
using SilverNote.Data.Models;

namespace SilverNote.Models
{
    public class ClipartModel : ModelBase, ICloneable
    {
        #region Fields

        readonly RepositoryModel _Repository;
        readonly ClipartGroupModel _ClipartGroup;
        readonly Int64 _ID;
        string _Name;
        string _Data;
        bool _NeedName;
        bool _NeedData;
        bool _IsDeleted;

        #endregion

        #region Constructors

        public ClipartModel(RepositoryModel repository, ClipartGroupModel clipartGroup, Int64 id)
        {
            _Repository = repository;
            _ClipartGroup = clipartGroup;
            _ID = id;
            _NeedName = true;
            _NeedData = true;
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return ClipartGroup.Source; }
        }

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public ClipartGroupModel ClipartGroup
        {
            get { return _ClipartGroup; }
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

        public string Data
        {
            get { return GetData(true); }
            set { SetData(value, true); }
        }

        public bool IsDeleted
        {
            get { return _IsDeleted; }
        }

        #endregion

        #region Methods

        public void Create(string name, string data)
        {
            Create(name, data, true);
        }

        public void Delete()
        {
            Delete(true);
        }

        #endregion

        #region Implementation

        public void Create(string name, string data, bool sync)
        {
            SetName(name, false);
            SetData(data, false);

            if (sync)
            {
                Source.Client.CreateClipart(ClipartGroup.ID, this.ID, name, data);
            }
        }

        public void Delete(bool sync)
        {
            _IsDeleted = true;

            if (sync)
            {
                Source.Client.DeleteClipart(ClipartGroup.ID, this.ID);
            }

            RaisePropertyChanged("IsDeleted");
        }

        #region Name

        public string GetName(bool sync)
        {
            if (sync && _NeedName)
            {
                Source.Client.GetClipartName(ClipartGroup.ID, this.ID);
            }
            return _Name;
        }

        public void SetName(string name, bool sync)
        {
            _NeedName = false;

            if (_Name != name)
            {
                _Name = name;

                if (sync)
                {
                    Source.Client.SetClipartName(ClipartGroup.ID, this.ID, name);
                }

                RaisePropertyChanged("Name");
            }
        }

        #endregion

        #region Data

        public string GetData(bool sync)
        {
            if (sync && _NeedData)
            {
                Source.Client.GetClipartData(ClipartGroup.ID, this.ID);
            }
            return _Data;
        }

        public void SetData(string data, bool sync)
        {
            _NeedData = false;

            if (!data.Equals(_Data))
            {
                _Data = data;

                if (sync)
                {
                    Source.Client.SetClipartData(ClipartGroup.ID, this.ID, data);
                }

                RaisePropertyChanged("Data");
            }
        }

        #endregion

        #endregion

        #region Data Model

        public void Update(ClipartDataModel update)
        {
            if (update.IsDeleted)
            {
                Delete(false);
            }

            if (update.Name != null)
            {
                SetName(update.Name, false);
            }

            if (update.Data != null)
            {
                SetData(update.Data, false);
            }
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
