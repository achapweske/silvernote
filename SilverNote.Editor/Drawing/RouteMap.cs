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
using SilverNote.Common;

namespace SilverNote.Editor
{
    public class RouteMap : AStarGraph
    {
        public RouteMap()
        {
            Obstacles = new List<Rect>();
        }
        
        public Point StartPoint { get; set; }

        public Direction StartDirection { get; set; }

        public Point EndPoint { get; set; }

        public Direction EndDirection { get; set; }

        public List<Rect> Obstacles { get; set; }

        public void Compile()
        {
            Clear();

            var xObstacles = Obstacles.OrderBy((b) => b.X).ToArray();
            var yObstacles = Obstacles.OrderBy((b) => b.Y).ToArray();

            var xCoords = new List<double>();
            var yCoords = new List<double>();

            for (int i = 1; i < xObstacles.Length; i++)
            {
                double right = xObstacles[i - 1].Right;
                double left = xObstacles[i].Left;

                if (right < left)
                {
                    xCoords.Add((right + left) / 2);
                }
                else
                {
                    xCoords.Add(right);
                    xCoords.Add(left);
                }
            }

            for (int i = 1; i < yObstacles.Length; i++)
            {
                double bottom = yObstacles[i - 1].Bottom;
                double top = yObstacles[i].Top;

                if (bottom < top)
                {
                    yCoords.Add((bottom + top) / 2);
                }
                else
                {
                    yCoords.Add(bottom);
                    yCoords.Add(top);
                }
            }

            xCoords.Add(StartPoint.X);
            yCoords.Add(StartPoint.Y);
            xCoords.Add(EndPoint.X);
            yCoords.Add(EndPoint.Y);

            if (StartDirection != Direction.None)
            {
                Point startPoint2 = StartPoint;

                var obstacle = Obstacles.FirstOrDefault((rect) => rect.Contains(StartPoint));
                if (!obstacle.IsEmpty)
                {
                    startPoint2 = Intersect(StartPoint, StartDirection, obstacle);
                }

                if (StartDirection == Direction.North && startPoint2.Y == yCoords.Min() ||
                    StartDirection == Direction.East && startPoint2.X == xCoords.Max() ||
                    StartDirection == Direction.South && startPoint2.Y == yCoords.Max() ||
                    StartDirection == Direction.West && startPoint2.X == xCoords.Min())
                {
                    double length = (startPoint2 - StartPoint).Length + 20;
                    startPoint2 = StartPoint + Directions.ToVector(StartDirection) * length; 
                }

                if (startPoint2 != StartPoint)
                {
                    xCoords.Add(startPoint2.X);
                    yCoords.Add(startPoint2.Y);
                    AddEdge(StartPoint, startPoint2);
                }
            }

            if (EndDirection != Direction.None)
            {
                Point endPoint2 = EndPoint;

                var obstacle = Obstacles.FirstOrDefault((rect) => rect.Contains(EndPoint));
                if (!obstacle.IsEmpty)
                {
                    endPoint2 = Intersect(EndPoint, Directions.Reverse(EndDirection), obstacle);
                }

                if (EndDirection == Direction.North && endPoint2.Y == yCoords.Max() ||
                    EndDirection == Direction.East && endPoint2.X == xCoords.Min() ||
                    EndDirection == Direction.South && endPoint2.Y == yCoords.Min() ||
                    EndDirection == Direction.West && endPoint2.X == xCoords.Max())
                {
                    double length = (endPoint2 - EndPoint).Length + 20;
                    endPoint2 = EndPoint - Directions.ToVector(EndDirection) * length;
                }

                if (endPoint2 != EndPoint)
                {
                    xCoords.Add(endPoint2.X);
                    yCoords.Add(endPoint2.Y);
                    AddEdge(endPoint2, EndPoint);
                }
            }

            xCoords.Sort();
            xCoords = xCoords.Distinct().ToList();
            yCoords.Sort();
            yCoords = yCoords.Distinct().ToList();

            for (int i = 0; i < xCoords.Count; i++)
            {
                for (int j = 0; j < yCoords.Count - 1; j++)
                {
                    var point1 = new Point(xCoords[i], yCoords[j]);
                    var point2 = new Point(xCoords[i], yCoords[j + 1]);

                    if (!IsLineInRectangle(point1, point2, Obstacles))
                    {
                        AddEdge(point1, point2);
                    }
                }
            }

            for (int i = 0; i < yCoords.Count; i++)
            {
                for (int j = 0; j < xCoords.Count - 1; j++)
                {
                    var point1 = new Point(xCoords[j], yCoords[i]);
                    var point2 = new Point(xCoords[j + 1], yCoords[i]);

                    if (!IsLineInRectangle(point1, point2, Obstacles))
                    {
                        AddEdge(point1, point2);
                    }
                }
            }
        }

