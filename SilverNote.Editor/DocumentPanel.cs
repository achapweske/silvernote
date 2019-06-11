/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.IO;
using System.Diagnostics;
using DOM;
using DOM.CSS;
using DOM.HTML;
using DOM.SVG;

namespace SilverNote.Editor
{
    public class DocumentPanel : Panel, ISelectable, IEditable, IMovable, IResizable, IHasResources, INodeSource, IStyleable
    {
        #region Constructors

        public DocumentPanel()
        {

        }

        public DocumentPanel(DocumentPanel copy)
        {
            Margin = copy.Margin;
            Padding = copy.Padding;

            // Positioning
            SetPositioning(this, GetPositioning(copy));
            Left = copy.Left;
            Top = copy.Top;
            Right = copy.Right;
            Bottom = copy.Bottom;
            Width = copy.Width;
            Height = copy.Height;
            HorizontalAlignment = copy.HorizontalAlignment;
            Background = copy.Background;
            // BorderLeft
            BorderLeftStyle = copy.BorderLeftStyle;
            BorderLeftColor = copy.BorderLeftColor;
            BorderLeftWidth = copy.BorderLeftWidth;
            // BorderTop
            BorderTopStyle = copy.BorderTopStyle;
            BorderTopColor = copy.BorderTopColor;
            BorderTopWidth = copy.BorderTopWidth;
            // BorderRight
            BorderRightStyle = copy.BorderRightStyle;
            BorderRightColor = copy.BorderRightColor;
            BorderRightWidth = copy.BorderRightWidth;
            // BorderBottom
            BorderBottomStyle = copy.BorderBottomStyle;
            BorderBottomColor = copy.BorderBottomColor;
            BorderBottomWidth = copy.BorderBottomWidth;
            // BorderRadius
            BorderTopLeftRadius = copy.BorderTopLeftRadius;
            BorderTopRightRadius = copy.BorderTopRightRadius;
            BorderBottomRightRadius = copy.BorderBottomRightRadius;
            BorderBottomLeftRadius = copy.BorderBottomLeftRadius;

            foreach (UIElement child in copy.InternalChildren)
            {
                if (child is ICloneable)
                {
                    InternalChildren.Add((UIElement)((ICloneable)child).Clone());
                }
            }
        }

        #endregion
        
        #region Properties

        #region OwnerDocument

        public static readonly DependencyProperty OwnerDocumentProperty = DependencyProperty.RegisterAttached(
            "OwnerDocument",
            typeof(HTMLDocument),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits)
        );

        public static void SetOwnerDocument(DependencyObject target, HTMLDocument value)
        {
            target.SetValue(OwnerDocumentProperty, value);
        }

        public static HTMLDocument GetOwnerDocument(DependencyObject target)
        {
            return (HTMLDocument)target.GetValue(OwnerDocumentProperty);
        }

        public virtual HTMLDocument OwnerDocument
        {
            get { return GetOwnerDocument(this); }
            set { SetOwnerDocument(this, value); }
        }

        #endregion

        #region Positioning

        public static readonly DependencyProperty PositioningProperty = DependencyProperty.RegisterAttached(
            "Positioning",
            typeof(Positioning),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata ( 
                Positioning.Static, 
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange,
                new PropertyChangedCallback(OnPositioningChanged))
        );

        public static void SetPositioning(DependencyObject dep, Positioning value)
        {
            dep.SetValue(PositioningProperty, value);
        }

        public static Positioning GetPositioning(DependencyObject dep)
        {
            return (Positioning)dep.GetValue(PositioningProperty);
        }

        public static Positioning GetPositioning(object target)
        {
            if (target is DependencyObject)
            {
                return GetPositioning((DependencyObject)target);
            }
            else
            {
                return Positioning.Static;
            }
        }

        public Positioning Positioning
        {
            get { return (Positioning)GetValue(PositioningProperty); }
            set { SetValue(PositioningProperty, value); }
        }

        public Positioning DefaultPositioning
        {
            get { return (Positioning)PositioningProperty.GetMetadata(this).DefaultValue; }
        }

        public static CSSValue ToCSSPositioning(Positioning positioning)
        {
            switch (positioning)
            {
                case Positioning.Static:
                    return CSSValues.Static;
                case Positioning.Relative:
                    return CSSValues.Relative;
                case Positioning.Absolute:
                    return CSSValues.Absolute;
                case Positioning.Fixed:
                    return CSSValues.Fixed;
                case Positioning.Overlapped:
                    return CSSValues.Ident("overlapped");
                default:
                    return CSSValues.Static;
            }
        }

        public static Positioning FromCSSPositioning(CSSValue positioning)
        {
            if (positioning == CSSValues.Static)
            {
                return Positioning.Static;
            }
            if (positioning == CSSValues.Relative)
            {
                return Positioning.Relative;
            }
            if (positioning == CSSValues.Absolute)
            {
                return Positioning.Absolute;
            }
            if (positioning == CSSValues.Fixed)
            {
                return Positioning.Fixed;
            }
            if (positioning == CSSValues.Ident("overlapped"))
            {
                return Positioning.Overlapped;
            }

            return Positioning.Static;
        }

        public static Predicate<object> PositioningEquals(Positioning value)
        {
            return (obj) => GetPositioning(obj) == value;
        }

        public static bool IsPositioningStatic(object obj)
        {
            return GetPositioning(obj) == Positioning.Static;
        }

        public static bool IsPositioningNotStatic(object obj)
        {
            return GetPositioning(obj) != Positioning.Static;
        }

        /// <summary>
        /// Called when the value of PositioningProperty changes on an object
        /// </summary>
        private static void OnPositioningChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var panel = FindPanel(sender);
            var child = sender as UIElement;

