/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Windows;
using System.Windows.Controls;

namespace SilverNote.Controls
{
    public class NoteTabPanel : Panel
    {
        public NoteTabPanel()
        { }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size(0, 0);

            Size availableChildSize = new Size(availableSize.Width / InternalChildren.Count, availableSize.Height);

            foreach (UIElement child in InternalChildren)
            {
                child.Measure(availableChildSize);

                desiredSize.Width += child.DesiredSize.Width;
                desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
            }

            return new Size(availableSize.Width, desiredSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;

            foreach (UIElement child in InternalChildren)
            {
                Size finalChildSize = new Size(finalSize.Width / InternalChildren.Count, finalSize.Height);

                if (child.GetValue(FrameworkElement.MaxWidthProperty) != DependencyProperty.UnsetValue)
                {
                    double maxWidth = (double)child.GetValue(FrameworkElement.MaxWidthProperty);
                    if (!Double.IsNaN(maxWidth))
                    {
                        finalChildSize.Width = Math.Min(finalChildSize.Width, maxWidth);
                    }
                }

                Rect childRect = new Rect(new Point(x, 0), finalChildSize);
                child.Arrange(childRect);
                x += childRect.Width;
            }

            return finalSize;
        }
    }
}
