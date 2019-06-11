/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public interface IDataConverter
    {
        string Format { get; }
        void SetData(IDataObject obj, IList<object> items);
        IList<object> GetData(IDataObject obj);
    }
}
