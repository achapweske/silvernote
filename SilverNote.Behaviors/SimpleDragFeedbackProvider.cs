/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SilverNote.Behaviors
{
    public class SimpleDragFeedbackProvider : IDragFeedbackProvider
    {
        #region Static Properties

        private static SimpleDragFeedbackProvider _Instance;

        public static SimpleDragFeedbackProvider Instance
        {
            get { return _Instance ?? (_Instance = new SimpleDragFeedbackProvider()); }
        }

        #endregion

        #region Constructors

        public SimpleDragFeedbackProvider()
        {

        }

        #endregion

        #region Attached Properties

        public static readonly DependencyProperty DragFeedbackAdornerProperty = DependencyProperty.RegisterAttached(
            "DragFeedbackAdorner",
            typeof(SimpleDragFeedbackAdorner),
            typeof(SimpleDragFeedbackProvider),
            new PropertyMetadata(null));

        public static SimpleDragFeedbackAdorner GetDragFeedbackAdorner(DependencyObject element)
        {
            return (SimpleDragFeedbackAdorner)element.GetValue(DragFeedbackAdornerProperty);
        }

        public static void SetDragFeedbackAdorner(DependencyObject element, SimpleDragFeedbackAdorner value)
        {
            element.SetValue(DragFeedbackAdornerProperty, value);
        }

        #endregion

        #region Methods

        public void DragStarted(object sender, MouseEventArgs e)
        {
            var element = sender as UIElement;
            if (element != null)
            {
                var window = LayoutHelper.GetAncestor<Window>(element);
                if (window != null)
                {
                    var content = window.Content as UIElement;
                    if (content != null)
                    {
                        var layer = AdornerLayer.GetAdornerLayer(content);
                        if (layer != null)
                        {
                            var adorner = new SimpleDragFeedbackAdorner(content);
                            adorner.Content = CreateAdornerVisual(element, e.GetPosition(element));
                            layer.Add(adorner);
                            SetDragFeedbackAdorner(element, adorner);
                        }
                    }
                }
            }
        }

        public void GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            var element = sender as UIElement;
            if (element != null)
            {
                var window = LayoutHelper.GetAncestor<Window>(element);
                if (window != null)
                {
                    var adorner = GetDragFeedbackAdorner(element);
                    if (adorner != null)
                    {
                        var layer = adorner.Parent as AdornerLayer;
                        if (layer != null)
                        {
                            adorner.Position = GetMousePosition(layer);

                            if (e.Effects == DragDropEffects.None)
                            {
                                Mouse.SetCursor(Cursors.Arrow);
                                e.UseDefaultCursors = false;
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }

        public void DragEnded(object sender)
        {
            var element = sender as UIElement;
            if (element != null)
            {
                var window = LayoutHelper.GetAncestor<Window>(element);
                if (window != null)
                {
                    var adorner = GetDragFeedbackAdorner(element);
                    if (adorner != null)
                    {
                        var layer = adorner.Parent as AdornerLayer;
                        if (layer != null)
                        {
                            layer.Remove(adorner);
                            SetDragFeedbackAdorner(element, null);
                        }
                    }
                }
            }
        }

        #endregion

        #region Implementation

        private static Visual CreateAdornerVisual(Visual target, Point offset = default(Point))
        {
            var drawing = new DrawingVisual();

            using (var dc = drawing.RenderOpen())
            {
                var brush = new VisualBrush(target);
                var bounds = VisualTreeHelper.GetDescendantBounds(target);
                bounds.X = -offset.X;
                bounds.Y = -offset.Y;

                dc.PushOpacity(0.5);
                dc.DrawRectangle(brush, null, bounds);
            }

            return drawing;
        }

        #region GetMousePosition

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hwnd, ref Win32Point pt);

        public static Point GetMousePosition(Visual relativeTo)
        {
            Win32Point mouse = new Win32Point();

            GetCursorPos(ref mouse);

            var presentationSource = (HwndSource)PresentationSource.FromVisual(relativeTo);

            ScreenToClient(presentationSource.Handle, ref mouse);

            var transform = relativeTo.TransformToAncestor(presentationSource.RootVisual);

            var offset = transform.Transform(new Point(0, 0));

            return new Point(mouse.X - offset.X, mouse.Y - offset.Y);
        }

        #endregion

        #endregion
    }

    
}
