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
    public interface IMovable
    {
        void MoveStarted();

        void MoveDelta(Vector delta);

        void MoveCompleted();
    }

    public static class Movable
    {
        #region RequestingBeginMove

        public static readonly RoutedEvent RequestingBeginMove = EventManager.RegisterRoutedEvent (
            "RequestingBeginMove", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler),
            typeof(Movable)
        );

        public static void AddRequestingBeginMoveHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(RequestingBeginMove, handler);
            }
        }

        public static void RemoveRequestingBeginMoveHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(RequestingBeginMove, handler);
            }
        }

        #endregion

        #region RequestingMoveDelta

        public static readonly RoutedEvent RequestingMoveDelta = EventManager.RegisterRoutedEvent(
            "RequestingMoveDelta",
            RoutingStrategy.Bubble,
            typeof(MoveDeltaEventHandler),
            typeof(Movable)
        );

        public static void AddRequestingMoveDeltaHandler(DependencyObject dep, MoveDeltaEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(RequestingMoveDelta, handler);
            }
        }

        public static void RemoveRequestingMoveDeltaHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(RequestingMoveDelta, handler);
            }
        }

        #endregion

        #region RequestingEndMove

        public static readonly RoutedEvent RequestingEndMove = EventManager.RegisterRoutedEvent(
            "RequestingEndMove",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Movable)
        );

        public static void AddRequestingEndMoveHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.AddHandler(RequestingEndMove, handler);
            }
        }

        public static void RemoveRequestingEndMoveHandler(DependencyObject dep, RoutedEventHandler handler)
        {
            var element = dep as UIElement;
            if (element != null)
            {
                element.RemoveHandler(RequestingEndMove, handler);
            }
        }

        #endregion
    }

    public delegate void MoveDeltaEventHandler(object sender, MoveDeltaEventArgs e);

    public class MoveDeltaEventArgs : RoutedEventArgs
    {
        public MoveDeltaEventArgs(Vector delta)
        {
            Delta = delta;
        }

        public MoveDeltaEventArgs(RoutedEvent e, Vector delta)
            : base(e)
        {
            Delta = delta;
        }

        public MoveDeltaEventArgs(RoutedEvent e, object source, Vector delta)
            : base(e, source)
        {
            Delta = delta;
        }

        public Vector Delta { get; set; }
    }
}