            if (panel != null && child != null)
            {
                var oldValue = (Positioning)e.OldValue;
                var newValue = (Positioning)e.NewValue;

                panel.OnPositioningChanged(child, oldValue, newValue);
            }
        }

        protected virtual void OnPositioningChanged(UIElement element, Positioning oldPositioning, Positioning newPositioning)
        {
            // Determine the element's position relative to its NPanel

            Point absolutePosition;
            if (oldPositioning == Positioning.Absolute)
            {
                // Use the old LeftProperty and TopProperty values if valid
                absolutePosition = GetPosition(element);
            }
            else
            {
                // Otherwise determine its current layout position
                absolutePosition = LayoutHelper.GetPosition(element, relativeTo: this);
            }

            var absoluteRect = new Rect(absolutePosition, element.RenderSize);

            // If changing Static -> Absolute, fill the void with blank lines

            if (oldPositioning == Positioning.Static &&
                (newPositioning == Positioning.Absolute || newPositioning == Positioning.Overlapped))
            {
                int index = InternalChildren.IndexOf(element);
                Fill(index, new TextParagraph(), absoluteRect.Height);
                UpdateLayout();
            }

            // Update the position based on the new PositioningProperty value:

            if (newPositioning == Positioning.Absolute)
            {
                SetPosition(element, absolutePosition);
            }
            else
            {
                Debug.Assert(element.GetType() != typeof(PrimaryCanvas));

                int oldIndex = InternalChildren.IndexOf(element);

                // To set an element's relative position, we must first determine
                // what index within InternalChildren it should be relative to

                int newIndex;
                HorizontalAlignment alignment;
                Point relativePosition = GetRelativePosition(absoluteRect, out newIndex, out alignment);

                // Remove the element
                InternalChildren.RemoveAt(oldIndex);
                if (oldIndex < newIndex)
                {
                    newIndex--;
                }

                if (newPositioning == Positioning.Static)
                {
                    // TODO: set left margin to relativePosition.Left
                }
                if (newPositioning == Positioning.Overlapped)
                {
                    // Update its position properties
                    SetPosition(element, relativePosition);
                    element.SetValue(Panel.HorizontalAlignmentProperty, alignment);
                }
                // Insert at its new index
                InternalChildren.Insert(newIndex, element);
            }
        }

        #endregion

        #region Position

        #region Position

        public static Point GetPosition(DependencyObject element)
        {
            return new Point(GetLeft(element), GetTop(element));
        }

        public static void SetPosition(DependencyObject element, Point position)
        {
            SetLeft(element, position.X);
            SetTop(element, position.Y);
        }

        public Point Position
        {
            get { return GetPosition(this); }
            set { SetPosition(this, value); }
        }

        #endregion

        #region Left

        public static readonly DependencyProperty LeftProperty = DependencyProperty.RegisterAttached(
            "Left",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

        public static void SetLeft(DependencyObject element, double value)
        {
            element.SetValue(LeftProperty, value);
        }

        public static double GetLeft(DependencyObject element)
        {
            return (double)element.GetValue(LeftProperty);
        }

        public double Left
        {
            get { return GetLeft(this); }
            set { SetLeft(this, value); }
        }

        public double DefaultLeft
        {
            get { return (double)LeftProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Top

        public static readonly DependencyProperty TopProperty = DependencyProperty.RegisterAttached(
            "Top",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

        public static void SetTop(DependencyObject element, double value)
        {
            Debug.Assert(!Double.IsInfinity(value));
            element.SetValue(TopProperty, value);
        }

        public static double GetTop(DependencyObject element)
        {
            return (double)element.GetValue(TopProperty);
        }

        public double Top
        {
            get { return GetTop(this); }
            set { SetTop(this, value); }
        }

        public double DefaultTop
        {
            get { return (double)TopProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Right

        public static readonly DependencyProperty RightProperty = DependencyProperty.RegisterAttached(
            "Right",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

        public static void SetRight(DependencyObject element, double value)
        {
            element.SetValue(RightProperty, value);
        }

        public static double GetRight(DependencyObject element)
        {
            return (double)element.GetValue(RightProperty);
        }

        public double Right
        {
            get { return GetRight(this); }
            set { SetRight(this, value); }
        }

        public double DefaultRight
        {
            get { return (double)RightProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Bottom

        public static readonly DependencyProperty BottomProperty = DependencyProperty.RegisterAttached(
            "Bottom",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

        public static void SetBottom(DependencyObject element, double value)
        {
            element.SetValue(BottomProperty, value);
        }

        public static double GetBottom(DependencyObject element)
        {
            return (double)element.GetValue(BottomProperty);
        }

        public double Bottom
        {
            get { return GetBottom(this); }
            set { SetBottom(this, value); }
        }

        public double DefaultBottom
        {
            get { return (double)BottomProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region Float

        public enum FloatDirection
        {
            Left,
            Right,
            None
        }

        public static FloatDirection FromFloatCSS(CSSValue cssFloat)
        {
            if (cssFloat == CSSValues.Left)
                return DocumentPanel.FloatDirection.Left;
            if (cssFloat == CSSValues.Right)
                return DocumentPanel.FloatDirection.Right;
            if (cssFloat == CSSValues.None)
                return DocumentPanel.FloatDirection.None;

            return DocumentPanel.FloatDirection.None;
        }

        public static CSSValue ToFloatCSS(DocumentPanel.FloatDirection floatValue)
        {
            switch (floatValue)
            {
                case DocumentPanel.FloatDirection.Left:
                    return CSSValues.Left;
                case DocumentPanel.FloatDirection.Right:
                    return CSSValues.Right;
                case DocumentPanel.FloatDirection.None:
                default:
                    return CSSValues.None;
            }
        }

        public static readonly DependencyProperty FloatProperty = DependencyProperty.RegisterAttached(
            "Float",
            typeof(FloatDirection),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(FloatDirection.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

        public static void SetFloat(UIElement element, FloatDirection value)
        {
            element.SetValue(FloatProperty, value);
        }

        public static FloatDirection GetFloat(UIElement element)
        {
            return (FloatDirection)element.GetValue(FloatProperty);
        }

        public FloatDirection Float
        {
            get { return GetFloat(this); }
            set { SetFloat(this, value); }
        }

        public FloatDirection DefaultFloat
        {
            get { return (FloatDirection)FloatProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Clear

        public enum ClearDirection
        {
            Left,
            Right,
            Both,
            None
        }

        public static ClearDirection FromClearCSS(CSSValue cssClear)
        {
            if (cssClear == CSSValues.Left)
                return DocumentPanel.ClearDirection.Left;
            if (cssClear == CSSValues.Right)
                return DocumentPanel.ClearDirection.Right;
            if (cssClear == CSSValues.Both)
                return DocumentPanel.ClearDirection.Both;
            if (cssClear == CSSValues.None)
                return DocumentPanel.ClearDirection.None;

            return DocumentPanel.ClearDirection.None;
        }

        public static CSSValue ToClearCSS(ClearDirection clearValue)
        {
            switch (clearValue)
            {
                case DocumentPanel.ClearDirection.Left:
                    return CSSValues.Left;
                case DocumentPanel.ClearDirection.Right:
                    return CSSValues.Right;
                case DocumentPanel.ClearDirection.Both:
                    return CSSValues.Both;
                case DocumentPanel.ClearDirection.None:
                default:
                    return CSSValues.None;
            }
        }

        public static readonly DependencyProperty ClearProperty = DependencyProperty.RegisterAttached(
            "Clear",
            typeof(ClearDirection),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(ClearDirection.None, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange)
        );

        public static void SetClear(UIElement element, ClearDirection value)
        {
            element.SetValue(ClearProperty, value);
        }

        public static ClearDirection GetClear(UIElement element)
        {
            return (ClearDirection)element.GetValue(ClearProperty);
        }

        public ClearDirection Clear
        {
            get { return GetClear(this); }
            set { SetClear(this, value); }
        }

        public ClearDirection DefaultClear
        {
            get { return (ClearDirection)ClearProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Background

        public static new readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush DefaultBackground
        {
            get { return (Brush)BackgroundProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Border

        #region BorderStyle

        public enum BorderStyles
        {
            None,
            Solid,
            Dotted,
            Dashed,
            Double,
            Groove,
            Ridge,
            Inset,
            Outset
        }

        public static BorderStyles FromBorderStyleCSS(CSSValue cssBorderStyle)
        {
            if (cssBorderStyle == CSSValues.None)
                return BorderStyles.None;
            if (cssBorderStyle == CSSValues.Solid)
                return BorderStyles.Solid;
            if (cssBorderStyle == CSSValues.Dotted)
                return BorderStyles.Dotted;
            if (cssBorderStyle == CSSValues.Dashed)
                return BorderStyles.Dashed;
            if (cssBorderStyle == CSSValues.Double)
                return BorderStyles.Double;
            if (cssBorderStyle == CSSValues.Groove)
                return BorderStyles.Groove;
            if (cssBorderStyle == CSSValues.Ridge)
                return BorderStyles.Ridge;
            if (cssBorderStyle == CSSValues.Inset)
                return BorderStyles.Inset;
            if (cssBorderStyle == CSSValues.Outset)
                return BorderStyles.Outset;

            return BorderStyles.Solid;
        }

        public static CSSValue ToBorderStyleCSS(BorderStyles borderStyle)
        {
            switch (borderStyle)
            {
                case BorderStyles.None:
                    return CSSValues.None;
                case BorderStyles.Solid:
                    return CSSValues.Solid;
                case BorderStyles.Dotted:
                    return CSSValues.Dotted;
                case BorderStyles.Dashed:
                    return CSSValues.Dashed;
                case BorderStyles.Double:
                    return CSSValues.Double;
                case BorderStyles.Groove:
                    return CSSValues.Groove;
                case BorderStyles.Ridge:
                    return CSSValues.Ridge;
                case BorderStyles.Inset:
                    return CSSValues.Inset;
                case BorderStyles.Outset:
                    return CSSValues.Outset;
                default:
                    return CSSValues.None;
            }
        }

        public BorderStyles BorderStyle
        {
            get
            {
                DocumentPanel.BorderStyles style = BorderLeftStyle;

                if (BorderTopStyle == style &&
                    BorderRightStyle == style &&
                    BorderBottomStyle == style)
                {
                    return style;
                }
                else
                {
                    return DefaultBorderLeftStyle;
                }
            }
            set
            {
                BorderLeftStyle = value;
                BorderTopStyle = value;
                BorderRightStyle = value;
                BorderBottomStyle = value;
            }
        }

        #region BorderLeftStyle

        public static readonly DependencyProperty BorderLeftStyleProperty = DependencyProperty.Register(
            "BorderLeftStyle",
            typeof(BorderStyles),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(BorderStyles.None, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public BorderStyles BorderLeftStyle
        {
            get { return (BorderStyles)GetValue(BorderLeftStyleProperty); }
            set { SetValue(BorderLeftStyleProperty, value); }
        }

        public BorderStyles DefaultBorderLeftStyle
        {
            get { return (BorderStyles)BorderLeftStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopStyle

        public static readonly DependencyProperty BorderTopStyleProperty = DependencyProperty.Register(
            "BorderTopStyle",
            typeof(BorderStyles),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(BorderStyles.None, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public BorderStyles BorderTopStyle
        {
            get { return (BorderStyles)GetValue(BorderTopStyleProperty); }
            set { SetValue(BorderTopStyleProperty, value); }
        }

        public BorderStyles DefaultBorderTopStyle
        {
            get { return (BorderStyles)BorderTopStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderRightStyle

        public static readonly DependencyProperty BorderRightStyleProperty = DependencyProperty.Register(
            "BorderRightStyle",
            typeof(BorderStyles),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(BorderStyles.None, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public BorderStyles BorderRightStyle
        {
            get { return (BorderStyles)GetValue(BorderRightStyleProperty); }
            set { SetValue(BorderRightStyleProperty, value); }
        }

        public BorderStyles DefaultBorderRightStyle
        {
            get { return (BorderStyles)BorderRightStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomStyle

        public static readonly DependencyProperty BorderBottomStyleProperty = DependencyProperty.Register(
            "BorderBottomStyle",
            typeof(BorderStyles),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(BorderStyles.None, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public BorderStyles BorderBottomStyle
        {
            get { return (BorderStyles)GetValue(BorderBottomStyleProperty); }
            set { SetValue(BorderBottomStyleProperty, value); }
        }

        public BorderStyles DefaultBorderBottomStyle
        {
            get { return (BorderStyles)BorderBottomStyleProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region BorderColor

        public Color BorderColor
        {
            get
            {
                Color color = BorderLeftColor;

                if (Object.Equals(BorderTopColor, color) &&
                    Object.Equals(BorderRightColor, color) &&
                    Object.Equals(BorderBottomColor, color))
                {
                    return color;
                }
                else
                {
                    return DefaultBorderLeftColor;
                }
            }
            set
            {
                BorderLeftColor = value;
                BorderTopColor = value;
                BorderRightColor = value;
                BorderBottomColor = value;
            }
        }

        #region BorderLeftColor

        public static readonly DependencyProperty BorderLeftColorProperty = DependencyProperty.Register(
            "BorderLeftColor",
            typeof(Color),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Color BorderLeftColor
        {
            get { return (Color)GetValue(BorderLeftColorProperty); }
            set { SetValue(BorderLeftColorProperty, value); }
        }

        public Color DefaultBorderLeftColor
        {
            get { return (Color)BorderLeftColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopColor

        public static readonly DependencyProperty BorderTopColorProperty = DependencyProperty.Register(
            "BorderTopColor",
            typeof(Color),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Color BorderTopColor
        {
            get { return (Color)GetValue(BorderTopColorProperty); }
            set { SetValue(BorderTopColorProperty, value); }
        }

        public Color DefaultBorderTopColor
        {
            get { return (Color)BorderTopColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderRightColor

        public static readonly DependencyProperty BorderRightColorProperty = DependencyProperty.Register(
            "BorderRightColor",
            typeof(Color),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Color BorderRightColor
        {
            get { return (Color)GetValue(BorderRightColorProperty); }
            set { SetValue(BorderRightColorProperty, value); }
        }

        public Color DefaultBorderRightColor
        {
            get { return (Color)BorderRightColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomColor

        public static readonly DependencyProperty BorderBottomColorProperty = DependencyProperty.Register(
            "BorderBottomColor",
            typeof(Color),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public Color BorderBottomColor
        {
            get { return (Color)GetValue(BorderBottomColorProperty); }
            set { SetValue(BorderBottomColorProperty, value); }
        }

        public Color DefaultBorderBottomColor
        {
            get { return (Color)BorderBottomColorProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region BorderWidth

        public double BorderWidth
        {
            get
            {
                double width = BorderLeftWidth;

                if (Object.Equals(BorderTopWidth, width) &&
                    Object.Equals(BorderRightWidth, width) &&
                    Object.Equals(BorderBottomWidth, width))
                {
                    return width;
                }
                else
                {
                    return DefaultBorderLeftWidth;
                }
            }
            set
            {
                BorderLeftWidth = value;
                BorderTopWidth = value;
                BorderRightWidth = value;
                BorderBottomWidth = value;
            }
        }

        #region BorderLeftWidth

        public static readonly DependencyProperty BorderLeftWidthProperty = DependencyProperty.Register(
            "BorderLeftWidth",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderLeftWidth
        {
            get { return (double)GetValue(BorderLeftWidthProperty); }
            set { SetValue(BorderLeftWidthProperty, value); }
        }

        public double DefaultBorderLeftWidth
        {
            get { return (double)BorderLeftWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopWidth

        public static readonly DependencyProperty BorderTopWidthProperty = DependencyProperty.Register(
            "BorderTopWidth",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderTopWidth
        {
            get { return (double)GetValue(BorderTopWidthProperty); }
            set { SetValue(BorderTopWidthProperty, value); }
        }

        public double DefaultBorderTopWidth
        {
            get { return (double)BorderTopWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderRightWidth

        public static readonly DependencyProperty BorderRightWidthProperty = DependencyProperty.Register(
            "BorderRightWidth",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderRightWidth
        {
            get { return (double)GetValue(BorderRightWidthProperty); }
            set { SetValue(BorderRightWidthProperty, value); }
        }

        public double DefaultBorderRightWidth
        {
            get { return (double)BorderRightWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomWidth

        public static readonly DependencyProperty BorderBottomWidthProperty = DependencyProperty.Register(
            "BorderBottomWidth",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderBottomWidth
        {
            get { return (double)GetValue(BorderBottomWidthProperty); }
            set { SetValue(BorderBottomWidthProperty, value); }
        }

        public double DefaultBorderBottomWidth
        {
            get { return (double)BorderBottomWidthProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #region BorderRadius

        public double BorderRadius
        {
            set
            {
                BorderTopLeftRadius = value;
                BorderTopRightRadius = value;
                BorderBottomRightRadius = value;
                BorderBottomLeftRadius = value;
            }
        }

        #region BorderTopLeftRadius

        public static readonly DependencyProperty BorderTopLeftRadiusProperty = DependencyProperty.Register(
            "BorderTopLeftRadius",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderTopLeftRadius
        {
            get { return (double)GetValue(BorderTopLeftRadiusProperty); }
            set { SetValue(BorderTopLeftRadiusProperty, value); }
        }

        public double DefaultBorderTopLeftRadius
        {
            get { return (double)BorderTopLeftRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderTopRightRadius

        public static readonly DependencyProperty BorderTopRightRadiusProperty = DependencyProperty.Register(
            "BorderTopRightRadius",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderTopRightRadius
        {
            get { return (double)GetValue(BorderTopRightRadiusProperty); }
            set { SetValue(BorderTopRightRadiusProperty, value); }
        }

        public double DefaultBorderTopRightRadius
        {
            get { return (double)BorderTopRightRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomRightRadius

        public static readonly DependencyProperty BorderBottomRightRadiusProperty = DependencyProperty.Register(
            "BorderBottomRightRadius",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderBottomRightRadius
        {
            get { return (double)GetValue(BorderBottomRightRadiusProperty); }
            set { SetValue(BorderBottomRightRadiusProperty, value); }
        }

        public double DefaultBorderBottomRightRadius
        {
            get { return (double)BorderBottomRightRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region BorderBottomLeftRadius

        public static readonly DependencyProperty BorderBottomLeftRadiusProperty = DependencyProperty.Register(
            "BorderBottomLeftRadius",
            typeof(double),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public double BorderBottomLeftRadius
        {
            get { return (double)GetValue(BorderBottomLeftRadiusProperty); }
            set { SetValue(BorderBottomLeftRadiusProperty, value); }
        }

        public double DefaultBorderBottomLeftRadius
        {
            get { return (double)BorderBottomLeftRadiusProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #endregion

        #endregion

        #region Margin

        public Thickness DefaultMargin
        {
            get { return (Thickness)MarginProperty.GetMetadata(this).DefaultValue; }
        }

        #endregion

        #region Padding

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register(
            "Padding",
            typeof(Thickness),
            typeof(DocumentPanel),
            new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure)
        );

        public virtual Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        #endregion

        #endregion

        #region Children

        /// <summary>
        /// Append a new child element
        /// </summary>
        /// <param name="element">The element to be appended</param>
        public virtual void Append(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Insert(Children.Count, element);
        }

        /// <summary>
        /// Insert a new child element
        /// </summary>
        /// <param name="index">Index where the element will be inserted</param>
        /// <param name="element">The element to be inserted</param>
        public virtual void Insert(int index, UIElement element)
        {
            if (index < 0 || index > Children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            InternalChildren.Insert(index, element);

            OnChildAdded(element);
        }

        /// <summary>
        /// Insert a collection of new child elements
        /// </summary>
        /// <param name="index">Index where the element(s) will be inserted</param>
        /// <param name="elements">The element to be inserted</param>
        public void InsertRange(int index, IEnumerable<UIElement> elements)
        {
            foreach (var element in elements)
            {
                Insert(index, element);

                index++;
            }
        }

        /// <summary>
        /// Fill an area with an element.
        /// 
        /// Clones of the given element are repeatedly inserted until
        /// they occupy the space given by the height parameter.
        /// </summary>
        /// <param name="index">Begin filling at this index</param>
        /// <param name="element">Element to be used for filling</param>
        /// <param name="size">Size of the area to be filled</param>
        public virtual int Fill(int index, UIElement element, double height)
        {
            var cloneable = element as ICloneable;
            if (cloneable == null)
            {
                throw new ArgumentException("element must be cloneable");
            }

            Insert(index, element);

            double elementHeight = 16;

            if (!(element is TextParagraph))
            {
                UpdateLayout();
                elementHeight = element.RenderSize.Height;
            }

            height -= elementHeight;

            int count = 1;

            while (height > 0)
            {
                var clone = (UIElement)cloneable.Clone();
                Insert(index + count, clone);
                count++;
                height -= elementHeight;
            }

            return count;
        }

        /// <summary>
        /// Remove the given element
        /// </summary>
        /// <param name="element">The element to be removed</param>
        /// <returns>The index where the element was removed</returns>
        public virtual int Remove(UIElement element)
        {
            int index = InternalChildren.IndexOf(element);
            if (index != -1)
            {
                Children.RemoveAt(index);

                OnChildRemoved(element);
            }
            return index;
        }

        /// <summary>
        /// Remove a collection of child elements
        /// </summary>
        /// <param name="elements">The child elements to be removed</param>
        public virtual int RemoveRange(IEnumerable<UIElement> elements)
        {
            int result = 0;

            foreach (UIElement element in elements)
            {
                if (Remove(element) != -1)
                {
                    result++;
                }
            }

            return result;
        }

        /// <summary>
        /// Remove all child elements
        /// </summary>
        public virtual void RemoveAll()
        {
            var children = InternalChildren.OfType<UIElement>().ToArray();

            foreach (var child in children)
            {
                Remove(child);
            }
        }

        public void Move(UIElement element, int insertBefore)
        {
            int elementIndex = InternalChildren.IndexOf(element);

            Remove(element);

            if (insertBefore > elementIndex)
            {
                insertBefore -= 1;
            }

            Insert(insertBefore, element);
        }

        public void MoveRange(IEnumerable<UIElement> elements, int insertBefore)
        {
            foreach (var element in elements)
            {
                int elementIndex = InternalChildren.IndexOf(element);

                Remove(element);

                if (insertBefore > elementIndex)
                {
                    insertBefore -= 1;
                }

                Insert(insertBefore, element);

                insertBefore++;
            }
        }

        public T Find<T>(Predicate<T> match) where T : class
        {
            return Find<T>(0, match);
        }

        public T Find<T>(int startIndex, Predicate<T> match) where T : class
        {
            int index = FindIndex<T>(startIndex, match);
            if (index != -1)
            {
                return InternalChildren[index] as T;
            }
            else
            {
                return null;
            }
        }

        public int FindIndex<T>(int startIndex, Predicate<T> match) where T : class
        {
            for (int i = startIndex; i < InternalChildren.Count; i++)
            {
                var item = InternalChildren[i] as T;
                if (item != null && match(item))
                {
                    return i;
                }
            }

            return -1;
        }

        public T FindLast<T>(Predicate<T> match) where T : class
        {
            return FindLast<T>(InternalChildren.Count - 1, match);
        }

        public T FindLast<T>(int startIndex, Predicate<T> match) where T : class
        {
            int index = FindLastIndex<T>(startIndex, match);
            if (index != -1)
            {
                return InternalChildren[index] as T;
            }
            else
            {
                return null;
            }
        }

        public int FindLastIndex<T>(Predicate<T> match) where T : class
        {
            return FindLastIndex<T>(InternalChildren.Count - 1, match);
        }

        public int FindLastIndex(int startIndex, Predicate<UIElement> match)
        {
            return FindLastIndex<UIElement>(startIndex, match);
        }

        public int FindLastIndex<T>(int startIndex, Predicate<T> match) where T : class
        {
            for (int i = startIndex; i >= 0; i--)
            {
                var item = InternalChildren[i] as T;
                if (item != null && match(item))
                {
                    return i;
                }
            }

            return -1;
        }

        public T FindNext<T>(UIElement after, Predicate<T> match) where T : class
        {
            int startIndex = InternalChildren.IndexOf(after);
            if (startIndex != -1)
            {
                return Find<T>(startIndex + 1, match);
            }
            else
            {
                return null;
            }
        }

        public T FindNext<T>(UIElement after, Predicate<T> match, Visibility visibility) where T : class
        {
            int startIndex = InternalChildren.IndexOf(after);
            if (startIndex == -1)
            {
                return null;
            }

            int resultIndex = FindIndex<T>(startIndex + 1, match);

            while (resultIndex != -1)
            {
                if (InternalChildren[resultIndex].Visibility == visibility)
                {
                    break;
                }

                resultIndex = FindIndex<T>(resultIndex + 1, match);
            }

            if (resultIndex != -1)
            {
                return InternalChildren[resultIndex] as T;
            }
            else
            {
                return null;
            }
        }

        public T FindPrevious<T>(UIElement before, Predicate<T> match) where T : class
        {
            int startIndex = InternalChildren.IndexOf(before);
            if (startIndex != -1)
            {
                return FindLast<T>(startIndex - 1, match);
            }
            else
            {
                return null;
            }
        }

        public T FindPrevious<T>(UIElement before, Predicate<T> match, Visibility visibility) where T : class
        {
            int startIndex = InternalChildren.IndexOf(before);
            if (startIndex == -1)
            {
                return null;
            }

            int resultIndex = FindLastIndex<T>(startIndex - 1, match);

            while (resultIndex != -1)
            {
                if (InternalChildren[resultIndex].Visibility == visibility)
                {
                    break;
                }

                resultIndex = FindLastIndex<T>(resultIndex - 1, match);
            }

            if (resultIndex != -1)
            {
                return InternalChildren[resultIndex] as T;
            }
            else
            {
                return null;
            }
        }

        protected virtual void OnChildAdded(UIElement element)
        {

        }

        protected virtual void OnChildRemoved(UIElement element)
        {

        }

        #endregion

        #region Visual Children

        private VisualCollection _BackgroundVisuals;

        public VisualCollection BackgroundVisuals
        {
            get { return _BackgroundVisuals ?? (_BackgroundVisuals = new VisualCollection(this)); }
        }

        private VisualCollection _ForegroundVisuals;

        public VisualCollection ForegroundVisuals
        {
            get { return _ForegroundVisuals ?? (_ForegroundVisuals = new VisualCollection(this)); }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                int result = base.VisualChildrenCount;

                if (_BackgroundVisuals != null)
                {
                    result += _BackgroundVisuals.Count;
                }
                if (_ForegroundVisuals != null)
                {
                    result += _ForegroundVisuals.Count;
                }

                return result;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (_BackgroundVisuals != null)
            {
                if (index < _BackgroundVisuals.Count)
                {
                    return _BackgroundVisuals[index];
                }

                index -= _BackgroundVisuals.Count;
            }

            if (index < base.VisualChildrenCount)
            {
                return base.GetVisualChild(index);
            }

            index -= base.VisualChildrenCount;

            if (_ForegroundVisuals != null)
            {
                return _ForegroundVisuals[index];
            }

            throw new IndexOutOfRangeException();
        }

        #endregion

        #region ISelectable

        private bool _IsSelected;

        public virtual bool IsSelected 
        {
            get { return _IsSelected; }
        }

        public virtual void Select()
        {
            _IsSelected = true;

            AddSelectionAdorner();
        }

        public virtual void Unselect()
        {
            _IsSelected = false;

            RemoveSelectionAdorner();
        }

        #region Selection Adorner

        public Adorner SelectionAdorner { get; set; }

        private Adorner CurrentSelectionAdorner { get; set; }

        private void AddSelectionAdorner()
        {
            RemoveSelectionAdorner();

            if (SelectionAdorner != null)
            {
                AddAdorner(SelectionAdorner);
                CurrentSelectionAdorner = SelectionAdorner;
            }
        }

        private void RemoveSelectionAdorner()
        {
            if (CurrentSelectionAdorner != null)
            {
                RemoveAdorner(CurrentSelectionAdorner);
                CurrentSelectionAdorner = null;
            }

        }

        public void AddAdorner(Adorner adorner)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Add(adorner);
            }
        }

        public void RemoveAdorner(Adorner adorner)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (adornerLayer != null)
            {
                adornerLayer.Remove(adorner);
            }
        }

        #endregion

        #endregion

        #region IEditable

        public virtual IList<object> Cut()
        {
            return new object[] { this };
        }

        public virtual IList<object> Copy()
        {
            var cloneable = this as ICloneable;
            if (cloneable != null)
            {
                return new object[] { cloneable.Clone() };
            }
            else
            {
                return new object[0];
            }
        }

        public virtual IList<object> Paste(IList<object> objects)
        {
            return objects;
        }

        public virtual bool Delete()
        {
            return false;
        }

        #endregion

        #region IFormattable

        public virtual bool HasProperty(string name)
        {
            return false;
        }

        public virtual object GetProperty(string name)
        {
            return null;
        }

        public virtual void SetProperty(string name, object value)
        {

        }

        public virtual void ResetProperties()
        {

        }

        public virtual int ChangeProperty(string name, object oldValue, object newValue)
        {
            return 0;
        }

        #endregion

        #region IMovable

        public virtual void MoveStarted()
        {
            Positioning = Positioning.Absolute;
            Margin = new Thickness(0);
        }

        public virtual void MoveDelta(Vector delta)
        {
            Position += delta;
        }

        public virtual void MoveCompleted()
        {
            Positioning = Positioning.Overlapped;
        }

        public void RequestBeginMove()
        {
            var e = new RoutedEventArgs(Movable.RequestingBeginMove);
            this.RaiseEvent(e);
        }

        public void RequestMoveDelta(Vector delta)
        {
            var e = new MoveDeltaEventArgs(Movable.RequestingMoveDelta, delta);
            this.RaiseEvent(e);
        }

        public void RequestEndMove()
        {
            var e = new RoutedEventArgs(Movable.RequestingEndMove);
            this.RaiseEvent(e);
        }

        #endregion

        #region IResizable

        public virtual void Resize(Vector delta)
        {
            double width = ComputedWidth;
            double height = ComputedHeight;
            Width = Math.Max(width + delta.X, 0);
            Height = Math.Max(height + delta.Y, 0);
        }

        protected virtual double ComputedWidth
        {
            get
            {
                if (!Double.IsNaN(Width))
                {
                    return Width;
                }
                else if (IsArrangeValid)
                {
                    return RenderSize.Width;
                }
                else if (IsMeasureValid)
                {
                    return DesiredSize.Width;
                }
                else
                {
                    return 0;
                }
            }
        }

        protected virtual double ComputedHeight
        {
            get
            {
                if (!Double.IsNaN(Height))
                {
                    return Height;
                }
                else if (IsArrangeValid)
                {
                    return RenderSize.Height;
                }
                else if (IsMeasureValid)
                {
                    return DesiredSize.Height;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        #region IHasResources

        #region ResourceChangedEvent

        public static RoutedEvent ResourceChangedEvent = ResourceContainer.ResourceChangedEvent.AddOwner(typeof(DocumentPanel));

        protected void RaiseResourceChanged(string url)
        {
            RaiseEvent(new ResourceEventArgs(ResourceChangedEvent, this, url));
        }

        public event ResourceEventHandler ResourceChanged
        {
            add { base.AddHandler(ResourceChangedEvent, value); }
            remove { base.RemoveHandler(ResourceChangedEvent, value); }
        }

        #endregion

        #region ResourceRequestedEvent

        public static RoutedEvent ResourceRequestedEvent = ResourceContainer.ResourceRequestedEvent.AddOwner(typeof(DocumentPanel));

        protected void RaiseResourceRequested(string url)
        {
            RaiseEvent(new ResourceEventArgs(ResourceRequestedEvent, this, url));
        }

        public event ResourceEventHandler ResourceRequested
        {
            add { base.AddHandler(ResourceRequestedEvent, value); }
            remove { base.RemoveHandler(ResourceRequestedEvent, value); }
        }

        #endregion

        public virtual IEnumerable<string> ResourceNames
        {
            get 
            {
                foreach (var child in InternalChildren.OfType<IHasResources>())
                {
                    foreach (var fileName in child.ResourceNames)
                    {
                        yield return fileName;
                    }
                }
            }
        }

        public void GetResource(string url, Stream stream)
        {
            OnGetResource(url, stream);
        }

        protected virtual void OnGetResource(string url, Stream stream)
        {
            foreach (var child in InternalChildren.OfType<IHasResources>())
            {
                if (child.ResourceNames.Contains(url))
                {
                    child.GetResource(url, stream);
                    break;
                }
            }
        }

        public void SetResource(string url, Stream stream)
        {
            OnSetResource(url, stream);
        }

        protected virtual void OnSetResource(string url, Stream stream)
        {
            foreach (var child in InternalChildren.OfType<IHasResources>())
            {
                if (child.ResourceNames.Contains(url))
                {
                    child.SetResource(url, stream);
                    break;
                }
            }
        }

        #endregion

        #region INodeSource

        public virtual NodeType GetNodeType(NodeContext context)
        {
            if (context is HTMLDocument || context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLNodeType(context);
            }
            else
            {
                return GetSVGNodeType(context);
            }
        }

        public virtual string GetNodeName(NodeContext context)
        {
            if (context is HTMLDocument || context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLNodeName(context);
            }
            else
            {
                return GetSVGNodeName(context);
            }
        }

        public virtual IList<string> GetNodeAttributes(ElementContext context)
        {
            if (context is HTMLElement)
            {
                return GetHTMLAttributes(context);
            }
            else
            {
                return GetSVGAttributes(context);
            }
        }

        public virtual string GetNodeAttribute(ElementContext context, string name)
        {
            if (context is HTMLElement)
            {
                return GetHTMLAttribute(context, name);
            }
            else
            {
                return GetSVGAttribute(context, name);
            }
        }

        public virtual void SetNodeAttribute(ElementContext context, string name, string value)
        {
            if (context is HTMLElement)
            {
                SetHTMLAttribute(context, name, value);
            }
            else
            {
                SetSVGAttribute(context, name, value);
            }
        }

        public virtual void ResetNodeAttribute(ElementContext context, string name)
        {
            if (context is HTMLElement)
            {
                ResetHTMLAttribute(context, name);
            }
            else
            {
                ResetSVGAttribute(context, name);
            }
        }

        public virtual object GetParentNode(NodeContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLParentNode(context);
            }
            else
            {
                return GetSVGParentNode(context);
            }
        }

        public virtual IEnumerable<object> GetChildNodes(NodeContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLChildNodes(context);
            }
            else
            {
                return GetSVGChildNodes(context);
            }
        }

        public virtual object CreateNode(NodeContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return CreateHTMLNode(context);
            }
            else
            {
                return CreateSVGNode(context);
            }
        }

        public virtual void AppendNode(NodeContext context, object newChild)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                AppendHTMLNode(context, newChild);
            }
            else
            {
                AppendSVGNode(context, newChild);
            }
        }

        public virtual void InsertNode(NodeContext context, object newChild, object refChild)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                InsertHTMLNode(context, newChild, refChild);
            }
            else
            {
                InsertSVGNode(context, newChild, refChild);
            }
        }

        public virtual void RemoveNode(NodeContext context, object oldChild)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                RemoveHTMLNode(context, oldChild);
            }
            else
            {
                RemoveSVGNode(context, oldChild);
            }
        }

        public event NodeEventHandler NodeEvent;

        protected void RaiseNodeEvent(IEventSource e)
        {
            if (NodeEvent != null)
            {
                NodeEvent(e);
            }
        }

        #endregion

        #region IStyleable

        public virtual IList<string> GetSupportedStyles(ElementContext context)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLStyles(context);
            }
            else
            {
                return GetSVGStyles(context);
            }
        }

        public virtual CSSValue GetStyleProperty(ElementContext context, string name)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                return GetHTMLStyle(context, name);
            }
            else
            {
                return GetSVGStyle(context, name);
            }
        }

        public virtual void SetStyleProperty(ElementContext context, string name, CSSValue value)
        {
            if (context.OwnerDocument is HTMLDocument)
            {
                SetHTMLStyle(context, name, value);
            }
            else
            {
                SetSVGStyle(context, name, value);
            }
        }

        #endregion

        #region HTML

        public virtual NodeType GetHTMLNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        string _NodeName;

        public void SetNodeName(string nodeName)
        {
            _NodeName = nodeName;
        }

        public virtual string GetHTMLNodeName(NodeContext context)
        {
            if (_NodeName != null)
            {
                return _NodeName;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public virtual IList<string> GetHTMLAttributes(ElementContext context)
        {
            return context.GetInternalAttributes();
        }

        public virtual string GetHTMLAttribute(ElementContext context, string name)
        {
            return context.GetInternalAttribute(name);
        }

        public virtual void SetHTMLAttribute(ElementContext context, string name, string value)
        {
            context.SetInternalAttribute(name, value);
        }

        public virtual void ResetHTMLAttribute(ElementContext context, string name)
        {
            context.RemoveInternalAttribute(name);
        }

        public virtual object GetHTMLParentNode(NodeContext context)
        {
            return Parent;
        }

        public virtual IEnumerable<object> GetHTMLChildNodes(NodeContext context)
        {
            return InternalChildren.OfType<INodeSource>();
        }

        public virtual object CreateHTMLNode(NodeContext newNode)
        {
            var element = newNode as HTMLElement;
            if (element == null)
            {
                return null;
            }

            if (element.TagName == HTMLElements.OBJECT)
            {
                switch (element.GetAttribute(HTMLAttributes.TYPE))
                {
                    case NFile.MIME_TYPE:
                        return new NFile();
                    case NCanvas.MIME_TYPE:
                        return new NCanvas();
                    default:
                        return null;
                }
            }

            switch (element.TagName)
            {
                case HTMLElements.BR:
                    return new TextParagraph();
                case HTMLElements.H1:
                    return new NHeading();
                case HTMLElements.IMG:
                    return new NImage();
                case HTMLElements.LI:
                    return new TextParagraph();
                case HTMLElements.P:
                case HTMLElements.PRE:
                    return new TextParagraph();
                case HTMLElements.SPAN:
                    return new TextFragment();
                case HTMLElements.H2:
                case HTMLElements.H3:
                case HTMLElements.H4:
                case HTMLElements.H5:
                case HTMLElements.H6:
                    var heading = new TextParagraph();
                    heading.SetProperty(TextProperties.FontClassProperty, element.TagName.ToLower());
                    return heading;
                case HTMLElements.TABLE:
                    return new NTable();
                // SVG is normally referenced by an OBJECT element, but we support inline SVG as well
                case SVGElements.SVG:
                    return new NCanvas();
                default:
                    return null;
            }
        }

        public virtual void AppendHTMLNode(NodeContext context, object newChild)
        {
            Append((UIElement)newChild);
        }

        public virtual void InsertHTMLNode(NodeContext context, object newChild, object refChild)
        {
            int index = InternalChildren.IndexOf((UIElement)refChild);
            if (index != -1)
            {
                Insert(index, (UIElement)newChild);
            }
            else
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public virtual void RemoveHTMLNode(NodeContext context, object oldChild)
        {
            if (Remove((UIElement)oldChild) == -1)
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        private static readonly string[] _HTMLStyles = new[]
            {
                CSSProperties.BackgroundColor,
                CSSProperties.BorderTopColor,
                CSSProperties.BorderRightColor,
                CSSProperties.BorderBottomColor,
                CSSProperties.BorderLeftColor,
                CSSProperties.BorderTopStyle,
                CSSProperties.BorderRightStyle,
                CSSProperties.BorderBottomStyle,
                CSSProperties.BorderLeftStyle,
                CSSProperties.BorderTopWidth,
                CSSProperties.BorderRightWidth,
                CSSProperties.BorderBottomWidth,
                CSSProperties.BorderLeftWidth,
                CSSProperties.BorderTopLeftRadius,
                CSSProperties.BorderTopRightRadius,
                CSSProperties.BorderBottomRightRadius,
                CSSProperties.BorderBottomLeftRadius,
                CSSProperties.MarginTop,
                CSSProperties.MarginRight,
                CSSProperties.MarginBottom,
                CSSProperties.MarginLeft,
                CSSProperties.PaddingTop,
                CSSProperties.PaddingRight,
                CSSProperties.PaddingBottom,
                CSSProperties.PaddingLeft,
                CSSProperties.Position,
                CSSProperties.Left,
                CSSProperties.Top,
                CSSProperties.Right,
                CSSProperties.Bottom,
                CSSProperties.Width,
                CSSProperties.Height,
                CSSProperties.Float,
                CSSProperties.Clear
            };

        /// <summary>
        /// Get the CSS styles supported by this element
        /// </summary>
        public virtual IList<string> GetHTMLStyles(ElementContext context)
        {
            return _HTMLStyles;
        }

        /// <summary>
        /// Get a CSS style for this element
        /// </summary>
        /// <param name="name">Name of the style to retrieve</param>
        /// <returns>The CSS value, or null if not set</returns>
        public virtual CSSValue GetHTMLStyle(ElementContext context, string name)
        {
            switch (name)
            {
                case CSSProperties.BackgroundColor:

                    return CSSConverter.ToCSSValue(Background, CSSValues.Transparent);

                case CSSProperties.BorderTopColor:

                    return CSSConverter.ToCSSValue(BorderTopColor);

                case CSSProperties.BorderRightColor:

                    return CSSConverter.ToCSSValue(BorderRightColor);

                case CSSProperties.BorderBottomColor:

                    return CSSConverter.ToCSSValue(BorderBottomColor);

                case CSSProperties.BorderLeftColor:

                    return CSSConverter.ToCSSValue(BorderLeftColor);

                case CSSProperties.BorderTopStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderTopStyle);

                case CSSProperties.BorderRightStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderRightStyle);

                case CSSProperties.BorderBottomStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderBottomStyle);

                case CSSProperties.BorderLeftStyle:

                    return DocumentPanel.ToBorderStyleCSS(BorderLeftStyle);

                case CSSProperties.BorderTopWidth:

                    if (BorderTopStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderTopWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderRightWidth:

                    if (BorderRightStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderRightWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderBottomWidth:

                    if (BorderBottomStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderBottomWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderLeftWidth:

                    if (BorderLeftStyle != DocumentPanel.BorderStyles.None)
                        return CSSValues.Pixels(BorderLeftWidth);
                    else
                        return CSSValues.Zero;

                case CSSProperties.BorderTopLeftRadius:

                    var borderTopLeftRadius = CSSValues.Pixels(BorderTopLeftRadius);
                    return CSSValues.List(new[] { borderTopLeftRadius });

                case CSSProperties.BorderTopRightRadius:

                    var borderTopRightRadius = CSSValues.Pixels(BorderTopRightRadius);
                    return CSSValues.List(new[] { borderTopRightRadius });

                case CSSProperties.BorderBottomRightRadius:

                    var borderBottomRightRadius = CSSValues.Pixels(BorderBottomRightRadius);
                    return CSSValues.List(new[] { borderBottomRightRadius });

                case CSSProperties.BorderBottomLeftRadius:

                    var borderBottomLeftRadius = CSSValues.Pixels(BorderBottomLeftRadius);
                    return CSSValues.List(new[] { borderBottomLeftRadius });

                case CSSProperties.MarginTop:

                    return CSSValues.Pixels(Margin.Top);

                case CSSProperties.MarginRight:

                    if (DocumentPanel.GetRelativeAlignment(this) == HorizontalAlignment.Center)
                        return CSSValues.Auto;
                    return CSSValues.Pixels(Margin.Right);

                case CSSProperties.MarginBottom:

                    return CSSValues.Pixels(Margin.Bottom);

                case CSSProperties.MarginLeft:

                    if (DocumentPanel.GetRelativeAlignment(this) == HorizontalAlignment.Center)
                        return CSSValues.Auto;
                    return CSSValues.Pixels(Margin.Left);

                case CSSProperties.PaddingTop:

                    return CSSValues.Pixels(Padding.Top);

                case CSSProperties.PaddingRight:

                    return CSSValues.Pixels(Padding.Right);

                case CSSProperties.PaddingBottom:

                    return CSSValues.Pixels(Padding.Bottom);

                case CSSProperties.PaddingLeft:

                    return CSSValues.Pixels(Padding.Left);

                case CSSProperties.Position:

                    var positioning = this.Positioning;
                    if (positioning == Editor.Positioning.Absolute)
                        return DocumentPanel.ToCSSPositioning(Editor.Positioning.Overlapped);
                    else
                        return DocumentPanel.ToCSSPositioning(positioning);

                case CSSProperties.Left:

                    double left = DocumentPanel.GetRelativePosition(this).X;
                    if (Double.IsNaN(left))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(left);

                case CSSProperties.Top:

                    double top = DocumentPanel.GetRelativePosition(this).Y;
                    if (Double.IsNaN(top))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(top);

                case CSSProperties.Right:

                    if (DocumentPanel.GetRelativeAlignment(this) == HorizontalAlignment.Right)
                        return CSSValues.Pixels(0);
                    else if (Double.IsNaN(Right) || Right == 0)
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Right);

                case CSSProperties.Bottom:

                    if (Double.IsNaN(Bottom))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Bottom);

                case CSSProperties.Width:

                    if (Double.IsNaN(Width))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Width);

                case CSSProperties.Height:

                    if (Double.IsNaN(Height))
                        return CSSValues.Auto;
                    else
                        return CSSValues.Pixels(Height);

                case CSSProperties.Float:

                    return DocumentPanel.ToFloatCSS(Float);

                case CSSProperties.Clear:

                    return DocumentPanel.ToClearCSS(Clear);

                default:
                    return null;
            }
        }

        private bool _IsMarginLeftAuto;
        private bool _IsMarginRightAuto;

        /// <summary>
        /// Set a CSS style for this element
        /// </summary>
        public virtual void SetHTMLStyle(ElementContext context, string name, CSSValue value)
        {
            switch (name)
            {
                case CSSProperties.BackgroundColor:

                    var backgroundColor = (CSSPrimitiveValue)value;
                    if (backgroundColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        Background = CSSConverter.ToBrush(backgroundColor);
                    else if (backgroundColor == CSSValues.Transparent)
                        Background = DefaultBackground;
                    break;

                case CSSProperties.BorderTopColor:

                    var borderTopColor = (CSSPrimitiveValue)value;
                    if (borderTopColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderTopColor = CSSConverter.ToColor(borderTopColor);
                    else if (borderTopColor == CSSValues.Transparent)
                        BorderTopColor = DefaultBorderTopColor;
                    break;

                case CSSProperties.BorderRightColor:

                    var borderRightColor = (CSSPrimitiveValue)value;
                    if (borderRightColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderRightColor = CSSConverter.ToColor(borderRightColor);
                    else if (borderRightColor == CSSValues.Transparent)
                        BorderRightColor = DefaultBorderRightColor;
                    break;

                case CSSProperties.BorderBottomColor:

                    var borderBottomColor = (CSSPrimitiveValue)value;
                    if (borderBottomColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderBottomColor = CSSConverter.ToColor(borderBottomColor);
                    else if (borderBottomColor == CSSValues.Transparent)
                        BorderBottomColor = DefaultBorderBottomColor;
                    break;

                case CSSProperties.BorderLeftColor:

                    var borderLeftColor = (CSSPrimitiveValue)value;
                    if (borderLeftColor.PrimitiveType == CSSPrimitiveType.CSS_RGBCOLOR)
                        BorderLeftColor = CSSConverter.ToColor(borderLeftColor);
                    else if (borderLeftColor == CSSValues.Transparent)
                        BorderLeftColor = DefaultBorderLeftColor;
                    break;

                case CSSProperties.BorderTopStyle:

                    var borderTopStyle = (CSSPrimitiveValue)value;
                    BorderTopStyle = DocumentPanel.FromBorderStyleCSS(borderTopStyle);
                    break;

                case CSSProperties.BorderRightStyle:

                    var borderRightStyle = (CSSPrimitiveValue)value;
                    BorderRightStyle = DocumentPanel.FromBorderStyleCSS(borderRightStyle);
                    break;

                case CSSProperties.BorderBottomStyle:

                    var borderBottomStyle = (CSSPrimitiveValue)value;
                    BorderBottomStyle = DocumentPanel.FromBorderStyleCSS(borderBottomStyle);
                    break;

                case CSSProperties.BorderLeftStyle:

                    var borderLeftStyle = (CSSPrimitiveValue)value;
                    BorderLeftStyle = DocumentPanel.FromBorderStyleCSS(borderLeftStyle);
                    break;

                case CSSProperties.BorderTopWidth:

                    var borderTopWidth = (CSSPrimitiveValue)value;
                    if (borderTopWidth.IsLength())
                        BorderTopWidth = borderTopWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderRightWidth:

                    var borderRightWidth = (CSSPrimitiveValue)value;
                    if (borderRightWidth.IsLength())
                        BorderRightWidth = borderRightWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderBottomWidth:

                    var borderBottomWidth = (CSSPrimitiveValue)value;
                    if (borderBottomWidth.IsLength())
                        BorderBottomWidth = borderBottomWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderLeftWidth:

                    var borderLeftWidth = (CSSPrimitiveValue)value;
                    if (borderLeftWidth.IsLength())
                        BorderLeftWidth = borderLeftWidth.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.BorderTopLeftRadius:

                    var borderTopLeftRadius = (CSSValueList)value;
                    if (borderTopLeftRadius.Length > 0)
                    {
                        var borderTopLeftRadiusX = (CSSPrimitiveValue)borderTopLeftRadius[0];
                        if (borderTopLeftRadiusX.IsLength())
                        {
                            BorderTopLeftRadius = borderTopLeftRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.BorderTopRightRadius:

                    var borderTopRightRadius = (CSSValueList)value;
                    if (borderTopRightRadius.Length > 0)
                    {
                        var borderTopRightRadiusX = (CSSPrimitiveValue)borderTopRightRadius[0];
                        if (borderTopRightRadiusX.IsLength())
                        {
                            BorderTopRightRadius = borderTopRightRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.BorderBottomRightRadius:

                    var borderBottomRightRadius = (CSSValueList)value;
                    if (borderBottomRightRadius.Length > 0)
                    {
                        var borderBottomRightRadiusX = (CSSPrimitiveValue)borderBottomRightRadius[0];
                        if (borderBottomRightRadiusX.IsLength())
                        {
                            BorderBottomRightRadius = borderBottomRightRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.BorderBottomLeftRadius:

                    var borderBottomLeftRadius = (CSSValueList)value;
                    if (borderBottomLeftRadius.Length > 0)
                    {
                        var borderBottomLeftRadiusX = (CSSPrimitiveValue)borderBottomLeftRadius[0];
                        if (borderBottomLeftRadiusX.IsLength())
                        {
                            BorderBottomLeftRadius = borderBottomLeftRadiusX.GetFloatValue(CSSPrimitiveType.CSS_PX);
                        }
                    }
                    break;

                case CSSProperties.MarginTop:

                    var marginTop = (CSSPrimitiveValue)value;
                    if (marginTop.IsLength())
                        Margin = new Thickness(Margin.Left, marginTop.GetFloatValue(CSSPrimitiveType.CSS_PX), Margin.Right, Margin.Bottom);
                    break;

                case CSSProperties.MarginRight:

                    var marginRight = (CSSPrimitiveValue)value;
                    if (marginRight.IsLength())
                        Margin = new Thickness(Margin.Left, Margin.Top, marginRight.GetFloatValue(CSSPrimitiveType.CSS_PX), Margin.Bottom);
                    _IsMarginRightAuto = (marginRight == CSSValues.Auto);
                    if (_IsMarginLeftAuto && _IsMarginRightAuto)
                        HorizontalAlignment = HorizontalAlignment.Center;
                    break;

                case CSSProperties.MarginBottom:

                    var marginBottom = (CSSPrimitiveValue)value;
                    if (marginBottom.IsLength())
                        Margin = new Thickness(Margin.Left, Margin.Top, Margin.Right, marginBottom.GetFloatValue(CSSPrimitiveType.CSS_PX));
                    break;

                case CSSProperties.MarginLeft:

                    var marginLeft = (CSSPrimitiveValue)value;
                    if (marginLeft.IsLength())
                        Margin = new Thickness(marginLeft.GetFloatValue(CSSPrimitiveType.CSS_PX), Margin.Top, Margin.Right, Margin.Bottom);
                    _IsMarginLeftAuto = (marginLeft == CSSValues.Auto);
                    if (_IsMarginLeftAuto && _IsMarginRightAuto)
                        HorizontalAlignment = HorizontalAlignment.Center;
                    break;

                case CSSProperties.PaddingTop:

                    var paddingTop = (CSSPrimitiveValue)value;
                    if (paddingTop.IsLength())
                        Padding = new Thickness(Padding.Left, paddingTop.GetFloatValue(CSSPrimitiveType.CSS_PX), Padding.Right, Padding.Bottom);
                    break;

                case CSSProperties.PaddingRight:

                    var paddingRight = (CSSPrimitiveValue)value;
                    if (paddingRight.IsLength())
                        Padding = new Thickness(Padding.Left, Padding.Top, paddingRight.GetFloatValue(CSSPrimitiveType.CSS_PX), Padding.Bottom);
                    break;

                case CSSProperties.PaddingBottom:

                    var paddingBottom = (CSSPrimitiveValue)value;
                    if (paddingBottom.IsLength())
                        Padding = new Thickness(Padding.Left, Padding.Top, Padding.Right, paddingBottom.GetFloatValue(CSSPrimitiveType.CSS_PX));
                    break;

                case CSSProperties.PaddingLeft:

                    var paddingLeft = (CSSPrimitiveValue)value;
                    if (paddingLeft.IsLength())
                        Padding = new Thickness(paddingLeft.GetFloatValue(CSSPrimitiveType.CSS_PX), Padding.Top, Padding.Right, Padding.Bottom);
                    break;

                case CSSProperties.Position:

                    var position = (CSSPrimitiveValue)value;
                    Positioning = DocumentPanel.FromCSSPositioning(position);
                    break;

                case CSSProperties.Left:

                    var left = (CSSPrimitiveValue)value;
                    if (left == CSSValues.Auto)
                        Left = DefaultLeft;
                    else if (left.IsLength())
                        Left = left.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Top:

                    var top = (CSSPrimitiveValue)value;
                    if (top == CSSValues.Auto)
                        Top = DefaultTop;
                    else if (top.IsLength())
                        Top = top.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Right:

                    var right = (CSSPrimitiveValue)value;
                    if (right == CSSValues.Auto)
                        Right = DefaultRight;
                    else if (right.IsLength())
                        Right = right.GetFloatValue(CSSPrimitiveType.CSS_PX);

                    if (!Right.Equals(DefaultRight))
                        HorizontalAlignment = HorizontalAlignment.Right;
                    break;

                case CSSProperties.Bottom:

                    var bottom = (CSSPrimitiveValue)value;
                    if (bottom == CSSValues.Auto)
                        Bottom = DefaultBottom;
                    else if (bottom.IsLength())
                        Bottom = bottom.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Width:

                    var width = (CSSPrimitiveValue)value;
                    if (width == CSSValues.Auto)
                        Width = Double.NaN;
                    else if (width.IsLength())
                        Width = width.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Height:

                    var height = (CSSPrimitiveValue)value;
                    if (height == CSSValues.Auto)
                        Height = Double.NaN;
                    else if (height.IsLength())
                        Height = height.GetFloatValue(CSSPrimitiveType.CSS_PX);
                    break;

                case CSSProperties.Float:

                    var floatValue = (CSSPrimitiveValue)value;
                    Float = DocumentPanel.FromFloatCSS(floatValue);
                    break;

                case CSSProperties.Clear:

                    var clearValue = (CSSPrimitiveValue)value;
                    Clear = DocumentPanel.FromClearCSS(clearValue);
                    break;

                default:
                    break;
            }
        }

        #endregion

        #region SVG

        public virtual NodeType GetSVGNodeType(NodeContext context)
        {
            return NodeType.ELEMENT_NODE;
        }

        public virtual string GetSVGNodeName(NodeContext context)
        {
            throw new NotImplementedException();
        }

        public virtual IList<string> GetSVGAttributes(ElementContext context)
        {
            return context.GetInternalAttributes();
        }

        public virtual string GetSVGAttribute(ElementContext context, string name)
        {
            return context.GetInternalAttribute(name);
        }

        public virtual void SetSVGAttribute(ElementContext context, string name, string value)
        {
            context.SetInternalAttribute(name, value);
        }

        public virtual void ResetSVGAttribute(ElementContext context, string name)
        {
            context.RemoveInternalAttribute(name);
        }

        public virtual object GetSVGParentNode(NodeContext context)
        {
            return Parent;
        }

        public virtual IEnumerable<object> GetSVGChildNodes(NodeContext context)
        {
            return new INodeSource[0];
        }

        public virtual object CreateSVGNode(NodeContext newNode)
        {
            return null;
        }

        public virtual void AppendSVGNode(NodeContext context, object newChild)
        {
            Append((UIElement)newChild);
        }

        public virtual void InsertSVGNode(NodeContext context, object newChild, object refChild)
        {
            int index = InternalChildren.IndexOf((UIElement)refChild);
            if (index != -1)
            {
                Insert(index, (UIElement)newChild);
            }
            else
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public virtual void RemoveSVGNode(NodeContext context, object oldChild)
        {
            if (Remove((UIElement)oldChild) == -1)
            {
                throw new DOMException(DOMException.NOT_FOUND_ERR);
            }
        }

        public virtual IList<string> GetSVGStyles(ElementContext context)
        {
            return null;
        }

        public virtual CSSValue GetSVGStyle(ElementContext context, string name)
        {
            return null;
        }

        public virtual void SetSVGStyle(ElementContext context, string name, CSSValue value)
        {

        }

        #endregion

        #region Implementation

        Size DoLayout(Size availableSize, bool arrange = false)
        {
            Thickness padding = Padding;

            double xPadding = padding.Left + padding.Right;
            double yPadding = padding.Top + padding.Bottom;
            
            availableSize.Width = Math.Max(availableSize.Width - xPadding, 0);
            availableSize.Height = Math.Max(availableSize.Height - yPadding, 0);

            Size desiredSize = new Size(0, 0);
            Size actualSize = new Size(0, 0);

            double leftOffset = 0;
            double rightOffset = 0;
            double topOffset = 0;
            double lastBottomMargin = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                var positioning = GetPositioning(child);

                if (positioning == Positioning.Absolute)
                {
                    continue;
                }

                var childMargin = (Thickness)child.GetValue(FrameworkElement.MarginProperty);

                leftOffset += childMargin.Left;
                topOffset += Math.Max(childMargin.Top - lastBottomMargin, 0);   // collapse margins

                double availableWidth = Math.Max(availableSize.Width - leftOffset - rightOffset - childMargin.Right, 0);
                double availableHeight = Math.Max(availableSize.Height - topOffset, 0);

                if (!arrange)
                {
                    // Measure

                    double availableChildWidth = (double)child.GetValue(FrameworkElement.WidthProperty);
                    if (Double.IsNaN(availableChildWidth))
                    {
                        availableChildWidth = availableWidth;
                    }

                    double availableChildHeight = (double)child.GetValue(FrameworkElement.HeightProperty);
                    if (Double.IsNaN(availableChildHeight))
                    {
                        availableChildHeight = availableHeight;
                    }

                    var availableChildSize = new Size(availableChildWidth, availableChildHeight);

                    Measure(child, availableChildSize);

                    // child's DesiredSize is now set
                }

                Size childDesiredSize = GetDesiredSize(child);
                double assignedChildWidth = Math.Min(childDesiredSize.Width, availableWidth);
                double assignedChildHeight = childDesiredSize.Height; // Math.Min(childDesiredSize.Height, availableHeight);

                // determine child x-offset

                double x = leftOffset;

                FloatDirection floatDirection = GetFloat(child);
                switch (floatDirection)
                {
                    case FloatDirection.Left:
                        x = leftOffset;
                        break;
                    case FloatDirection.Right:
                        x = availableSize.Width - rightOffset - assignedChildWidth;
                        break;
                }

                var horizontalAlignment = (HorizontalAlignment)child.GetValue(FrameworkElement.HorizontalAlignmentProperty);
                switch (horizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        x = leftOffset;
                        break;
                    case HorizontalAlignment.Right:
                        x = availableSize.Width - rightOffset - assignedChildWidth;
                        break;
                    case HorizontalAlignment.Center:
                        x = (availableSize.Width - rightOffset - leftOffset) / 2 - assignedChildWidth / 2;
                        break;
                }

                if (positioning == Positioning.Relative || positioning == Positioning.Overlapped)
                {
                    double left = DocumentPanel.GetLeft(child);
                    if (!Double.IsNaN(left))
                    {
                        x += left;
                    }
                }

                // determine child y-offset

                double y = topOffset;

                if (positioning == Positioning.Relative || positioning == Positioning.Overlapped)
                {
                    double top = DocumentPanel.GetTop(child);
                    if (!Double.IsNaN(top))
                    {
                        y += top;
                    }
                }

                if (arrange)
                {
                    var assignedChildRect = new Rect(x + padding.Left, y + padding.Top, assignedChildWidth, assignedChildHeight);
                    Arrange(child, assignedChildRect);
                }

                // update total size

                desiredSize.Width = Math.Max(desiredSize.Width, x + childDesiredSize.Width);
                desiredSize.Height = Math.Max(desiredSize.Height, y + childDesiredSize.Height);

                actualSize.Width = Math.Max(actualSize.Width, x + assignedChildWidth);
                actualSize.Height = Math.Max(actualSize.Height, y + assignedChildHeight);

                leftOffset += childMargin.Right;
                topOffset += childMargin.Bottom;

                if (positioning == Positioning.Static)
                {
                    if (floatDirection == FloatDirection.Left)
                    {
                        leftOffset += assignedChildWidth;
                    }
                    else if (floatDirection == FloatDirection.Right)
                    {
                        rightOffset += assignedChildWidth;
                    }
                    else
                    {
                        leftOffset = rightOffset = 0;
                        topOffset += assignedChildHeight;
                    }

                    if (child.Visibility != Visibility.Collapsed)
                    {
                        lastBottomMargin = childMargin.Bottom;
                    }
                }
            }

            desiredSize.Width = Math.Max(desiredSize.Width, MinWidth);
            desiredSize.Height = Math.Max(desiredSize.Height, MinHeight);
            actualSize.Width = Math.Max(actualSize.Width, MinWidth);
            actualSize.Height = Math.Max(actualSize.Height, MinHeight);

            foreach (UIElement child in InternalChildren)
            {
                if (GetPositioning(child) != Positioning.Absolute)
                {
                    continue;
                }

                double left = GetLeft(child);
                if (Double.IsNaN(left))
                    left = 0;
                double top = GetTop(child);
                if (Double.IsNaN(top))
                    top = 0;

                if (!arrange)
                {
                    double right = GetRight(child);
                    if (Double.IsNaN(right))
                        right = 0;
                    double bottom = GetBottom(child);
                    if (Double.IsNaN(bottom))
                        bottom = 0;

                    double maxWidth = Math.Max(actualSize.Width + padding.Left + padding.Right - left - right, 0);
                    double maxHeight = Math.Max(actualSize.Height + padding.Top + padding.Bottom - top - bottom, 0);

                    Measure(child, new Size(maxWidth, maxHeight));
                }

                var childDesiredSize = GetDesiredSize(child);
                double width = childDesiredSize.Width;
                double height = childDesiredSize.Height;

                if (arrange)
                {
                    var assignedChildRect = new Rect(left, top, width, height);
                    Arrange(child, assignedChildRect);
                }

                desiredSize.Width = Math.Max(desiredSize.Width, left + width - padding.Left - padding.Right);
                desiredSize.Height = Math.Max(desiredSize.Height, top + height - padding.Top - padding.Bottom);
                actualSize.Width = Math.Max(actualSize.Width, left + width - padding.Left - padding.Right);
                actualSize.Height = Math.Max(actualSize.Height, top + height - padding.Top - padding.Bottom);
            }

            Size resultSize = arrange ? availableSize : desiredSize;
            resultSize.Width += xPadding;
            resultSize.Height += yPadding;
            return resultSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            TextParagraph.UpdateSectionNumbering(InternalChildren);
            TextParagraph.UpdateListNumbering(InternalChildren);
            return DoLayout(availableSize, false);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return DoLayout(finalSize, true);
        }

        /// <summary>
        /// Call the given element's Measure() method.
        /// 
        /// Internally, WPF subtracts an element's margin from the value passed to Measure().
        /// This method offsets that so that we can deal with margins ourself.
        /// </summary>
        private static void Measure(UIElement element, Size availableSize)
        {
            var margin = (Thickness)element.GetValue(FrameworkElement.MarginProperty);

            double width = availableSize.Width + margin.Left + margin.Right;
            width = Math.Max(width, 0);
            double height = availableSize.Height + margin.Top + margin.Bottom;
            height = Math.Max(height, 0);

            element.Measure(new Size(width, height));
        }

        /// <summary>
        /// Get the given element's DesiredSize.
        /// 
        /// Internally, WPF subtracts an element's margin from the value passed to Measure().
        /// This method offsets that so that we can deal with margins ourself.
        /// </summary>
        private static Size GetDesiredSize(UIElement element)
        {
            var margin = (Thickness)element.GetValue(FrameworkElement.MarginProperty);

            double width = element.DesiredSize.Width - margin.Left - margin.Right;
            width = Math.Max(width, 0);
            double height = element.DesiredSize.Height - margin.Top - margin.Bottom;
            height = Math.Max(height, 0);

            return new Size(width, height);
        }

        /// <summary>
        /// Call the given element's Arrange() method.
        /// 
        /// Internally, WPF subtracts an element's margin from the value passed to Arrange().
        /// This method offsets that so that we can deal with margins ourself.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="finalRect"></param>
        private static void Arrange(UIElement element, Rect finalRect)
        {
            Rect adjustedRect = new Rect(finalRect.X, finalRect.Y, finalRect.Width, finalRect.Height);

            var margin = (Thickness)element.GetValue(FrameworkElement.MarginProperty);

            adjustedRect.X -= margin.Left;
            adjustedRect.Y -= margin.Top;
            adjustedRect.Width += margin.Left + margin.Right;
            adjustedRect.Height += margin.Top + margin.Bottom;

            element.Arrange(adjustedRect);
        }

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            var rect = new Rect(RenderSize);

            // Background

            var geometry = DocumentPanel.GetBorderGeometry(
                rect,
                BorderLeftStyle, BorderLeftColor, BorderLeftWidth, BorderTopLeftRadius,
                BorderTopStyle, BorderTopColor, BorderTopWidth, BorderTopRightRadius,
                BorderRightStyle, BorderRightColor, BorderRightWidth, BorderBottomRightRadius,
                BorderBottomStyle, BorderBottomColor, BorderBottomWidth, BorderBottomLeftRadius
            );

            dc.DrawGeometry(Background, null, geometry);

            // Borders
            DocumentPanel.DrawBorders(
                dc, rect,
                BorderLeftStyle, BorderLeftColor, BorderLeftWidth, BorderTopLeftRadius,
                BorderTopStyle, BorderTopColor, BorderTopWidth, BorderTopRightRadius,
                BorderRightStyle, BorderRightColor, BorderRightWidth, BorderBottomRightRadius,
                BorderBottomStyle, BorderBottomColor, BorderBottomWidth, BorderBottomLeftRadius
            );
        }


        internal static void DrawBorders(
            DrawingContext dc, Rect region,
            BorderStyles leftStyle, Color leftColor, double leftWidth, double topLeftRadius,
            BorderStyles topStyle, Color topColor, double topWidth, double topRightRadius,
            BorderStyles rightStyle, Color rightColor, double rightWidth, double bottomRightRadius,
            BorderStyles bottomStyle, Color bottomColor, double bottomWidth, double bottomLeftRadius,
            double zoom = 1.0
        )
        {
            Pen leftPen = BorderPen(leftStyle, leftColor, leftWidth);
            Pen topPen = BorderPen(topStyle, topColor, topWidth);
            Pen rightPen = BorderPen(rightStyle, rightColor, rightWidth);
            Pen bottomPen = BorderPen(bottomStyle, bottomColor, bottomWidth);

            // Find the endpoints of each segment we need to draw:
            //
            //     2          3
            //      +--------+
            //     /          \
            //  1 +            + 4
            //    |            |
            //    |            |
            //  0 +            + 5
            //     \          /
            //      +--------+
            //     7          6

            Point point0 = region.BottomLeft;
            point0.Y -= bottomLeftRadius;
            Point point1 = region.TopLeft;
            point1.Y += topLeftRadius;
            Point point2 = region.TopLeft;
            point2.X += topLeftRadius;
            Point point3 = region.TopRight;
            point3.X -= topRightRadius;
            Point point4 = region.TopRight;
            point4.Y += topRightRadius;
            Point point5 = region.BottomRight;
            point5.Y -= bottomRightRadius;
            Point point6 = region.BottomRight;
            point6.X -= bottomRightRadius;
            Point point7 = region.BottomLeft;
            point7.X += bottomLeftRadius;

            // By default, AlignPoint shifts the point by -0.5, -0.5 when
            // the specified width is an odd value (adjusted for zoom).
            // This shifts our top-left borders out of the clipping region 
            // and they are not drawn at all if they are 1 pixel in width. 
            //
            // If this panel is larger than its layout size, then the bottom-
            // right portion of the panel (including the borders) will be
            // clipped as well and no borders are visible. 
            //
            // This results in no inner borders being drawn when an NTableCell 
            // is clipped by neighboring cells when the border width = 1 pixel.
            // The fix is to apply a bias of +0.5, +0.5 instead.

            point0 = LayoutHelper.Align(point0, leftWidth, zoom, 0.5);
            point1 = LayoutHelper.Align(point1, leftWidth, zoom, 0.5);
            point2 = LayoutHelper.Align(point2, topWidth, zoom, 0.5);
            point3 = LayoutHelper.Align(point3, topWidth, zoom, 0.5);
            point4 = LayoutHelper.Align(point4, rightWidth, zoom, 0.5);
            point5 = LayoutHelper.Align(point5, rightWidth, zoom, 0.5);
            point6 = LayoutHelper.Align(point6, bottomWidth, zoom, 0.5);
            point7 = LayoutHelper.Align(point7, bottomWidth, zoom, 0.5);

            // Shortcut for drawing a simple rectangle
            if (leftStyle == BorderStyles.Solid && topStyle == BorderStyles.Solid && rightStyle == BorderStyles.Solid && bottomStyle == BorderStyles.Solid &&
                leftColor == topColor && topColor == rightColor && rightColor == bottomColor &&
                leftWidth == topWidth && topWidth == rightWidth && rightWidth == bottomWidth &&
                topLeftRadius == 0 && topRightRadius == 0 && bottomRightRadius == 0 && bottomLeftRadius == 0)
            {
                Rect rect = new Rect(point2, point5);
                Pen pen = BorderPen(leftStyle, leftColor, leftWidth);
                dc.DrawRectangle(null, pen, rect);
                return;
            }

            // Left border
            if (leftWidth > 0 && leftStyle != BorderStyles.None)
            {
                dc.DrawLine(leftPen, point0, point1);
            }

            // Top-left corner

            if (topLeftRadius > 0 && topPen != null)
            {
                DrawArc(dc, null, topPen, point1, point2, topLeftRadius);
            }

            // Top border
            if (topPen != null)
            {
                dc.DrawLine(topPen, point2, point3);
            }

            // Top-right corner

            if (topRightRadius > 0 && topPen != null)
            {
                DrawArc(dc, null, topPen, point3, point4, topRightRadius);
            }

            // Right border
            if (rightPen != null)
            {
                dc.DrawLine(rightPen, point4, point5);
            }

            // Bottom-right corner

            if (bottomRightRadius > 0 && bottomPen != null)
            {
                DrawArc(dc, null, bottomPen, point5, point6, bottomRightRadius);
            }

            // Bottom border
            if (bottomPen != null)
            {
                dc.DrawLine(bottomPen, point6, point7);
            }

            // Bottom-left corner

            if (bottomLeftRadius > 0 && bottomPen != null)
            {
                DrawArc(dc, null, bottomPen, point7, point0, bottomLeftRadius);
            }
        }

        internal static Geometry GetBorderGeometry(Rect region,
            BorderStyles leftStyle, Color leftColor, double leftWidth, double topLeftRadius,
            BorderStyles topStyle, Color topColor, double topWidth, double topRightRadius,
            BorderStyles rightStyle, Color rightColor, double rightWidth, double bottomRightRadius,
            BorderStyles bottomStyle, Color bottomColor, double bottomWidth, double bottomLeftRadius)
        {
            bool isLeftStroked = leftStyle != BorderStyles.None && leftWidth > 0;
            bool isTopStroked = topStyle != BorderStyles.None && topWidth > 0;
            bool isRightStroked = rightStyle != BorderStyles.None && rightWidth > 0;
            bool isBottomStroked = bottomStyle != BorderStyles.None && bottomWidth > 0;

            // Find the endpoints of each segment we need to draw:
            //
            //     2          3
            //      +--------+
            //     /          \
            //  1 +            + 4
            //    |            |
            //    |            |
            //  0 +            + 5
            //     \          /
            //      +--------+
            //     7          6

            Point point0 = region.BottomLeft;
            point0.Y -= bottomLeftRadius;
            Point point1 = region.TopLeft;
            point1.Y += topLeftRadius;
            Point point2 = region.TopLeft;
            point2.X += topLeftRadius;
            Point point3 = region.TopRight;
            point3.X -= topRightRadius;
            Point point4 = region.TopRight;
            point4.Y += topRightRadius;
            Point point5 = region.BottomRight;
            point5.Y -= bottomRightRadius;
            Point point6 = region.BottomRight;
            point6.X -= bottomRightRadius;
            Point point7 = region.BottomLeft;
            point7.X += bottomLeftRadius;

            point0 = LayoutHelper.Align(point0, leftWidth, 1, 0.5);
            point1 = LayoutHelper.Align(point1, leftWidth, 1, 0.5);
            point2 = LayoutHelper.Align(point2, topWidth, 1, 0.5);
            point3 = LayoutHelper.Align(point3, topWidth, 1, 0.5);
            point4 = LayoutHelper.Align(point4, rightWidth, 1, 0.5);
            point5 = LayoutHelper.Align(point5, rightWidth, 1, 0.5);
            point6 = LayoutHelper.Align(point6, bottomWidth, 1, 0.5);
            point7 = LayoutHelper.Align(point7, bottomWidth, 1, 0.5);

            var figure = new PathFigure();
            figure.StartPoint = point0;

            // Left border

            var leftSegment = new LineSegment(point1, isLeftStroked);
            leftSegment.Freeze();
            figure.Segments.Add(leftSegment);

            // Top-left corner
            
            if (topLeftRadius > 0)
            {
                var topLeftSegment = new ArcSegment(point2, new Size(topLeftRadius, topLeftRadius), 0, false, SweepDirection.Clockwise, isTopStroked);
                topLeftSegment.Freeze();
                figure.Segments.Add(topLeftSegment);
            }

            // Top border
            
            var topSegment = new LineSegment(point3, isTopStroked);
            topSegment.Freeze();
            figure.Segments.Add(topSegment);

            // Top-right corner
            
            if (topRightRadius > 0)
            {
                var topRightSegment = new ArcSegment(point4, new Size(topRightRadius, topRightRadius), 0, false, SweepDirection.Clockwise, isTopStroked);
                topRightSegment.Freeze();
                figure.Segments.Add(topRightSegment);
            }

            // Right border
            
            var rightSegment = new LineSegment(point5, isRightStroked);
            rightSegment.Freeze();
            figure.Segments.Add(rightSegment);

            // Bottom-right corner
            
            if (bottomRightRadius > 0)
            {
                var bottomRightSegment = new ArcSegment(point6, new Size(bottomRightRadius, bottomRightRadius), 0, false, SweepDirection.Clockwise, isBottomStroked);
                bottomRightSegment.Freeze();
                figure.Segments.Add(bottomRightSegment);
            }

            // Bottom border
            
            var bottomSegment = new LineSegment(point7, isBottomStroked);
            bottomSegment.Freeze();
            figure.Segments.Add(bottomSegment);

            // Bottom-left corner
            
            if (bottomLeftRadius > 0)
            {
                var bottomLeftSegment = new ArcSegment(point0, new Size(bottomLeftRadius, bottomLeftRadius), 0, false, SweepDirection.Clockwise, isBottomStroked);
                bottomLeftSegment.Freeze();
                figure.Segments.Add(bottomLeftSegment);
            }

            figure.Freeze();

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();

            return geometry;
        }


        private static Pen BorderPen(BorderStyles style, Color color, double width)
        {
            if (style == BorderStyles.None || width == 0)
            {
                return null;
            }

            var brush = new SolidColorBrush(color);
            brush.Freeze();

            var pen = new Pen(brush, width);

            switch (style)
            {
                case DocumentPanel.BorderStyles.Dashed:
                    pen.DashStyle = DashStyles.Dash;
                    break;
                case DocumentPanel.BorderStyles.Dotted:
                    pen.DashStyle = DashStyles.Dot;
                    break;
                case DocumentPanel.BorderStyles.Solid:
                default:
                    pen.DashStyle = DashStyles.Solid;
                    break;
            }

            pen.Freeze();

            return pen;
        }

        /// <summary>
        /// Draw an arc to the given drawing context
        /// </summary>
        private static void DrawArc(DrawingContext dc, Brush brush, Pen pen, Point startPoint, Point endPoint, double radius)
        {
            var segment = new ArcSegment(endPoint, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true);
            segment.Freeze();

            var figure = new PathFigure();
            figure.StartPoint = startPoint;
            figure.Segments.Add(segment);
            figure.Freeze();

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();

            dc.DrawGeometry(brush, pen, geometry);
        }

        #endregion

        #region Helpers

        public UIElement ChildFromPoint(Point point)
        {
            DependencyObject result = InputHitTest(point) as DependencyObject;
            while (result != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(result);
                if (parent == this)
                    break;
                result = parent;
            }
            return result as UIElement;
        }

        public UIElement ClosestChildFromPoint(Point point)
        {
            return ClosestChildFromPoint(InternalChildren.OfType<UIElement>(), point);
        }

        public UIElement ClosestChildFromPoint(IEnumerable<UIElement> children, Point point)
        {
            UIElement above = null;     // element above the given point
            UIElement below = null;     // element below the given point

            foreach (UIElement child in children)
            {
                below = child;

                Point childPoint = TransformToDescendant(child).Transform(point);
                if (childPoint.Y < 0)
                {
                    // point is above child

                    if (above != null && (TransformToDescendant(above).Transform(point).Y - above.RenderSize.Height) < -childPoint.Y)
                    {
                        return above;
                    }
                    else
                    {
                        return below;
                    }
                }

                above = below;
            }

            // point is after the last child

            return above;
        }

        public int ClosestIndexFromPoint(Point point)
        {
            UIElement element = ClosestChildFromPoint(point);
            if (element != null)
            {
                return InternalChildren.IndexOf(element);
            }
            else
            {
                return -1;
            }
        }


        protected ITextElement ClosestTextElement(Point point)
        {
            ITextElement result = null;
            foreach (UIElement child in InternalChildren)
            {
                if (child is ITextElement)
                {
                    result = (ITextElement)child;
                    Point childPoint = TransformToDescendant(child).Transform(point);
                    if (childPoint.Y < child.RenderSize.Height)
                        return result;
                }
            }
            return result;
        }


        public static DocumentPanel FindPanel(DependencyObject descendant)
        {
            while (descendant != null)
            {
                descendant = VisualTreeHelper.GetParent(descendant);
                if (descendant is DocumentPanel)
                    break;
            }
            return (DocumentPanel)descendant;
        }

        /// <summary>
        /// This function finds the index of the statically-positioned child 
        /// element rendered just above the given point.
        /// </summary>
        /// <param name="point">The point to evaluate</param>
        /// <returns>An InternalChildren index of the statically-positioned 
        /// child rendered just above the given point, or -1 if none</returns>
        public int PreceedingStaticChildFromPoint(Point point)
        {
            int result = -1;
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                if (DocumentPanel.GetPositioning(child) == Positioning.Static && child.Visibility == Visibility.Visible)
                {
                    Point childPoint = TransformToDescendant(child).Transform(point);
                    if (childPoint.Y < 0)
                        break;
                    result = i;
                }
            }
            return result;
        }

        /// <summary>
        /// Given an absolute position, this function finds the corresponding
        /// index into InternalChildren and transforms the position relative 
        /// to that index in the vertical plane and relative to either the
        /// left, center, or right in the horizontal plane.
        /// </summary>
        /// <param name="absoluteRect"></param>
        /// <param name="index"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        public Point GetRelativePosition(Rect absoluteRect, out int index, out HorizontalAlignment alignment)
        {
            Point relativePosition = GetRelativePosition(absoluteRect.Location, out index);

            alignment = ComputeHorizontalAlignment(absoluteRect);

            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    relativePosition.X -= (this.ActualWidth - this.Padding.Left - this.Padding.Right - absoluteRect.Width) / 2;
                    break;
                case HorizontalAlignment.Right:
                    relativePosition.X -= this.ActualWidth - this.Padding.Left - this.Padding.Right - absoluteRect.Width;
                    break;
            }

            return relativePosition;
        }

        /// <summary>
        /// Given an absolute position, this function finds the corresponding
        /// index into InternalChildren and transforms the position relative 
        /// to that index.
        /// </summary>
        /// <param name="absolutePosition">A position relative to this panel</param>
        /// <param name="relativePosition">Accepts the InternalChildren index corresponding to absolutePosition</param>
        /// <returns>A position relative to an InternalChildren index</returns>
        public Point GetRelativePosition(Point absolutePosition, out int index)
        {
            // Find the child just above the given absolutePosition
            int preceedingIndex = PreceedingStaticChildFromPoint(absolutePosition);
            if (preceedingIndex < 0)
            {
                // There are no statically-positioned children
                index = 0;
                return new Point(absolutePosition.X + Padding.Left, absolutePosition.Y + Padding.Top);
            }

            UIElement preceedingElement = InternalChildren[preceedingIndex];

            index = preceedingIndex + 1;

            // Now compute the position relative to this index
            Point relativePosition;
            while (preceedingElement.Visibility == Visibility.Collapsed)
            {
                // can't use collapsed elements for transforms - use preceeding one instead
                if (preceedingIndex == 0)
                {
                    return new Point(absolutePosition.X + Padding.Left, absolutePosition.Y + Padding.Top);
                }
                preceedingIndex = FindLastIndex<UIElement>(preceedingIndex - 1, PositioningEquals(Positioning.Static));
                if (preceedingIndex == -1)
                {
                    return new Point(absolutePosition.X + Padding.Left, absolutePosition.Y + Padding.Top);
                }
                preceedingElement = InternalChildren[preceedingIndex];
            }
            relativePosition = TransformToDescendant(preceedingElement).Transform(absolutePosition);
            relativePosition.Y -= LayoutHelper.GetHeight(preceedingElement);

            // Adjust for the element's margin
            object margin = preceedingElement.GetValue(FrameworkElement.MarginProperty);
            if (margin != DependencyProperty.UnsetValue)
            {
                relativePosition.X += ((Thickness)margin).Left;
                relativePosition.Y -= ((Thickness)margin).Bottom;
            }

            return relativePosition;
        }

        public Point GetRelativePosition(Rect absoluteRect)
        {
            int index;
            HorizontalAlignment alignment;
            return GetRelativePosition(absoluteRect, out index, out alignment);
        }

        public HorizontalAlignment GetRelativeAlignment(Rect absoluteRect)
        {
            int index;
            HorizontalAlignment alignment;
            GetRelativePosition(absoluteRect, out index, out alignment);
            return alignment;
        }

        /// <summary>
        /// Get the target element's position; if it is absolutely positioned,
        /// then get the position that will be assigned to it when it is 
        /// relatively positioned
        /// </summary>
        /// <param name="dep"></param>
        /// <returns></returns>
        public static Point GetRelativePosition(DependencyObject dep)
        {
            var panel = FindPanel(dep);

            if (panel != null && GetPositioning(dep) == Positioning.Absolute)
            {
                var rect = panel.GetAbsoluteRect(dep);

                return panel.GetRelativePosition(rect);
            }
            else
            {
                return GetPosition(dep);
            }
        }

        /// <summary>
        /// Get the target element's horizontal alignment; if it is absolutely
        /// positioned, then get the horizontal alignment that will be assigned
        /// to it when it is relatively positioned
        /// </summary>
        /// <param name="dep"></param>
        /// <returns></returns>
        public static HorizontalAlignment GetRelativeAlignment(DependencyObject dep)
        {
            var panel = FindPanel(dep);

            if (panel != null && GetPositioning(dep) == Positioning.Absolute)
            {
                var rect = panel.GetAbsoluteRect(dep);

                return panel.GetRelativeAlignment(rect);
            }
            else
            {
                return (HorizontalAlignment)dep.GetValue(FrameworkElement.HorizontalAlignmentProperty);
            }
        }

        private Rect GetAbsoluteRect(DependencyObject dep)
        {
            var position = GetAbsolutePosition(dep);
            var size = ((UIElement)dep).RenderSize;
            return new Rect(position, size);
        }

        private Point GetAbsolutePosition(DependencyObject dep)
        {
            if (GetPositioning(dep) == Positioning.Absolute)
            {
                return GetPosition(dep);
            }
            else
            {
                return LayoutHelper.GetPosition((Visual)dep, relativeTo: this);
            }
        }

        private HorizontalAlignment ComputeHorizontalAlignment(Rect rect)
        {
            if (!IsArrangeValid && Parent is UIElement)
            {
                ((UIElement)Parent).UpdateLayout();
            }
            if (!IsArrangeValid)
            {
                return HorizontalAlignment.Left;
            }
            double panelWidth = ActualWidth;

            double targetLeft = rect.Left;
            double targetCenter = (rect.Left + rect.Right) / 2;
            double targetRight = rect.Right;

            double panelLeft = 0;
            double panelCenter = panelWidth / 2;
            double panelRight = panelWidth;

            // Elements that are clipped by the viewport should remain
            // in the same location as the size of the viewport changes
            if (targetRight > panelRight)
            {
                return HorizontalAlignment.Left;
            }

            double deltaLeft = Math.Abs(targetLeft - panelLeft);
            double deltaCenter = Math.Abs(targetCenter - panelCenter);
            double deltaRight = Math.Abs(targetRight - panelRight);

            double minDelta = Math.Min(deltaLeft, Math.Min(deltaCenter, deltaRight));

            if (minDelta == deltaCenter)
            {
                return HorizontalAlignment.Center;
            }
            else if (minDelta == deltaRight)
            {
                return HorizontalAlignment.Right;
            }
            else
            {
                return HorizontalAlignment.Left;
            }
        }

        /// <summary>
        /// Move a child element within the InternalChildren array from the
        /// given source index to the given destination index, and return
        /// the new index of the child that was previously located just after
        /// the source index.
        /// </summary>
        int MoveChildElement(int sourceIndex, int destinationIndex)
        {
            UIElement child = InternalChildren[sourceIndex];
            InternalChildren.RemoveAt(sourceIndex);
            if (sourceIndex < destinationIndex)
                destinationIndex--;
            InternalChildren.Insert(destinationIndex, child);
            if (destinationIndex <= sourceIndex)
                sourceIndex++;
            return sourceIndex;
        }

        /// <summary>
        /// For each absolute-positioned element, update its offset within
        /// InternalChildren to that corresponding to its current position.
        /// 
        /// This is called prior to ensure all elements are in their proper
        /// order prior to serialization. It does not affect the actual
        /// PositioningProperty, LeftProperty or TopProperty of the children.
        /// </summary>
        public void Normalize()
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                UIElement child = InternalChildren[i];
                if (GetPositioning(child) == Positioning.Absolute)
                {
                    Point position = GetPosition(child);
                    int neighborIndex = PreceedingStaticChildFromPoint(position);
                    i = MoveChildElement(i, neighborIndex + 1) - 1;
                }
            }
        }

        #endregion

    }

    public enum Positioning
    {
        Static,
        Relative,
        Absolute,
        Fixed,
        Overlapped
    }

}
