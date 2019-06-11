/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using SilverNote.Common;
using DOM;
using DOM.SVG;
using DOM.Helpers;

namespace SilverNote.Editor
{
    public class RoutedLine : PolyLine
    {
        #region Constructors

        public RoutedLine()
        {

        }

        public RoutedLine(IEnumerable<Point> points)
            : base(points)
        {
            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public RoutedLine(params double[] coordinates)
            : base(coordinates)
        {
            StrokeBrush = Brushes.Black;
            StrokeWidth = 2;
        }

        public RoutedLine(RoutedLine copy)
            : base(copy)
        {

        }

        #endregion

        #region NLineBase

        public override Point StartPoint
        {
	        get 
	        { 
		        return base.StartPoint;
	        }
	        set 
	        { 
		        base.StartPoint = value;

                if (Points.Count >= 2)
                {
                    Route(StartPoint, EndPoint);
                }
	        }
        }

        public override Point EndPoint
        {
            get
            {
                return base.EndPoint;
            }
            set
            {
                base.EndPoint = value;

                if (Points.Count >= 2)
                {
                    Route(StartPoint, EndPoint);
                }
            }
        }

        #endregion

        #region NDrawing

        public override void Place(Point position)
        {
            Points.Clear();
            Points.Add(position);
        }

        public override bool CompletePlacing()
        {
            return true;
        }

        public override void Draw(Point point)
        {
            EndPoint = point;
        }

        public override bool CompleteDrawing()
        {
            return true;
        }

        public override bool CancelDrawing()
        {
            return true;
        }

        public override int HandleCount
        {
            get
            {
                return Points.Count + 1;
            }
        }

        protected override Point GetHandleInternal(int index)
        {
            if (index < 0 || index > HandleCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index == 0)
            {
                return StartPoint;
            }

            if (index == 1)
            {
                return EndPoint;
            }

            return Line.Evaluate(Points[index-2], Points[index-1], 0.5);
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            if (index < 0 || index > HandleCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (index == 0)
            {
                StartPoint = value;
                return;
            }

            if (index == 1)
            {
                EndPoint = value;
                return;
            }


            var delta = value - GetHandleInternal(index);
            var point1 = Points[index - 2];
            var point2 = Points[index - 1];

            if (Math.Abs(point2.X - point1.X) < Math.Abs(point2.Y - point1.Y))
            {
                // Treat as vertical segment - move in horizontal plane

                delta = new Vector(delta.X, 0);
            }
            else
            {
                // Treat as horizontal segment - move in vertical plane

                delta = new Vector(0, delta.Y);
            }

            Points[index - 2] += delta;
            Points[index - 1] += delta;
            InvalidateRender();
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            base.OnRenderVisual(dc);

            if (Canvas != null && DocumentElement.GetDebugFlags(Canvas) == NDebugFlags.All && Map != null)
            {
                Pen pen = new Pen(Brushes.Red, 1);
                pen.Freeze();
                dc.PushTransform(GeometryTransform);
                Map.Draw(dc, pen);
                dc.Pop();
            }
        }

        #endregion

        #region Algorithms

        class RouteNode : AStarNode
        {
            public bool IsStartNode { get; set; }

            public bool IsEndNode { get; set; }

            public Direction Direction { get; set; }

            private double Length { get; set; }

            private int Bends { get; set; }

            double LengthFrom(Point point)
            {
                return (this.Point - point).Length;
            }

            Direction DirectionFrom(Point point)
            {
                return Directions.FromVector(this.Point - point);
            }

            int ComputeBends(Direction from, Direction to)
            {
                return (from == Direction.None || to == Direction.None || from == to) ? 0 : 1;
            }

            public override double CostFrom(AStarNode _fromNode)
            {
                RouteNode fromNode = (RouteNode)_fromNode;

                var length = LengthFrom(fromNode.Point);
                var direction = DirectionFrom(fromNode.Point);
                var bends = ComputeBends(fromNode.Direction, direction);

                if (this.IsEndNode && Direction != Direction.None && direction != Direction)
                {
                    return Double.PositiveInfinity;
                }

                if (fromNode.IsStartNode && bends > 0)
                {
                    return Double.PositiveInfinity;
                }

                var totalLength = fromNode.Length + length;
                var totalBends = fromNode.Bends + bends;
                var totalCost = totalLength * (1 + totalBends);

                if (totalCost < this.Cost)
                {
                    Direction = direction;
                    Bends = totalBends;
                    Length = totalLength;
                }

                return totalCost;
            }

            double LengthTo(Point point)
            {
                return Math.Abs(point.X - Point.X) + Math.Abs(point.Y - Point.Y);
            }

            int BendsTo(Point point, Direction direction)
            {
                if (direction == this.Direction &&
                    direction == Directions.FromVector(point - this.Point))
                {
                    return 0;
                }
                if ((Directions.Left(direction) == this.Direction || Directions.Right(direction) == this.Direction) &&
                    Directions.FromVector(point - this.Point).HasFlag(direction))
                {
                    return 1;
                }
                if (Directions.Reverse(direction) == this.Direction &&
                    Directions.FromVector(point - this.Point) != direction)
                {
                    return 2;
                }

                return 4;
            }

            public override double HeuristicCostTo(AStarNode _node)
            {
                RouteNode node = (RouteNode)_node;

                return LengthTo(node.Point) * (1 + BendsTo(node.Point, node.Direction));
            }
        }

        RouteMap Map { get; set; }

        AStarPathFinder PathFinder = new AStarPathFinder();

        public void Route(Point startPoint, Point endPoint)
        {
            var startDirection = GetStartDirection(startPoint);

            var startNode = new RouteNode
            {
                Point = startPoint,
                Direction = startDirection,
                IsStartNode = true
            };

            var endDirection = GetEndDirection(endPoint);

            var endNode = new RouteNode
            {
                Point = endPoint,
                Direction = endDirection,
                IsEndNode = true
            };

            Map = new RouteMap
            {
                StartPoint = startPoint,
                StartDirection = startDirection,
                EndPoint = endPoint,
                EndDirection = endDirection
            };

            var startObstacle = DrawingBoundsFromSnapPoint(startPoint);
            if (!startObstacle.IsEmpty)
            {
                Map.Obstacles.Add(startObstacle);
            }

            var endObstacle = DrawingBoundsFromSnapPoint(endPoint);
            if (!endObstacle.IsEmpty)
            {
                Map.Obstacles.Add(endObstacle);
            }

            Point[] path = null;

            if (Map.Obstacles.Count > 0)
            {
                Map.Compile();

                path = PathFinder.FindPath(startNode, endNode, Map);
            }

            if (path == null)
            {
                path = DefaultRoute(startPoint, endPoint);
            }

            path = NormalizeRoute(path);

            // Pixel-align all points
            path = path.Select(point => new Point(Math.Round(point.X), Math.Round(point.Y))).ToArray();

            Points = new PointCollection(path);
        }

        static Point[] DefaultRoute(Point startPoint, Point endPoint)
        {
            var path = new Point[4];

            path[0] = startPoint;

            if (Math.Abs(endPoint.X - startPoint.X) > Math.Abs(endPoint.Y - startPoint.Y))
            {
                double centerX = (startPoint.X + endPoint.X) / 2;

                path[1] = new Point(centerX, startPoint.Y);
                path[2] = new Point(centerX, endPoint.Y);
            }
            else
            {
                double centerY = (startPoint.Y + endPoint.Y) / 2;

                path[1] = new Point(startPoint.X, centerY);
                path[2] = new Point(endPoint.X, centerY);
            }

            path[3] = endPoint;

            return path;
        }

        /// <summary>
        /// Remove colinear points from the given route
        /// </summary>
        Point[] NormalizeRoute(Point[] points)
        {
            if (points.Length < 3)
            {
                return points;
            }

            var results = new List<Point>();
            results.Add(points.First());

            for (int i = 1; i < points.Length - 1; i++)
            {
                if (!IsColinear(points[i - 1], points[i], points[i + 1]))
                {
                    results.Add(points[i]);
                }
            }

            results.Add(points.Last());

            return results.ToArray();
        }

        static bool IsColinear(Point point1, Point point2, Point point3)
        {
            var direction1 = Directions.FromVector(point2 - point1);
            var direction2 = Directions.FromVector(point3 - point2);

            return direction1 == direction2;
        }

        Direction GetStartDirection(Point startPoint)
        {
            Rect startRect = DrawingBoundsFromSnapPoint(startPoint);
            if (startRect.IsEmpty)
            {
                return Direction.None;
            }

            double centerX = (startRect.Left + startRect.Right) / 2;
            double centerY = (startRect.Top + startRect.Bottom) / 2;
            Point centerPoint = new Point(centerX, centerY);
            Vector startVector = startPoint - centerPoint;
            return Directions.CardinalFromVector(startVector);
        }

        Direction GetEndDirection(Point endPoint)
        {
            Rect endRect = DrawingBoundsFromSnapPoint(endPoint);
            if (endRect.IsEmpty)
            {
                return Direction.None;
            }

            double centerX = (endRect.Left + endRect.Right) / 2;
            double centerY = (endRect.Top + endRect.Bottom) / 2;
            Point centerPoint = new Point(centerX, centerY);
            Vector startVector = centerPoint - endPoint;
            return Directions.CardinalFromVector(startVector);
        }

        Rect DrawingBoundsFromSnapPoint(Point point)
        {
            Visual reference = (Visual)Canvas.Parent;
            point = GeometryTransform.Transform(point);
            point = TransformToAncestor(reference).Transform(point);

            Rect result = DrawingBoundsFromSnapPoint(reference, point);
            result = reference.TransformToDescendant(this).TransformBounds(result);
            result = GeometryTransform.Inverse.TransformBounds(result);

            return result;
        }

        static Rect DrawingBoundsFromSnapPoint(Visual reference, Point point)
        {
            var flags = ShapeHitTestFlags.HitSnaps | ShapeHitTestFlags.ExcludeLineTypes;
            var drawings = ShapeHitTest.HitAll(reference, point, flags);
            return Shape.GetBounds(reference, drawings);
        }


        #endregion

        #region INodeSource

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    return DOMHelper.PrependClass(classNames, "routedLine");
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        #endregion

        #region Object

        public override object Clone()
        {
            return new RoutedLine(this);
        }

        #endregion
    }
}
