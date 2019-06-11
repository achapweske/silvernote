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

namespace SilverNote.Editor
{
    public class SmartPencil
    {
        #region Fields

        bool _IsReset;
        NPath _Path;
        QuadraticBezierSegment _CurrentSegment;
        List<Point> _Points = new List<Point>();
        Vector _Tangent;
        bool _NeedTangent;
        Stopwatch _DrawingTimer = new Stopwatch();

        #endregion

        #region Constructors

        public SmartPencil(NPath path)
        {
            _Path = path ?? new NPath();
            _IsReset = true;
            Smoothness = 256;
        }

        #endregion

        #region Properties

        public double Smoothness { get; set; }

        public NPath Path
        {
            get { return _Path; }
        }

        #endregion

        #region Methods

        public void Reset()
        {
            _IsReset = true;
        }

        public void AddPoint(Point point)
        {
            if (_IsReset)
            {
                Path.StartPoint = point;
                _Points.Add(point);
                _DrawingTimer.Restart();
                _NeedTangent = true;
                _IsReset = false;
                return;
            }

            // Compute drawing interval and velocity

            var delta = point - _Points.Last();
            var interval = _DrawingTimer.ElapsedMilliseconds;
            var speed = delta.Length / interval;
            _DrawingTimer.Restart();

            // If long delay between motions, start a new segment

            if (interval > 100)
            {
                ResetSegment();
            }

            // Compute the start tangent for the current segment

            if (_NeedTangent && _Points.Count <= 8)
            {
                _Tangent = point - _Points[0];
                _Tangent.Normalize();
            }

            _Points.Add(point);

            // Fit points to a quadratic bezier
            var bezier = QuadraticBezier.FitCurve(_Tangent, _Points);

            // Start a new segment when error of current fit is high

            var error = QuadraticBezier.MaxError(bezier[0], bezier[1], bezier[2], _Points);
            var errorThreshold = Smoothness * speed;    // (FYI: 64 is a good baseline threshold)
            if (error < errorThreshold)
            {
                // Error is below threshold - update _CurrentSegment
                UpdateSegment(bezier);
            }
            else
            {
                // Error is above threshold - start a new segment
                EmitSegment();
            }
        }

        #endregion

        #region Implementation

        private void ResetSegment()
        {
            _Points.RemoveRange(0, _Points.Count - 1);
            _CurrentSegment = null;
            _NeedTangent = true;
        }

        private void UpdateSegment(IList<Point> bezier)
        {
            if (Path.Figure.Segments.Count > 0 && Path.Figure.Segments.Last() == _CurrentSegment)
            {
                Path.Figure.Segments.RemoveAt(Path.Figure.Segments.Count - 1);
            }

            _CurrentSegment = new QuadraticBezierSegment(bezier[1], bezier[2], true);

            Path.Figure.Segments.Add(_CurrentSegment);
        }

        private void EmitSegment()
        {
            _Points.RemoveRange(0, _Points.Count - 1);

            if (_CurrentSegment != null)
            {
                _Tangent = _CurrentSegment.Point2 - _CurrentSegment.Point1;
                _Tangent.Normalize();
                _NeedTangent = false;
            }

            _CurrentSegment = null;
        }

        #endregion
    }
}
