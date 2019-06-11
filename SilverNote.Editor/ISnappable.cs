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
    public interface ISnappable
    {
        bool Snap(Point[] points, ref Vector delta);
    }

    public static class Snappable
    {
        public static bool Snap(this ISnappable target, Rect rect, ref Vector delta)
        {
            var points = new Point[] { 
                rect.TopLeft, 
                rect.TopRight, 
                rect.BottomLeft, 
                rect.BottomRight
            };

            return target.Snap(points, ref delta);
        }

        public static Point Snap(this ISnappable target, Point point)
        {
            Point[] points = new Point[] { point };

            Vector delta = new Vector(0, 0);

            if (target.Snap(points, ref delta))
            {
                return point + delta;
            }
            else
            {
                return point;
            }
        }
    }
}
