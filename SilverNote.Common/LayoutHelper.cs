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
using System.Diagnostics;

namespace SilverNote
{
    public static class LayoutHelper
    {
        public static Point GetPosition(Visual visual, Visual relativeTo)
        {
            return visual.TransformToVisual(relativeTo).Transform(new Point(0, 0));
        }

        public static double GetHeight(UIElement element)
        {
            if (element.Visibility == Visibility.Collapsed)
            {
                return 0;
            }
            else if (element.IsArrangeValid)
            {
                return element.RenderSize.Height;
            }
            else if (element.IsMeasureValid)
            {
                return element.DesiredSize.Height;
            }

            Debug.WriteLine("Warning: Layout height not available for " + element);
            return VisualTreeHelper.GetDescendantBounds(element).Height;
        }

        public static bool IsSelfOrDescendant(DependencyObject ancestor, DependencyObject descendant)
        {
            do
            {
                if (descendant == ancestor)
                    return true;

                if (descendant is Visual)
                    descendant = VisualTreeHelper.GetParent(descendant);
                else
                    descendant = LogicalTreeHelper.GetParent(descendant);

            } while (descendant != null);

            return false;
        }

        public static bool IsDescendant(DependencyObject ancestor, DependencyObject descendant)
        {
            return ancestor != descendant && IsSelfOrDescendant(ancestor, descendant);
        }

        public static T GetAncestor<T>(DependencyObject target, int level) where T : class
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (level < 0)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            if (level == 0)
            {
                return target as T;
            }

            if (target is Visual)
            {
                target = VisualTreeHelper.GetParent(target);
            }
            else
            {
                target = LogicalTreeHelper.GetParent(target);
            }

            if (target == null)
            {
                return null;
            }

            return GetAncestor<T>(target, level - 1);
        }

        public static T GetAncestor<T>(DependencyObject target)
        {
            if (target != null)
            {
                do
                {
                    if (target is Visual)
                        target = VisualTreeHelper.GetParent(target);
                    else
                        target = LogicalTreeHelper.GetParent(target);

                } while (target != null && !(target is T));
            }

            return (T)(object)target;
        }

        public static T GetSelfOrAncestor<T>(DependencyObject target)
        {
            while (target != null && !(target is T))
            {
                target = VisualTreeHelper.GetParent(target);
            }
            return (T)(object)target;
        }

        public static DependencyObject ChildFromDescendant(DependencyObject parent, DependencyObject descendant)
        {
            while (descendant != null)
            {
                DependencyObject next = VisualTreeHelper.GetParent(descendant);
                if (next == parent)
                    break;
                descendant = next;
            }
            return descendant;
        }

        public static DependencyObject[] ChildrenFromGeometry(DependencyObject parent, Geometry geometry)
        {
            return ChildrenFromGeometry(parent, geometry, null);
        }

        public static DependencyObject[] ChildrenFromGeometry(DependencyObject parent, Geometry geometry, Func<DependencyObject, bool> filter)
        {
            var resultList = new List<DependencyObject>();
            var resultSet = new HashSet<DependencyObject>();

            VisualTreeHelper.HitTest(
                (Visual)parent,
                (hit) =>
                {
                    var child = ChildFromDescendant(parent, hit);

                    if (child == null || resultSet.Contains(child) || (filter != null && !filter(child)))
                    {
                        return HitTestFilterBehavior.ContinueSkipSelf;
                    }
                    
                    return HitTestFilterBehavior.Continue;
                },
                (hit) =>
                {
                    var child = LayoutHelper.ChildFromDescendant(parent, hit.VisualHit);

                    if (child != null && !resultSet.Contains(child) && (filter == null || filter(child)))
                    {
                        resultSet.Add(child);
                        resultList.Add(child);
                    }
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(geometry)
            );

            return resultList.ToArray();
        }

        public static Point[] TransformToAncestor(Visual target, Visual ancestor, Point[] points)
        {
            var transform = target.TransformToAncestor(ancestor);

            var result = new Point[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                result[i] = transform.Transform(points[i]);
            }

            return result;
        }

        public static Geometry TransformToDescendant(Visual target, Visual descendant, Geometry geometry)
        {
            var transform = (Transform)target.TransformToDescendant(descendant);

            return Geometry.Combine(geometry, Geometry.Empty, GeometryCombineMode.Union, transform);
        }

        public static double Align(double coordinate, double thickness, double scale = 1.0, double bias = -0.5)
        {
            coordinate = Math.Round(coordinate * scale) / scale;

            if (Math.Round((thickness * scale) % 2) == 1)
            {
                coordinate += bias / scale;
            }

            return coordinate;
        }

        public static Point Align(Point point, double thickness, double scale = 1.0, double bias = -0.5)
        {
            double x = point.X;
            double y = point.Y;

            x = Math.Round(x * scale) / scale;
            y = Math.Round(y * scale) / scale;

            if (Math.Round((thickness * scale) % 2) == 1)
            {
                x += bias / scale;
                y += bias / scale;
            }

            return new Point(x, y);
        }
    }
}
