/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace SilverNote.ViewModels.CategoryTree
{
    public interface ITreeNode : IList<ITreeNode>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        Uri Uri { get; }
        ITreeNode Parent { get; }
        int Depth { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        void Open();
        bool Delete();
        void Remove();
    }
}
