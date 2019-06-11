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

namespace SilverNote.Common
{
    public enum Direction
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        NorthEast = North | East,
        SouthEast = South | East,
        SouthWest = South | West,
        NorthWest = North | West
    };

    public static class Directions
    {
        private static Direction[] _CardinalDirections;

        public static IEnumerable<Direction> CardinalDirections
        {
            get
            {
                if (_CardinalDirections == null)
                {
                    _CardinalDirections = new[] { Direction.North, Direction.East, Direction.South, Direction.West };
                }
                return _CardinalDirections;
            }
        }

        public static Direction Left(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.West;
                case Direction.East:
                    return Direction.North;
                case Direction.South:
                    return Direction.East;
                case Direction.West:
                    return Direction.South;
                default:
                    return Direction.None;
            }
        }

        public static Direction Right(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.East;
                case Direction.East:
                    return Direction.South;
                case Direction.South:
                    return Direction.West;
                case Direction.West:
                    return Direction.North;
                default:
                    return Direction.None;
            }
        }

        public static Direction Reverse(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Direction.South;
                case Direction.East:
                    return Direction.West;
                case Direction.South:
                    return Direction.North;
                case Direction.West:
                    return Direction.East;
                default:
                    return Direction.None;
            }
        }

        public static Vector ToVector(Direction direction)
        {
            double x = 0;
            double y = 0;

            if (direction.HasFlag(Direction.North))
            {
                y = -1;
            }

            if (direction.HasFlag(Direction.East))
            {
                x = 1;
            }

            if (direction.HasFlag(Direction.South))
            {
                y = 1;
            }

            if (direction.HasFlag(Direction.West))
            {
                x = -1;
            }

            var result = new Vector(x, y);
            result.Normalize();
            return result;
        }

        public static Direction FromVector(Vector vector)
        {
            Direction result = 0;

            if (vector.X > 0)
            {
                result |= Direction.East;
            }
            else if (vector.X < 0)
            {
                result |= Direction.West;
            }

            if (vector.Y > 0)
            {
                result |= Direction.South;
            }
            else if (vector.Y < 0)
            {
                result |= Direction.North;
            }

            return result;
        }

        public static Direction CardinalFromVector(Vector vector)
        {
            var result = FromVector(vector);

            if (Math.Abs(vector.X) > Math.Abs(vector.Y))
            {
                result &= (Direction.East | Direction.West);
            }
            else
            {
                result &= (Direction.North | Direction.South);
            }

            return result;
        }

    }
}
