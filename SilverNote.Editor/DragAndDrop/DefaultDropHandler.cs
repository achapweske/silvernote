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
using System.Windows.Media;

namespace SilverNote.Editor
{
    /// <summary>
    /// A drag-and-drop handler for common content
    /// </summary>
    class DefaultDropHandler : DropHandlerBase
    {
        #region Constructors

        public DefaultDropHandler(InteractivePanel panel)
            : base(panel)
        {

        }

        #endregion

        #region Methods

        public override bool DragEnter(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat) ||
                e.Data.GetDataPresent(DataFormats.Bitmap) ||
                e.Data.GetDataPresent(DataFormats.Html))
            {
                e.Handled = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void DragOver(DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(Panel);

            if (e.Data.GetDataPresent(DataFormats.Html) ||
                e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                ShowStaticDropVisual(dropPoint);
                e.Effects = SupportedEffects(e.Data, e.AllowedEffects);
                e.Handled = true;
            }
        }

        public override void Drop(DragEventArgs e)
        {
            HideDropVisual();

            var dropPoint = e.GetPosition(Panel);
            var dropData = NDataObject.GetData(e.Data).OfType<UIElement>();

            if (DoDropStatic(dropData, dropPoint))
            {
                e.Effects = SupportedEffects(e.Data, e.AllowedEffects);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        #endregion

        #region Implementation

        private DragDropEffects SupportedEffects(IDataObject data, DragDropEffects allowedEffects)
        {
            if (DragDropBehavior.DragContext != null &&
                DragDropBehavior.DragContext.DragSource != null &&
                !LayoutHelper.IsSelfOrDescendant(Panel, DragDropBehavior.DragContext.DragSource))
            {
                // We know the drag source is NOT our owner panel, so we should only support copying
                return DragDropEffects.Copy;
            }
            else
            {
                // We don't know what the drag source is, so allow copying or moving
                return DragDropEffects.Copy | DragDropEffects.Move;
            }
        }

        #endregion
    }

}
