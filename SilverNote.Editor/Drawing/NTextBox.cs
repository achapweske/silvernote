/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Xml;
using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;
using DOM.Helpers;
using SilverNote.Commands;

namespace SilverNote.Editor
{
    public class NTextBox : Shape, IEditable, ISelectableText, ISearchable
    {
        #region Fields

        double _X;
        double _Y;
        double _Width;
        double _Height;
        TextAlignment _HorizontalAlignment;
        VerticalAlignment _VerticalAlignment;
        TextParagraph _Paragraph;
        string _Watermark;

        #endregion

        #region Constructors

        public NTextBox()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            HorizontalAlignment = TextAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
 
            Paragraph = new TextParagraph();
            Paragraph.Background = null;
            Paragraph.Width = 0;
            Paragraph.Padding = new Thickness(3);
            Paragraph.AcceptsReturn = true;
            Paragraph.CapturesMouse = true;

            LoadCommandBindings();
        }

        public NTextBox(NTextBox copy)
            : base(copy)
        {
            X = copy.X;
            Y = copy.Y;
            Width = copy.Width;
            Height = copy.Height;
            HorizontalAlignment = copy.HorizontalAlignment;
            VerticalAlignment = copy.VerticalAlignment;
            Watermark = copy.Watermark;
            HasInitialFocus = copy.HasInitialFocus;

            Paragraph = (TextParagraph)copy.Paragraph.Clone();

            LoadCommandBindings();
        }

        #endregion

        #region Properties

