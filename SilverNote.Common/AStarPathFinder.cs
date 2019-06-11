/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SilverNote.Common
{
    public class AStarPathFinder
    {
        public Point[] FindPath(Point startPoint, Point endPoint, IAStarMap map)
        {
            var startNode = new AStarNode(startPoint);
            var endNode = new AStarNode(endPoint);

            return FindPath(startNode, endNode, map);
        }

        public Point[] FindPath<T>(T startNode, T endNode, IAStarMap map)
            where T : AStarNode, new()
        {
            var nodes = new Dictionary<Point, AStarNode>();
            nodes[startNode.Point] = startNode;
            nodes[endNode.Point] = endNode;

            var openList = new PriorityQueue<double, AStarNode>();
            var closedList = new HashSet<AStarNode>();

            AStarNode searchNode = startNode;

            while (searchNode != endNode)
            {
                closedList.Add(searchNode);

                // Add each neighbor to open list if not on closed list,
                // and it is not the node we're coming from
                
                Point[] neighbors = map.GetNeighbors(searchNode.Point);
                foreach (Point neighbor in neighbors)
                {
                    AStarNode neighborNode;
                    if (!nodes.TryGetValue(neighbor, out neighborNode))
                    {
                        neighborNode = new T { Point = neighbor };
                        nodes[neighbor] = neighborNode;
                    }

                    if (!closedList.Contains(neighborNode))
                    {
                        double cost = neighborNode.CostFrom(searchNode);
                        if (cost < neighborNode.Cost)
                        {
                            neighborNode.Cost = cost;
                            neighborNode.Parent = searchNode;
                            double costToEnd = neighborNode.HeuristicCostTo(endNode);
                            openList.Enqueue(neighborNode, neighborNode.Cost + costToEnd);
                        }
                    }
                }

                if (openList.Count > 0)
                {
                    searchNode = openList.Dequeue();
                }
                else
                {
                    return null;
                }
            }

            // Construct the path

            var path = new LinkedList<Point>();

            while (searchNode != null)
            {
                path.AddFirst(searchNode.Point);

                searchNode = searchNode.Parent;
            }

            return path.ToArray();
        }
    }

    public class AStarNode
    {
        public AStarNode()
        {
            Parent = null;
            Cost = Double.PositiveInfinity;
        }

        public AStarNode(Point point)
            : this()
        {
            Point = point;
        }

        public Point Point { get; set; }

        public AStarNode Parent { get; set; }

        public double Cost { get; set; }

        public virtual double CostFrom(AStarNode fromNode)
        {
            return (this.Point - fromNode.Point).Length;
        }

        public virtual double HeuristicCostTo(AStarNode node)
        {
            // Manhattan distance
            return Math.Abs(node.Point.X - this.Point.X) + Math.Abs(node.Point.Y - this.Point.Y);
        }
    }

    public interface IAStarMap
    {
        Point[] GetNeighbors(Point point);
    }

    public class AStarGraph : IAStarMap
    {
        Dictionary<Point, HashSet<Point>> Neighbors = new Dictionary<Point, HashSet<Point>>();

        public void Clear()
        {
            Neighbors.Clear();
        }

        public void AddEdge(Point point1, Point point2)
        {
            InternalGetNeighbors(point1).Add(point2);
            InternalGetNeighbors(point2).Add(point1);
        }

        public Point[] GetNeighbors(Point point)
        {
            return InternalGetNeighbors(point).ToArray();
        }

        HashSet<Point> InternalGetNeighbors(Point point)
        {
            HashSet<Point> result;
            if (!Neighbors.TryGetValue(point, out result))
            {
                result = new HashSet<Point>();
                Neighbors[point] = result;
            }

            return result;
        }

        public void Draw(DrawingContext dc, Pen pen)
        {
            foreach (var item in Neighbors)
            {
                var point = item.Key;

                foreach (var neighbor in item.Value)
                {
                    dc.DrawLine(pen, point, neighbor);
                }
            }
        }
    }

}
