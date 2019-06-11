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
    public class FileModel : ModelBase, ICloneable
    {
        #region Fields

        readonly RepositoryModel _Repository;
        readonly NotebookModel _Notebook;
        readonly NoteModel _Note;
        readonly string _Name;
        bool _NeedData;
        byte[] _Data;
        bool _IsDeleted;

        #endregion

        #region Constructors

        public FileModel(RepositoryModel repository, NotebookModel notebook, NoteModel note, string name)
        {
            _Repository = repository;
            _Notebook = notebook;
            _Note = note;
            _Name = name;
            _NeedData = true;
        }

        #endregion

        #region Properties

        public ClientManager Source
        {
            get { return Note.Source; }
        }

        public RepositoryModel Repository
        {
            get { return _Repository; }
        }

        public NotebookModel Notebook
        {
            get { return _Notebook; }
        }

        public NoteModel Note
        {
            get { return _Note; }
        }

        public string Name
        {
            get { return _Name; }
        }

        public byte[] Data
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

        public void Create()
        {
            Create(true);
        }

        public void Delete()
        {
            Delete(true);
        }

        #endregion

        #region Data Model

        public void Update(FileDataModel update)
        {
            if (update.Data != null && _NeedData)
            {
                SetData(update.Data, false);
            }
        }

        #endregion

        #region Implementation

        void Create(bool sync)
        {
            _NeedData = false;

            if (sync)
            {
                Source.Client.CreateFile(Notebook.ID, Note.ID, this.Name);
            }
        }

        void Delete(bool sync)
        {
            _IsDeleted = true;

            if (sync)
            {
                Source.Client.DeleteFile(Notebook.ID, Note.ID, this.Name);
            }

            RaisePropertyChanged("IsDeleted");
        }

        byte[] GetData(bool sync)
        {
            if (sync && _NeedData)
            {
                Source.Client.GetFile(Notebook.ID, Note.ID, this.Name);
            }

            return _Data;
        }

        void SetData(byte[] data, bool sync)
        {
            _NeedData = false;

            if (_Data == null || !data.SequenceEqual(_Data))
            {
                _Data = data;

                if (sync)
                {
                    Source.Client.UpdateFile(Notebook.ID, Note.ID, this.Name, data);
                }

                RaisePropertyChanged("Data");
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