        public TextParagraph Paragraph 
        {
            get 
            { 
                return _Paragraph; 
            }
            set
            {
                if (value != _Paragraph)
                {
                    var oldValue = _Paragraph;
                    _Paragraph = value;
                    Paragraph_Changed(oldValue);
                }
            }
        }

        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                if (value != _X)
                {
                    _X = value;
                    InvalidateRender();
                }
            }
        }

        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                if (value != _Y)
                {
                    _Y = value;
                    InvalidateRender();
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
                    InvalidateRender();
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
                    InvalidateRender();
                }
            }
        }

        public TextAlignment HorizontalAlignment
        {
            get
            {
                return _HorizontalAlignment;
            }
            set
            {
                if (value != _HorizontalAlignment)
                {
                    _HorizontalAlignment = value;
                    InvalidateRender();
                }
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return _VerticalAlignment;
            }
            set
            {
                if (value != _VerticalAlignment)
                {
                    _VerticalAlignment = value;
                    InvalidateRender();
                }
            }
        }

        public double RenderedX
        {
            get
            {
                Point point = new Point(X, Y);
                point = GeometryTransform.Transform(point);
                return point.X;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point = new Point(value, RenderedY);
                    point = inverse.Transform(point);
                    X = point.X;
                }
            }
        }

        public double RenderedY
        {
            get
            {
                Point point = new Point(X, Y);
                point = GeometryTransform.Transform(point);
                return point.Y;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    Point point = new Point(RenderedX, value);
                    point = inverse.Transform(point);
                    Y = point.Y;
                }
            }
        }

        public double RenderedWidth
        {
            get
            {
                return RenderedBounds.Width;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    var rect = RenderedBounds;
                    rect.Width = value;
                    rect = inverse.TransformBounds(rect);
                    Width = rect.Width;
                }
            }
        }

        public double RenderedHeight
        {
            get
            {
                return RenderedBounds.Height;
            }
            set
            {
                var inverse = GeometryTransform.Inverse;
                if (inverse != null)
                {
                    var rect = RenderedBounds;
                    rect.Height = value;
                    rect = inverse.TransformBounds(rect);
                    Height = rect.Height;
                }
            }
        }

        public TextAlignment RenderedHorizontalAlignment
        {
            get
            {
                TextAlignment horizontal = HorizontalAlignment;
                VerticalAlignment vertical = VerticalAlignment;
                TransformAlignment(GeometryTransform, ref horizontal, ref vertical);
                return horizontal;
            }
            set
            {
                GeneralTransform transform = GeometryTransform.Inverse;
                if (transform == null)
                {
                    HorizontalAlignment = value;
                }
                else
                {
                    TextAlignment horizontal = value;
                    VerticalAlignment vertical = RenderedVerticalAlignment;
                    TransformAlignment(transform, ref horizontal, ref vertical);
                    HorizontalAlignment = horizontal;
                }
                Paragraph.TextAlignment = value;
            }
        }

        public VerticalAlignment RenderedVerticalAlignment
        {
            get
            {
                TextAlignment horizontal = HorizontalAlignment;
                VerticalAlignment vertical = VerticalAlignment;
                TransformAlignment(GeometryTransform, ref horizontal, ref vertical);
                return vertical;
            }
            set
            {
                GeneralTransform transform = GeometryTransform.Inverse;
                if (transform == null)
                {
                    VerticalAlignment = value;
                }
                else
                {
                    TextAlignment horizontal = RenderedHorizontalAlignment;
                    VerticalAlignment vertical = value;
                    TransformAlignment(transform, ref horizontal, ref vertical);
                    VerticalAlignment = vertical;
                }

                Paragraph.VerticalContentAlignment = value;
            }
        }

        void TransformAlignment(GeneralTransform transform, ref TextAlignment horizontal, ref VerticalAlignment vertical)
        {
            double x;
            switch (horizontal)
            {
                case TextAlignment.Left:
                    x = X;
                    break;
                case TextAlignment.Right:
                    x = X + Width;
                    break;
                default:
                    x = X + Width / 2;
                    break;
            }

            double y;
            switch (vertical)
            {
                case VerticalAlignment.Top:
                    y = Y;
                    break;
                case VerticalAlignment.Bottom:
                    y = Y + Height;
                    break;
                default:
                    y = Y + Height / 2;
                    break;
            }

            Point point = new Point(x, y);
            Point center = new Point(X + Width / 2, Y + Height / 2);

            point = transform.Transform(point);
            center = transform.Transform(center);

            double angle = Math.Atan2(-(point.Y - center.Y), point.X - center.X);

            Rect bounds = transform.TransformBounds(Bounds);
            double nw = Math.Atan2(-(bounds.Top - center.Y), bounds.Left - center.X);
            double n = Math.PI / 2;
            double ne = Math.Atan2(-(bounds.Top - center.Y), bounds.Right - center.X);
            double e = 0;
            double se = Math.Atan2(-(bounds.Bottom - center.Y), bounds.Right - center.X);
            double s = -Math.PI / 2;
            double sw = Math.Atan2(-(bounds.Bottom - center.Y), bounds.Left - center.X);


            if (Math.Abs(point.X - center.X) > 1)
            {
                if (angle < ((ne + n) / 2) && angle > ((se + s) / 2))
                    horizontal = TextAlignment.Right;
                else if (angle > ((nw + n) / 2) && angle <= Math.PI ||
                         angle < ((sw + s) / 2) && angle >= -Math.PI)
                    horizontal = TextAlignment.Left;
                else
                    horizontal = TextAlignment.Center;
            }

            if (Math.Abs(point.Y - center.Y) > 1)
            {
                if (angle > ((ne + e) / 2) && angle < ((nw + Math.PI) / 2))
                    vertical = VerticalAlignment.Top;
                else if (angle < ((se + e) / 2) && angle > ((sw - Math.PI) / 2))
                    vertical = VerticalAlignment.Bottom;
                else
                    vertical = VerticalAlignment.Center;
            }
        }

        public string Watermark
        {
            get
            {
                return _Watermark;
            }
            set
            {
                if (value != _Watermark)
                {
                    _Watermark = value;
                    UpdateWatermark();
                }
            }
        }

        public bool HasInitialFocus { get; set; }

        #endregion

        #region NDrawing

        public override string Text
        {
            get 
            {
                string result = Paragraph.Text;

                if (!String.IsNullOrEmpty(result))
                {
                    result += "\n";
                }

                return result;
            }
        }

        protected void UpdateWatermark()
        {
            if (Paragraph != null)
            {
                if (IsAdornerVisible)
                {
                    Paragraph.Watermark = Watermark;
                }
                else
                {
                    Paragraph.Watermark = String.Empty;
                }
            }
        }

        protected void UpdateOutlineDrawing()
        {
            if (IsAdornerVisible && (ParentDrawing == null))
            {
                if (!Children.Contains(OutlineDrawing))
                {
                    Children.Add(OutlineDrawing);
                }
            }
            else
            {
                Children.Remove(OutlineDrawing);
            }
        }

        private DrawingVisual _OutlineDrawing = null;

        protected DrawingVisual OutlineDrawing
        {
            get
            {
                if (_OutlineDrawing == null)
                {
                    _OutlineDrawing = new DrawingVisual();
                }
                return _OutlineDrawing;
            }
        }

        protected override Rect Bounds
        {
            get { return new Rect(X, Y, Width, Height); }
        }

        public override void Simplify(bool verySmall = false)
        {
            base.Simplify(verySmall);

            // Set font size to 6pt

            Paragraph.SelectAll();
            Paragraph.SetProperty(TextProperties.FontSizeProperty, 6.0 * 96.0 / 72.0);
        }

        public override void Normalize()
        {
            Rect bounds = GeometryTransform.TransformBounds(Bounds);

            X = bounds.X;
            Y = bounds.Y;
            Width = bounds.Width;
            Height = bounds.Height;
            HorizontalAlignment = RenderedHorizontalAlignment;
            VerticalAlignment = RenderedVerticalAlignment;

            GeometryTransform = null;
        }

        Point startPosition;

        public override void Place(Point position)
        {
            startPosition = position;

            X = position.X;
            Y = position.Y;
            Width = Height = 0;
        }

        public override void Draw(Point point)
        {
            Rect rect = new Rect(startPosition, point);

            X = rect.X;
            Y = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
        }

        public override bool CompleteDrawing()
        {
            Paragraph.Focus();
            return true;
        }

        public override int HandleCount
        {
            get { return 4; }
        }

        protected override Point GetHandleInternal(int index)
        {
            Rect bounds = Bounds;

            switch (index)
            {
                case 0:
                    return bounds.TopLeft;
                case 1:
                    return bounds.TopRight;
                case 2:
                    return bounds.BottomLeft;
                case 3:
                    return bounds.BottomRight;
                default:
                    return new Point();
            }
        }

        protected override void SetHandleInternal(int index, Point value)
        {
            double left = X;
            double top = Y;
            double right = X + Width;
            double bottom = Y + Height;

            switch (index)
            {
                case 0:
                    left = Math.Min(value.X, right);
                    top = Math.Min(value.Y, bottom);
                    break;
                case 1:
                    right = Math.Max(value.X, left);
                    top = Math.Min(value.Y, bottom);
                    break;
                case 2:
                    left = Math.Min(value.X, right);
                    bottom = Math.Max(value.Y, top);
                    break;
                case 3:
                    right = Math.Max(value.X, left);
                    bottom = Math.Max(value.Y, top);
                    break;
            }

            X = left;
            Y = top;
            Width = right - left;
            Height = bottom - top;
        }

        protected override void OnRenderVisual(DrawingContext dc)
        {
            Rect bounds = RenderedBounds;

            RedrawParagraph(bounds);
            RedrawOutline(bounds);

            dc.DrawRectangle(Brushes.Transparent, null, bounds);
        }

        protected void RedrawParagraph(Rect bounds)
        {
            Paragraph.Width = bounds.Width;
            Paragraph.Height = bounds.Height;
            Paragraph.TextAlignment = RenderedHorizontalAlignment;
            Paragraph.VerticalContentAlignment = RenderedVerticalAlignment;

            // Measure
            Size availableSize = new Size(bounds.Width, bounds.Height);
            if (Double.IsNaN(availableSize.Height))
            {
                availableSize.Height = Double.PositiveInfinity;
            }
            Paragraph.Measure(availableSize);

            // Arrange
            Rect finalSize = new Rect(
                Math.Round(bounds.X),
                Math.Round(bounds.Y),
                Paragraph.DesiredSize.Width,
                Paragraph.DesiredSize.Height
            );
            Paragraph.Arrange(finalSize);
        }

        protected void RedrawOutline(Rect bounds)
        {
            DrawingContext dc = OutlineDrawing.RenderOpen();
            {
                Pen stroke = new Pen(Brushes.Transparent, 5.0);
                stroke.Freeze();

                dc.DrawRectangle(null, stroke, bounds);

                stroke = new Pen(Brushes.Gray, 1.0);
                stroke.DashStyle = DashStyles.Dash;
                stroke.Freeze();

                bounds.X = Math.Round(bounds.X) - 0.5;
                bounds.Y = Math.Round(bounds.Y) - 0.5;
                bounds.Width = Math.Round(bounds.Width);
                bounds.Height = Math.Round(bounds.Height);
                dc.DrawRectangle(null, stroke, bounds);
            }
            dc.Close();
        }

        void OnLayoutUpdated(object sender, EventArgs e)
        {
            //InvalidateRender();
        }

        #endregion

        #region ISelectableText

        public bool IsTextSelected
        {
            get
            {
                return Paragraph.IsTextSelected;
            }
        }

        public string SelectedText
        {
            get
            {
                return Paragraph.SelectedText;
            }
        }

        #endregion

        #region IFormattable

        public override bool HasProperty(string name)
        {
            if (base.HasProperty(name))
            {
                return true;    
            }

            if (Paragraph.IsSelected)
            {
                return Paragraph.HasProperty(name);
            }

            return false;
        }

        public override void SetProperty(string name, object value)
        {
            // Disallow certain properties for text boxes

            if (name == TextParagraph.IsListItemPropertyName && Object.Equals(value, true) ||
                name == TextParagraph.IsHeadingPropertyName && Object.Equals(value, true))
            {
                return;
            }

            base.SetProperty(name, value);

            if (Paragraph.IsSelected)
            {
                Paragraph.SetProperty(name, value);

                if (name == TextField.TextAlignmentPropertyName)
                {
                    RenderedHorizontalAlignment = (TextAlignment)value;
                }
            }
        }

        public override object GetProperty(string name)
        {
            var result = base.GetProperty(name);

            if (result == null && Paragraph.IsSelected)
            {
                result = Paragraph.GetProperty(name);
            }

            return result;
        }

        public override void ResetProperties()
        {
            base.ResetProperties();

            Paragraph.ResetProperties();

            RenderedHorizontalAlignment = Paragraph.TextAlignment;
        }

        public override int ChangeProperty(string name, object oldValue, object newValue)
        {
            int result = base.ChangeProperty(name, oldValue, newValue);

            result += Paragraph.ChangeProperty(name, oldValue, newValue);

            return result;
        }

        #endregion

        #region IEditable

        public IList<object> Cut()
        {
            if (Paragraph.IsFocused)
            {
                return Paragraph.Cut();
            }
            else
            {
                return new object[] { this };
            }
        }

        public IList<object> Copy()
        {
            if (Paragraph.IsFocused)
            {
                return Paragraph.Copy();
            }
            else
            {
                return new object[] { this };
            }
        }

        public IList<object> Paste(IList<object> objects)
        {
            if (Paragraph.IsFocused)
            {
                return Paragraph.Paste(objects);
            }
            else
            {
                return objects;
            }
        }

        public bool Delete()
        {        
            if (Paragraph.IsFocused)
            {
                Paragraph.Delete();
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region ISearchable

        public int Find(string findText)
        {
            return Paragraph.Find(findText);
        }

        public bool FindFirst()
        {
            return Paragraph.FindFirst();
        }

        public bool FindLast()
        {
            return Paragraph.FindLast();
        }

        public bool FindNext()
        {
            return Paragraph.FindNext();
        }

        public bool FindPrevious()
        {
            return Paragraph.FindPrevious();
        }

        public bool FindFirst(string pattern, RegexOptions options)
        {
            return Paragraph.FindFirst(pattern, options);
        }

        public bool FindLast(string pattern, RegexOptions options)
        {
            return Paragraph.FindLast(pattern, options);
        }

        public bool FindNext(string pattern, RegexOptions options)
        {
            return Paragraph.FindNext(pattern, options);
        }

        public bool FindPrevious(string pattern, RegexOptions options)
        {
            return Paragraph.FindPrevious(pattern, options);
        }

        #endregion

        #region Commands

        void LoadCommandBindings()
        {
            // SilverNote Commands

            Paragraph.CommandBindings.AddRange(new[] {

                // Text Commands
                
                new CommandBinding(NEditingCommands.DeleteForward, DeleteForwardCommand_Executed),
                new CommandBinding(NEditingCommands.DeleteBack, DeleteBackCommand_Executed),
                new CommandBinding(NTextCommands.DeleteForwardByWord, DeleteForwardByWordCommand_Executed),
                new CommandBinding(NTextCommands.DeleteForwardByParagraph, DeleteForwardByParagraphCommand_Executed),
                new CommandBinding(NTextCommands.DeleteBackByWord, DeleteBackByWordCommand_Executed),
                new CommandBinding(NTextCommands.DeleteBackByParagraph, DeleteBackByParagraphCommand_Executed),

                // Navigation Commands

                new CommandBinding(NNavigationCommands.MoveDownByLine, MoveDownByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveLeftByCharacter, MoveLeftByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveRightByCharacter, MoveRightByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveLeftByWord, MoveLeftByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveRightByWord, MoveRightByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToLineEnd, MoveToLineEndCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToLineStart, MoveToLineStartCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToParagraphEnd, MoveToParagraphEndCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveToParagraphStart, MoveToParagraphStartCommand_Executed),
                new CommandBinding(NNavigationCommands.MoveUpByLine, MoveUpByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectDownByLine, SelectDownByLineCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectLeftByCharacter, SelectLeftByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectRightByCharacter, SelectRightByCharacterCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectLeftByWord, SelectLeftByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectRightByWord, SelectRightByWordCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToLineEnd, SelectToLineEndCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToLineStart, SelectToLineStartCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToParagraphEnd, SelectToParagraphEndCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectToParagraphStart, SelectToParagraphStartCommand_Executed),
                new CommandBinding(NNavigationCommands.SelectUpByLine, SelectUpByLineCommand_Executed)
            });
        }

        void DeleteForwardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.DeleteForward();
        }

        void DeleteBackCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.DeleteBack();
        }

        void DeleteForwardByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.DeleteForwardByWord();
        }

        void DeleteForwardByParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.DeleteForwardByParagraph();
        }

        void DeleteBackByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.DeleteBackByWord();
        }

        void DeleteBackByParagraphCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.DeleteBackByParagraph();
        }

        void MoveDownByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveDown();
        }

        void MoveLeftByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveLeft();
        }

        void MoveRightByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveRight();
        }

        void MoveLeftByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveLeftByWord();
        }

        void MoveRightByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveRightByWord();
        }

        void MoveToLineEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveToLineEnd();
        }

        void MoveToLineStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveToLineStart();
        }

        void MoveToParagraphEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveToParagraphEnd();
        }

        void MoveToParagraphStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveToParagraphStart();
        }

        void MoveUpByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.MoveUp();
        }

        void SelectDownByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectDown();
        }

        void SelectLeftByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectLeft();
        }

        void SelectRightByCharacterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectRight();
        }

        void SelectLeftByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectLeftByWord();
        }

        void SelectRightByWordCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectRightByWord();
        }

        void SelectToLineEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectToLineEnd();
        }

        void SelectToLineStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectToLineStart();
        }

        void SelectToParagraphEndCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectToParagraphEnd();
        }

        void SelectToParagraphStartCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectToParagraphStart();
        }

        void SelectUpByLineCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Paragraph.SelectUp();
        }

        #endregion

        #region INodeSource

        public override string GetNodeName(NodeContext context)
        {
            return Paragraph.GetNodeName(context);
        }

        public override IList<string> GetNodeAttributes(ElementContext context)
        {
            return base.GetNodeAttributes(context).Union(Paragraph.GetNodeAttributes(context)).ToList();
        }

        public override string GetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    string classNames = base.GetNodeAttribute(context, name) ?? "";
                    if (HasInitialFocus)
                        classNames = DOMHelper.PrependClass(classNames, "autofocus");
                    return !String.IsNullOrWhiteSpace(classNames) ? classNames : null;
                case SVGAttributes.X:
                    Paragraph.Left = Round(RenderedX, -3);
                    return Paragraph.GetNodeAttribute(context, name);
                case SVGAttributes.Y:
                    Paragraph.Top = Round(RenderedY, -3);
                    return Paragraph.GetNodeAttribute(context, name);
                case HTMLAttributes.PLACEHOLDER:
                    return Watermark;
                case SVGAttributesExt.LAYOUT:
                    return base.GetNodeAttribute(context, name);
                default:
                    return Paragraph.GetNodeAttribute(context, name);
            }
        }

        public override void SetNodeAttribute(ElementContext context, string name, string value)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    HasInitialFocus = DOMHelper.HasClass(value, "autofocus");
                    value = DOMHelper.RemoveClass(value, "autofocus");
                    base.SetNodeAttribute(context, name, value);
                    break;
                case SVGAttributes.X:
                    Paragraph.SetNodeAttribute(context, name, value);
                    RenderedX = Paragraph.Left;
                    break;
                case SVGAttributes.Y:
                    Paragraph.SetNodeAttribute(context, name, value);
                    RenderedY = Paragraph.Top;
                    break;
                case HTMLAttributes.PLACEHOLDER:
                    Watermark = value;
                    break;
                case SVGAttributesExt.LAYOUT:
                    base.SetNodeAttribute(context, name, value);
                    break;
                default:
                    Paragraph.SetNodeAttribute(context, name, value);
                    break;
            }
        }

        public override void ResetNodeAttribute(ElementContext context, string name)
        {
            switch (name)
            {
                case SVGAttributes.CLASS:
                    HasInitialFocus = false;
                    break;
                case SVGAttributes.X:
                    Paragraph.ResetNodeAttribute(context, name);
                    X = 0;
                    break;
                case SVGAttributes.Y:
                    Paragraph.ResetNodeAttribute(context, name);
                    Y = 0;
                    break;
                case HTMLAttributes.PLACEHOLDER:
                    Watermark = null;
                    break;
                case SVGAttributesExt.LAYOUT:
                    base.ResetNodeAttribute(context, name);
                    break;
                default:
                    Paragraph.ResetNodeAttribute(context, name);
                    break;
            }
        }

        public override IEnumerable<object> GetChildNodes(NodeContext context)
        {
            if (!Paragraph.IsMeasureValid || !Paragraph.IsArrangeValid)
            {
                RedrawParagraph(RenderedBounds);
            }

            return Paragraph.GetChildNodes(context);
        }

        public override object CreateNode(NodeContext context)
        {
            return Paragraph.CreateNode(context);
        }

        public override void AppendNode(NodeContext context, object newChild)
        {
            Paragraph.AppendNode(context, newChild);
        }
        public override void InsertNode(NodeContext context, object newChild, object refChild)
        {
            Paragraph.InsertNode(context, newChild, refChild);
        }

        public override void RemoveNode(NodeContext context, object oldChild)
        {
            Paragraph.RemoveNode(context, oldChild);
        }

        #endregion

        #region IStyleable

        public override IList<string> GetSupportedStyles(ElementContext context)
        {
            return Paragraph.GetSupportedStyles(context);
        }

        public override void SetStyleProperty(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.Width:
                    Paragraph.SetStyleProperty(context, name, value);
                    RenderedWidth = Paragraph.Width;
                    break;
                case CSSProperties.Height:
                    Paragraph.SetStyleProperty(context, name, value);
                    RenderedHeight = Paragraph.Height;
                    break;
                case CSSProperties.VerticalAlign:
                    Paragraph.SetStyleProperty(context, name, value);
                    RenderedVerticalAlignment = Paragraph.VerticalContentAlignment;
                    break;
                case CSSProperties.TextAlign:
                    Paragraph.SetStyleProperty(context, name, value);
                    RenderedHorizontalAlignment = Paragraph.TextAlignment;
                    break;
                case "label":  // For backward compatibility; we now use the placeholder attribute
                    if (value != null)
                    {
                        Paragraph.SetStyleProperty(context, name, value);
                        Watermark = Paragraph.Watermark;
                    }
                    break;
                default:
                    Paragraph.SetStyleProperty(context, name, value);
                    break;
            }
        }

        public override CSSValue GetStyleProperty(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.Width:
                    Paragraph.Width = Round(RenderedWidth, -3);
                    return Paragraph.GetStyleProperty(context, name);
                case CSSProperties.Height:
                    Paragraph.Height = Round(RenderedHeight, -3);
                    return Paragraph.GetStyleProperty(context, name);
                case CSSProperties.VerticalAlign:
                    Paragraph.VerticalContentAlignment = RenderedVerticalAlignment;
                    return Paragraph.GetStyleProperty(context, name);
                case CSSProperties.TextAlign:
                    Paragraph.TextAlignment = RenderedHorizontalAlignment;
                    return Paragraph.GetStyleProperty(context, name);
                default:
                    return Paragraph.GetStyleProperty(context, name);
            }
        }

        #endregion

        #region ICloneable

        public override object Clone()
        {
            return new NTextBox(this);
        }

        #endregion

        #region Implementation

        void Paragraph_Changed(TextParagraph oldValue)
        {
            if (oldValue != null)
            {
                oldValue.LayoutUpdated -= OnLayoutUpdated;
                oldValue.GotFocus -= OnChildGotFocus;
                oldValue.LostFocus -= OnChildLostFocus;

                Children.Remove(oldValue);
            }

            var newValue = Paragraph;

            if (newValue != null)
            {
                newValue.LayoutUpdated += OnLayoutUpdated;
                newValue.GotFocus += OnChildGotFocus;
                newValue.LostFocus += OnChildLostFocus;

                Children.Add(newValue);
            }

            UpdateWatermark();
        }

        void OnChildGotFocus(object sender, RoutedEventArgs e)
        {
            Paragraph.Select();
            
            if (Canvas != null)
            {
                Paragraph.UndoStack = Canvas.UndoStack;
            }
        }

        void OnChildLostFocus(object sender, RoutedEventArgs e)
        {
            Paragraph.Unselect();
        }

        protected override void OnShowAdorner()
        {
            UpdateOutlineDrawing();
            UpdateWatermark();
        }

        protected override void OnHideAdorner()
        {
            UpdateOutlineDrawing();
            UpdateWatermark();
        }

        #endregion

    }

}
