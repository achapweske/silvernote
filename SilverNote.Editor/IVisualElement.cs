/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SilverNote.Editor
{
    public interface IVisualElement
    {
        Rect VisualBounds { get; }
    }

    public static class VisualElement
    {
        public static Rect GetVisualBounds(Visual visual)
        {
            var element = visual as IVisualElement;
            if (element != null)
            {
                return element.VisualBounds;
            }
            else
            {
                return VisualTreeHelper.GetDescendantBounds(visual);
            }
        }

        public static Rect GetVisualBounds(Visual visual, Visual relativeTo)
        {
            Rect bounds = GetVisualBounds(visual);

            return visual.TransformToAncestor(relativeTo).TransformBounds(bounds);
        }

        public static Rect GetVisualBounds(IEnumerable<Visual> visuals, Visual relativeTo)
        {
            Rect result = Rect.Empty;

            foreach (var visual in visuals)
            {
                if (result.IsEmpty)
                {
                    result = GetVisualBounds(visual, relativeTo);
                }
                else
                {
                    result.Union(GetVisualBounds(visual, relativeTo));
                }
            }

            return result;
        }

    }
    
}
