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
using System.Windows.Threading;

namespace SilverNote.Editor
{
    public class Caret : DrawingVisual
    {
        #region Fields

        Brush _Brush;
        double _Width;
        double _Height;
        bool _IsEnabled;
        DispatcherTimer _BlinkTimer;
        bool _IsDrawn;

        #endregion

        public Caret()
        {
            _Brush = Brushes.Black;
            _Width = SystemParameters.CaretWidth;
            _Height = 16.0;
            _IsEnabled = false;
            _IsDrawn = false;
        }

        public Brush Brush
        {
            get
            {
                return _Brush;
            }
            set
            {
                if (value != _Brush)
                {
                    _Brush = value;
                    Draw();
                }
            }
        }

        public double Width 
        {
            get
            {
                return _Width;
            }
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    Draw();
                }
            }
        }

        public double Height 
        {
            get
            {
                return _Height;
            }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    Draw();
                }
            }
        }

        public bool IsEnabled
        {
            get 
            { 
                return _IsEnabled; 
            }
            set
            {
                _IsEnabled = value;

                if (value)
                {
                    if (!_IsDrawn) Draw();
                    Opacity = 1;
                    BlinkTimer.Start();
                }
                else
                {
                    Opacity = 0;
                    BlinkTimer.Stop();
                }
            }
        }

        public int BlinkInterval
        {
            get { return (int)BlinkTimer.Interval.TotalMilliseconds; }
            set { BlinkTimer.Interval = new TimeSpan(0, 0, 0, 0, milliseconds: value); }
        }

        private DispatcherTimer BlinkTimer
        {
            get
            {
                if (_BlinkTimer == null)
                {
                    _BlinkTimer = new DispatcherTimer(
                        new TimeSpan(500 * 10000),
                        DispatcherPriority.Background,
                        new EventHandler(BlinkTimer_Fired),
                        Dispatcher
                    );
                }

                return _BlinkTimer;
            }
        }

        protected void BlinkTimer_Fired(object source, EventArgs args)
        {
            if (Opacity == 0)
            {
                Opacity = 1;
            }
            else
            {
                Opacity = 0;
            }
        }
        
        protected void Draw()
        {
            DrawingContext dc = RenderOpen();
            Rect rect = new Rect(0, 0, Width, Height);
            dc.DrawRectangle(_Brush, null, rect);
            dc.Close();
            Opacity = 1;
            _IsDrawn = true;
        }
    }
}
