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
using System.Windows.Media;

namespace SilverNote.Editor
{
    public abstract class DropHandlerBase : IDropHandler
    {
        #region Fields

        readonly InteractivePanel _Panel;

        #endregion

        #region Constructors

        public DropHandlerBase(InteractivePanel panel)
        {
            _Panel = panel;
        }

        #endregion

        #region Properties

        public InteractivePanel Panel
        {
            get { return _Panel; }
        }

        #endregion

        #region Methods

        public abstract bool DragEnter(DragEventArgs e);

        public virtual void DragLeave(DragEventArgs e)
        {
            HideDropVisual();
        }

        public virtual void DragOver(DragEventArgs e)
        {

        }

        public virtual void Drop(DragEventArgs e)
        {
            HideDropVisual();
        }

        #endregion

        #region Implementation

        #region DoDrop

        protected void DoDropOverlapped(UIElement element, Point position)
        {
            DocumentPanel.SetPositioning(element, Positioning.Absolute);
            DocumentPanel.SetPosition(element, position);
            Panel.Append(element);
            DocumentPanel.SetPositioning(element, Positioning.Overlapped);
        }

        /// <summary>
        /// Drop the given static elements at the given position on our owner panel
        /// </summary>
        /// <param name="elements">Elements to be dropped</param>
        /// <param name="dropPoint">Position at which to drop them</param>
        /// <returns>True on success, or false if unable to drop at the given point</returns>
        protected bool DoDropStatic(IEnumerable<UIElement> elements, Point dropPoint)
        {
            UIElement target = FindStaticDropTarget(dropPoint);
            if (target == null)
            {
                return false;
            }

            int index = Panel.Children.IndexOf(target);
            if (ShouldDropAfter(target, dropPoint))
            {
                index++;
            }

            // Skip hidden elements

            if (index < Panel.Children.Count)
            {
                // starting at index, find the first visible element
                index = Panel.FindIndex<UIElement>(index, item => item.Visibility == Visibility.Visible);
                if (index == -1)
                {
                    // not found - insert at the end of the document
                    index = Panel.Children.Count;
                }
            }

            if (elements.Any())
            {
                AlignElements(target, elements);
                Panel.InsertRange(index, elements);
            }

            return true;
        }

        #endregion

        #region DropVisual

        private DrawingVisual _DropVisual;

        protected DrawingVisual DropVisual
        {
            get { return _DropVisual ?? (_DropVisual = new DrawingVisual()); }
        }

        protected void ShowInlineDropVisual(Point dropPoint)
        {
            UIElement child = FindStaticDropTarget(dropPoint);
            if (child == null)
            {
                return;
            }


        }

        protected void ShowStaticDropVisual(Point dropPoint)
        {
            UIElement child = FindStaticDropTarget(dropPoint);
            if (child == null || DocumentPanel.GetPositioning(child) != Positioning.Static)
            {
                return;
            }

            Rect childRect = new Rect(new Point(0, 0), child.RenderSize);
            childRect = child.TransformToAncestor(Panel).TransformBounds(childRect);

            Rect dropRegion;
            if (ShouldDropAfter(child, dropPoint))
            {
                dropRegion = new Rect(childRect.X, childRect.Bottom, childRect.Width, 0);
            }
            else
            {
                dropRegion = new Rect(childRect.X, childRect.Top, childRect.Width, 0);
            }

            var formattable = child as IFormattable;
            if (formattable != null && !formattable.GetBoolProperty(TextParagraph.IsHeadingPropertyName))
            {
                double indent = formattable.GetDoubleProperty(TextParagraph.LeftMarginPropertyName);
                if (formattable.GetBoolProperty(TextParagraph.IsListItemPropertyName))
                {
                    indent += 10;
                }
                dropRegion.X += indent;
                dropRegion.Width -= indent;
            }

            ShowStaticDropVisual(dropRegion);
        }

        void ShowStaticDropVisual(Rect rect)
        {
            var dc = DropVisual.RenderOpen();
            try
            {
                Pen pen = new Pen(Brushes.DarkGray, 2);
                pen.Freeze();
                dc.DrawRectangle(null, pen, rect);
            }
            finally
            {
                dc.Close();
            }

            ShowDropVisual();
        }

        protected void ShowDropVisual()
        {
            if (_DropVisual != null && !Panel.BackgroundVisuals.Contains(_DropVisual))
            {
                Panel.BackgroundVisuals.Add(_DropVisual);
            }
        }

        protected void HideDropVisual()
        {
            if (_DropVisual != null)
            {
                Panel.BackgroundVisuals.Remove(_DropVisual);
            }
        }

        #endregion

        #region Helpers

        protected UIElement FindStaticDropTarget(Point dropPoint)
        {
            // Find the element at the given point.
            // If none found at that point, find the visible child closest to that point

            var target = Panel.ChildFromPoint(dropPoint);
            if (target == null)
            {
                var visibleChildren = Panel.Children.OfType<UIElement>().Where(element => element.Visibility == Visibility.Visible);
                target = Panel.ClosestChildFromPoint(visibleChildren, dropPoint);
            }

            return target;
        }

        /// <summary>
        /// Determine whether a drop operation should occur before or after the given child
        /// </summary>
        /// <param name="child">The child to be tested</param>
        /// <param name="dropPoint">The position where the drop will occur</param>
        /// <returns>true if the drop should occur AFTER child, or false if before</returns>
        protected bool ShouldDropAfter(UIElement child, Point dropPoint)
        {
            if (child is NHeading)
            {
                return true;
            }

            // Return true if dropPoint is below the center of child

            dropPoint = Panel.TransformToDescendant(child).Transform(dropPoint);

            return dropPoint.Y > child.RenderSize.Height / 2;
        }

        /// <summary>
        /// Align a set of elements with the given reference element
        /// </summary>
        /// <param name="refElement"></param>
        /// <param name="elements"></param>
        void AlignElements(UIElement refElement, IEnumerable<UIElement> elements)
        {
            double refIndent = 0;
            int refListLevel = 0;
            IListStyle refListStyle = null;

            if (refElement is TextParagraph)
            {
                var refParagraph = (TextParagraph)refElement;
                refIndent = refParagraph.LeftMargin;
                refListLevel = refParagraph.ListLevel;
                refListStyle = refParagraph.ListStyle;
            }

            double deltaIndent = 0;
            int deltaListLevel = 0;

            if (elements.FirstOrDefault() is TextParagraph)
            {
                var firstElement = (TextParagraph)elements.FirstOrDefault();
                deltaIndent = refIndent - firstElement.LeftMargin;
                deltaListLevel = refListLevel - firstElement.ListLevel;
            }

            foreach (TextParagraph paragraph in elements.OfType<TextParagraph>())
            {
                var indent = paragraph.LeftMargin;
                var listLevel = paragraph.ListLevel;

                indent += deltaIndent;
                listLevel += deltaListLevel;

                paragraph.LeftMargin = indent;
                paragraph.ListLevel = listLevel;

                if (refListStyle != null)
                {
                    IListStyle listStyle = refListStyle;

                    for (int i = refListLevel; i < listLevel; i++)
                    {
                        listStyle = listStyle.NextStyle;
                    }

                    for (int i = refListLevel; i > listLevel; i--)
                    {
                        listStyle = listStyle.PreviousStyle;
                    }

                    paragraph.ListStyle = listStyle;
                }
            }
        }

        #endregion

        #endregion
    }
}
