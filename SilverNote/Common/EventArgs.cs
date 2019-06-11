/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SilverNote
{
    public class EventArgs<T> : EventArgs
    {
        private readonly T _Data;

        public EventArgs(T data)
        {
            _Data = data;
        }

        public T Data
        {
            get { return _Data; }
        }
    }
}
