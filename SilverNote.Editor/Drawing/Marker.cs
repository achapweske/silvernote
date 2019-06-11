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
using System.Xml;
using DOM;
using DOM.SVG;

namespace SilverNote.Editor
{
    public class Marker : ShapeGroup
    {
        #region Fields

        double _MarkerWidth;
        double _MarkerHeight;
        TranslateTransform _Translate = new TranslateTransform();
        ScaleTransform _Scale = new ScaleTransform();
        RotateTransform _Rotate = new RotateTransform();

        #endregion

        #region Constructors

        public Marker()
        {
            Initialize();
        }

        public Marker(Shape drawing)
        {
            Initialize();

            Rect bounds = drawing.RenderedBounds;
            drawing.RenderedBounds = new Rect(0, 0, bounds.Width, bounds.Height);
            Drawings.Add(drawing);
            MarkerWidth = bounds.Width;
            MarkerHeight = bounds.Height;
            RefX = MarkerWidth + drawing.StrokeWidth / 2;
            RefY = MarkerHeight / 2;
        }

        public Marker(ShapeGroup group)
            : base(group)
        {
            Initialize();

            Rect bounds = this.RenderedBounds;
            RenderedBounds = new Rect(0, 0, bounds.Width, bounds.Height);
            MarkerWidth = bounds.Width;
            MarkerHeight = bounds.Height;
            RefX = MarkerWidth;
            RefY = MarkerHeight / 2;
        }

        public Marker(Marker copy)
            : base(copy)
        {
            _Translate = copy._Translate.Clone();
            _Scale = copy._Scale.Clone();
            _Rotate = copy._Rotate.Clone();

            var transform = new TransformGroup();
            transform.Children.Add(_Translate);
            transform.Children.Add(_Scale);
            transform.Children.Add(_Rotate);
            Transform = transform;

            MarkerWidth = copy.MarkerWidth;
            MarkerHeight = copy.MarkerHeight;
            RefX = copy.RefX;
            RefY = copy.RefY;
            Orient = copy.Orient;
        }

        void Initialize()
        {
            var transform = new TransformGroup();
            transform.Children.Add(_Translate);
            transform.Children.Add(_Scale);
            transform.Children.Add(_Rotate);
            Transform = transform;

            MarkerWidth = 0;
            MarkerHeight = 0;
            RefX = 0;
            RefY = 0;
            Orient = 0;
        }

        #endregion

        #region Properties

        public double RefX 
        {
            get { return -_Translate.X; }
            set { _Translate.X = -value; } 
        }

        public double RefY 
        {
            get { return -_Translate.Y; }
            set { _Translate.Y = -value; } 
        }

        public double MarkerWidth
        {
            get
            {
                return _MarkerWidth;
            }
            set
            {
                double renderedMarkerWidth = RenderedMarkerWidth;
                if (renderedMarkerWidth != 0)
                {
                    _Scale.ScaleX = value / renderedMarkerWidth;
                }
                _MarkerWidth = value;
            }
        }

        public double MarkerHeight
        {
            get
            {
                return _MarkerHeight;
            }
            set
            {
                double renderedMarkerHeight = RenderedMarkerHeight;
                if (renderedMarkerHeight != 0)
                {
                    _Scale.ScaleY = value / renderedMarkerHeight;
                }
                _MarkerHeight = value;
            }
        }

        public double Orient 
        {
            get { return _Rotate.Angle; }
            set { _Rotate.Angle = value; }
        }

        public Brush Brush
        {
            set
            {

            }
        }

        public double RenderedRefX
        {
            get
            {
                Point refPoint = new Point(RefX, RefY);
                refPoint = GeometryTransform.Transform(refPoint);
                return refPoint.X;
            }
            set
            {
                Point refPoint = new Point(value, RenderedRefY);
                refPoint = GeometryTransform.Inverse.Transform(refPoint);
                RefX = refPoint.X;
            }
        }

        public double RenderedRefY
        {
            get
            {
                Point refPoint = new Point(RefX, RefY);
                refPoint = GeometryTransform.Transform(refPoint);
                return refPoint.Y;
            }
            set
            {
                Point refPoint = new Point(RenderedRefX, value);
                refPoint = GeometryTransform.Inverse.Transform(refPoint);
                RefY = refPoint.Y;
            }
        }

        public double RenderedMarkerWidth
        {
            get
            {
                Rect bounds = new Rect(0, 0, MarkerWidth, MarkerHeight);
                bounds = GeometryTransform.TransformBounds(bounds);
                return bounds.Width;
            }
            set
            {
                Rect bounds = new Rect(0, 0, MarkerWidth, MarkerHeight);
                bounds = GeometryTransform.TransformBounds(bounds);
                bounds.Width = value;
                bounds = GeometryTransform.Inverse.TransformBounds(bounds);
                MarkerWidth = bounds.Width;
            }
        }

        public double RenderedMarkerHeight
        {
            get
            {
                Rect bounds = new Rect(0, 0, MarkerWidth, MarkerHeight);
                bounds = GeometryTransform.TransformBounds(bounds);
                return bounds.Height;
            }
            set
            {
                Rect bounds = new Rect(0, 0, MarkerWidth, MarkerHeight);
                bounds = GeometryTransform.TransformBounds(bounds);
                bounds.Height = value;
                bounds = GeometryTransform.Inverse.TransformBounds(bounds);
                MarkerHeight = bounds.Height;
            }
        }

