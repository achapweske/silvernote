/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using SilverNote.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace SilverNote.Editor
{
    public class DrawingDropHandler : DropHandlerBase
    {
        #region Constructors

        public DrawingDropHandler(NoteEditor panel)
            : base(panel)
        {

        }

        #endregion

        #region Properties

        public NoteEditor Editor
        {
            get { return (NoteEditor)Panel; }
        }

        #endregion

        #region Methods

        public override bool DragEnter(DragEventArgs e)
        {
            if (DragDropBehavior.DragContext == null)
            {
                return false;
            }
            
            var drawing = DragDropBehavior.DragContext.DragData as Shape;
            if (drawing == null)
            {
                return false;
            }

            drawing = (Shape)drawing.Clone();
            Editor.InsertClipart(drawing, oneClick: true);
            
            DragDropBehavior.DragContext.Action = DragAction.Drop;
            e.Handled = true;
            return true;
        }

        public override void DragLeave(DragEventArgs e)
        {
            e.Handled = true;
        }

        public override void Drop(DragEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

    }
}
