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
    public delegate void FileDroppedEventHandler(object sender, FileDroppedEventArgs e);

    public class FileDroppedEventArgs : RoutedEventArgs
    {
        public FileDroppedEventArgs()
        {

        }

        public FileDroppedEventArgs(RoutedEvent routedEvent)
            : base(routedEvent)
        {
            
        }

        public FileDroppedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            
        }

        public bool IsImage { get; set; }
        public bool DropAsImage { get; set; }
        public bool Cancel { get; set; }
    }
}
