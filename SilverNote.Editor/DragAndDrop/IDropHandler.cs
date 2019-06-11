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
    public interface IDropHandler
    {
        bool DragEnter(DragEventArgs e);
        void DragLeave(DragEventArgs e);
        void DragOver(DragEventArgs e);
        void Drop(DragEventArgs e);
    }
}
