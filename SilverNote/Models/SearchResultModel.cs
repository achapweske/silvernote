/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SilverNote.Data.Models;

namespace SilverNote.Models
{
    public class SearchResultModel : ModelBase
    {
        public struct OffsetLength
        {
            public OffsetLength(int offset, int length)
            {
                Offset = offset;
                Length = length;
            }

            public readonly int Offset;
            public readonly int Length;
        }

        #region Fields

        SearchModel _Search;
        NoteModel _Note;
        string _Text;
        OffsetLength[] _Offsets;

        #endregion

        #region Constructors

        public SearchResultModel(SearchModel search)
        {
            _Search = search;
        }

        public SearchResultModel(SearchModel search, NoteModel note)
            : this(search)
        {
            _Note = note;
        }

        #endregion

        #region Properties

        public SearchModel Search
        {
            get { return _Search; }
        }

        public NoteModel Note
        {
            get 
            { 
                return _Note; 
            }
            private set
            {
                _Note = value;
                RaisePropertyChanged("Note");
            }
        }

        public string Text
        {
            get 
            { 
                return _Text; 
            }
            private set
            {
                _Text = value;
                RaisePropertyChanged("Text");
            }
        }

        public OffsetLength[] Offsets
        {
            get 
            { 
                return _Offsets; 
            }
            private set
            {
                _Offsets = value;
                RaisePropertyChanged("Offsets");
            }
        }

        #endregion

        #region Data Model

        public void Update(SearchResultDataModel newResult)
        {
            if (newResult.Note != null)
            {
                var note = Search.Notebook.GetNote(newResult.Note.ID);
                note.Update(newResult.Note);
                Note = note;
            }

            if (newResult.Text != null)
            {
                Text = newResult.Text;
            }

            if (newResult.Offsets != null)
            {
                Offsets = (from offset in newResult.Offsets 
                           select new OffsetLength(offset.Offset, offset.Length))
                           .ToArray();              
            }
        }

        #endregion
    }
}
