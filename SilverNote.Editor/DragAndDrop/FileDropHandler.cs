/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace SilverNote.Editor
{
    public class FileDropHandler : DropHandlerBase
    {
        #region Constructors

        public FileDropHandler(InteractivePanel panel)
            : base(panel)
        {

        }

        #endregion

        #region Methods

        public override bool DragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
                e.Handled = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Drop(DragEventArgs e)
        {
            var dropPoint = e.GetPosition(Panel);
            var dropData = NDataObject.GetData(e.Data);

            foreach (var file in dropData.OfType<NFile>())
            {
                DoDropFile(file, dropPoint);
            }

            e.Handled = true;
        }

        #endregion

        #region Events

        public static readonly RoutedEvent FileDroppedEvent = EventManager.RegisterRoutedEvent(
            "FileDropped",
            RoutingStrategy.Bubble,
            typeof(FileDroppedEventHandler),
            typeof(FileDropHandler)
        );


        public static void AddFileDroppedHandler(DependencyObject dep, FileDroppedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(FileDroppedEvent, handler);
            }
        }

        public static void RemoveFileDroppedHandler(DependencyObject dep, FileDroppedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(FileDroppedEvent, handler);
            }
        }

        #endregion

        #region Implementation

        void DoDropFile(NFile file, Point position)
        {
            var args = new FileDroppedEventArgs(FileDroppedEvent, Panel);

            string extension = Path.GetExtension(file.FileName).ToLower();
            args.IsImage = NImage.SupportedDecoders.Contains(extension);
            args.DropAsImage = false;

            Panel.RaiseEvent(args);
            if (args.Cancel)
            {
                return;
            }

            if (args.DropAsImage)
            {
                // If this file contains an image, optionally insert it as an 
                // image instead of a file according to the user's preferences.

                var image = new NImage { Data = file.Data };
                DoDropOverlapped(image, position);
            }
            else
            {
                DoDropOverlapped(file, position);
            }
        }

        #endregion
    }
}