        #endregion

        #region NDrawing

        public override Shape ThumbSmall
        {
            get
            {
                var result = (Marker)base.ThumbSmall;
                result.MarkerWidth = result.RenderedBounds.Width;
                result.MarkerHeight = result.RenderedBounds.Height;
                result.RefX = 0;
                result.RefY = 0;
                return result;
            }
        }

        public Marker ThumbSmallMarker
        {
            get
            {
                var result = (Marker)Clone();
                result.StrokeWidth = 1;
                result.GeometryTransform = null;
                Rect bounds = result.RenderedBounds;

                Rect newBounds;
                if (bounds.Width >= bounds.Height)
                {
                    double aspectRatio = Math.Max(bounds.Height / bounds.Width, 0.5);
                    double width = 7;
                    double height = width * aspectRatio;
                    double offset = (7 - height) / 2;
                    newBounds = new Rect(0.5, offset, width, height);
                }
                else
                {
                    double aspectRatio = Math.Max(bounds.Width / bounds.Height, 0.5);
                    double height = 7;
                    double width = height * aspectRatio;
                    double offset = (7 - width) / 2;
                    newBounds = new Rect(offset, 0.5, width, height);
                }

                double refX = newBounds.X + (result.RefX - bounds.X) * newBounds.Width / bounds.Width;
                double refY = newBounds.Y + (result.RefY - bounds.Y) * newBounds.Height / bounds.Height;
                result.RenderedBounds = newBounds;
                result.MarkerWidth = newBounds.Width;
                result.MarkerHeight = newBounds.Height;
                result.RefX = refX;
                result.RefY = refY;

                return result;
            }
        }

        public override void Normalize()
        {
            MarkerWidth = RenderedMarkerWidth;
            MarkerHeight = RenderedMarkerHeight;
            RefX = RenderedRefX;
            RefY = RenderedRefY;

            base.Normalize();
        }

        #endregion

        #region INodeSource

        private readonly string _NodeName = SVGElements.NAMESPACE + ' ' + SVGElements.MARKER;

        public override string GetNodeName(NodeContext context)
        {
            return _NodeName;
        }

        private readonly string[] _NodeAttributes = new string[] { 
            SVGAttributes.REF_X,
            SVGAttributes.REF_Y,
            SVGAttributes.MARKER_WIDTH,
            SVGAttributes.MARKER_HEIGHT,
            SVGAttributes.MARKER_UNITS,
            SVGAttributes.ORIENT
        };

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Concat(_NodeAttributes).ToArray();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            Rect bounds = GeometryTransform.TransformBounds(Bounds);

            switch (name)
            {
                case SVGAttributes.REF_X:
                    return SVGFormatter.FormatLength(RenderedRefX);
                case SVGAttributes.REF_Y:
                    return SVGFormatter.FormatLength(RenderedRefY);
                case SVGAttributes.MARKER_WIDTH:
                    return SVGFormatter.FormatLength(RenderedMarkerWidth);
                case SVGAttributes.MARKER_HEIGHT:
                    return SVGFormatter.FormatLength(RenderedMarkerHeight);
                case SVGAttributes.MARKER_UNITS:
                    return "userSpaceOnUse";
                case SVGAttributes.ORIENT:
                    return "auto";
                default:
                    return base.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.REF_X:
                    SVGLength refX = SVGParser.ParseLength(value);
                    if (refX != null)
                        RenderedRefX = refX.Value;
                    break;
                case SVGAttributes.REF_Y:
                    SVGLength refY = SVGParser.ParseLength(value);
                    if (refY != null)
                        RenderedRefY = refY.Value;
                    break;
                case SVGAttributes.MARKER_WIDTH:
                    SVGLength markerWidth = SVGParser.ParseLength(value);
                    if (markerWidth != null)
                        RenderedMarkerWidth = markerWidth.Value;
                    break;
                case SVGAttributes.MARKER_HEIGHT:
                    SVGLength markerHeight = SVGParser.ParseLength(value);
                    if (markerHeight != null)
                        RenderedMarkerHeight = markerHeight.Value;
                    break;
                case SVGAttributes.MARKER_UNITS:
                    break;
                case SVGAttributes.ORIENT:
                    break;
                default:
                    base.SetNodeAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.REF_X:
                    RenderedRefX = 0;
                    break;
                case SVGAttributes.REF_Y:
                    RenderedRefY = 0;
                    break;
                case SVGAttributes.MARKER_WIDTH:
                    RenderedMarkerWidth = 0;
                    break;
                case SVGAttributes.MARKER_HEIGHT:
                    RenderedMarkerHeight = 0;
                    break;
                case SVGAttributes.MARKER_UNITS:
                    break;
                case SVGAttributes.ORIENT:
                    break;
                default:
                    base.ResetNodeAttribute(context, name);
                    break;
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new Marker(this);
        }

        #endregion

    }
}
