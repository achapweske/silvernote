/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote.ViewModels
{
    public class MessageViewModel : ViewModelBase<MessageModel, MessageViewModel>
    {
        #region Properties

        public string Title
        {
            get { return Model.Title; }
        }

        public string Content
        {
            get { return Model.Content; }
        }

        #endregion
    }
}
