/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SilverNote.Views
{
    /// <summary>
    /// Interaction logic for ScreenCaptureOverlay.xaml
    /// </summary>
    public partial class ScreenCaptureOverlay : Window
    {
        #region Fields

        Point _StartPoint;
        Brush _OverlayBrush;
        Rect _CaptureRect;

        #endregion

        #region Constructors

        public ScreenCaptureOverlay()
        {
            InitializeComponent();

            _OverlayBrush = new SolidColorBrush(Color.FromArgb(25, 0, 0, 255));
            _OverlayBrush.Freeze();
        }

        #endregion

        #region Properties

        // The selected rectangle, in screen coordinates
        public Rect CaptureRect { get; set; }

        #endregion

        #region Implementation

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Activate();
            CaptureMouse();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ReleaseMouseCapture();

            // Convert capture region to screen coordinates

            if (_CaptureRect != null)
            {
                Point screenPosition = PointToScreen(_CaptureRect.TopLeft);
                CaptureRect = new Rect(screenPosition, _CaptureRect.Size);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _StartPoint = e.GetPosition(this);

            e.Handled = true;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {         
            DialogResult = true;

            e.Handled = true;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var endPoint = e.GetPosition(this);

            MessageTransform.X = endPoint.X;
            MessageTransform.Y = endPoint.Y;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            double x = Math.Min(_StartPoint.X, endPoint.X);
            double y = Math.Min(_StartPoint.Y, endPoint.Y);
            double width = Math.Abs(endPoint.X - _StartPoint.X);
            double height = Math.Abs(endPoint.Y - _StartPoint.Y);

            _CaptureRect = new Rect(x, y, width, height);

            InvalidateVisual();

            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            DialogResult = false;

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var renderRect = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
            var renderGeometry = new RectangleGeometry(renderRect);
            var captureGeometry = new RectangleGeometry(_CaptureRect);
            var geometry = Geometry.Combine(renderGeometry, captureGeometry, GeometryCombineMode.Exclude, null);
            geometry.Freeze();

            dc.DrawRectangle(Brushes.Transparent, null, renderRect);
            dc.DrawGeometry(_OverlayBrush, null, geometry);

            base.OnRender(dc);
        }

        #endregion
    }
}
