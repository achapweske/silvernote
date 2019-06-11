/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.Models
{
    public class MessageModel : ModelBase
    {
        #region Fields

        string _Title;
        string _Content;

        #endregion

        #region Constructors

        public MessageModel()
        {

        }

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        public string Content
        {
            get
            {
                return _Content;
            }
            set
            {
                if (value != _Content)
                {
                    _Content = value;
                    RaisePropertyChanged("Content");
                }
            }
        }

        #endregion

        #region Methods

        #endregion

        #region Implementation

        #endregion
    }
}