        static bool IsLineInRectangle(Point point1, Point point2, IEnumerable<Rect> rects)
        {
            if (point1.X == point2.X)
            {
                // vertical line segment

                double x = point1.X;
                double y1 = Math.Min(point1.Y, point2.Y);
                double y2 = Math.Max(point1.Y, point2.Y);

                foreach (var rect in rects)
                {
                    if (rect.Left < x && rect.Right > x &&
                        ((rect.Top <= y1 && rect.Bottom > y1) || (rect.Top < y2 && rect.Bottom >= y2)))
                    {
                        return true;
                    }
                }
            }

            if (point1.Y == point2.Y)
            {
                // horizontal line segment

                foreach (var rect in rects)
                {
                    double y = point1.Y;
                    double x1 = Math.Min(point1.X, point2.X);
                    double x2 = Math.Max(point1.X, point2.X);

                    if (rect.Top < y && rect.Bottom > y &&
                        ((rect.Left <= x1 && rect.Right > x1) || (rect.Left < x2 && rect.Right >= x2)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the next obstacle from the specified point in the given direction
        /// </summary>
        private Rect FindObstacle(Point point, Direction direction)
        {
            IEnumerable<Rect> result;

            switch (direction)
            {
                case Direction.North:

                    result = Obstacles
                        .OrderBy((r) => r.Bottom)
                        .Where((r) => r.Bottom <= point.Y && r.Left < point.X && r.Right > point.X);
                    break;

                case Direction.East:

                    result = Obstacles
                        .OrderBy((r) => r.Left)
                        .Where((r) => r.Left <= point.X && r.Top < point.Y && r.Bottom > point.Y);
                    break;

                case Direction.South:

                    result = Obstacles
                        .OrderBy((r) => r.Top)
                        .Where((r) => r.Top >= point.Y && r.Left < point.X && r.Right > point.X);
                    break;

                case Direction.West:

                    result = Obstacles
                        .OrderBy((r) => r.Right)
                        .Where((r) => r.Right >= point.X && r.Top < point.Y && r.Bottom > point.Y);
                    break;

                default:

                    throw new ArgumentException("direction must be one of: North, East, South, or West");
            }

            return result.Any() ? result.First() : Rect.Empty;
        }

        static Point Intersect(Point point, Direction direction, Rect rect)
        {
            switch (direction)
            {
                case Direction.North:
                    if (point.Y >= rect.Bottom)
                        return new Point(point.X, rect.Bottom);
                    else if (point.Y >= rect.Top)
                        return new Point(point.X, rect.Top);
                    else
                        return new Point(point.X, Double.NegativeInfinity);
                case Direction.East:
                    if (point.X <= rect.Left)
                        return new Point(rect.Left, point.Y);
                    else if (point.X <= rect.Right)
                        return new Point(rect.Right, point.Y);
                    else
                        return new Point(Double.PositiveInfinity, point.Y);
                case Direction.South:
                    if (point.Y <= rect.Top)
                        return new Point(point.X, rect.Top);
                    else if (point.Y <= rect.Bottom)
                        return new Point(point.X, rect.Bottom);
                    else
                        return new Point(point.X, Double.PositiveInfinity);
                case Direction.West:
                    if (point.X >= rect.Right)
                        return new Point(rect.Right, point.Y);
                    else if (point.X >= rect.Left)
                        return new Point(rect.Left, point.Y);
                    else
                        return new Point(Double.NegativeInfinity, point.Y);
                default:
                    throw new ArgumentException("Direction must be one of North, East, South, or West.");
            }
        }

    }
}
