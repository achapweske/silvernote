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
    public class ShapeHitTest
    {
        #region Static Methods

        /// <summary>
        /// Get the first drawing at the specified point
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <returns>hit test result, or null if there are no drawings at the given point</returns>
        public static Shape HitOne(Visual reference, Point point)
        {
            return HitTest(reference, point, ShapeHitTestFlags.None, 1).FirstOrDefault();
        }

        /// <summary>
        /// Get all drawings at the specified point
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <returns>hit test result, or an empty array if there are no drawings at the given point</returns>
        public static Shape[] HitAll(Visual reference, Point point)
        {
            return HitTest(reference, point, ShapeHitTestFlags.None, int.MaxValue);
        }

        /// <summary>
        /// Get the first drawing at the specified point
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <param name="flags">filtering flags</param>
        /// <returns>hit test result, or null if there are no drawings at the given point</returns>
        public static Shape HitOne(Visual reference, Point point, ShapeHitTestFlags flags)
        {
            return HitTest(reference, point, flags, 1).FirstOrDefault();
        }

        /// <summary>
        /// Get all drawings at the specified point
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <param name="flags">filtering flags</param>
        /// <returns>hit test result, or an empty array if there are no drawings at the given point</returns>
        public static Shape[] HitAll(Visual reference, Point point, ShapeHitTestFlags flags)
        {
            return HitTest(reference, point, flags, int.MaxValue);
        }

        /// <summary>
        /// Get all drawings at the specified point
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="points">the points to hit test against</param>
        /// <param name="flags">filtering flags</param>
        /// <returns>hit test result, or an empty array if there are no drawings at the given point</returns>
        public static Shape[] HitAll(Visual reference, IEnumerable<Point> points, ShapeHitTestFlags flags)
        {
            return HitTest(reference, points, flags, int.MaxValue);
        }

        /// <summary>
        /// Get all drawings at the specified point.
        /// 
        /// If a hit drawing is the descendant of another drawing (such as NDrawingGroup), the ancestor drawing is returned.
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <param name="flags">filtering flags</param>
        /// <param name="limit">maximum number of results to return</param>
        /// <returns>hit test result, or an empty array if there are no drawings at the given point</returns>
        public static Shape[] HitTest(Visual reference, Point point, ShapeHitTestFlags flags, int limit)
        {
            var helper = new ShapeHitTest(reference, point, flags, limit);

            helper.HitTest();

            return helper.Results.ToArray();
        }

        /// <summary>
        /// Get all drawings at the specified point.
        /// 
        /// If a hit drawing is the descendant of another drawing (such as NDrawingGroup), the ancestor drawing is returned.
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="points">the points to hit test against</param>
        /// <param name="flags">filtering flags</param>
        /// <param name="limit">maximum number of results to return</param>
        /// <returns>hit test result, or an empty array if there are no drawings at the given point</returns>
        public static Shape[] HitTest(Visual reference, IEnumerable<Point> points, ShapeHitTestFlags flags, int limit)
        {
            var helper = new ShapeHitTest(reference, default(Point), flags, limit);

            foreach (var point in points)
            {
                helper.Point = point;
                helper.HitTest();
            }

            return helper.Results.ToArray();
        }

        /// <summary>
        /// Determine if the given point is over a text area
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <returns>true if the given point is over a text area; otherwise false</returns>
        public static bool IsText(Visual reference, Point point)
        {
            HitTestResult hit = VisualTreeHelper.HitTest(reference, point);

            return (hit != null) && (hit.VisualHit is TextLineVisual);
        }

        /// <summary>
        /// Determine if the given point is over a text area
        /// </summary>
        /// <param name="reference">the Visual to hit test</param>
        /// <param name="point">the point to hit test against</param>
        /// <param name="relativeTo">point is relative to this Visual</param>
        /// <returns>true if the given point is over a text area; otherwise false</returns>
        public static bool IsText(Visual reference, Point point, Visual relativeTo)
        {
            point = relativeTo.TransformToVisual(reference).Transform(point);

            return IsText(reference, point);
        }

        #endregion

        #region Constructors

        public ShapeHitTest(Visual reference, Point point, ShapeHitTestFlags flags, int limit)
        {
            Reference = reference;
            Point = point;
            Flags = flags;
            Limit = limit;
            Results = new List<Shape>();
        }

        #endregion

        #region Properties

        public Visual Reference { get; set; }

        public Point Point { get; set; }

        public ShapeHitTestFlags Flags { get; set; }

        public int Limit { get; set; }

        public List<Shape> Results { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Hit test against the Visual specified by Reference, using the point or geometry specified by Parameters.
        /// 
        /// The results will be added to the Results member.
        /// </summary>
        public void HitTest()
        {
            var parameters = CreateParameters(Point, Flags);

            VisualTreeHelper.HitTest(Reference, FilterCallback, ResultCallback, parameters);
        }

        /// <summary>
        /// Hit test filter callback
        /// </summary>
        public HitTestFilterBehavior FilterCallback(DependencyObject dep)
        {
            var drawing = dep as Shape;
            if (drawing == null)
            {
                return HitTestFilterBehavior.ContinueSkipSelf;
            }

            if (drawing.Canvas != null && drawing.Canvas.Visibility != Visibility.Visible)
            {
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            }

            // Get the top-level drawing

            if (!Flags.HasFlag(ShapeHitTestFlags.ReturnDirectHits))
            {
                while (drawing != Reference && drawing.Parent is Shape)
                {
                    drawing = (Shape)drawing.Parent;
                }
            }

            if (Results.Contains(drawing))
            {
                return HitTestFilterBehavior.ContinueSkipSelf;
            }

            // Filter selected drawings

            if (Flags.HasFlag(ShapeHitTestFlags.ExcludeSelected) && 
                drawing.Canvas != null && drawing.Canvas.IsSelected)
            {
                return HitTestFilterBehavior.ContinueSkipSelf;
            }

            // Filter based on drawing type

            var drawingType = drawing.GetType();

            if (Flags.HasFlag(ShapeHitTestFlags.ExcludeLines) && drawingType == typeof(PolyLine) ||
                Flags.HasFlag(ShapeHitTestFlags.ExcludeQuadraticBeziers) && drawingType == typeof(QuadraticBezier) ||
                Flags.HasFlag(ShapeHitTestFlags.ExcludeQuadraticCurves) && drawingType == typeof(QuadraticCurve) ||
                Flags.HasFlag(ShapeHitTestFlags.ExcludePolyLines) && drawingType == typeof(PolyLine) ||
                Flags.HasFlag(ShapeHitTestFlags.ExcludeRoutedLines) && drawingType == typeof(RoutedLine))
            {
                return HitTestFilterBehavior.ContinueSkipSelf;
            }

            return HitTestFilterBehavior.Continue;
        }

        /// <summary>
        /// Hit test result callback
        /// </summary>
        public HitTestResultBehavior ResultCallback(HitTestResult hit)
        {
            var drawing = (Shape)hit.VisualHit;

            // Get the top-level drawing

            if (!Flags.HasFlag(ShapeHitTestFlags.ReturnDirectHits))
            {
                while (drawing != Reference && drawing.Parent is Shape)
                {
                    drawing = (Shape)drawing.Parent;
                }
            }

            // Filter results

            var hitPoint = Reference.TransformToVisual(drawing).Transform(Point);

            if (Flags.HasFlag(ShapeHitTestFlags.HitSnaps) || Flags.HasFlag(ShapeHitTestFlags.HitHandles))
            {
                if (Flags.HasFlag(ShapeHitTestFlags.HitDrawings) ||
                    Flags.HasFlag(ShapeHitTestFlags.HitSnaps) && drawing.HitSnap(hitPoint) != -1 ||
                    Flags.HasFlag(ShapeHitTestFlags.HitHandles) && drawing.HitHandle(hitPoint) != -1)
                {
                    Results.Add(drawing);
                }
            }
            else
            {
                Results.Add(drawing);
            }

            // Limit number of results as specified

            if (Results.Count >= Limit)
            {
                return HitTestResultBehavior.Stop;
            }
            else
            {
                return HitTestResultBehavior.Continue;
            }
        }

        #endregion

        #region Implementation

        public static HitTestParameters CreateParameters(Point point, ShapeHitTestFlags flags)
        {
            if (flags.HasFlag(ShapeHitTestFlags.HitSnaps) ||
                flags.HasFlag(ShapeHitTestFlags.HitHandles))
            {
                double width = 4, height = 4;
                double x = point.X - width / 2;
                double y = point.Y - height / 2;

                var rectangle = new Rect(x, y, width, height);
                var geometry = new RectangleGeometry(rectangle);
                return new GeometryHitTestParameters(geometry);
            }
            else
            {
                return new PointHitTestParameters(point);
            }
        }

        #endregion

    }

    /// <summary>
    /// These flags specify the behavior of ShapeHitTest.HitTest()
    /// </summary>
    public enum ShapeHitTestFlags
    {
        None = 0,
        HitDrawings = 0x01,
        HitSnaps = 0x02,
        HitHandles = 0x04,
        ExcludeSelected = 0x08,
        ExcludeLines = 0x10,
        ExcludeQuadraticBeziers = 0x20,
        ExcludeQuadraticCurves = 0x40,
        ExcludePolyLines = 0x80,
        ExcludeRoutedLines = 0x100,
        ExcludeLineTypes = ExcludeLines | ExcludeQuadraticBeziers | ExcludeQuadraticCurves | ExcludePolyLines | ExcludeRoutedLines,
        ReturnDirectHits
    };

}
